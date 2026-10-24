CREATE DATABASE EcommerceSDb;
GO
USE EcommerceSDb;
GO
CREATE TABLE Users(
UserId INT IDENTITY(1,1) PRIMARY KEY,
Username NVARCHAR(50) NOT NULL UNIQUE,
PasswordHash NVARCHAR (255)NOT NULL,
 Role NVARCHAR(20) NOT NULL,
    CONSTRAINT CK_Users_Role
        CHECK (Role IN ('Customer', 'Admin'))
);
CREATE TABLE Products (
ProductId INT IDENTITY (1,1) PRIMARY KEY,
Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500) NULL,
    Price DECIMAL(18,2) NOT NULL,
    StockQuantity INT NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1,

    CONSTRAINT CK_Products_Price
        CHECK (Price >= 0),
        CONSTRAINT CK_Products_Stock
        CHECK (StockQuantity  >= 0),
);
CREATE TABLE CartItems(

 CartItemId INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    ProductId INT NOT NULL,
    Quantity INT NOT NULL,

    CONSTRAINT FK_CartItems_Users
        FOREIGN KEY (UserId) REFERENCES Users(UserId),

    CONSTRAINT FK_CartItems_Products
        FOREIGN KEY (ProductId) REFERENCES Products(ProductId),

    CONSTRAINT UQ_CartItems_User_Product
        UNIQUE (UserId, ProductId),

    CONSTRAINT CK_CartItems_Quantity
        CHECK (Quantity > 0)
);

CREATE TABLE Orders
(
    OrderId INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    OrderDate DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    TotalAmount DECIMAL(18,2) NOT NULL,
    Status NVARCHAR(30) NOT NULL DEFAULT 'Pending',

    CONSTRAINT FK_Orders_Users
        FOREIGN KEY (UserId) REFERENCES Users(UserId),

    CONSTRAINT CK_Orders_Total
        CHECK (TotalAmount >= 0)
);

CREATE TABLE OrderItems
(
    OrderItemId INT IDENTITY(1,1) PRIMARY KEY,
    OrderId INT NOT NULL,
    ProductId INT NOT NULL,
    ProductName NVARCHAR(100) NOT NULL,
    UnitPrice DECIMAL(18,2) NOT NULL,
    Quantity INT NOT NULL,

    CONSTRAINT FK_OrderItems_Orders
        FOREIGN KEY (OrderId) REFERENCES Orders(OrderId),

    CONSTRAINT FK_OrderItems_Products
        FOREIGN KEY (ProductId) REFERENCES Products(ProductId),

    CONSTRAINT CK_OrderItems_Price
        CHECK (UnitPrice >= 0),

    CONSTRAINT CK_OrderItems_Quantity
        CHECK (Quantity > 0)
);

PRINT 'EcommerceDb created successfully!';

INSERT INTO Products (Name, Description, Price, StockQuantity, IsActive)
VALUES
('Laptop', 'Laptop for daily use', 25000.00, 10, 1),
('Mouse', 'Wireless mouse', 450.00, 30, 1),
('Keyboard', 'Mechanical keyboard', 1200.00, 15, 1),
('Headphones', 'Over-ear headphones', 850.00, 20, 1);
GO

SELECT * FROM Products;