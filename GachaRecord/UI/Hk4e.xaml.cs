using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace GachaRecord;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class Hk4e : Page
{
    public Hk4e()
    {
        InitializeComponent();
        DataContext = App.MainWindow.Hk4eViewModel;
    }
}
