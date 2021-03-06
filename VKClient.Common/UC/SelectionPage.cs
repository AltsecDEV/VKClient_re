using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Navigation;
using VKClient.Common.Framework;

namespace VKClient.Common.UC
{
  public class SelectionPage : PageBase
  {
    private bool _contentLoaded;

    public SelectionPage()
    {
      this.InitializeComponent();
      this.SuppressMenu = true;
    }

    protected override void HandleOnNavigatedTo(NavigationEventArgs e)
    {
      base.HandleOnNavigatedTo(e);
      this.DataContext = (object) new SelectionPageViewModel((CustomListPicker) ParametersRepository.GetParameterForIdAndReset("ParentPicker"));
    }

    private void Item_OnClicked(object sender, GestureEventArgs e)
    {
      ((SelectionPageViewModel) this.DataContext).UpdateSelectedItem((SelectionPageItem) ((FrameworkElement) sender).DataContext);
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/VKClient.Common;component/UC/CustomListPicker/SelectionPage.xaml", UriKind.Relative));
    }
  }
}
