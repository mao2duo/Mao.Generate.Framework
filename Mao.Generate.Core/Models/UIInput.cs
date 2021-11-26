using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mao.Generate.Core.Models
{
    public abstract class UIInput
    {
        /// <summary>
        /// 控制項的唯一識別碼
        /// </summary>
        public virtual Guid Guid { get; } = Guid.NewGuid();
        /// <summary>
        /// 控制項自訂名稱
        /// </summary>
        public virtual string Name { get; set; }

        /// <summary>
        /// 顯示名稱
        /// </summary>
        public virtual string DisplayName { get; set; }
        /// <summary>
        /// 控制項說明文字
        /// </summary>
        public virtual string Description { get; set; }

        /// <summary>
        /// 未輸入值時的提示內容
        /// </summary>
        public virtual string Placeholder { get; set; }
        /// <summary>
        /// 控制項的值
        /// </summary>
        public virtual object Value { get; set; }

        /// <summary>
        /// 是否可見
        /// </summary>
        public virtual bool Visible { get; set; }
        /// <summary>
        /// 是否啟用
        /// </summary>
        public virtual bool Enabled { get; set; }


        public virtual object Tag { get; set; }
    }
}
