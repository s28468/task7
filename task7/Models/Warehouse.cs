namespace task7.Models;
using System.ComponentModel.DataAnnotations;

public class Warehouse
{
    [Required]
    public int IdWarehouse { get; set; }
    [Required]
    public string Name { get; set; }
    [Required]
    public string Address { get; set; }
}

