using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mao.Generate.Core.Models
{
    public abstract class UISelect<TValue> : UIInput
    {
        /// <summary>
        /// 資料集
        /// </summary>
        public UIDataSource<TValue> DataSource { get; set; }
        /// <summary>
        /// 是否可以選擇多個值
        /// </summary>
        public virtual bool Multiple { get; set; }

        /// <summary>
        /// 目前選擇的第一個值
        /// </summary>
        public virtual TValue SelectedValue { get; set; }
        /// <summary>
        /// 目前選擇的所有值
        /// </summary>
        public virtual IEnumerable<TValue> SelectedValues { get; set; }
        /// <summary>
        /// 目前選擇項目或輸入的顯示文字
        /// </summary>
        public virtual string SelectedText { get; set; }

        /// <summary>
        /// 是否具有輸入文字的功能
        /// </summary>
        public virtual bool Editable { get; set; }
        /// <summary>
        /// 是否可以輸入資料集以外的文字
        /// </summary>
        public virtual bool AllowCustomText { get; set; }
    }
}
