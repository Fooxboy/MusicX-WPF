using System.Security.Claims;
using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicX.Shared;

namespace MusicX.Server.Controllers;

[Route("metrics")]
[Authorize]
public class MetricsController(IWriteApiAsync writeApi) : Controller
{
    [HttpPost("{eventName}/report")]
    public async Task Report(string eventName, [FromBody] ReportMetricRequest metricRequest)
    {
        var userId = User.Claims.Single(b => b.Type == ClaimTypes.NameIdentifier).Value;

        var point = PointData.Measurement(eventName)
            .Tag("userId", userId)
            .Tag("version", metricRequest.AppVersion)
            .Field("value", 1)
            .Timestamp(DateTime.Now, WritePrecision.Ms);
        
        if (!string.IsNullOrEmpty(metricRequest.Source))
            point = point.Tag("source", metricRequest.Source);
        
        await writeApi.WritePointAsync(point);
    }
}