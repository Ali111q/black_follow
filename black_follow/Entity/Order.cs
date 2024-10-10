using System.ComponentModel.DataAnnotations;

namespace black_follow.Entity;

public class Order
{
    [Key]
    public Guid Id { get; set; }
    public string Name { get; set; }
}