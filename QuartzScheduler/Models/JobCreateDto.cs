using System.ComponentModel.DataAnnotations;

namespace QuartzScheduler.Api.Models
{
    public class JobCreateDto
    {
        [Required] public string ParentName { get; set; }

        [Required] public string ParentGroup { get; set; }

        [Required] public string? Description { get; set; }

        [Required] public DateTimeOffset ExecuteAt { get; set; }

        [Required][StringLength(4)] public string Icao { get; set; }
    }
}
