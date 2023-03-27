using System.Web;
using System.Collections.Specialized;
using System.Net.Http.Json;

namespace TypesElma;


// * ////////////////////////////////////////////// Main Response with Entities
public class WebData
{
    public List<WebDataItem> Items { get; set; }
    public object Value { get; set; }
}
public class WebDataItem
{
    public WebData Data { get; set; }
    public List<WebData> DataArray { get; set; }
    public string Name { get; set; }
    public string Value { get; set; }

}
// * ///////////////////////////////////////////// Main Response with Entities

/// <summary>
/// All name TypeUid is unique and can't be repeated (it's fair for Processes and Entities)
/// </summary>
public class TypeObj 
{
    public string Name { get; set; }
    /// <summary> уникальный идентификатор типа </summary>
    public string Uid { get; set; } 
    public string NameDesc { get; set; }
}
public enum TypesObj 
{
    Process, 
    Entity
}

/// <summary>
/// CRTP (Curiously recurring template pattern) Применение паттерна CRTP в C#
/// </summary>
public class QParams<T> where T : QParams<T>
{
    public Dictionary<string, string> Params = new Dictionary<string, string>();
    public QParams() { }
    /// <summary> add new url parameters in storage </summary>
    public T Add(string key, string value)
    {
        if (String.IsNullOrEmpty(key) || String.IsNullOrEmpty(value)) {
            throw new Exception($"Url parameters can't be null or empty string: key: \"{key}\", value: \"{value}\"");
        }
        Params.Add(key, value);
        return (T)this;
    }
    /// <summary> уникльный идентификатор типа </summary>
    public T TypeUid(string value) 
    {
        Params.Add("type", value);
        return (T)this;
    }
    /// <summary> create url parameter for Eql (elma query lanaguage) for difficult query to Elma</summary>
    public T Eql(string value)
    {
        this.Add("q", value);
        return (T)this;
    }
    /// <summary> specify how many objects need to get </summary>
    public T Limit(int value)
    {
        this.Add("limit", value.ToString());
        return (T)this;
    }
    /// <summary> specify the start element </summary>
    public T Offset(int value)
    {
        this.Add("offset", value.ToString());
        return (T)this;
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
    public T Select(string value)
    {
        this.Add("select", value);
        return (T)this;
    }

    /// <summary>
    /// Значения полей для фильтра сущности в формате: Property1:Значение1,Property2:Значение2
    /// Наименование свойства возможно задавать с точкой (.) для получения доступа к подсвойству: Property1.Property2:Значение1
    /// Для указания в значении свойства символа : (двоеточие), \ (обратный слэш) или , (запятая), его нужно экранировать черз \ (обратный слэш)
    /// </summary>
    public T Filter(string value)
    {
        this.Add("filter", value);
        return (T)this;
    }
}

public class PrepareHttpRequestElma : QParams<PrepareHttpRequestElma>
{
    private HttpClient _httpClient;
    private NameValueCollection queryParamsUrl = HttpUtility.ParseQueryString(string.Empty);
    private string pathUrl;
    HttpMethod httpMethod;
    public PrepareHttpRequestElma(HttpClient httpClient, string type, string pathUrl, HttpMethod httpMethod) : base()
    {
        this._httpClient = httpClient;
        this.pathUrl = pathUrl;
        this.httpMethod = httpMethod;
        this.TypeUid(type); // it's the most important parameter, without that won't work
    }
    public async Task<List<WebData>> Execute()
    {
        if (this.Params.Count != 0)
        {
            foreach (var record in this.Params)
            {
                queryParamsUrl[record.Key] = record.Value;
            }
        }
        
        var request = new HttpRequestMessage(httpMethod, 
            pathUrl + (queryParamsUrl.Count != 0 ? $"?{queryParamsUrl.ToString()}" : ""));
        
        var response = await _httpClient.SendAsync(request);
        
        return await response.Content.ReadFromJsonAsync<List<WebData>>();
    }
}