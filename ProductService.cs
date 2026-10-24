using ConsEcommerceConsole.Data;
using ConsEcommerceConsole.Models;
using Microsoft.Data.SqlClient;
using Microsoft.SqlServer;
using System.Data;

namespace ConsEcommerceConsole.Services
{
    public class ProductService
    {
        public List<Product> GetAvailableProducts()
        {
            List<Product> products = new List<Product>();
            using SqlConnection connection = DbConnectionFactory.CreateConnection();

            string query = @"
                SELECT ProductId, Name, Description, Price,
                       StockQuantity, IsActive
                FROM Products
                WHERE IsActive = 1";

            using SqlCommand command = new SqlCommand(query, connection);
            connection.Open();
            using SqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                Product product = new Product
                {
                    ProductId = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Description = reader.IsDBNull(2) ? null : reader.GetString(2),
                    Price = reader.GetDecimal(3),
                    StockQuantity = reader.GetInt32(4),
                    IsActive = reader.GetBoolean(5)

                };
                products.Add(product);
            }
            return products;
        }
        public void AddProduct(Product product)
        {
            using SqlConnection connection =
                DbConnectionFactory.CreateConnection();

            string query = @"
            INSERT INTO Products
                (Name, Description, Price, StockQuantity, IsActive)
            VALUES
                (@Name, @Description, @Price, @StockQuantity, @IsActive)";

            using SqlCommand command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@Name", product.Name);

            command.Parameters.AddWithValue(
                "@Description",
                (object?)product.Description ?? DBNull.Value);

            command.Parameters.AddWithValue("@Price", product.Price);

            command.Parameters.AddWithValue(
                "@StockQuantity", product.StockQuantity);

            command.Parameters.AddWithValue("@IsActive", product.IsActive);

            connection.Open();
            command.ExecuteNonQuery();
        }

        public bool UpdateProduct(Product product)
        {
            using SqlConnection connection =
                DbConnectionFactory.CreateConnection();

            string query = @"
            UPDATE Products
            SET Name = @Name,
                Description = @Description,
                Price = @Price,
                StockQuantity = @StockQuantity,
                IsActive = @IsActive
            WHERE ProductId = @ProductId";

            using SqlCommand command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@ProductId", product.ProductId);
            command.Parameters.AddWithValue("@Name", product.Name);

            command.Parameters.AddWithValue(
                "@Description",
                (object?)product.Description ?? DBNull.Value);

            command.Parameters.AddWithValue("@Price", product.Price);

            command.Parameters.AddWithValue(
                "@StockQuantity", product.StockQuantity);

            command.Parameters.AddWithValue("@IsActive", product.IsActive);

            connection.Open();

            return command.ExecuteNonQuery() > 0;
        }

        public bool ProductExists(int productId)
        {
            using SqlConnection connection =
                DbConnectionFactory.CreateConnection();

            string query = @"
            SELECT COUNT(*)
            FROM Products
            WHERE ProductId = @ProductId";

            using SqlCommand command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@ProductId", productId);

            connection.Open();

            return (int)command.ExecuteScalar()! > 0;
        }
    
}
}
