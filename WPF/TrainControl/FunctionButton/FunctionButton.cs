using System;
using System.Windows;
using System.Windows.Controls;
using Persistence;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Effects;
using Persistence.Entities;

namespace Shell.WPF.TrainControl.FunctionButton
{
  internal static class FunctionButton
  {
    internal static void ApplyStyle<T>(T button, VehicleFunctionEntity vehicleFunctionEntity) where T : ButtonBase
    {
      button.Height = 50;
      button.Width = 95;

      RichTextBox textBox = new RichTextBox
                            {
                              IsReadOnly = true,
                              IsHitTestVisible = false,
                              Focusable = false,
                              Background = Brushes.Transparent,
                              BorderThickness = new(0)
                            };

      FlowDocument doc = new();

      Paragraph para = new()
                       {
                         TextAlignment = TextAlignment.Center
                       };

      Run firstLine = new(ShortenString(vehicleFunctionEntity.Name))
                      {
                        Foreground = Brushes.Black
                      };
      para.Inlines.Add(firstLine);

      if (vehicleFunctionEntity.ShowFunctionNumber)
      {
        LineBreak lineBreak = new();
        para.Inlines.Add(lineBreak);

        Run secondLine = new($"F{vehicleFunctionEntity.Address}")
                         {
                           FontSize = 12,
                           Foreground = Brushes.Gray
                         };
        para.Inlines.Add(secondLine);
      }

      doc.Blocks.Add(para);

      textBox.Document = doc;

      button.Content = textBox;
      button.Tag = vehicleFunctionEntity;
      button.ToolTip = vehicleFunctionEntity.ToString();
      button.Margin = new(10);
      button.Padding = new(2);
      button.BorderThickness = new Thickness(0);
      button.Effect = new DropShadowEffect() { Opacity = 0.2 };
    }

    private static string ShortenString(string input)
    {
      if (input.Length <= 13)
      {
        return input;
      }
      else
      {
        return input.Substring(0, 13) + "..";
      }
    }
  }
}