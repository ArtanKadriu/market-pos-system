# MarketPOS

Production-style single-PC supermarket Point of Sale and Inventory Management desktop application.

## Stack

- C# Windows Forms
- .NET 8 Windows
- SQLite via `Microsoft.Data.Sqlite`
- Database file compatible with DB Browser for SQLite

## Default Login

- Username: `admin`
- Password: `admin123`

Change this before real use.

## How To Run

1. Open `MarketPOS.sln` or `MarketPOS.csproj` in Visual Studio.
2. Restore NuGet packages if Visual Studio asks.
3. Press `F5`.

The database is created automatically at:

`bin\Debug\net8.0-windows\Data\marketpos.db`

The full SQL schema is in:

`Database\schema.sql`

## Modules Included

- Login with Admin, Manager, and Cashier roles
- Role-aware sidebar navigation
- Dashboard with sales/revenue/product/low-stock KPIs and top sellers
- Product CRUD with barcode, category, cost, sale price, stock, min stock
- POS cashier checkout with barcode keyboard scanner support, cart, discounts, tax, cash/card payment, change, receipt printing
- Transactional sale processing with automatic stock deduction
- Inventory stock in, stock out, adjustment, and movement history
- Supplier CRUD
- Purchases and stock receiving
- Reports for daily sales, monthly sales, profit, top-selling products, inventory value
- CSV export compatible with Excel
- Returns/refunds with stock returned
- Settings for tax, currency, store name, and receipt footer
- Database backup and restore
- Audit logs
- ESC/POS receipt printer placeholder through the print workflow

## Notes

- Money values use `decimal`.
- Sales and refunds use SQLite transactions.
- The repository/service structure keeps database access separate from UI code so the project can later be migrated to SQL Server.
- The UI is programmatic WinForms rather than designer-generated, which keeps the full source compact and easy to review.
