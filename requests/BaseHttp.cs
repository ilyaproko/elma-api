using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace ELMA_API;

class BaseHttp
{
    protected string hostaddress;
    protected ResponseAuthorization auth;

    public BaseHttp(string hostaddress, string token, string user, string password)
    {
        // get environment variable localhost address
        this.hostaddress = hostaddress;
        this.auth = this.getAuth(token, user, password);
    }

    /// <summary>
    /// preparation (configurable) request-http
    /// </summary>
    public static HttpWebRequest reqConfigure(
        string hostaddress,
        string path,
        string method,
        string contentType = null,
        string body = null,
        Dictionary<string, string> queryParams = null,
        Dictionary<string, string> headers = null,
        int timeOut = 100000)
    {
        string fullUrl = "http://" + hostaddress + path;

        // * queryParameters if passed to the method
        if (queryParams != null)
        {
            List<string> qParams = new List<string>();

            foreach (var record in queryParams)
                qParams.Add($"{record.Key}={record.Value}");

            fullUrl += "?" + String.Join('&', qParams.ToArray());
        }

        HttpWebRequest request = WebRequest.Create(String.Format(fullUrl)) as HttpWebRequest;
        request.Method = method;

        request.Timeout = timeOut;
        request.ContentType = contentType;

        // * headers if passed to the method
        if (headers != null)
            foreach (var record in headers)
                request.Headers.Add(record.Key, record.Value);

        // * body request if passed to the method
        if (body != null)
        {
            var sendBody = Encoding.UTF8.GetBytes(body ?? "");
            request.ContentLength = sendBody.Length;
            Stream sendStream = request.GetRequestStream();
            sendStream.Write(sendBody, 0, sendBody.Length);
        }

        return request;
    }

    public static ResponseOnRequest request(
        string hostaddress,
        string path,
        string method,
        string contentType = null,
        string body = null,
        Dictionary<string, string> queryParams = null,
        Dictionary<string, string> headers = null,
        List<Cookie> cookies = null,
        int timeOut = 100000)
    {
        var reqConfig = BaseHttp.reqConfigure(
            hostaddress,
            path,
            method,
            contentType,
            body,
            queryParams,
            headers,
            timeOut: timeOut
        );

        // добавление кук в запрос
        if (cookies != null)
        {
            // if CookieContainer isn't initilized in request
            reqConfig.CookieContainer ??= new CookieContainer();
            cookies.ForEach(cookie =>
            {
                reqConfig.CookieContainer.Add(cookie);
            });
        }

        // read response on request
        var response = reqConfig.GetResponse() as HttpWebResponse;
        var resStream = response.GetResponseStream();
        var sr = new StreamReader(resStream, Encoding.UTF8);

        return new ResponseOnRequest
        {
            bodyResponse = sr.ReadToEnd(),
            response = response,
            request = reqConfig
        };
    }

    public static ResponseOnRequest<T> request<T>(
        string hostaddress,
        string path,
        string method,
        string contentType = "application/json; charset=UTF-8",
        string body = null,
        Dictionary<string, string> queryParams = null,
        Dictionary<string, string> headers = null,
        int timeOut = 100000
    )
    {
        // execute request
        var execRequest = BaseHttp.request(hostaddress, path, method, contentType, body, queryParams, headers, timeOut: timeOut);

        // преобразование ответа строки http в объект C# с типом данных generic T
        var jsonBody = JsonConvert.DeserializeObject<T>(execRequest.bodyResponse);

        return new ResponseOnRequest<T>
        {
            bodyResponse = execRequest.bodyResponse,
            response = execRequest.response,
            bodyDeserializd = jsonBody,
            request = execRequest.request
        };
    }

