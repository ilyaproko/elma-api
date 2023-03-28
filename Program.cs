using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Refit;
using Spectre.Console;
using Client;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using TypesElma;

namespace ELMA_API;
class Program
{
    static async Task Main(string[] args)
    {
        // this setting need for spinner progress status currently show up
        System.Console.OutputEncoding = Encoding.UTF8;
        System.Console.InputEncoding = Encoding.UTF8;
        // end setting block

        Log.Launch();
        Log.Success(SuccessTitle.launch, "start elma-api application is successful");

        // get environment variables
        var env = new EnvModule(".env");
        string username = env.getVar("USER");
        string token = env.getVar("TOKEN");
        string password = env.getVar("PASSWORD");
        string hostaddress = env.getVar("HOSTADDRESS");
        string dataSourceDb = env.getVar("DATASOURCE");
        string initialCatalogDb = env.getVar("INITIALCATALOG");

        // экземпляр для произведения запросов http
        // var baseHttp = new BaseHttp(hostaddress, token, username, password);

        // экземпляр класса с методами для запросов к серверу Elma
        // для получение данных от сервера elma, в основном данные из
        // объектов справочников
        // var elmaApi = new ElmaApi(baseHttp);

        // экземпляр базы данных MsSql Server
        var dbMsSql = new DbMsSql(dataSourceDb, initialCatalogDb);

        // экземпляр предоставляющий допоплнительные 
        // возможности работы с elma
        // var possiElma = new ExtendedUserElma(elmaApi, baseHttp);

        // экземпляр класса для загрузки данных на сервер Elma
        // которые есть в БД но отсутвуют на сервере Elma
        // var uploadData = new UploadData(baseHttp, elmaApi, dbMsSql);

        // выгрузка данных на сервер elma из excel файлов
        // var uploadExcel = new UploadExcel(baseHttp, elmaApi);

        // * раздел для выгрузки данных из БД деканата в Elma
        // спинер Информирующий о Стутесе Процесса Программы
        // AnsiConsole.Status()
        //     .Spinner(Spinner.Known.BouncingBar)
        //     .SpinnerStyle(Style.Parse("green1"))
        //     .Start("[white]Process uploding from database[/]", ctx =>
        // {
        //     // загрузка справочников "учебные планы" которые отсутсвуют на сервере ELMA
        //     ctx.Status("educational plans");
        //     var uploadEduPlans =  uploadData.Educational_plans(); // "учебные планы" из базы данных деканат
        //     Log.UploadDb(uploadEduPlans);

        //     // загрузка справочников "факультеты" которые отсутствуют на сервере ELMA
        //     ctx.Status("faculties");
        //     var uploadFaculties = uploadData.Faculties(); // факультеты из БД деканат
        //     Log.UploadDb(uploadFaculties);

        //     // загрузка спраовочников "дисциплины" которые отсутствуют на сервере ELMA
        //     ctx.Status("disciplines");
        //     var uploadDiscplines = uploadData.Disciplines(); // дисциплины из БД деканат
        //     Log.UploadDb(uploadDiscplines);

        //     // загрузка спраовочников "направления подготовки" которые отсутствуют на сервере ELMA
        //     ctx.Status("direction preparations");
        //     var uploadDirecPreps = uploadData.Direction_preparations(); // направеления подготовки из БД деканат
        //     Log.UploadDb(uploadDirecPreps);

        //     // загрузка спраовочников "кафедры" которые отсутствуют на сервере ELMA
        //     ctx.Status("departments");
        //     var uploadDepartments = uploadData.Departments(); // кафедры из БД деканат
        //     Log.UploadDb(uploadDepartments);

        //     // загрузка спраовочников "профили подготовки" которые отсутствуют на сервере ELMA
        //     // ctx.Status("profile preparations");
        //     // uploadData.ProfilePrep(); // профили подготовки из БД деканат
        // });

        // * раздел для выгрузки данных из Статичных файлов в директории STATIC в Elma
        // спинер Информирующий о Стутесе Процесса Программы
        // AnsiConsole.Status()
        //     .Spinner(Spinner.Known.BouncingBar)
        //     .SpinnerStyle(Style.Parse("green1"))
        //     .Start("[white]Process uploding from static files[/]", ctx => 
        // {
        //     // // выгрузка пользователей которые ОТСУТСТВУЮТ на сервере elma, справочник User
        //     // ctx.Status("add new users");
        //     // uploadExcel.addUsers(Path.Combine(Environment.CurrentDirectory, "static", "ППС.xlsx"), baseHttp);

        //     // // обновление учёных званий для системного справочника elma -> пользователь (user)
        //     // ctx.Status("academic titles");
        //     // uploadExcel.academicTitle(Path.Combine(Environment.CurrentDirectory, "static", "ППС.xlsx"));
        // });

        // * раздел доп. возможностей (операций) elma
        // спинер Информирующий о Стутесе Процесса Программы
        // AnsiConsole.Status()
        //     .Spinner(Spinner.Known.BouncingBar)
        //     .SpinnerStyle(Style.Parse("green1"))
        //     .Start("[white]additional operations in elma (possibilities)[/]", ctx =>
        // {
        //     // Удаление групп у которых нет студентов
        //     ctx.Status("deleting groups without students");
        //     possiElma.deleteGroupsWithoutStudents();

        //     // обновление объекта-справочника elma "Приложение 3 к договору на практику"
        //     // т.е. добавит Приложение 3 для студентов у которых их нет или которые отсутствуют
        //     // хотя для группы в которой находится студент есть Приктики
        //     ctx.Status("updating appendix Three for document of the practice"); 
        //     possiElma.Update_appendixes_three("", ctx);
        // });


        var elmaClient = await new ElmaClient(token, hostaddress, username, password).Build();

        // для выборки массива данных с возможностями фильтрации через методы Eql или Filter
        var result2 = await elmaClient.QueryEntity("Praktiki").Limit(10).Offset(50)
            .Select("DataS").Filter("Kurs:2").Eql("not DataS is null").Execute(); // или можно так Semestr = 2 AND Kurs = 2
        
        var result3 = await elmaClient.LoadEntity("Praktiki", id: 4508).Select("Kurs").Execute();

        var result4 = elmaClient.InsertEntity("Praktiki");
        result4.WebItem("Kurs", "1");
        result4.WebItem("Semestr", "2");
        result4.WebItem("Disciplina", "Id", "12");
        result4.WebItem("KodKafedry", "1234567890");
        result4.WebItem("Kurs", "10");

        var injectNewObj = await result4.Execute();

        // System.Console.WriteLine(JsonConvert.SerializeObject(result2));
        // System.Console.WriteLine(injectNewObj);
        // System.Console.WriteLine(JsonConvert.SerializeObject(result4.webData));

        


        // var data = new Data()
        // {
        //     Items = new List<Item>() {
        //         new Item() { Name = "Naimenovanie", Value = "updating name test"}
        //     }
        // };

        // var result4 = await elmaClient.UpdateEntity("UchebnyePlany",
        //     id: result3, 
        //     data);

        // System.Console.WriteLine("updated entity with id: " + result4);



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
    public static readonly string profilePreparation = "92392fcf-620d-4f0c-bede-af6dffbc41c4";
    // группы
    public static readonly string groups = "1b5dca14-da97-4a7e-816f-b3531276149c";
    // пользователя - Системый Справочник Elma
    public static readonly string users = "18faf3ae-03c9-4e64-b02a-95dd63e54c4d";
    // студенты
    public static readonly string students = "eb7e76a9-9bd1-410f-b8ff-877ce69ea850";
    // практики 
    public static readonly string practices = "23e32dbd-d6ca-4997-ab08-1fddfeac2f34";
    // приложение 3 к договору на практику
    public static readonly string appendixThreePracticeDoc = "34fecf2f-70e0-45c5-90b6-5ae43b508e21";
}

