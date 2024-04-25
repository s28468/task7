using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System;
using System.Data;
using task7.Models;
using System.ComponentModel.DataAnnotations;


namespace task7.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WarehouseController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public WarehouseController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost]
        public IActionResult Post([FromBody] WarehouseUpdateRequest request)
        {
            // check input data
            if (request.Amount <= 0)
            {
                return BadRequest("Amount must be greater than 0.");
            }

            using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                connection.Open();
                var transaction = connection.BeginTransaction();

                try
                {
                    // if the product with the given id exists and retrieve the price
                    var productPrice = GetProductPrice(connection, transaction, request.IdProduct);
                    if (productPrice == null)
                    {
                        transaction.Rollback();
                        return BadRequest("Product not found.");
                    }
                    // if the product with the given id exists
                    if (!CheckIfExists(connection, transaction, "master.dbo.Product", "IdProduct", request.IdProduct))
                    {
                        transaction.Rollback();
                        return BadRequest("Product not found.");
                    }
                    // if the warehouse with the given id exists
                    if (!CheckIfExists(connection, transaction, "master.dbo.Warehouse", "IdWarehouse", request.IdWarehouse))
                    {
                        transaction.Rollback();
                        return BadRequest("Warehouse not found.");
                    }
                    // check if order is available 
                    var orderId = CheckOrder(connection, transaction, request);
                    if (!orderId.HasValue)
                    {
                        transaction.Rollback();
                        return BadRequest("No matching or unfulfilled order found.");
                    }
                    
                    
                    var productWarehouseId = InsertProductWarehouse(connection, transaction, request, productPrice.Value, orderId.Value);
                    if (!productWarehouseId.HasValue)
                    {
                        transaction.Rollback();
                        return StatusCode(500, "An error occurred while inserting into Product_Warehouse.");
                    }
                    
                    // if everything is ok
                    UpdateOrder(connection, transaction, orderId.Value, request.CreatedAt);
                    transaction.Commit();
                    return Ok(new { Message = "Warehouse updated successfully and order fulfilled.", ProductWarehouseId = productWarehouseId.Value });
                }
                catch
                {
                    transaction.Rollback();
                    return StatusCode(500, "An error occurred while processing your request.");
                }
            }
        }
        
        [HttpPost("/warehouse/procedure")]
        public IActionResult ExecuteAddProductToWarehouse([FromBody] WarehouseUpdateRequest request)
        {
            using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                using (var command = new SqlCommand("master.dbo.AddProductToWarehouse", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@IdProduct", request.IdProduct);
                    command.Parameters.AddWithValue("@IdWarehouse", request.IdWarehouse);
                    command.Parameters.AddWithValue("@Amount", request.Amount);
                    command.Parameters.AddWithValue("@CreatedAt", request.CreatedAt);

                    try
                    {
                        connection.Open();
                        var productWarehouseId = command.ExecuteScalar();
                        if (productWarehouseId != null)
                        {
                            return Ok(new { ProductWarehouseId = productWarehouseId });
                        }
                        else
                        {
                            return NotFound(new { Message = "No order was fulfilled or product added to warehouse." });
                        }
                    }
                    catch (SqlException ex)
                    {
                        return StatusCode(500, new { Message = ex.Message });
                    }
                }
            }
        }



        private decimal? GetProductPrice(SqlConnection connection, SqlTransaction transaction, int productId)
        {
            using (var command = new SqlCommand("SELECT Price FROM master.dbo.Product WHERE IdProduct = @IdProduct", connection, transaction))
            {
                command.Parameters.AddWithValue("@IdProduct", productId);

                try
                {
                    var result = command.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                    {
                        return (decimal)result;
                    }
                    else
                    {
                        return null;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in GetProductPrice: {ex.Message}");
                    return null;
                }
            }
        }
        private bool CheckIfExists(SqlConnection connection, SqlTransaction transaction, string tableName, string columnName, object value)
        {
            using (var command = new SqlCommand($"SELECT COUNT(1) FROM {tableName} WHERE {columnName} = @value", connection, transaction))
            {
                command.Parameters.AddWithValue("@value", value);
                try
                {
                    var result = command.ExecuteScalar();
                    return result != null && Convert.ToInt32(result) > 0;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in CheckIfExists: {ex.Message}");
                    return false;
                }
            }
        }
        private int? CheckOrder(SqlConnection connection, SqlTransaction transaction, WarehouseUpdateRequest request)
        {
            using (var command = new SqlCommand("SELECT IdOrder FROM [master].[dbo].[Order] WHERE IdProduct = @IdProduct AND Amount = @Amount AND CreatedAt < @CreatedAt", connection, transaction))
            {
                command.Parameters.AddWithValue("@IdProduct", request.IdProduct);
                command.Parameters.AddWithValue("@Amount", request.Amount);
                command.Parameters.AddWithValue("@CreatedAt", request.CreatedAt);
                try
                {
                    var result = command.ExecuteScalar();
                    if (result != DBNull.Value && result != null)
                    {
                        return (int)result;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred: {ex.Message}");
                }
            }
            return null;
        }
        private void UpdateOrder(SqlConnection connection, SqlTransaction transaction, int orderId, DateTime createdAt)
        {
            using (var command = new SqlCommand("UPDATE [master].[dbo].[Order] SET FulfilledAt = @FulfilledAt WHERE IdOrder = @IdOrder", connection, transaction))
            {
                command.Parameters.AddWithValue("@FulfilledAt", DateTime.UtcNow); 
                command.Parameters.AddWithValue("@IdOrder", orderId);

                command.ExecuteNonQuery();
            }
        }
        private int? InsertProductWarehouse(SqlConnection connection, SqlTransaction transaction, WarehouseUpdateRequest request, decimal productPrice, int orderID)
        {
            var query = @"
        INSERT INTO master.dbo.Product_Warehouse (IdProduct, IdWarehouse, IdOrder, Amount, Price, CreatedAt) 
        VALUES (@IdProduct, @IdWarehouse, @IdOrder, @Amount, @Price, @CreatedAt);
        SELECT CAST(SCOPE_IDENTITY() as int);
    ";

            using (var command = new SqlCommand(query, connection, transaction))
            {
                command.Parameters.AddWithValue("@IdProduct", request.IdProduct);
                command.Parameters.AddWithValue("@IdWarehouse", request.IdWarehouse);
                command.Parameters.AddWithValue("@IdOrder", orderID);
                command.Parameters.AddWithValue("@Amount", request.Amount);
                command.Parameters.AddWithValue("@Price", productPrice);
                command.Parameters.AddWithValue("@CreatedAt", DateTime.UtcNow);

                try
                {
                    var result = command.ExecuteScalar();
                    return (int?)result;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred during Product_Warehouse insert: {ex.Message}");
                    return null;
                }
            }
        }



    }
    
    
    public class WarehouseUpdateRequest
    {
        [Required]
        public int IdProduct { get; set; }
        [Required]
        public int IdWarehouse { get; set; }
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Amount must be greater than 0.")]
        public int Amount { get; set; }
        [Required]
        public DateTime CreatedAt { get; set; }
    }
}
