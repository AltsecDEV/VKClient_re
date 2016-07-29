using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace VKClient.Common.UC.Registration
{
  public class RegistrationStep5UC : UserControl
  {
    internal Grid LayoutRoot;
    internal FriendsSearchUC ucFriendsSearch;
    private bool _contentLoaded;

    public RegistrationStep5UC()
    {
      this.InitializeComponent();
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/VKClient.Common;component/UC/Registration/RegistrationStep5UC.xaml", UriKind.Relative));
      this.LayoutRoot = (Grid) this.FindName("LayoutRoot");
      this.ucFriendsSearch = (FriendsSearchUC) this.FindName("ucFriendsSearch");
    }
  }
}
