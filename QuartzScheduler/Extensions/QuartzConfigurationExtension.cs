using Quartz;
using QuartzScheduler.Jobs.AviationWeather;
using QuartzScheduler.Jobs.AviationWeather.Models.Enums;

namespace QuartzScheduler.Extensions
{
    public static class QuartzConfigurationExtension
    {
        public static void AddQuartzScheduler(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddQuartz(q =>
            {
                // Inject Quartz Service for usage in the API
                q.UseMicrosoftDependencyInjectionJobFactory();

                // Set up the once daily job for downloading forecast
                // Job will execute at 08:15 each day
                var jobMap = new JobDataMap(){
                        new KeyValuePair<string, object>(JobMapKeysEnum.ICAO.Name, "KPHL") };
                var jobKey = new JobKey("fcstJob", "dailyJob");
                q.AddJob<StationForecastJob>(jobKey, j =>
                    j.WithDescription($"Retrieves TAF for {jobMap[JobMapKeysEnum.ICAO.Name]} every once a day")
                    .UsingJobData(jobMap));

                q.AddTrigger(t =>
                    t.ForJob(jobKey)
                    .StartNow()
                    .WithCronSchedule(CronScheduleBuilder.DailyAtHourAndMinute(08, 15)));

                // Set up an hourly job to get the latest observation on 15 minutes past the hour
                jobKey = new JobKey("obsJob", "hourlyJob");
                q.AddJob<StationObservationJob>(jobKey, j =>
                    j.WithDescription($"Retrieves METAR for {jobMap[JobMapKeysEnum.ICAO.Name]} hourly")
                    .UsingJobData(jobMap));

                // Set up the start time to be the previous hour so we execute right away as well
                // as the 15 after the hour going forward
                var previousHour = DateTime.Now.AddHours(-1);
                var startTime = new DateTime(previousHour.Year, previousHour.Month, previousHour.Day, previousHour.Hour, 15, 0);
                q.AddTrigger(t =>
                    t.ForJob(jobKey)
                    .StartAt(startTime)
                    .WithSimpleSchedule(x =>
                        x.WithIntervalInHours(1)
                        .RepeatForever())
                    );
            });

            services.AddQuartzHostedService(opt =>
            {
                opt.WaitForJobsToComplete = true;
                opt.AwaitApplicationStarted = true;
            });
        }
    }
}
