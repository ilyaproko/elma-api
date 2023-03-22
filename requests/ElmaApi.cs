using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ELMA_API;

class ElmaApi
{
    public BaseHttp baseHttp;
    // url to get entities from elma server
    protected string elmaEntityQueryPath = "/API/REST/Entity/Query";
    // example insert full url /API/REST/Entity/Insert/<TYPEUID_ELMA_ENTITY>
    // after /Insert/ should be typeuid which need to insert to server elma
    protected string elmaEntityInsertPath = "/API/REST/Entity/Insert/";
    // url to count entities in elma server
    protected string elmaEntityCountPath = "/API/REST/Entity/Count";
    // url to update entity in serlve elma, where 0 - it's TypeUidElma entity, 1 - it's Id entity elma to update
    protected string elmaEntiityUpdatePath = "/API/REST/Entity/Update/{0}/{1}";
    // url to launch process by http
    protected string elmaStartProcessPath = "/API/REST/Workflow/StartProcess";
    // url to get all starable processes 
    protected string elmaStarableProcessesPath = "/API/REST/Workflow/StartableProcesses";
    // url to get all starable processes from external apps
    protected string elmaStarableProcessesExternalAppsPath = "/API/REST/Workflow/StartableProcessesFromExternalApps";

    public ElmaApi(BaseHttp baseHttp)
    {
        this.baseHttp = baseHttp;
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
        List<Item> items,
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

    /// <summary>
    /// Создание Item для тела http-запроса к серверу elma,
    /// Item представляет простой элемент внедрение/обновления данных на сервере elma,
    /// каждый Item вкладывается в список Items класса Root, класс Root представляет
    /// корень тело http-запроса. Если передается идентификатор сущности elma для которого
    /// будет создаваться данный Item тогда будет дополнительная проверка что ключ Item name
    /// существует в данной сущности Elma, если токого ключа нет выбросится exception
    /// </summary>
    /// <param name="name">Название Item</param>
    /// <param name="value">Значение Item</param>
    /// <param name="typUID">Уникльный идентификатор сущности Elma</param>
    /// <returns></returns>
    static public Item makeItem(
        string name,
        string value,
        Data data = null,
        string typeUID = "")
    {
        // если будет передан typeUID для объкта сущности на сервере elma для которой 
        // создается данный item тогда будет предварительная проверка на существование 
        // такой записи же "ключ/тип" сущности elma, если item создается с ключом которого нет
        // на у данной сущности тогда выбросится exception
        if (typeUID.Length != 0 && !ElmaGuard.existRecord(typeUID, name))
        {
            throw new Exception($"key {name} doesn't exists in object elma : -> {typeUID}");
        }

        return new Item()
        {
            Data = data,
            DataArray = new List<Data>(),
            Name = name,
            Value = value
        };
    }

    static public Item makeItem(
        string name,
        string nestedName,
        string nestedValue,
        Data data = null,
        string typeUID = "")
    {
        data = new Data()
        {
            Items = new List<Item>() { ElmaApi.makeItem(nestedName, nestedValue, typeUID: typeUID) },
            Value = null
        };

        // if the method get null for paramter nestedItemValue then
        // just don't create any item for field Data cause it's no sense 
        // to create empty dependency without information about it
        if (nestedValue == null) data = null;

        return new Item()
        {
            Data = data,
            DataArray = new List<Data>(),
            Name = name,
            Value = null
        };
    }

    /// <summary>
    /// Основной корень тела http-запроса к серверу elma
    /// </summary>
    /// <param name="items">принимает Item для тело запроса к серверу elma</param>
    /// <returns></returns>
    static public Root makeBodyJson(params Item[] items)
    {
        return new Root()
        {
            Items = items.ToList(),
            Value = null
        };
    }

    /// <summary>
    /// если generic type T будет передан как string
    /// тогда метод сериализует объект С# в string формата json
    /// и вернет его, если будет передан generic type T отличный от
    /// string тогда метод вернут null
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="items"></param>
    /// <returns></returns>
    static public string makeBodyJson<T>(params Item[] items) where T : class
    {
        var makeBody = ElmaApi.makeBodyJson(items);

        if (typeof(T) == typeof(string))
        {
            return JsonConvert.SerializeObject(makeBody);
        }
        return null;
    }

    /// <summary>
    /// поиск Справочника Направления Подготовки в ELMA по Id
    /// </summary>
    /// <param name="id">Id объекта-справочника elma "Напрпвление Подготовки"</param>
    /// <returns></returns>
    public Data findDirectPrepById(string id)
    {
        // пробуем найти Направление Подготовки ПО ID 
        // если не найдет тогда функция вернет null
        // и будет логгирования в консоле
        try
        {
            var findDirectPrep = this.baseHttp.requestElma<Data>(
                path: "/API/REST/Entity/Load",
                method: "GET",
                queryParams: new Dictionary<string, string>()
                {
                    ["type"] = TypesUidElma.direcPreparations,
                    ["id"] = id
                }
            );

            return findDirectPrep.bodyDeserializd;

        }
        catch (System.Exception)
        {

            Log.Warn(WarnTitle.notFoundDirectPrep, $"Elma hasn't direction preparation with ID = {id ?? "null"}");
            return null;
        }
    }

    public List<Item> findFaculties(string nameShort, string nameLong)
    {
        var eql = "";

        if (String.IsNullOrEmpty(nameShort) && String.IsNullOrEmpty(nameLong))
            return null;
        // Elma Query Language - найти все ФАКУЛЬТЕТЫ 
        // где ПолноеНаименование и Сокращенное Наименование соответсвуют ЗНАЧЕНИЮ ПОИСКА
        // или где ПолноеНаименование НЕ ПУСТОЕ и СокращенноеНаименование соответсвует ЗНАЧЕНИЮ ПОИСКА
        // или где ПолноеНаименование соответсвует ЗНАЧЕНИЮ ПОИСКА а СокращенноеНаименование НЕ ПУСТОЕ
        else if (!String.IsNullOrEmpty(nameLong) && !String.IsNullOrEmpty(nameShort))
            eql = @$"(NaimenovaniePolnoe LIKE `{nameLong}` AND NaimenovanieSokraschennoe LIKE `{nameShort}`) 
                OR (NOT NaimenovaniePolnoe LIKE `` AND NaimenovanieSokraschennoe LIKE `{nameShort}`)
                OR (NaimenovaniePolnoe LIKE `{nameLong}` AND NOT NaimenovanieSokraschennoe LIKE ``)";
        else if (!String.IsNullOrEmpty(nameShort))
            eql = $"NaimenovanieSokraschennoe LIKE `{nameShort}`";
        else if (!String.IsNullOrEmpty(nameLong))
            eql = $"NaimenovaniePolnoe LIKE `{nameLong}`";

        var findFaculties = this.queryEntityList(
            type: TypesUidElma.faculties,
            new QueryParameter("q", eql),
            new QueryParameter("limit", "1")
        );

        return findFaculties.Count != 0 ? findFaculties[0].Items : null;
    }

    public List<PrepProfileElma> preparationProfile()
    {
        // TypesUidElma.preparationProfile уникальный индентификатор для справочников "профили подготовки" из базы данных Elma
        var getPreProfiles = this.baseHttp.requestElma<List<Root>>(
            path: "/API/REST/Entity/Query",
            method: "GET",
            queryParams: new Dictionary<string, string>()
            {
                ["type"] = TypesUidElma.profilePreparation
            }
        );

        // storage for Name Preparation Profile and Code's Direction Preparation 
        // -> Наименование Профеля Подготовки, Шифр и Id Направления подготовки
        List<PrepProfileElma> storageProfiles = new List<PrepProfileElma>();

        foreach (var profile in getPreProfiles.bodyDeserializd)
        {
            string nameProfile = ElmaApi.getValueItem(profile.Items, "Naimenovanie"); // наименование подготовки
            string idDirectPrep = ElmaApi.getValueItem(profile.Items, "NapravleniePodgotovki", "Id"); // ID направления подготовки
            string codeDirectPrep = null; // Шифр направления подготовки

            // если в профиле есть вложеннная зависимость -> Направление Подготовки
            // т.е. для данного профеля указано направление подготовки
            if (idDirectPrep != null)
            {
                codeDirectPrep = ElmaApi.getValueItem(this.findDirectPrepById(idDirectPrep).Items, "Kod");
            }

            // добавление в хранилище объекта
            storageProfiles.Add(
                new PrepProfileElma(nameProfile, idDirectPrep, codeDirectPrep)
            );
        }

        return storageProfiles;
    }

    public ResponseOnRequest<List<Root>> queryEntity(string type, params QueryParameter[] queryParameters)
    {
        var queryParams = new Dictionary<string, string>()
        {
            ["type"] = type
        };

        // добавление дополнительных парамтров если будут переданы для метода
        if (queryParameters.Length != 0)
            queryParameters.ToList().ForEach(queryParam =>
            {
                queryParams.Add(queryParam.Key, queryParam.Value);
            });

        var getEntities = this.baseHttp.requestElma<List<Root>>(
            path: elmaEntityQueryPath,
            method: HttpMethods.Get,
            queryParams: queryParams
        );

        return getEntities;
    }

    public List<Root> queryEntityList(string type, params QueryParameter[] queryParameters)
    {
        var getEntities = queryEntity(type, queryParameters);
        return getEntities.bodyDeserializd;
    }

    public ResponseOnRequest insertEntity(string type, string body, string contentType = "application/json; charset=UTF-8")
    {
        var insertEntity = this.baseHttp.requestElma(
            path: elmaEntityInsertPath + type,
            method: HttpMethods.Post,
            contentType: contentType,
            body: body
        );

        return insertEntity;
    }

    public ResponseOnRequest updateEntity(string type, string idEntity, string body, string contentType = "application/json; charset=UTF-8")
    {
        var updateEntity = this.baseHttp.requestElma(
            path: String.Format(elmaEntiityUpdatePath, type, idEntity),
            method: HttpMethods.Post,
            contentType: contentType,
            body: body
        );

        return updateEntity;
    }

    public ResponseOnRequest countEntity(string type, string contentType = "application/json; charset=UTF-8")
    {
        var countEntities = this.baseHttp.requestElma(
            path: elmaEntityCountPath,
            method: HttpMethods.Get,
            contentType: contentType,
            queryParams: new Dictionary<string, string>()
            {
                ["type"] = type
            }
        );

        return countEntities;
    }

    public ResponseOnRequest launchProcess(string body, string contentType = "application/json; charset=UTF-8")
    {
        var launchRequest = this.baseHttp.requestElma(
            path: elmaStartProcessPath,
            method: HttpMethods.Post,
            contentType: contentType,
            body: body
        );

        return launchRequest;
    }

    /// <summary>
    /// получение списка всех доступных процессов для запуска на сервер Elma
    /// </summary>
    public List<List<Item>> StartableProcesses()
    {
        var startableProcess = baseHttp.requestElma<Root>(
                path: elmaStarableProcessesPath,
                method: HttpMethods.Post,
                contentType: "application/json; charset=UTF-8"
        ).bodyDeserializd.Items
            .FindAll(rootItem => rootItem.Name == "Processes")
            .Select(itemProcess => itemProcess.DataArray.Select(item => item.Items).ToList())
            .ToList()[0];

        return startableProcess;
    }

    /// <summary>
    /// получение списка всех доступных процессов для запуска из внешних систем
    /// </summary>
    public List<List<Item>> StartableProcessesFromExternalApps()
    {
        var startableProcessExternal = baseHttp.requestElma<Root>(
            path: elmaStarableProcessesExternalAppsPath,
            method: HttpMethods.Post,
            contentType: "application/json; charset=UTF-8"
        ).bodyDeserializd.Items
            .FindAll(rootItem => rootItem.Name == "Processes")
            .Select(itemProcess => itemProcess.DataArray.Select(item => item.Items).ToList())
            .ToList()[0];

        return startableProcessExternal;
    }

}