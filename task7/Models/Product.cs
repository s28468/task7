namespace task7.Models;
using System.ComponentModel.DataAnnotations;

public class Product
{
    [Required]
    public int IdProduct { get; set; }
    [Required]
    public string Name { get; set; }
    public string Description { get; set; }
    [Required]
    public decimal Price { get; set; }
}
