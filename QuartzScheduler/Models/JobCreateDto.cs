using System.ComponentModel.DataAnnotations;

namespace QuartzScheduler.Api.Models
{
    public record JobCreateDto
    {
        [Required] public string ParentName { get; init; }

        [Required] public string ParentGroup { get; init; }

        [Required] public string? Description { get; init; }

        [Required] public DateTimeOffset ExecuteAt { get; init; }

        [Required][StringLength(4)] public string Icao { get; init; }
    }
}
