using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace ELMA_API
{
    class BaseHttp
    {
        protected string hostaddress;
        protected AuthJsonResponse auth;

        public BaseHttp(string hostaddress, string token, string user, string password) {
            // get environment variable localhost address
            this.hostaddress = hostaddress;
            this.auth = this.getAuth(token, user, password);
        }

        /// <summary>
        /// Запросы Http
        /// </summary>
        /// <param name="path"></param>
        /// <param name="method"></param>
        /// <param name="body">тело запросы, если есть</param>
        /// <param name="queryParams">параметры строки запросы</param>
        /// <param name="elmaTokens">
        ///  Добавляет к запросы headers c токенами которые
        ///  необходимы для запросов к серверу elma,
        ///  поумолчанию true т.е. всегда добавляет заголовки с токенами
        ///  </param>
        public ResponseOnRequest request(
            String path, 
            String method, 
            String body = null, 
            Dictionary<string, string> queryParams = null,
            bool elmaTokens = true)
        {
            string fullUrl = "http://" + this.hostaddress + path;

            // * if pass queryParameters
            if (queryParams != null) {
                List<string> qParams = new List<string>();

                foreach (var record in queryParams)
                    qParams.Add($"{record.Key}={record.Value}");
                
                fullUrl += "?" + String.Join('&', qParams.ToArray());
            }
            
            HttpWebRequest req = WebRequest.Create(String.Format(fullUrl)) as HttpWebRequest;
            req.Method = method;

            // добавление в заголовки токенов elma
            if (elmaTokens) {
                req.Headers.Add("AuthToken", this.auth.AuthToken);
                req.Headers.Add("SessionToken", this.auth.SessionToken);
            }
            
            req.Timeout = 10000;
            req.ContentType = "application/json; charset=utf-8";

            // * body request
            if (body != null) {
                var sendBody = Encoding.UTF8.GetBytes(body ?? "");
                req.ContentLength = sendBody.Length;
                Stream sendStream = req.GetRequestStream();
                sendStream.Write(sendBody, 0, sendBody.Length);
            }
            
            var response = req.GetResponse() as HttpWebResponse;
            var resStream = response.GetResponseStream();
            var sr = new StreamReader(resStream, Encoding.UTF8);

            return new ResponseOnRequest
            {
                bodyString = sr.ReadToEnd(),
                response = response,
            };
        }

        public ResponseOnRequest<T> request<T>(
            String path, 
            String method, 
            String body = null, 
            Dictionary<string, string> queryParams = null,
            bool elmaTokens = true)
        {
            var resp = this.request(path, method, body, queryParams, elmaTokens);

            // ! преобразование ответа строки http в объект C# с типом данных generic T
            var jsonBody = JsonConvert.DeserializeObject<T>(resp.bodyString);

            return new ResponseOnRequest<T>
            {
                bodyString = resp.bodyString,
                response = resp.response,
                jsonBody = jsonBody,
            };
        }

        private AuthJsonResponse getAuth(string applicationToken, string user, string password)
        {
            //создаем http запрос
            HttpWebRequest req = WebRequest.Create(String.Format(
                "http://{0}/API/REST/Authorization/LoginWith?username={1}", hostaddress, user)) as HttpWebRequest;
            req.Headers.Add("ApplicationToken", applicationToken);
            req.Method = "POST";
            req.Timeout = 10000;
            req.ContentType = "application/json; charset=utf-8";

            // данные для отправки. используется для передачи пароля. пароль нужно записать вместо пустой строки
            // обезательно добавлять кавычики \" в начало и конец пароля !
            var sentData = Encoding.UTF8.GetBytes($"\"{password}\"");
            req.ContentLength = sentData.Length;
            Stream sendStream = req.GetRequestStream();
            sendStream.Write(sentData, 0, sentData.Length);

            // Console.WriteLine($"\"{password}\"");
            // Console.WriteLine(user);
            // Console.WriteLine(password);
            // Console.WriteLine(this.hostaddress);

            //получение ответа
            var res = req.GetResponse() as HttpWebResponse;
            var resStream = res.GetResponseStream();
            var sr = new StreamReader(resStream, Encoding.UTF8);
            string responseBody = sr.ReadToEnd();

            // Пример ответа от сервера ELMA при вызове метода sr.ReadToEnd() вернет строковое представление
            // {
            //  "AuthToken":"e8d36fd8-fb2c-4f9b-b011-af8900bdbeb7",
            //  "CurrentUserId":"1",
            //  "Lang":"ru-RU",
            //  "SessionToken":"9F0E3BDF5678D3F4056471F585506003E8F343D46FC231A5FB526403AB902E1AC3E0B31D084EA5295147E0C154139509A6F7D567747FB5860772E814A0342C7F"
            // }

            // получение необходимых данных из запроса
            var responseJson = JsonConvert.DeserializeObject<AuthJsonResponse>(responseBody);

            // Logging 
            Log.Success(SuccessTitle.loginElma, "connection is successful");

            return responseJson;
        }
    }

    // для обпределение модели ответа в формате json, нужен для Newtonsoft.Json.JsonConvert.DeserailizeObject()
    class AuthJsonResponse
    {
        public string AuthToken { get; set; }
        public string CurrentUserId { get; set; }
        public string Lang { get; set; }
        public string SessionToken { get; set; }
    }

    /// <summary>
    /// Ответ на запрос по http
    /// </summary>
    /// <typeparam name="T">Нужен что определить преобразование тела ответа из строки в С# объект</typeparam>
    class ResponseOnRequest<T> 
    {
        public HttpWebResponse response { get; set; }
        public string bodyString { get; set; }
        public T jsonBody { get; set; }
    }
     class ResponseOnRequest 
    {
        public HttpWebResponse response { get; set; }
        public string bodyString { get; set; }
    }
}
