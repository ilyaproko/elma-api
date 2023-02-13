using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace ELMA_API
{
    class RequestElma
    {
        public BaseHttp baseHttp;

        public RequestElma(BaseHttp baseHttp) 
        {
            this.baseHttp = baseHttp;
        }

        public List<string> educationalPlans(string typeUid_UchebnyePlany)
        {
            // ! -> typeUid_UchebnyePlany уникальный индентификатор для справочников 'учебные планы' 

            var getAllPlans = this.baseHttp.request(
                path: String.Format("/API/REST/Entity/Query?type={0}", typeUid_UchebnyePlany),
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
        
        public List<FacultyGuide> faculties(string typeUid_faculties)
        {
            // ! -> typeUid_faculties уникальный индентификатор для справочников 'факультеты' из базы данных Elma

            var getAllFaculties = this.baseHttp.request(
                path: String.Format("/API/REST/Entity/Query?type={0}", typeUid_faculties),
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

        public List<String> disciplines(string typeUid_discipline)
        {
            // ! -> typeUid_discipline уникальный индентификатор для справочников 'дисциплины' из базы данных Elma

            var getAllDisciplines = this.baseHttp.request(
                path: String.Format("/API/REST/Entity/Query?type={0}", typeUid_discipline),
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

        public List<DirectionPreparation> directions_pre(string typeUid_directionPre)
        {
            // ! -> typeUid_directionPre уникальный индентификатор для справочников "направления подготовки" из базы данных Elma

            var getAllDirectionPre = this.baseHttp.request(
                path: String.Format("/API/REST/Entity/Query?type={0}", typeUid_directionPre),
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
        
        public List<Department> departments(string typeUid_department)
        {
            // ! -> typeUid_department уникальный индентификатор для справочников "кафедры" из базы данных Elma

            var getAllDepartments = this.baseHttp.request(
                path: String.Format("/API/REST/Entity/Query?type={0}", typeUid_department),
                method: "GET"
            ); // -> тип Тела-Ответа вернет как string(json)

            // преобразование ответа от сервера типа string(json) в объектный тип
            List<Department> resJsonDepartments = 
                JsonConvert.DeserializeObject<List<Department>>(getAllDepartments);

            // Console.WriteLine(getAllDepartments);

            return resJsonDepartments;
        }


        public List<String> groups(string typeUidGroups)
        {
            var getGroups = this.baseHttp.request(
                path: String.Format("/API/REST/Entity/Query?type={0}", typeUidGroups),
                method: "GET"
            ); // -> тип Тела-Ответа вернет как string(json)

            List<string> uniqueNameGroups = new List<string>();

            // преобразование ответа от сервера типа string(json) в объектный тип
            List<Root> respGroups = JsonConvert.DeserializeObject<List<Root>>(getGroups);

            foreach (var group in respGroups)
                foreach (var item in group.Items)
                    if (item.Name == "Naimenovanie" 
                        && !uniqueNameGroups.Contains(item.Value)) uniqueNameGroups.Add(item.Value);

            return new List<string>();
        }
    }
}
