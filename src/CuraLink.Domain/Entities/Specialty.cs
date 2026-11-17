namespace CuraLink.Domain.Entities;

public class Specialty
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public ICollection<Doctor> Doctors { get; set; } = new List<Doctor>();
}
