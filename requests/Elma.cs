using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace ELMA_API
{
    class Elma
    {
        public BaseHttp baseHttp;

        public Elma(BaseHttp baseHttp) 
        {
            this.baseHttp = baseHttp;
        }

        /// <summary>
        /// method for comfortable get needed data from List of Items 
        /// if pass nestedNameItem that mean that main Object has dependency
        /// that It has pair Name/Value that we want to get
        /// </summary>
        /// <param name="items">Список Item</param>
        /// <param name="nameItem">
        /// Принимает наименование Item или если это вложенный Item (т.е. зависимость)
        /// указывается наименование данной зависимости и обезательно параметр nestedItemName
        /// который и будет вытаскивать необходимый Item в этой вложенной завимимости
        /// </param>
        /// <param name="nestedItemName"></param>
        /// <returns>
        /// При условии что наименование Item указано правильно вернет значение 
        /// данного Item или если не найдет тогда null
        /// </returns>
        static public string getValueItem(
            List<Item> items, 
            string nameItem, 
            string nestedItemName = null)
        {
            foreach (var item in items) {
                // if nestedNameItem wasn't passed in 
                if (nestedItemName == null) { // GET OBJECT DATA WITH ITEMS WHICH IS QUALS nameItem
                    if (item.Name == nameItem) return item.Value; // RETURN !!!
                } else {
                    if (item.Name == nameItem && item.Data != null) { // GET OBJECT DATA WITH ITEMS WHICH IS QUALS nameItem
                        foreach (var itemNested in item.Data.Items) { // HAS NESTED DEPENDENCY
                            if (itemNested.Name == nestedItemName) return itemNested.Value; // RETURN !!!
                        }
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// поиск Справочника Направления Подготовки в ELMA по Id
        /// </summary>
        /// <param name="id">Id объекта-справочника elma "Напрпвление Подготовки"</param>
        /// <returns></returns>
        public Data findDirectPrepById(string id)
        {
            // пробуем найти Направление Подготовки ПО ID 
            // если не найдет тогда функция вернет null
            // и будет логгирования в консоле
            try 
            {
                var findDirectPrep = this.baseHttp.request<Data>(
                    path: "/API/REST/Entity/Load",
                    method: "GET",
                    queryParams: new Dictionary<string, string>() {
                        ["type"] = TypesUidElma.direcPreparations,
                        ["id"] = id
                    }
                );

                return findDirectPrep.jsonBody;

            } 
            catch (System.Exception) 
            {

                Log.Warn(WarnTitle.notFoundDirectPrep, $"Elma hasn't direction preparation with ID = {id ?? "null"}");
                return null;
            }
        }

        public List<string> educationalPlans()
        {
            // TypesUidElma.eduPlans уникальный индентификатор для справочников 'учебные планы' 
            var getAllPlans = this.baseHttp.request<List<Root>>(
                path: "/API/REST/Entity/Query",
                method: "GET",
                queryParams: new Dictionary<string, string>() {
                    ["type"] = TypesUidElma.eduPlans
                }
            );

            // list storage for uchebnyePlany from ELMA-server -> get value Naimenovanie (value example : z23030312-18.plx)
            List<string> eduPlansElma = new List<string>();

            // получение значений атрибута "Naimenovanie" справочника "учебные планы"
            foreach (Root plan in getAllPlans.jsonBody)
                eduPlansElma.Add(Elma.getValueItem(plan.Items, "Naimenovanie"));

            return eduPlansElma;
        }
        
        public List<FacultyGuide> faculties()
        {
            // TypesUidElma.faculties уникальный индентификатор для справочников 'факультеты' из базы данных Elma
            var getAllFaculties = this.baseHttp.request<List<Root>>(
                path: "/API/REST/Entity/Query",
                method: "GET",
                queryParams: new Dictionary<string, string>() {
                    ["type"] = TypesUidElma.faculties
                }
            );

            // list storage for faculties from Elma-server -> get values NaimenovanieSokraschennoe, NaimenovaniePolnoe
            List<FacultyGuide> faculties_elma = new List<FacultyGuide>();

            // получение значений атрибута "NaimenovanieSokraschennoe", "NaimenovaniePolnoe" справочника "факультеты"
            foreach (Root facultyElma in getAllFaculties.jsonBody) {
                FacultyGuide faculty = new FacultyGuide();

                faculty.longName = Elma.getValueItem(facultyElma.Items, "NaimenovaniePolnoe");
                faculty.shortName = Elma.getValueItem(facultyElma.Items, "NaimenovanieSokraschennoe");

                faculties_elma.Add(faculty);
            }

            return faculties_elma;
        }

        public List<String> disciplines()
        {
            // TypesUidElma.disciplines уникальный индентификатор для справочников 'дисциплины' из базы данных Elma
            var getAllDisciplines = this.baseHttp.request<List<Root>>(
                path: "/API/REST/Entity/Query",
                method: "GET",
                queryParams: new Dictionary<string, string>() {
                    ["type"] = TypesUidElma.disciplines
                }
            );

            // list storage for disciplines from Elma-server -> get Naimenovanie
            List<String> disciplines = new List<String>();

            // получение значений атрибута "Naimenovanie" справочника "дисциплины"
            foreach (var discipline in getAllDisciplines.jsonBody) {
                disciplines.Add(Elma.getValueItem(discipline.Items, "Naimenovanie"));
            }

            return disciplines;
        }

        /// направления подготовки
        public List<DirectionPreparation> directsPreps()
        {
            // TypesUidElma.direcPreparation уникальный индентификатор для справочников "направления подготовки" из базы данных Elma
            var getAllDirectionPre = this.baseHttp.request<List<Root>>(
                path: "/API/REST/Entity/Query",
                method: "GET",
                queryParams: new Dictionary<string, string>() {
                    ["type"] = TypesUidElma.direcPreparations
                }
            );

            // list storage for directions preparetions from Elma-server -> get Naimenovanie and Kod
            List<DirectionPreparation> direcsPre = new List<DirectionPreparation>();

            foreach (Root directPreElma in getAllDirectionPre.jsonBody)
            {
                var direcPre = new DirectionPreparation(); // создание сущности для напрв. подготовки

                direcPre.Kod = Elma.getValueItem(directPreElma.Items, "Kod");
                direcPre.Naimenovanie = Elma.getValueItem(directPreElma.Items, "Naimenovanie");

                direcsPre.Add(direcPre); // добавление направ. подготовки в хранилище
            }

            return direcsPre;
        }
        
        public List<Root> departments()
        {
            // TypesUidElma.department уникальный индентификатор для справочников "кафедры" из базы данных Elma
            var getAllDepartments = this.baseHttp.request<List<Root>>(
                path: "/API/REST/Entity/Query",
                method: "GET",
                queryParams: new Dictionary<string, string>() {
                    ["type"] = TypesUidElma.departments
                }
            ); 

            return getAllDepartments.jsonBody;
        }

        public List<Item> findFaculties(string nameLong, string nameShort)
        {
            // Elma Query Language - найти все ФАКУЛЬТЕТЫ 
            // где ПолноеНаименование и Сокращенное Наименование соответсвуют ЗНАЧЕНИЮ ПОИСКА
            // или где ПолноеНаименование НЕ ПУСТОЕ и СокращенноеНаименование соответсвует ЗНАЧЕНИЮ ПОИСКА
            // или где ПолноеНаименование соответсвует ЗНАЧЕНИЮ ПОИСКА а СокращенноеНаименование НЕ ПУСТОЕ
            var EQLquery = @$"
            (NaimenovaniePolnoe LIKE `{nameLong}` AND NaimenovanieSokraschennoe LIKE `{nameShort}`) 
            OR (NOT NaimenovaniePolnoe LIKE `` AND NaimenovanieSokraschennoe LIKE `{nameShort}`)
            OR (NaimenovaniePolnoe LIKE `{nameLong}` AND NOT NaimenovanieSokraschennoe LIKE ``)";
            
            // TypesUidElma.faculties уникальный идентификатор для справочников Факультеты на сервере Elma
            var findFaculty = this.baseHttp.request<List<Root>>(
                path: "/API/REST/Entity/Query",
                method: "GET",
                queryParams: new Dictionary<string, string>() {
                    ["type"] = TypesUidElma.faculties,
                    ["q"] = EQLquery,
                    ["limit"] = "1"
                }
            );

            return findFaculty.jsonBody.Count != 0 ? findFaculty.jsonBody[0].Items : null;
        }

        public List<PrepProfileElma> preparationProfile()
        {
            // TypesUidElma.preparationProfile уникальный индентификатор для справочников "профили подготовки" из базы данных Elma
            var getPreProfiles = this.baseHttp.request<List<Root>>(
                path: "/API/REST/Entity/Query",
                method: "GET",
                queryParams: new Dictionary<string, string>() {
                    ["type"] = TypesUidElma.preparationProfile
                }
            );

            // storage for Name Preparation Profile and Code's Direction Preparation 
            // -> Наименование Профеля Подготовки, Шифр и Id Направления подготовки
            List<PrepProfileElma> storageProfiles = new List<PrepProfileElma>();

            foreach (var profile in getPreProfiles.jsonBody)
            {   
                string nameProfile = Elma.getValueItem(profile.Items, "Naimenovanie"); // наименование подготовки
                string idDirectPrep = Elma.getValueItem(profile.Items, "NapravleniePodgotovki", "Id"); // ID направления подготовки
                string codeDirectPrep = null; // Шифр направления подготовки
                
                // если в профиле есть вложеннная зависимость -> Направление Подготовки
                // т.е. для данного профеля указано направление подготовки
                if (idDirectPrep != null) {
                    codeDirectPrep = Elma.getValueItem(this.findDirectPrepById(idDirectPrep).Items, "Kod"); 
                }
                
                // добавление в хранилище объекта
                storageProfiles.Add(
                    new PrepProfileElma(nameProfile, idDirectPrep, codeDirectPrep)
                );
            }

            return storageProfiles;
        }

        public List<Root> groups()
        {
            // TypesUidElma.groups уникальный иднетификатор для справочников Группы на сервере
            var getGroups = this.baseHttp.request<List<Root>>(
                path: "/API/REST/Entity/Query",
                method: "GET",
                queryParams: new Dictionary<string, string>() {
                    ["type"] = TypesUidElma.groups
                }
            );

            return getGroups.jsonBody;
        }

        public List<Root> students()
        {
            var getStudents = this.baseHttp.request<List<Root>>(
                path: "/API/REST/Entity/Query",
                method: "GET",
                queryParams: new Dictionary<string, string>() {
                    ["type"] = TypesUidElma.students
                }
            );

            return getStudents.jsonBody;
        }
        public List<Root> users() 
        {
            var getUsers = this.baseHttp.request<List<Root>>(
                path: "/API/REST/Entity/Query",
                method: "GET",
                queryParams: new Dictionary<string, string>() {
                    ["type"] = TypesUidElma.users
                }
            );

            return getUsers.jsonBody;
        }
    }
}
