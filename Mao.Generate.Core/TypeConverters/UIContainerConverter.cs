using Mao.Generate.Core.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Mao.Generate.Core.TypeConverters
{
    public class UIContainerConverter : BaseTypeConverter
    {
        /// <summary>
        /// CsType To UIContainer
        /// </summary>
        protected UIContainer ConvertFrom(CsType csType)
        {
            UIContainer uiContainer = new UIContainer();
            uiContainer.GenerateType = UIContainerGenerateType.Object;
            //foreach (var property in properties)
            //{
            //    IEnumerable<UIInput> inputs;
            //    if (property.PropertyType.IsArray)
            //    {
            //        inputs = GenerateArrayInputs(property);
            //    }
            //    else if (!typeof(string).IsAssignableFrom(property.PropertyType) && property.PropertyType.IsClass)
            //    {
            //        inputs = GenerateObjectInputs(property);
            //    }
            //    else
            //    {
            //        inputs = GenerateValueInputs(property);
            //    }
            //    if (inputs != null && inputs.Any())
            //    {
            //        uiContainer.Children.AddRange(inputs);
            //    }
            //}
            return uiContainer;
        }
    }
}
