namespace QuartzScheduler.Api.Models
{
    public record JobDetailsDto(string Name, string Group, string? Description, DateTimeOffset? LastExecutionAt, DateTimeOffset? NextExecutionAt);

}
