using BNolan.AviationWx.NET.Models.DTOs;
using BNolan.AviationWx.NET.Models.Enums;
using Microsoft.Extensions.Logging;
using Quartz;
using System.Diagnostics;

namespace QuartzScheduler.Jobs.AviationWeather
{
    public class StationForecastJob : IJob
    {
        private readonly ILogger<StationForecastJob> _logger;

        public StationForecastJob(ILogger<StationForecastJob> logger)
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
                    var taf = await GetLatestForecastAsync(icao);
                    if (taf != null)
                        _logger.LogInformation($"Latest forecast for '{icao}': {taf.RawTAF}");
                    else
                        _logger.LogInformation($"Unable to find a forecast for '{icao}'");
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


        public async Task<TAFDto?> GetLatestForecastAsync(string icao)
        {
            var aviationParser = new BNolan.AviationWx.NET.AviationWeather(ParserType.CSV);
            var fcst = await aviationParser.GetLatestForecastsAsync(new[] { icao });

            return fcst.FirstOrDefault()?.TAF.FirstOrDefault();
        }
    }
}
