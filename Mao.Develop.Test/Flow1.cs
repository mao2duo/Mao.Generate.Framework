using Mao.CodeDom.Flow.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mao.Develop.Test
{
    public class Flow1 : FlowBase
    {
        public void Execute()
        {
            var inputWrapper1 = AddTextBox();
            var inputWrapper2 = AddTextBox();

            int a = Convert.ToInt32(inputWrapper1.Input.Text);
            int b = Convert.ToInt32(inputWrapper2.Input.Text);

            Alert("");

        }
    }
}
