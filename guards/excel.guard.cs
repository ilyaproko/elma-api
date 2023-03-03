using Aspose.Cells;

namespace ELMA_API
{
    class ExcelGuard
    {
        /// <summary>
        /// Проверка на присутствие необходимых колонок и
        /// правильность их расположение в excel файле
        /// если присутствуют и положение колонок правильное 
        /// тогда true иначе false
        /// Необходимые колонки и их располжение:
        /// ФИО-полностью | ФИО-сокращенно | УченоеЗвание | Кафедра
        /// </summary>
        static public bool academicTitle(Worksheet ws, string pathExcel)
        {
            var fullName = ws.Cells[0, 0].StringValue.Trim().ToLower();
            var shortName = ws.Cells[0, 1].StringValue.Trim().ToLower();
            var level = ws.Cells[0, 2].StringValue.Trim().ToLower();
            var department = ws.Cells[0, 3].StringValue.Trim().ToLower();
            
            if (fullName != "фио-полностью" || shortName != "фио-сокращенно"
                || level != "ученоезвание" || department != "кафедра") 
            {
                Log.Warn(WarnTitle.columnsExcel, $"excel файл {pathExcel}");
                Log.Notice(NoticeTitle.check, $"колонки в должны быть: фио-полностью | фио-сокращенно | ученоезвание | кафедра");
                Log.Notice(NoticeTitle.check, $"колонки в excel файле: {fullName} | {shortName} | {level} | {department}");
                Log.Notice(NoticeTitle.check, "проверьте расположение колонок и соответствие наименования");
                return false;
            }

            return true;
        }
    }

}