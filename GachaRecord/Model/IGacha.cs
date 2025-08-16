using GachaRecord.ViewModel;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace GachaRecord.Model;

public interface IGacha
{
    public string Id { get; }
    public string Name { get; }
    public IDictionary<string, string> GachaTypes { get; }
    public IDictionary<string, GachaWishShowData> GachaShowWishes(string uid);
    public IList<string> Uids { get; }

    public JsonElement Config { get; set; }
    public void ClearConfig();

    public void Import(JsonElement uigf);
    public JsonElement Export();
    public Task Update(bool fullUpdate = false, Action<string> processMessage = null);

    public void ClearData();
}
