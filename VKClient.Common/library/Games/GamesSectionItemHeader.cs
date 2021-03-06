using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace VKClient.Common.Library.Games
{
  public abstract class GamesSectionItemHeader : GamesSectionItem, INotifyPropertyChanged
  {
    private string _title;

    public string Title
    {
      get
      {
        return this._title;
      }
      set
      {
        this._title = value;
        this.OnPropertyChanged("Title");
      }
    }

    public bool SupportsMore { get; private set; }

    public event PropertyChangedEventHandler PropertyChanged;

    protected GamesSectionItemHeader(GamesSectionType type, string title, bool supportsMore)
      : base(type)
    {
      this.Title = title;
      this.SupportsMore = supportsMore;
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
      // ISSUE: reference to a compiler-generated field
      PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
      if (propertyChanged == null)
        return;
      propertyChanged(this, new PropertyChangedEventArgs(propertyName));
    }
  }
}
