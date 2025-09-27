using ITTitans.Hackathon2025.Utils;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ITTitans.Hackathon2025.EntityModel.DataSource;

[Table(name: "DataSource")]
public class DataSourceEntity
{
    public Guid Id { get; set; }

    [StringLength(StringLengths.Name)]
    public string Name { get; set; } = null!;

    [StringLength(StringLengths.Description)]
    public string? Description { get; set; }

    public List<DataSourceRequirementEntity> Requirements { get; init; } = [];

    public bool IsDeleted { get; set; }
}