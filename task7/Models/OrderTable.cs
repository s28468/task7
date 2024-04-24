namespace task7.Models;
using System.ComponentModel.DataAnnotations;

public class OrderTable
{
    [Required]
    public int IdOrder { get; set; }
    [Required]
    public int IdProduct { get; set; }
    [Required]
    public Product Product { get; set; }
    [Required]
    public int Amount { get; set; }
    [Required]
    public DateTime CreatedAt { get; set; }
    [Required]
    public DateTime? FulfilledAt { get; set; } 
}