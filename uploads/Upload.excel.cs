using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Spectre.Console;

namespace ELMA_API
{
    class UploadExcel
    {
        public BaseHttp baseHttp;
        public Elma reqElma;
        public UploadExcel(BaseHttp baseHttp, Elma reqElma)
        {
            this.baseHttp = baseHttp; // нужен для простых запросов-http к серверу Elma
            this.reqElma = reqElma; // нужен для уже подготовленных запросов к Elma
        }

        /// <summary>
        /// добавление учёных званий для пользователей elma которые являются преподователями
        /// ! ОБЯЗАТЕЛЬНО для СИСТЕМНОГО СПРАВОЧНИКА пользователь в elma дожен быть дополнительное 
        /// ! кастомное свой-во UchyonoeZvanie - УчёноеЗвание, значение которого является 
        /// ! перечисление UchyonoeZvanie - со следующими значениями	(Docent,Доцент,0) или (Professor,Профессор,1)
        /// * колонки в excel-файле дожны быть следующие: ФИО-Полностью | ФИО-Сокращенно | УченоеЗвание | Кафедра
        /// </summary>
        public void academicTitle(string pathExcel) 
        {
            Log.Info(InfoTitle.uploadExcel, "academic title", colorTitle: "orangered1");

            // проверка на содержание и правильное расположнеие колонок в excel файле
            if (!ExcelGuard.academicTitle(pathExcel))
                throw new Exception("Error Excel File");

            // проверка существует ли в объекте-спрвочнике ELMA "user"
            // ключ с конкретным типом данных 
            if (!ElmaGuard.existRecord(baseHttp, TypesUidElma.users, "UchyonoeZvanie", "перечисление"))
                throw new Exception("Error existing record object ELMA");

            // получение всех пользоваетелей user из сервера elma
            var getUsersElma = this.reqElma.users();

            // хранилище пользователей elma после фильтрации через цикл
            List<UserElma> usersELma = new List<UserElma>();

            // хранилище ответов от сервера elma после добавление ученых званий пользователям
            List<string> responsesElma = new List<string>();
            
            foreach (Root user in getUsersElma)
            {
                // нужно вытащить Id и FullName и добавить в хранилище users, опционально userName, 
                // также берется учёное звание, оно нужно чтобы сравнить с тем что находится в excel файле
                // чтобы избежать загрузки одинаковых учёных звание для пользователей
                var id = Elma.getValueItem(user.Items, "Id");
                var fullName = Elma.getValueItem(user.Items, "FullName");
                var userName = Elma.getValueItem(user.Items, "UserName"); // учетная запись
                var academicTitle = Elma.getValueItem(user.Items, "UchyonoeZvanie");
                // добавление пользователя в хранилище
                usersELma.Add(new UserElma(id, fullName, userName,academicTitle));
            }

            // загрузка excel файла
            var workbook = new Aspose.Cells.Workbook(pathExcel);
            var currentSheet = workbook.Worksheets[0];
            int currRow = 1, currCell = 0;

            // перебор по файлу excel, проверяем не пустое ли первое поле,  если  ДА, то  завершаем цикл
            while (currentSheet.Cells[currRow, currCell].StringValue != "")
            {
                // полное фио
                string fullNameExcel = currentSheet.Cells[currRow, currCell].StringValue; 
                // учёное звание
                string academicTitleExcel = currentSheet.Cells[currRow, currCell + 2].StringValue; 
                // попытка найти пользователя по ФИО
                var tryFindUser = usersELma.Find(c => c.fullName.Trim().ToLower() == fullNameExcel.Trim().ToLower());
                
                // если найдет такого же пользователя на сервере Elma а также в excel-файле 
                // для данного пользователя поле учёное звание не должно быть пустым
                if (tryFindUser != null && academicTitleExcel != "") 
                {
                    // определение из excel-файла учёное звание конкретного пользователя
                    // где значение выступает перечисление в Elma 
                    // UchyonoeZvanie УчёноеЗвание. (тип: УчёноеЗвание (Перечисление))
                    // 0 - Доцент | 1 - Профессор
                    // если нет учёного звания у пользователя тогда будет null
                    string prepareAcadTitle = null;
                    if (academicTitleExcel.ToLower().Contains("профессор") 
                        || academicTitleExcel.ToLower().Contains("проф.") )
                        prepareAcadTitle = "1";
                    if (academicTitleExcel.ToLower().Contains("доцент") 
                        || academicTitleExcel.ToLower().Contains("доц."))
                        prepareAcadTitle = "0";

                    var itemAcademicTitle = new Item() 
                    {
                        Data = null,
                        DataArray = new List<object>(),
                        Name = "UchyonoeZvanie",
                        Value = prepareAcadTitle
                    };
                    var requestBody = new Root()
                    {
                        Items = new List<Item>() {itemAcademicTitle},
                        Value = ""
                    };

                    // проверка на то что учёное звание отличатеся в excel файле и на сервере elma
                    // если это так тогда выполнит код ниже
                    if (tryFindUser.academicTitle != prepareAcadTitle) {
                        // зарпос к Elma для обновление поля
                        // УчёноеЗвание / UchyonoeZvanie для пользователя 
                        var resp = this.baseHttp.request(
                            path: $"/API/REST/Entity/Update/{TypesUidElma.users}/{tryFindUser.entityId}",
                            method: "POST",
                            body: JsonConvert.SerializeObject(requestBody)
                        ).bodyString;

                        // добавление ответа в хранилище от Elma на запрос обновление данных пользоваетля
                        responsesElma.Add(resp);
                    }

                } 
                // * смещение строки на следующую, не удалять иначе бесконечный цикл
                currRow++;
            }
            
            // хранилище для пользователей с учёч. званием Профессор
            List<Root> professors = new List<Root>();
            // хранилище для пользователей с учёч. званием Доцент
            List<Root> docents = new List<Root>();
            // хранилище для пользователей без учёчю звания
            List<Root> usersWithoutAcadTitle = new List<Root>();

            // новый запрос к серверу elma с уже обновленными учеными званиями
            var againReqUserElma = this.reqElma.users();
            againReqUserElma.ForEach(user => {
                var acadTitle = Elma.getValueItem(user.Items, "UchyonoeZvanie");
                if (acadTitle == "0") docents.Add(user);
                else if (acadTitle == "1") professors.Add(user);
                else if (acadTitle == null) usersWithoutAcadTitle.Add(user);
            });

            // логиррование
            Log.Info(InfoTitle.dataElma, $"professors in elma server: {professors.Count}");
            Log.Info(InfoTitle.dataElma, $"docents in elma server: {docents.Count}");
            Log.Info(InfoTitle.dataElma, $"users without academic titles: {usersWithoutAcadTitle.Count}");
            // логирует обновление учёных званий если обновит хотябы один на сервер elma
            if (responsesElma.Count != 0)
                Log.Success(SuccessTitle.updatedAcadTitle, $"academic titles are updated for users: {responsesElma.Count}");
            else
                Log.Success(SuccessTitle.synchronized, $"all academic titles synchronized");
        }

