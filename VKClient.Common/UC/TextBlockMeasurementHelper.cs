using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace VKClient.Common.UC
{
  public static class TextBlockMeasurementHelper
  {
    private static readonly TextBlock _textBlock = new TextBlock();

    public static double MeasureHeight(double width, string text, FontFamily fontFamily, double fontSize, double lineHeight, LineStackingStrategy lineStackingStrategy, TextWrapping textWrapping, Thickness margin)
    {
      TextBlockMeasurementHelper._textBlock.Width = width;
      TextBlockMeasurementHelper._textBlock.Text = text;
      TextBlockMeasurementHelper._textBlock.FontFamily = fontFamily;
      TextBlockMeasurementHelper._textBlock.FontSize = fontSize;
      TextBlockMeasurementHelper._textBlock.LineHeight = lineHeight;
      TextBlockMeasurementHelper._textBlock.LineStackingStrategy = lineStackingStrategy;
      TextBlockMeasurementHelper._textBlock.TextWrapping = textWrapping;
      return TextBlockMeasurementHelper._textBlock.ActualHeight + margin.Top + margin.Bottom;
    }
  }
}
