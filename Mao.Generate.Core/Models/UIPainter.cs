using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mao.Generate.Core.Models
{
    public interface IUIPainter<out TCanvas, out TInput>
        where TCanvas : UICanvas
        where TInput : UIInput
    {
    }

    public abstract class UIPainter<TCanvas, TInput> : IUIPainter<TCanvas, UIInput>
        where TCanvas : UICanvas
        where TInput : UIInput
    {
        public abstract TCanvas Canvas { get; }

        /// <summary>
        /// 將控制項新增至載體的方法
        /// </summary>
        public abstract void Add(TInput input);
        /// <summary>
        /// 將控制項從載體移除的方法
        /// </summary>
        public abstract void Remove(TInput input);

    }
}
