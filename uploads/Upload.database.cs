using System;
using System.Collections.Generic;
using System.Linq;
using Spectre.Console;

namespace ELMA_API;

class UploadData
{
    private BaseHttp baseHttp;
    private ElmaApi elmaApi;
    private DbMsSql dbMsSql;

    public UploadData(BaseHttp baseHttp, ElmaApi reqElma, DbMsSql dbMsSql)
    {
        this.baseHttp = baseHttp; // нужен для простых запросов-http к серверу Elma
        this.elmaApi = reqElma; // нужен для уже подготовленных запросов к Elma
        this.dbMsSql = dbMsSql; // нужен для доступа к базе данных
    }

    /// <summary>
    /// производит вызгрузку отсутсвующих данных на сервере ELMA
    /// </summary>
    /// <param name="jsonAuthResp">объект с атрибутами предназначенными для заголовков доступа в http запросе</param>
    /// <param name="plans">в данном объекте есть поле educational_plans_upload в котором находсят данные которых 
    /// нет на сервер ELMA и которые соотвественно функция загрзут</param>
    public UploadResult Educational_plans()
    {
        // count entities before upload data to server elma
        int eduPlansElmaBefore = int.Parse(elmaApi.countEntity(TypesUidElma.eduPlans).bodyResponse);

        // educational plan from ELMA -> requust // учебные планы из сервера Elma
        List<string> eduPlansElma = elmaApi.queryEntityList(type: TypesUidElma.eduPlans)
            .FindAll(eduPlan => ElmaApi.getValueItem(eduPlan.Items, "Naimenovanie") != null)
            .Select(eduPlan => ElmaApi.getValueItem(eduPlan.Items, "Naimenovanie")).ToList();

        // list educational plans that aren't in ELMA server but they're in database dekanat
        // проверяем полученный учебный план из базы, есть ли он на сервере elma, 
        // если нет тогда добавляем хранилище
        List<string> eduPlansMissed = dbMsSql.educationalPlans().FindAll(eduPlanDb =>
            !eduPlansElma.Contains(eduPlanDb));

        // Хранилище ответов http на внедрение данных от сервера ELMA 
        List<string> insertedData = new List<string>();

        // внедрение учебных планов в elma которые отсутсвуют
        foreach (string eduPlanMissed in eduPlansMissed)
        {
            var responseInsert = this.elmaApi.insertEntity(
                type: TypesUidElma.eduPlans,
                body: ElmaApi.makeBodyJson<string>(
                    ElmaApi.makeItem("Naimenovanie", eduPlanMissed)
                )
            );

            // добавление результата запроса на внедрение данных в список
            insertedData.Add(responseInsert.bodyResponse);
        }

        return new UploadResult
        {
            EntitiesElmaBefore = eduPlansElmaBefore,
            EntitiesElmaAfter =
                int.Parse(elmaApi.countEntity(TypesUidElma.eduPlans).bodyResponse),
            EntitiesForInject = dbMsSql.educationalPlans().Count,
            EntitiesMissed = eduPlansMissed.Count,
            EntitiesIdInjected = insertedData,
            NameMethodUpload = System.Reflection.MethodBase.GetCurrentMethod().Name
        };

    }

