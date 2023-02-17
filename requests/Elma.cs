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

        /// <summary>
        /// method for comfortable get needed data from List of Items 
        /// if pass nestedNameItem that mean that main Object has dependency
        /// that It has pair Name/Value that we want to get
        /// </summary>
        public string getValueItem(
            List<Item> items, 
            string nameItem, 
            string nestedItemName = null)
        {
            foreach (var item in items) {
                // if nestedNameItem wasn't passed in 
                if (nestedItemName == null) { // GET OBJECT DATA WITH ITEMS WHICH IS QUALS nameItem
                    if (item.Name == nameItem) return item.Value; // RETURN !!!
                } else {
                    if (item.Name == nameItem) { // GET OBJECT DATA WITH ITEMS WHICH IS QUALS nameItem
                        foreach (var itemNested in item.Data.Items) { // HAS NESTED DEPENDENCY
                            if (itemNested.Name == nestedItemName) return itemNested.Value; // RETURN !!!
                        }
                    }
                }
            }
            return null;
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
            List<Root> resJsonUchebnyePlany = JsonConvert.DeserializeObject<List<Root>>(getAllPlans);

            // list storage for uchebnyePlany from ELMA-server -> get value Naimenovanie (value example : z23030312-18.plx)
            List<string> eduPlansElma = new List<string>();

            // получение значений атрибута "Naimenovanie" справочника "учебные планы"
            foreach (Root row in resJsonUchebnyePlany)
                eduPlansElma.Add(this.getValueItem(row.Items, "Naimenovanie"));

            return eduPlansElma;
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
            List<Root> resJsonFaculties = JsonConvert.DeserializeObject<List<Root>>(getAllFaculties);

            // list storage for faculties from Elma-server -> get values NaimenovanieSokraschennoe, NaimenovaniePolnoe
            List<FacultyGuide> faculties_elma = new List<FacultyGuide>();

            // получение значений атрибута "NaimenovanieSokraschennoe", "NaimenovaniePolnoe" справочника "факультеты"
            foreach (Root row in resJsonFaculties) {
                FacultyGuide faculty = new FacultyGuide();
                foreach (Item cell in row.Items) {
                    if (cell.Name == "NaimenovaniePolnoe") faculty.longName = cell.Value;
                    if (cell.Name == "NaimenovanieSokraschennoe") faculty.shortName = cell.Value;
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
            List<Root> resJsonDiscipline = JsonConvert.DeserializeObject<List<Root>>(getAllDisciplines);

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

        /// направления подготовки
        public List<DirectionPreparation> directsPreps()
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
            List<Root> resJsonDirectionsPre = 
                JsonConvert.DeserializeObject<List<Root>>(getAllDirectionPre);

            // list storage for directions preparetions from Elma-server -> get Naimenovanie and Kod
            List<DirectionPreparation> direcsPre = new List<DirectionPreparation>();

            foreach (Root item in resJsonDirectionsPre)
            {
                var direcPre = new DirectionPreparation(); // создание сущности для напрв. подготовки
                foreach (Item datas in item.Items)
                {
                    if (datas.Name == "Kod") direcPre.Kod = datas.Value;
                    if (datas.Name == "Naimenovanie") direcPre.Naimenovanie = datas.Value;
                }
                direcsPre.Add(direcPre); // добавление направ. подготовки в хранилище
            }

            return direcsPre;
        }

        /// поиск Справочника Направления Подготовки в ELMA по Id
        public Data findDirectPrepById(string id)
        {
            var queryParameters = new Dictionary<string, string>() {
                ["type"] = this.typesUidElma.direcPreparations,
                ["id"] = id
            };

            // пробуем найти Направление Подготовки ПО ID 
            // если не найдет тогда функция вернет null
            // и будет логгирования в консоле
            try {
                var findDirectPrep = this.baseHttp.request(
                    path: "/API/REST/Entity/Load",
                    queryParams: queryParameters,
                    method: "GET"
                );

                // преобразование ответа от сервера типа string(json) в объектный тип
                Data directPrep = JsonConvert.DeserializeObject<Data>(findDirectPrep);

                return directPrep;
            } catch (System.Exception) {  
                Logging.Warn(WarnTitle.notFoundDirectPrep, $"Elma hasn't direction preparation with ID = {id ?? "null"}");
                return null;
            }
        }
        
        public List<Root> departments()
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
            List<Root> resJsonDepartments = 
                JsonConvert.DeserializeObject<List<Root>>(getAllDepartments);

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

        public List<PrepProfileElma> preparationProfile()
        {
            // this.typesUidElma.preparationProfile уникальный индентификатор для справочников "профили подготовки" из базы данных Elma
            // define query parameters for url-http
            var queryParameters = new Dictionary<string, string>() {
                ["type"] = this.typesUidElma.preparationProfile
            };

            var getPreProfiles = this.baseHttp.request(
                path: "/API/REST/Entity/Query",
                queryParams: queryParameters,
                method: "GET"
            ); // -> тип Тела-Ответа вернет как string(json)

            // storage for Name Preparation Profile and Code's Direction Preparation 
            // -> Наименование Профеля Подготовки, Шифр и Id Направления подготовки
            List<PrepProfileElma> storageProfiles = new List<PrepProfileElma>();
            

            // преобразование ответа от сервера типа string(json) в объектный тип
            List<Root> resPreProfiles = 
                JsonConvert.DeserializeObject<List<Root>>(getPreProfiles);

            foreach (var profile in resPreProfiles)
            {   
                string nameProfile = this.getValueItem(profile.Items, "Naimenovanie"); // наименование подготовки
                string idDirectPrep = this.getValueItem(profile.Items, "NapravleniePodgotovki", "Id"); // ID направления подготовки
                string codeDirectPrep = null; // Шифр направления подготовки
                
                // если в профиле есть вложеннная зависимость -> Направление Подготовки
                if (idDirectPrep != null) {
                    codeDirectPrep = this.getValueItem(this.findDirectPrepById(idDirectPrep).Items, "Kod"); 
                }
                
                // добавление в хранилище объекта
                storageProfiles.Add(
                    new PrepProfileElma(nameProfile, idDirectPrep, codeDirectPrep)
                );
            }

            return storageProfiles;
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
