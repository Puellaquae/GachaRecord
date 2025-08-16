using GachaRecord.Model;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace GachaRecord.ViewModel;

public partial class Hk4eViewModel : INotifyPropertyChanged
{
    private GachaWishShowData _standardWish;
    public GachaWishShowData StandardWish => _standardWish;
    private GachaWishShowData _newbieWish;
    public GachaWishShowData NewbieWish => _newbieWish;
    private GachaWishShowData _characterWish;
    public GachaWishShowData CharacterWish => _characterWish;
    private GachaWishShowData _weaponWish;
    public GachaWishShowData WeaponWish => _weaponWish;
    private GachaWishShowData _mixWish;
    public GachaWishShowData MixWish => _mixWish;

    public string CurrentUid { get; set; }
    private readonly string gameId = "Hk4e";

    private readonly MainData _mainData;

    public Hk4eViewModel(MainData mainData)
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
                if (wish.TryGetValue("200", out _standardWish))
                {
                    OnPropertyChanged(nameof(StandardWish));
                }
                if (wish.TryGetValue("100", out _newbieWish))
                {
                    OnPropertyChanged(nameof(NewbieWish));
                }
                if (wish.TryGetValue("301", out _characterWish))
                {
                    OnPropertyChanged(nameof(CharacterWish));
                }
                if (wish.TryGetValue("302", out _weaponWish))
                {
                    OnPropertyChanged(nameof(WeaponWish));
                }
                if (wish.TryGetValue("500", out _mixWish))
                {
                    OnPropertyChanged(nameof(MixWish));
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
