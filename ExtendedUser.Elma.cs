using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Spectre.Console;

namespace ELMA_API;

/// <summary>
/// Предоставляет дополнительные пользовательские возмжности работы с Elma
/// </summary>
class ExtendedUserElma
{
    ElmaApi elmaApi; // зависимость для получения данных от сервера Elma
    BaseHttp baseHttp; // дает возможность совершать запросы для изменений на сервере elma
    public ExtendedUserElma(ElmaApi elmaApi, BaseHttp baseHttp)
    {
        this.elmaApi = elmaApi;
        this.baseHttp = baseHttp;
    }


    /// <summary>
    /// for every group search its students and practices
    /// </summary>
    public List<GroupStudentsPractices> groupStudentsPractices()
    {
        // get all groups in elma server
        var groups = elmaApi.queryEntityList(type: TypesUidElma.groups);
        // get all students in elma server
        var students = elmaApi.queryEntityList(type: TypesUidElma.students);
        // get all practices in elma server
        var practices = elmaApi.queryEntityList(type: TypesUidElma.practices);

        List<GroupStudentsPractices> listGroupStudentsPractices = new List<GroupStudentsPractices>();

        // loop for every group
        foreach (var group in groups)
        {
            // get id current group
            var groupId = ElmaApi.getValueItem(group.Items, "Id");

            // find all students for this current group
            List<Root> studentsGroup = students.FindAll(student =>
                ElmaApi.getValueItem(student.Items, "Gruppa", "Id") == groupId ? true : false
            );

            // find all practices for this current group
            List<Root> practicesGroup = practices.FindAll(practice =>
                ElmaApi.getValueItem(practice.Items, "Gruppa", "Id") == groupId ? true : false
            );

            // add assembled structure for every group: group/studetns/practices
            listGroupStudentsPractices.Add(
                new GroupStudentsPractices()
                {
                    Group = group,
                    Students = studentsGroup,
                    Practices = practicesGroup
                }
            );
        }

        return listGroupStudentsPractices;
    }


    /// <summary>
    /// Обновляет Item "Naimenovanie" объекта справочника в Elma, добавляя
    /// в наименование фразу по которой можно будет в дальнейшем идентифировать
    /// объект-справочник который нужно удалить
    /// </summary>
    /// <returns></returns>
    public void deleteGroupsWithoutStudents()
    {

        Log.Info(InfoTitle.operateElma, "delete groups without students", colorTitle: "orangered1");


        // хранилище ответов от сервера elma при запросах
        List<string> responsesElma = new List<string>();

        var groupsWithoutStudents = groupStudentsPractices()
            .FindAll(assemble => assemble.Students.Count == 0)
            .Select(assemble => assemble.Group).ToList();

        groupsWithoutStudents.GetRange(0, 1).ForEach(group =>
        {
            var nameGroup = ElmaApi.getValueItem(group.Items, "Naimenovanie");
            var idGroup = ElmaApi.getValueItem(group.Items, "Id");

            // проверка не добавлена ли данная группа в процесс подготовки к удалению
            // т.е. если уже для данной группы в наименование добавлена фраза идентификатор DeleteFor_No_Students
            // если фраза-идентификатор добавлена тогда пропускает данную группу т.к. нет смысла добовлять повторно
            if (nameGroup != null && !nameGroup.Contains("DeleteFor_No_Students"))
            {

                // var reqDelete = this.baseHttp.requestElma(
                //     path: $"/API/REST/Entity/Update/{TypesUidElma.groups}/{idGroup}",
                //     method: HttpMethods.Post,
                //     body: ElmaApi.makeBodyJson<string>(Naimenovanie)
                // );

                var reqAddDeleteName = elmaApi.updateEntity(
                    type: TypesUidElma.groups,
                    idEntity: idGroup,
                    body: ElmaApi.makeBodyJson<string>(
                        ElmaApi.makeItem(
                            name: "Naimenovanie",
                            value: $"DeleteFor_No_Students {nameGroup}",
                            typeUID: TypesUidElma.groups
                        )
                    )
                );

                // добавление ответа в хранилище
                responsesElma.Add(reqAddDeleteName.bodyResponse);

                AnsiConsole.MarkupLine(nameGroup + " " + idGroup);
            }

        });

        // после того как к наименованию групп без студентов добавлен идентификатор DeleteFor_No_Students
        // запускается бизнес-процесс elma "deleteGroupsWithoutStudents" который находит данные группы 
        // по идентификатору в наименовании и удаляет каждую из них
        // var reqDeleteProcess = elmaApi.launchProcess(
        //     body: ElmaApi.makeBodyJson<string>(
        //         ElmaApi.makeItem("ProcessToken", tokenProcessLaunch)
        //     )
        // );
        // Console.WriteLine(reqDeleteProcess.bodyResponse);


        Log.Info(InfoTitle.dataElma, $"groups without students : {groupsWithoutStudents.Count}");

        if (responsesElma.Count != 0)
            Log.Success(SuccessTitle.deleteEntities, $"number of deleted groups without students: {responsesElma.Count}");
    }

