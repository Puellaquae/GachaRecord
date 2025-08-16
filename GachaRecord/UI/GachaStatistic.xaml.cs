using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using GachaRecord.ViewModel;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace GachaRecord;

public sealed partial class GachaStatistic : UserControl
{
    public GachaStatistic()
    {
        InitializeComponent();

        DataContextChanged += GachaStatistic_DataContextChanged;
    }

    private void GachaStatistic_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
    {
        Visibility newVis;
        if (DataContext is GachaWishShowData data)
        {
            newVis = (data.WishName != null && data.TotalGachaCount != 0) ? Visibility.Visible : Visibility.Collapsed;
        }
        else
        {
            newVis = Visibility.Collapsed;
        }
        if (newVis != Visibility)
        {
            Visibility = newVis;
            if (Parent is UIElement elem)
            {
                elem.InvalidateMeasure();
            }
        }
    }
}
