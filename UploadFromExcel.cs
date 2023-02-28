using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ELMA_API
{
    class UploadFromExcel
    {
        public BaseHttp baseHttp;
        public RequestElma reqElma;
        public TypesUidElma typesUidElma;
        public UploadFromExcel(BaseHttp baseHttp, RequestElma reqElma, TypesUidElma typesUidElma)
        {
            this.baseHttp = baseHttp; // нужен для простых запросов-http к серверу Elma
            this.reqElma = reqElma; // нужен для уже подготовленных запросов к Elma
            this.typesUidElma = typesUidElma; // нужен для уникальных идентификаторов Elma
        }

        // добавление учёных званий для пользователей elma которые являются преподователями
        // ! ОБЯЗАТЕЛЬНО для СИСТЕМНОГО СПРАВОЧНИКА пользователь в elma дожен быть дополнительное 
        // ! кастомное свой-во UchyonoeZvanie - УчёноеЗвание, значение которого является 
        // ! перечисление UchyonoeZvanie - со следующими значениями	(Docent,Доцент,0) или (Professor,Профессор,1)
        public void academicTitle(string pathExcel) 
        {
            // получение всех пользоваетелей user из сервера elma
            var getUsers = this.reqElma.users();

            // хранилище пользователей после фильтрации через цикл
            List<UserElma> users = new List<UserElma>();
            
            foreach (Root user in getUsers)
            {
                // нужно вытащить Id и FullName и добавить в хранилище users
                var id = this.reqElma.getValueItem(user.Items, "Id");
                var fullName = this.reqElma.getValueItem(user.Items, "FullName");
                // добавление пользователя в хранилище
                users.Add(new UserElma(id, fullName));
            }

            // загрузка excel файла
            // * КОЛОНКИ дожны быть следующие:
            // ФИО-Полностью 	ФИО-Сокращенно	УченоеЗвание	Кафедра
            var workbook = new Aspose.Cells.Workbook(pathExcel);
            var currentSheet = workbook.Worksheets[0];
            int currRow = 0, currCell = 0;
            // перебор по файлу excel, проверяем не пустое ли первое поле,  если  ДА, то  завершаем цикл
            while (currentSheet.Cells[currRow, currCell].StringValue != "") 
            {
                string fullNameExcel = currentSheet.Cells[currRow, currCell].StringValue; // полное фио
                string academicTitleExcel = currentSheet.Cells[currRow, currCell + 2].StringValue; // учёное звание

                var tryFindUser = users.Find(c => c.fullName.Trim().ToLower() == fullNameExcel.Trim().ToLower());
                // если найдет такого же пользователя на сервере Elma как в данном excel файле
                if (tryFindUser != null) 
                {
                    Console.WriteLine("User is found :-> " + tryFindUser.fullName);

                    // ! так дожен выглядеть загружаймый item
                    // ! это перечисление для свойства пол в справочнике студенты
                    // "Data": null,
                    // "DataArray": [],
                    // "Name": "Pol",
                    // "Value": "1",
                }
                
                // * смещение строки на следующую
                currRow++;
            }

            // Обновить существующий объект в системе
            // /API/REST/Entity/Update/{TYPEUID}/{ENTITYID}
            // * нужено всего два значения для обновления typeuid и entityid


        }
    }

    class UserElma 
    {
        public string entityId;
        public string fullName;
        public UserElma(string entityId, string fullName) 
        {
            this.entityId = entityId;
            this.fullName = fullName;
        }
    }
}