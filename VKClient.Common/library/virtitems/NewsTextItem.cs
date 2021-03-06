using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using VKClient.Audio.Base.Events;
using VKClient.Audio.Base.Library;
using VKClient.Common.Framework;
using VKClient.Common.Localization;

namespace VKClient.Common.Library.VirtItems
{
  public class NewsTextItem : VirtualizableItemBase
  {
    private readonly ScrollableTextBlock _textBlockPreview = new ScrollableTextBlock() { DisableHyperlinks = false };
    private const double LineHeight = 31.6;
    private readonly double _fontSize;
    private readonly FontFamily _fontFamily;
    private readonly double _lineHeight;
    private readonly Brush _foreground;
    private readonly double _horizontalWidth;
    private readonly double _verticalWidth;
    private readonly HorizontalAlignment _horizontalContentAlignment;
    private readonly TextAlignment _textAlignment;
    private bool _supportExpandText;
    private bool _expandOnNewPageOnly;
    private ScrollableTextBlock _textBlockFull;
    private readonly string _textId;
    private string _text;
    private double _verticalHeight;
    private double _horizontalHeight;
    private bool _preview;
    private bool _showReadFull;
    private string _parentPostId;
    private readonly bool _disableHyperlinks;
    private readonly bool _hideHyperlinksForeground;
    private bool _isHorizontalOrientation;

    public bool IsHorizontalOrientation
    {
      get
      {
        return this._isHorizontalOrientation;
      }
      set
      {
        if (this._isHorizontalOrientation == value)
          return;
        this._isHorizontalOrientation = value;
        this.UpdateLayout();
      }
    }

    public override double FixedHeight
    {
      get
      {
        if (!this._isHorizontalOrientation)
          return this._verticalHeight;
        return this._horizontalHeight;
      }
    }

    public NewsTextItem(double width, Thickness margin, string text, bool preview, Action tapCallback = null, double fontSize = 0.0, FontFamily fontFamily = null, double lineHeight = 0.0, Brush foreground = null, bool isHorizontalOrientation = false, double horizontalWidth = 0.0, HorizontalAlignment horizontalContentAlignment = 0, string textId = "", TextAlignment textAlignment = TextAlignment.Left, bool supportExpandText = true, string parentPostId = null, bool disableHyperlinks = false, bool hideHyperlinksForeground = false)
      : base(width, margin,  new Thickness())
    {
      this._text = text ?? "";
      this._textId = textId;
      this._fontSize = fontSize;
      this._fontFamily = fontFamily;
      this._lineHeight = lineHeight;
      this._foreground = foreground;
      this._horizontalContentAlignment = horizontalContentAlignment;
      this._textAlignment = textAlignment;
      this._supportExpandText = supportExpandText;
      this._preview = preview;
      this._isHorizontalOrientation = isHorizontalOrientation;
      this._verticalWidth = width;
      this._horizontalWidth = horizontalWidth;
      this._parentPostId = parentPostId;
      this._disableHyperlinks = disableHyperlinks;
      this._hideHyperlinksForeground = hideHyperlinksForeground;
      base.Width = this._isHorizontalOrientation ? horizontalWidth : width;
      this.MeasureText();
    }

    private new void UpdateLayout()
    {
      base.Width = this._isHorizontalOrientation ? this._horizontalWidth : this._verticalWidth;
      if (this._textBlockFull == null)
        return;
      this._textBlockFull.Width = base.Width;
    }

