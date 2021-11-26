using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mao.Generate.Core.Models
{
    public abstract class UICanvas
    {
        public List<UIInput> Inputs { get; } = new List<UIInput>();
        protected Dictionary<Guid, Dictionary<string, object>> InputData { get; } = new Dictionary<Guid, Dictionary<string, object>>();

        /// <summary>
        /// 取得繪製指定控制項的物件
        /// </summary>
        protected abstract IUIPainter<UICanvas, TInput> GetPainter<TInput>()
            where TInput : UIInput;

        /// <summary>
        /// 加入控制項
        /// </summary>
        public virtual void Add<TInput>(TInput input) where TInput : UIInput
        {
            var painter = GetPainter<TInput>();
            typeof(UIPainter<,>)
                .MakeGenericType(new Type[] { this.GetType(), typeof(TInput) })
                .GetMethod("Add")
                .Invoke(painter, new object[] { input });
            Inputs.Add(input);
        }
        /// <summary>
        /// 移除控制項
        /// </summary>
        public virtual void Remove<TInput>(TInput input) where TInput : UIInput
        {
            var painter = GetPainter<TInput>();
            Inputs.Remove(input);
            typeof(UIPainter<,>)
                .MakeGenericType(new Type[] { this.GetType(), typeof(TInput) })
                .GetMethod("Add")
                .Invoke(painter, new object[] { input });
        }

        /// <summary>
        /// 設定對應控制項的指定名稱資料
        /// </summary>
        public void SetInputData<TValue>(UIInput uiInput, string name, TValue value)
        {
            Dictionary<string, object> dictionary;
            if (InputData.ContainsKey(uiInput.Guid))
            {
                dictionary = InputData[uiInput.Guid];
            }
            else
            {
                InputData.Add(uiInput.Guid, dictionary = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase));
            }
            if (dictionary.ContainsKey(name))
            {
                dictionary[name] = value;
            }
            else
            {
                dictionary.Add(name, value);
            }
        }
        /// <summary>
        /// 取得對應控制項的指定名稱資料
        /// </summary>
        public TValue GetInputData<TValue>(UIInput uiInput, string name)
        {
            if (InputData.ContainsKey(uiInput.Guid))
            {
                var dictionary = InputData[uiInput.Guid];
                if (dictionary.ContainsKey(name))
                {
                    return (TValue)dictionary[name];
                }
            }
            return default(TValue);
        }

        /// <summary>
        /// 以 Guid 取得控制項
        /// </summary>
        public UIInput GetInputByGuid(Guid guid)
        {
            return Inputs.FirstOrDefault(x => x.Guid == guid);
        }
        /// <summary>
        /// 以名稱取得控制項
        /// </summary>
        public UIInput GetInputByName(string name)
        {
            return Inputs.FirstOrDefault(x => string.Equals(x.Name, name));
        }
        /// <summary>
        /// 以名稱取得控制項
        /// </summary>
        public UIInput GetInputByName(string name, StringComparison stringComparison)
        {
            return Inputs.FirstOrDefault(x => string.Equals(x.Name, name, stringComparison));
        }
        /// <summary>
        /// 以名稱取得多個控制項
        /// </summary>
        public ICollection<UIInput> GetInputsByName(string name)
        {
            return Inputs.Where(x => string.Equals(x.Name, name)).ToArray();
        }
        /// <summary>
        /// 以名稱取得多個控制項
        /// </summary>
        public ICollection<UIInput> GetInputsByName(string name, StringComparison stringComparison)
        {
            return Inputs.Where(x => string.Equals(x.Name, name, stringComparison)).ToArray();
        }
    }
}
