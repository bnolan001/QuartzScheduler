using BNolan.AviationWx.NET.Models.DTOs;
using BNolan.AviationWx.NET.Models.Enums;
using Microsoft.Extensions.Logging;
using Quartz;
using System.Diagnostics;

namespace QuartzScheduler.Jobs.AviationWeather
{
    public class StationObservationJob : IJob
    {
        private readonly ILogger<StationObservationJob> _logger;

        public StationObservationJob(ILogger<StationObservationJob> logger)
        {
            (_logger) = (logger);
        }
        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                var icao = context.MergedJobDataMap["icao"].ToString();
                _logger.LogInformation($"---------------------------------------------------");
                _logger.LogInformation($"{this.GetType().Name} starting execution of {icao}");
                var sw = new Stopwatch();
                sw.Start();

                if (!context.CancellationToken.IsCancellationRequested)
                {
                    var obs = await GetLatestObservationAsync(icao);

                    if (obs != null)
                        _logger.LogInformation($"Latest observation for '{icao}': {obs.RawMETAR}");
                    else
                        _logger.LogInformation($"Unable to find an observation for '{icao}'");
                }
                else
                {
                    _logger.LogInformation("Job cancelled");
                }

                sw.Stop();
                _logger.LogInformation($"{this.GetType().Name} completed execution of {context.MergedJobDataMap["icao"]} in {sw.Elapsed.TotalMilliseconds} milliseconds");
                _logger.LogInformation($"---------------------------------------------------");
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to execute job", ex);
            }
        }

        public async Task<METARDto?> GetLatestObservationAsync(string icao)
        {
            var aviationParser = new BNolan.AviationWx.NET.AviationWeather(ParserType.CSV);
            var obs = await aviationParser.GetLatestObservationAsync(new[] { icao });

            return obs.FirstOrDefault()?.METAR.FirstOrDefault();
        }
    }
}