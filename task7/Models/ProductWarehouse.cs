namespace task7.Models;

public class ProductWarehouse
{
    public int IdProductWarehouse { get; set; }
    public int IdWarehouse { get; set; }
    public Warehouse Warehouse { get; set; }
    public int IdProduct { get; set; }
    public Product Product { get; set; }
    public int IdOrder { get; set; }
    public OrderTable Order { get; set; }
    public int Amount { get; set; }
    public decimal Price { get; set; }
    public DateTime CreatedAt { get; set; }
}