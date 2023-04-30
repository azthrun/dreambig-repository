using DreamBig.Repository.Abstractions;
using DreamBig.Repository.Cosmos.Attributes;

namespace SampleApi.Models;

[UseRepo]
public sealed class Activity : IEntity
{
    public string? Id { get; set; }
    public string? KidId { get; set; }
    public int? Scores { get; set; }
    public int? IntervalType { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
