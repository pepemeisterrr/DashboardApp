using System.Linq;
using System.Windows;
using System.Windows.Input;
using OxyPlot;

namespace DashboardApp;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void CategoryChart_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is not ViewModels.MainViewModel vm) return;

        var plotView = sender as OxyPlot.Wpf.PlotView;
        if (plotView?.Model == null) return;

        var position = e.GetPosition(plotView);
        var screenPoint = new ScreenPoint(position.X, position.Y);

        // Получаем результаты hit-test
        var hitResults = plotView.Model.HitTest(new HitTestArguments(screenPoint, 10));
        var hitResult = hitResults.FirstOrDefault();

        if (hitResult?.Element is OxyPlot.Series.BarSeries && hitResult.Index >= 0)
        {
            if (hitResult.Index < vm.CategorySummaries.Count)
            {
                string categoryName = vm.CategorySummaries[(int)hitResult.Index].CategoryName;
                vm.OnCategoryChartClicked(categoryName);
            }
        }
    }
}