using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace GachaRecord.Model;

public partial class NapGacha : GachaDataBase<NapGacha, NapGacha.MiGachaItem>, IGacha
{
    public string Id => "Nap";
    public string Name => "绝区零";

    public IDictionary<string, string> GachaTypes => new Dictionary<string, string>() {
        { "1", "常驻频段" },
        { "2", "独家频段" },
        { "3", "音擎频段" },
        { "5", "邦布频段" },
    };

    public record MiGachaItem(
        string Uid,
        string GachaId,
        string GachaType,
        string ItemId,
        string Count,
        string Time,
        string Name,
        string Lang,
        string ItemType,
        string RankType,
        string Id
    );

    public override string GachaTypeKey(MiGachaItem val) { return val.GachaType; }
    public override string GachaItemIdKey(MiGachaItem val) { return val.Id; }
    public override string GachaRankTypeKey(MiGachaItem val) { return val.RankType; }
    public override string GachaTimeKey(MiGachaItem val) { return val.Time; }
    public override string GachaNameKey(MiGachaItem val) { return val.Name; }
    public override (string Rank5, string Rank4, string Rank3) RankTypeStr() => ("4", "3", "2");

    public class ConfigType
    {
        public string GamePath { get; set; }
    }

    private readonly ConfigType config = new();

    public JsonElement Config
    {
        get
        {
            return JsonSerializer.SerializeToElement(config);
        }
        set
        {
            var newConfig = value.Deserialize<ConfigType>();
            if (newConfig != null && newConfig.GamePath != null)
            {
                config.GamePath = newConfig.GamePath;
            }
        }
    }

    public void ClearConfig()
    {
        config.GamePath = null;
    }

    public override async IAsyncEnumerable<IList<MiGachaItem>> LoadGachaDataAsync(string gachaTypeId)
    {
        string qureyparams = await GetQueryParamsAsync();
        HttpClient httpClient = new();
        int page = 1;
        string end_id = "0";
        while (true)
        {
            string qurl = $"https://public-operation-nap.mihoyo.com/common/gacha_record/api/getGachaLog?{qureyparams}&real_gacha_type={gachaTypeId}&page={page}&size=20&end_id={end_id}";
            MiQueryResult res = await FetchLog(httpClient, qurl);
            if (res.Data == null || res.Data.List.Count == 0) { yield break; }
            yield return res.Data.List;
            end_id = res.Data.List.Last().Id;
            page++;
            await Task.Delay(page % 10 == 0 ? 1000 : 500);
        }
    }

    public override async Task<string> TryFetchAndGetUID()
    {
        HttpClient httpClient = new();
        string qureyparams = await GetQueryParamsAsync();
        foreach (var gachaTypeId in GachaTypes.Keys)
        {
            string qurl = $"https://public-operation-nap.mihoyo.com/common/gacha_record/api/getGachaLog?{qureyparams}&real_gacha_type={gachaTypeId}&page=1&size=1&end_id=0";
            MiQueryResult res = await FetchLog(httpClient, qurl);
            if (res.Data == null || res.Data.List.Count == 0) { continue; }
            return res.Data.List.First().Uid;
        }
        throw new Exception("All Empty Gacha History");
    }

    public class MiQueryResult
    {
        public int Retcode { get; set; }
        public string Message { get; set; }
        public MiGachaData Data { get; set; }
    }

    public class MiGachaData
    {
        public required string Page { get; set; }
        public required string Size { get; set; }
        public required IList<MiGachaItem> List { get; set; }
        public string Region { get; set; }
        public int RegionTimeZone { get; set; }
    }

    [GeneratedRegex("\\w:\\/.*?\\/ZenlessZoneZero_Data\\/")]
    private static partial Regex GameLocaleRegex();

    static string DetectGameLocale()
    {
        var pathname = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "AppData", "LocalLow", "miHoYo", "绝区零", "Player.log");
        if (File.Exists(pathname))
        {
            var log = File.ReadAllText(pathname);
            var match = GameLocaleRegex().Match(log);
            if (match.Success)
            {
                return Path.GetFullPath(Path.Combine(match.Value, ".."));
            }
        }
        return null;
    }

    public async Task<string> GetQueryParamsAsync()
    {
        if (config.GamePath == null)
        {
            var path = DetectGameLocale();
            if (path != null)
            {
                config.GamePath = path;
            }
            else
            {
                var pathManual = await Utils.PickFolder("选择游戏安装目录");
                if (pathManual != null)
                {
                    config.GamePath = pathManual;
                }
                else
                {
                    throw new Exception("Require Set Game Installed Path");
                }
            }
        }
        DirectoryInfo webCacheFolder = new($@"{config.GamePath}\ZenlessZoneZero_Data\webCaches");
        if (webCacheFolder.Exists)
        {
            var queries = Utils.GetQueryFromMihoyoWebCacheLog(webCacheFolder, "public-operation-nap.mihoyo.com/common/gacha_record/api/getGachaLog"u8);
            queries.Remove("page");
            queries.Remove("size");
            queries.Remove("gacha_type");
            queries.Remove("real_gacha_type");
            queries.Remove("end_id");
            return queries.ToString()!;
        }
        throw new Exception("Not Found WebCache Dir");
    }

    private static async Task<MiQueryResult> FetchLog(HttpClient client, string qurl, int retryCnt = 5)
    {
        try
        {
            MiQueryResult res = await client.GetFromJsonAsync<MiQueryResult>(qurl, Utils.JsonOpt);
            if (res != null && res.Retcode == -1)
            {
                retryCnt = 0;
                throw new Exception("AuthKey Timeout");
            }
            return res!;
        }
        catch (Exception)
        {
            if (retryCnt > 0)
            {
                await Task.Delay(5000);
                return await FetchLog(client, qurl, retryCnt - 1);
            }
            else
            {
                throw;
            }
        }
    }
}
