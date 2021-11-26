using Mao.Generate.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mao.CodeDom.Flow.Models
{
    public abstract class FlowBase
    {
        public void Alert(string message)
        {
        }
        public bool Confirm(string message)
        {
            return true;
        }

        public FlowInputWrapper<UICheckbox> AddCheckBox(Action<UICheckbox> options = null)
        {
            throw new NotImplementedException();
        }
        public FlowInputWrapper<UISelect<TValue>> AddSelect<TValue>(Action<UISelect<TValue>> options = null)
        {
            throw new NotImplementedException();
        }
        public FlowInputWrapper<UITextbox> AddTextBox(Action<UITextbox> options = null)
        {
            throw new NotImplementedException();
        }
    }
}
