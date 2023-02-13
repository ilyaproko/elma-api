using System.Collections.Generic;

namespace ELMA_API
{

    public class CellResponseEduPlan
    {
        public object Data { get; set; }
        public List<object> DataArray { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
    }

    public class RowResponseEduPlan
    {
        public List<CellResponseEduPlan> Items { get; set; }
        public object Value { get; set; }
    }

    public class JsonPlans
    {
        public List<string> educational_plans_upload { get; set; }
        public List<string> educational_plans_db { get; set; }
        public List<string> educational_plans_elma { get; set; }
    }

    public class FacultyGuide
    {
        public string long_name { get; set; }
        public string short_name { get; set; }
    }

    public class DataFaculty
    {
        public List<CellResponseFaculty> Items { get; set; }
        public object Value { get; set; }
    }

    public class CellResponseFaculty
    {
        public DataFaculty Data { get; set; }
        public List<object> DataArray { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
    }

    public class RowResponseFaculty
    {
        public List<CellResponseFaculty> Items { get; set; }
        public object Value { get; set; }
    }

    public class DataDiscipline
    {
        public object Data { get; set; }
        public List<object> DataArray { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
    }

    public class Discipline
    {
        public List<DataDiscipline> Items { get; set; }
        public object Value { get; set; }
    }

    public class DirectionPreparation
    {
        public string Kod { get; set; }
        public string Naimenovanie { get; set; }
    }

    
    public class DataDirectionsPre
    {
        public object Data { get; set; }
        public List<object> DataArray { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
    }

    public class DirectionsPre
    {
        public List<DataDirectionsPre> Items { get; set; }
        public object Value { get; set; }
    }

    public class DataDepartment
    {
        public List<ItemDepartment> Items { get; set; }
        public object Value { get; set; }
    }

    public class ItemDepartment
    {
        public DataDepartment Data { get; set; }
        public List<object> DataArray { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
    }

    public class Department
    {
        public List<ItemDepartment> Items { get; set; }
        public object Value { get; set; }
    }

    public class DepartmentFromDB 
    {
        public string Code { get; set; } // КодКафедры
        public string NameLong { get; set; } // НазваниеКафедры
        public string NameShort { get; set; } // КафедраСокращенно
        public string FacultyShort { get; set; } // ФакультетСокращенно
        public string FacultyLong { get; set; } // Факультет
        public string Room { get; set; } // АудиторияКафедры
        public string Phone { get; set; } // ТелефонКафедры
        public string HeadOfDepartment { get; set; } // заведущий кафедрой
    }


    public class GroupFromDB 
    {
        public string NameGroup { get; set; } // название группы
        public string Code { get; set; } // код
        public string FacultyShort { get; set; } // факульетет сокращенно
        public string DirectionPreparation { get; set; } // направление подготовки
        public string CodeKafedy { get; set; } // код кафедры
        public string ProfilePreparation { get; set; } // профиль подготовки
        public string Kurs { get; set; } // курс
        public string EducationalPlan { get; set; } // учебный план
        public string FormStudy { get; set; } // форма обучения
    }


    // ! Main Structure of Elma Object in Server ! // // // // // // // // // // // // // // // // // // // //
    public class Data
    {
        public List<Item> Items { get; set; }
        public object Value { get; set; }
    }

    public class Item
    {
        public Data Data { get; set; }
        public List<object> DataArray { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
    }

    public class Root
    {
        public List<Item> Items { get; set; }
        public object Value { get; set; }
    }

    // ! END ! // // // // // // // // // // // // // // // // // // // //

}

