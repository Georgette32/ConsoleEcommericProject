using ConsEcommerceConsole.Data;
using ConsEcommerceConsole.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ConsEcommerceConsole.Services
{
    public class OrderService
    {
        // Checkout: create order, save items, decrease stock and clear cart.
        public string Checkout(int userId)
        {
            using SqlConnection connection =
                DbConnectionFactory.CreateConnection();

            connection.Open();

            using SqlTransaction transaction =
                connection.BeginTransaction(IsolationLevel.Serializable);

            try
            {
                var cartItems = new List<CartItem>();

                string cartQuery = @"
                    SELECT
                        c.ProductId,
                        c.Quantity,
                        p.Name,
                        p.Price,
                        p.StockQuantity,
                        p.IsActive
                    FROM CartItems c WITH (UPDLOCK, HOLDLOCK)
                    INNER JOIN Products p WITH (UPDLOCK, HOLDLOCK)
                        ON c.ProductId = p.ProductId
                    WHERE c.UserId = @UserId;";

                using (SqlCommand command =
                    new SqlCommand(cartQuery, connection, transaction))
                {
                    command.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;

                    using SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        int productId = reader.GetInt32(0);
                        int quantity = reader.GetInt32(1);
                        string productName = reader.GetString(2);
                        decimal price = reader.GetDecimal(3);
                        int stock = reader.GetInt32(4);
                        bool isActive = reader.GetBoolean(5);

                        if (!isActive || quantity <= 0 || quantity > stock)
                        {
                            transaction.Rollback();
                            return $"Checkout failed: invalid stock for {productName}.";
                        }

                        cartItems.Add(new CartItem
                        {
                            ProductId = productId,
                            Quantity = quantity,
                            ProductName = productName,
                            UnitPrice = price,
                            UserId = userId
                        });
                    }
                }

                if (cartItems.Count == 0)
                {
                    transaction.Rollback();
                    return "Your cart is empty.";
                }

                decimal totalAmount = cartItems.Sum(
                    item => item.UnitPrice * item.Quantity);

                int orderId;

                string orderQuery = @"
                    INSERT INTO Orders (UserId, OrderDate, TotalAmount, Status)
                    OUTPUT INSERTED.OrderId
                    VALUES (@UserId, GETDATE(), @TotalAmount, @Status);";

                using (SqlCommand command =
                    new SqlCommand(orderQuery, connection, transaction))
                {
                    command.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;

                    SqlParameter totalParameter =
                        command.Parameters.Add("@TotalAmount", SqlDbType.Decimal);
                    totalParameter.Precision = 18;
                    totalParameter.Scale = 2;
                    totalParameter.Value = totalAmount;

                    command.Parameters.Add("@Status", SqlDbType.NVarChar, 30)
                        .Value = "Pending";

                    orderId = Convert.ToInt32(command.ExecuteScalar());
                }

                foreach (CartItem item in cartItems)
                {
                    // Decrease stock only if enough stock remains.
                    string stockQuery = @"
                        UPDATE Products
                        SET StockQuantity = StockQuantity - @Quantity
                        WHERE ProductId = @ProductId
                          AND IsActive = 1
                          AND StockQuantity >= @Quantity;";

                    using (SqlCommand command =
                        new SqlCommand(stockQuery, connection, transaction))
                    {
                        command.Parameters.Add("@Quantity", SqlDbType.Int)
                            .Value = item.Quantity;

                        command.Parameters.Add("@ProductId", SqlDbType.Int)
                            .Value = item.ProductId;

                        int affectedRows = command.ExecuteNonQuery();

                        if (affectedRows != 1)
                        {
                            throw new InvalidOperationException(
                                $"Insufficient stock for {item.ProductName}.");
                        }
                    }

                    // Save the product name and price as a purchase snapshot.
                    string itemQuery = @"
                        INSERT INTO OrderItems
                            (OrderId, ProductId, ProductName, UnitPrice, Quantity)
                        VALUES
                            (@OrderId, @ProductId, @ProductName, @UnitPrice, @Quantity);";

                    using (SqlCommand command =
                        new SqlCommand(itemQuery, connection, transaction))
                    {
                        command.Parameters.Add("@OrderId", SqlDbType.Int)
                            .Value = orderId;

                        command.Parameters.Add("@ProductId", SqlDbType.Int)
                            .Value = item.ProductId;

                        command.Parameters.Add("@ProductName", SqlDbType.NVarChar, 200)
                            .Value = item.ProductName;

                        SqlParameter priceParameter =
                            command.Parameters.Add("@UnitPrice", SqlDbType.Decimal);
                        priceParameter.Precision = 18;
                        priceParameter.Scale = 2;
                        priceParameter.Value = item.UnitPrice;

                        command.Parameters.Add("@Quantity", SqlDbType.Int)
                            .Value = item.Quantity;

                        command.ExecuteNonQuery();
                    }
                }

                string clearCartQuery =
                    "DELETE FROM CartItems WHERE UserId = @UserId;";

                using (SqlCommand command =
                    new SqlCommand(clearCartQuery, connection, transaction))
                {
                    command.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
                    command.ExecuteNonQuery();
                }

                transaction.Commit();

                return $"Checkout successful! Your Order ID is {orderId}. Total: {totalAmount:F2} EGP";
            }
            catch (Exception ex)
            {
                try
                {
                    transaction.Rollback();
                }
                catch
                {
                    // The transaction may already be rolled back.
                }

                return $"Checkout failed: {ex.Message}";
            }
        }

        // Return only orders belonging to the current customer.
        public List<Order> GetCustomerOrders(int userId)
        {
            var orders = new List<Order>();

            using SqlConnection connection =
                DbConnectionFactory.CreateConnection();

            connection.Open();

            string query = @"
                SELECT OrderId, UserId, OrderDate, TotalAmount, Status
                FROM Orders
                WHERE UserId = @UserId
                ORDER BY OrderDate DESC;";

            using SqlCommand command = new SqlCommand(query, connection);

            command.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;

            using SqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                orders.Add(new Order
                {
                    OrderId = reader.GetInt32(0),
                    UserId = reader.GetInt32(1),
                    OrderDate = reader.GetDateTime(2),
                    TotalAmount = reader.GetDecimal(3),
                    Status = reader.GetString(4)
                });
            }

            return orders;
        }

        // Return all orders for the admin.
        public List<Order> GetAllOrders()
        {
            var orders = new List<Order>();

            using SqlConnection connection =
                DbConnectionFactory.CreateConnection();

            connection.Open();

            string query = @"
                SELECT o.OrderId, o.UserId, o.OrderDate,
                       o.TotalAmount, o.Status, u.Username
                FROM Orders o
                INNER JOIN Users u ON o.UserId = u.UserId
                ORDER BY o.OrderDate DESC;";

            using SqlCommand command = new SqlCommand(query, connection);

            using SqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                orders.Add(new Order
                {
                    OrderId = reader.GetInt32(0),
                    UserId = reader.GetInt32(1),
                    OrderDate = reader.GetDateTime(2),
                    TotalAmount = reader.GetDecimal(3),
                    Status = reader.GetString(4),
                    Username = reader.GetString(5)
                });
            }

            return orders;
        }
    }
}