using GachaRecord.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace GachaRecord.Model;

public abstract class GachaDataBase<B, T>
    where B : GachaDataBase<B, T>, IGacha
{
    public abstract Task<string> TryFetchAndGetUID();
    public abstract IAsyncEnumerable<IList<T>> LoadGachaDataAsync(string gachaType);
    public abstract string GachaTypeKey(T val);
    public abstract string GachaItemIdKey(T val);
    public abstract string GachaRankTypeKey(T val);
    public abstract string GachaTimeKey(T val);
    public abstract string GachaNameKey(T val);

    internal readonly Dictionary<string, Dictionary<string, List<T>>> database = [];

    public record GameUigf(
        string Uid,
        int Timezone,
        string Lang,
        IList<T> List
    );

    public void Import(JsonElement uigf)
    {
        var datas = uigf.Deserialize<IList<GameUigf>>(Utils.JsonOpt);
        if (datas != null && datas.Any())
        {
            foreach (var data in datas)
            {
                var newerData = data.List.GroupBy(GachaTypeKey).Select(items => (items.Key, items.OrderBy(GachaItemIdKey)));
                if (database.TryGetValue(data.Uid, out var uidDatas) && uidDatas != null)
                {
                    foreach (var (type, newlist) in newerData)
                    {
                        if (uidDatas.TryGetValue(type, out var oldlist) && oldlist != null)
                        {
                            uidDatas[type] = [.. oldlist!.UnionBy(newlist!, GachaItemIdKey)];
                        }
                        else
                        {
                            uidDatas.Add(type, [.. newlist!]);
                        }
                    }
                }
                else
                {
                    database.Add(data.Uid, newerData.Select(x => (x.Key, x.Item2.ToList())).ToDictionary());
                }
            }
        }
    }

    public async Task Update(bool fullUpdate = false, Action<string> processMessage = null)
    {
        var uid = await TryFetchAndGetUID();
        var hasUidData = database.TryGetValue(uid, out var uiddata);
        foreach (var (gachaType, gachaName) in ((B)this).GachaTypes)
        {
            List<T> olddata = null;
            var hasold = uiddata?.TryGetValue(gachaType, out olddata) ?? false;
            hasold &= olddata?.Count != 0;
            var lastId = hasold ? GachaItemIdKey(olddata!.Last()) : null;
            List<T> newlist = [];
            int page = 1;
            await foreach (var list in LoadGachaDataAsync(gachaType))
            {
                processMessage?.Invoke($"抓取{gachaName}第{page++}页");
                newlist.AddRange(list);
                if (!fullUpdate && lastId != null && list.Any(x => GachaItemIdKey(x) == lastId))
                {
                    break;
                }
            }
            if (newlist.Count != 0)
            {
                var orderlist = newlist.OrderBy(GachaItemIdKey);
                if (!hasUidData)
                {
                    database.Add(uid, []);
                    hasUidData = true;
                    uiddata = database[uid];
                }
                if (!hasold)
                {
                    uiddata!.Add(gachaType, [.. orderlist]);
                }
                else
                {
                    uiddata![gachaType] = olddata!.UnionBy(orderlist, GachaItemIdKey).ToList();
                }
            }
            await Task.Delay(1000);
        }
    }

    public JsonElement Export()
    {
        var uigfs = database.Select(kp => new GameUigf(kp.Key, 8, "zh-cn", [.. kp.Value.SelectMany(x => x.Value).OrderBy(GachaItemIdKey)])).ToList();
        return JsonSerializer.SerializeToElement(uigfs, Utils.JsonOpt);
    }

    public virtual (string Rank5, string Rank4, string Rank3) RankTypeStr() => ("5", "4", "3");

    public IDictionary<string, GachaWishShowData> GachaShowWishes(string uid)
    {
        var (Rank5, Rank4, Rank3) = RankTypeStr();
        Dictionary<string, GachaWishShowData> datas = [];
        var gachaTypes = ((IGacha)this).GachaTypes;
        if (database.TryGetValue(uid, out var uidDatas))
        {
            foreach (var (gacha, list) in uidDatas)
            {
                if (list.Count == 0)
                {
                    continue;
                }
                var cnt = list.Count;
                var rank3Count = list.Where(x => GachaRankTypeKey(x) == Rank3).Count();
                var rank4Count = list.Where(x => GachaRankTypeKey(x) == Rank4).Count();
                int lastIndex = -1;
                var rank5List = list.Select((item, index) => (item, index)).Where(x => GachaRankTypeKey(x.item) == Rank5);
                List<GachaWishShowData.GachaItem> showRank5List = [];
                foreach (var (item, index) in rank5List)
                {
                    int gachaCnt = index - lastIndex;
                    showRank5List.Add(new(GachaNameKey(item), GachaTimeKey(item), gachaCnt));
                    lastIndex = index;
                }
                var afterLastRank5Count = cnt - 1 - lastIndex;
                showRank5List.Reverse();
                GachaWishShowData gachaWish = new()
                {
                    WishName = gachaTypes[gacha],
                    TotalGachaCount = cnt,
                    Rank3GachaCount = rank3Count,
                    Rank4GachaCount = rank4Count,
                    AfterLastRank5GachaCount = afterLastRank5Count,
                    RecordBeginDate = DateTime.Parse(GachaTimeKey(list.First())),
                    RecordEndDate = DateTime.Parse(GachaTimeKey(list.Last())),
                    Rank5List = showRank5List,
                };
                datas.Add(gacha, gachaWish);
            }
        }
        foreach (var (gacha, name) in gachaTypes)
        {
            if (!datas.ContainsKey(gacha))
            {
                datas.Add(gacha, new GachaWishShowData()
                {
                    WishName = gachaTypes[gacha],
                    TotalGachaCount = 0,
                    Rank3GachaCount = 0,
                    Rank4GachaCount = 0,
                    AfterLastRank5GachaCount = 0,
                    RecordBeginDate = DateTime.Now,
                    RecordEndDate = DateTime.Now,
                    Rank5List = [],
                });
            }
        }
        return datas;
    }

    public IList<string> Uids => [.. database.Keys];

    public void ClearData()
    {
        database.Clear();
    }
}
