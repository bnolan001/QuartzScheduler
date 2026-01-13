using Quartz;
using QuartzScheduler.Jobs.AviationWeather;
using QuartzScheduler.Jobs.AviationWeather.Models.Enums;

namespace QuartzScheduler.Extensions
{
    public static class QuartzConfigurationExtension
    {
        public static void AddQuartzScheduler(this IServiceCollection services)
        {
            services.AddQuartz(q =>
            {
                // Set up the once daily job for downloading forecast
                // Job will execute at 08:30 each day
                var fcstJobKey = new JobKey("fcstJob", "dailyJob");
                var jobMap = new JobDataMap(){
                        new KeyValuePair<string, object>(JobMapKeysEnum.ICAO.Name, "KPHL") };
                q.AddJob<StationForecastJob>(fcstJobKey, j =>
                    j.WithDescription($"Retrieves TAF for {jobMap[JobMapKeysEnum.ICAO.Name]} once a day")
                    .UsingJobData(jobMap)
                    .StoreDurably(true));

                q.AddTrigger(t =>
                    t.ForJob(fcstJobKey)
                    .WithIdentity("fcstCronJobTrigger")
                    .StartAt(DateTime.UtcNow.AddHours(-1))
                    .WithCronSchedule(CronScheduleBuilder.DailyAtHourAndMinute(08, 30)));
                
                // Set up an hourly job to get the latest observation on 15 minutes past the hour
                var obsJobKey = new JobKey("obsJob", "hourlyJob");
                q.AddJob<StationObservationJob>(obsJobKey, j =>
                    j.WithDescription($"Retrieves METAR for {jobMap[JobMapKeysEnum.ICAO.Name]} hourly")
                    .UsingJobData(jobMap)
                    .StoreDurably(true));

                // Set up the start time to the 15 minutes past the previous hour.  This way it is set to
                // execte at the 15 minute mark going forward
                var previous = DateTime.UtcNow.AddHours(-1);
                var startTime = new DateTime(previous.Year, previous.Month, previous.Day, previous.Hour, 15, 0);
                q.AddTrigger(t =>
                    t.ForJob(obsJobKey)
                    .WithIdentity("obsSimpleJobTrigger")
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
