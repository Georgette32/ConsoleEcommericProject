
# E-Commerce Console Application

## Project Overview
A console-based E-Commerce application developed using C#, .NET, ADO.NET, and SQL Server.

The system allows customers to browse products, add items to their shopping carts, place orders, and view their order history. Administrators can manage products and view all customer orders.

## Technologies Used
- C#
- .NET
- ADO.NET
- Microsoft SQL Server
- Microsoft.Data.SqlClient
- Microsoft.Extensions.Identity.Core

## Features

### Admin
- Login authentication.
- View available products.
- Add new products.
- Edit product details, prices, stock quantities, and active status.
- View all customer orders.

### Customer
- Login authentication.
- View available products.
- Add products to the shopping cart.
- View cart items and calculate the total price.
- Checkout and place orders.
- View personal order history.

## Database Design

Database Name: `EcommerceDb`

The database contains the following tables:

- **Users:** Stores usernames, password hashes, and user roles.
- **Products:** Stores product names, descriptions, prices, stock quantities, and active status.
- **CartItems:** Stores products and quantities added to each customer's cart.
- **Orders:** Stores order information, customer ID, order date, total amount, and status.
- **OrderItems:** Stores the products purchased in each order, including the product name and price at the time of purchase.

## Security and Data Integrity
- Passwords are stored as hashes instead of plain text.
- SQL queries use parameters to reduce SQL injection risks.
- Customers can view their own orders.
- Product stock is validated before checkout.
- Checkout uses a SQL transaction to ensure that order creation, order items, stock updates, and cart clearing succeed together or are rolled back.
- Order items preserve the product name and unit price at the time of purchase.

## Requirements
- .NET SDK
- Microsoft SQL Server
- Visual Studio
- SQL Server Management Studio (SSMS)

## How to Run
1. Clone or download the repository.
2. Open the solution in Visual Studio.
3. Create the `EcommerceDb` database and its tables using the SQL setup script.
4. Update the connection string in `Data/DbConnectionFactory.cs` if necessary.
5. Restore the NuGet packages.
6. Build the solution.
7. Run the application.

## Connection String
The current example uses a local SQL Server instance:

`Server=.;Database=EcommerceDb;Integrated Security=True;TrustServerCertificate=True;`

Change the server name and authentication settings if your SQL Server configuration is different.

## Demo Accounts

| Role | Username | Password |
|---|---|---|
| Admin | `admin` | `Admin@123` |
| Customer | `customer1` | `Customer@123` |
| Customer | `customer2` | `Customer@456` |

These are demo credentials for local testing only.

## Project Structure

```text
ConsEcommerceConsole/
├── Data/
│   └── DbConnectionFactory.cs
├── Models/
│   ├── User.cs
│   ├── Product.cs
│   ├── CartItem.cs
│   ├── Order.cs
│   └── OrderItem.cs
├── Services/
│   ├── AuthService.cs
│   ├── ProductService.cs
│   ├── CartService.cs
│   └── OrderService.cs
└── Program.cs
```

## Future Improvements
- Update and remove cart items.
- Add order status management.
- Improve input validation and error handling.
- Add unit tests.

## Author
Developed as a C# and SQL Server E-Commerce project.
