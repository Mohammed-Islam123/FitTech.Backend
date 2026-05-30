using Aggregation.Shared;
using Carter;
using ErrorOr;
using Microsoft.AspNetCore.Mvc;

namespace Aggregation.Features.Reports.DownloadExcelReport;

public class DownloadExcelReportEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/reports/excel", Handle)
            .WithName("DownloadExcelReport")
            .WithTags("Reports")
            .WithDescription("Generates and downloads an Excel report with revenue, users, and monthly breakdown.")
            .RequireAuthorization("AdminOnly")
            .Produces(StatusCodes.Status200OK, contentType: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
    }

    private static async Task<IResult> Handle(
        [FromServices] DownloadExcelReportHandler handler, CancellationToken ct)
    {
        var result = await handler.Handle(ct);
        return result.Match(
            bytes => Results.File(bytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"FitTech_Report_{DateTime.UtcNow:yyyyMMdd}.xlsx"),
            errors => ErrorOnExtensions.MapErrorsToResult(errors));
    }
}
