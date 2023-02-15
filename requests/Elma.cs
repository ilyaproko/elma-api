using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace ELMA_API
{
    class RequestElma
    {
        public BaseHttp baseHttp;
        public TypesUidElma typesUidElma;

        public RequestElma(BaseHttp baseHttp, TypesUidElma typesUidElma) 
        {
            this.baseHttp = baseHttp;
            this.typesUidElma = typesUidElma;
        }

        public List<string> educationalPlans()
        {
            // this.typesUidElma.eduPlans уникальный индентификатор для справочников 'учебные планы' 
            // define query parameters for url-http
            var queryParameters = new Dictionary<string, string>() {
                ["type"] = this.typesUidElma.eduPlans
            };

            var getAllPlans = this.baseHttp.request(
                path: "/API/REST/Entity/Query",
                queryParams: queryParameters,
                method: "GET"
            ); // -> тип Тела-Ответа вернет как string(json)

            // преобразование ответа от сервера типа string(json) в объектный тип
            List<RowResponseEduPlan> resJsonUchebnyePlany = JsonConvert.DeserializeObject<List<RowResponseEduPlan>>(getAllPlans);

            // list storage for uchebnyePlany from ELMA-server -> get value Naimenovanie (value example : z23030312-18.plx"")
            List<string> educational_plans_elma = new List<string>();

            // получение значений атрибута "Naimenovanie" справочника "учебные планы"
            foreach (RowResponseEduPlan row in resJsonUchebnyePlany)
                foreach (CellResponseEduPlan item in row.Items)
                    if (item.Name == "Naimenovanie") educational_plans_elma.Add(item.Value);

            return educational_plans_elma;
        }
        
        public List<FacultyGuide> faculties()
        {
            // this.typesUidElma.faculties уникальный индентификатор для справочников 'факультеты' из базы данных Elma
            // define query parameters for url-http
            var queryParameters = new Dictionary<string, string>() {
                ["type"] = this.typesUidElma.faculties
            };

            var getAllFaculties = this.baseHttp.request(
                path: "/API/REST/Entity/Query",
                queryParams: queryParameters,
                method: "GET"
            ); // -> тип Тела-Ответа вернет как string(json)
            
            // преобразование ответа от сервера типа string(json) в объектный тип
            List<RowResponseFaculty> resJsonFaculties = JsonConvert.DeserializeObject<List<RowResponseFaculty>>(getAllFaculties);

            // list storage for faculties from Elma-server -> get values NaimenovanieSokraschennoe, NaimenovaniePolnoe
            List<FacultyGuide> faculties_elma = new List<FacultyGuide>();

            // получение значений атрибута "NaimenovanieSokraschennoe", "NaimenovaniePolnoe" справочника "факультеты"
            foreach (RowResponseFaculty row in resJsonFaculties) {
                FacultyGuide faculty = new FacultyGuide();
                foreach (CellResponseFaculty cell in row.Items) {
                    if (cell.Name == "NaimenovaniePolnoe") faculty.long_name = cell.Value;
                    if (cell.Name == "NaimenovanieSokraschennoe") faculty.short_name = cell.Value;
                }
                faculties_elma.Add(faculty);
            }

            return faculties_elma;
        }

        public List<String> disciplines()
        {
            // this.typesUidElma.disciplines уникальный индентификатор для справочников 'дисциплины' из базы данных Elma
            // define query parameters for url-http
            var queryParameters = new Dictionary<string, string>() {
                ["type"] = this.typesUidElma.disciplines
            };

            var getAllDisciplines = this.baseHttp.request(
                path: "/API/REST/Entity/Query",
                queryParams: queryParameters,
                method: "GET"
            ); // -> тип Тела-Ответа вернет как string(json)

            // преобразование ответа от сервера типа string(json) в объектный тип
            List<Discipline> resJsonDiscipline = JsonConvert.DeserializeObject<List<Discipline>>(getAllDisciplines);

            // list storage for disciplines from Elma-server -> get Naimenovanie
            List<String> disciplines = new List<String>();

            // получение значений атрибута "Naimenovanie" справочника "дисциплины"
            foreach (var discipline in resJsonDiscipline) {
                foreach (var dataDiscipline in discipline.Items) {
                    if (dataDiscipline.Name == "Naimenovanie") disciplines.Add(dataDiscipline.Value);
                }
            }

            return disciplines;
        }

        public List<DirectionPreparation> directions_pre()
        {
            // this.typesUidElma.direcPreparation уникальный индентификатор для справочников "направления подготовки" из базы данных Elma
            // define query parameters for url-http
            var queryParameters = new Dictionary<string, string>() {
                ["type"] = this.typesUidElma.direcPreparations
            };

            var getAllDirectionPre = this.baseHttp.request(
                path: "/API/REST/Entity/Query",
                queryParams: queryParameters,
                method: "GET"
            ); // -> тип Тела-Ответа вернет как string(json)

            // преобразование ответа от сервера типа string(json) в объектный тип
            List<DirectionsPre> resJsonDirectionsPre = 
                JsonConvert.DeserializeObject<List<DirectionsPre>>(getAllDirectionPre);

            // list storage for directions preparetions from Elma-server -> get Naimenovanie and Kod
            List<DirectionPreparation> direcsPre = new List<DirectionPreparation>();

            foreach (DirectionsPre item in resJsonDirectionsPre)
            {
                var direcPre = new DirectionPreparation(); // создание сущности для напрв. подготовки
                foreach (DataDirectionsPre datas in item.Items)
                {
                    if (datas.Name == "Kod") direcPre.Kod = datas.Value;
                    if (datas.Name == "Naimenovanie") direcPre.Naimenovanie = datas.Value;
                }
                direcsPre.Add(direcPre); // добавление направ. подготовки в хранилище
            }

            return direcsPre;
        }
        
        public List<Department> departments()
        {
            // this.typesUidElma.department уникальный индентификатор для справочников "кафедры" из базы данных Elma
            // define query parameters for url-http
            var queryParameters = new Dictionary<string, string>() {
                ["type"] = this.typesUidElma.departments
            };

            var getAllDepartments = this.baseHttp.request(
                path: "/API/REST/Entity/Query",
                queryParams: queryParameters,
                method: "GET"
            ); // -> тип Тела-Ответа вернет как string(json)

            // преобразование ответа от сервера типа string(json) в объектный тип
            List<Department> resJsonDepartments = 
                JsonConvert.DeserializeObject<List<Department>>(getAllDepartments);

            // Console.WriteLine(getAllDepartments);

            return resJsonDepartments;
        }

        public List<Item> findFaculty(string nameLong, string nameShort)
        {
            // Elma Query Language - найти все ФАКУЛЬТЕТЫ 
            // где ПолноеНаименование и Сокращенное Наименование соответсвуют ЗНАЧЕНИЮ ПОИСКА
            // или где ПолноеНаименование НЕ ПУСТОЕ и СокращенноеНаименование соответсвует ЗНАЧЕНИЮ ПОИСКА
            // или где ПолноеНаименование соответсвует ЗНАЧЕНИЮ ПОИСКА а СокращенноеНаименование НЕ ПУСТОЕ
            var EQLquery = @$"
            (NaimenovaniePolnoe LIKE `{nameLong}` AND NaimenovanieSokraschennoe LIKE `{nameShort}`) 
            OR (NOT NaimenovaniePolnoe LIKE `` AND NaimenovanieSokraschennoe LIKE `{nameShort}`)
            OR (NaimenovaniePolnoe LIKE `{nameLong}` AND NOT NaimenovanieSokraschennoe LIKE ``)";
            // this.typesUidElma.faculties уникальный идентификатор для справочников Факультеты на сервере Elma
            // define query parameters for url-http
            var queryParameters = new Dictionary<string, string>() {
                ["type"] = this.typesUidElma.faculties,
                ["q"] = EQLquery,
                ["limit"] = "1"
            };

            var findFaculty = this.baseHttp.request(
                path: "/API/REST/Entity/Query",
                queryParams: queryParameters,
                method: "GET"
            );

            // преобразование ответа от сервера типа string(json) в объектный тип
            List<Root> resJsonFaculty = JsonConvert.DeserializeObject<List<Root>>(findFaculty);

            // Console.WriteLine($"\n\nNaimenovaniePolnoe LIKE `{nameLong}` OR NaimenovanieSokraschennoe LIKE `{nameShort}`");
            // Console.WriteLine(findFaculty);

            return resJsonFaculty.Count != 0 ? resJsonFaculty[0].Items : null;
        }


        public List<String> groups()
        {
            // this.typesUidElma.groups уникальный иднетификатор для справочников Группы на сервере
            // define query parameters for url-http
            var queryParameters = new Dictionary<string, string>() {
                ["type"] = this.typesUidElma.groups
            };

            var getGroups = this.baseHttp.request(
                path: "/API/REST/Entity/Query",
                queryParams: queryParameters,
                method: "GET"
            ); // -> тип Тела-Ответа вернет как string(json)

            List<string> uniqueNameGroups = new List<string>();

            // преобразование ответа от сервера типа string(json) в объектный тип
            List<Root> respGroups = JsonConvert.DeserializeObject<List<Root>>(getGroups);

            foreach (var group in respGroups)
                foreach (var item in group.Items)
                    if (item.Name == "Naimenovanie" 
                        && !uniqueNameGroups.Contains(item.Value)) uniqueNameGroups.Add(item.Value);

            foreach (var item in uniqueNameGroups)
            {
                Console.WriteLine(item);
            }
            Console.WriteLine(uniqueNameGroups.Count);


            return new List<string>();
        }
    }
}
