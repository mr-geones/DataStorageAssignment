namespace Data.Entities;

public class CustomerEntity
{
    public int CustomerId { get; set; }
    public string Name { get; set; } = null!;

    // Navigation property
    public ICollection<ProjectEntity> Projects { get; set; } = new List<ProjectEntity>();
}