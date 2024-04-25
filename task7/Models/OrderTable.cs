namespace task7.Models;
using System.ComponentModel.DataAnnotations;

public class OrderTable
{
    public int IdOrder { get; set; }
    public int IdProduct { get; set; }
    public Product Product { get; set; }
    public int Amount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? FulfilledAt { get; set; } 
}