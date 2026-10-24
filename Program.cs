using ConsEcommerceConsole.Models;
using ConsEcommerceConsole.Services;
CartService cartService = new CartService();
AuthService authService = new AuthService();
ProductService productService = new ProductService();
OrderService orderService = new OrderService();
// Initialize demo accounts
try
{
    authService.CreateDemoAccounts();
}
catch (Exception)
{
    Console.WriteLine("Could not initialize demo accounts.");
    return;
}

// Main Menu
while (true)
{
    Console.WriteLine("\n===== E-Commerce System =====");
    Console.WriteLine("1. Login");
    Console.WriteLine("2. Exit");
    Console.Write("Choose: ");

    string? choice = Console.ReadLine();

    if (choice == "2")
    {
        Console.WriteLine("Goodbye!");
        break;
    }

    if (choice != "1")
    {
        Console.WriteLine("Invalid choice. Try again.");
        continue;
    }

    Console.Write("Username: ");
    string username = Console.ReadLine() ?? "";

    Console.Write("Password: ");
    string password = Console.ReadLine() ?? "";

    try
    {
        User? user = authService.Login(username, password);

        if (user == null)
        {
            Console.WriteLine("Invalid username or password.");
            continue;
        }

        Console.WriteLine($"Welcome, {user.UserName}!");

        if (user.Role == "Admin")
        {
            AdminMenu(productService, orderService);
        }
        else if (user.Role == "Customer")
        {
            CustomerMenu(user, productService, cartService, orderService);
        }
        else
        {
            Console.WriteLine("Unknown user role.");
        }
    }
    catch (Exception)
    {
        Console.WriteLine("Login failed. Please try again.");
    }
}

// Admin Menu
static void AdminMenu(
    ProductService productService,
    OrderService orderService)
{
    while (true)
    {
        Console.WriteLine("\n===== Admin Menu =====");
        Console.WriteLine("1. View Products");
        Console.WriteLine("2. Add Product");
        Console.WriteLine("3. Edit Product");
        Console.WriteLine("4. View All Orders");
        Console.WriteLine("5. Logout");
        Console.Write("Choose: ");

        string? choice = Console.ReadLine();

        try
        {
            switch (choice)
            {
                case "1":
                    {
                        List<Product> products =
                            productService.GetAvailableProducts();

                        if (products.Count == 0)
                        {
                            Console.WriteLine("No products available.");
                            break;
                        }

                        Console.WriteLine("\n===== Products =====");

                        foreach (Product product in products)
                        {
                            Console.WriteLine(
                                $"ID: {product.ProductId} | " +
                                $"Name: {product.Name} | " +
                                $"Price: {product.Price:F2} EGP | " +
                                $"Stock: {product.StockQuantity}");
                        }

                        break;
                    }

                case "2":
                    {
                        Console.Write("Product name: ");
                        string name = Console.ReadLine() ?? "";

                        if (string.IsNullOrWhiteSpace(name))
                        {
                            Console.WriteLine("Product name is required.");
                            break;
                        }

                        Console.Write("Description: ");
                        string? description = Console.ReadLine();

                        Console.Write("Price: ");

                        if (!decimal.TryParse(
                            Console.ReadLine(), out decimal price) ||
                            price < 0)
                        {
                            Console.WriteLine("Invalid price.");
                            break;
                        }

                        Console.Write("Stock quantity: ");

                        if (!int.TryParse(
                            Console.ReadLine(), out int stock) ||
                            stock < 0)
                        {
                            Console.WriteLine("Invalid stock quantity.");
                            break;
                        }

                        Product newProduct = new Product
                        {
                            Name = name.Trim(),
                            Description =
                                string.IsNullOrWhiteSpace(description)
                                    ? null
                                    : description.Trim(),
                            Price = price,
                            StockQuantity = stock,
                            IsActive = true
                        };

                        productService.AddProduct(newProduct);

                        Console.WriteLine("Product added successfully!");
                        break;
                    }

                case "3":
                    {
                        Console.Write("Enter product ID: ");

                        if (!int.TryParse(
                            Console.ReadLine(), out int productId) ||
                            productId <= 0)
                        {
                            Console.WriteLine("Invalid product ID.");
                            break;
                        }

                        if (!productService.ProductExists(productId))
                        {
                            Console.WriteLine("Product not found.");
                            break;
                        }

                        Console.Write("New product name: ");
                        string newName = Console.ReadLine() ?? "";

                        if (string.IsNullOrWhiteSpace(newName))
                        {
                            Console.WriteLine("Product name is required.");
                            break;
                        }

                        Console.Write("New description: ");
                        string? newDescription = Console.ReadLine();

                        Console.Write("New price: ");

                        if (!decimal.TryParse(
                            Console.ReadLine(), out decimal newPrice) ||
                            newPrice < 0)
                        {
                            Console.WriteLine("Invalid price.");
                            break;
                        }

                        Console.Write("New stock quantity: ");

                        if (!int.TryParse(
                            Console.ReadLine(), out int newStock) ||
                            newStock < 0)
                        {
                            Console.WriteLine("Invalid stock quantity.");
                            break;
                        }

                        Console.Write("Is active? (y/n): ");
                        string? activeChoice =
                            Console.ReadLine()?.Trim().ToLowerInvariant();

                        if (activeChoice != "y" &&
                            activeChoice != "n")
                        {
                            Console.WriteLine("Enter y or n.");
                            break;
                        }

                        Product updatedProduct = new Product
                        {
                            ProductId = productId,
                            Name = newName.Trim(),
                            Description =
                                string.IsNullOrWhiteSpace(newDescription)
                                    ? null
                                    : newDescription.Trim(),
                            Price = newPrice,
                            StockQuantity = newStock,
                            IsActive = activeChoice == "y"
                        };

                        bool updated =
                            productService.UpdateProduct(updatedProduct);

                        Console.WriteLine(
                            updated
                                ? "Product updated successfully!"
                                : "Product was not updated.");

                        break;
                    }

                case "4":
                    {
                        List<Order> orders = orderService.GetAllOrders();

                        if (orders.Count == 0)
                        {
                            Console.WriteLine("No orders found.");
                            break;
                        }

                        Console.WriteLine("\n===== All Orders =====");

                        foreach (Order order in orders)
                        {
                            Console.WriteLine(
                                $"Order ID: {order.OrderId} | " +
                                $"Customer: {order.Username} | " +
                                $"Date: {order.OrderDate:g} | " +
                                $"Total: {order.TotalAmount:F2} EGP | " +
                                $"Status: {order.Status}");
                        }

                        break;
                    }

                case "5":
                    Console.WriteLine("Logging out...");
                    return;

                default:
                    Console.WriteLine("Invalid choice. Try again.");
                    break;
            }
        }
        catch (Exception)
        {
            Console.WriteLine(
                "Operation failed. Check your database and input values.");
        }
    }
}

