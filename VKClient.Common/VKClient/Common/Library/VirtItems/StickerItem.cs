using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Stickers.ViewModels;

namespace VKClient.Common.Library.VirtItems
{
  public class StickerItem : VirtualizableItemBase
  {
    private readonly Sticker _sticker;
    private readonly bool _rightAlign;
    private bool _isHorizontal;
    private readonly double _horizontalWidth;
    private readonly double _verticalWidth;

    public override double FixedHeight { get { return 512.0 / 3.0; } }//

    public Thickness StickerMargin
    {
      get
      {
        if (!this._rightAlign)
          return new Thickness();
        return new Thickness(this.Width - this.FixedHeight, 0.0, 0.0, 0.0);
      }
    }

    public bool IsHorizontal
    {
      get
      {
        return this._isHorizontal;
      }
      set
      {
        if (this._isHorizontal == value)
          return;
        this._isHorizontal = value;
        this.Width = this._isHorizontal ? this._horizontalWidth : this._verticalWidth;
        ((VirtualizableItemBase) this.VirtualizableChildren.First<IVirtualizable>()).Margin = this.StickerMargin;
      }
    }

    public StickerItem(double width, Thickness thickness, Sticker sticker, bool isHorizontal = false, double horizontalWidth = 0.0, bool rightAlign = false)
      : base(width, thickness, new Thickness())
    {
      this._sticker = sticker;
      this._rightAlign = rightAlign;
      this._isHorizontal = isHorizontal;
      this._horizontalWidth = horizontalWidth;
      this._verticalWidth = width;
      if (this._isHorizontal)
        this.Width = this._horizontalWidth;
      this.CreateLayout();
      this._view.Tap += new EventHandler<GestureEventArgs>(this.View_OnTap);
    }

    private void View_OnTap(object sender, GestureEventArgs gestureEventArgs)
    {
      EventAggregator.Current.Publish((object) new StickersTapEvent((long) this._sticker.product_id, (int) this._sticker.id));
    }

    private void CreateLayout()
    {
      double fixedHeight1 = this.FixedHeight;
      string str = this._sticker.photo_256;
      if (!str.Contains("b.png"))
        str = str.Replace(".png", "b.png");
      double fixedHeight2 = this.FixedHeight;
      Thickness stickerMargin = this.StickerMargin;
      string uriStr = str;
      object local1 = null;
      string tag = "1";
      int num1 = 0;
      int num2 = 0;
      int num3 = 2;
      object local2 = null;
      double placeholderOpacity = -1.0;
      int num4 = 0;
      int num5 = 0;
      this.VirtualizableChildren.Add((IVirtualizable) new VirtualizableImage(fixedHeight1, fixedHeight2, stickerMargin, uriStr, (Action<VirtualizableImage>) local1, tag, num1 != 0, num2 != 0, (Stretch) num3, (Brush) local2, placeholderOpacity, num4 != 0, num5 != 0));
    }
  }
}
