using Microsoft.AspNetCore.Mvc;
using Quartz;
using Quartz.Impl.Matchers;
using QuartzScheduler.Api.Models;
using Serilog;
using System.Text.Json;

namespace QuartzScheduler.Api.Extensions
{
    public static class ControllerConfigurationExtension
    {
        public static void AddApiControllers(this WebApplication app)
        {
            app.MapGet("/schedules", async (ISchedulerFactory sf) =>
            {
                var scheduler = await sf.GetScheduler();
                var definedJobDetails = new List<JobDetailsDto>();

                var jobKeys = await scheduler.GetJobKeys(GroupMatcher<JobKey>.AnyGroup());
                foreach (var jobKey in jobKeys)
                {
                    var jobDetail = await scheduler.GetJobDetail(jobKey);
                    var jobSchedule = await scheduler.GetTriggersOfJob(jobKey);
                    if (jobDetail != null
                        && jobSchedule != null
                        && jobSchedule.Count > 0)
                    {
                        definedJobDetails.Add(new JobDetailsDto(
                            jobDetail.Key.Name,
                            jobDetail.Key.Group,
                            jobDetail.Description,
                            jobSchedule.First().GetPreviousFireTimeUtc(),
                            jobSchedule.First().GetNextFireTimeUtc())
                        );
                    }
                }

                return definedJobDetails;
            }).WithName("GetJobSchedules");

            app.MapPost("/schedules", async ([FromBody] JobCreateDto jobCreate, ISchedulerFactory sf) =>
            {
                Log.Debug($"Received Create request: {JsonSerializer.Serialize(jobCreate)}");

                var scheduler = await sf.GetScheduler();

                IJobDetail? parentJob = await scheduler.GetJobDetail(new JobKey(jobCreate.ParentName, jobCreate.ParentGroup));

                // If we didn't find the parent job then let the caller know
                if (parentJob == null)
                {
                    return Results.NotFound();
                }

                var jobKey = new JobKey($"{jobCreate.ParentName}-{jobCreate.ExecuteAt.Ticks}", jobCreate.ParentGroup);

                // Create the job based off of the parent job but set new job data and description
                var newJob = parentJob.GetJobBuilder().SetJobData(new JobDataMap()
                    {
                        new KeyValuePair<string, object>("icao", jobCreate.Icao)
                    }).WithDescription(jobCreate.Description)
                    .WithIdentity(jobKey)
                    .Build();

                // Create a trigger to execute the job one time and at the time requested
                var trigger = TriggerBuilder.Create()
                    .ForJob(jobKey)
                    .WithSimpleSchedule(s => s.WithRepeatCount(0))
                    .StartAt(jobCreate.ExecuteAt)
                    .Build();

                // Schedule the job and the trigger
                await scheduler.ScheduleJob(newJob, trigger);

                return Results.Ok();
            }).WithName("CreateJobSchedule");
        }
    }
}
