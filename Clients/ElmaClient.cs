using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Web;
using ELMA_API;
using Newtonsoft.Json;

namespace Client;

public class ElmaClient 
{
    private string ElmaTokenApi;
    private readonly HttpClient _httpClient;
    private readonly string Username;
    private readonly string Password;
    private ResponseAuthorization AuthorizationData;
    // url to get authorization token from elma server for requests
    private readonly string UrlAuthorization = "/API/REST/Authorization/LoginWith";
    // url to get entities from elma server
    private readonly string UrlEntityQuery = "/API/REST/Entity/Query";
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
    /// получение authorization token и session token from elma server. These tokens 
    /// need for workflow with elma resp api
    /// </summary>
    public async Task<ElmaClient> GetAuthorization()
    {
        var request = new HttpRequestMessage(HttpMethod.Post, $"/API/REST/Authorization/LoginWith?username={Username}");
        request.Headers.Add("ApplicationToken", ElmaTokenApi);
        request.Content = new StringContent($"\"{Password}\"", Encoding.UTF8, "application/json");

        var response = await _httpClient.SendAsync(request);
        this.AuthorizationData = await response.Content.ReadFromJsonAsync<ResponseAuthorization>();

        // automatically add tokens to every request's headers form this client to server http
        _httpClient.DefaultRequestHeaders.Add("AuthToken", AuthorizationData.AuthToken);
        _httpClient.DefaultRequestHeaders.Add("SessionToken", AuthorizationData.SessionToken);

        return this;
    }

    /// <summary>
    /// получение сущностей (объект-справочник) от сервера elma
    /// </summary>
    /// <param name="type">уникльный идентификтор типа сущности справочника</param>
    /// <param name="queryParams">дополнительные паретры запроса. Например можно передать q т.е. EQL 
    /// (Elma Query Language) чтобы произвести выборка сущностей на стороне сервера</param>
    public async Task<List<Root>> QueryEntity(string type, Dictionary<string, string> queryParams = null)
    {
        var query = HttpUtility.ParseQueryString(string.Empty);
        query["type"] = type;

        if (queryParams != null)
        {
            foreach (var record in queryParams)
            {
                query[record.Key] = record.Value;
            }
        }
        
        var request = new HttpRequestMessage(HttpMethod.Get, UrlEntityQuery + $"?{query.ToString()}");
        var response = await _httpClient.SendAsync(request);
        
        return await response.Content.ReadFromJsonAsync<List<Root>>();
    }

    /// <summary> get number of entities via unique type identifier </summary>
    /// <param name="type">unique entity's type identifier</param>
    /// <returns>number of entities via unique type identifier</returns>
    public async Task<int> CountEntity(string type)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, UrlEntityCount + $"?type={type}");
        var response = await _httpClient.SendAsync(request);
        return int.Parse(await response.Content.ReadAsStringAsync());
    }

    /// <summary> inserted new entity to server elma </summary>
    /// <param name="type">unique entity's type identifier</param>
    /// <param name="data">data which will be inserted to server elma</param>
    /// <returns>id the new entity which was inserted</returns>
    public async Task<int> InsertEntity(string type, Data data)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, UrlEntityInsert + type);
        
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
    /// <param name="type">unique entity's type identifier</param>
    /// <param name="id">entity's id which will be updated</param>
    /// <param name="data">new data for uploading for entity via id</param>
    /// <returns></returns>
    public async Task<int> UpdateEntity(string type, int id, Data data)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, String.Format(UrlEntiityUpdate, type, id));
        
        request.Content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");

        request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json")
        {
            CharSet = "utf-8"
        };
        
        var response = await _httpClient.SendAsync(request);
        var body = await response.Content.ReadAsStringAsync();

        return int.Parse(body.Replace("\"", string.Empty));
    }

    public async Task<string> TypesEntity()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, UrlPageTypes);

        var response = await _httpClient.SendAsync(request);
        var body = await response.Content.ReadAsStringAsync();

        return body;
    }
}