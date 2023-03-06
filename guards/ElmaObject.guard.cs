using System;
using System.Collections.Generic;
using HtmlAgilityPack;

namespace ELMA_API
{
    class ElmaObject
    {
        /// <summary>
        /// проверка на существование конкретного ключа в объекта-справочнике ELMA
        /// с возможность более точной проверки через указание типа в параметре type
        /// </summary>
        /// <returns>вернет true если найдет ключ в спрвочнике Elma, иначе null</returns>
        static public bool existRecord(
            BaseHttp baseHttpElma, 
            string typeUidElma,
            string key,
            string type = null) 
        {
            var respHtml = baseHttpElma.request(
                path: "/API/Help/Type",
                method: "GET",
                queryParams: new Dictionary<string, string>() {
                    ["uid"] = typeUidElma
            });
            
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(respHtml);

            // наименование объкта-справочника ELMA, дожне быть всегда на html странице
            string nameObjectElma = htmlDoc.DocumentNode.SelectSingleNode("html/body/div[1]").InnerText.Trim();

            // поиск всех тэгов html c ключами и типами для объекта справочника ELMA
            var htmlNodes = htmlDoc.DocumentNode.SelectNodes("html/body/table[@class=\"def-table\"]/tr");

            // хранилище ключей и соотвественно типов данного объекта справочника ELMA
            Dictionary<string, string> structureElmaObject = new Dictionary<string, string>();

            // цикл для перебора ключий и типов с добавление их в словарь
            foreach (var node in htmlNodes)
            {
                var keyDict = node.SelectSingleNode("td[1]");
                var valueDict = node.SelectSingleNode("td[2]").ChildNodes[1];
                
                // добавит в словарь только при условии что
                // найден ключ и его тип объекта-справочника ELMA
                if (!String.IsNullOrEmpty(keyDict.InnerText) 
                    && !String.IsNullOrEmpty(valueDict.InnerText))
                    structureElmaObject.TryAdd(
                        keyDict.InnerText.Trim(), 
                        valueDict.InnerText.Trim());
            }

            // если будет передаваться аргументом тип тогда нужно 
            // дополнительно проверять есть ли такой ключ без учета
            // данного типа, Это нужно в случаи если функция найдет 
            // ключ без типа, нужно будет уведомить пользователя об этом
            // так как возможно неправльно указан тип для данного ключа
            // уведомить через логирование в конце этой функции
            bool tryFindOnlyByKey = false;
            // также будет возможность показать какой тип указан 
            // для данного ключа объекта-справочника elma
            string findTypeForKey = null;

            foreach (var record in structureElmaObject)
            {
                // поиск только по названию ключа существеует 
                // ли он в объекте справочнике, регистр букв учивывается
                if (type == null && key.Trim() == record.Key.Trim()) 
                {
                    return true;
                }

                // поиск с учетом типа данных 
                if (type != null) {
                    var indexStartType = record.Value.ToLower().IndexOf("тип:");
                    if (record.Value.ToLower().Substring(indexStartType == -1 ? 0 : indexStartType).Contains(type.ToLower()) 
                        && key.Trim() == record.Key.Trim()) 
                    {
                        return true;
                    }
                    // в случаи если найдет такой ключ в объекте-справочнике
                    if (key.Trim() == record.Key.Trim())
                    {
                        tryFindOnlyByKey = true;
                        findTypeForKey = record.Value.Trim();
                    }
                        
                }
            }

            // если найдет ключ для данного объекта-справочника elma
            if (tryFindOnlyByKey) {
                Log.Warn(WarnTitle.keyObjectElma, $"ключ \"{key.Trim()}\" для объекта-справочника elma \"{nameObjectElma}\" найден");
                Log.Notice(NoticeTitle.check, $"передан тип для функции, следует проверить переданный тип: \"{type.ToLower()}\"");
                Log.Notice(NoticeTitle.check, $"для ключа \"{key.Trim()}\", типом является \"{findTypeForKey.ToLower()}\"");

            }
            else {
                Log.Warn(WarnTitle.keyObjectElma, $"ключ \"{key.Trim()}\" для объекта-справочника elma \"{nameObjectElma}\" не найден");
            }

            return false;
        }
    }
}