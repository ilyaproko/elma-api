using System;
using System.Collections.Generic;

namespace ELMA_API
{
    /// <summary>
    /// Предоставляет дополнительные возмжности работы с Elma,
    /// например подсчет кол-во студентов для каждой группы
    /// </summary>
    class PossibilitiesElma
    {
        RequestElma reqElma; // зависимость для получения данных от сервера Elma
        public PossibilitiesElma(RequestElma reqElma) {
            this.reqElma = reqElma;
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
                var idGroup = reqElma.getValueItem(group.Items, "Id");

                int countStudents = 0;
                students.ForEach(student =>
                {
                    var idGroupInStudent = reqElma.getValueItem(student.Items, "Gruppa", "Id");
                    if (idGroupInStudent == idGroup)
                        countStudents++;
                });

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
                return int.Parse(reqElma.getValueItem(group.Items, "HowManyStudents")) == 0;
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
                var gruppa = reqElma.getValueItem(stud.Items, "Gruppa", "Id");

                return gruppa == null ? true : false;
            });
        }
    }
}