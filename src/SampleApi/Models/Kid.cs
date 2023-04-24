using DreamBig.Repository.Abstractions;

namespace SampleApi.Models;

public sealed class Kid : IEntity
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string[]? GuardianEmails { get; set; }
    public int? Scores { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
