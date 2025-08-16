using System;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Foundation;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace GachaRecord.UI;

public sealed partial class HorizontalEqualPanel : Panel
{
    private Size effectiveSize;

    public static readonly DependencyProperty SpacingProperty = 
        DependencyProperty.Register("Spacing", typeof(double), typeof(HorizontalEqualPanel), null);

    public double Spacing
    {
        get { return (double)GetValue(SpacingProperty); }
        set { SetValue(SpacingProperty, value); }
    }

    public static readonly DependencyProperty MinItemWidthProperty =
        DependencyProperty.Register("MinItemWidth", typeof(double), typeof(HorizontalEqualPanel), null);

    public double MinItemWidth
    {
        get { return (double)GetValue(MinItemWidthProperty); }
        set { SetValue(MinItemWidthProperty, value); }
    }

    public HorizontalEqualPanel()
    {
        EffectiveViewportChanged += OnEffectiveViewportChanged;
        Unloaded += OnUnloaded;
    }

    public static double GetTotalLength(double itemLength, int itemsCount, double spacing)
    {
        return Math.Max(0, (itemLength + spacing) * itemsCount - spacing);
    }

    public static double GetItemLength(double totalLength, int itemsCount, double spacing)
    {
        return Math.Max(0, (totalLength + spacing) / itemsCount - spacing);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        ReadOnlySpan<UIElement> visibleItems = [.. Children.Where(child => child.Visibility is Visibility.Visible)];
        int visibleItemsCount = visibleItems.Length;

        if (visibleItemsCount <= 0)
        {
            return default;
        }

        double minItemWidth = Math.Max(MinItemWidth, GetItemLength(effectiveSize.Width, visibleItemsCount, Spacing));

        foreach (ref readonly UIElement child in visibleItems)
        {
            child.Measure(new(minItemWidth, effectiveSize.Height));
        }

        return new(Math.Max(effectiveSize.Width, GetTotalLength(minItemWidth, visibleItemsCount, Spacing)), effectiveSize.Height);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        ReadOnlySpan<UIElement> visibleItems = [.. Children.Where(child => child.Visibility is Visibility.Visible)];
        double availableItemWidth = GetItemLength(finalSize.Width, visibleItems.Length, Spacing);
        double actualItemWidth = Math.Max(MinItemWidth, availableItemWidth);

        double offset = 0;
        foreach (ref readonly UIElement visibleChild in visibleItems)
        {
            visibleChild.Arrange(new(offset, 0, actualItemWidth, effectiveSize.Height));
            offset += actualItemWidth + Spacing;
        }

        return new Size(Math.Max(0, offset - Spacing), effectiveSize.Height);
    }

    private void OnEffectiveViewportChanged(FrameworkElement sender, EffectiveViewportChangedEventArgs args)
    {
        if (args.EffectiveViewport.IsEmpty)
        {
            return;
        }

        effectiveSize.Height = args.EffectiveViewport.Bottom - args.EffectiveViewport.Top;
        effectiveSize.Width = args.EffectiveViewport.Right - args.EffectiveViewport.Left;
        effectiveSize.Width -= Margin.Left + Margin.Right;
        effectiveSize.Height -= Margin.Top + Margin.Bottom;

        InvalidateMeasure();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        EffectiveViewportChanged -= OnEffectiveViewportChanged;
        Unloaded -= OnUnloaded;
    }
}
