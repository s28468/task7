namespace task7.Models;
using System.ComponentModel.DataAnnotations;

public class ProductWarehouse
{
    [Required]
    public int IdProductWarehouse { get; set; }
    [Required]
    public int IdWarehouse { get; set; }
    [Required]
    public Warehouse Warehouse { get; set; }
    [Required]
    public int IdProduct { get; set; }
    [Required]
    public Product Product { get; set; }
    [Required]
    public int IdOrder { get; set; }
    [Required]
    public OrderTable Order { get; set; }
    [Required]
    public int Amount { get; set; }
    [Required]
    public decimal Price { get; set; }
    [Required]
    public DateTime CreatedAt { get; set; }
}