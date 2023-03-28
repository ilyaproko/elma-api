using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using ELMA_API;
using HtmlAgilityPack;
using Newtonsoft.Json;
using TypesElma;

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
    // url to get a certain one entity using TypeUID and its id
    private readonly string UrlEntityLoadTree = "/API/REST/Entity/LoadTree";
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
    // url to html page with specific Object's information (also will need UrlParameter 'uid')
    private readonly string UrlPageType = "/API/Help/Type";

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
        await GetNamesItemsForObjects();
        return this;
    }

    public async Task GetNamesItemsForObjects()
    {
        // for all elma objects add for them fields' name for every one
        var allObject = this.TypesUidProcesses.Concat(TypesUidEntities);

        foreach (var obj in allObject)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"{UrlPageType}?uid={obj.Uid}");

            var response = await _httpClient.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(body);
            
            var nodesHtml = htmlDoc.DocumentNode.SelectNodes("//body/table/tr/td[1]")
                .Select(node => node.InnerText.Trim()).ToList();

            obj.NamesFields ??= nodesHtml;
        }
    }

    /// <summary>
    /// получение authorization token и session token from elma server. These tokens 
    /// need for workflow with elma resp api
    /// </summary>
    private async Task GetAuthorization()
    {
        var request = new HttpRequestMessage(HttpMethod.Post, $"{UrlAuthorization}?username={Username}");
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
    /// <param name="type">имя униклього идентификтора типа сущности elma</param>
    public PrepareHttpQuery<List<WebData>> QueryEntity(string type) // QParams queryParams = null
    {
        // получаем тип обьекта по его наименованию и его TypeUID для запросов
        var getTypeObj = this.GetTypeObj(type, TypesObj.Entity);

        return new PrepareHttpQuery<List<WebData>>(_httpClient, getTypeObj.Uid, UrlEntityQueryTree, HttpMethod.Get);
    }

    /// <summary>
    /// get a certain entity by Its id
    /// </summary>
    public PrepareHttpLoad<WebData> LoadEntity(string type, int id)
    {
        // получаем тип обьекта по его наименованию и его TypeUID для запросов
        var getTypeObj = this.GetTypeObj(type, TypesObj.Entity);

        return new PrepareHttpLoad<WebData>(_httpClient, getTypeObj.Uid, UrlEntityLoadTree, HttpMethod.Get, id);
    }

    /// <summary>
    /// Count all entities elma by name of type
    /// </summary>
    /// <param name="type">имя униклього идентификтора типа сущности elma</param>
    public async Task<int> CountEntity(string type)
    {
        // получаем тип обьекта по его наименованию и его TypeUID для запросов
        var getTypeObj = this.GetTypeObj(type, TypesObj.Entity);

        var request = new HttpRequestMessage(HttpMethod.Get, UrlEntityCount + $"?type={getTypeObj.Uid}");
        var response = await _httpClient.SendAsync(request);
        return int.Parse(await response.Content.ReadAsStringAsync());
    }

    /// <summary> inserted new entity to server elma </summary>
    /// <param name="type">имя униклього идентификтора типа сущности elma</param>
    public PrepareHttpInsert InsertEntity(string type)
    {
        // получаем тип обьекта по его наименованию и его TypeUID для запросов
        var getTypeObj = this.GetTypeObj(type, TypesObj.Entity);

        return new PrepareHttpInsert(_httpClient, getTypeObj, UrlEntityInsert, HttpMethod.Post);
    }

    /// <summary> update entity via id with new data </summary>
    /// <param name="type">имя униклього идентификтора типа сущности elma</param>
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
        List<WebDataItem> items,
        string nameItem,
        string nestedItemName = null)
    {
        foreach (var item in items)
        {
            // if nestedNameItem wasn't passed in 
            if (nestedItemName == null)
            { // GET OBJECT DATA WITH ITEMS WHICH IS QUALS nameItem
                if (item.Name == nameItem) return item.Value; // RETURN !!!
            }
            else
            {
                if (item.Name == nameItem && item.Data != null)
                { // GET OBJECT DATA WITH ITEMS WHICH IS QUALS nameItem
                    foreach (var itemNested in item.Data.Items)
                    { // HAS NESTED DEPENDENCY
                        if (itemNested.Name == nestedItemName) return itemNested.Value; // RETURN !!!
                    }
                }
            }
        }
        return null;
    }

}