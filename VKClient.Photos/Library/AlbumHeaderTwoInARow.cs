using System;
using System.Windows;
using VKClient.Common.Framework;
using VKClient.Common.Library;

namespace VKClient.Photos.Library
{
  public class AlbumHeaderTwoInARow : ViewModelBase, IItemsCount
  {
    private AlbumHeader _albumHeader1;
    private AlbumHeader _albumHeader2;

    public AlbumHeader AlbumHeader1
    {
      get
      {
        return this._albumHeader1;
      }
      set
      {
        this._albumHeader1 = value;
        this.NotifyPropertyChanged<AlbumHeader>((System.Linq.Expressions.Expression<Func<AlbumHeader>>) (() => this.AlbumHeader1));
        this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>) (() => this.HaveFirstHeader));
      }
    }

    public AlbumHeader AlbumHeader2
    {
      get
      {
        return this._albumHeader2;
      }
      set
      {
        this._albumHeader2 = value;
        this.NotifyPropertyChanged<AlbumHeader>((System.Linq.Expressions.Expression<Func<AlbumHeader>>) (() => this.AlbumHeader2));
        this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>) (() => this.HaveSecondHeader));
      }
    }

    public Visibility HaveSecondHeader
    {
      get
      {
        return this.AlbumHeader2 == null ? Visibility.Collapsed : Visibility.Visible;
      }
    }

    public Visibility HaveFirstHeader
    {
      get
      {
        return this.AlbumHeader1 == null ? Visibility.Collapsed : Visibility.Visible;
      }
    }

    public bool IsMenuEnabled
    {
      get
      {
        if (this.AlbumHeader1 != null)
          return this.AlbumHeader1.AlbumType == AlbumType.NormalAlbum;
        return false;
      }
    }

    public bool IsMenu2Enabled
    {
      get
      {
        if (this.AlbumHeader2 != null)
          return this.AlbumHeader2.AlbumType == AlbumType.NormalAlbum;
        return false;
      }
    }

    internal void DeleteAlbumHeader(AlbumHeader album)
    {
      if (album == this.AlbumHeader1)
      {
        this.AlbumHeader1 = (AlbumHeader) null;
      }
      else
      {
        if (album != this.AlbumHeader2)
          return;
        this.AlbumHeader2 = (AlbumHeader) null;
      }
    }

    public int GetItemsCount()
    {
      int num = 0;
      if (this.AlbumHeader1 != null)
        ++num;
      if (this.AlbumHeader2 != null)
        ++num;
      return num;
    }
  }
}
