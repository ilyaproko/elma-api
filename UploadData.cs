using System;
using System.Collections.Generic;

namespace ELMA_API
{
    class UploadData : BaseHttp
    {
        public AuthJsonResponse authenticationJson;

        public UploadData(AuthJsonResponse authenticationJson, string hostaddress) : base(hostaddress) {
            this.authenticationJson = authenticationJson;
        }


        /// <summary>
        /// производит вызгрузку отсутсвующих данных на сервере ELMA
        /// </summary>
        /// <param name="jsonAuthResp">объект с атрибутами предназначенными для заголовков доступа в http запросе</param>
        /// <param name="typeUid_uchebnyePlany">уникальный идентификатор типа на сервере ELMA для спраовчника 'учебные планы'</param>
        /// <param name="plans">в данном объекте есть поле educational_plans_upload в котором находсят данные которых 
        /// нет на сервер ELMA и которые соотвественно функция загрзут</param>
        public void EducationalPlans(
            string typeUid_uchebnyePlany,
            List<string> eduPlansElma,
            List<string> eduPlansDB)
        {
            Logging.Info(InfoTitle.start_upload, "EDUCATIONAL PLANS");

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

                var responseInsert = this.request(
                    path: String.Format("/API/REST/Entity/Insert/{0}", typeUid_uchebnyePlany),
                    method: "POST",
                    authJson: this.authenticationJson,
                    body: textReqInsert
                );

                // добавление результата запроса на внедрение данных в список
                plans_inserted.Add($"{plan} plan, id {responseInsert} successfully injected on ELMA");
            }

            // Logging information
            Logging.Info(InfoTitle.data_elma, $"{eduPlansElma.Count} educational plans in elma");
            Logging.Info(InfoTitle.data_db, $"{eduPlansDB.Count} educational plans in database");
            Logging.Info(InfoTitle.missed, $"{eduPlansMissed.Count} missed educational plans");
            Logging.Info(InfoTitle.inject_data, $"{plans_inserted.Count} injected educational plans to elma-server");

        }

        public void Faculties(
            string typeUid_faculties,
            List<FacultyGuide> facultiesElma,
            List<FacultyGuide> facultiesDB)
        {
            Logging.Info(InfoTitle.start_upload, "FACULTIES");

            // list faculties that aren't in ELMA server but they're in database dekanat
            List<FacultyGuide> facultiesMissed = new List<FacultyGuide>();

            // проверяем полученный факультет из базы, есть ли он на сервере elma, 
            // если нет тогда добавляем хранилище
            foreach (FacultyGuide facultyDB in facultiesDB)
            {
                var matches = facultiesElma.FindAll(facultyElma => facultyElma.long_name == facultyDB.long_name);
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
                .Replace("INJECTED_VALUE_LONG_NAME", facultyMissed.long_name) // заменяем значение в тексте
                .Replace("INJECTED_VALUE_SHORT_NAME", facultyMissed.short_name); // заменяем значение в тексте

                var responseInsert = this.request(
                    path: String.Format("/API/REST/Entity/Insert/{0}", typeUid_faculties),
                    method: "POST",
                    authJson: this.authenticationJson,
                    body: textReqInsert
                );
                // добавление результата запроса на внедрение данных в список
                insertedData.Add($"{facultyMissed.long_name} faculty, id {responseInsert} successfully injected on ELMA");
            }

            // logging information
            Logging.Info(InfoTitle.data_elma, $"{facultiesElma.Count} faculties in elma");
            Logging.Info(InfoTitle.data_db, $"{facultiesDB.Count} faculties in database");
            Logging.Info(InfoTitle.missed, $"{facultiesMissed.Count} missed faculties plans");
            Logging.Info(InfoTitle.inject_data, $"{insertedData.Count} injected faculties to elma-server");
        }

        public void Disciplines(
            string typeUid_discipline,
            List<String> disciplinesElma,
            List<String> disciplinesDB)
        {
            Logging.Info(InfoTitle.start_upload, "DISCIPLINES");

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

                var responseInsert = this.request(
                    path: String.Format("/API/REST/Entity/Insert/{0}", typeUid_discipline),
                    method: "POST",
                    authJson: this.authenticationJson,
                    body: textReqInsert
                );

                // добавление результата запроса на внедрение данных в список
                insertedData.Add($"{discipline} discipline, id {responseInsert} successfully injected on ELMA");
            }

            // logging information
            Logging.Info(InfoTitle.data_elma, $"{disciplinesElma.Count} disciplines in elma");
            Logging.Info(InfoTitle.data_db, $"{disciplinesDB.Count} disciplines in database");
            Logging.Info(InfoTitle.missed, $"{disciplinesMissed.Count} missed disciplines plans");
            Logging.Info(InfoTitle.inject_data, $"{insertedData.Count} injected disciplines to elma-server");
        }
     
        public void DirecsPre( // загрузка направлений подготовки
            string typeUid_direcsPre,
            List<DirectionPreparation> direcsPreElma,
            List<DirectionPreparation> direcsPreDB)
        {
            Logging.Info(InfoTitle.start_upload, "DIRECTIONS PREPARATIONS");

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

                var responseInsert = this.request(
                    path: String.Format("/API/REST/Entity/Insert/{0}", typeUid_direcsPre),
                    method: "POST",
                    authJson: this.authenticationJson,
                    body: textReqInsert
                );

                // добавление результата запроса на внедрение данных в список
                insertedData.Add($"{directPre.Kod} direction preparations code, id {responseInsert} successfully injected on ELMA");
            }


            // logging information
            Logging.Info(InfoTitle.data_elma, $"{direcsPreElma.Count} directions preparations in elma");
            Logging.Info(InfoTitle.data_db, $"{direcsPreDB.Count} directions preparations in database");
            Logging.Info(InfoTitle.missed, $"{direcsPreMissed.Count} missed directions preparations plans");
            Logging.Info(InfoTitle.inject_data, $"{insertedData.Count} injected directions preparations to elma-server");
        }
    
        public void Departments( // загрузка кафедр
            string typeUid_department,
            List<Department> departmentsElma,
            List<DepartmentFromDB> departmentsDB
        ) {
            Logging.Info(InfoTitle.start_upload, "departments");

            // list departments that aren't in ELMA server but they're in database dekanat
            List<DirectionPreparation> departmentsMissed = new List<DirectionPreparation>();

            // получаем все код кафедр которые есть на сервере Elma
            List<String> codesDepElma = new List<String>();
            // получаем ячейку "KodKafedry" и проверяем есть ли она в хранилище 
            // codesDepElma, если нет тогда добавляем
            foreach (Department dep in departmentsElma)
                foreach (ItemDepartment itemDep in dep.Items)
                    if (itemDep.Name == "KodKafedry" 
                    && !codesDepElma.Contains(itemDep.Value)) codesDepElma.Add(itemDep.Value);

            

            
            // logging information
            Logging.Info(InfoTitle.data_elma, $"{departmentsElma.Count} departments in elma");
            Logging.Info(InfoTitle.data_db, $"{departmentsDB.Count} departments in database");
            Logging.Info(InfoTitle.missed, $"{departmentsMissed.Count} departments in database");
        }
    }

}
