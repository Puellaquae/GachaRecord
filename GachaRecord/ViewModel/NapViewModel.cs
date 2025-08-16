using GachaRecord.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace GachaRecord.ViewModel;

public partial class NapViewModel : INotifyPropertyChanged
{
    private GachaWishShowData _standardWish;
    public GachaWishShowData StandardWish => _standardWish;
    private GachaWishShowData _characterWish;
    public GachaWishShowData CharacterWish => _characterWish;
    private GachaWishShowData _weaponWish;
    public GachaWishShowData WeaponWish => _weaponWish;
    private GachaWishShowData _bangbooWish;
    public GachaWishShowData BangbooWish => _bangbooWish;

    public string CurrentUid { get; set; }
    private readonly string gameId = "Nap";

    private readonly MainData _mainData;

    public NapViewModel(MainData mainData)
    {
        _mainData = mainData;
        _mainData.DataUpdated += GachaDataUpdated;
        GachaDataUpdated(gameId);
    }

    private void GachaDataUpdated(string obj)
    {
        if (obj == gameId)
        {
            CurrentUid ??= _mainData.GetUids(gameId).FirstOrDefault();

            if (CurrentUid != null)
            {
                var wish = _mainData.GachaWishShows(gameId, CurrentUid);
                if (wish.TryGetValue("1", out _standardWish))
                {
                    OnPropertyChanged(nameof(StandardWish));
                }
                if (wish.TryGetValue("2", out _characterWish))
                {
                    OnPropertyChanged(nameof(CharacterWish));
                }
                if (wish.TryGetValue("3", out _weaponWish))
                {
                    OnPropertyChanged(nameof(WeaponWish));
                }
                if (wish.TryGetValue("5", out _bangbooWish))
                {
                    OnPropertyChanged(nameof(BangbooWish));
                }
            }
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