// Customer Menu
static void CustomerMenu(
    User user,
    ProductService productService,
    CartService cartService,
      OrderService orderService)
{
    while (true)
    {
        Console.WriteLine("\n===== Customer Menu =====");
        Console.WriteLine("1. View Products");
        Console.WriteLine("2. Add Product to Cart");
        Console.WriteLine("3. View Cart");
        Console.WriteLine("4. Checkout");
        Console.WriteLine("5. View My Orders");
        Console.WriteLine("6. Logout");
        Console.Write("Choose: ");

        string? choice = Console.ReadLine();

        try
        {
            switch (choice)
            {
                case "1":
                    {
                        List<Product> products =
                            productService.GetAvailableProducts();

                        if (products.Count == 0)
                        {
                            Console.WriteLine("No products available.");
                            break;
                        }

                        Console.WriteLine("\n===== Available Products =====");

                        foreach (Product product in products)
                        {
                            Console.WriteLine(
                                $"ID: {product.ProductId} | " +
                                $"Name: {product.Name} | " +
                                $"Price: {product.Price:F2} EGP | " +
                                $"Stock: {product.StockQuantity}");
                        }

                        break;
                    }

                case "2":
                    {
                        Console.Write("Enter product ID: ");

                        if (!int.TryParse(
                            Console.ReadLine(), out int productId) ||
                            productId <= 0)
                        {
                            Console.WriteLine("Invalid product ID.");
                            break;
                        }

                        Console.Write("Enter quantity: ");

                        if (!int.TryParse(
                            Console.ReadLine(), out int quantity) ||
                            quantity <= 0)
                        {
                            Console.WriteLine(
                                "Quantity must be greater than zero.");
                            break;
                        }

                        string result = cartService.AddToCart(
                            user.UserId,
                            productId,
                            quantity);

                        Console.WriteLine(result);

                        break;
                    }

                case "3":
                    {
                        List<CartItem> cartItems =
                            cartService.GetCartItems(user.UserId);

                        if (cartItems.Count == 0)
                        {
                            Console.WriteLine("Your cart is empty.");
                            break;
                        }

                        decimal total = 0;

                        Console.WriteLine("\n===== Your Shopping Cart =====");

                        foreach (CartItem item in cartItems)
                        {
                            Console.WriteLine(
                                $"Product ID: {item.ProductId} | " +
                                $"Name: {item.ProductName} | " +
                                $"Price: {item.UnitPrice:F2} EGP | " +
                                $"Quantity: {item.Quantity} | " +
                                $"Subtotal: {item.LineTotal:F2} EGP");

                            total += item.LineTotal;
                        }

                        Console.WriteLine("------------------------------");
                        Console.WriteLine(
                            $"Total: {total:F2} EGP");

                        break;
                    }

                case "4":
                    {
                        Console.WriteLine(orderService.Checkout(user.UserId));
                        break;
                    }

                case "5":
                    {
                        List<Order> orders =
                            orderService.GetCustomerOrders(user.UserId);

                        if (orders.Count == 0)
                        {
                            Console.WriteLine("You have no orders yet.");
                            break;
                        }

                        Console.WriteLine("\n===== My Orders =====");

                        foreach (Order order in orders)
                        {
                            Console.WriteLine(
                                $"Order ID: {order.OrderId} | " +
                                $"Date: {order.OrderDate:g} | " +
                                $"Total: {order.TotalAmount:F2} EGP | " +
                                $"Status: {order.Status}");
                        }

                        break;
                    }
                case "6":
                    Console.WriteLine("Logging out...");
                    return;

                default:
                    Console.WriteLine("Invalid choice. Try again.");
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                $"Operation failed: {ex.Message}");
        }
    }
}