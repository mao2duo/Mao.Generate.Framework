using Mao.CodeDom.Flow.Models;
using Mao.CodeDom.Flow.WPF.Models.Inputs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mao.CodeDom.Flow.WPF.Models
{
    public abstract class WpfFlowBase : FlowBase
    {
        public FlowInputWrapper<WpfCheckBox> AddWpfCheckBox(Action<WpfCheckBox> options = null)
        {
            throw new NotImplementedException();
        }
        public FlowInputWrapper<WpfCheckBoxList<TValue>> AddWpfCheckBoxList<TValue>(Action<WpfCheckBoxList<TValue>> options = null)
        {
            throw new NotImplementedException();
        }
        public FlowInputWrapper<WpfComboBox<TValue>> AddWpfComboBox<TValue>(Action<WpfComboBox<TValue>> options = null)
        {
            throw new NotImplementedException();
        }
        public FlowInputWrapper<WpfRadioButtonList<TValue>> AddWpfRadioButtonList<TValue>(Action<WpfRadioButtonList<TValue>> options = null)
        {
            throw new NotImplementedException();
        }
        public FlowInputWrapper<WpfTextBox> AddWpfTextBox(Action<WpfTextBox> options = null)
        {
            throw new NotImplementedException();
        }
    }
}
