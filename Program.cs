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
            string user = env.getEnv("USER");
            string token = env.getEnv("TOKEN");
            string password = env.getEnv("PASSWORD");
            string hostaddress = env.getEnv("HOSTADDRESS");

            var baseHttpElma = new BaseHttp(hostaddress, token, user, password);
            // экземпляр класса с методами для запросов к серверу Elma
            // для получение данных от сервера elma, в основном данные из
            // объектов справочников
            var reqElma = new RequestElma(baseHttpElma);
            // экземпляр класса для загрузки данных на сервер Elma
            // которые есть в БД но отсутвуют на сервере Elma
            var uploadData = new UploadData(baseHttpElma);

            // загрузка справочников "учебные планы" которые отсутсвуют на сервере ELMA
            uploadData.EducationalPlans(
                typeUid_uchebnyePlany: TypesUid.edu_plans,
                // получение данных по справочнику "учебные планы" из сервера Elma
                eduPlansElma: reqElma.educationalPlans(TypesUid.edu_plans),
                // получение данных "учебные планы" из базы данных деканат
                eduPlansDB: RequestDatabase.getUchebnyePlany());

            // загрузка справочников "факультеты" которые отсутствуют на сервере ELMA
            uploadData.Faculties(
                typeUid_faculties: TypesUid.faculties,
                facultiesElma: reqElma.faculties(TypesUid.faculties), // факультеты из Elma server
                facultiesDB: RequestDatabase.getFakuljtety()); // факультеты из БД деканат

            // загрузка спраовочников "дисциплины" которые отсутствуют на сервере ELMA
            uploadData.Disciplines(
                typeUid_discipline: TypesUid.disciplines,
                disciplinesElma: reqElma.disciplines(TypesUid.disciplines), // дисциплины из Elma server
                disciplinesDB: RequestDatabase.getDisciplines()); // дисциплины из БД деканат

            // загрузка спраовочников "направления подготовки" которые отсутствуют на сервере ELMA
            uploadData.DirecsPre(
                typeUid_direcsPre: TypesUid.directions_pre,
                direcsPreElma: reqElma.directions_pre(TypesUid.directions_pre), // напр. подготов. из Elma server
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
                typeUid_department: TypesUid.department,
                departmentsElma: reqElma.departments(TypesUid.department), // кафедры из Elma server
                departmentsDB: RequestDatabase.getDepartments()); // кафедры из БД деканат

            
            
        }


        // * уникальные идентификаторы типов спрвочников сервера ELMA
        public static class TypesUid 
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


            public static readonly string groups = "1b5dca14-da97-4a7e-816f-b3531276149c";
        }
    }

}