        /// <summary>
        /// добавление новых пользователей которые ОТСУТСТВУЮТ на сервере elma,
        /// новые пользователи определяются по ФИО на сервере ELMA
        /// ! ОБЯЗАТЕЛЬНО для СИСТЕМНОГО СПРАВОЧНИКА пользователь в elma дожен быть дополнительное 
        /// ! кастомное свой-во UchyonoeZvanie - УчёноеЗвание
        /// * колонки в excel-файле дожны быть следующие: ФИО-Полностью | ФИО-Сокращенно | УченоеЗвание | Кафедра
        /// </summary>
        public void addUsers(string pathExcel, BaseHttp baseHttpElma) 
        {
            Log.Info(InfoTitle.uploadExcel, "new users", colorTitle: "orangered1");

            // проверка на содержание и правильное расположнеие колонок в excel файле
            if (!ExcelGuard.academicTitle(pathExcel))
                throw new Exception("Error Excel File");

            // проверка существует ли в объекте-спрвочнике ELMA "user"
            // ключ с конкретным типом данных 
            if (!ElmaGuard.existRecord(baseHttp, TypesUidElma.users, "UchyonoeZvanie", "перечисление"))
                throw new Exception("Error existing record object ELMA");

            // получение всех пользоваетелей user из сервера elma
            var getUsersElma = this.reqElma.users();
            // хранилище пользователей elma после фильтрации через цикл
            List<UserElma> usersELma = new List<UserElma>();
            // хранилище пользователей которых нет на сервере Elma
            List<NewUser> newies = new List<NewUser>();
            // хранилище ответов от сервера elma после добавление новых пользователей
            List<string> responsesElma = new List<string>();
            
            foreach (Root user in getUsersElma)
            {
                // нужно вытащить Id и FullName и UserName и добавить в хранилище
                var id = Elma.getValueItem(user.Items, "Id");
                var fullName = Elma.getValueItem(user.Items, "FullName");
                var userName = Elma.getValueItem(user.Items, "UserName");
                var academicTitle = Elma.getValueItem(user.Items, "UchyonoeZvanie");
                // добавление пользователя в хранилище
                usersELma.Add(new UserElma(id, fullName, userName, academicTitle));
            }
            
            // загрузка excel файла
            var workbook = new Aspose.Cells.Workbook(pathExcel);
            var currentSheet = workbook.Worksheets[0];
            int currRow = 1, currCell = 0;

            // цикл для добавление пользователей в excel файле которых нет на сервере ELMA
            // перебор по файлу excel, проверяем не пустое ли первое поле,  если  ДА, то  завершаем цикл
            while (currentSheet.Cells[currRow, currCell].StringValue != "")
            {
                // полное фио
                string fullNameExcel = currentSheet.Cells[currRow, currCell].StringValue; 
                // учёное звание
                string academicTitleExcel = currentSheet.Cells[currRow, currCell + 2].StringValue; 
                // попытка найти пользователя по ФИО
                var tryFindUser = usersELma.Find(c => c.fullName.Trim().ToLower() == fullNameExcel.Trim().ToLower());

                // если не найдет на сервере Elma пользователя с идентичным ФИО
                // и ФИО указано полноценно (после Split(" ") дожно быть 3 элемента)
                if (tryFindUser == null && fullNameExcel.Split(" ").Length == 3)
                {
                    string[] fio = fullNameExcel.Split(" ");
                    string lastName = fio[0];
                    string firstName = fio[1];
                    string middleName = fio[2];

                    NewUser newie = new NewUser(lastName, firstName, middleName, academicTitleExcel);
                    // добавление в хранилище новых пользователей
                    newies.Add(newie);
                }
                // * смещение строки на следующую, не удалять иначе бесконечный цикл
                currRow++;
            }

            newies.ForEach(newie => {
                // проверка что такой учетной записи нет, тогда исполнит код ниже
                // пол учетной записью подразумевается Уникальный Логин пользователя 
                // по которому он заходит в систему ELMA
                if (usersELma.Find(user => user.userName == newie.userName) == null)
                {
                    Item iFirstName = new Item() 
                    {
                        Data = null, DataArray = new List<object>(),
                        Name = "FirstName", Value = newie.firstName
                    };
                    Item iLastName = new Item() 
                    {
                        Data = null, DataArray = new List<object>(),
                        Name = "LastName", Value = newie.lastName
                    };
                    Item iMiddleName = new Item() 
                    {
                        Data = null, DataArray = new List<object>(),
                        Name = "MiddleName", Value = newie.middleName
                    };
                    Item iAcadTitle = new Item() 
                    {
                        Data = null, DataArray = new List<object>(),
                        Name = "UchyonoeZvanie", Value = newie.acadTitle
                    };
                    Item iUserName = new Item()
                    {
                        Data = null, DataArray = new List<object>(),
                        Name = "UserName", Value = newie.userName
                    };
                    Root requestBody = new Root()
                    {
                        Items = new List<Item>() {iAcadTitle, iFirstName, iLastName, iMiddleName, iUserName},
                        Value = ""
                    };

                    // зарпос к Elma для создание нового пользователя
                    var resp = this.baseHttp.request(
                        path: $"/API/REST/Entity/Insert/{TypesUidElma.users}",
                        method: "POST",
                        body: JsonConvert.SerializeObject(requestBody)
                    ).bodyString;

                    // добавление ответа в хранилище от Elma на запрос обновление данных пользоваетля
                    responsesElma.Add(resp);
                }
            });

            // логирует кол-во пользователей
            Log.Info(InfoTitle.dataElma, $"number of users in elma: {getUsersElma.Count}");

            // логирует кол-во пользователей которых нет на сервере elma
            if (newies.Count != 0)
                Log.Notice(NoticeTitle.missedData, $"users aren't in server elma: {newies.Count}");

            // логирует ответы от сервера elma если добавит хотябы одного пользователя
            if (responsesElma.Count != 0) 
                Log.Success(SuccessTitle.uploadNewUsers, $"number of new users added: {responsesElma.Count}");
            else Log.Success(SuccessTitle.synchronized, "all users synchronized");
        }
    }