    public UploadResult Faculties()
    {
        // count entities before upload data to server elma
        int facultiesElmaBefore = int.Parse(elmaApi.countEntity(TypesUidElma.faculties).bodyResponse);

        // faculties from Elma -> request // факультеты из Elma server
        var facultiesElma = elmaApi.queryEntityList(type: TypesUidElma.faculties)
            .Select(entity =>
            {
                return new FacultyGuide()
                {
                    longName = ElmaApi.getValueItem(entity.Items, "NaimenovaniePolnoe"),
                    shortName = ElmaApi.getValueItem(entity.Items, "NaimenovanieSokraschennoe")
                };
            }).ToList();

        // list faculties that aren't in ELMA server but they're in database dekanat
        // проверяем полученный факультет из базы, есть ли он на сервере elma, 
        // если нет тогда добавляем хранилище
        List<FacultyGuide> facultiesMissed = dbMsSql.faculties().FindAll(facultyDB =>
            facultiesElma.FindAll(facultyElma =>
                facultyElma.longName.ToLower() == facultyDB.longName.ToLower() &&
                facultyElma.shortName == facultyDB.shortName).Count == 0
        );

        // Хранилище ответов http на внедрение данных от сервера ELMA 
        List<string> insertedData = new List<string>();

        // внедрение факультетов которые отсутствуют в elma
        foreach (FacultyGuide facultyMissed in facultiesMissed)
        {
            var responseInsert = this.elmaApi.insertEntity(
                type: TypesUidElma.faculties,
                body: ElmaApi.makeBodyJson<string>(
                    ElmaApi.makeItem("NaimenovaniePolnoe", facultyMissed.longName),
                    ElmaApi.makeItem("NaimenovanieSokraschennoe", facultyMissed.shortName)
                )
            );

            // добавление результата запроса на внедрение данных в список
            insertedData.Add(responseInsert.bodyResponse);
        }

        return new UploadResult
        {
            EntitiesElmaBefore = facultiesElmaBefore,
            EntitiesElmaAfter =
                int.Parse(elmaApi.countEntity(TypesUidElma.faculties).bodyResponse),
            EntitiesForInject = dbMsSql.faculties().Count,
            EntitiesMissed = facultiesMissed.Count,
            EntitiesIdInjected = insertedData,
            NameMethodUpload = System.Reflection.MethodBase.GetCurrentMethod().Name
        };
    }

    public UploadResult Disciplines()
    {
        // count entities before upload data to server elma
        int disciplinesElmaBefore = int.Parse(elmaApi.countEntity(TypesUidElma.disciplines).bodyResponse);

        // disciplines from server Elma
        var disciplinesElma = elmaApi.queryEntityList(type: TypesUidElma.disciplines)
            .FindAll(discipline =>
                ElmaApi.getValueItem(discipline.Items, "Naimenovanie") != null)
            .Select(discipline =>
                ElmaApi.getValueItem(discipline.Items, "Naimenovanie")).ToList();

        // list disciplines that aren't in ELMA server but they're in database dekanat
        List<String> disciplinesMissed = dbMsSql.disciplines().FindAll(disciplineDB =>
            !disciplinesElma.Contains(disciplineDB));

        // Хранилище ответов http на внедрение данных от сервера ELMA 
        List<string> insertedData = new List<string>();

        // внедрение дисциплин которые отсутствуют в ELMA
        foreach (String disciplineMissed in disciplinesMissed)
        {
            // request POST to insert entity to elma server
            var responseInsert = this.elmaApi.insertEntity(
                type: TypesUidElma.disciplines,
                body: ElmaApi.makeBodyJson<string>(
                    ElmaApi.makeItem("Naimenovanie", disciplineMissed)
                )
            ).bodyResponse;

            // добавление результата запроса на внедрение данных в список
            insertedData.Add($"{disciplineMissed} discipline, id {responseInsert} successfully injected on ELMA");
        }

        return new UploadResult
        {
            EntitiesElmaBefore = disciplinesElmaBefore,
            EntitiesElmaAfter =
                int.Parse(elmaApi.countEntity(TypesUidElma.disciplines).bodyResponse),
            EntitiesForInject = dbMsSql.disciplines().Count,
            EntitiesMissed = disciplinesMissed.Count,
            EntitiesIdInjected = insertedData,
            NameMethodUpload = System.Reflection.MethodBase.GetCurrentMethod().Name
        };
    }

