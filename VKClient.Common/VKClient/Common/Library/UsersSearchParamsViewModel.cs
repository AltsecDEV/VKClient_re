using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using VKClient.Audio.Base.Events;
using VKClient.Audio.Base.Library;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Localization;
using VKClient.Common.Utils;

using VKClient.Audio.Base.Extensions;

namespace VKClient.Common.Library
{
  public class UsersSearchParamsViewModel : ViewModelBase
  {
    private static readonly List<BGType> _relationshipTypesListMale;
    private static readonly List<BGType> _relationshipTypesListFemale;
    private static readonly City _defaultCity;
    private static readonly Country _defaultCountry;
    private const int MinAge = 14;
    private const int MaxAge = 80;
    private const int DefaultMinAge = 18;
    private const int DefaultMaxAge = 24;
    private List<AgeType> _agesFrom;
    private AgeType _ageFromSelected;
    private List<AgeType> _agesTo;
    private AgeType _ageToSelected;
    private readonly SearchParams _searchParams;

    public List<BGType> RelationshipTypes
    {
      get
      {
        if (!this.IsFemale)
          return UsersSearchParamsViewModel._relationshipTypesListMale;
        return UsersSearchParamsViewModel._relationshipTypesListFemale;
      }
    }

    public BGType RelationshipType
    {
      get
      {
        return this.RelationshipTypes.FirstOrDefault<BGType>((Func<BGType, bool>) (t => t.id == this._searchParams.GetValue<int>("status")));
      }
      set
      {
        this._searchParams.SetValue<int>("status", value == null ? 0 : value.id, false);
        this.NotifyPropertyChanged("RelationshipType");
      }
    }

