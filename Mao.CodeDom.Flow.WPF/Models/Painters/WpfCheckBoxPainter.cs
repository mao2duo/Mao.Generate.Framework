using Mao.CodeDom.Flow.WPF.Models.Inputs;
using Mao.Generate.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Mao.CodeDom.Flow.WPF.Models.Painters
{
    public class WpfCheckBoxPainter : UIPainter<WpfCanvas, WpfCheckBox>
    {
        public override WpfCanvas Canvas { get; }
        public WpfCheckBoxPainter(WpfCanvas canvas)
        {
            Canvas = canvas;
        }

        public override void Add(WpfCheckBox input)
        {
            Grid grid = Canvas.Container;

            int rowIndex = grid.RowDefinitions.Count;
            var primaryRow = new RowDefinition() { Height = GridLength.Auto };
            grid.RowDefinitions.Add(primaryRow);

            // 加入顯示名稱的控制項
            var textBlock1 = new TextBlock()
            {
                Text = input.DisplayName,
                Foreground = (Brush)Application.Current.Resources["TextInfo"],
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 16,
                Margin = new Thickness(10)
            };

            Grid.SetRow(textBlock1, rowIndex);
            Grid.SetColumn(textBlock1, 0);
            grid.Children.Add(textBlock1);

            var checkBox = new CheckBox()
            {
                IsChecked = input.Checked,
                FontSize = 14,
                Margin = new Thickness(10)
            };

            Grid.SetRow(checkBox, rowIndex);
            Grid.SetColumn(checkBox, 1);
            grid.Children.Add(checkBox);

            Canvas.SetInputData(input, "PrimaryRow", primaryRow);
            Canvas.SetInputData(input, "TextBlock1", textBlock1);
            Canvas.SetInputData(input, "CheckBox", checkBox);

            if (!string.IsNullOrWhiteSpace(input.Description))
            {
                // 加入顯示描述的控制項
                rowIndex = rowIndex + 1;
                var secondaryRow = new RowDefinition() { Height = GridLength.Auto };
                grid.RowDefinitions.Add(secondaryRow);

                var textBlock2 = new TextBlock()
                {
                    Text = input.Description,
                    Foreground = new SolidColorBrush(Colors.Black),
                    FontSize = 11
                };
                var border1 = new Border()
                {
                    Padding = new Thickness(5),
                    Margin = new Thickness(10, -10, 10, 10),
                    Background = (Brush)Application.Current.Resources["BgInfo"]
                };
                border1.Child = textBlock2;
                Grid.SetRow(border1, rowIndex);
                Grid.SetColumn(border1, 1);
                grid.Children.Add(border1);

                Canvas.SetInputData(input, "SecondaryRow", secondaryRow);
                Canvas.SetInputData(input, "Border1", border1);
            }
        }
        public override void Remove(WpfCheckBox input)
        {
            Grid grid = Canvas.Container;

            var primaryRow = Canvas.GetInputData<RowDefinition>(input, "PrimaryRow");
            var secondaryRow = Canvas.GetInputData<RowDefinition>(input, "SecondaryRow");
            var textBlock1 = Canvas.GetInputData<TextBlock>(input, "TextBlock1");
            var checkBox = Canvas.GetInputData<CheckBox>(input, "CheckBox");
            var border1 = Canvas.GetInputData<Border>(input, "Border1");
            if (border1 != null)
            {
                grid.Children.Remove(border1);
            }
            if (secondaryRow != null)
            {
                grid.RowDefinitions.Remove(secondaryRow);
            }
            grid.Children.Remove(textBlock1);
            grid.Children.Remove(checkBox);
            grid.RowDefinitions.Remove(primaryRow);
        }
    }
}
