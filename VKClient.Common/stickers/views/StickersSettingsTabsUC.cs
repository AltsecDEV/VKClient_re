using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VKClient.Common.Stickers.ViewModels;

namespace VKClient.Common.Stickers.Views
{
  public class StickersSettingsTabsUC : UserControl
  {
    private bool _contentLoaded;

    private StickersManageViewModel ViewModel
    {
      get
      {
        return base.DataContext as StickersManageViewModel;
      }
    }

    public StickersSettingsTabsUC()
    {
      //base.\u002Ector();
      this.InitializeComponent();
    }

    private void TabActive_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      this.ViewModel.StickersListSource = StickersManageViewModel.CurrentSource.Active;
    }

    private void TabHidden_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      this.ViewModel.StickersListSource = StickersManageViewModel.CurrentSource.Hidden;
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/Stickers/Views/StickersSettingsTabsUC.xaml", UriKind.Relative));
    }
  }
}
