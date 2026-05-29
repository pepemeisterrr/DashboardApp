using System;

namespace DashboardApp.Models;

public class FilterSettings
{
    public DateTime FromDate { get; set; } = DateTime.Today.AddMonths(-1);
    public DateTime ToDate { get; set; } = DateTime.Today;
    public int? CategoryId { get; set; } = null;
    public int? RegionId { get; set; } = null;
}