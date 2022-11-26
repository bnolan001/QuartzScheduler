using Quartz;
using QuartzScheduler.Jobs.AviationWeather;

namespace QuartzScheduler.Extensions
{
    public static class QuartzConfigurationExtension
    {
        public static void AddQuartzScheduler(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddQuartz(q =>
            {
                q.UseMicrosoftDependencyInjectionJobFactory();

                // Set up the once daily job for downloading forecast
                // Job will execute at 09:15 each day
                var jobMap = new JobDataMap(){
                        new KeyValuePair<string, object>("icao", "KPHL") };
                var jobKey = new JobKey("fcstJob", "dailyJob");
                q.AddJob<StationForecastJob>(jobKey, j =>
                    j.WithDescription($"Retrieves TAF for {jobMap["icao"]} every once a day")
                    .UsingJobData(jobMap));

                q.AddTrigger(t =>
                    t.ForJob(jobKey)
                    .StartNow()
                    .WithCronSchedule(CronScheduleBuilder.DailyAtHourAndMinute(08, 15)));

                // Set up an hourly job to get the latest observation on 15 minutes past the hour
                jobKey = new JobKey("obsJob", "hourlyJob");
                q.AddJob<StationObservationJob>(jobKey, j =>
                    j.WithDescription($"Retrieves METAR for {jobMap["icao"]} hourly")
                    .UsingJobData(jobMap));

                var now = DateTime.Now.AddHours(-1);
                var startTime = new DateTime(now.Year, now.Month, now.Day, now.Hour, 15, 0);
                q.AddTrigger(t =>
                    t.ForJob(jobKey)
                    .StartAt(now)
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