    // загрузка направлений подготовки
    public UploadResult Direction_preparations()
    {
        // count entities before upload data to server elma
        int direcsPreElmaBefore = int.Parse(elmaApi.countEntity(TypesUidElma.direcPreparations).bodyResponse);

        // напр. подготов. из Elma server
        List<DirectionPreparation> direcsPrepElma = elmaApi.queryEntityList(type: TypesUidElma.direcPreparations)
            .FindAll(direcPrepElma =>
                !String.IsNullOrEmpty(ElmaApi.getValueItem(direcPrepElma.Items, "Kod")) &&
                !String.IsNullOrEmpty(ElmaApi.getValueItem(direcPrepElma.Items, "Naimenovanie")))
            .Select(direcPrepElma =>
                new DirectionPreparation()
                {
                    Kod = ElmaApi.getValueItem(direcPrepElma.Items, "Kod"),
                    Naimenovanie = ElmaApi.getValueItem(direcPrepElma.Items, "Naimenovanie")
                }
            ).ToList();

        // list directions preparations that aren't in ELMA server but they're in database dekanat
        // фильтрация пропущенных направлений подготовки осуществляется по уникльному коду
        List<DirectionPreparation> direcsPrepMissed = dbMsSql.directionPreparations().FindAll(direcPrepDb =>
            direcsPrepElma.FindAll(direcPrepElma =>
                direcPrepElma.Kod.ToLower() == direcPrepDb.Kod.ToLower()).Count == 0
        );

        // Хранилище ответов http на внедрение данных от сервера ELMA 
        List<string> insertedData = new List<string>();

        // внедрение направлений подготовки которые отсутствуют в ELMA
        foreach (var direcPrepMissed in direcsPrepMissed)
        {
            var responseInsert = this.elmaApi.insertEntity(
                type: TypesUidElma.direcPreparations,
                body: ElmaApi.makeBodyJson<string>(
                    ElmaApi.makeItem("Kod", direcPrepMissed.Kod),
                    ElmaApi.makeItem("Naimenovanie", direcPrepMissed.Naimenovanie)
                )
            );

            // добавление результата запроса на внедрение данных в список
            insertedData.Add(responseInsert.bodyResponse);
        }

        return new UploadResult
        {
            EntitiesElmaBefore = direcsPreElmaBefore,
            EntitiesElmaAfter =
                int.Parse(elmaApi.countEntity(TypesUidElma.direcPreparations).bodyResponse),
            EntitiesForInject = dbMsSql.directionPreparations().Count,
            EntitiesMissed = direcsPrepMissed.Count,
            EntitiesIdInjected = insertedData,
            NameMethodUpload = System.Reflection.MethodBase.GetCurrentMethod().Name
        };
    }

