using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using ELMA_API;
using HtmlAgilityPack;
using Newtonsoft.Json;

namespace Client;

public class ElmaClient 
{
    private string ElmaTokenApi;
    private readonly HttpClient _httpClient;
    private readonly string Username;
    private readonly string Password;
    private ResponseAuthorization AuthorizationData;
    private List<TypeObj> TypesUidEntities; // all available entities in elma
    private List<TypeObj> TypesUidProcesses; // all available processes in elma
    // url to get authorization token from elma server for requests
    private readonly string UrlAuthorization = "/API/REST/Authorization/LoginWith";
    // url to get entities from elma server
    private readonly string UrlEntityQueryTree = "/API/REST/Entity/QueryTree";
    // example insert full url /API/REST/Entity/Insert/<TYPEUID_ELMA_ENTITY>
    // after /Insert/ should be typeuid which need to insert to server elma
    private readonly string UrlEntityInsert = "/API/REST/Entity/Insert/";
    // url to count entities in elma server
    private readonly string UrlEntityCount = "/API/REST/Entity/Count";
    // url to update entity in serlve elma, where 0 - it's TypeUidElma entity, 1 - it's Id entity elma to update
    private readonly string UrlEntiityUpdate = "/API/REST/Entity/Update/{0}/{1}";
    // url to launch process by http
    private readonly string UrlStartProcess = "/API/REST/Workflow/StartProcess";
    // url to get all starable processes 
    private readonly string UrlStarableProcesses = "/API/REST/Workflow/StartableProcesses";
    // url to get all starable processes from external apps
    private readonly string UrlStarableProcessesExternalApps = "/API/REST/Workflow/StartableProcessesFromExternalApps";
    // url to html page with all elma acccessable elma entities
    private readonly string UrlPageTypes = "/API/Help/Types";

    public ElmaClient(string elmaTokenApi, string hostaddress, string username, string password)
    {
        this.ElmaTokenApi = elmaTokenApi;
        this.Username = username;
        this.Password = password;
        this._httpClient = new() { BaseAddress = new Uri($"http://{hostaddress}") };
    }

    /// <summary>
    /// assamble instance of ElmaClient, must be call before any operations with instance of clss ElmaClient
    /// </summary>
    public async Task<ElmaClient> Build()
    {
        await GetAuthorization();
        await GetTypesUid();
        return this;
    }

    /// <summary>
    /// получение authorization token и session token from elma server. These tokens 
    /// need for workflow with elma resp api
    /// </summary>
    private async Task GetAuthorization()
    {
        var request = new HttpRequestMessage(HttpMethod.Post, $"/API/REST/Authorization/LoginWith?username={Username}");
        request.Headers.Add("ApplicationToken", ElmaTokenApi);
        request.Content = new StringContent($"\"{Password}\"", Encoding.UTF8, "application/json");

        var response = await _httpClient.SendAsync(request);
        this.AuthorizationData = await response.Content.ReadFromJsonAsync<ResponseAuthorization>();

        // automatically add tokens to every request's headers form this client to server http
        _httpClient.DefaultRequestHeaders.Add("AuthToken", AuthorizationData.AuthToken);
        _httpClient.DefaultRequestHeaders.Add("SessionToken", AuthorizationData.SessionToken);
    }

    /// <summary>
    /// получение сущностей (объект-справочник) от сервера elma
    /// </summary>
    /// <param name="type">уникльный идентификтор типа сущности elma</param>
    /// <param name="queryParams">дополнительные паретры запроса. Например можно передать q т.е. EQL 
    /// (Elma Query Language) чтобы произвести выборка сущностей на стороне сервера</param>
    public async Task<List<Root>> QueryEntity(string type, QParams queryParams = null)
    {
        // получаем тип обьекта по его наименованию и его TypeUID для запросов
        var getTypeObj = this.GetTypeObj(type, TypesObj.Entity); 

        var query = HttpUtility.ParseQueryString(string.Empty);
        query["type"] = getTypeObj.Uid; // it's the most important parameter, without that won't work

        if (queryParams != null)
        {
            foreach (var record in queryParams.Params)
            {
                query[record.Key] = record.Value;
            }
        }
        
        var request = new HttpRequestMessage(HttpMethod.Get, UrlEntityQueryTree + $"?{query.ToString()}");
        var response = await _httpClient.SendAsync(request);
        
        return await response.Content.ReadFromJsonAsync<List<Root>>();
    }