    public Country Country
    {
      get
      {
        return this._searchParams.GetValue<Country>("country") ?? UsersSearchParamsViewModel._defaultCountry;
      }
      set
      {
        Country country = this._searchParams.GetValue<Country>("country");
        if (country != null && value != null && country.id == value.id)
          return;
        this._searchParams.SetValue<Country>("country", value, value == null || value.id == 0L);
        this.NotifyPropertyChanged("Country");
        this.City = (City) null;
        this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>) (() => this.CitySelectorVisibility));
      }
    }

    public Visibility CitySelectorVisibility
    {
      get
      {
        return this.Country.id <= 0L ? Visibility.Collapsed : Visibility.Visible;
      }
    }

    public City City
    {
      get
      {
        return this._searchParams.GetValue<City>("city") ?? UsersSearchParamsViewModel._defaultCity;
      }
      set
      {
        this._searchParams.SetValue<City>("city", value, value == null || value.id == 0L);
        this._searchParams.SetValue<City>("city", value, false);
        this.NotifyPropertyChanged("City");
      }
    }

    public int Sex
    {
      get
      {
        return this._searchParams.GetValue<int>("sex");
      }
      set
      {
        this._searchParams.SetValue<int>("sex", value, false);
        this.UpdateRelationshipTypes();
      }
    }

    public bool IsAnySex
    {
      get
      {
        return this.Sex < 1;
      }
      set
      {
        if (!value)
          return;
        this.Sex = 0;
      }
    }

    public bool IsFemale
    {
      get
      {
        return this.Sex == 1;
      }
      set
      {
        if (!value)
          return;
        this.Sex = 1;
      }
    }

    public bool IsMale
    {
      get
      {
        return this.Sex == 2;
      }
      set
      {
        if (!value)
          return;
        this.Sex = 2;
      }
    }

    public List<AgeType> AgesFrom
    {
      get
      {
        return this._agesFrom;
      }
    }

    public AgeType AgeFromSelected
    {
      get
      {
        if (!this._agesFrom.Contains(this._ageFromSelected))
          return this._ageFromSelected = this._agesFrom.First<AgeType>();
        return this._ageFromSelected;
      }
      set
      {
        this._ageFromSelected = value;
        this.NotifyPropertyChanged("AgeFromSelected");
        if (value != null)
          this._searchParams.SetValue<int>("age_from", value.Age, this.IsAnyAge);
        this.UpdateAgesTo();
      }
    }

    public List<AgeType> AgesTo
    {
      get
      {
        return this._agesTo;
      }
    }

    public AgeType AgeToSelected
    {
      get
      {
        if (!this._agesTo.Contains(this._ageToSelected))
          return this._ageToSelected = this._agesTo.First<AgeType>();
        return this._ageToSelected;
      }
      set
      {
        this._ageToSelected = value;
        this.NotifyPropertyChanged("AgeToSelected");
        if (value == null)
          return;
        this._searchParams.SetValue<int>("age_to", value.Age, this.IsAnyAge);
      }
    }

    public bool IsAnyAge
    {
      get
      {
        return !this._searchParams.GetValue<bool>("DisableAnyAge");
      }
      set
      {
        if (value)
        {
          this._searchParams.ResetValue("DisableAnyAge");
          this._searchParams.ResetValue("age_from");
          this._searchParams.ResetValue("age_to");
        }
        else
        {
          this._searchParams.SetValue<bool>("DisableAnyAge", true, false);
          this._searchParams.SetValue<int>("age_from", 18, false);
          this._searchParams.SetValue<int>("age_to", 24, false);
        }
      }
    }

    public bool IsWithPhoto
    {
      get
      {
        return this._searchParams.GetValue<bool>("has_photo");
      }
      set
      {
        this._searchParams.SetValue<bool>("has_photo", value, false);
      }
    }

    public bool IsOnlineNow
    {
      get
      {
        return this._searchParams.GetValue<bool>("online");
      }
      set
      {
        this._searchParams.SetValue<bool>("online", value, false);
      }
    }

    static UsersSearchParamsViewModel()
    {
      List<BGType> bgTypeList1 = new List<BGType>();
      BGType bgType1 = new BGType();
      bgType1.id = 0;
      string lowerInvariant1 = CommonResources.Settings_EditProfile_NoneSelected.ToLowerInvariant();
      bgType1.name = lowerInvariant1;
      bgTypeList1.Add(bgType1);
      BGType bgType2 = new BGType();
      bgType2.id = 1;
      string lowerInvariant2 = CommonResources.Relationship_Single_Male.ToLowerInvariant();
      bgType2.name = lowerInvariant2;
      bgTypeList1.Add(bgType2);
      BGType bgType3 = new BGType();
      bgType3.id = 2;
      string lowerInvariant3 = CommonResources.Relationship_InARelationship_Male.ToLowerInvariant();
      bgType3.name = lowerInvariant3;
      bgTypeList1.Add(bgType3);
      BGType bgType4 = new BGType();
      bgType4.id = 3;
      string lowerInvariant4 = CommonResources.Relationship_Engaged_Male.ToLowerInvariant();
      bgType4.name = lowerInvariant4;
      bgTypeList1.Add(bgType4);
      BGType bgType5 = new BGType();
      bgType5.id = 4;
      string lowerInvariant5 = CommonResources.Relationship_Married_Male.ToLowerInvariant();
      bgType5.name = lowerInvariant5;
      bgTypeList1.Add(bgType5);
      BGType bgType6 = new BGType();
      bgType6.id = 5;
      string lowerInvariant6 = CommonResources.Relationship_ItIsComplicated.ToLowerInvariant();
      bgType6.name = lowerInvariant6;
      bgTypeList1.Add(bgType6);
      BGType bgType7 = new BGType();
      bgType7.id = 6;
      string lowerInvariant7 = CommonResources.Relationship_ActivelySearching.ToLowerInvariant();
      bgType7.name = lowerInvariant7;
      bgTypeList1.Add(bgType7);
      BGType bgType8 = new BGType();
      bgType8.id = 7;
      string lowerInvariant8 = CommonResources.Relationship_InLove_Male.ToLowerInvariant();
      bgType8.name = lowerInvariant8;
      bgTypeList1.Add(bgType8);
      UsersSearchParamsViewModel._relationshipTypesListMale = bgTypeList1;
      List<BGType> bgTypeList2 = new List<BGType>();
      BGType bgType9 = new BGType();
      bgType9.id = 0;
      string lowerInvariant9 = CommonResources.Settings_EditProfile_NoneSelected.ToLowerInvariant();
      bgType9.name = lowerInvariant9;
      bgTypeList2.Add(bgType9);
      BGType bgType10 = new BGType();
      bgType10.id = 1;
      string lowerInvariant10 = CommonResources.Relationship_Single_Female.ToLowerInvariant();
      bgType10.name = lowerInvariant10;
      bgTypeList2.Add(bgType10);
      BGType bgType11 = new BGType();
      bgType11.id = 2;
      string lowerInvariant11 = CommonResources.Relationship_InARelationship_Female.ToLowerInvariant();
      bgType11.name = lowerInvariant11;
      bgTypeList2.Add(bgType11);
      BGType bgType12 = new BGType();
      bgType12.id = 3;
      string lowerInvariant12 = CommonResources.Relationship_Engaged_Female.ToLowerInvariant();
      bgType12.name = lowerInvariant12;
      bgTypeList2.Add(bgType12);
      BGType bgType13 = new BGType();
      bgType13.id = 4;
      string lowerInvariant13 = CommonResources.Relationship_Married_Female.ToLowerInvariant();
      bgType13.name = lowerInvariant13;
      bgTypeList2.Add(bgType13);
      BGType bgType14 = new BGType();
      bgType14.id = 5;
      string lowerInvariant14 = CommonResources.Relationship_ItIsComplicated.ToLowerInvariant();
      bgType14.name = lowerInvariant14;
      bgTypeList2.Add(bgType14);
      BGType bgType15 = new BGType();
      bgType15.id = 6;
      string lowerInvariant15 = CommonResources.Relationship_ActivelySearching.ToLowerInvariant();
      bgType15.name = lowerInvariant15;
      bgTypeList2.Add(bgType15);
      BGType bgType16 = new BGType();
      bgType16.id = 7;
      string lowerInvariant16 = CommonResources.Relationship_InLove_Female.ToLowerInvariant();
      bgType16.name = lowerInvariant16;
      bgTypeList2.Add(bgType16);
      UsersSearchParamsViewModel._relationshipTypesListFemale = bgTypeList2;
      UsersSearchParamsViewModel._defaultCity = new City()
      {
        id = 0L,
        title = CommonResources.Settings_EditProfile_NoneSelected.ToLowerInvariant()
      };
      UsersSearchParamsViewModel._defaultCountry = new Country()
      {
        id = 0L,
        title = CommonResources.Settings_EditProfile_NoneSelected.ToLowerInvariant()
      };
    }

    public UsersSearchParamsViewModel(SearchParams searchParams = null)
    {
      if (searchParams == null)
      {
        searchParams = new SearchParams();
        this._searchParams = searchParams;
        this.SetDefaultValues();
      }
      else
      {
        this._searchParams = searchParams;
        if (!this.IsAnyAge)
        {
          int minAge = searchParams.GetValue<int>("age_from");
          int maxAge = searchParams.GetValue<int>("age_to");
          if (minAge == 0)
            minAge = 18;
          if (maxAge == 0)
            maxAge = 24;
          this.InitAges(minAge, maxAge);
        }
        else
          this.InitAges(18, 24);
      }
    }

    private void UpdateRelationshipTypes()
    {
      int relTypeId = this.RelationshipType.id;
      this.NotifyPropertyChanged<List<BGType>>((System.Linq.Expressions.Expression<Func<List<BGType>>>) (() => this.RelationshipTypes));
      this.RelationshipType = this.RelationshipTypes.FirstOrDefault<BGType>((Func<BGType, bool>) (type => type.id == relTypeId));
    }

    public static string ToPrettyString(SearchParams searchParams)
    {
      List<string> stringList = new List<string>();
      int sex1 = searchParams.GetValue<int>("sex");
      if (sex1 > 0)
      {
        VKClient.Common.Backend.DataObjects.Sex sex2 = (VKClient.Common.Backend.DataObjects.Sex) sex1;
        stringList.Add(sex2.GetSexStr());
      }
      int num1 = !searchParams.GetValue<bool>("DisableAnyAge") ? 1 : 0;
      int number1 = searchParams.GetValue<int>("age_from");
      int number2 = searchParams.GetValue<int>("age_to");
      if (num1 == 0 && number1 > 0 && (number2 > 0 && number2 > number1))
      {
        string str1 = UIStringFormatterHelper.FormatNumberOfSomething(number1, CommonResources.OneFromAgeFrm, CommonResources.TwoFourFromAgeFrm, CommonResources.FiveFromAgeFrm, true, null, false);
        string str2 = UIStringFormatterHelper.FormatNumberOfSomething(number2, CommonResources.OneToAgeFrm, CommonResources.TwoFourToAgeFrm, CommonResources.FiveToAgeFrm, true, null, false);
        stringList.Add(string.Format("{0} {1}", (object) str1, (object) str2));
      }
      int num2 = searchParams.GetValue<int>("status");
      if (num2 > 0)
      {
        RelationshipStatus status = (RelationshipStatus) num2;
        stringList.Add(status.GetRelationshipStr(sex1).ToLowerInvariant());
      }
      Country country = searchParams.GetValue<Country>("country");
      if (country != null && country.id > 0L)
        stringList.Add(country.title);
      City city = searchParams.GetValue<City>("city");
      if (city != null && city.id > 0L)
        stringList.Add(city.name);
      if (searchParams.GetValue<bool>("has_photo"))
        stringList.Add(CommonResources.UsersSearch_WithPhoto.ToLowerInvariant());
      if (searchParams.GetValue<bool>("online"))
        stringList.Add(CommonResources.UsersSearch_OnlineNow.ToLowerInvariant());
      return string.Join(", ", (IEnumerable<string>) stringList).Capitalize();
    }

    public void Save()
    {
      EventAggregator.Current.Publish((object) new SearchParamsUpdated(this._searchParams));
    }

    private void SetDefaultValues()
    {
      this.Country = UsersSearchParamsViewModel._defaultCountry;
      this.City = UsersSearchParamsViewModel._defaultCity;
      this.Sex = 0;
      this.RelationshipType = this.RelationshipTypes.First<BGType>();
      this.IsAnyAge = true;
      this.InitAges(18, 24);
    }

    private void InitAges(int minAge = 18, int maxAge = 24)
    {
      this._agesFrom = new List<AgeType>();
      for (int age = 14; age <= 80; ++age)
        this._agesFrom.Add(new AgeType(CommonResources.UsersSearch_AgeFrom, age));
      this.AgeFromSelected = this._agesFrom.FirstOrDefault<AgeType>((Func<AgeType, bool>) (a => a.Age == minAge));
      this.NotifyPropertyChanged<List<AgeType>>((System.Linq.Expressions.Expression<Func<List<AgeType>>>) (() => this.AgesFrom));
      this.NotifyPropertyChanged<AgeType>((System.Linq.Expressions.Expression<Func<AgeType>>) (() => this.AgeFromSelected));
      this.UpdateAgesTo();
      this.AgeToSelected = this._agesTo.First<AgeType>((Func<AgeType, bool>) (a => a.Age == maxAge));
    }

    private void UpdateAgesTo()
    {
      int age1 = this._ageFromSelected.Age;
      this._agesTo = new List<AgeType>();
      for (int age2 = age1; age2 <= 80; ++age2)
        this._agesTo.Add(new AgeType(CommonResources.UsersSearch_AgeTo, age2));
      this.AgeToSelected = this._ageToSelected == null || this._ageToSelected.Age < age1 ? this._agesTo.First<AgeType>() : this._agesTo.First<AgeType>((Func<AgeType, bool>) (a => a.Age == this._ageToSelected.Age));
      this.NotifyPropertyChanged<List<AgeType>>((System.Linq.Expressions.Expression<Func<List<AgeType>>>) (() => this.AgesTo));
      this.NotifyPropertyChanged<AgeType>((System.Linq.Expressions.Expression<Func<AgeType>>) (() => this.AgeToSelected));
    }
  }
}