    public ResponseOnRequest requestElma(
        string path,
        string method,
        string contentType = null,
        string body = null,
        Dictionary<string, string> queryParams = null,
        Dictionary<string, string> headers = null,
        int timeOut = 100000
    )
    {
        // добавление в заголовки токенов elma
        headers ??= new Dictionary<string, string>(); // if isn't initialized
        headers.Add("AuthToken", this.auth.AuthToken);
        headers.Add("SessionToken", this.auth.SessionToken);

        var reqConfig = BaseHttp.reqConfigure(
            this.hostaddress,
            path,
            method,
            contentType,
            body,
            queryParams,
            headers,
            timeOut: timeOut
        );

        // automatically will add header Cookie to response, it's important
        reqConfig.CookieContainer ??= new CookieContainer();

        // read response on request > make request
        var response = reqConfig.GetResponse() as HttpWebResponse;
        var resStream = response.GetResponseStream();
        var sr = new StreamReader(resStream, Encoding.UTF8);

        return new ResponseOnRequest
        {
            bodyResponse = sr.ReadToEnd(),
            response = response,
            request = reqConfig
        };
    }

    public ResponseOnRequest<T> requestElma<T>(
        string path,
        string method,
        string contentType = "application/json; charset=UTF-8",
        string body = null,
        Dictionary<string, string> queryParams = null,
        Dictionary<string, string> headers = null,
        int timeOut = 100000
    )
    {
        // execute request
        var requestElma = this.requestElma(
            path, method,
            contentType: contentType,
            body, queryParams, headers, timeOut: timeOut);

        // преобразование ответа строки http в объект C# с типом данных generic T
        var bodyDeserialized = JsonConvert.DeserializeObject<T>(requestElma.bodyResponse);

        return new ResponseOnRequest<T>
        {
            bodyResponse = requestElma.bodyResponse,
            response = requestElma.response,
            bodyDeserializd = bodyDeserialized,
            request = requestElma.request
        };
    }

    private ResponseAuthorization getAuth(string applicationToken, string user, string password)
    {
        // create http request
        var request = BaseHttp.request<ResponseAuthorization>(
            hostaddress: hostaddress,
            path: "/API/REST/Authorization/LoginWith",
            method: HttpMethods.Post,
            contentType: "application/json; charset=utf-8",
            body: $"\"{password}\"",  // кавычики \" в начало и конец пароля !
            queryParams: new Dictionary<string, string>
            {
                ["username"] = user
            },
            headers: new Dictionary<string, string>()
            {
                ["ApplicationToken"] = applicationToken
            }
        );

        // Logging 
        Log.Success(SuccessTitle.loginElma, "connection is successful");

        return request.bodyDeserializd;
    }
}

class QueryParameter
{
    public string Key;
    public string Value;
    public QueryParameter(string key, string value)
    {
        this.Key = key;
        this.Value = value;
    }
}

class ParameterEql : QueryParameter
{
    public ParameterEql(params string[] value) : base("q", String.Join(' ', value)) { }
}

// для обпределение модели ответа в формате json, нужен для Newtonsoft.Json.JsonConvert.DeserailizeObject()
class ResponseAuthorization
{
    public string AuthToken { get; set; }
    public string CurrentUserId { get; set; }
    public string Lang { get; set; }
    public string SessionToken { get; set; }
}

/// <summary>
/// Ответ на запрос по http
/// </summary>
/// <typeparam name="T">Нужен что определить преобразование тела ответа (json) из строки в С# объект</typeparam>
class ResponseOnRequest<T>
{
    public HttpWebResponse response { get; set; }
    public string bodyResponse { get; set; }
    public T bodyDeserializd { get; set; }
    public HttpWebRequest request { get; set; }
}
class ResponseOnRequest
{
    public HttpWebResponse response { get; set; }
    public string bodyResponse { get; set; }
    public HttpWebRequest request { get; set; }
}

// methods http
public static class HttpMethods
{
    public static readonly string Connect = "CONNECT";
    public static readonly string Delete = "DELETE";
    public static readonly string Get = "GET";
    public static readonly string Head = "HEAD";
    public static readonly string Options = "OPTIONS";
    public static readonly string Patch = "PATCH";
    public static readonly string Post = "POST";
    public static readonly string Put = "PUT";
    public static readonly string Trace = "TRACE";
}