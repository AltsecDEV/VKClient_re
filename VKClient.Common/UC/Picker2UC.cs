using Microsoft.Phone.Controls;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace VKClient.Common.UC
{
  public class Picker2UC : UserControl
  {
    internal Grid LayoutRoot;
    internal LongListSelector listPicker;
    private bool _contentLoaded;

    public Action<object> OnItemTap { get; set; }

    public Picker2UC()
    {
      this.InitializeComponent();
    }

    private void Grid_Tap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      if (this.OnItemTap == null)
        return;
      this.OnItemTap((sender as FrameworkElement).DataContext);
    }

    private void Border_Tap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      if (this.OnItemTap == null)
        return;
      this.OnItemTap(null);
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/VKClient.Common;component/UC/Picker2UC.xaml", UriKind.Relative));
      this.LayoutRoot = (Grid) this.FindName("LayoutRoot");
      this.listPicker = (LongListSelector) this.FindName("listPicker");
    }
  }
}
