using Aggregation.Common.Security;
using Aggregation.Domain;
using ClosedXML.Excel;
using ErrorOr;
using Microsoft.EntityFrameworkCore;

namespace Aggregation.Features.Reports.DownloadExcelReport;

public class DownloadExcelReportHandler(AggregationDbContext context, IUserAccessor userAccessor)
{
    public async Task<ErrorOr<byte[]>> Handle(CancellationToken ct)
    {
        if (!userAccessor.IsAdmin)
            return Error.Unauthorized("Report.Unauthorized", "Only Administrators can download reports.");

        var stats = await context.AggregatedStats.FindAsync([1], ct);
        var breakdowns = await context.MonthlyBreakdowns
            .AsNoTracking().OrderBy(m => m.Year).ThenBy(m => m.Month).ToListAsync(ct);

        using var workbook = new XLWorkbook();
        var summary = workbook.AddWorksheet("Summary");
        summary.Cell("A1").Value = "FitTech Report";
        summary.Cell("A2").Value = "Total Revenue"; summary.Cell("B2").Value = stats?.TotalRevenue ?? 0;
        summary.Cell("A3").Value = "Total Users"; summary.Cell("B3").Value = stats?.TotalUsers ?? 0;
        summary.Cell("A4").Value = "Total Registrations"; summary.Cell("B4").Value = stats?.TotalRegistrations ?? 0;
        summary.Cell("A5").Value = "Total Subscriptions"; summary.Cell("B5").Value = stats?.TotalSubscriptions ?? 0;
        summary.Column("A").Width = 25;

        var monthly = workbook.AddWorksheet("Monthly Breakdown");
        monthly.Cell("A1").Value = "Month";
        monthly.Cell("B1").Value = "Revenue";
        monthly.Cell("C1").Value = "New Members";
        monthly.Cell("D1").Value = "Sessions";
        int row = 2;
        foreach (var m in breakdowns)
        {
            monthly.Cell(row, 1).Value = $"{m.Year}-{m.Month:D2}";
            monthly.Cell(row, 2).Value = m.Revenue;
            monthly.Cell(row, 3).Value = m.NewMembers;
            monthly.Cell(row, 4).Value = m.TotalSessions;
            row++;
        }
        monthly.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}