    class UserElma 
    {
        public readonly string entityId; // id пользователя
        public readonly string fullName; // полное ФИО
        public readonly string userName; // учетная запись пользователя
        public readonly string academicTitle; // учёное звание
        public UserElma(string entityId, string fullName, string userName, string academicTitle) 
        {
            this.entityId = entityId;
            this.fullName = fullName;
            this.userName = userName;
            this.academicTitle = academicTitle;
        }
    }

    class NewUser
    {
        public readonly string firstName;
        public readonly string lastName;
        public readonly string middleName;
        public readonly string acadTitle; // учёное звание
        public readonly string userName;
        public NewUser(string lastName, string firstName, string middleName, string academicTitle)
        {
            this.firstName = firstName;
            this.lastName = lastName;
            this.middleName = middleName;
            this.acadTitle = this.findOutAcademicTitle(academicTitle);
            this.userName = 
                this.makeUserName(lastName + "_" + firstName.Substring(0, 1) + middleName.Substring(0, 1));
        }

        public string findOutAcademicTitle(string acadTitle) 
        {
            if (acadTitle.ToLower().Contains("профессор") 
                || acadTitle.ToLower().Contains("проф."))
                return "1";
            if (acadTitle.ToLower().Contains("доцент") 
                || acadTitle.ToLower().Contains("доц."))
                return "0";

            return null;
        }

