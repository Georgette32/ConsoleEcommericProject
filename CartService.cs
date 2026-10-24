using ConsEcommerceConsole.Data;
using ConsEcommerceConsole.Models;
using Microsoft.Data.SqlClient;

namespace ConsEcommerceConsole.Services
{
    public class CartService
    {
        
        public string AddToCart(int userId, int productId, int quantity)
        {
            if (quantity <= 0)
            {
                return "Quantity must be greater than zero.";
            }

            using SqlConnection connection =
                DbConnectionFactory.CreateConnection();

            connection.Open();

            using SqlTransaction transaction =
                connection.BeginTransaction();

            try
            {
              
                int stockQuantity;

                string productQuery = @"
                    SELECT StockQuantity
                    FROM Products WITH (UPDLOCK, HOLDLOCK)
                    WHERE ProductId = @ProductId
                      AND IsActive = 1;";

                using (SqlCommand command =
                    new SqlCommand(productQuery, connection, transaction))
                {
                    command.Parameters.AddWithValue("@ProductId", productId);

                    object? result = command.ExecuteScalar();

                    if (result == null)
                    {
                        transaction.Rollback();
                        return "Product not found or inactive.";
                    }

                    stockQuantity = Convert.ToInt32(result);
                }

               
                int existingQuantity = 0;

                string cartQuery = @"
                    SELECT Quantity
                    FROM CartItems WITH (UPDLOCK, HOLDLOCK)
                    WHERE UserId = @UserId
                      AND ProductId = @ProductId;";

                using (SqlCommand command =
                    new SqlCommand(cartQuery, connection, transaction))
                {
                    command.Parameters.AddWithValue("@UserId", userId);
                    command.Parameters.AddWithValue("@ProductId", productId);

                    object? result = command.ExecuteScalar();

                    if (result != null)
                    {
                        existingQuantity = Convert.ToInt32(result);
                    }
                }

                int totalQuantity = existingQuantity + quantity;

              
                if (totalQuantity > stockQuantity)
                {
                    transaction.Rollback();

                    return $"Not enough stock. Available quantity: {stockQuantity}.";
                }

                if (existingQuantity > 0)
                {
                    
                    string updateQuery = @"
                        UPDATE CartItems
                        SET Quantity = @Quantity
                        WHERE UserId = @UserId
                          AND ProductId = @ProductId;";

                    using SqlCommand command =
                        new SqlCommand(updateQuery, connection, transaction);

                    command.Parameters.AddWithValue(
                        "@Quantity", totalQuantity);

                    command.Parameters.AddWithValue("@UserId", userId);
                    command.Parameters.AddWithValue("@ProductId", productId);

                    command.ExecuteNonQuery();
                }
                else
                {
                 
                    string insertQuery = @"
                        INSERT INTO CartItems
                            (UserId, ProductId, Quantity)
                        VALUES
                            (@UserId, @ProductId, @Quantity);";

                    using SqlCommand command =
                        new SqlCommand(insertQuery, connection, transaction);

                    command.Parameters.AddWithValue("@UserId", userId);
                    command.Parameters.AddWithValue("@ProductId", productId);
                    command.Parameters.AddWithValue("@Quantity", quantity);

                    command.ExecuteNonQuery();
                }

                transaction.Commit();

                return "Product added to cart successfully!";
            }
            catch
            {
                try
                {
                    transaction.Rollback();
                }
                catch
                {
                   
                }

                throw;
            }
        }

        
        public List<CartItem> GetCartItems(int userId)
        {
            List<CartItem> cartItems = new List<CartItem>();

            using SqlConnection connection =
                DbConnectionFactory.CreateConnection();

            connection.Open();

            string query = @"
                SELECT
                    c.CartItemId,
                    c.UserId,
                    c.ProductId,
                    c.Quantity,
                    p.Name,
                    p.Price
                FROM CartItems c
                INNER JOIN Products p
                    ON c.ProductId = p.ProductId
                WHERE c.UserId = @UserId
                ORDER BY c.CartItemId;";

            using SqlCommand command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@UserId", userId);

            using SqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                CartItem item = new CartItem
                {
                    CartItemId = reader.GetInt32(0),
                    UserId = reader.GetInt32(1),
                    ProductId = reader.GetInt32(2),
                    Quantity = reader.GetInt32(3),
                    ProductName = reader.GetString(4),
                    UnitPrice = reader.GetDecimal(5)
                };

                cartItems.Add(item);
            }

            return cartItems;
        }
    }
}