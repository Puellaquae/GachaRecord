using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace GachaRecord.ViewModel;

public class GachaWishShowData
{
    public string WishName { get; set; }

    public int TotalGachaCount { get; set; }

    public int Rank5GachaCount => Rank5List.Count;
    public string AverageRank5GachaCountDisplay
    {
        get
        {
            if (Rank5List.Count == 0)
            {
                return $"0.00 抽";
            }
            return
                $"{Rank5List.Average(x => x.GachaCount):F2} 抽";
        }
    }
    public string Rank5GachaCountDisplay => $"{Rank5GachaCount} 抽";
    public string Rank5GachaCountPercentDisplay => $"{Rank5GachaCount * 100.0 / TotalGachaCount:F2}%";
    public string BestWorstRank5GachaDisplay
    {
        get
        {
            if (Rank5List.Count == 0)
            {
                return $"0/0 抽";
            }
            return $"{Rank5List.MinBy(x => x.GachaCount).GachaCount}/{Rank5List.MaxBy(x => x.GachaCount).GachaCount} 抽";
        }
    }

    public int AfterLastRank5GachaCount { get; set; }
    public string AfterLastRank5GachaCountDisplay => $"{AfterLastRank5GachaCount} 抽";
    public int Rank4GachaCount { get; set; }
    public string Rank4GachaCountDisplay => $"{Rank4GachaCount} 抽";
    public string Rank4GachaCountPercentDisplay => $"{Rank4GachaCount * 100.0 / TotalGachaCount:F2}%";
    public int Rank3GachaCount { get; set; }
    public string Rank3GachaCountDisplay => $"{Rank3GachaCount} 抽";
    public string Rank3GachaCountPercentDisplay => $"{Rank3GachaCount * 100.0 / TotalGachaCount:F2}%";

    public DateTime RecordBeginDate { get; set; }
    public DateTime RecordEndDate { get; set; }
    public string RecordRangeDisplay => $"{RecordBeginDate:yyyy.MM.dd} - {RecordEndDate:yyyy.MM.dd}";

    public record GachaItem(string Name, string Time, int GachaCount);

    public IList<GachaItem> Rank5List { get; set; }
}
