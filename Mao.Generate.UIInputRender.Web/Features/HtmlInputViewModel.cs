using Mao.Generate.Core.Models;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;

namespace Mao.Generate.UIInputRender.Web.Features
{
    public class HtmlInputViewModel<TInput> where TInput : UIInput
    {
        public TInput Input { get; }
        public Dictionary<string, object> Parameters { get; }
        public NameValueCollection Returns { get; }

        public HtmlInputViewModel(TInput input, Dictionary<string, object> parameters)
        {
            Input = input;
            Parameters = parameters ?? new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            Returns = new NameValueCollection(StringComparer.OrdinalIgnoreCase);
        }
    }
}