    // загрузка кафедр
    public UploadResult Departments()
    {
        // count entities before upload data to server elma
        int departmentsElmaBefore = int.Parse(elmaApi.countEntity(TypesUidElma.departments).bodyResponse);

        // кафедры из Elma server
        List<Root> departmentsElma = elmaApi.queryEntityList(type: TypesUidElma.departments);

        // получаем СокращенныеНаименования Кафедр которые есть на сервере Elma
        var namesShortDepElma = departmentsElma
            .FindAll(departmentElma => !String.IsNullOrEmpty(ElmaApi.getValueItem(departmentElma.Items, "NaimenovanieSokraschennoe")))
            .Select(departmentElma => ElmaApi.getValueItem(departmentElma.Items, "NaimenovanieSokraschennoe"))
            .ToList();

        // list departments that aren't in ELMA server but they're in database dekanat
        List<DepartmentDb> departmentsMissed = dbMsSql.departments().FindAll(departmentDb =>
            !namesShortDepElma.Contains(departmentDb.NameShort));

        // Хранилище ответов http на внедрение данных от сервера ELMA 
        List<string> insertedData = new List<string>();

        // внедрение направлений подготовки которые отсутствуют в ELMA
        foreach (var depMissed in departmentsMissed)
        {
            // count entities in elma server before upload data
            var countEntitieElma = elmaApi.countEntity(TypesUidElma.departments).bodyResponse;

            // получение зависимости в Справочнике Кафедра -> ФАКУЛЬТЕТ : Id, TypeUid, Uid, Name
            var foundFaculty = this.elmaApi.findFaculties(depMissed.FacultyShort, depMissed.FacultyLong);

            // ЕСЛИ НЕ НАЙДЕТ ФАКУЛЬТЕТ СООТВЕТВУЮЩИМИ ЗНАЧЕНИЯМИ, ТОГДА ЗАЛОГИРУЕТ И ПРОПУСТИТ ИТЕРАЦИЮ
            if (foundFaculty == null)
            {
                Log.Warn(WarnTitle.faildInserted,
                    $"department: shortName =\"{depMissed.NameShort}\", longName=\"{depMissed.NameLong}\"" +
                    " wasn't injectd cause not found faculty");
                Log.Warn(WarnTitle.notFoundFaculty,
                    $"FacultyShort=\"{depMissed.FacultyShort ?? "null"}\"" +
                    $", FacultyLong=\"{depMissed.FacultyLong ?? "null"}\"");
                continue;
            }

            string facultyId = ElmaApi.getValueItem(foundFaculty, "Id");

            var responseInsert = this.elmaApi.insertEntity(
                type: TypesUidElma.departments,
                body: ElmaApi.makeBodyJson<string>(
                    ElmaApi.makeItem("NaimenovaniePolnoe", depMissed.NameLong),
                    ElmaApi.makeItem("NaimenovanieSokraschennoe", depMissed.NameShort),
                    ElmaApi.makeItem("KodKafedry", depMissed.Code),
                    ElmaApi.makeItem("Kabinet", depMissed.Room),
                    ElmaApi.makeItem("Fakuljtet", nestedName: "Id", nestedValue: facultyId)
                )
            ).bodyResponse;

            insertedData.Add(responseInsert);
        }

        return new UploadResult
        {
            EntitiesElmaBefore = departmentsElmaBefore,
            EntitiesElmaAfter =
                int.Parse(elmaApi.countEntity(TypesUidElma.departments).bodyResponse),
            EntitiesForInject = dbMsSql.departments().Count,
            EntitiesMissed = departmentsMissed.Count,
            EntitiesIdInjected = insertedData,
            NameMethodUpload = System.Reflection.MethodBase.GetCurrentMethod().Name
        };
    }

    // профили подготовки (включены в направления подготовки)
    public UploadResult ProfilePrep(
        List<PrepProfileDB> profilesDB
    )
    {
        // count entities before upload data to server elma
        int profilesPrepElmaBefore = int.Parse(elmaApi.countEntity(TypesUidElma.profilePreparation).bodyResponse);

        // профили подготовки из Elma server
        List<PrepProfileElma> profilesElma = elmaApi.preparationProfile();

        // list "prepatations profiles" that aren't in ELMA server but they're in database dekanat
        List<PrepProfileDB> profilesPrepMissed = new List<PrepProfileDB>();

        // Хранилище ответов http на внедрение данных от сервера ELMA 
        List<string> insertedData = new List<string>();

        List<string> codeNameProfileELma = new List<string>();
        foreach (var profileElma in profilesElma)
            codeNameProfileELma.Add(profileElma.codeDirectPrep + " " + profileElma.name);

        foreach (var profileDB in profilesDB)
        {
            if (!codeNameProfileELma.Contains(profileDB.codeDirectPrep + " " + profileDB.name))
            {
                profilesPrepMissed.Add(profileDB);
            }
        }


        // foreach (var item in profilesMissed)
        // {
        //     AnsiConsole.MarkupLine(item.codeDirectPrep + " " + item.name);
        // }


        return new UploadResult
        {
            EntitiesElmaBefore = profilesPrepElmaBefore,
            EntitiesElmaAfter =
                int.Parse(elmaApi.countEntity(TypesUidElma.profilePreparation).bodyResponse),
            EntitiesForInject = dbMsSql.departments().Count,
            EntitiesMissed = profilesPrepMissed.Count,
            EntitiesIdInjected = insertedData,
            NameMethodUpload = System.Reflection.MethodBase.GetCurrentMethod().Name
        };
    }
}