using System;
using System.Collections.Generic;

namespace ELMA_API
{
    class UploadData
    {
        public BaseHttp baseHttp;
        public RequestElma reqElma;
        public TypesUidElma typesUidElma;

        public UploadData(BaseHttp baseHttp, RequestElma reqElma, TypesUidElma typesUidElma) {
            this.baseHttp = baseHttp; // нужен для простых запросов-http к серверу Elma
            this.reqElma = reqElma; // нужен для уже подготовленных запросов к Elma
            this.typesUidElma = typesUidElma; // нужен для уникальных идентификаторов Elma
        }

        /// <summary>
        /// производит вызгрузку отсутсвующих данных на сервере ELMA
        /// </summary>
        /// <param name="jsonAuthResp">объект с атрибутами предназначенными для заголовков доступа в http запросе</param>
        /// <param name="plans">в данном объекте есть поле educational_plans_upload в котором находсят данные которых 
        /// нет на сервер ELMA и которые соотвественно функция загрзут</param>
        public void EducationalPlans(List<string> eduPlansDB)
        {
            Log.Success(SuccessTitle.uploadElma, "EDUCATIONAL PLANS");
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
            List<string> plans_inserted = new List<string>();

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
                    path: String.Format("/API/REST/Entity/Insert/{0}", this.typesUidElma.eduPlans),
                    method: "POST",
                    body: textReqInsert
                );

                // добавление результата запроса на внедрение данных в список
                plans_inserted.Add($"{plan} plan, id {responseInsert} successfully injected on ELMA");
            }

            // Logging information
            Log.Success(SuccessTitle.dataElma, $"{eduPlansElma.Count} educational plans in elma");
            Log.Success(SuccessTitle.dataDB, $"{eduPlansDB.Count} educational plans in database");
            Log.Success(SuccessTitle.missed, $"{eduPlansMissed.Count} missed educational plans");
            Log.Success(SuccessTitle.injectedData, $"{plans_inserted.Count} injected educational plans to elma-server");

        }

        public void Faculties(
            List<FacultyGuide> facultiesDB)
        {
            Log.Success(SuccessTitle.uploadElma, "FACULTIES");

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
                    path: String.Format("/API/REST/Entity/Insert/{0}", this.typesUidElma.faculties),
                    method: "POST",
                    body: textReqInsert
                );
                // добавление результата запроса на внедрение данных в список
                insertedData.Add($"{facultyMissed.longName} faculty, id {responseInsert} successfully injected on ELMA");
            }

            // logging information
            Log.Success(SuccessTitle.dataElma, $"{facultiesElma.Count} faculties in elma");
            Log.Success(SuccessTitle.dataDB, $"{facultiesDB.Count} faculties in database");
            Log.Success(SuccessTitle.missed, $"{facultiesMissed.Count} missed faculties plans");
            Log.Success(SuccessTitle.injectedData, $"{insertedData.Count} injected faculties to elma-server");
        }

        public void Disciplines(
            List<String> disciplinesDB)
        {
            Log.Success(SuccessTitle.uploadElma, "DISCIPLINES");

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
                    path: String.Format("/API/REST/Entity/Insert/{0}", this.typesUidElma.disciplines),
                    method: "POST",
                    body: textReqInsert
                );

                // добавление результата запроса на внедрение данных в список
                insertedData.Add($"{discipline} discipline, id {responseInsert} successfully injected on ELMA");
            }

            // logging information
            Log.Success(SuccessTitle.dataElma, $"{disciplinesElma.Count} disciplines in elma");
            Log.Success(SuccessTitle.dataDB, $"{disciplinesDB.Count} disciplines in database");
            Log.Success(SuccessTitle.missed, $"{disciplinesMissed.Count} missed disciplines plans");
            Log.Success(SuccessTitle.injectedData, $"{insertedData.Count} injected disciplines to elma-server");
        }
     
        public void DirecsPre( // загрузка направлений подготовки
            List<DirectionPreparation> direcsPreDB)
        {
            Log.Success(SuccessTitle.uploadElma, "DIRECTIONS PREPARATIONS");

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
                    path: String.Format("/API/REST/Entity/Insert/{0}", this.typesUidElma.direcPreparations),
                    method: "POST",
                    body: textReqInsert
                );

                // добавление результата запроса на внедрение данных в список
                insertedData.Add($"{directPre.Kod} direction preparations code, id {responseInsert} successfully injected on ELMA");
            }


            // logging information
            Log.Success(SuccessTitle.dataElma, $"{direcsPreElma.Count} directions preparations in elma");
            Log.Success(SuccessTitle.dataDB, $"{direcsPreDB.Count} directions preparations in database");
            Log.Success(SuccessTitle.missed, $"{direcsPreMissed.Count} missed directions preparations plans");
            Log.Success(SuccessTitle.injectedData, $"{insertedData.Count} injected directions preparations to elma-server");
        }
    
        public void Departments( // загрузка кафедр
            List<DepartmentFromDB> departmentsDB
        ) {
            Log.Success(SuccessTitle.uploadElma, "departments");

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
                var foundFaculty = this.reqElma.findFaculty(department.FacultyLong, department.FacultyShort);
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
                    path: String.Format("/API/REST/Entity/Insert/{0}", this.typesUidElma.departments),
                    method: "POST",
                    body: textReqInsert
                );

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
            Log.Success(SuccessTitle.dataElma, $"{departmentsElma.Count} departments in elma");
            Log.Success(SuccessTitle.dataDB, $"{departmentsDB.Count} departments in database");
            Log.Success(SuccessTitle.missed, $"{departmentsMissed.Count} missed departments");
            Log.Success(SuccessTitle.injectedData, $"{insertedData.Count} injected departments to elma-server");
        }
   
        public void ProfilePrep(
            List<PrepProfileDB> profilesDB
        ) {
            Log.Success(SuccessTitle.uploadElma, "profiles preparations");

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
            Log.Success(SuccessTitle.dataElma, $"{profilesElma.Count} departments in elma");
            Log.Success(SuccessTitle.dataDB, $"{profilesDB.Count} departments in database");
            Log.Success(SuccessTitle.missed, $"{profilesMissed.Count} missed departments");
            Log.Success(SuccessTitle.injectedData, $"{insertedData.Count} injected departments to elma-server");
        }
    }

}