    private void MeasureText()
    {
      string text = this._text;
      if (this._preview)
      {
        ScrollableTextBlock textBlockFull = this._textBlockFull;
        if ((textBlockFull != null ? textBlockFull.Parent :  null) != null)
          return;
        this._textBlockPreview.TextId = this._textId;
        this._textBlockPreview.FontSize=(this._fontSize == 0.0 ? 20.0 : this._fontSize);
        this._textBlockPreview.FontFamily=(this._fontFamily ?? new FontFamily("Segoe WP"));
        this._textBlockPreview.Foreground = (this._foreground ?? Application.Current.Resources["PhoneAlmostBlackBrush"] as Brush);
        this._textBlockPreview.TextWrapping = TextWrapping.Wrap;
        this._textBlockPreview.Width = this._verticalWidth;
        this._textBlockPreview.Text = this._text;
        this._textBlockPreview.DisableHyperlinks = this._disableHyperlinks;
        this._textBlockPreview.HideHyperlinksForeground = this._hideHyperlinksForeground;
        if (this._lineHeight > 0.0)
          this._textBlockPreview.LineHeight = this._lineHeight;
        Grid content = (Grid) ((UserControl) ((ContentControl) Application.Current.RootVisual).Content).Content;
        Canvas canvas = new Canvas();
        content.Children.Add(canvas);
        canvas.Children.Add(this._textBlockPreview);
        canvas.UpdateLayout();
        int int32 = Convert.ToInt32((this._textBlockPreview).ActualHeight / 28.0);
        if (int32 > 15)
        {
          string str = BrowserNavigationService.CutTextGently(text, 399);
          if (str != text)
          {
            this._textBlockPreview.Text = str.Trim() + "...";
            this._showReadFull = true;
            canvas.UpdateLayout();
          }
          if (int32 > 150)
            this._expandOnNewPageOnly = true;
        }
        if (this._horizontalWidth > 0.1)
          this._horizontalHeight = this._textBlockPreview.ActualHeight;
        ((UIElement) canvas).UpdateLayout();
        this._verticalHeight = ((FrameworkElement) this._textBlockPreview).ActualHeight;
        canvas.Children.Remove((UIElement) this._textBlockPreview);
        content.Children.Remove((UIElement) canvas);
        if (!this._showReadFull || !this._supportExpandText)
          return;
        this._verticalHeight = this._verticalHeight + 31.6;
      }
      else
      {
        ScrollableTextBlock textBlockFull = this._textBlockFull;
        if ((textBlockFull != null ? ((FrameworkElement) textBlockFull).Parent :  null) != null)
          return;
        ScrollableTextBlock scrollableTextBlock = new ScrollableTextBlock();
        scrollableTextBlock.TextId = this._textId;
        int num1 = 0;
        ((FrameworkElement) scrollableTextBlock).VerticalAlignment = ((VerticalAlignment) num1);
        HorizontalAlignment contentAlignment = this._horizontalContentAlignment;
        scrollableTextBlock.HorizontalContentAlignment = contentAlignment;
        TextAlignment textAlignment = this._textAlignment;
        scrollableTextBlock.TextAlignment = textAlignment;
        string str = text;
        scrollableTextBlock.Text = str;
        double num2 = this._fontSize == 0.0 ? 20.0 : this._fontSize;
        scrollableTextBlock.FontSize = num2;
        FontFamily fontFamily = this._fontFamily ?? new FontFamily("Segoe WP");
        scrollableTextBlock.FontFamily = fontFamily;
        Brush brush = this._foreground ?? Application.Current.Resources["PhoneAlmostBlackBrush"] as Brush;
        scrollableTextBlock.Foreground = brush;
        this._textBlockFull = scrollableTextBlock;
        this._textBlockFull.DisableHyperlinks = this._disableHyperlinks;
        this._textBlockFull.HideHyperlinksForeground = this._hideHyperlinksForeground;
        if (this._lineHeight > 0.0)
          this._textBlockFull.LineHeight = this._lineHeight;
        if (this._horizontalWidth > 0.1)
          ((FrameworkElement) this._textBlockFull).Width = this._horizontalWidth;
        Grid content = (Grid) ((UserControl) ((ContentControl) Application.Current.RootVisual).Content).Content;
        Canvas canvas = new Canvas();
        content.Children.Add((UIElement) canvas);
        canvas.Children.Add((UIElement) this._textBlockFull);
        ((UIElement) canvas).UpdateLayout();
        if (this._horizontalWidth > 0.1)
          this._horizontalHeight = ((FrameworkElement) this._textBlockFull).ActualHeight;
        ((FrameworkElement) this._textBlockFull).Width = this._verticalWidth;
        ((UIElement) canvas).UpdateLayout();
        this._verticalHeight = ((FrameworkElement) this._textBlockFull).ActualHeight;
        ((PresentationFrameworkCollection<UIElement>) ((Panel) canvas).Children).Remove((UIElement) this._textBlockFull);
        ((PresentationFrameworkCollection<UIElement>) ((Panel) content).Children).Remove((UIElement) canvas);
      }
    }

    protected override void GenerateChildren()
    {
      if (string.IsNullOrEmpty(this._text))
        return;
      if (this._preview)
      {
        base.Children.Add(this._textBlockPreview);
      }
      else
      {
        this.UpdateLayout();
        base.Children.Add(this._textBlockFull);
      }
      if (!this._showReadFull || !this._supportExpandText)
        return;
      Border border1 = new Border();
      Thickness thickness1 = new Thickness(-8.0, this._verticalHeight - 31.6, -8.0, -8.0);
      ((FrameworkElement) border1).Margin = thickness1;
      SolidColorBrush solidColorBrush1 = new SolidColorBrush(Colors.Transparent);
      border1.Background = ((Brush) solidColorBrush1);
      Border border2 = border1;
      TextBlock textBlock1 = new TextBlock();
      Thickness thickness2 = new Thickness(8.0);
      ((FrameworkElement) textBlock1).Margin = thickness2;
      SolidColorBrush solidColorBrush2 = Application.Current.Resources["PhoneDarkBlueBrush"] as SolidColorBrush;
      textBlock1.Foreground = ((Brush) solidColorBrush2);
      FontWeight semiBold = FontWeights.SemiBold;
      textBlock1.FontWeight = semiBold;
      string str = string.Format("{0}...", CommonResources.ExpandText);
      textBlock1.Text = str;
      TextBlock textBlock2 = textBlock1;
      if (this._fontSize > 0.0)
        textBlock2.FontSize = this._fontSize;
      if (this._fontFamily != null)
        textBlock2.FontFamily = this._fontFamily;
      border2.Child = ((UIElement) textBlock2);
      ((UIElement) border2).Tap += (new EventHandler<System.Windows.Input.GestureEventArgs>(this.TextBlockReadFull_OnTap));
      base.Children.Add(border2);
      MetroInMotion.SetTilt(border2, 1.5);
    }

    public FrameworkElement GetView()
    {
      if (!this._preview)
        return this._textBlockFull;
      return this._textBlockPreview;
    }

    private void TextBlockReadFull_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      if (this._expandOnNewPageOnly)
        return;
      e.Handled = true;
      this._preview = false;
      this._showReadFull = false;
      this.MeasureText();
      base.RegenerateChildren();
      base.NotifyHeightChanged();
      StatsEventsTracker.Instance.Handle(new PostActionEvent()
      {
        PostId = this._parentPostId,
        ActionType = PostActionType.Expanded
      });
    }
  }
}
