using ConsEcommerceConsole.Data;
using ConsEcommerceConsole.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.SqlServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;


namespace ConsEcommerceConsole.Services
{
    public class AuthService
    {
        private readonly PasswordHasher<User> passwordHasher =
            new PasswordHasher<User>();

        public User? Login(string username, string password)
        {
            using SqlConnection connection =
                DbConnectionFactory.CreateConnection();

            string query = @"
                SELECT UserId, Username, PasswordHash, Role
                FROM Users
                WHERE Username = @Username";

            using SqlCommand command =
                new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@Username", username);

            connection.Open();

            using SqlDataReader reader = command.ExecuteReader();

            if (!reader.Read())
                return null;

            User user = new User
            {
                UserId = reader.GetInt32(0),
                UserName = reader.GetString(1),
                PasswordHash = reader.GetString(2),
                Role = reader.GetString(3)
            };

            var result = passwordHasher.VerifyHashedPassword(
                user, user.PasswordHash, password);

            if (result == PasswordVerificationResult.Failed)
                return null;

            return user;
        }

        public void CreateDemoAccounts()
        {
            using SqlConnection connection =
                DbConnectionFactory.CreateConnection();

            connection.Open();

            CreateUserIfMissing(
                connection, "admin", "Admin@123", "Admin");

            CreateUserIfMissing(
                connection, "customer1", "Customer@123", "Customer");

            CreateUserIfMissing(
                connection, "customer2", "Customer@456", "Customer");
        }

        private void CreateUserIfMissing(
            SqlConnection connection,
            string username,
            string password,
            string role)
        {
            string checkQuery =
                "SELECT COUNT(*) FROM Users WHERE Username = @Username";

            using SqlCommand checkCommand =
                new SqlCommand(checkQuery, connection);

            checkCommand.Parameters.AddWithValue("@Username", username);

            int count = (int)checkCommand.ExecuteScalar()!;

            if (count > 0)
                return;

            User user = new User
            {
                UserName = username,
                Role = role
            };

            user.PasswordHash =
                passwordHasher.HashPassword(user, password);

            string insertQuery = @"
                INSERT INTO Users (Username, PasswordHash, Role)
                VALUES (@Username, @PasswordHash, @Role)";

            using SqlCommand insertCommand =
                new SqlCommand(insertQuery, connection);

            insertCommand.Parameters.AddWithValue(
                "@Username", user.UserName);

            insertCommand.Parameters.AddWithValue(
                "@PasswordHash", user.PasswordHash);

            insertCommand.Parameters.AddWithValue(
                "@Role", user.Role);

            insertCommand.ExecuteNonQuery();
        }
    }
}
