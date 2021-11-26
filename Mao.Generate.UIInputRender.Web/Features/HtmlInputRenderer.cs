using Mao.Generate.Core.Models;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;

namespace Mao.Generate.UIInputRender.Web.Features
{
    public class HtmlInputRenderer
    {
        public HtmlHelper Html { get; }

        public HtmlInputRenderer(HtmlHelper helper)
        {
            Html = helper;
        }

        public IHtmlString Render(UIInput input, Dictionary<string, object> parameters, out NameValueCollection returns)
        {
            if (input is UITextbox textbox)
            {
                var viewModel = new HtmlInputViewModel<UITextbox>(textbox, parameters);
                returns = viewModel.Returns;
                return RenderPartial(viewModel);
            }
            returns = null;
            return null;
        }

        protected IHtmlString RenderPartial(HtmlInputViewModel<UITextbox> viewModel)
        {
            return Html.Partial("~/Views/Shared/UIInputs/UITextbox.cshtml", viewModel);
        }
    }
}