    /// <summary>
    /// Обновляет объекты-справочники elma "приложения 3 к договору на практику"
    /// исключительно только для групп у которых есть студенты и практики
    /// </summary>
    /// <param name="patternNameGroup">Паттерн по которму будет осуществляться поиск групп по их наименованию</param>
    public void Update_appendixes_three(string patternNameGroup, StatusContext ctx = null)
    {
        Log.Info(
            InfoTitle.updateEntities,
            System.Reflection.MethodBase.GetCurrentMethod().Name,
            colorTitle: "orangered1"
        );

        // if the method's parameter patterNameGroup get empty string or null
        // then break method's process
        if (String.IsNullOrEmpty(patternNameGroup))
        {
            Log.Warn(WarnTitle.methodProcess, $"patterNameGroup is null or empty string");
            return;
        }

        // find groups which has students and practices
        var assembleds = groupStudentsPractices().FindAll(assemble =>
            assemble.Students.Count != 0 && assemble.Practices.Count != 0
            && ElmaApi.getValueItem(assemble.Group.Items, "Naimenovanie").Contains(patternNameGroup));

        List<string> injectedData = new List<string>(); // info number of added appendixThree document of practice

        // loop every assembled structre of group its students and practices
        foreach (var assembled in assembleds)
        {
            // get id group
            var idGroup = ElmaApi.getValueItem(assembled.Group.Items, "Id");
            // department the group :> Kafedra
            var idDepartment = ElmaApi.getValueItem(assembled.Group.Items, "VypuskayuschayaKafedra", "Id");

            // loop every student in current group
            foreach (var student in assembled.Students)
            {
                // get id student
                var idStudent = ElmaApi.getValueItem(student.Items, "Id");
                // loop every practice in current grupo
                foreach (var practice in assembled.Practices)
                {
                    // get id practice
                    var idDisciplineOfPractice = ElmaApi.getValueItem(practice.Items, "Disciplina", "Id");
                    // get id semester of the practice
                    var semester = ElmaApi.getValueItem(practice.Items, "Semestr");

                    // if current practice doesn't have included discipline (so we can't get id discipline to add to the appendix)
                    // then we skip the iteration
                    if (idDisciplineOfPractice == null) continue;

                    // check for every studens in group if he/she has the same practices in 
                    // appendix three document of practice (elma object "Prilozhenie2KDogovoruNaPraktiku")
                    var searchAppendixesThree = elmaApi.queryEntityList(type: TypesUidElma.appendixThreePracticeDoc,
                        new ParameterEql($"Gruppa = {idGroup} AND Student = {idStudent} AND Disciplina = {idDisciplineOfPractice}")
                    );

                    // if appendixThreePraciseDoc already exist for current student then skip iteration
                    // It's doesn't neccessary create the same object elma
                    if (searchAppendixesThree.Count > 0) continue;

                    // insert new appendix Three practice document
                    var responseInsert = this.elmaApi.insertEntity(
                        type: TypesUidElma.appendixThreePracticeDoc,
                        body: ElmaApi.makeBodyJson<string>(
                            ElmaApi.makeItem("Student", nestedName: "Id", nestedValue: idStudent),
                            ElmaApi.makeItem("Gruppa", nestedName: "Id", nestedValue: idGroup),
                            ElmaApi.makeItem("Disciplina", nestedName: "Id", nestedValue: idDisciplineOfPractice),
                            ElmaApi.makeItem("Kafedra", nestedName: "Id", nestedValue: idDepartment),
                            ElmaApi.makeItem("Semestr", semester)
                        )
                    );

                    // add response from server elma to storage
                    injectedData.Add(responseInsert.bodyResponse);

                    // log status
                    if (ctx != null)
                    {
                        string[] prep = ctx.Status.Split(' ');
                        var isNumeric = int.TryParse(prep[prep.Length - 1], out int n);
                        if (!isNumeric) ctx.Status(String.Join(' ', prep) + ": " + injectedData.Count);
                        else ctx.Status(String.Join(' ', prep.ToList().GetRange(0, prep.Length - 1).ToArray()) + $" {injectedData.Count}");
                    }
                }
            }
        }

        Log.Info(
            InfoTitle.common,
            $"with pattern for group's name: \"{patternNameGroup}\", found groups: {assembleds.Count}"
        );

        if (injectedData.Count == 0)
            Log.Success(
                SuccessTitle.synchronized,
                $"{System.Reflection.MethodBase.GetCurrentMethod().Name} synchronized"
            );
        else
            Log.Success(
                SuccessTitle.injectedData,
                $"injected new appendixes three: {injectedData.Count}"
            );
    }
}