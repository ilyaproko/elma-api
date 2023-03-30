using System.Web;
using System.Collections.Specialized;
using System.Net.Http.Json;
using System.Text;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using Client;

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
/// every Uid is unique and can't be repeated (it's fair for Processes and Entities)
/// </summary>
public class TypeObj 
{
    public string Name { get; set; }
    /// <summary> уникальный идентификатор типа </summary>
    public string Uid { get; set; } 
    public string NameDesc { get; set; }
    public List<string> NamesFields { get; set; }
}
/// <summary>
/// Enum from Server Elma, every Uid is unique and can't be repeated
/// </summary>
public class TypeEnum
{
    public string Name { get; set; }
    /// <summary> уникальный идентификатор типа </summary>
    public string Uid { get; set; } 
    public string NameDesc { get; set; }
    /// <summary> Can be NULL !!! </summary>
    public string[] Values { get; set; }
}
public enum TypesObj 
{
    Process, 
    Entity
}

public class QParamsBase
{
    public readonly Dictionary<string, string> Params = new Dictionary<string, string>();
    public QParamsBase() { }
    /// <summary> add new url parameters in storage </summary>
    public QParamsBase Add(string key, string value)
    {
        if (String.IsNullOrEmpty(key) || String.IsNullOrEmpty(value)) {
            throw new Exception($"Url parameters can't be null or empty string: key: \"{key}\", value: \"{value}\"");
        }
        Params.Add(key, value);
        return  this;
    }
    /// <summary> уникльный идентификатор типа </summary>
    protected QParamsBase TypeUid(string value) 
    {
        Params.Add("type", value);
        return  this;
    }
    /// <summary> create url parameter for Eql (elma query lanaguage) for difficult query to Elma</summary>
    protected QParamsBase Eql(string value)
    {
        this.Add("q", value);
        return  this;
    }
    /// <summary> specify how many objects need to get </summary>
    protected QParamsBase Limit(int value)
    {
        this.Add("limit", value.ToString());
        return  this;
    }
    /// <summary> specify the start (сдвиг) element </summary>
    protected QParamsBase Offset(int value)
    {
        this.Add("offset", value.ToString());
        return  this;
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
    protected QParamsBase Select(string value)
    {
        this.Add("select", value);
        return  this;
    }

    /// <summary>
    /// Значения полей для фильтра сущности в формате: Property1:Значение1,Property2:Значение2
    /// Наименование свойства возможно задавать с точкой (.) для получения доступа к подсвойству: Property1.Property2:Значение1
    /// Для указания в значении свойства символа : (двоеточие), \ (обратный слэш) или , (запятая), его нужно экранировать черз \ (обратный слэш)
    /// </summary>
    protected QParamsBase Filter(string value)
    {
        this.Add("filter", value);
        return  this;
    }

    /// <summary> search by a certain id </summary>
    protected QParamsBase Id(int id) 
    {
        this.Add("id", id.ToString());
        return  this;
    }

    /// <summary> сортировка по указанному свойству объекта </summary>
    protected QParamsBase Sort(string value) 
    {
        this.Add("sort", value);
        return  this;
    }
}

interface IPrepareHttpBase<T>
{
    public Task<T> Execute();
}

public class PrepareHttpBase<T> : QParamsBase, IPrepareHttpBase<T> 
{
    protected HttpClient _httpClient;
    protected NameValueCollection queryParamsUrl = HttpUtility.ParseQueryString(string.Empty);
    protected string pathUrl;
    protected HttpMethod httpMethod;
    public PrepareHttpBase(HttpClient httpClient, string typeUid, string pathUrl, HttpMethod httpMethod) : base()
    {
        this._httpClient = httpClient;
        this.pathUrl = pathUrl;
        this.httpMethod = httpMethod;
        this.TypeUid(typeUid); // it's the most important parameter, without that won't work
    }
    
    /// <summary>
    /// make http-request to server elma
    /// </summary>
    public async Task<T> Execute()
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

        // if response from server wan't equels 200 (successful result), then throw exception
        if ((int)response.StatusCode != 200)
            throw new Exception("Bad request, server's body response:> " 
                + await response.Content.ReadAsStringAsync());

        return await response.Content.ReadFromJsonAsync<T>();
    }
}

public class PrepareHttpQuery<T> : PrepareHttpBase<T>
{
    public PrepareHttpQuery(HttpClient httpClient, string typeUid, string pathUrl, HttpMethod httpMethod)
         : base(httpClient, typeUid, pathUrl, httpMethod) { }

