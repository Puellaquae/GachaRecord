using GachaRecord.Model;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace GachaRecord.ViewModel;

public partial class HkrpgViewModel : INotifyPropertyChanged
{
    private GachaWishShowData _standardWish;
    public GachaWishShowData StandardWish => _standardWish;
    private GachaWishShowData _newbieWish;
    public GachaWishShowData NewbieWish => _newbieWish;
    private GachaWishShowData _characterWish;
    public GachaWishShowData CharacterWish => _characterWish;
    private GachaWishShowData _weaponWish;
    public GachaWishShowData WeaponWish => _weaponWish;
    private GachaWishShowData _crossoverCharacterWish;
    public GachaWishShowData CrossoverCharacterWish => _crossoverCharacterWish;
    private GachaWishShowData _crossoverWeaponWish;
    public GachaWishShowData CrossoverWeaponWish => _crossoverWeaponWish;

    public string CurrentUid { get; set; }
    private readonly string gameId = "Hkrpg";

    private readonly MainData _mainData;

    public HkrpgViewModel(MainData mainData)
    {
        _mainData = mainData;
        _mainData.DataUpdated += GachaDataUpdated;
        GachaDataUpdated(gameId);
    }

    private void GachaDataUpdated(string obj)
    {
        if (obj == gameId)
        {
            var uids = _mainData.GetUids(gameId);
            CurrentUid ??= uids.FirstOrDefault();

            if (CurrentUid != null)
            {
                var wish = _mainData.GachaWishShows(gameId, CurrentUid);
                if (wish.TryGetValue("1", out _standardWish))
                {
                    OnPropertyChanged(nameof(StandardWish));
                }
                if (wish.TryGetValue("2", out _newbieWish))
                {
                    OnPropertyChanged(nameof(NewbieWish));
                }
                if (wish.TryGetValue("11", out _characterWish))
                {
                    OnPropertyChanged(nameof(CharacterWish));
                }
                if (wish.TryGetValue("12", out _weaponWish))
                {
                    OnPropertyChanged(nameof(WeaponWish));
                }
                if (wish.TryGetValue("21", out _crossoverCharacterWish))
                {
                    OnPropertyChanged(nameof(CrossoverCharacterWish));
                }
                if (wish.TryGetValue("22", out _crossoverWeaponWish))
                {
                    OnPropertyChanged(nameof(CrossoverWeaponWish));
                }
            }

            if (uids.Count == 0)
            {
                CurrentUid = null;
            }
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