    /// <summary> get number of entities via unique type identifier </summary>
    /// <param name="typeObj">unique object's type identifier</param>
    /// <returns>number of entities via unique type identifier</returns>
    public async Task<int> CountEntity(string type)
    {
        // получаем тип обьекта по его наименованию и его TypeUID для запросов
        var getTypeObj = this.GetTypeObj(type, TypesObj.Entity);

        var request = new HttpRequestMessage(HttpMethod.Get, UrlEntityCount + $"?type={getTypeObj.Uid}");
        var response = await _httpClient.SendAsync(request);
        return int.Parse(await response.Content.ReadAsStringAsync());
    }

    /// <summary> inserted new entity to server elma </summary>
    /// <param name="typeObj">unique object's type identifier</param>
    /// <param name="data">data which will be inserted to server elma</param>
    /// <returns>id the new entity which was inserted</returns>
    public async Task<int> InsertEntity(string type, Data data)
    {
        // получаем тип обьекта по его наименованию и его TypeUID для запросов
        var getTypeObj = this.GetTypeObj(type, TypesObj.Entity);

        var request = new HttpRequestMessage(HttpMethod.Post, UrlEntityInsert + getTypeObj.Uid);
        
        request.Content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");

        request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json")
        {
            CharSet = "utf-8"
        };
        
        var response = await _httpClient.SendAsync(request);
        var body = await response.Content.ReadAsStringAsync();

        return int.Parse(body.Replace("\"", String.Empty));
    }

    /// <summary> update entity via id with new data </summary>
    /// <param name="typeObj">unique object's type identifier</param>
    /// <param name="id">entity's id which will be updated</param>
    /// <param name="data">new data for uploading for entity via id</param>
    /// <returns></returns>
    public async Task<int> UpdateEntity(string type, int id, Data data)
    {
        // получаем тип обьекта по его наименованию и его TypeUID для запросов
        var getTypeObj = this.GetTypeObj(type, TypesObj.Entity);

        var request = new HttpRequestMessage(HttpMethod.Post, String.Format(UrlEntiityUpdate, getTypeObj.Uid, id));
        
        request.Content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");

        request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json")
        {
            CharSet = "utf-8"
        };
        
        var response = await _httpClient.SendAsync(request);
        var body = await response.Content.ReadAsStringAsync();

        return int.Parse(body.Replace("\"", string.Empty));
    }