    /// <summary> create url parameter for Eql (elma query lanaguage) for difficult query to Elma</summary>
    public new PrepareHttpQuery<T> Eql(string value) 
    {
        base.Eql(value);
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
    public new PrepareHttpQuery<T> Select(string value) 
    {
        base.Select(value);
        return this;
    }
    /// <summary> specify how many objects need to get </summary>
    public new PrepareHttpQuery<T> Limit(int value) 
    {
        base.Limit(value);
        return this;
    }
    /// <summary> specify the start (сдвиг) element </summary>
    public new PrepareHttpQuery<T> Offset(int value) 
    {
        base.Offset(value);
        return this;
    }
    public new PrepareHttpQuery<T> Sort(string value) 
    {
        base.Sort(value);
        return this;
    }
    /// <summary>
    /// Значения полей для фильтра сущности в формате: Property1:Значение1,Property2:Значение2
    /// Наименование свойства возможно задавать с точкой (.) для получения доступа к подсвойству: Property1.Property2:Значение1
    /// Для указания в значении свойства символа : (двоеточие), \ (обратный слэш) или , (запятая), его нужно экранировать черз \ (обратный слэш)
    /// </summary>
    public new PrepareHttpQuery<T> Filter(string value) 
    {
        base.Filter(value);
        return this;
    }
}
public class PrepareHttpLoad<T> : PrepareHttpBase<T>
{
    public PrepareHttpLoad(HttpClient httpClient, string typeUid, string pathUrl, HttpMethod httpMethod, int id)
        : base(httpClient, typeUid, pathUrl, httpMethod) 
    {
        this.Id(id);
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
    public new PrepareHttpLoad<T> Select(string value) 
    {
        base.Select(value);
        return this;
    }
}

public class PrepareHttpInsertOrUpdate : PrepareHttpBase<int>
{
    public WebData webData = null;
    public TypeObj typeObj;
    public PrepareHttpInsertOrUpdate(HttpClient httpClient, TypeObj typeObj, string pathUrl, HttpMethod httpMethod)
        : base(httpClient, typeObj.Uid, pathUrl, httpMethod) 
    {
        this.typeObj = typeObj;
    }

    public async new Task<int> Execute()
    {
        // if data wasn't provided then throw an exception
        if (webData == null) throw new Exception("Field webData is null. Need data to upload to server");

        var request = new HttpRequestMessage(httpMethod, pathUrl);

        request.Content = new StringContent(JsonConvert.SerializeObject(webData), Encoding.UTF8, "application/json");

        request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json")
        {
            CharSet = "utf-8"
        };
        
        var response = await _httpClient.SendAsync(request);

        // if response from server wan't equels 200 (successful result), then throw exception
        if ((int)response.StatusCode != 200)
            throw new Exception("Bad request, server's body response:> " 
                + await response.Content.ReadAsStringAsync());

        var body = await response.Content.ReadAsStringAsync();

        return int.Parse(body.Replace("\"", String.Empty));
    }

    /// <summary>
    /// Создать новый WebItem с названием name и значением value, Если такой WebItem уже есть 
    /// тогда заменит значение в данном WebItem с названием name. Перед созданием происходит проверка
    /// названия WebItem name, есть ли похожее поле в Объекте Elma, если нет тогда выбросит ошибку
    /// </summary>
    public PrepareHttpInsertOrUpdate WebItem(string name, string value)
    {
        // check if the name exists for certain object elma which the WebItem creating
        // if the Name Of creating Item don't specified then throw Exception
        if (!this.typeObj.NamesFields.Contains(name))
        {
            throw new Exception(
                $"Elma Object \"{typeObj.Name}\", don't have field \"{name}\". "
                + $"Available fileds: {String.Join(", ", typeObj.NamesFields)}"
            );
        }

        this.webData ??= new WebData();
        this.webData.Items ??= new List<WebDataItem>();

        var tryFindItemByName = this.webData.Items.FirstOrDefault(item => item.Name == name);

        if (tryFindItemByName == null)
            this.webData.Items.Add(new WebDataItem { Name = name, Value = value });
        else
            tryFindItemByName.Value = value;

        return this;
    }
    
    /// <summary>
    /// Создать новый WebItem с названием nameObject и значением вложенного WebItem c названием nameItem и значением value, 
    /// Если такой WebItem c nameObject уже есть тогда уже нет потребшности заменять его, будет происходить замена
    /// уже вложенных WebItem если будет совпадение по названию nameItem. Перед созданием происходит проверка
    /// названия WebItem name, есть ли похожее поле в Объекте Elma, если нет тогда выбросит ошибку
    /// </summary>
    public PrepareHttpInsertOrUpdate WebItem(string nameObject, string nameItem, string value) 
    {
        // check if the name exists for certain object elma which the WebItem creating
        // if the Name Of creating Item don't specified then throw Exception
        if (!this.typeObj.NamesFields.Contains(nameObject))
        {
            throw new Exception(
                $"Elma Object \"{typeObj.Name}\", don't have field \"{nameObject}\". "
                + $"Available fileds: {String.Join(", ", typeObj.NamesFields)}"
            );
        }

        this.webData ??= new WebData();
        this.webData.Items ??= new List<WebDataItem>();

        var tryFindDependency = this.webData.Items.FirstOrDefault(item => 
            item.Name == nameObject);

        // if didn't create before, then create new
        if (tryFindDependency == null)
        {
            // create new item
            this.webData.Items.Add(
                new WebDataItem { Name = nameObject, Data = new WebData { Items = new List<WebDataItem>() } }
            );
            
            var findNewItem = this.webData.Items.First(item => item.Name == nameObject);

            // add new item for referenced object
            findNewItem.Data.Items.Add(new WebDataItem { Name = nameItem, Value = value });
        }
        else 
        {
            // check if item with 'nameItem' for referenced object has already created before 
            var tryFindItemRefObj = tryFindDependency.Data.Items.FirstOrDefault(item => item.Name == nameItem);

            if (tryFindItemRefObj == null)
                tryFindDependency.Data.Items.Add(new WebDataItem { Name = nameItem, Value = value });
            else
                tryFindItemRefObj.Value = value;
        }

        return this;
    }
}