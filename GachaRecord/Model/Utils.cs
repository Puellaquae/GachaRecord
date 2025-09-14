using System;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using Windows.Storage.Pickers;

namespace GachaRecord.Model;

public partial class Utils
{
    public static JsonSerializerOptions JsonOpt { get; } = new JsonSerializerOptions()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    [GeneratedRegex("^[1-9]+?\\.[0-9]+?\\.[0-9]+?\\.[0-9]+?$")]
    public static partial Regex VersionRegex();

    public static NameValueCollection GetQueryFromMihoyoWebCacheLog(DirectoryInfo webCacheFolder, ReadOnlySpan<byte> urlPattern)
    {
        var cacheFolder = webCacheFolder.EnumerateDirectories()
                .Where(dir => VersionRegex().IsMatch(dir.Name))
                .MaxBy(dir => dir.LastWriteTime);
        var cacheFile = Path.Combine(cacheFolder!.FullName, @"Cache\Cache_Data\data_2");
        var tmpFile = Path.GetTempFileName();
        File.Copy(cacheFile, tmpFile, true);
        using FileStream reader = new(tmpFile, FileMode.Open, FileAccess.Read, FileShare.None, 4096, FileOptions.DeleteOnClose);
        using MemoryStream ms = new();
        reader.CopyTo(ms);
        ReadOnlySpan<byte> cache = ms.ToArray();
        int idx = cache.LastIndexOf(urlPattern);
        int end = cache[idx..].IndexOf("\0"u8);
        var url = Encoding.UTF8.GetString(cache.Slice(idx, end));
        Uri uri = new($"https://{url}");
        return HttpUtility.ParseQueryString(uri.Query);
    }

    public static async Task<string> PickFile(params string[] fileTypes)
    {
        var openPicker = new FileOpenPicker();

        // See the sample code below for how to make the window accessible from the App class.
        var window = App.MainWindow;

        // Retrieve the window handle (HWND) of the current WinUI 3 window.
        var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);

        // Initialize the file picker with the window handle (HWND).
        WinRT.Interop.InitializeWithWindow.Initialize(openPicker, hWnd);

        // Set options for your file picker
        openPicker.ViewMode = PickerViewMode.Thumbnail;
        foreach (var fileType in fileTypes)
        {
            openPicker.FileTypeFilter.Add(fileType);
        }

        // Open the picker for the user to pick a file
        var file = await openPicker.PickSingleFileAsync();
        return file?.Path;
    }

    public static async Task<string> PickSaveFile(string suggestName)
    {
        var savePicker = new FileSavePicker();

        // See the sample code below for how to make the window accessible from the App class.
        var window = App.MainWindow;

        // Retrieve the window handle (HWND) of the current WinUI 3 window.
        var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);

        // Initialize the file picker with the window handle (HWND).
        WinRT.Interop.InitializeWithWindow.Initialize(savePicker, hWnd);

        // Set options for your file picker
        savePicker.FileTypeChoices.Add("JSON", [".json"]);
        savePicker.SuggestedFileName = suggestName;

        // Open the picker for the user to pick a file
        var file = await savePicker.PickSaveFileAsync();
        return file?.Path;
    }

    public static async Task<string> PickFolder(string buttonText)
    {
        var openPicker = new FolderPicker();

        // See the sample code below for how to make the window accessible from the App class.
        var window = App.MainWindow;

        // Retrieve the window handle (HWND) of the current WinUI 3 window.
        var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);

        // Initialize the file picker with the window handle (HWND).
        WinRT.Interop.InitializeWithWindow.Initialize(openPicker, hWnd);

        // Set options for your file picker
        openPicker.CommitButtonText = buttonText;

        // Open the picker for the user to pick a file
        var file = await openPicker.PickSingleFolderAsync();
        return file?.Path;
    }
}