    /// <summary>
    /// get all types uid for accessable entities and processes in elma server
    /// </summary>
    private async Task GetTypesUid()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, UrlPageTypes);

        var response = await _httpClient.SendAsync(request);
        var body = await response.Content.ReadAsStringAsync();

        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(body);

        var nodesHtml = htmlDoc.DocumentNode.SelectNodes("html/body/table/tr")
            .ToList()
            .Select(node => node.InnerHtml.Replace("\t", "").Replace("\r", "").Replace("\n", "")).ToList()
            .FindAll(node => new Regex("href=\"/API/Help/Type\\?uid=(.*)\">.*</a>", RegexOptions.IgnoreCase)
                    .Match(node).Success);

        foreach (var node in nodesHtml)
        {
            List<string> wrapNode = new() { 
                "<body>",
                node.Replace("\t", "").Replace("\r", "").Replace("\n", ""),
                "</body>" };

            string nodeConvertStr = String.Join("", wrapNode);

            htmlDoc.LoadHtml(nodeConvertStr);
            var nodeTypesInfo = htmlDoc.DocumentNode.SelectNodes("/body/td").ToList();

            // if didn't find two nodes structured together in one node
            if (nodeTypesInfo.Count != 2) continue;

            var nodeNameAndTypeUid = nodeTypesInfo.First().InnerHtml.Trim();
            var nodeNameDesc = nodeTypesInfo.Last().InnerHtml.Trim();

            var typeUid = nodeNameAndTypeUid.Substring(28, 36);
            var nameTypeUid = nodeNameAndTypeUid.Substring(66).Replace("</a>", "");

            // if name type uid starts with "P_" then it's a processe and then pass it 
            // to storage List TypesUidStorage
            if (nameTypeUid.StartsWith("P_"))
            {
                this.TypesUidProcesses ??= new List<TypeObj>(); // if isn't initialized (null)
                this.TypesUidProcesses.Add(
                    new TypeObj
                    {
                        Name = nameTypeUid,
                        Uid = typeUid,
                        NameDesc = nodeNameDesc
                    });
            }
            // if not then it's a elma entity
            else {
                this.TypesUidEntities ??= new List<TypeObj>(); // if isn't initialized (null)
                this.TypesUidEntities.Add(
                    new TypeObj
                    {
                        Name = nameTypeUid,
                        Uid = typeUid,
                        NameDesc = nodeNameDesc
                    });
            }

        }
    }


    /// <summary> Method for searching object's unique elma type </summary>
    /// <param name="name">Unique name of the object's type</param>
    /// <param name="type">enums can be only Entity or Process</param>
    /// <exception cref="Exception">If won't find the object's unique type then throw excepiton</exception>
    public TypeObj GetTypeObj(string name, TypesObj type)
    {
        var tryFind = TypesObj.Process.Equals(type)
            ? this.TypesUidProcesses.Find(typeUid => typeUid.Name == name)
            : this.TypesUidEntities.Find(typeUid => typeUid.Name == name);

        string entityOrProcess = TypesObj.Process.Equals(type) ? "Process" : "Entity";

        if (tryFind == null)
        {
            throw new Exception(
                $"{entityOrProcess} with name : \"{(String.IsNullOrEmpty(name) ? "null" : name)}\" "
                + $"isn't found. Check please: Letter Case, the {entityOrProcess} is published "
                + $"to the server, access to the {entityOrProcess}");
        }

        return tryFind;
    }

    public TypeObj GetTypeObj(string name)
    {
        var processesAndEntities = this.TypesUidProcesses.Concat(TypesUidEntities).ToList();

        var tryFind = processesAndEntities.Find(obj => obj.Name == name);

        if (tryFind == null)
        {
            throw new Exception(
                $"With name : \"{(String.IsNullOrEmpty(name) ? "null" : name)}\" "
                + $"isn't found any process and enitties. Check please: Letter Case, is the one published? "
                + $"to the server, access to the one");
        }
        return tryFind;
    }

}
/// <summary>
/// All name TypeUid is unique and can't be repeated (it's fair for Processes and Entities)
/// </summary>
public class TypeObj 
{
    public string Name { get; set; }
    public string Uid { get; set; }
    public string NameDesc { get; set; }
}

public enum TypesObj 
{
    Process, 
    Entity
}

public class QParams
{
    public Dictionary<string, string> Params = new Dictionary<string, string>();
    public QParams() { }
    /// <summary> add new url parameters in storage </summary>
    public QParams Add(string key, string value)
    {
        if (String.IsNullOrEmpty(key) || String.IsNullOrEmpty(value)) {
            throw new Exception($"Url parameters can't be null or empty string: key: \"{key}\", value: \"{value}\"");
        }
        Params.Add(key, value);
        return this;
    }
    /// <summary> create url parameter for Eql (elma query lanaguage) for difficult query to Elma</summary>
    public QParams Eql(string value)
    {
        this.Add("q", value);
        return this;
    }
    /// <summary> specify how many objects need to get </summary>
    public QParams Limit(int value)
    {
        this.Add("limit", value.ToString());
        return this;
    }
    /// <summary> specify the start element </summary>
    public QParams Offset(int value)
    {
        this.Add("offset", value.ToString());
        return this;
    }
    /// <summary> 
    /// необходимо передавать строку выборки свойств и вложенных объектов.
    /// * - универсальная подстановка для выбора всех свойств объекта на один уровень вложенности
    /// / - разделитель уровней вложенности свойств объекта
    /// , - объединяет результаты нескольких запросов
    /// Subject,Comments/* – для типа объекта Задача требуется выбрать свойство Тема и для всех объектов в свойстве Комментарии выбрать все их доступные свойства;
    /// Subject, Description, CreationAuthor/UserName, CreationAuthor/FullName - для типа объекта Задача
    /// требуется выбрать только свойства Тема, Описание и для свойства Автор (тип объекта Пользователь)
    /// выбрать свойства Логин и Полное имя;
    /// </summary>
    public QParams Select(string value)
    {
        this.Add("select", value);
        return this;
    }
}