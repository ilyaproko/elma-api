using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace ELMA_API
{
    class BaseHttp
    {
        protected string hostaddress;

        public BaseHttp(string hostaddress) {
            // get environment variable localhost address
            this.hostaddress = hostaddress;
        }

        public String request(String path, String method, AuthJsonResponse authJson, String body = null)
        {
            HttpWebRequest req = WebRequest.Create(String.Format("http://" + this.hostaddress + path)) as HttpWebRequest;
            req.Method = method;
            req.Headers.Add("AuthToken", authJson.AuthToken);
            req.Headers.Add("SessionToken", authJson.SessionToken);
            req.Timeout = 10000;
            req.ContentType = "application/json; charset=utf-8";

            // * body request
            if (body != null) {
                var sendBody = Encoding.UTF8.GetBytes(body ?? "");
                req.ContentLength = sendBody.Length;
                Stream sendStream = req.GetRequestStream();
                sendStream.Write(sendBody, 0, sendBody.Length);
            }
            
            var res = req.GetResponse() as HttpWebResponse;
            var resStream = res.GetResponseStream();
            var sr = new StreamReader(resStream, Encoding.UTF8);

            return sr.ReadToEnd();
        }
    }

    class RequestElma : BaseHttp
    {

        public RequestElma(string hostaddress) : base(hostaddress) {}

        public List<string> educationalPlans(AuthJsonResponse authenticationJson, string typeUid_UchebnyePlany)
        {
            // ! -> typeUid_UchebnyePlany уникальный индентификатор для справочников 'учебные планы' 

            var getAllPlans = this.request(
                path: String.Format("/API/REST/Entity/Query?type={0}", typeUid_UchebnyePlany),
                method: "GET",
                authJson: authenticationJson
            ); // ! -> тип Тела-Ответа вернет как string(json)

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
        
        public List<FacultyGuide> faculties(AuthJsonResponse authenticationJson, string typeUid_faculties)
        {
            // ! -> typeUid_faculties уникальный индентификатор для справочников 'факультеты' из базы данных Elma

            var getAllFaculties = this.request(
                path: String.Format("/API/REST/Entity/Query?type={0}", typeUid_faculties),
                method: "GET",
                authJson: authenticationJson
            ); // ! -> тип Тела-Ответа вернет как string(json)
            
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

        public List<String> disciplines(AuthJsonResponse authenticationJson, string typeUid_discipline)
        {
            // ! -> typeUid_discipline уникальный индентификатор для справочников 'дисциплины' из базы данных Elma

            var getAllDisciplines = this.request(
                path: String.Format("/API/REST/Entity/Query?type={0}", typeUid_discipline),
                method: "GET",
                authJson: authenticationJson
            ); // ! -> тип Тела-Ответа вернет как string(json)

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

        public List<DirectionPreparation> directions_pre(AuthJsonResponse authenticationJson, string typeUid_directionPre)
        {
            // ! -> typeUid_directionPre уникальный индентификатор для справочников "направления подготовки" из базы данных Elma

            var getAllDirectionPre = this.request(
                path: String.Format("/API/REST/Entity/Query?type={0}", typeUid_directionPre),
                method: "GET",
                authJson: authenticationJson
            ); // ! -> тип Тела-Ответа вернет как string(json)

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
        
        public List<Department> departments(AuthJsonResponse authJson, string typeUid_department)
        {
            // ! -> typeUid_department уникальный индентификатор для справочников "кафедры" из базы данных Elma

            var getAllDepartments = this.request(
                path: String.Format("/API/REST/Entity/Query?type={0}", typeUid_department),
                method: "GET",
                authJson: authJson
            ); // ! -> тип Тела-Ответа вернет как string(json)

            // преобразование ответа от сервера типа string(json) в объектный тип
            List<Department> resJsonDepartments = 
                JsonConvert.DeserializeObject<List<Department>>(getAllDepartments);

            // Console.WriteLine(getAllDepartments);

            return resJsonDepartments;
        }
    }
}
