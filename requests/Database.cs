﻿using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace ELMA_API;

class DbMsSql
{
    // создание экземпляра подключения к БД MS Sql Server
    public ConnectionDatabase database;

    public DbMsSql(string dataSource, string initialCatalog)
    {
        this.database = new ConnectionDatabase(dataSource, initialCatalog);
    }

    public DataTable requestDb(String query_string)
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

    public List<string> educationalPlans()
    {
        // строка запроса к БД для выборки уникальных значений "учебныйх планов" представления ШвебельГруппы
        string query_string = $"select distinct \"Учебный_План\" from dbo.ШвебельГруппы";

        // получение данных из БД по запросу sql
        DataTable table = requestDb(query_string: query_string);

        // list storage for unique uchebnyePlany from DATABASE dekanat tabale dbo.ШвебельГруппы
        List<string> educational_plans_db = new List<string>();

        // fill storage educational_plans_db from database dekanat
        foreach (DataRow row in table.Rows)
        {
            string educational_plan = row.Field<string>("Учебный_План"); // получаем учебный план
            educational_plans_db.Add(educational_plan); // добавляем полученный учебный план в хранилище
        }

        return educational_plans_db;
    }

    public List<FacultyGuide> faculties()
    {
        // строка запроса к БД для выборки уникальных значений таблицы Факультеты
        string query_string = $"select distinct \"Факультет\", \"Сокращение\" from \"Факультеты\"";

        // Получение данных из БД по запросу sql
        DataTable table = requestDb(query_string: query_string);

        // list storage for unique Fakuljtety from DATABASE dekanat table Факультеты
        List<FacultyGuide> fakuljtety_db = new List<FacultyGuide>();

        // fill storage educational_plans_db from database dekanat
        foreach (DataRow row in table.Rows)
        {
            string fakuljtet_long = row.Field<string>("Факультет"); // получение полного название факультета
            string fakuljtet_short = row.Field<string>("Сокращение"); // получение сокращенного названия факультета

            // добавляем факультет в хранилище
            fakuljtety_db.Add(
                new FacultyGuide()
                {
                    shortName = fakuljtet_short,
                    longName = fakuljtet_long,
                });
        }

        return fakuljtety_db;
    }

    public List<String> disciplines()
    {
        // строка запроса к БД для выборки уникальных значений таблицы Факультеты
        string query_string = $"select distinct \"Дисциплина\" from \"Деканат\".\"dbo\".\"ШвебельПрактикиБезКомпет\"";

        // Получение данных из БД по запросу sql
        DataTable table = requestDb(query_string: query_string);

        // list storage for unique Disciplines from DATABASE dekanat view ШвебельПрактикиБезКомпет
        List<String> disciplines_db = new List<String>();

        foreach (DataRow row in table.Rows)
        {
            disciplines_db.Add(row.Field<string>("Дисциплина"));
        }

        return disciplines_db;
    }

    public List<DirectionPreparation> directionPreparations()
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
        DataTable table = requestDb(query_string: query_string);

        // list storage for unique Entity from DATABASE dekanat table Направления подготовки
        List<DirectionPreparation> directions_pre_db = new List<DirectionPreparation>();
        List<String> codes = new List<String>();

