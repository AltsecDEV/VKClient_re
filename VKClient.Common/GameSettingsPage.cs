using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Navigation;
using VKClient.Common.Framework;
using VKClient.Common.Library.Games;
using VKClient.Common.Localization;
using VKClient.Common.UC;

namespace VKClient.Common
{
  public class GameSettingsPage : PageBase
  {
    private bool _isInitialized;
    internal GenericHeaderUC ucHeader;
    private bool _contentLoaded;

    private GameSettingsViewModel VM
    {
      get
      {
        return this.DataContext as GameSettingsViewModel;
      }
    }

    public GameSettingsPage()
    {
      this.InitializeComponent();
      this.ucHeader.textBlockTitle.Text = CommonResources.PageTitle_Games_Game;
    }

    protected override void HandleOnNavigatedTo(NavigationEventArgs e)
    {
      base.HandleOnNavigatedTo(e);
      if (this._isInitialized)
        return;
      long result = 0;
      if (this.NavigationContext.QueryString.ContainsKey("GameId"))
        long.TryParse(this.NavigationContext.QueryString["GameId"], out result);
      GameSettingsViewModel settingsViewModel = new GameSettingsViewModel(result);
      settingsViewModel.LoadGameInfo();
      this.DataContext = (object) settingsViewModel;
      this._isInitialized = true;
    }

    private void DisconnectButton_OnClicked(object sender, RoutedEventArgs e)
    {
      this.VM.DisconnectGame();
      Navigator.Current.GoBack();
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/VKClient.Common;component/GameSettingsPage.xaml", UriKind.Relative));
      this.ucHeader = (GenericHeaderUC) this.FindName("ucHeader");
    }
  }
}
