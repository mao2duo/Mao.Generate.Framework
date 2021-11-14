using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mao.Generate.Core.Models
{
    public class SqlDatabase
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public SqlDatabaseState State { get; set; }
    }

    public enum SqlDatabaseState
    {
        ONLINE,
        RESTORING,
        RECOVERING,
        RECOVERY_PENDING,
        SUSPECT,
        EMERGENCY,
        OFFLINE,
        COPYING,
        OFFLINE_SECONDARY = 10
    }
}
