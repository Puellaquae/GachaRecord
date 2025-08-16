using GachaRecord.Model;
using GachaRecord.ViewModel;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using System;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace GachaRecord
{

    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public MainData MainData { get; }

        public Hk4eViewModel Hk4eViewModel { get; }
        public HkrpgViewModel HkrpgViewModel { get; }
        public NapViewModel NapViewModel { get; }

        public MainWindow()
        {
            InitializeComponent();
            Title = "GachaRecord";
            ExtendsContentIntoTitleBar = true;
            AppWindow.TitleBar.PreferredHeightOption = TitleBarHeightOption.Tall;
            MainData = new MainData("data.json", new Hk4eGacha(), new HkrpgGacha(), new NapGacha());
            Hk4eViewModel = new Hk4eViewModel(MainData);
            HkrpgViewModel = new HkrpgViewModel(MainData);
            NapViewModel = new NapViewModel(MainData);
        }

        private int previousSelectedIndex = 0;

        private void GameSelectorChanged(SelectorBar sender, SelectorBarSelectionChangedEventArgs args)
        {
            SelectorBarItem selectedItem = sender.SelectedItem;
            int currentSelectedIndex = sender.Items.IndexOf(selectedItem);
            Type pageType = currentSelectedIndex switch
            {
                0 => typeof(Hk4e),
                1 => typeof(Hkrpg),
                2 => typeof(Nap),
                _ => typeof(Hk4e),
            };
            var slideNavigationTransitionEffect = currentSelectedIndex - previousSelectedIndex > 0 ? SlideNavigationTransitionEffect.FromRight : SlideNavigationTransitionEffect.FromLeft;

            ContentFrame.Navigate(pageType, null, new SlideNavigationTransitionInfo() { Effect = slideNavigationTransitionEffect });

            previousSelectedIndex = currentSelectedIndex;

        }

        private async void OnImport(object sender, RoutedEventArgs e)
        {
            var file = await Utils.PickFile(".json");
            var gameId = ContentFrame.CurrentSourcePageType.Name;
            if (MainData.HasGameId(gameId) && file != null)
            {
                await MainData.Import(gameId, file);
            }
        }

        private async void OnExport(object sender, RoutedEventArgs e)
        {
            var gameId = ContentFrame.CurrentSourcePageType.Name;
            var file = await Utils.PickSaveFile($"UIGF-{gameId.ToUpper()}-{DateTime.Now:yyyyMMdd}");
            if (MainData.HasGameId(gameId) && file != null)
            {
                await MainData.Export(gameId, file);
            }
        }

        private async void OnUpdate(object sender, RoutedEventArgs e)
        {
            var gameId = ContentFrame.CurrentSourcePageType.Name;
            if (MainData.HasGameId(gameId))
            {
                ProcessTip.Title = $"更新{MainData.GameName(gameId)}抽卡记录";
                ProcessTipBar.ShowError = false;
                ProcessTip.IsOpen = true;
                try
                {
                    await MainData.Update(gameId, false, x => ProcessTip.Subtitle = x);
                    ProcessTip.IsOpen = false;
                }
                catch (Exception ex)
                {
                    ProcessTip.Subtitle = ex.Message;
                    ProcessTipBar.ShowError = true;
                }
            }
        }

        private void OnSwitchDebugMode(object sender, RoutedEventArgs e)
        {
            if (sender is ToggleMenuFlyoutItem btn)
            {
                MainData.SwitchDebug(btn.IsChecked);
            }
        }

        private async void OnClearData(object sender, RoutedEventArgs e)
        {
            var gameId = ContentFrame.CurrentSourcePageType.Name;
            if (MainData.HasGameId(gameId))
            {
                await MainData.ClearData(gameId);
            }
        }
    }
}
