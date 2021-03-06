using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using VKClient.Common.Library.VirtItems;

namespace VKClient.Common.VideoCatalog
{
  public class DividerSpaceUpUC : UserControlVirtualizable
  {
    internal Grid LayoutRoot;
    private bool _contentLoaded;

    public DividerSpaceUpUC()
    {
      this.InitializeComponent();
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Video;component/VideoCatalog/DividerSpaceUpUC.xaml", UriKind.Relative));
      this.LayoutRoot = (Grid) base.FindName("LayoutRoot");
    }
  }
}