        foreach (DataRow row in table.Rows)
        {
            // проверка если такой код уже добавили в хранилище тогда пропустить эту запись
            if (!codes.Contains(row.Field<String>("Код").Trim()))
            {
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

    public List<DepartmentDb> departments()
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
        DataTable table = requestDb(query_string: query_string);

        // list storage for unique Entity from DATABASE dekanat table Кафедры
        List<DepartmentDb> departments_db = new List<DepartmentDb>();

        foreach (DataRow row in table.Rows)
        {
            departments_db.Add(
                new DepartmentDb()
                {
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

        // test block data which isn't in Elma server
        // departments_db.AddRange(new List<DepartmentDb> {
        //     new DepartmentDb
        //     {
        //         Code = "10.10.01",
        //         NameLong = "test1Long",
        //         NameShort = "test1",
        //         FacultyShort = "ИСУ",
        //         FacultyLong = "Информационные системы в управлении ",
        //     },

        //     new DepartmentDb
        //     {
        //         Code = "20.20.02",
        //         NameLong = "test2Long",
        //         NameShort = "test2",
        //         FacultyShort = "ИСУ",
        //     },

        //     new DepartmentDb
        //     {
        //         Code = "30.30.03",
        //         NameLong = "test3Long",
        //         NameShort = "test3",
        //         FacultyLong = "Информационные системы в управлении",
        //     },

        //     new DepartmentDb
        //     {
        //         Code = "40.40.04",
        //         NameLong = "test4Long",
        //         NameShort = "test4",
        //     }
        // });

        return departments_db;
    }

    public List<PrepProfileDB> preparationProfiles()
    {
        // строка запроса к БД для выборки уникальных значений "кафедры"
        string queryStr = @"
            select
                distinct trim(Expr2) as 'Профиль',
                trim(Шифр) as 'КодНаправленияПодготовки'
            from dbo.ШвебельГруппы
            where trim(Шифр) <> '' AND trim(Шифр) IS NOT null;";

        // Получение данных из БД по запросу sql
        DataTable table = requestDb(query_string: queryStr);

        // list storage for unique Entity from DATABASE dekanat table Кафедры
        List<PrepProfileDB> profilesDB = new List<PrepProfileDB>();

        foreach (DataRow row in table.Rows)
        {
            // получение атрибутов из БД по столбцу
            string nameProfile = row.Field<String>("Профиль");
            string codeDirectPrep = row.Field<String>("КодНаправленияПодготовки");

            // добавление в хранилище
            profilesDB.Add(new PrepProfileDB(nameProfile, codeDirectPrep));
        }

        return profilesDB;
    }

    public void groups()
    {
        // строка запроса к БД для выборки уникальных значений "кафедры"
        string query_string = @"select
	                    distinct dbo.ШвебельГруппы.Название as 'НазваниеГруппы',
						dbo.ШвебельГруппы.Код as 'Код',
	                    trim(dbo.Факультеты.Сокращение) as 'ФакультетСокращенное',
	                    dbo.ШвебельГруппы.Шифр as 'НаправленияПодготовки',
	                    dbo.ШвебельГруппы.КодПрофКафедры as 'КодВыпускающейКафедры',
	                    trim(dbo.ШвебельГруппы.Expr2) as 'ПрофильПодготовки',
	                    dbo.ШвебельГруппы.Курс as 'Курс',
	                    dbo.ШвебельГруппы.Учебный_План as 'УчебныйПлан',
	                    dbo.ШвебельГруппы.ФормаОбучения as 'ФормаОбучения'
                    from dbo.ШвебельГруппы
                    join dbo.Факультеты on dbo.Факультеты.Факультет = dbo.ШвебельГруппы.Факультет
					order by dbo.ШвебельГруппы.Название, 'Курс' desc;";

        // Получение данных из БД по запросу sql
        DataTable table = requestDb(query_string: query_string);

        // list storage for unique Entity from DATABASE dekanat table Группы
        List<GroupFromDB> groups_db = new List<GroupFromDB>();
        // list unique group's names
        List<string> uniqueGroupsNames = new List<string>();


        foreach (DataRow row in table.Rows)
        {
            if (!uniqueGroupsNames.Contains(row.Field<String>("НазваниеГруппы")))
            {
                groups_db.Add(new GroupFromDB()
                {
                    NameGroup = row.Field<string>("НазваниеГруппы"),
                    Code = row.Field<string>("Код")
                });

                uniqueGroupsNames.Add(row.Field<String>("НазваниеГруппы"));
            };

        }

        // public string NameGroup { get; set; } // название группы
        // public string Code { get; set; } // код
        // public string FacultyShort { get; set; } // факульетет сокращенно
        // public string DirectionPreparation { get; set; } // направление подготовки
        // public string CodeKafedy { get; set; } // код кафедры
        // public string ProfilePreparation { get; set; } // профиль подготовки
        // public string Kurs { get; set; } // курс
        // public string EducationalPlan { get; set; } // учебный план
        // public string FormStudy { get; set; } // форма обучения
    }

}