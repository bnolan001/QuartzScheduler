using Microsoft.Extensions.Logging;
using Quartz;
using QuartzScheduler.Jobs.AviationWeather.Models.Enums;
using System.Diagnostics;

namespace QuartzScheduler.Jobs.AviationWeather
{
    public class StationForecastJob : IJob
    {
        private readonly ILogger<StationForecastJob> _logger;
        private static readonly HttpClient _httpClient = new HttpClient();

        public StationForecastJob(ILogger<StationForecastJob> logger)
        {
            (_logger) = (logger);
        }

        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                var icao = context.MergedJobDataMap.GetString(JobMapKeysEnum.ICAO.Name);
                _logger.LogInformation($"---------------------------------------------------");

                if (string.IsNullOrWhiteSpace(icao))
                {
                    _logger.LogWarning($"Job ICAO is null or empty for job {context.JobDetail.Key.Name}-{context.JobDetail.Key.Group}");
                    return;
                }

                _logger.LogInformation($"{this.GetType().Name} starting execution of {icao}");
                var sw = new Stopwatch();
                sw.Start();

                if (!context.CancellationToken.IsCancellationRequested)
                {
                    var taf = await GetLatestForecastAsync(icao);
                    if (taf != null)
                        _logger.LogInformation($"Latest forecast for '{icao}': {taf}");
                    else
                        _logger.LogInformation($"Unable to find a forecast for '{icao}'");
                }
                else
                {
                    _logger.LogInformation("Job cancelled");
                }

                sw.Stop();
                _logger.LogInformation($"{this.GetType().Name} completed execution of {icao} in {sw.Elapsed.TotalMilliseconds} milliseconds");
                _logger.LogInformation($"---------------------------------------------------");
            }
            // Catch all exceptions, otherwise the job may re-execute immediately after finishing
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to execute job");
            }
        }


        public async Task<string?> GetLatestForecastAsync(string icao)
        {
            var request = await _httpClient.GetAsync($"https://aviationweather.gov/api/data/taf?ids={icao}&format=raw");
            var fcst = await request.Content.ReadAsStringAsync();

            return fcst;
        }
    }
}
