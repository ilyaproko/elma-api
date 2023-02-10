using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace ELMA_API
{
    class RequestsDatabase
    {

        // создание экземпляра подключения к БД MS Sql Server
        public static ConnectionDatabase database = new ConnectionDatabase();

        public static DataTable requestDB(String query_string, ConnectionDatabase database)
        {
            SqlDataAdapter adapter = new SqlDataAdapter();
            SqlCommand command = new SqlCommand(query_string, database.getConnection());
            DataTable table = new DataTable();
            adapter.SelectCommand = command;
            adapter.Fill(table);

            // закрытие соединение с сервером базы данных если соединение открыто
            database.closeConnection();

            return table;
        }

        public static List<string> getUchebnyePlany()
        {
            // строка запроса к БД для выборки уникальных значений "учебныйх планов" представления ШвебельГруппы
            string query_string = $"select distinct \"Учебный_План\" from dbo.ШвебельГруппы";

            // получение данных из БД по запросу sql
            DataTable table = requestDB(query_string: query_string, database: database);

            // list storage for unique uchebnyePlany from DATABASE dekanat tabale dbo.ШвебельГруппы
            List<string> educational_plans_db = new List<string>();

            // block testing
            educational_plans_db.Add("12345678.plx");
            // educational_plans_db.Add("40056.plx");
            educational_plans_db.Add("123456.plx");
            // educational_plans_db.Add("9456.plx");
            // educational_plans_db.Add("01023456.plx");
            // educational_plans_db.Add("456.plx");
            // blockEnd testing

            // fill storage educational_plans_db from database dekanat
            foreach (DataRow row in table.Rows)
            {
                string educational_plan = row.Field<string>("Учебный_План"); // получаем учебный план
                educational_plans_db.Add(educational_plan); // добавляем полученный учебный план в хранилище
            }

            return educational_plans_db;
        }

        public static List<FacultyGuide> getFakuljtety()
        {
            // строка запроса к БД для выборки уникальных значений таблицы Факультеты
            string query_string = $"select distinct \"Факультет\", \"Сокращение\" from \"Факультеты\"";

            // Получение данных из БД по запросу sql
            DataTable table = requestDB(query_string: query_string, database: database);

            // list storage for unique Fakuljtety from DATABASE dekanat table Факультеты
            List<FacultyGuide> fakuljtety_db = new List<FacultyGuide>();

            // fill storage educational_plans_db from database dekanat
            foreach (DataRow row in table.Rows)
            {
                string fakuljtet_long = row.Field<string>("Факультет"); // получение полного название факультета
                string fakuljtet_short = row.Field<string>("Сокращение"); // получение сокращенного названия факультета
                
                // добавляем факультет в хранилище
                fakuljtety_db.Add(
                    new FacultyGuide() {
                        short_name = fakuljtet_short,
                        long_name = fakuljtet_long,
                    } ); 
            }

            return fakuljtety_db;
        }

        public static List<String> getDisciplines()
        {
            // строка запроса к БД для выборки уникальных значений таблицы Факультеты
            string query_string = $"select distinct \"Дисциплина\" from \"Деканат\".\"dbo\".\"ШвебельПрактикиБезКомпет\"";

            // Получение данных из БД по запросу sql
            DataTable table = requestDB(query_string: query_string, database: database);
            
            // list storage for unique Disciplines from DATABASE dekanat view ШвебельПрактикиБезКомпет
            List<String> disciplines_db = new List<String>();

            foreach (DataRow row in table.Rows)
            {
                disciplines_db.Add(row.Field<string>("Дисциплина"));
            }
            
            return disciplines_db;
        }

        public static List<DirectionPreparation> getDirectionPreparation()
        {
            // строка запроса к БД для выборки уникальных значений "направлений подготовки и специальности" представления ШвебельГруппы
            string query_string = @"
                                    select 
                                        Наименование,
                                        Код
                                    from (
                                        select
                                        distinct lower(trim(replace(
                                        replace(replace(
                                        replace(replace(
                                        replace(replace(
                                        replace(replace(
                                        replace(replace(
                                        replace(replace(
                                        replace(replace(
                                        Титул, '  ', ''), '-', ''), 'Специальность', ''), 'Направление', ''), '9', ''), '8', ''), '7', ''), '6', ''), '5', ''), '4', ''), '3', ''), '2', ''), '1', ''), '.', ''), '0', '')))
                                        as Наименование, 
                                        Шифр as Код
                                        from dbo.ШвебельГруппы
                                        where Шифр <> ''
                                    ) as subSelect;";

            // Получение данных из БД по запросу sql
            DataTable table = requestDB(query_string: query_string, database: database);

            // list storage for unique Entity from DATABASE dekanat table Направления подготовки
            List<DirectionPreparation> directions_pre_db = new List<DirectionPreparation>();
            List<String> codes = new List<String>();

            foreach (DataRow row in table.Rows)
            {
                // проверка если такой код уже добавили в хранилище тогда пропустить эту запись
                if (!codes.Contains(row.Field<String>("Код").Trim())) {
                    codes.Add(row.Field<String>("Код").Trim());
                    directions_pre_db.Add(
                        new DirectionPreparation()
                        {
                            Naimenovanie = row.Field<String>("Наименование").Trim(),
                            Kod = row.Field<String>("Код").Trim()
                        }
                    );
                }
            }

            return directions_pre_db;
        }
    
        public static List<DepartmentFromDB> getDepartments() 
        {
            // строка запроса к БД для выборки уникальных значений "кафедры"
            string query_string = @"
    select
        distinct dbo.ШвебельПрактики.КодКафедры as 'КодКафедры',
        dbo.Кафедры.Название as 'НазваниеКафедры',
        dbo.Кафедры.Сокращение as 'КафедраСокращенно',
        dbo.Факультеты.Сокращение as 'ФакультетСокращенно',
        dbo.Факультеты.Факультет as 'Факультет',
        dbo.Кафедры.Аудитория as 'АудиторияКафедры',
        dbo.Кафедры.Телефон as 'ТелефонКафедры',
        dbo.Кафедры.ЗавКафедрой  as 'ЗавКафедрой'
    from dbo.ШвебельПрактики
        join dbo.Кафедры on dbo.ШвебельПрактики.КодКафедры = dbo.Кафедры.Код
        join dbo.Факультеты on dbo.Факультеты.Код = dbo.Кафедры.Код_Факультета";

            // Получение данных из БД по запросу sql
            DataTable table = requestDB(query_string: query_string, database: database);

            // list storage for unique Entity from DATABASE dekanat table Кафедры
            List<DepartmentFromDB> departments_db = new List<DepartmentFromDB>();

            foreach (DataRow row in table.Rows) 
            {
                departments_db.Add(
                    new DepartmentFromDB() {
                        Code = row.Field<int>("КодКафедры").ToString(),
                        NameLong = row.Field<String>("НазваниеКафедры"),
                        NameShort = row.Field<String>("КафедраСокращенно"),
                        FacultyShort = row.Field<String>("ФакультетСокращенно"),
                        FacultyLong = row.Field<String>("Факультет"),
                        Room = row.Field<String>("АудиторияКафедры"),
                        Phone = row.Field<String>("ТелефонКафедры"),
                        HeadOfDepartment = row.Field<String>("ЗавКафедрой")
                    }
                );
            }

            return departments_db;
        }

    }
}
