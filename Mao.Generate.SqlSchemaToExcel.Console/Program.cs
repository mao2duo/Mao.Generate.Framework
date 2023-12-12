using Excel = Microsoft.Office.Interop.Excel;
using Mao.Generate.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        /// 是否為不能連接至目標資料庫的環境
        /// </summary>
        const bool isRemoteMode = false;
        /// <summary>
        /// 資料交換的目錄位置
        /// <para><see cref="isRemoteMode"/> = true 才需要設定</para>
        /// </summary>
        const string remoteDataExchangePath = @"D:\Temp\Exchange";
        /// <summary>
        /// 要匯出資料結構的資料庫的的連接字串
        /// <para><see cref="isRemoteMode"/> = false 才需要設定</para>
        /// </summary>
        const string connectionString = @"";
        /// <summary>
        /// 匯出檔案要儲存的位置及名稱
        /// </summary>
        const string exportFilePathFormat = @"D:\Temp\DbSchema-{0:yyyy-MM-dd HH_mm_ss}.xlsx";

        const string sheet1Name = "清單";
        const string sheet2Name = "系統主表";
        const string sheet3Name = "物件關聯";

        private readonly SqlService _sqlService;
        public App()
        {
            _sqlService = isRemoteMode ?
                new RemoteSqlService()
                {
                    TempPath = remoteDataExchangePath,
                    Awaiter = message =>
                    {
                        System.Console.WriteLine(message);
                        System.Console.Write("儲存完成後按 Y 載入資料；或按 ESC 不載入資料繼續：");
                        while (true)
                        {
                            var consoleRead = System.Console.ReadKey();
                            if (consoleRead.Key == ConsoleKey.Y)
                            {
                                System.Console.WriteLine();
                                return true;
                            }
                            if (consoleRead.Key == ConsoleKey.Escape)
                            {
                                System.Console.WriteLine();
                                return false;
                            }
                        }
                    }
                } :
                new SqlService();
        }

        private string databaseName;
        private SqlTable[] sqlTables;
        private SqlView[] sqlViews;
        private SqlProcedure[] sqlProcedures;
        private SqlObject[] sqlObjects;

        private Dictionary<SqlObject, int> sqlObjectWidthCache = new Dictionary<SqlObject, int>();
        private Dictionary<SqlObject, int> sqlObjectHeightCache = new Dictionary<SqlObject, int>();
        private Dictionary<string, string> toListTableLinkAddressCache = new Dictionary<string, string>();
        private Dictionary<string, string> toListViewLinkAddressCache = new Dictionary<string, string>();
        private Dictionary<string, string> toListProcedureLinkAddressCache = new Dictionary<string, string>();
        private Dictionary<string, string> tableLinkAddressCache = new Dictionary<string, string>();
        private Dictionary<string, string> viewLinkAddressCache = new Dictionary<string, string>();
        private Dictionary<string, string> procedureLinkAddressCache = new Dictionary<string, string>();

        /// <summary>
        /// 預先載入資料
        /// </summary>
        protected void Preloading()
        {
            databaseName = _sqlService.GetDefaultDatabaseName(connectionString);
            sqlTables = _sqlService.GetSqlTables(connectionString) ?? new SqlTable[0];
            sqlViews = _sqlService.GetSqlViews(connectionString) ?? new SqlView[0];
            sqlProcedures = _sqlService.GetSqlProcedures(connectionString) ?? new SqlProcedure[0];
            sqlObjects = _sqlService.GetSqlObjectReference(connectionString) ?? new SqlObject[0];
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
                Excel.Worksheet worksheet;
                // 清單
                worksheet = workbook.Worksheets.Add(After: workbook.Sheets[workbook.Sheets.Count]);
                FillSheet1(worksheet);
                // 系統主表
                worksheet = workbook.Worksheets.Add(After: workbook.Sheets[workbook.Sheets.Count]);
                FillSheet2(worksheet);
                // 物件關聯
                worksheet = workbook.Worksheets.Add(After: workbook.Sheets[workbook.Sheets.Count]);
                FillSheet3(worksheet);
                // 把原先僅剩的 Sheet 移除
                workbook.Worksheets[1].Delete();
                // 讓 Excel 打開顯示第一個 Sheet
                workbook.Worksheets[1].Activate();
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

            #region 資料表區塊
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
                    // 連結位置
                    GetTableAddress(databaseName, sqlTable.Schema, sqlTable.Name),
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
            }
            #endregion

            // 資料表跟檢視中間空一行
            rowIndex++;

            #region 檢視區塊
            // 檢視區塊
            range = worksheet.Range[$"A{rowIndex}", $"C{rowIndex + sqlViews.Length}"];
            // 檢視區塊 框線
            range.Cells.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
            // 檢視區塊 粗外框線
            range.BorderAround(Type.Missing, (Excel.XlBorderWeight)3, Excel.XlColorIndex.xlColorIndexAutomatic, Type.Missing);
            // 檢視區塊 字體
            range.Font.Name = "微軟正黑體";
            range.Font.Size = 11;
            range.Font.Bold = true;
            // 檢視區塊 標題列
            worksheet.Cells[rowIndex, 2] = "View Name";
            worksheet.Cells[rowIndex, 3] = "Description";
            range = worksheet.Range[$"A{rowIndex}", $"C{rowIndex}"];
            // 標題列置中
            range.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
            // 標題列文字顏色
            range.Font.Color = RGB(155, 0, 128);
            // 標題列背景色
            range.Interior.Color = RGB(204, 255, 204);
            rowIndex++;
            // 檢視名稱 列表
            num = 1;
            foreach (var sqlView in sqlViews)
            {
                // 設定列高
                range = worksheet.Range[$"A{rowIndex}"];
                range.RowHeight = 20.1;
                // 行數
                worksheet.Cells[rowIndex, 1] = num;
                // 檢視名稱
                // 取得連結
                var linkAddress = GetViewAddress(databaseName, sqlView.Schema, sqlView.Name);
                if (string.IsNullOrEmpty(linkAddress))
                {
                    // 沒有連結直接顯示檢視名稱
                    worksheet.Cells[rowIndex, 2] = sqlView.Name;
                }
                else
                {
                    // 有連結為檢視名稱加上連結
                    Excel.Hyperlink hyperlink = worksheet.Hyperlinks.Add(
                        worksheet.Cells[rowIndex, 2],
                        // 連結目標在同一個檔案內不用給值
                        "",
                        // 連結位置
                        linkAddress,
                        // 連結文字 (檢視名稱)
                        TextToDisplay: sqlView.Name);
                    // 設定連結字體會跑掉，需要重設
                    range = hyperlink.Range;
                    range.Font.Name = "微軟正黑體";
                    range.Font.Bold = true;
                    // 檢視名稱 文字顏色
                    range.Font.Color = RGB(0, 112, 192);
                }
                worksheet.Cells[rowIndex, 3] = sqlView.Description;
                rowIndex++;
                num++;
            }
            #endregion

            // 檢視跟預存程序中間空一行
            rowIndex++;

            #region 預存程序區塊
            // 預存程序區塊
            range = worksheet.Range[$"A{rowIndex}", $"C{rowIndex + sqlProcedures.Length}"];
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
            foreach (var sqlProcedure in sqlProcedures)
            {
                // 設定列高
                range = worksheet.Range[$"A{rowIndex}"];
                range.RowHeight = 20.1;
                // 行數
                worksheet.Cells[rowIndex, 1] = num;
                // 預存程序名稱
                // 取得連結
                var linkAddress = GetProcedureAddress(databaseName, sqlProcedure.Schema, sqlProcedure.Name);
                if (string.IsNullOrEmpty(linkAddress))
                {
                    // 沒有連結直接顯示預存程序名稱
                    worksheet.Cells[rowIndex, 2] = sqlProcedure.Name;
                }
                else
                {
                    // 有連結為預存程序名稱加上連結
                    Excel.Hyperlink hyperlink = worksheet.Hyperlinks.Add(
                        worksheet.Cells[rowIndex, 2],
                        // 連結目標在同一個檔案內不用給值
                        "",
                        // 連結位置
                        linkAddress,
                        // 連結文字 (預存程序名稱)
                        TextToDisplay: sqlProcedure.Name);
                    // 設定連結字體會跑掉，需要重設
                    range = hyperlink.Range;
                    range.Font.Name = "微軟正黑體";
                    range.Font.Bold = true;
                    // 預存程序名稱 文字顏色
                    range.Font.Color = RGB(0, 112, 192);
                }
                worksheet.Cells[rowIndex, 3] = sqlProcedure.Description;
                rowIndex++;
                num++;
            }
            #endregion

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

            foreach (var sqlTable in sqlTables)
            {
                // 回清單 連結
                Excel.Hyperlink hyperlink = worksheet.Hyperlinks.Add(
                    worksheet.Cells[rowIndex, 2],
                    // 連結目標在同一個檔案內不用給值
                    "",
                    // 連結位置
                    GetTableToListAddress(databaseName, sqlTable.Schema, sqlTable.Name),
                    // 連結文字
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

                // 標題列
                range = worksheet.Range[$"B{rowIndex}", $"G{rowIndex}"];
                // 標題列置中
                range.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                // 標題列文字顏色
                range.Font.Color = RGB(155, 0, 128);
                // 標題列背景色
                range.Interior.Color = RGB(204, 255, 204);
                // 標題列文字
                worksheet.Cells[rowIndex, 2] = "Table Name";
                worksheet.Cells[rowIndex, 3] = "Column";
                worksheet.Cells[rowIndex, 4] = "Data_type";
                worksheet.Cells[rowIndex, 5] = "null";
                worksheet.Cells[rowIndex, 6] = "Chinese Remark";
                worksheet.Cells[rowIndex, 7] = "SAMPLE";
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
            }

            worksheet.Columns.AutoFit();
        }

        /// <summary>
        /// 第 3 個 Sheet 的內容 (物件關聯)
        /// </summary>
        private void FillSheet3(Excel.Worksheet worksheet)
        {
            worksheet.Name = sheet3Name;

            Excel.Range range;

            // sqlObjects 總共會用到幾欄
            int columnWidth = sqlObjects.Any() ? sqlObjects.Max(GetWidth) : 0;
            // 從第幾個列開始寫入 sqlObjects
            const int startRowIndex = 1;
            // 從第幾個欄開始寫入 sqlObjects
            const int startColunmIndex = 2;

            // 將多個 SqlObject 寫入 Worksheet 的邏輯
            void FillSqlObjects(IEnumerable<SqlObject> sqlObjects, int rowIndex, int columnIndex)
            {
                if (sqlObjects != null && sqlObjects.Any())
                {
                    foreach (var sqlObject in sqlObjects)
                    {
                        if (startColunmIndex < columnIndex || sqlObject.Reference != null && sqlObject.Reference.Any())
                        {
                            FillSqlObject(sqlObject, rowIndex, columnIndex);
                            rowIndex += GetHeight(sqlObject);
                            if (startColunmIndex == columnIndex)
                            {
                                // +回清單
                                if (!string.IsNullOrEmpty(GetObjectToListAddress(sqlObject)))
                                {
                                    rowIndex++;
                                }
                                // +標題列 +說明列 +空行
                                rowIndex += 3;
                            }
                        }
                    }
                }
            }

            // 將一個 SqlObject 寫入 Worksheet 的邏輯
            void FillSqlObject(SqlObject sqlObject, int rowIndex, int columnIndex)
            {
                int rowspan = GetHeight(sqlObject);
                int colspan = sqlObject.Reference != null && sqlObject.Reference.Any() ? 1 : (columnWidth - columnIndex + startColunmIndex);

                #region 第一層物件前置作業
                if (startColunmIndex == columnIndex)
                {
                    // 回清單
                    var toListAddress = GetObjectToListAddress(sqlObject);
                    if (!string.IsNullOrEmpty(toListAddress))
                    {
                        // 回清單 連結
                        Excel.Hyperlink hyperlink = worksheet.Hyperlinks.Add(
                            worksheet.Cells[rowIndex, columnIndex],
                            // 連結目標在同一個檔案內不用給值
                            "",
                            // 連結位置
                            toListAddress,
                            // 連結文字
                            TextToDisplay: "回清單");
                        // 設定連結字體會跑掉，需要重設
                        range = hyperlink.Range;
                        range.Font.Name = "微軟正黑體";
                        range.Font.Bold = true;
                        // 回清單 框線
                        range.Cells.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
                        rowIndex++;
                    }

                    // 物件區塊 (+2 = +標題列 +說明列)
                    range = worksheet.Range[GetAddress(rowIndex, columnIndex), GetAddress(rowIndex + rowspan - 1 + 2, columnIndex + columnWidth - 1)];
                    // 物件區塊 框線
                    range.Cells.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
                    // 物件區塊 外框線
                    //range.BorderAround(Type.Missing, Excel.XlBorderWeight.xlThin, Excel.XlColorIndex.xlColorIndexAutomatic, Type.Missing);
                    // 物件區塊 垂直置中
                    range.VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;
                    // 物件區塊 字體
                    range.Font.Name = "微軟正黑體";
                    range.Font.Size = 10;

                    // 標題列
                    range = worksheet.Range[GetAddress(rowIndex, columnIndex), GetAddress(rowIndex, columnIndex + columnWidth - 1)];
                    // 標題列 水平置中
                    range.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                    // 標題列 粗體
                    range.Font.Bold = true;
                    // 標題列 文字顏色
                    range.Font.Color = RGB(155, 0, 128);
                    // 標題列 背景色
                    range.Interior.Color = RGB(204, 255, 204);
                    // 標題列 文字
                    worksheet.Cells[rowIndex, columnIndex] = $"{sqlObject.ObjectTypeDesc.ToUpperCamelCase(" ")} Name";
                    worksheet.Cells[rowIndex, columnIndex + 1] = "Reference";
                    // 標題列 Reference 合併儲存格
                    range = worksheet.Range[GetAddress(rowIndex, columnIndex + 1), GetAddress(rowIndex, columnIndex + columnWidth - 1)];
                    range.Merge();
                    rowIndex++;
                }
                #endregion

                range = worksheet.Range[GetAddress(rowIndex, columnIndex), GetAddress(rowIndex + rowspan - 1, columnIndex + colspan - 1)];
                // 合併儲存格
                if (rowspan > 1 || colspan > 1)
                {
                    range.Merge();
                }

                if (startColunmIndex == columnIndex)
                {
                    // 第一層物件不用顯示資料表名稱
                    range.Value = $"{sqlObject.ObjectName} {sqlObject.ErrorMessage}";
                    range.Font.Bold = true;
                    range.Characters[sqlObject.ObjectName.Length + 2].Font.Color = RGB(230, 95, 90);
                }
                else
                {
                    // 顯示文字 = Alias + 資料表名稱 + schema + 物件名稱
                    string alias = $"[{sqlObject.ObjectAlias}]";
                    string schema = sqlObject.SchemaName;
                    string displayText = $"{alias} {sqlObject.DatabaseName}.{schema}.{sqlObject.ObjectName} {sqlObject.ErrorMessage}";
                    string linkAddress = startColunmIndex < columnIndex ? GetObjectAddress(sqlObject) : null;
                    if (string.IsNullOrEmpty(linkAddress))
                    {
                        // 沒有連結直接顯示文字
                        range.Value = displayText;
                    }
                    else
                    {
                        // 有連結為顯示文字加上連結
                        Excel.Hyperlink hyperlink = worksheet.Hyperlinks.Add(
                            range,
                            // 連結目標在同一個檔案內不用給值
                            "",
                            // 連結位置
                            linkAddress,
                            // 連結文字
                            TextToDisplay: displayText);
                        // 設定連結字體會跑掉，需要重設
                        range = hyperlink.Range;
                        range.Font.Name = "微軟正黑體";
                        range.Font.Size = 10;
                    }
                    // 區別 資料表名稱 schema 物件名稱 文字顏色
                    int aliasLength = alias.Length;
                    int databaseNameLength = sqlObject.DatabaseName.Length;
                    int schemaLength = schema.Length;
                    int objectNameLength = sqlObject.ObjectName.Length;
                    range.Characters[1, aliasLength].Font.Color = RGB(65, 65, 65);
                    range.Characters[aliasLength + 2, databaseNameLength].Font.Color = RGB(20, 120, 145);
                    range.Characters[aliasLength + databaseNameLength + 3, schemaLength].Font.Color = RGB(100, 60, 150);
                    range.Characters[aliasLength + databaseNameLength + schemaLength + 4, objectNameLength].Font.Color = RGB(0, 80, 135);
                    range.Characters[aliasLength + databaseNameLength + schemaLength + objectNameLength + 5].Font.Color = RGB(230, 95, 90);
                }
                if (!string.IsNullOrEmpty(sqlObject.ErrorMessage))
                {
                    range.RowHeight = 33;
                }

                // 展開關聯物件
                FillSqlObjects(sqlObject.Reference, rowIndex, columnIndex + colspan);
                rowIndex += rowspan;

                #region 第一層物件後置作業
                if (startColunmIndex == columnIndex)
                {
                    // 物件說明
                    worksheet.Cells[rowIndex, columnIndex] = $"{sqlObject.ObjectAlias}說明";
                    // 說明列 背景色
                    range = worksheet.Range[GetAddress(rowIndex, columnIndex), GetAddress(rowIndex, columnIndex + columnWidth - 1)];
                    range.Interior.Color = RGB(240, 240, 240);
                    // 說明列 空白區塊 合併儲存格
                    range = worksheet.Range[GetAddress(rowIndex, columnIndex + 1), GetAddress(rowIndex, columnIndex + columnWidth - 1)];
                    range.Merge();

                    rowIndex++;
                }
                #endregion
            }

            FillSqlObjects(sqlObjects, startRowIndex, startColunmIndex);

            worksheet.Columns.AutoFit();
        }

        private int RGB(int r, int g, int b)
        {
            return r + g * 256 + b * 256 * 256;
        }

        /// <summary>
        /// 將數字轉成 Excel 欄位用的 A B C
        /// </summary>
        private string GetAddress(int columnIndex)
        {
            StringBuilder stringBuilder = new StringBuilder();
            while (columnIndex > 0)
            {
                int modulo = (columnIndex - 1) % 26;
                stringBuilder.Insert(0, Convert.ToChar('A' + modulo));
                columnIndex = (columnIndex - modulo) / 26;
            }
            return stringBuilder.ToString();
        }
        /// <summary>
        /// 將數字轉成 Excel 欄位 A1 A2 B1 B2
        /// </summary>
        private string GetAddress(int rowIndex, int columnIndex)
        {
            return $"{GetAddress(columnIndex)}{rowIndex}";
        }
        /// <summary>
        /// 取得 <see cref="SqlObject"/> 包含 Reference 總共需要寫入幾欄
        /// </summary>
        private int GetWidth(SqlObject sqlObject)
        {
            if (!sqlObjectWidthCache.ContainsKey(sqlObject))
            {
                int width = 1;
                if (sqlObject.Reference != null && sqlObject.Reference.Any())
                {
                    width = sqlObject.Reference.Max(GetWidth) + 1;
                }
                sqlObjectWidthCache.Add(sqlObject, width);
            }
            return sqlObjectWidthCache[sqlObject];
        }
        /// <summary>
        /// 取得 <see cref="SqlObject"/> 包含 Reference 總共需要寫入幾列
        /// </summary>
        private int GetHeight(SqlObject sqlObject)
        {
            if (!sqlObjectHeightCache.ContainsKey(sqlObject))
            {
                int height = 1;
                if (sqlObject.Reference != null && sqlObject.Reference.Any())
                {
                    height = sqlObject.Reference.Sum(GetHeight);
                }
                sqlObjectHeightCache.Add(sqlObject, height);
            }
            return sqlObjectHeightCache[sqlObject];
        }

        /// <summary>
        /// 取得 <see cref="SqlObject"/> 回清單連結
        /// </summary>
        private string GetObjectToListAddress(SqlObject sqlObject)
        {
            switch (sqlObject.ObjectType)
            {
                case "U":
                    return GetTableToListAddress(sqlObject.DatabaseName, sqlObject.SchemaName, sqlObject.ObjectName);
                case "V":
                    return GetViewToListAddress(sqlObject.DatabaseName, sqlObject.SchemaName, sqlObject.ObjectName);
                case "P":
                    return GetProcedureToListAddress(sqlObject.DatabaseName, sqlObject.SchemaName, sqlObject.ObjectName);
            }
            return null;
        }
        /// <summary>
        /// 取得資料表回清單連結
        /// </summary>
        private string GetTableToListAddress(string databaseName, string schema, string tableName)
        {
            string cacheKey = $"{databaseName}.{schema}.{tableName}";
            if (!toListTableLinkAddressCache.ContainsKey(cacheKey))
            {
                string linkAddress = null;
                if (databaseName == this.databaseName)
                {
                    // 從 1 開始 +資料表標題列
                    int linkIndex = 1 + 1;
                    foreach (var sqlTable in sqlTables)
                    {
                        if (sqlTable.Name == tableName)
                        {
                            linkAddress = $"{sheet1Name}!B{linkIndex}";
                            break;
                        }
                        linkIndex++;
                    }
                }
                toListTableLinkAddressCache.Add(cacheKey, linkAddress);
            }
            return toListTableLinkAddressCache[cacheKey];
        }
        /// <summary>
        /// 取得檢視回清單連結
        /// </summary>
        private string GetViewToListAddress(string databaseName, string schema, string viewName)
        {
            string cacheKey = $"{databaseName}.{schema}.{viewName}";
            if (!toListViewLinkAddressCache.ContainsKey(cacheKey))
            {
                string linkAddress = null;
                if (databaseName == this.databaseName)
                {
                    // 從 1 開始 +資料表標題列 +資料表數量 +空行 +檢視標題列
                    int linkIndex = 1 + 1 + sqlTables.Length + 2;
                    foreach (var sqlView in sqlViews)
                    {
                        if (sqlView.Name == viewName)
                        {
                            linkAddress = $"{sheet1Name}!B{linkIndex}";
                            break;
                        }
                        linkIndex++;
                    }
                }
                toListViewLinkAddressCache.Add(cacheKey, linkAddress);
            }
            return toListViewLinkAddressCache[cacheKey];
        }
        /// <summary>
        /// 取得預存程序回清單連結
        /// </summary>
        private string GetProcedureToListAddress(string databaseName, string schema, string procedureName)
        {
            string cacheKey = $"{databaseName}.{schema}.{procedureName}";
            if (!toListProcedureLinkAddressCache.ContainsKey(cacheKey))
            {
                string linkAddress = null;
                if (databaseName == this.databaseName)
                {
                    // 從 1 開始 +資料表標題列 +資料表數量 +空行 +檢視標題列 +檢視數量 +空行 +預存程序標題列
                    int linkIndex = 1 + 1 + sqlTables.Length + 2 + sqlViews.Length + 2;
                    foreach (var sqlProcedure in sqlProcedures)
                    {
                        if (sqlProcedure.Name == procedureName)
                        {
                            linkAddress = $"{sheet1Name}!B{linkIndex}";
                            break;
                        }
                        linkIndex++;
                    }
                }
                toListProcedureLinkAddressCache.Add(cacheKey, linkAddress);
            }
            return toListProcedureLinkAddressCache[cacheKey];
        }

        /// <summary>
        /// 取得 <see cref="SqlObject"/> 的連結
        /// </summary>
        private string GetObjectAddress(SqlObject sqlObject)
        {
            switch (sqlObject.ObjectType)
            {
                case "U":
                    return GetTableAddress(sqlObject.DatabaseName, sqlObject.SchemaName, sqlObject.ObjectName);
                case "V":
                    return GetViewAddress(sqlObject.DatabaseName, sqlObject.SchemaName, sqlObject.ObjectName);
                case "P":
                    return GetProcedureAddress(sqlObject.DatabaseName, sqlObject.SchemaName, sqlObject.ObjectName);
            }
            return null;
        }
        /// <summary>
        /// 取得資料表的連結
        /// </summary>
        private string GetTableAddress(string databaseName, string schema, string tableName)
        {
            string cacheKey = $"{databaseName}.{schema}.{tableName}";
            if (!tableLinkAddressCache.ContainsKey(cacheKey))
            {
                string linkAddress = null;
                if (databaseName == this.databaseName)
                {
                    int linkIndex = 1;
                    foreach (var sqlTable in sqlTables)
                    {
                        if (sqlTable.Name == tableName)
                        {
                            // 連結位置 (+2 = 回清單 + 標題列)
                            linkAddress = $"{sheet2Name}!B{linkIndex + 2}:B{linkIndex + 2 + sqlTable.Columns.Length - 1}";
                            break;
                        }
                        // (+5 = 回清單 + 標題列 + 資料表說明行 + 建立索引語法行 + 間隔)
                        linkIndex += sqlTable.Columns.Length + 5;
                    }
                }
                tableLinkAddressCache.Add(cacheKey, linkAddress);
            }
            return tableLinkAddressCache[cacheKey];
        }
        /// <summary>
        /// 取得檢視的連結
        /// </summary>
        private string GetViewAddress(string databaseName, string schema, string viewName)
        {
            string cacheKey = $"{databaseName}.{schema}.{viewName}";
            if (!viewLinkAddressCache.ContainsKey(cacheKey))
            {
                string linkAddress = null;
                var sqlViewObject = sqlObjects.FirstOrDefault(x => x.ObjectType == "V" && x.DatabaseName == databaseName && x.ObjectName == viewName);
                if (sqlViewObject != null)
                {
                    int linkIndex = 1;
                    foreach (var sqlObject in sqlObjects)
                    {
                        if (sqlObject.Reference != null && sqlObject.Reference.Any())
                        {
                            if (sqlObject == sqlViewObject)
                            {
                                linkAddress = $"{sheet3Name}!B{linkIndex + 2}";
                                break;
                            }
                            linkIndex += GetHeight(sqlObject);
                            // +回清單
                            if (!string.IsNullOrEmpty(GetObjectToListAddress(sqlObject)))
                            {
                                linkIndex++;
                            }
                            // +標題列 +說明列 +空行
                            linkIndex += 3;
                        }
                    }
                }
                viewLinkAddressCache.Add(cacheKey, linkAddress);
            }
            return viewLinkAddressCache[cacheKey];
        }
        /// <summary>
        /// 取得預存程序的連結
        /// </summary>
        private string GetProcedureAddress(string databaseName, string schema, string procedureName)
        {
            string cacheKey = $"{databaseName}.{schema}.{procedureName}";
            if (!procedureLinkAddressCache.ContainsKey(cacheKey))
            {
                string linkAddress = null;
                var sqlProcedureObject = sqlObjects.FirstOrDefault(x => x.ObjectType == "P" && x.DatabaseName == databaseName && x.ObjectName == procedureName);
                if (sqlProcedureObject != null)
                {
                    int linkIndex = 1;
                    foreach (var sqlObject in sqlObjects)
                    {
                        if (sqlObject.Reference != null && sqlObject.Reference.Any())
                        {
                            if (sqlObject == sqlProcedureObject)
                            {
                                linkAddress = $"{sheet3Name}!B{linkIndex + 2}";
                                break;
                            }
                            linkIndex += GetHeight(sqlObject);
                            // +回清單
                            if (!string.IsNullOrEmpty(GetObjectToListAddress(sqlObject)))
                            {
                                linkIndex++;
                            }
                            // +標題列 +說明列 +空行
                            linkIndex += 3;
                        }
                    }
                }
                procedureLinkAddressCache.Add(cacheKey, linkAddress);
            }
            return procedureLinkAddressCache[cacheKey];
        }
    }
}
