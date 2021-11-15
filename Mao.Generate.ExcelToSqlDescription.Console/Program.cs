using Excel = Microsoft.Office.Interop.Excel;
using Mao.Generate.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mao.Generate.ExcelToSqlDescription.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            App app = new App();
            app.Execute();
        }
    }

    public class App
    {
        /// <summary>
        /// 是否為不能連接至目標資料庫的環境
        /// </summary>
        const bool isRemoteMode = false;
        /// <summary>
        /// 資料交換的目錄位置
        /// <para>isRemoteMode = true 才需要設定</para>
        /// </summary>
        const string outputCommandPath = @"D:\Temp\Command";
        /// <summary>
        /// 需要更新描述的資料庫連接字串
        /// <para>isRemoteMode = false 才需要設定</para>
        /// </summary>
        const string connectionString = @"";
        /// <summary>
        /// 匯入檔案的完整路徑
        /// </summary>
        const string importFilePath = @"D:\Temp\DbSchema_UpdateDescription.xlsx";
        /// <summary>
        /// 如果讀取到的描述是空字串，是否要更新
        /// </summary>
        const bool updateDescriptionOnEmpty = false;

        const string sheet1Name = "清單";
        const string sheet2Name = "系統主表";

        private readonly SqlService _sqlService;
        public App()
        {
            _sqlService = isRemoteMode ?
                new RemoteSqlService()
                {
                    TempPath = outputCommandPath
                } :
                new SqlService();
        }

        public void Execute()
        {
            Excel.Application application = new Excel.Application();
            Excel.Workbook workbook = application.Workbooks.Open(importFilePath);
            try
            {
                Excel.Worksheet worksheet;
                // 清單
                worksheet = workbook.Worksheets[sheet1Name];
                ResolveSheet1(worksheet);
                // 系統主表
                worksheet = workbook.Worksheets[sheet2Name];
                ResolveSheet2(worksheet);
            }
            finally
            {
                workbook.Close();
                application.Quit();
            }
        }

        /// <summary>
        /// 逐行由左至右尋找第一個指定文字內容的欄位位置
        /// </summary>
        private (int RowNumber, int ColNumber) FindCell(Excel.Worksheet worksheet, string text, int startRowNumber = 1, int startColNumber = 1, int? endRowNumber = null, int? endColNumber = null)
        {
            for (int rowNumber = startRowNumber; rowNumber <= (endRowNumber ?? worksheet.UsedRange.Rows.Count); rowNumber++)
            {
                for (int colNumber = startColNumber; colNumber <= (endColNumber ?? worksheet.UsedRange.Columns.Count); colNumber++)
                {
                    if (string.Equals(
                        (Convert.ToString(worksheet.Cells[rowNumber, colNumber].Value) ?? "").Trim(),
                        (text ?? "").Trim(), StringComparison.OrdinalIgnoreCase))
                    {
                        return (rowNumber, colNumber);
                    }
                }
            }
            return (0, 0);
        }
        /// <summary>
        /// 從第 1 個 Sheet (清單) 取得資料表、檢視、預存程序的描述進行更新
        /// </summary>
        private void ResolveSheet1(Excel.Worksheet worksheet)
        {
            int rowNumber, colNumber;
            int horizontal = 1;
            // 資料表
            (rowNumber, colNumber) = FindCell(worksheet, "Table Name", horizontal);
            if (rowNumber != 0)
            {
                rowNumber++;
                while (true)
                {
                    string tableName = Convert.ToString(worksheet.Cells[rowNumber, colNumber].Value);
                    if (string.IsNullOrWhiteSpace(tableName))
                    {
                        break;
                    }
                    string description = Convert.ToString(worksheet.Cells[rowNumber, colNumber + 1].Value);
                    if (!string.IsNullOrWhiteSpace(description) || updateDescriptionOnEmpty)
                    {
                        _sqlService.UpdateTableDescription(connectionString, tableName, description);
                    }
                    rowNumber++;
                }
                horizontal = rowNumber;
            }
            // 檢視
            (rowNumber, colNumber) = FindCell(worksheet, "View Name", horizontal);
            if (rowNumber != 0)
            {
                rowNumber++;
                while (true)
                {
                    string viewName = Convert.ToString(worksheet.Cells[rowNumber, colNumber].Value);
                    if (string.IsNullOrWhiteSpace(viewName))
                    {
                        break;
                    }
                    string description = Convert.ToString(worksheet.Cells[rowNumber, colNumber + 1].Value);
                    if (!string.IsNullOrWhiteSpace(description) || updateDescriptionOnEmpty)
                    {
                        _sqlService.UpdateViewDescription(connectionString, viewName, description);
                    }
                    rowNumber++;
                }
                horizontal = rowNumber;
            }
            // 預存程序
            (rowNumber, colNumber) = FindCell(worksheet, "Procedure Name", horizontal);
            if (rowNumber != 0)
            {
                rowNumber++;
                while (true)
                {
                    string procedureName = Convert.ToString(worksheet.Cells[rowNumber, colNumber].Value);
                    if (string.IsNullOrWhiteSpace(procedureName))
                    {
                        break;
                    }
                    string description = Convert.ToString(worksheet.Cells[rowNumber, colNumber + 1].Value);
                    if (!string.IsNullOrWhiteSpace(description) || updateDescriptionOnEmpty)
                    {
                        _sqlService.UpdateProcedureDescription(connectionString, procedureName, description);
                    }
                    rowNumber++;
                }
                horizontal = rowNumber;
            }
        }
        /// <summary>
        /// 從第 2 個 Sheet (系統主表) 取得資料表欄位的描述進行更新
        /// </summary>
        private void ResolveSheet2(Excel.Worksheet worksheet)
        {
            int rowNumber, tableNameColNumber, columnNameColNumber, descriptionColNumber;
            Excel.Range range;
            rowNumber = 1;
            while (true)
            {
                // 找到資料表名稱在 Excel 的第幾欄
                (rowNumber, tableNameColNumber) = FindCell(worksheet, "Table Name", rowNumber);
                if (rowNumber == 0 || tableNameColNumber == 0)
                {
                    break;
                }
                // 找到資料表欄位名稱在 Excel 的第幾欄
                (_, columnNameColNumber) = FindCell(worksheet, "Column", rowNumber, endRowNumber: rowNumber);
                if (columnNameColNumber == 0)
                {
                    break;
                }
                // 找到資料表欄位描述在 Excel 的第幾欄
                (_, descriptionColNumber) = FindCell(worksheet, "Chinese Remark", rowNumber, endRowNumber: rowNumber);
                if (descriptionColNumber == 0)
                {
                    break;
                }
                rowNumber++;
                // 取得資料表名稱的欄位
                range = worksheet.Cells[rowNumber, tableNameColNumber];
                string tableName = Convert.ToString(range.Value);
                // 根據資料表名稱欄位的 rowspan 來讀取資料表欄位
                for (int i = 0; i < range.MergeArea.Rows.Count; i++)
                {
                    string columnName = Convert.ToString(worksheet.Cells[rowNumber, columnNameColNumber].Value);
                    string description = Convert.ToString(worksheet.Cells[rowNumber, descriptionColNumber].Value);
                    if (!string.IsNullOrWhiteSpace(description) || updateDescriptionOnEmpty)
                    {
                        _sqlService.UpdateColumnDescription(connectionString, tableName, columnName, description);
                    }
                    rowNumber++;
                }
                rowNumber++;
            }
        }
    }
}
