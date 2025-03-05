using FastMember;
using SpendingAndSavingsMonthlyTracker.Models.ProcessedModels;
using Syncfusion.XlsIO;
using System.Data;

namespace SpendingAndSavingsMonthlyTracker
{
    internal class XlsxDataInsertion
    {
        private static void InsertDataIntoSheet<T>(IWorksheet worksheet, List<T> recordsToInsert)
        {
            DataTable dataTable = new DataTable();
            using (var reader = ObjectReader.Create(recordsToInsert))
            {
                dataTable.Load(reader);
            }

            worksheet.ImportDataTable(dataTable, true, 1, 1);
        }

        public static void PopulateTemplate(string templateFilePath,
            List<SavingsOverTime> savingsOverTime,
            List<SpendingOverTime> spendingOverTime,
            List<SpendingThisPeriod> spendingThisPeriod,
            string outputFilePath)
        {
            var inputStream = new FileStream(templateFilePath, FileMode.Open, FileAccess.Read);
            var outputFileStream = new FileStream(outputFilePath, FileMode.Create);

            using (ExcelEngine excelEngine = new ExcelEngine())
            {
                IApplication application = excelEngine.Excel;
                application.DefaultVersion = ExcelVersion.Excel2016;

                IWorkbook workbook = application.Workbooks.Open(inputStream);

                var savingsOverTimeWorksheet = workbook.Worksheets.Single(x => x.Name.Equals("Savings Over Time"));
                var spendingOverTimeWorksheet = workbook.Worksheets.Single(x => x.Name.Equals("Spending Over Time"));
                var spendingThisPeriodWorksheet = workbook.Worksheets.Single(x => x.Name.Equals("Spending This Period"));

                InsertDataIntoSheet(savingsOverTimeWorksheet, savingsOverTime);
                InsertDataIntoSheet(spendingOverTimeWorksheet, spendingOverTime);
                InsertDataIntoSheet(spendingThisPeriodWorksheet, spendingThisPeriod);

                workbook.SaveAs(outputFileStream);
            }
        }
    }
}
