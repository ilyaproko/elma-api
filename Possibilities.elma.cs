using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Spectre.Console;

namespace ELMA_API
{
    /// <summary>
    /// Предоставляет дополнительные возмжности работы с Elma,
    /// например подсчет кол-во студентов для каждой группы
    /// </summary>
    class PossibilitiesElma
    {
        Elma reqElma; // зависимость для получения данных от сервера Elma
        BaseHttp baseHttp; // дает возможность совершать запросы для изменений на сервере elma
        public PossibilitiesElma(Elma reqElma, BaseHttp baseHttp) {
            this.reqElma = reqElma;
            this.baseHttp = baseHttp;
        }

        /// <summary>
        /// Подсчет кол-во студентов для каждой группы
        /// </summary>
        /// <returns>
        /// вернет модифицированный список групп
        /// с доп. Item -> "HowManyStudents", который будет 
        /// указывать кол-во студентов для каждой группы
        /// </returns>
        public List<Root> studentsEveryGroup() {
            var groups = reqElma.groups();
            var students = reqElma.students();

            foreach (var group in groups)
            {
                // id группы
                var idGroup = Elma.getValueItem(group.Items, "Id");

                // кол-во студентов для данной группы (которая в нынешней итерации)
                int countStudents = 0;

                // перебор по всем студентам, каждый раз для каждой группы
                students.ForEach(student =>
                {
                    var idGroupInStudent = Elma.getValueItem(student.Items, "Gruppa", "Id");

                    // если id группы студента и id группы соответствует тогда инкремент
                    if (idGroupInStudent == idGroup)
                        countStudents++;
                });

                // добавление нового Item, с информацией о кол-во студнтов в группе
                group.Items.Add(new Item()
                {
                    Data = null,
                    DataArray = new List<object>(),
                    Name = "HowManyStudents",
                    Value = countStudents.ToString()
                });
            }

            return groups;
        }

        /// <summary>
        /// Поиск групп у которых нет студентов 
        /// </summary>
        /// <returns>вернет группы у которых нет студентов</returns>
        public List<Root> groupsWithoutStudents() {
            return this.studentsEveryGroup().FindAll(group => 
            {
                return int.Parse(Elma.getValueItem(group.Items, "HowManyStudents")) == 0;
            });
        }

        /// <summary>
        /// Поиск студентов у которых не определена группа
        /// </summary>
        /// <returns>вернет список студентов у которых нет группы</returns>
        public List<Root> studentsWithoutGroup() {
            return reqElma.students().FindAll(stud =>
            {
                // если вернет null значит для данного студента 
                // группа не определена
                var gruppa = Elma.getValueItem(stud.Items, "Gruppa", "Id");

                return gruppa == null ? true : false;
            });
        }

        /// <summary>
        /// Обновляет Item "Naimenovanie" объекта справочника в Elma, добавляя
        /// в наименование фразу по которой можно будет в дальнейшем идентифировать
        /// объект-справочник который нужно удалить
        /// </summary>
        /// <returns></returns>
        public bool deleteGroupsWithoutStudents() {

            Log.Info(InfoTitle.operateElma, "delete groups without students", colorTitle: "orangered1");

            // token запуска бизнес - процесса который удаляет группы без студентов на сервере elma
            string tokenProcessLaunch = "81ba4e55-5cb2-422c-9e4d-7bd03208a8c3";

            // хранилище ответов от сервера elma при запросах
            List<string> responsesElma = new List<string>();

            var groupsWithoutStudents = this.groupsWithoutStudents();

            groupsWithoutStudents.GetRange(0, 1).ForEach(group =>
            {
                var nameGroup = Elma.getValueItem(group.Items, "Naimenovanie");
                var idGroup = Elma.getValueItem(group.Items, "Id");

                // проверка не добавлена ли данная группа в процесс подготовки к удалению
                // т.е. если уже для данной группы в наименование добавлена фраза идентификатор DeleteFor_No_Students
                if (nameGroup != null && !nameGroup.Contains("DeleteFor_No_Students")) 
                {

                    AnsiConsole.MarkupLine(nameGroup + " " + idGroup);

                    Item Naimenovanie = new Item()
                    {
                        Data = null, DataArray = new List<object>(),
                        Name = "Naimenovanie", Value = $"DeleteFor_No_Students {nameGroup}"
                    };
                    Root requestBody = new Root()
                    {
                        Items = new List<Item>() {Naimenovanie},
                        Value = ""
                    };

                    var reqDelete = this.baseHttp.request(
                        path: $"/API/REST/Entity/Update/{TypesUidElma.groups}/{idGroup}",
                        method: "POST",
                        body: JsonConvert.SerializeObject(requestBody)
                    );

                    // добавление ответа в хранилище
                    responsesElma.Add(reqDelete.bodyString);
                }

                // после того как к наименованию групп без студентов добавлен идентификатор DeleteFor_No_Students
                // запускается бизнес-процесс elma "deleteGroupsWithoutStudents" который находит данные группы 
                // по идентификатору в наименовании и удаляет каждую из них
                var reqDeleteProcess = this.baseHttp.request(
                    path:  $"/Processes/ProcessHeader/RunByWebQuery/{tokenProcessLaunch}",
                    method: "GET"
                );

                Console.WriteLine(reqDeleteProcess.bodyString);


            });

            Log.Info(InfoTitle.dataElma, $"groups without students : {groupsWithoutStudents.Count}");

            if (responsesElma.Count != 0) 
                Log.Success(SuccessTitle.uploadNewUsers, $"number of deleted groups without students: {responsesElma.Count}");

            return true;
        }
    }
}