using GachaRecord.ViewModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace GachaRecord.Model;

public class MainData
{
    private readonly Dictionary<string, IGacha> gachaGames;
    private readonly string storageFilePath;
    private bool debugMode = false;

    public MainData(string storagePath, params IGacha[] games)
    {
        storageFilePath = storagePath;
        gachaGames = games.Select(x => (x.Id, x)).ToDictionary();
        LoadFromStorage();
    }

    public class Storage
    {
        [JsonPropertyName("config")]
        public Dictionary<string, JsonElement> Config { get; set; }
        [JsonExtensionData]
        public Dictionary<string, JsonElement> Games { get; set; }
    }

    public void LoadFromStorage()
    {
        if (File.Exists(storageFilePath))
        {
            using FileStream f = File.OpenRead(storageFilePath);
            var data = JsonSerializer.Deserialize<Storage>(f);
            foreach (var (name, value) in data.Games)
            {
                if (gachaGames.TryGetValue(name, out IGacha game))
                {
                    game.Import(value);
                }
            }
            foreach (var (name, value) in data.Config)
            {
                if (gachaGames.TryGetValue(name, out IGacha game))
                {
                    game.Config = value;
                }
            }
        }
    }

    public void SaveToStorage()
    {
        if (debugMode) { return; }
        Storage storage = new()
        {
            Config = [],
            Games = []
        };
        foreach (var (name, game) in gachaGames)
        {
            storage.Config.Add(name, game.Config);
            storage.Games.Add(name, game.Export());
        }
        File.WriteAllBytes(storageFilePath, JsonSerializer.SerializeToUtf8Bytes(storage));
    }

    public bool HasGameId(string gameId)
    {
        return gachaGames.ContainsKey(gameId);
    }

    public string GameName(string gameId)
    {
        return gachaGames[gameId].Name;
    }

    public class Uigf
    {
        [JsonPropertyName("info")]
        public Dictionary<string, object> Info { get; set; }
        [JsonExtensionData]
        public Dictionary<string, JsonElement> Games { get; set; }
    }

    public event Action<string> DataUpdated;

    public async Task Import(string gameId, string filePath)
    {
        using FileStream f = File.OpenRead(filePath);
        var data = await JsonSerializer.DeserializeAsync<Uigf>(f);
        if (data?.Games != null && data.Games.TryGetValue(gameId.ToLower(), out JsonElement value))
        {
            gachaGames[gameId].Import(value);
        }

        DataUpdated.Invoke(gameId);
        SaveToStorage();
    }

    public async Task Export(string gameId, string filePath)
    {
        Uigf uigf = new()
        {
            Info = [],
            Games = [],
        };
        uigf.Info.Add("export_timestamp", new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds());
        uigf.Info.Add("export_app", "GachaRecord");
        uigf.Info.Add("export_app_version", "v1.0.0");
        uigf.Info.Add("version", "v4.0");
        var json = gachaGames[gameId].Export();
        uigf.Games.Add(gameId.ToLower(), json);
        await File.WriteAllBytesAsync(filePath, JsonSerializer.SerializeToUtf8Bytes(uigf, Utils.JsonOpt));
    }

    public async Task Update(string gameId, bool fullUpdate, Action<string> message)
    {
        await gachaGames[gameId].Update(fullUpdate, message);
        DataUpdated.Invoke(gameId);
        SaveToStorage();
    }

    public IDictionary<string, GachaWishShowData> GachaWishShows(string gameId, string uid)
    {
        return gachaGames[gameId].GachaShowWishes(uid);
    }

    public IList<string> GetUids(string gameId)
    {
        return gachaGames[gameId].Uids;
    }

    public void SwitchDebug(bool onDebugMode)
    {
        if (debugMode && !onDebugMode)
        {
            foreach (var game in gachaGames.Values)
            {
                game.ClearData();
                game.ClearConfig();
            }
            LoadFromStorage();
            foreach (var gameId in gachaGames.Keys)
            {
                DataUpdated.Invoke(gameId);
            }
        }
        else if (!debugMode && onDebugMode)
        {
            foreach (var (gameId, game) in gachaGames)
            {
                game.ClearData();
                game.ClearConfig();
                DataUpdated.Invoke(gameId);
            }
        }
        debugMode = onDebugMode;
    }

    public async Task ClearData(string gameId)
    {
        ContentDialog dialog = new()
        {
            XamlRoot = App.MainWindow.Content.XamlRoot,
            Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
            Title = $"是否删除{GameName(gameId)}抽卡记录数据",
            PrimaryButtonText = "删除",
            CloseButtonText = "取消",
            DefaultButton = ContentDialogButton.Close
        };

        var result = await dialog.ShowAsync();

        if (result == ContentDialogResult.Primary)
        {
            gachaGames[gameId].ClearData();
            DataUpdated.Invoke(gameId);
            SaveToStorage();
        }
    }
}
