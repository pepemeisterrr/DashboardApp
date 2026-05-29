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
        if (CategoryChart.Model == null) return;

        // Получаем координаты клика относительно графика
        var position = e.GetPosition(CategoryChart);
        var hitResult = CategoryChart.Model.HitTest(new HitTestArguments(position, 10));

        if (hitResult?.Element is OxyPlot.Series.BarItem barItem)
        {
            // Определяем индекс категории
            int index = CategoryChart.Model.Series[0].Items.IndexOf(barItem);
            if (index >= 0 && index < vm.CategorySummaries.Count)
            {
                string categoryName = vm.CategorySummaries[index].CategoryName;
                vm.OnCategoryChartClicked(categoryName);
            }
        }
    }
}