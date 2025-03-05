using FastMember;
using SpendingAndSavingsMonthlyTracker.Models.ProcessedModels;
using Syncfusion.XlsIO;
using Syncfusion.XlsIO.Implementation.PivotTables;
using System.Data;

namespace SpendingAndSavingsMonthlyTracker
{
    internal class XlsxDataInsertion
    {
        private static void InsertDataIntoSheet<T>(IWorksheet worksheet, List<T> recordsToInsert)
        {
            int nextRow = 2;

            foreach (var record in recordsToInsert)
            {
                worksheet.InsertRow(nextRow, 1, ExcelInsertOptions.FormatAsBefore);

                var dataTable = new DataTable();

                using (var reader = ObjectReader.Create([record]))
            {
                dataTable.Load(reader);
            }

                worksheet.ImportDataTable(dataTable, false, nextRow, 1);
                nextRow++;
            }

            worksheet.DeleteRow(nextRow);
        }

        private static void RefreshPivotTables(IWorkbook workbook)
        {
            foreach(var worksheet in workbook.Worksheets)
            {
                for (int i = 0; i < worksheet.PivotTables.Count; i++)
                {
                    var pivotTable = worksheet.PivotTables[i] as PivotTableImpl;
                    pivotTable!.Cache.IsRefreshOnLoad = true;
                }
            }
        }

        public static void PopulateTemplate(string templateFilePath,
            List<SavingsOverTime> savingsOverTime,
            List<SpendingOverTime> spendingOverTime,
            List<SpendingThisPeriod> spendingThisPeriod,
            List<ISAUsage> isaUsage,
            string outputFilePath)
        {
            var inputStream = new FileStream(templateFilePath, FileMode.Open, FileAccess.Read);
            var outputFileStream = new FileStream(outputFilePath, FileMode.Create);

            using (var excelEngine = new ExcelEngine())
            {
                IApplication application = excelEngine.Excel;
                application.DefaultVersion = ExcelVersion.Excel2016;

                IWorkbook workbook = application.Workbooks.Open(inputStream);

                var savingsOverTimeWorksheet = workbook.Worksheets.Single(x => x.Name.Equals("Savings Over Time"));
                var spendingOverTimeWorksheet = workbook.Worksheets.Single(x => x.Name.Equals("Spending Over Time"));
                var spendingThisPeriodWorksheet = workbook.Worksheets.Single(x => x.Name.Equals("Spending This Period"));
                var isaUsageWorksheet = workbook.Worksheets.Single(x => x.Name.Equals("ISA Usage"));

                InsertDataIntoSheet(savingsOverTimeWorksheet, savingsOverTime);
                InsertDataIntoSheet(spendingOverTimeWorksheet, spendingOverTime);
                InsertDataIntoSheet(spendingThisPeriodWorksheet, spendingThisPeriod);
                InsertDataIntoSheet(isaUsageWorksheet, isaUsage);

                RefreshPivotTables(workbook);

                workbook.SaveAs(outputFileStream);
            }
        }
    }
}
