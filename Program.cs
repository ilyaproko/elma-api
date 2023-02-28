using System;
using System.Collections.Generic;
using System.Text;
using Spectre.Console;


namespace ELMA_API
{
    class Program
    {
        static void Main(string[] args)
        {
            // this setting need for spinner progress status currently show up
            System.Console.OutputEncoding = Encoding.UTF8;
            System.Console.InputEncoding = Encoding.UTF8;
            // end setting block
            
            Logging.StartApp();
            Logging.Info(InfoTitle.launch, "start elma-api application is successful");
            
            // get environment variables
            var env = new EnvModule(".env");
            string user = env.getEnv("USER");
            string token = env.getEnv("TOKEN");
            string password = env.getEnv("PASSWORD");
            string hostaddress = env.getEnv("HOSTADDRESS");

            var baseHttpElma = new BaseHttp(hostaddress, token, user, password);

            // экземпляр с атрибутами уникальных идентификаторов Спраовчников Elma
            // нужен как Зависимость только для UploadData и RequestElma
            // т.е. там где нужны запросы к серверу Elma
            var typesUidElma = new TypesUidElma();
            
            // экземпляр класса с методами для запросов к серверу Elma
            // для получение данных от сервера elma, в основном данные из
            // объектов справочников
            var reqElma = new RequestElma(baseHttpElma, typesUidElma);
            
            // экземпляр класса для загрузки данных на сервер Elma
            // которые есть в БД но отсутвуют на сервере Elma
            var uploadData = new UploadData(baseHttpElma, reqElma, typesUidElma);
            
            // выгрузка данных на сервер elma из excel файлов
            var uploadFromExcel = new UploadFromExcel(baseHttpElma, reqElma, typesUidElma);

            // раздел для выгрузки данных
            // спинер Информирующий о Стутесе Процесса Программы
            // AnsiConsole.Status()
            //     .Spinner(Spinner.Known.BouncingBall)
            //     .SpinnerStyle(Style.Parse("green1"))
            //     .Start("[white]Process uploding[/]", ctx => 
            // {
            //     // загрузка справочников "учебные планы" которые отсутсвуют на сервере ELMA
            //     uploadData.EducationalPlans(
            //         eduPlansDB: RequestDatabase.getUchebnyePlany()); // "учебные планы" из базы данных деканат

            //     // загрузка справочников "факультеты" которые отсутствуют на сервере ELMA
            //     uploadData.Faculties(
            //         facultiesDB: RequestDatabase.getFakuljtety()); // факультеты из БД деканат
            
            //     // загрузка спраовочников "дисциплины" которые отсутствуют на сервере ELMA
            //     uploadData.Disciplines(
            //         disciplinesDB: RequestDatabase.getDisciplines()); // дисциплины из БД деканат

            //     // загрузка спраовочников "направления подготовки" которые отсутствуют на сервере ELMA
            //     uploadData.DirecsPre(
            //         direcsPreDB: RequestDatabase.getDirectionPreparation()); // направеления подготовки из БД деканат

            //     // загрузка спраовочников "кафедры" которые отсутствуют на сервере ELMA
            //     uploadData.Departments(
            //         departmentsDB: RequestDatabase.getDepartments()); // кафедры из БД деканат

            //     // загрузка спраовочников "профили подготовки" которые отсутствуют на сервере ELMA
            //     uploadData.ProfilePrep(
            //         profilesDB: RequestDatabase.getPrepProfiles()); // профили подготовки из БД деканат
            // });

            uploadFromExcel.academicTitle("C:\\Users\\prokopiev_ia\\Desktop\\ППС.xlsx");

            // var queryParameters = new Dictionary<string, string>() {
            //     ["type"] = typesUidElma.students,
            //     ["limit"] = "10"
            // };

            // Console.WriteLine(baseHttpElma.request(
            //     path: "/API/REST/Entity/Query",
            //     queryParams: queryParameters,
            //     method: "GET"));


        } 
    }

    // * уникальные идентификаторы типов спрвочников сервера ELMA
    public class TypesUidElma 
    {
        // учебные планы
        public readonly string eduPlans = "4bbebaa3-4c81-4cdc-8115-23b9721726cb";
        // факультеты
        public readonly string faculties = "862af194-c1df-49c5-8692-21ffca0988c7";
        // дисциплины
        public readonly string disciplines = "b1bce0ec-1cf6-44ea-9915-844969fd7823";
        // напраления подготовки
        public readonly string direcPreparations = "c6f8bf73-f973-4f59-8fea-0084e3f95597";
        // кафедры
        public readonly string departments = "d65309ba-779a-4074-82a0-55560d8e4674"; 
        // направления подготовки
        public readonly string preparationProfile = "92392fcf-620d-4f0c-bede-af6dffbc41c4";
        // группы
        public readonly string groups = "1b5dca14-da97-4a7e-816f-b3531276149c";
        // пользователя - Системый Справочник Elma
        public readonly string users = "18faf3ae-03c9-4e64-b02a-95dd63e54c4d";
        // студенты
        public readonly string students = " eb7e76a9-9bd1-410f-b8ff-877ce69ea850";
    }

}