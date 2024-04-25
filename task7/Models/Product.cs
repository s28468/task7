namespace task7.Models;
using System.ComponentModel.DataAnnotations;

public class Product
{
    public int IdProduct { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
}
