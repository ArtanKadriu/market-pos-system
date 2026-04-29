namespace MarketPOS.Models;

public enum UserRole { Admin = 1, Manager = 2, Cashier = 3 }
public enum PaymentMethod { Cash, Card }

public sealed class User
{
    public int Id { get; set; }
    public string Username { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public string FullName { get; set; } = "";
    public UserRole Role { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class Product
{
    public int Id { get; set; }
    public string Barcode { get; set; } = "";
    public string Name { get; set; } = "";
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = "";
    public decimal CostPrice { get; set; }
    public decimal SalePrice { get; set; }
    public decimal StockQuantity { get; set; }
    public decimal MinStockAlert { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class Category { public int Id { get; set; } public string Name { get; set; } = ""; }
public sealed class Supplier { public int Id { get; set; } public string Name { get; set; } = ""; public string Phone { get; set; } = ""; public string Email { get; set; } = ""; public string Address { get; set; } = ""; }

public sealed class CartItem
{
    public int ProductId { get; set; }
    public string Barcode { get; set; } = "";
    public string ProductName { get; set; } = "";
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Discount { get; set; }
    public decimal TaxRate { get; set; }
    public decimal LineSubtotal => Quantity * UnitPrice;
    public decimal LineTax => Math.Round((LineSubtotal - Discount) * TaxRate / 100m, 2);
    public decimal LineTotal => LineSubtotal - Discount + LineTax;
}

public sealed class Sale
{
    public int Id { get; set; }
    public string InvoiceNo { get; set; } = "";
    public DateTime SaleDate { get; set; }
    public int UserId { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Discount { get; set; }
    public decimal Tax { get; set; }
    public decimal Total { get; set; }
    public decimal Paid { get; set; }
    public decimal ChangeAmount { get; set; }
    public string PaymentMethod { get; set; } = "";
    public bool IsReturned { get; set; }
}

public sealed class AppSettings
{
    public string StoreName { get; set; } = "MarketPOS Supermarket";
    public string Currency { get; set; } = "$";
    public decimal TaxRate { get; set; }
    public string ReceiptFooter { get; set; } = "Thank you for shopping with us!";
}
