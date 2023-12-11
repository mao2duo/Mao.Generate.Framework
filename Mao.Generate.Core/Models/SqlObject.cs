using System;
using System.Collections.Generic;

namespace Mao.Generate.Core.Models
{
    public class SqlObject
    {
        public string DatabaseName { get; set; }
        public string SchemaName { get; set; }
        public string ObjectName { get; set; }
        public string ObjectType { get; set; }
        public string ObjectTypeDesc { get; set; }

        public string ObjectAlias
        {
            get
            {
                switch (ObjectType)
                {
                    case "AF":
                        return "彙總函式 (CLR)";
                    case "C":
                        return "CHECK 條件約束";
                    case "D":
                        return "DEFAULT (條件約束或獨立式)";
                    case "F":
                        return "FOREIGN KEY 條件約束";
                    case "FN":
                        return "SQL 純量函數";
                    case "FS":
                        return "組件 (CLR) 純量函數";
                    case "FT":
                        return "組件 (CLR) 資料表值函式";
                    case "IF":
                        return "SQL 嵌入資料表值函式";
                    case "IT":
                        return "內部資料表";
                    case "P":
                        return "SQL 預存程序";
                    case "PC":
                        return "組件 (CLR) 預存程序";
                    case "PG":
                        return "計畫指南";
                    case "PK":
                        return "PRIMARY KEY 條件約束";
                    case "R":
                        return "規則 (舊式、獨立式)";
                    case "RF":
                        return "複寫篩選程序";
                    case "S":
                        return "系統基底資料表";
                    case "SN":
                        return "同義字";
                    case "SO":
                        return "序列物件";
                    case "U":
                        //return "資料表 (使用者定義)";
                        return "資料表";
                    case "V":
                        return "檢視";
                    case "EC":
                        return "Edge 條件約束";
                    case "SQ":
                        return "服務佇列";
                    case "TA":
                        return "組件 (CLR) DML 觸發程序";
                    case "TF":
                        return "SQL 資料表值函式";
                    case "TR":
                        return "SQL DML 觸發程序";
                    case "TT":
                        return "資料表類型";
                    case "UQ":
                        return "UNIQUE 條件約束";
                    case "X":
                        return "擴充預存程序";
                    case "ST":
                        return "STATS_TREE";
                    case "ET":
                        return "外部資料表";
                }
                if (!string.IsNullOrEmpty(ObjectTypeDesc))
                {
                    return ObjectTypeDesc;
                }
                if (!string.IsNullOrEmpty(ClassDesc))
                {
                    return ClassDesc;
                }
                if (!string.IsNullOrEmpty(ObjectType))
                {
                    return ObjectType;
                }
                return Convert.ToString(Class);
            }
        }

        public byte Class { get; set; }
        public string ClassDesc { get; set; }

        public List<SqlObject> Reference { get; set; }
        public string ErrorMessage { get; set; }
    }
}
