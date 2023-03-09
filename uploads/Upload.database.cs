using System;
using System.Collections.Generic;

namespace ELMA_API
{
    class UploadData
    {
        public BaseHttp baseHttp;
        public Elma reqElma;

        public UploadData(BaseHttp baseHttp, Elma reqElma) {
            this.baseHttp = baseHttp; // нужен для простых запросов-http к серверу Elma
            this.reqElma = reqElma; // нужен для уже подготовленных запросов к Elma
        }

        /// <summary>
        /// производит вызгрузку отсутсвующих данных на сервере ELMA
        /// </summary>
        /// <param name="jsonAuthResp">объект с атрибутами предназначенными для заголовков доступа в http запросе</param>
        /// <param name="plans">в данном объекте есть поле educational_plans_upload в котором находсят данные которых 
        /// нет на сервер ELMA и которые соотвественно функция загрзут</param>
        public void EducationalPlans(List<string> eduPlansDB)
        {
            Log.Info(InfoTitle.uploadElma, "EDUCATIONAL PLANS", colorTitle: "orangered1");
            // AnsiConsole.MarkupLine("EDUCATIONAL PLANS");

            // educational plan from ELMA -> requust // учебные планы из сервера Elma
            List<string> eduPlansElma = this.reqElma.educationalPlans();

            // list educational plans that aren't in ELMA server but they're in database dekanat
            List<string> eduPlansMissed = new List<string>();

            // проверяем полученный учебный план из базы, есть ли он на сервере elma, 
            // если нет тогда добавляем хранилище
            foreach (string plan in eduPlansDB)
                if (!eduPlansElma.Contains(plan)) eduPlansMissed.Add(plan);

            // Хранилище ответов http на внедрение данных от сервера ELMA 
            List<string> plansInserted = new List<string>();

            // внедрение учебных планов в elma которые отсутсвуют
            foreach (string plan in eduPlansMissed)
            {
                // тела запроса для внедрение данных на сервер elma в формате json
                var textReqInsert = @"{
                     ""Items"":[{
                        ""Data"": null,
                        ""DataArray"":[],
                        ""Name"":""Naimenovanie"",
                        ""Value"": ""INJECTED_VALUE_IN_TEXT""
                    },
                    {
                        ""Data"": null,
                        ""DataArray"":[],
                        ""Name"":""SsylkaNaUchebnyyPlan"",
                        ""Value"": """"
                    }],
                    ""Value"": null
                }".Replace("INJECTED_VALUE_IN_TEXT", plan);

                var responseInsert = this.baseHttp.request(
                    path: String.Format("/API/REST/Entity/Insert/{0}", TypesUidElma.eduPlans),
                    method: "POST",
                    body: textReqInsert
                ).bodyString;

                // добавление результата запроса на внедрение данных в список
                plansInserted.Add($"{plan} plan, id {responseInsert} successfully injected on ELMA");
            }

            // Logging information
            Log.Info(InfoTitle.dataElma, $"{eduPlansElma.Count} educational plans in elma");
            Log.Info(InfoTitle.dataDB, $"{eduPlansDB.Count} educational plans in database");
            if (eduPlansMissed.Count != 0)
                Log.Info(InfoTitle.missed, $"{eduPlansMissed.Count} missed educational plans");
            // additional loggin if injected any data in server elma
            if (plansInserted.Count != 0)
                Log.Success(SuccessTitle.injectedData, $"{plansInserted.Count} injected educational plans to elma-server");
            else
                Log.Success(SuccessTitle.synchronized, $"educational plans synchronized");

        }

        public void Faculties( // факультеты
            List<FacultyGuide> facultiesDB)
        {
            Log.Info(InfoTitle.uploadElma, "FACULTIES", colorTitle: "orangered1");

            // faculties from Elma -> request // факультеты из Elma server
            List<FacultyGuide> facultiesElma = this.reqElma.faculties();

            // list faculties that aren't in ELMA server but they're in database dekanat
            List<FacultyGuide> facultiesMissed = new List<FacultyGuide>();

            // проверяем полученный факультет из базы, есть ли он на сервере elma, 
            // если нет тогда добавляем хранилище
            foreach (FacultyGuide facultyDB in facultiesDB)
            {
                var matches = facultiesElma.FindAll(facultyElma => facultyElma.longName == facultyDB.longName);
                // если нет найденных факультетов с именем из БД на elma server тогда добавить в хранилище facultiesMissed
                if (matches.Count == 0) facultiesMissed.Add(facultyDB);
            }

            // Хранилище ответов http на внедрение данных от сервера ELMA 
            List<string> insertedData = new List<string>();

            // внедрение факультетов которые отсутствуют в elma
            foreach (FacultyGuide facultyMissed in facultiesMissed)
            {
                // тела запроса для внедрение данных на сервер elma в формате json
                var textReqInsert = @"{
                    ""Items"" : [
                        {
                            ""Data"": null,
                            ""DataArray"":[],
                            ""Name"":""NaimenovaniePolnoe"",
                            ""Value"":""INJECTED_VALUE_LONG_NAME""
                        },
                        {
                            ""Data"": null,
                            ""DataArray"":[],
                            ""Name"":""NaimenovanieSokraschennoe"",
                            ""Value"":""INJECTED_VALUE_SHORT_NAME""
                        }
                    ],
                    ""Value"": null
                }"
                .Replace("INJECTED_VALUE_LONG_NAME", facultyMissed.longName) // заменяем значение в тексте
                .Replace("INJECTED_VALUE_SHORT_NAME", facultyMissed.shortName); // заменяем значение в тексте

                var responseInsert = this.baseHttp.request(
                    path: String.Format("/API/REST/Entity/Insert/{0}", TypesUidElma.faculties),
                    method: "POST",
                    body: textReqInsert
                ).bodyString;
                // добавление результата запроса на внедрение данных в список
                insertedData.Add($"{facultyMissed.longName} faculty, id {responseInsert} successfully injected on ELMA");
            }

            // logging information
            Log.Info(InfoTitle.dataElma, $"{facultiesElma.Count} faculties in elma");
            Log.Info(InfoTitle.dataDB, $"{facultiesDB.Count} faculties in database");
            if (facultiesMissed.Count != 0)
                Log.Info(InfoTitle.missed, $"{facultiesMissed.Count} missed faculties plans");
            if (insertedData.Count != 0)
                Log.Success(SuccessTitle.injectedData, $"{insertedData.Count} injected faculties to elma-server");
            else 
                Log.Success(SuccessTitle.synchronized, $"faculties synchronized");
        }

        public void Disciplines( // дисциплины
            List<String> disciplinesDB)
        {
            Log.Info(InfoTitle.uploadElma, "DISCIPLINES", colorTitle: "orangered1");

            // disciplines from server Elma
            List<String> disciplinesElma = this.reqElma.disciplines(); // дисциплины из Elma server

            // list disciplines that aren't in ELMA server but they're in database dekanat
            List<String> disciplinesMissed = new List<String>();

            // проверяем полученную дисциплину из базы, есть ли он на сервере elma, 
            // если нет тогда добавляем хранилище
            foreach (String discipline in disciplinesDB)
                if (!disciplinesElma.Contains(discipline)) disciplinesMissed.Add(discipline);

            // Хранилище ответов http на внедрение данных от сервера ELMA 
            List<string> insertedData = new List<string>();

            // внедрение дисциплин которые отсутствуют в ELMA
            foreach (String discipline in disciplinesMissed)
            {
                // тела запроса для внедрение данных на сервер elma в формате json
                var textReqInsert = @"{
                    ""Items"" : [
                        {
                            ""Data"": null,
                            ""DataArray"":[],
                            ""Name"":""Naimenovanie"",
                            ""Value"":""INJECTED_VALUE_NAIMENOVANIE""
                        }
                    ],
                    ""Value"": null
                }"
                .Replace("INJECTED_VALUE_NAIMENOVANIE", discipline); // заменяем значение в тексте

                var responseInsert = this.baseHttp.request(
                    path: String.Format("/API/REST/Entity/Insert/{0}", TypesUidElma.disciplines),
                    method: "POST",
                    body: textReqInsert
                ).bodyString;

                // добавление результата запроса на внедрение данных в список
                insertedData.Add($"{discipline} discipline, id {responseInsert} successfully injected on ELMA");
            }

            // logging information
            Log.Info(InfoTitle.dataElma, $"{disciplinesElma.Count} disciplines in elma");
            Log.Info(InfoTitle.dataDB, $"{disciplinesDB.Count} disciplines in database");
            if (disciplinesMissed.Count != 0)
                Log.Info(InfoTitle.missed, $"{disciplinesMissed.Count} missed disciplines plans");
            if (insertedData.Count != 0)
                Log.Success(SuccessTitle.injectedData, $"{insertedData.Count} injected disciplines to elma-server");
            else 
                Log.Success(SuccessTitle.synchronized, $"disciplines synchronized");
        }
     
        public void DirecsPre( // загрузка направлений подготовки
            List<DirectionPreparation> direcsPreDB)
        {
            Log.Info(InfoTitle.uploadElma, "DIRECTIONS PREPARATIONS", colorTitle: "orangered1");

            // напр. подготов. из Elma server
            List<DirectionPreparation> direcsPreElma = this.reqElma.directsPreps(); 

            // list directions preparations that aren't in ELMA server but they're in database dekanat
            List<DirectionPreparation> direcsPreMissed = new List<DirectionPreparation>();

            // извлекаем все коды полученные от Сервера ELMA по направлениям подготовки
            List<String> codesElma = new List<String>();
            foreach (DirectionPreparation direcPrep in direcsPreElma)
                if (direcPrep.Kod != "") codesElma.Add(direcPrep.Kod);

            // test block
            // direcsPreDatabase.Add(new DirectionPreparation() { Kod = "123123", Naimenovanie = "whatsup due?"});
            // direcsPreDatabase.Add(new DirectionPreparation() { Kod = "4448", Naimenovanie = "whatsup due?"});
            // end test

            // проверяем код направления подготовки из БД, есть ли он на сервере ELMA
            foreach (DirectionPreparation direcPrep in direcsPreDB)
            {
                // Проверяем если такой код из БД на сервере ELMA, 
                // если нет тогда добавляем в хранилище
                if (!codesElma.Contains(direcPrep.Kod)) {
                    direcsPreMissed.Add(direcPrep);
                }
            }

            // Хранилище ответов http на внедрение данных от сервера ELMA 
            List<string> insertedData = new List<string>();

            // внедрение направлений подготовки которые отсутствуют в ELMA
            foreach (var directPre in direcsPreMissed)
            {
                // тела запроса для внедрение данных на сервер elma в формате json
                var textReqInsert = @"{
                    ""Items"" : [
                        {
                            ""Data"": null,
                            ""DataArray"":[],
                            ""Name"":""Kod"",
                            ""Value"":""INJECTED_VALUE_KOD""
                        },
                        {
                            ""Data"": null,
                            ""DataArray"":[],
                            ""Name"":""Naimenovanie"",
                            ""Value"":""INJECTED_VALUE_NAIMENOVANIE""
                        }
                    ],
                    ""Value"": null
                }"
                .Replace("INJECTED_VALUE_NAIMENOVANIE", directPre.Naimenovanie) // заменяем значение в тексте
                .Replace("INJECTED_VALUE_KOD", directPre.Kod); // заменяем значение в тексте

                var responseInsert = this.baseHttp.request(
                    path: String.Format("/API/REST/Entity/Insert/{0}", TypesUidElma.direcPreparations),
                    method: "POST",
                    body: textReqInsert
                ).bodyString;

                // добавление результата запроса на внедрение данных в список
                insertedData.Add($"{directPre.Kod} direction preparations code, id {responseInsert} successfully injected on ELMA");
            }


            // logging information
            Log.Info(InfoTitle.dataElma, $"{direcsPreElma.Count} directions preparations in elma");
            Log.Info(InfoTitle.dataDB, $"{direcsPreDB.Count} directions preparations in database");
            if (direcsPreMissed.Count != 0)
                Log.Info(InfoTitle.missed, $"{direcsPreMissed.Count} missed directions preparations plans");
            if (insertedData.Count != 0)
                Log.Success(SuccessTitle.injectedData, $"{insertedData.Count} injected directions preparations to elma-server");
            else 
                Log.Success(SuccessTitle.synchronized, $"directions preparations synchronized");
        }
    
        public void Departments( // загрузка кафедр
            List<DepartmentFromDB> departmentsDB
        ) {
            Log.Info(InfoTitle.uploadElma, "departments", colorTitle: "orangered1");

            // кафедры из Elma server
            List<Root> departmentsElma = reqElma.departments(); 

            // list departments that aren't in ELMA server but they're in database dekanat
            List<DepartmentFromDB> departmentsMissed = new List<DepartmentFromDB>();

            // Хранилище ответов http на внедрение данных от сервера ELMA 
            List<string> insertedData = new List<string>();

            // получаем СокращенныеНаименование Кафедр которые есть на сервере Elma
            List<String> namesShortDepElma = new List<String>();
            // получаем ячейку "NaimenovanieSokraschennoe" и проверяем есть ли она в хранилище 
            // nameShortDepElma, если нет тогда добавляем
            foreach (Root dep in departmentsElma)
                foreach (Item itemDep in dep.Items)
                    if (itemDep.Name == "NaimenovanieSokraschennoe" 
                    && !namesShortDepElma.Contains(itemDep.Value)) namesShortDepElma.Add(itemDep.Value);

            
            // итерация по кафедрам из БД, проверяем в хранилище СокращенныхНаименовение из сервера ELMA
            // namesShortDepElma если там НЕТ такого же наименование как СокращенноеНаименование ИЗ БД
            // тогда добавляем в хранилище departmentsMissed данную кафедру из БД
            foreach (DepartmentFromDB dep in departmentsDB)
            {
                if (!namesShortDepElma.Contains(dep.NameShort)) {
                    departmentsMissed.Add(dep);
                }  
            }

            // внедрение направлений подготовки которые отсутствуют в ELMA
            foreach (var department in departmentsMissed)
            {
                // получение зависимости в Справочнике Кафедра -> ФАКУЛЬТЕТ : Id, TypeUid, Uid, Name
                var foundFaculty = this.reqElma.findFaculties(department.FacultyLong, department.FacultyShort);
                String facultyId = null, facultyTypeUid = null, facultyUid = null, facultyNameShort = null;
                
                // ЕСЛИ НЕ НАЙДЕТ ФАКУЛЬТЕТ СООТВЕТВУЮЩИМИ ЗНАЧЕНИЯМИ, ТОГДА ЗАЛОГИРУЕТ И ВЫБРОСИТ ОШИБКУ 
                if (foundFaculty == null) {
                    Log.Warn(WarnTitle.notFoundFaculty, $"{department.FacultyShort}, {department.FacultyLong}");
                    throw new Exception($"НЕ НАЙДЕН ФАКУЛЬТЕТ : {department.FacultyShort}, {department.FacultyLong}");
                }

                // добавление данных найденного факультета для переменных
                foreach (var item in foundFaculty) {
                    if (item.Name == "Id") facultyId = item.Value;
                    if (item.Name == "TypeUid") facultyTypeUid = item.Value;
                    if (item.Name == "Uid") facultyUid = item.Value;
                    if (item.Name == "NaimenovanieSokraschennoe") facultyNameShort = item.Value;
                }

                // тела запроса для внедрение данных на сервер elma в формате json
                var textReqInsert = @"{
                ""Items"": [
                    {
                        ""Data"": null,
                        ""DataArray"": [],
                        ""Name"": ""NaimenovaniePolnoe"",
                        ""Value"": ""INJECT_NAME_LONG""
                    },
                    {
                        ""Data"": null,
                        ""DataArray"": [],
                        ""Name"": ""NaimenovanieSokraschennoe"",
                        ""Value"": ""INJECT_NAME_SHORT""
                    },
                    {
                        ""Data"": null,
                        ""DataArray"": [],
                        ""Name"": ""KodKafedry"",
                        ""Value"": ""INJECT_CODE_DEPARTMENT""
                    },
                    {
                        ""Data"": null,
                        ""DataArray"": [],
                        ""Name"": ""Telefon"",
                        ""Value"": null
                    },
                    {
                        ""Data"": null,
                        ""DataArray"": [],
                        ""Name"": ""Kabinet"",
                        ""Value"": ""INJECT_ROOM_DEPARTMENT""
                    },
                    {
                        ""Data"": {
                            ""Items"": [
                                {
                                    ""Data"": null,
                                    ""DataArray"": [],
                                    ""Name"": ""Id"",
                                    ""Value"": ""INJECT_FACULTY_ID""
                                },
                                {
                                    ""Data"": null,
                                    ""DataArray"": [],
                                    ""Name"": ""Uid"",
                                    ""Value"": ""INJECT_FACULTY_UID""
                                },
                                {
                                    ""Data"": null,
                                    ""DataArray"": [],
                                    ""Name"": ""Name"",
                                    ""Value"": ""INJECT_FACULTY_SHORT_NAME""
                                }
                            ],
                            ""Value"": null
                        },
                        ""DataArray"": [],
                        ""Name"": ""Fakuljtet"",
                        ""Value"": null
                    }
                ],
                ""Value"": null
                }"
                .Replace("INJECT_FACULTY_ID", facultyId)
                .Replace("INJECT_FACULTY_UID",facultyUid)
                .Replace("INJECT_FACULTY_SHORT_NAME", facultyNameShort)
                .Replace("INJECT_NAME_LONG", department.NameLong)
                .Replace("INJECT_NAME_SHORT", department.NameShort)
                .Replace("INJECT_CODE_DEPARTMENT", department.Code)
                .Replace("INJECT_ROOM_DEPARTMENT", department.Room);
            
                var responseInsert = this.baseHttp.request(
                    path: String.Format("/API/REST/Entity/Insert/{0}", TypesUidElma.departments),
                    method: "POST",
                    body: textReqInsert
                ).bodyString;

                insertedData.Add(responseInsert);
            }

            // * ПРОВЕРКА запросы к Elma для новых справочников КАФЕДРЫ
            // foreach (var item in insertedData)
            // {
            //     var queryParams = new Dictionary<string, string>() {
            //         ["type"] = this.typesUidElma.departments,
            //         ["q"] = $"Id = {new Regex(@"[0-9]+").Match(item).ToString()}",
            //         ["limit"] = "1"
            //     };

            //     Console.WriteLine(
            //         this.baseHttp.request(
            //             path: "/API/REST/Entity/Query",
            //             method: "GET",
            //             queryParams: queryParams
            //         )
            //     );
            // }

            // logging information
            Log.Info(InfoTitle.dataElma, $"{departmentsElma.Count} departments in elma");
            Log.Info(InfoTitle.dataDB, $"{departmentsDB.Count} departments in database");
            if (departmentsMissed.Count != 0)
                Log.Info(InfoTitle.missed, $"{departmentsMissed.Count} missed departments");
            if (insertedData.Count != 0)
                Log.Success(SuccessTitle.injectedData, $"{insertedData.Count} injected departments to elma-server");
            else 
                Log.Success(SuccessTitle.synchronized, $"departments synchronized");
        }
   
        public void ProfilePrep( // профили
            List<PrepProfileDB> profilesDB
        ) {
            Log.Info(InfoTitle.uploadElma, "profiles preparations", colorTitle: "orangered1");

            // профили подготовки из Elma server
            List<PrepProfileElma> profilesElma = reqElma.preparationProfile();

            // list "prepatations profiles" that aren't in ELMA server but they're in database dekanat
            List<PrepProfileDB> profilesMissed = new List<PrepProfileDB>();

            // Хранилище ответов http на внедрение данных от сервера ELMA 
            List<string> insertedData = new List<string>();

            List<string> codeNameProfileELma = new List<string>();
            foreach (var profileElma in profilesElma)
                codeNameProfileELma.Add(profileElma.codeDirectPrep + " " + profileElma.name);

            foreach (var profileDB in profilesDB)
            {
                if (!codeNameProfileELma.Contains(profileDB.codeDirectPrep + " " + profileDB.name)) {
                    profilesMissed.Add(profileDB);
                }
            }


            // foreach (var item in profilesMissed)
            // {
            //     AnsiConsole.MarkupLine(item.codeDirectPrep + " " + item.name);
            // }


            // logging information
            Log.Info(InfoTitle.dataElma, $"{profilesElma.Count} profiles preparations in elma");
            Log.Info(InfoTitle.dataDB, $"{profilesDB.Count} profiles preparations in database");
            if (profilesMissed.Count != 0)
                Log.Info(InfoTitle.missed, $"{profilesMissed.Count} missed profiles preparations");
            if (insertedData.Count != 0)
                Log.Success(SuccessTitle.injectedData, $"{insertedData.Count} injected profiles preparations to elma-server");
            else 
                Log.Success(SuccessTitle.synchronized, $"profiles preparations synchronized");
        }
    }

}
