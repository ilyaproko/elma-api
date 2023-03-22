using System.Collections.Generic;

namespace ELMA_API;

// ! Main Structure of Elma Object in Server ! // // // // // // // // // // // // // // // // // // // //
public class Data
{
    public List<Item> Items { get; set; }
    public object Value { get; set; }
}

public class Item
{
    public Data Data { get; set; }
    public List<Data> DataArray { get; set; }
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

public class DepartmentDb
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

public class PrepProfileElma
{
    public string codeDirectPrep; // шифр направления подготовки для Даннного Профеля
    public string idDirectPrep; // ID направления подготовки для Данного Профеля
    public string name; // наименвоание профеля подготовки

    public PrepProfileElma(string name, string idDirectPrep, string codeDirectPrep)
    {
        this.name = name;
        this.idDirectPrep = idDirectPrep;
        this.codeDirectPrep = codeDirectPrep;
    }
}

public class PrepProfileDB
{
    public string name; // наименование направления подготовки
    public string codeDirectPrep; // шифр направления подготовки для Даннного Профеля

    public PrepProfileDB(string name, string codeDirectPrep)
    {
        this.name = name;
        this.codeDirectPrep = codeDirectPrep;
    }
}

/// <summary>For every group search its students and practices</summary>
public class GroupStudentsPractices
{
    public Root Group { get; set; }
    public List<Root> Students { get; set; }
    public List<Root> Practices { get; set; }
}

public class UploadResult
{
    /// <summary>Number of certain entities elma before start process of uploding data</summary>
    public int EntitiesElmaBefore { get; set; }
    /// <summary>Number of certain entities elma after uploding data</summary>
    public int EntitiesElmaAfter { get; set; }
    /// <summary>Number of entities from database which may be uploded to elma</summary>
    public int EntitiesForInject { get; set; }
    /// <summary>Number of entities which aren't in server elma but should be</summary>
    public int EntitiesMissed { get; set; }
    /// <summary>Number of entities injected to elma</summary>
    public List<string> EntitiesIdInjected { get; set; }
    public string NameMethodUpload { get; set; }
}