using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DashboardApp.Models;
using DashboardApp.Repositories;
using DashboardApp.Services;
using DashboardApp.Views;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace DashboardApp.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly ConfigService _configService = new();
    private SalesRepository? _salesRepository;
    private FilterSettings _currentFilter = new();

    [ObservableProperty] private string _statusMessage = "Готово";
    [ObservableProperty] private DateTime _lastUpdated = DateTime.Now;

    // KPI
    [ObservableProperty] private decimal _totalRevenue;
    [ObservableProperty] private int _orderCount;
    [ObservableProperty] private decimal _averageCheck;

    // Фильтры
    public ObservableCollection<CategoryItem> Categories { get; } = new();
    public ObservableCollection<RegionItem> Regions { get; } = new();

    [ObservableProperty] private CategoryItem? _selectedCategory;
    [ObservableProperty] private RegionItem? _selectedRegion;
    [ObservableProperty] private DateTime _fromDate = DateTime.Today.AddMonths(-3);
    [ObservableProperty] private DateTime _toDate = DateTime.Today;

    // График
    public ObservableCollection<ISeries> RevenueByCategorySeries { get; } = new();

    public MainViewModel()
    {
        _ = InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        try
        {
            string connStr = _configService.GetConnectionString();
            _salesRepository = new SalesRepository(connStr);

            await LoadFilterListsAsync();
            await LoadDashboardDataAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка: {ex.Message}";
        }
    }

    private async Task LoadFilterListsAsync()
    {
        if (_salesRepository == null) return;

        var cats = await _salesRepository.GetCategoriesAsync();
        var regs = await _salesRepository.GetRegionsAsync();

        Categories.Clear();
        foreach (var c in cats) Categories.Add(c);

        Regions.Clear();
        foreach (var r in regs) Regions.Add(r);
    }

    private async Task LoadDashboardDataAsync()
    {
        if (_salesRepository == null) return;

        StatusMessage = "Загрузка данных...";

        _currentFilter.FromDate = FromDate;
        _currentFilter.ToDate = ToDate;
        _currentFilter.CategoryId = SelectedCategory?.Id;
        _currentFilter.RegionId = SelectedRegion?.Id;

        TotalRevenue = await _salesRepository.GetTotalRevenueAsync(_currentFilter);
        OrderCount = await _salesRepository.GetOrderCountAsync(_currentFilter);
        AverageCheck = await _salesRepository.GetAverageCheckAsync(_currentFilter);

        await LoadChartsDataAsync();

        LastUpdated = DateTime.Now;
        StatusMessage = "Данные обновлены";
    }

    private async Task LoadChartsDataAsync()
    {
        if (_salesRepository == null) return;

        var data = await _salesRepository.GetRevenueByCategoryAsync(_currentFilter);

        RevenueByCategorySeries.Clear();

        var series = new ColumnSeries<decimal>
        {
            Values = data.Select(x => x.Revenue).ToArray(),
            Name = "Выручка"
        };

        RevenueByCategorySeries.Add(series);
    }

    private async Task ReinitializeDataAsync()
    {
        try
        {
            string connStr = _configService.GetConnectionString();
            _salesRepository = new SalesRepository(connStr);
            await LoadFilterListsAsync();
            await LoadDashboardDataAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка: {ex.Message}";
        }
    }

    [RelayCommand] private async Task ApplyFiltersAsync() => await LoadDashboardDataAsync();
    [RelayCommand]
    private async Task ResetFiltersAsync()
    {
        FromDate = DateTime.Today.AddMonths(-3);
        ToDate = DateTime.Today;
        SelectedCategory = null;
        SelectedRegion = null;
        await LoadDashboardDataAsync();
    }
    [RelayCommand] private async Task RefreshDataAsync() => await LoadDashboardDataAsync();

    [RelayCommand]
    private void OpenConnectionSettings()
    {
        var configService = new ConfigService();
        var vm = new ConnectionViewModel(configService);
        var dialog = new ConnectionDialog();
        dialog.DataContext = vm;

        bool? result = dialog.ShowDialog();
        if (result == true || vm.DialogResult)
        {
            StatusMessage = "Настройки сохранены";
            _ = ReinitializeDataAsync();
        }
    }
}