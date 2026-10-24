using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.SqlServer;

namespace ConsEcommerceConsole.Data
{
    public static class DbConnectionFactory
    {
        private static readonly string connectionString = "Server=.;Database=EcommercesDb;Integrated Security=True;TrustServerCertificate=True;";
public static SqlConnection CreateConnection()
        {
            return new SqlConnection(connectionString);
        }
    }
}
