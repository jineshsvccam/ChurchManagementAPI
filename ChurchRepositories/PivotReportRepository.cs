using System.Data;
using ChurchContracts;
using ChurchData;
using ChurchDTOs.DTOs.Utils;
using Dapper;
using Microsoft.EntityFrameworkCore;

namespace ChurchRepositories
{
    public class PivotReportRepository : IPivotReportRepository, ISingleHeadFiscalReportRepository, IMonthlyFiscalReportRepository
    {
        private readonly ApplicationDbContext _context;

        public PivotReportRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PivotReportResult> GetPivotReportAsync(
             int parishId,
             int year,
             string type,
             int[]? headIds = null,
             int? headCount = null)
        {
            var connection = _context.Database.GetDbConnection();
            try
            {
                if (connection.State != ConnectionState.Open)
                    await connection.OpenAsync();

                var rawRows = await connection.QueryAsync<RawPivotReport>(
                    @"SELECT * FROM public.get_pivot_report(@in_parish_id, @in_fyear, @in_type, @in_head_ids, @in_head_count)",
                    new
                    {
                        in_parish_id = parishId,
                        in_fyear = year,
                        in_type = type,
                        in_head_ids = headIds,
                        in_head_count = headCount
                    });

                var resultData = rawRows
                    .Select(r => new PivotReportDto
                    {
                        HeadId = r.HeadId,
                        HeadName = r.HeadName,
                        MonthlyAmounts = new decimal[]
                        {
                    r.Apr, r.May, r.Jun, r.Jul, r.Aug, r.Sep,
                    r.Oct, r.Nov, r.Dec, r.Jan, r.Feb, r.Mar
                        },
                        Total = r.Total,
                        Percentage = r.Percentage
                    })
                    .OrderBy(r => r.HeadName == "Others" ? 1 : 0)
                    .ThenByDescending(r => r.Percentage)
                    .ToList();

                var result = new PivotReportResult
                {
                    ParishId = parishId,
                    FiscalYear = year,
                    Type = type,
                    HeadIds = headIds,
                    ReportName = $"Pivot Report - {type} - {year}",
                    Data = resultData
                };

                return result;
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                    await connection.CloseAsync();
            }
        }

        public async Task<SingleHeadFiscalReportDto> GetSingleHeadFiscalReportAsync(int parishId, int headId, string type, int startYear, int endYear)
        {
            var connection = _context.Database.GetDbConnection();
            try
            {
                if (connection.State != ConnectionState.Open)
                    await connection.OpenAsync();

                // Step 1: Query the function (use SELECT for a function)
                var rawRows = await connection.QueryAsync<RawMonthlyFiscalReport>(
                    @"SELECT * FROM public.get_single_head_fiscal_report(@in_parish_id, @in_head_id, @in_type, @in_start_fyear, @in_end_fyear)",
                    new
                    {
                        in_parish_id = parishId,
                        in_head_id = headId,
                        in_type = type,
                        in_start_fyear = startYear,
                        in_end_fyear = endYear
                    });

                // Step 2: Transform into target DTO
                var response = new SingleHeadFiscalReportDto
                {
                    HeadId = headId,
                    HeadName = "", // You can fill this if you query the name separately
                    Type=type,
                    ReportName="Single Head Report",
                    FiscalYears = rawRows.Select(r => new FiscalYearData
                    {
                        Year = r.Fyear,
                        MonthlyAmounts = new decimal[]
                        {
                    r.Apr, r.May, r.Jun, r.Jul, r.Aug, r.Sep,
                    r.Oct, r.Nov, r.Dec, r.Jan, r.Feb, r.Mar
                        },
                        Total = r.Total
                    }).ToList()
                };

                return response;
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                    await connection.CloseAsync();
            }
        }

        public async Task<MonthlyFiscalReportResponse> GetMonthlyFiscalReportAsync(int parishId, int startYear, int endYear)
        {
            var connection = _context.Database.GetDbConnection();
            try
            {
                if (connection.State != ConnectionState.Open)
                    await connection.OpenAsync();

                // Step 1: Fetch Income
                var incomeRows = await connection.QueryAsync<RawMonthlyFiscalReport>(
                    @"SELECT * FROM public.get_monthly_fiscal_report(@in_parish_id, @in_type, @in_start_fyear, @in_end_fyear)",
                    new
                    {
                        in_parish_id = parishId,
                        in_type = "Income",
                        in_start_fyear = startYear,
                        in_end_fyear = endYear
                    });

                // Step 2: Fetch Expense
                var expenseRows = await connection.QueryAsync<RawMonthlyFiscalReport>(
                    @"SELECT * FROM public.get_monthly_fiscal_report(@in_parish_id, @in_type, @in_start_fyear, @in_end_fyear)",
                    new
                    {
                        in_parish_id = parishId,
                        in_type = "Expense",
                        in_start_fyear = startYear,
                        in_end_fyear = endYear
                    });

                // Step 3: Merge
                var response = new MonthlyFiscalReportResponse();

                foreach (var income in incomeRows)
                {
                    var expense = expenseRows.FirstOrDefault(e => e.Fyear == income.Fyear);

                    var monthlyData = new MonthlyIncomeExpense[12];

                    monthlyData[0] = new MonthlyIncomeExpense { Income = income.Apr, Expense = expense?.Apr ?? 0 };
                    monthlyData[1] = new MonthlyIncomeExpense { Income = income.May, Expense = expense?.May ?? 0 };
                    monthlyData[2] = new MonthlyIncomeExpense { Income = income.Jun, Expense = expense?.Jun ?? 0 };
                    monthlyData[3] = new MonthlyIncomeExpense { Income = income.Jul, Expense = expense?.Jul ?? 0 };
                    monthlyData[4] = new MonthlyIncomeExpense { Income = income.Aug, Expense = expense?.Aug ?? 0 };
                    monthlyData[5] = new MonthlyIncomeExpense { Income = income.Sep, Expense = expense?.Sep ?? 0 };
                    monthlyData[6] = new MonthlyIncomeExpense { Income = income.Oct, Expense = expense?.Oct ?? 0 };
                    monthlyData[7] = new MonthlyIncomeExpense { Income = income.Nov, Expense = expense?.Nov ?? 0 };
                    monthlyData[8] = new MonthlyIncomeExpense { Income = income.Dec, Expense = expense?.Dec ?? 0 };
                    monthlyData[9] = new MonthlyIncomeExpense { Income = income.Jan, Expense = expense?.Jan ?? 0 };
                    monthlyData[10] = new MonthlyIncomeExpense { Income = income.Feb, Expense = expense?.Feb ?? 0 };
                    monthlyData[11] = new MonthlyIncomeExpense { Income = income.Mar, Expense = expense?.Mar ?? 0 };

                    response.FiscalYears.Add(new FiscalYearIncomeExpenseData
                    {
                        Year = income.Fyear,
                        MonthlyData = monthlyData,
                        TotalIncome = income.Total,
                        TotalExpense = expense?.Total ?? 0
                    });
                }

                response.ParishId = parishId;
                response.StartYear = startYear;
                response.EndYear = endYear;
                response.ReportName = "Income vs Expense Summary";

                return response;
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                    await connection.CloseAsync();
            }
        }

    }
}
