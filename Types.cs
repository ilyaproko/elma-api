using System.Collections.Generic;

namespace ELMA_API
{
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
    
    public class FacultyGuide
    {
        public string longName { get; set; }
        public string shortName { get; set; }
    }

    public class DirectionPreparation
    {
        public string Kod { get; set; }
        public string Naimenovanie { get; set; }
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

    public class PrepProfile
    {
        public string codeDirectPrep; // шифр направления подготовки для Даннного Профеля
        public string idDirectPrep; // ID направления подготовки для Данного Профеля
        public string name; // наименвоание профеля подготовки

        public PrepProfile(string name, string idDirectPrep, string codeDirectPrep)
        {
            this.name = name;
            this.idDirectPrep = idDirectPrep;
            this.codeDirectPrep = codeDirectPrep;
        }
    }

    public class PrepProfileDB 
    {
        public string nameProfile; // наименование направления подготовки
        public string codeDirectPrep; // шифр направления подготовки для Даннного Профеля

        public PrepProfileDB(string name, string codeDirectPrep) {
            this.nameProfile = name;
            this.codeDirectPrep = codeDirectPrep;
        }
    }
}