using System;
using System.Net;
using System.IO;
using System.Text;
// для работы с response от сервера ELMA в формате json
using Newtonsoft.Json;

namespace ELMA_API
{
    class Program
    {

        static void Main(string[] args)
        {
            Logging.StartApp();
            Logging.Info(InfoTitle.launch, "start elma-api application is successful");
            
            // get environment variables
            var env = new EnvModule(".env");
            // token для http-зарпосов к серверу ELMA
            string token = env.getEnv("TOKEN");           
            string password = env.getEnv("PASSWORD");
            string hostaddress = env.getEnv("HOSTADDRESS");

            // -> получение AuthToken и SessionToken для запросов к серверу ELMA
            var auth = GetAuth(token, password); 

            // экземпляр класса с методами для запросов к серверу Elma
            // для получение данных от сервера elma, в основном данные из
            // объектов справочников
            var reqElma = new RequestElma(hostaddress);

            // экземпляр класса для загрузки данных на сервер Elma
            // которые есть в БД но отсутвуют на сервере Elma
            var uploadData = new UploadData(auth, hostaddress);

            // загрузка справочников "учебные планы" которые отсутсвуют на сервере ELMA
            uploadData.EducationalPlans(
                typeUid_uchebnyePlany: TypesUid_ELMA.edu_plans,
                // получение данных по справочнику "учебные планы" из сервера Elma
                eduPlansElma: reqElma.educationalPlans(auth, TypesUid_ELMA.edu_plans),
                // получение данных "учебные планы" из базы данных деканат
                eduPlansDB: RequestDatabase.getUchebnyePlany());

            // загрузка справочников "факультеты" которые отсутствуют на сервере ELMA
            uploadData.Faculties(
                typeUid_faculties: TypesUid_ELMA.faculties,
                facultiesElma: reqElma.faculties(auth, TypesUid_ELMA.faculties), // факультеты из Elma server
                facultiesDB: RequestDatabase.getFakuljtety()); // факультеты из БД деканат

            // загрузка спраовочников "дисциплины" которые отсутствуют на сервере ELMA
            uploadData.Disciplines(
                typeUid_discipline: TypesUid_ELMA.disciplines,
                disciplinesElma: reqElma.disciplines(auth, TypesUid_ELMA.disciplines), // дисциплины из Elma server
                disciplinesDB: RequestDatabase.getDisciplines()); // дисциплины из БД деканат

            // загрузка спраовочников "направления подготовки" которые отсутствуют на сервере ELMA
            uploadData.DirecsPre(
                typeUid_direcsPre: TypesUid_ELMA.directions_pre,
                direcsPreElma: reqElma.directions_pre(auth, TypesUid_ELMA.directions_pre), // напр. подготов. из Elma server
                direcsPreDB: RequestDatabase.getDirectionPreparation()); // направеления подготовки из БД деканат

            // var test = BaseHttp.request(
            //     url: "http://127.0.0.1:8000/API/REST/Entity/Load?type={TYPEUID}&id={ENTITYID}"
            //         .Replace("{TYPEUID}", "862af194-c1df-49c5-8692-21ffca0988c7")
            //         .Replace("{ENTITYID}", "1fd8ad56-8610-451f-a28d-15c4086e3864"),
            //     method: "GET",
            //     authJsonResponse
            // );

            // загрузка спраовочников "кафедры" которые отсутствуют на сервере ELMA
            uploadData.Departments(
                typeUid_department: TypesUid_ELMA.department,
                departmentsElma: reqElma.departments(auth, TypesUid_ELMA.department), // кафедры из Elma server
                departmentsDB: RequestDatabase.getDepartments()); // кафедры из БД деканат
        }

        static public AuthJsonResponse GetAuth(string applicationToken, string passW)
        {
            //создаем веб запрос
            HttpWebRequest req = WebRequest.Create(String.Format("http://localhost:8000/API/REST/Authorization/LoginWith?username=admin")) as HttpWebRequest;
            req.Headers.Add("ApplicationToken", applicationToken);
            req.Method = "POST";
            req.Timeout = 10000;
            req.ContentType = "application/json; charset=utf-8";

            // данные для отправки. используется для передачи пароля. пароль нужно записать вместо пустой строки
            // обезательно добавлять кавычики \" в начало и конец пароля !
            var sentData = Encoding.UTF8.GetBytes(passW);
            req.ContentLength = sentData.Length;
            Stream sendStream = req.GetRequestStream();
            sendStream.Write(sentData, 0, sentData.Length);

            //получение ответа
            var res = req.GetResponse() as HttpWebResponse;
            var resStream = res.GetResponseStream();
            var sr = new StreamReader(resStream, Encoding.UTF8);
            string responseBody = sr.ReadToEnd();

            // Пример ответа от сервера ELMA при вызове метода sr.ReadToEnd() вернет строковое представление
            // {
            //  "AuthToken":"e8d36fd8-fb2c-4f9b-b011-af8900bdbeb7",
            //  "CurrentUserId":"1",
            //  "Lang":"ru-RU",
            //  "SessionToken":"9F0E3BDF5678D3F4056471F585506003E8F343D46FC231A5FB526403AB902E1AC3E0B31D084EA5295147E0C154139509A6F7D567747FB5860772E814A0342C7F"
            // }

            // получение необходимых данных из запроса
            var responseJson = JsonConvert.DeserializeObject<AuthJsonResponse>(responseBody);

            // Logging 
            Logging.Info(InfoTitle.login_elma, "connection is successful");

            return responseJson;
        }

        // * уникальные идентификаторы типов спрвочников сервера ELMA
        public static class TypesUid_ELMA 
        {
            // учебные планы
            public static readonly string edu_plans = "4bbebaa3-4c81-4cdc-8115-23b9721726cb";
            // факультеты
            public static readonly string faculties = "862af194-c1df-49c5-8692-21ffca0988c7";
            // дисциплины
            public static readonly string disciplines = "b1bce0ec-1cf6-44ea-9915-844969fd7823";
            // напраления подготовки
            public static readonly string directions_pre = "c6f8bf73-f973-4f59-8fea-0084e3f95597";
            // кафедры
            public static readonly string department = "d65309ba-779a-4074-82a0-55560d8e4674"; 
        }
    }

}