using DashboardApp.Models;
using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DashboardApp.Repositories;

public class SalesRepository
{
    private readonly string _connectionString;

    public SalesRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    // ==================== KPI ====================
    public async Task<decimal> GetTotalRevenueAsync(FilterSettings filter)
    {
        const string sql = @"
            SELECT COALESCE(SUM(amount), 0) 
            FROM sales 
            WHERE sale_date BETWEEN @from AND @to
              AND (@categoryId IS NULL OR category_id = @categoryId)
              AND (@regionId IS NULL OR region_id = @regionId)";

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(sql, conn);

        cmd.Parameters.AddWithValue("from", filter.FromDate);
        cmd.Parameters.AddWithValue("to", filter.ToDate);

        var catParam = cmd.Parameters.Add("categoryId", NpgsqlDbType.Integer);
        catParam.Value = filter.CategoryId.HasValue ? filter.CategoryId.Value : DBNull.Value;

        var regParam = cmd.Parameters.Add("regionId", NpgsqlDbType.Integer);
        regParam.Value = filter.RegionId.HasValue ? filter.RegionId.Value : DBNull.Value;

        var result = await cmd.ExecuteScalarAsync();
        return Convert.ToDecimal(result);
    }

    public async Task<int> GetOrderCountAsync(FilterSettings filter)
    {
        const string sql = @"
            SELECT COUNT(*) 
            FROM sales 
            WHERE sale_date BETWEEN @from AND @to
              AND (@categoryId IS NULL OR category_id = @categoryId)
              AND (@regionId IS NULL OR region_id = @regionId)";

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(sql, conn);

        cmd.Parameters.AddWithValue("from", filter.FromDate);
        cmd.Parameters.AddWithValue("to", filter.ToDate);

        var catParam = cmd.Parameters.Add("categoryId", NpgsqlDbType.Integer);
        catParam.Value = filter.CategoryId.HasValue ? filter.CategoryId.Value : DBNull.Value;

        var regParam = cmd.Parameters.Add("regionId", NpgsqlDbType.Integer);
        regParam.Value = filter.RegionId.HasValue ? filter.RegionId.Value : DBNull.Value;

        var result = await cmd.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    public async Task<decimal> GetAverageCheckAsync(FilterSettings filter)
    {
        const string sql = @"
            SELECT COALESCE(AVG(amount), 0) 
            FROM sales 
            WHERE sale_date BETWEEN @from AND @to
              AND (@categoryId IS NULL OR category_id = @categoryId)
              AND (@regionId IS NULL OR region_id = @regionId)";

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(sql, conn);

        cmd.Parameters.AddWithValue("from", filter.FromDate);
        cmd.Parameters.AddWithValue("to", filter.ToDate);

        var catParam = cmd.Parameters.Add("categoryId", NpgsqlDbType.Integer);
        catParam.Value = filter.CategoryId.HasValue ? filter.CategoryId.Value : DBNull.Value;

        var regParam = cmd.Parameters.Add("regionId", NpgsqlDbType.Integer);
        regParam.Value = filter.RegionId.HasValue ? filter.RegionId.Value : DBNull.Value;

        var result = await cmd.ExecuteScalarAsync();
        return Convert.ToDecimal(result);
    }

    // ==================== Фильтры ====================
    public async Task<List<CategoryItem>> GetCategoriesAsync()
    {
        const string sql = "SELECT id, name FROM categories ORDER BY name";
        var list = new List<CategoryItem>();

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(sql, conn);
        await using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            list.Add(new CategoryItem { Id = reader.GetInt32(0), Name = reader.GetString(1) });
        }
        return list;
    }

    public async Task<List<RegionItem>> GetRegionsAsync()
    {
        const string sql = "SELECT id, name FROM regions ORDER BY name";
        var list = new List<RegionItem>();

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(sql, conn);
        await using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            list.Add(new RegionItem { Id = reader.GetInt32(0), Name = reader.GetString(1) });
        }
        return list;
    }

    // ==================== Графики ====================
    public async Task<List<CategoryRevenue>> GetRevenueByCategoryAsync(FilterSettings filter)
    {
        const string sql = @"
            SELECT c.name, COALESCE(SUM(s.amount), 0) as revenue
            FROM categories c
            LEFT JOIN sales s ON s.category_id = c.id 
                AND s.sale_date BETWEEN @from AND @to
                AND (@categoryId IS NULL OR s.category_id = @categoryId)
                AND (@regionId IS NULL OR s.region_id = @regionId)
            GROUP BY c.name
            ORDER BY revenue DESC";

        var list = new List<CategoryRevenue>();

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(sql, conn);

        cmd.Parameters.AddWithValue("from", filter.FromDate);
        cmd.Parameters.AddWithValue("to", filter.ToDate);

        var catParam = cmd.Parameters.Add("categoryId", NpgsqlDbType.Integer);
        catParam.Value = filter.CategoryId.HasValue ? filter.CategoryId.Value : DBNull.Value;

        var regParam = cmd.Parameters.Add("regionId", NpgsqlDbType.Integer);
        regParam.Value = filter.RegionId.HasValue ? filter.RegionId.Value : DBNull.Value;

        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            list.Add(new CategoryRevenue
            {
                CategoryName = reader.GetString(0),
                Revenue = reader.GetDecimal(1)
            });
        }
        return list;
    }
    public async Task<List<CategorySummary>> GetCategorySummaryAsync(FilterSettings filter)
    {
        string sql = @"
        SELECT c.name, 
               COALESCE(SUM(s.amount), 0) as total_revenue,
               COUNT(*) as order_count
        FROM categories c
        LEFT JOIN sales s ON s.category_id = c.id 
            AND s.sale_date BETWEEN @from AND @to
            AND (@categoryId IS NULL OR s.category_id = @categoryId)
            AND (@regionId IS NULL OR s.region_id = @regionId)
        GROUP BY c.name
        ORDER BY total_revenue DESC";

        var list = new List<CategorySummary>();

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand(sql, conn);

        cmd.Parameters.AddWithValue("from", filter.FromDate);
        cmd.Parameters.AddWithValue("to", filter.ToDate);

        var catParam = cmd.Parameters.Add("categoryId", NpgsqlDbType.Integer);
        catParam.Value = filter.CategoryId.HasValue ? filter.CategoryId.Value : DBNull.Value;

        var regParam = cmd.Parameters.Add("regionId", NpgsqlDbType.Integer);
        regParam.Value = filter.RegionId.HasValue ? filter.RegionId.Value : DBNull.Value;

        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            list.Add(new CategorySummary
            {
                CategoryName = reader.GetString(0),
                TotalRevenue = reader.GetDecimal(1),
                OrderCount = reader.GetInt32(2)
            });
        }
        return list;
    }
}