using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Localization;
using VKClient.Common.UC;

namespace VKClient.Common
{
  public class SettingsPrivacyPage : PageBase
  {
    private bool _isInitialized;
    internal Grid LayoutRoot;
    internal GenericHeaderUC Header;
    internal ExtendedLongListSelector mainList;
    private bool _contentLoaded;

    public SettingsPrivacyViewModel VM
    {
      get
      {
        return this.DataContext as SettingsPrivacyViewModel;
      }
    }

    public SettingsPrivacyPage()
    {
      this.InitializeComponent();
      this.Header.textBlockTitle.Text = CommonResources.Privacy_Title.ToUpperInvariant();
    }

    protected override void HandleOnNavigatedTo(NavigationEventArgs e)
    {
      base.HandleOnNavigatedTo(e);
      if (this._isInitialized)
        return;
      SettingsPrivacyViewModel privacyViewModel = new SettingsPrivacyViewModel();
      this.DataContext = (object) privacyViewModel;
      privacyViewModel.PrivacyCollection.LoadData(false, false, null, false);
      this._isInitialized = true;
    }

    private void PrivacyTap(object sender, GestureEventArgs e)
    {
      FrameworkElement frameworkElement = e.OriginalSource as FrameworkElement;
      if (frameworkElement == null)
        return;
      EditPrivacyViewModel vm = frameworkElement.DataContext as EditPrivacyViewModel;
      if (vm == null)
        return;
      Navigator.Current.NavigateToEditPrivacy(new EditPrivacyPageInputData()
      {
        PrivacyForEdit = vm,
        UpdatePrivacyCallback = (Action<PrivacyInfo>) (pi => this.VM.UpdatePrivacy(vm, pi))
      });
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/VKClient.Common;component/SettingsPrivacyPage.xaml", UriKind.Relative));
      this.LayoutRoot = (Grid) this.FindName("LayoutRoot");
      this.Header = (GenericHeaderUC) this.FindName("Header");
      this.mainList = (ExtendedLongListSelector) this.FindName("mainList");
    }
  }
}
