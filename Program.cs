using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using Newtonsoft.Json;
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
            
            Log.Launch();
            Log.Success(SuccessTitle.launch, "start elma-api application is successful");
            
            // get environment variables
            var env = new EnvModule(".env");
            string user = env.getVar("USER");
            string token = env.getVar("TOKEN");
            string password = env.getVar("PASSWORD");
            string hostaddress = env.getVar("HOSTADDRESS");

            // экземпляр для произведения запросов http
            var baseHttp = new BaseHttp(hostaddress, token, user, password);
            
            // экземпляр класса с методами для запросов к серверу Elma
            // для получение данных от сервера elma, в основном данные из
            // объектов справочников
            var reqElma = new Elma(baseHttp);

            // экземпляр предоставляющий допоплнительные 
            // возможности работы с elma
            var possiElma = new PossibilitiesElma(reqElma);

            // экземпляр класса для загрузки данных на сервер Elma
            // которые есть в БД но отсутвуют на сервере Elma
            var uploadData = new UploadData(baseHttp, reqElma);
            
            // выгрузка данных на сервер elma из excel файлов
            var uploadExcel = new UploadExcel(baseHttp, reqElma);

            // * раздел для выгрузки данных из БД деканата в Elma
            // спинер Информирующий о Стутесе Процесса Программы
            AnsiConsole.Status()
                .Spinner(Spinner.Known.BouncingBar)
                .SpinnerStyle(Style.Parse("green1"))
                .Start("[white]Process uploding from database[/]", ctx => 
            {
                // загрузка справочников "учебные планы" которые отсутсвуют на сервере ELMA
                ctx.Status("educational plans");
                uploadData.EducationalPlans(
                    eduPlansDB: RequestDatabase.getUchebnyePlany()); // "учебные планы" из базы данных деканат

                // загрузка справочников "факультеты" которые отсутствуют на сервере ELMA
                ctx.Status("faculties");
                uploadData.Faculties(
                    facultiesDB: RequestDatabase.getFakuljtety()); // факультеты из БД деканат

                // загрузка спраовочников "дисциплины" которые отсутствуют на сервере ELMA
                ctx.Status("disciplines");
                uploadData.Disciplines(
                    disciplinesDB: RequestDatabase.getDisciplines()); // дисциплины из БД деканат

                // загрузка спраовочников "направления подготовки" которые отсутствуют на сервере ELMA
                ctx.Status("direction preparations");
                uploadData.DirecsPre(
                    direcsPreDB: RequestDatabase.getDirectionPreparation()); // направеления подготовки из БД деканат

                // загрузка спраовочников "кафедры" которые отсутствуют на сервере ELMA
                ctx.Status("departments");
                uploadData.Departments(
                    departmentsDB: RequestDatabase.getDepartments()); // кафедры из БД деканат

                // загрузка спраовочников "профили подготовки" которые отсутствуют на сервере ELMA
                ctx.Status("profile preparations");
                uploadData.ProfilePrep(
                    profilesDB: RequestDatabase.getPrepProfiles()); // профили подготовки из БД деканат
            });

            // * раздел для выгрузки данных из Статичных файлов в директории STATIC в Elma
            // спинер Информирующий о Стутесе Процесса Программы
            AnsiConsole.Status()
                .Spinner(Spinner.Known.BouncingBar)
                .SpinnerStyle(Style.Parse("green1"))
                .Start("[white]Process uploding from static files[/]", ctx => 
            {
                // выгрузка пользователей которые ОТСУТСТВУЮТ на сервере elma, справочник User
                ctx.Status("add new users");
                uploadExcel.addUsers(Path.Combine(Environment.CurrentDirectory, "static", "ППС.xlsx"), baseHttp);

                // обновление учёных званий для системного справочника elma -> пользователь (user)
                ctx.Status("academic titles");
                uploadExcel.academicTitle(Path.Combine(Environment.CurrentDirectory, "static", "ППС.xlsx"));
            });

            // * раздел доп. возможностей elma
            // спинер Информирующий о Стутесе Процесса Программы
            AnsiConsole.Status()
                .Spinner(Spinner.Known.BouncingBar)
                .SpinnerStyle(Style.Parse("green1"))
                .Start("[white]additional operations in elma (possibilities)[/]", ctx => 
            {
                List<Root> gWithoutStudents;
                // подсчет кол-ва групп у которых нет студентов
                ctx.Status("searching groups without students");
                gWithoutStudents = possiElma.groupsWithoutStudents();
                Log.Info(InfoTitle.dataElma, $"groups without students : {gWithoutStudents.Count}");

                List<Root> studsWithoutGroups;
                // подсчет кол-ва студентов без группы
                ctx.Status("searching students without groups");
                studsWithoutGroups = possiElma.studentsWithoutGroup();
                Log.Info(InfoTitle.dataElma, $"students without groups : {studsWithoutGroups.Count}");
            });


            // foreach (var group in gWithoutStudents)
            // {
            //     var studs = Elma.getValueItem(group.Items, "HowManyStudents");
            //     var nameGroup = Elma.getValueItem(group.Items, "Naimenovanie");
            //     var idGroup = Elma.getValueItem(group.Items, "Id");

            //     Console.WriteLine(nameGroup + $", id: {idGroup} " + " -> " + studs);
            // }

            // reqElma.users().FindAll(user => {
            //     return reqElma.getValueItem(user.Items, "UchyonoeZvanie") != null;
            // }).ForEach(user => {
            //     Console.Write(reqElma.getValueItem(user.Items, "FullName"));
            //     Console.WriteLine(" | " + reqElma.getValueItem(user.Items, "UchyonoeZvanie"));
            // });

        } 
    }

    // * уникальные идентификаторы типов спрвочников сервера ELMA
    public class TypesUidElma 
    {
        /// <summary> учебные планы </summary>
        public static readonly string eduPlans = "4bbebaa3-4c81-4cdc-8115-23b9721726cb";
        /// <summary> факультеты </summary>
        public static readonly string faculties = "862af194-c1df-49c5-8692-21ffca0988c7";
        // дисциплины
        public static readonly string disciplines = "b1bce0ec-1cf6-44ea-9915-844969fd7823";
        // напраления подготовки
        public static readonly string direcPreparations = "c6f8bf73-f973-4f59-8fea-0084e3f95597";
        // кафедры
        public static readonly string departments = "d65309ba-779a-4074-82a0-55560d8e4674"; 
        // направления подготовки
        public static readonly string preparationProfile = "92392fcf-620d-4f0c-bede-af6dffbc41c4";
        // группы
        public static readonly string groups = "1b5dca14-da97-4a7e-816f-b3531276149c";
        // пользователя - Системый Справочник Elma
        public static readonly string users = "18faf3ae-03c9-4e64-b02a-95dd63e54c4d";
        // студенты
        public static readonly string students = " eb7e76a9-9bd1-410f-b8ff-877ce69ea850";
    }

}