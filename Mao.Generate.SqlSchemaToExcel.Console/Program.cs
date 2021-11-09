using Excel = Microsoft.Office.Interop.Excel;
using Mao.Generate.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mao.Generate.Core.Services;

namespace Mao.Generate.SqlSchemaToExcel.Console
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
        /// 要匯出資料結構的資料庫的的連接字串
        /// </summary>
        const string connectionString = @"";
        /// <summary>
        /// 匯出檔案要儲存的位置及名稱
        /// </summary>
        const string exportFilePathFormat = @"D:\Temp\DbSchema-{0:yyyy-MM-dd HH_mm_ss}.xlsx";

        const string sheet1Name = "清單";
        const string sheet2Name = "系統主表";

        private readonly SqlService _sqlService;
        public App()
        {
            _sqlService = new SqlService();
        }

        private SqlTable[] sqlTables;
        private string[] sqlProcedureNames;
        /// <summary>
        /// 預先載入資料
        /// </summary>
        protected void Preloading()
        {
            sqlTables = _sqlService.GetTableNames(connectionString)
                .Select(x => _sqlService.GetSqlTable(connectionString, x))
                .ToArray();
            sqlProcedureNames = _sqlService.GetProcedureNames(connectionString);
        }
        /// <summary>
        /// 主程序
        /// </summary>
        public void Execute()
        {
            Preloading();

            Excel.Application application = new Excel.Application();
            Excel.Workbook workbook = application.Workbooks.Add();
            try
            {
                // 把新增的 Excel 刪除 Sheet 直到剩一個 (Windows 限制最少要剩一個)
                while (workbook.Worksheets.Count > 1)
                {
                    workbook.Worksheets[2].Delete();
                }
                Excel.Worksheet defaultWorksheet = workbook.Worksheets[1];
                {
                    // 清單
                    Excel.Worksheet worksheet = workbook.Worksheets.Add(After: workbook.Sheets[workbook.Sheets.Count]);
                    FillSheet1(worksheet);
                }
                {
                    // 系統主表
                    Excel.Worksheet worksheet = workbook.Worksheets.Add(After: workbook.Sheets[workbook.Sheets.Count]);
                    FillSheet2(worksheet);
                }
                // 把原先僅剩的 Sheet 移除
                defaultWorksheet.Delete();
                // 儲存成檔案
                workbook.SaveAs(string.Format(exportFilePathFormat, DateTime.Now));
            }
            finally
            {
                workbook.Close();
                application.Quit();
            }
        }
        /// <summary>
        /// 第 1 個 Sheet 的內容 (清單)
        /// </summary>
        private void FillSheet1(Excel.Worksheet worksheet)
        {
            worksheet.Name = sheet1Name;

            int rowIndex = 1;
            Excel.Range range;

            // 資料表區塊
            range = worksheet.Range[$"A{rowIndex}", $"C{rowIndex + sqlTables.Length}"];
            // 資料表區塊 框線
            range.Cells.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
            // 資料表區塊 粗外框線
            range.BorderAround(Type.Missing, (Excel.XlBorderWeight)3, Excel.XlColorIndex.xlColorIndexAutomatic, Type.Missing);

            // 資料表區塊 字體
            range.Font.Name = "微軟正黑體";
            range.Font.Size = 11;
            range.Font.Bold = true;
            // 資料表區塊 標題列
            worksheet.Cells[rowIndex, 2] = "Table Name";
            worksheet.Cells[rowIndex, 3] = "Description";
            range = worksheet.Range[$"A{rowIndex}", $"C{rowIndex}"];
            // 標題列置中
            range.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
            // 標題列文字顏色
            range.Font.Color = RGB(155, 0, 128);
            // 標題列背景色
            range.Interior.Color = RGB(204, 255, 204);
            rowIndex++;
            // 資料表名稱 列表
            int num = 1;
            int linkIndex = 1;
            foreach (var sqlTable in sqlTables)
            {
                // 設定列高
                range = worksheet.Range[$"A{rowIndex}"];
                range.RowHeight = 20.1;
                // 行數
                worksheet.Cells[rowIndex, 1] = num;
                // 資料表名稱 連結
                Excel.Hyperlink hyperlink = worksheet.Hyperlinks.Add(
                    worksheet.Cells[rowIndex, 2],
                    // 連結目標在同一個檔案內不用給值
                    "",
                    // 連結位置 (+2 = 回清單 + 標題列)
                    $"{sheet2Name}!B{linkIndex + 2}:B{linkIndex + 2 + sqlTable.Columns.Length - 1}",
                    // 連結文字 (資料表名稱)
                    TextToDisplay: sqlTable.Name);
                // 設定連結字體會跑掉，需要重設
                range = hyperlink.Range;
                range.Font.Name = "微軟正黑體";
                range.Font.Bold = true;
                // 資料表名稱 文字顏色
                range.Font.Color = RGB(0, 112, 192);
                // 資料表描述
                worksheet.Cells[rowIndex, 3] = sqlTable.Description;
                rowIndex++;

                num++;
                // (+5 = 回清單 + 標題列 + 資料表說明行 + 建立索引語法行 + 間隔)
                linkIndex += sqlTable.Columns.Length + 5;
            }

            // 資料表跟預存程序中間空一行
            rowIndex++;

            // 預存程序區塊
            range = worksheet.Range[$"A{rowIndex}", $"C{rowIndex + sqlProcedureNames.Length}"];
            // 預存程序區塊 框線
            range.Cells.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
            // 預存程序區塊 粗外框線
            range.BorderAround(Type.Missing, (Excel.XlBorderWeight)3, Excel.XlColorIndex.xlColorIndexAutomatic, Type.Missing);
            // 預存程序區塊 字體
            range.Font.Name = "微軟正黑體";
            range.Font.Size = 11;
            range.Font.Bold = true;
            // 預存程序區塊 標題列
            worksheet.Cells[rowIndex, 2] = "Procedure Name";
            worksheet.Cells[rowIndex, 3] = "Description";
            range = worksheet.Range[$"A{rowIndex}", $"C{rowIndex}"];
            // 標題列置中
            range.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
            // 標題列文字顏色
            range.Font.Color = RGB(155, 0, 128);
            // 標題列背景色
            range.Interior.Color = RGB(204, 255, 204);
            rowIndex++;
            // 預存程序名稱 列表
            num = 1;
            foreach (var sqlProcedureName in sqlProcedureNames)
            {
                // 設定列高
                range = worksheet.Range[$"A{rowIndex}"];
                range.RowHeight = 20.1;
                // 行數
                worksheet.Cells[rowIndex, 1] = num;
                worksheet.Cells[rowIndex, 2] = sqlProcedureName;
                // 預存程序的說明，有地方取得嗎
                worksheet.Cells[rowIndex, 3] = "";
                rowIndex++;
                num++;
            }

            worksheet.Columns.AutoFit();
        }
        /// <summary>
        /// 第 2 個 Sheet 的內容 (系統主表)
        /// </summary>
        private void FillSheet2(Excel.Worksheet worksheet)
        {
            worksheet.Name = sheet2Name;

            int rowIndex = 1;
            Excel.Range range;

            int linkIndex = 1;
            foreach (var sqlTable in sqlTables)
            {
                worksheet.Cells[rowIndex, 2] = "回清單";
                // 回清單 連結
                Excel.Hyperlink hyperlink = worksheet.Hyperlinks.Add(
                    worksheet.Cells[rowIndex, 2],
                    // 連結目標在同一個檔案內不用給值
                    "",
                    // 連結位置 (+1 = 標題列)
                    $"{sheet1Name}!B{linkIndex + 1}",
                    // 連結文字 (資料表名稱)
                    TextToDisplay: "回清單");
                // 設定連結字體會跑掉，需要重設
                range = hyperlink.Range;
                range.Font.Name = "微軟正黑體";
                range.Font.Bold = true;
                // 回清單 框線
                range.Cells.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
                rowIndex++;

                // 資料表區塊 (+2 = 資料表說明行 + 建立索引語法行)
                range = worksheet.Range[$"B{rowIndex}", $"G{rowIndex + sqlTable.Columns.Length + 2}"];
                // 資料表區塊 框線
                range.Cells.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
                // 資料表區塊 外框線
                range.BorderAround(Type.Missing, Excel.XlBorderWeight.xlThin, Excel.XlColorIndex.xlColorIndexAutomatic, Type.Missing);
                // 資料表區塊 字體
                range.Font.Name = "微軟正黑體";
                range.Font.Size = 10;
                range.Font.Bold = true;

                worksheet.Cells[rowIndex, 2] = "Table Name";
                worksheet.Cells[rowIndex, 3] = "Column";
                worksheet.Cells[rowIndex, 4] = "Data_type";
                worksheet.Cells[rowIndex, 5] = "null";
                worksheet.Cells[rowIndex, 6] = "Chinese Remark";
                worksheet.Cells[rowIndex, 7] = "SAMPLE";
                range = worksheet.Range[$"B{rowIndex}", $"G{rowIndex}"];
                // 標題列置中
                range.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                // 標題列文字顏色
                range.Font.Color = RGB(155, 0, 128);
                // 標題列背景色
                range.Interior.Color = RGB(204, 255, 204);
                rowIndex++;

                // 資料表名稱
                worksheet.Cells[rowIndex, 2] = sqlTable.Name;
                // 資料表名稱置中
                range = worksheet.Range[$"B{rowIndex}", $"B{rowIndex + sqlTable.Columns.Length - 1}"];
                range.Merge();
                range.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                // null 欄位置中
                range = worksheet.Cells.Range[$"E{rowIndex}", $"E{rowIndex + sqlTable.Columns.Length - 1}"];
                range.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;

                // 資料表欄位
                foreach (var sqlColumn in sqlTable.Columns)
                {
                    worksheet.Cells[rowIndex, 3] = sqlColumn.Name;
                    if (sqlColumn.IsPrimaryKey)
                    {
                        // 主索引鍵背景色
                        worksheet.Cells.Range[$"C{rowIndex}", $"C{rowIndex}"].Interior.Color = RGB(255, 255, 153);
                    }
                    worksheet.Cells[rowIndex, 4] = sqlColumn.TypeFullName;
                    worksheet.Cells[rowIndex, 5] = sqlColumn.IsNullable ? "O" : "X";
                    worksheet.Cells[rowIndex, 6] = sqlColumn.Description;
                    // 把預設值放到 SAMPLE 欄位
                    if (!string.IsNullOrWhiteSpace(sqlColumn.DefaultDefine))
                    {
                        worksheet.Cells[rowIndex, 7] = $"Default value = {sqlColumn.DefaultDefine}";
                    }
                    rowIndex++;
                }

                // 資料表說明
                worksheet.Cells[rowIndex, 2] = "資料表說明";
                worksheet.Cells[rowIndex, 3] = sqlTable.Description;
                range = worksheet.Range[$"C{rowIndex}", $"G{rowIndex}"];
                range.Merge();
                rowIndex++;

                // 資料表索引建立語法
                // TODO:
                worksheet.Cells[rowIndex, 2] = "";
                range = worksheet.Range[$"B{rowIndex}", $"G{rowIndex}"];
                range.Merge();
                rowIndex++;

                // 資料表之間空一行
                rowIndex++;

                linkIndex++;
            }

            worksheet.Columns.AutoFit();
        }

        private int RGB(int r, int g, int b)
        {
            return r + g * 256 + b * 256 * 256;
        }
    }
}