        /// <summary>
        /// транспилирует строку из русского на английский
        /// и добавляет рандомное число в конце от 10 до 100 
        /// для того чтобы уменьшить вероятность что userName
        /// уже кем-то занет на серве Elma
        /// </summary>
        public string makeUserName(string userNameRus)
        {
            Random rnd = new Random();
            return $"{Translit.TranslitString(userNameRus.ToLower())}_{rnd.Next(1, 100)}";
        }
    }

    public class Translit
    {
        // объявляем и заполняем словарь с заменами
        // при желании можно исправить словать или дополнить
        static Dictionary<string, string> dictionaryChar = new Dictionary<string, string>()
        {
            {"а","a"},
            {"б","b"},
            {"в","v"},
            {"г","g"},
            {"д","d"},
            {"е","e"},
            {"ё","yo"},
            {"ж","zh"},
            {"з","z"},
            {"и","i"},
            {"й","y"},
            {"к","k"},
            {"л","l"},
            {"м","m"},
            {"н","n"},
            {"о","o"},
            {"п","p"},
            {"р","r"},
            {"с","s"},
            {"т","t"},
            {"у","u"},
            {"ф","f"},
            {"х","h"},
            {"ц","ts"},
            {"ч","ch"},
            {"ш","sh"},
            {"щ","sch"},
            {"ъ","ij"},
            {"ы","yi"},
            {"ь","j"},
            {"э","e"},
            {"ю","yu"},
            {"я","ya"}
        };
        /// <summary>
        /// метод делает транслит на латиницу
        /// </summary>
        /// <param name="source"> это входная строка для транслитерации </param>
        /// <returns>получаем строку после транслитерации</returns>
        static public string TranslitString(string source)
        {
            var result = "";
            // проход по строке для поиска символов подлежащих замене которые находятся в словаре dictionaryChar
            foreach (var ch in source)
            {
                var ss = "";
                // берём каждый символ строки и проверяем его на нахождение его в словаре для замены,
                // если в словаре есть ключ с таким значением то получаем true 
                // и добавляем значение из словаря соответствующее ключу
                if (dictionaryChar.TryGetValue(ch.ToString(), out ss))
                {
                    result += ss;
                }
                // иначе добавляем тот же символ
                else result += ch;
            }
            return result;
        }
    }
}