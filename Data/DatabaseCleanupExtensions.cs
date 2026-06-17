using Microsoft.EntityFrameworkCore;

namespace DSM.Data {
    public static class DatabaseCleanupExtensions {
        public static async Task RepairLegacyStockRowsAsync(this WebApplication app) {
            using var scope = app.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDatabaseContext>();

            await context.Database.ExecuteSqlRawAsync(@"
IF OBJECT_ID(N'[dbo].[Stocks]', N'U') IS NOT NULL
BEGIN
    IF COL_LENGTH('dbo.Stocks', 'Product') IS NOT NULL
        EXEC(N'UPDATE [dbo].[Stocks] SET [Product] = '''' WHERE [Product] IS NULL');

    IF COL_LENGTH('dbo.Stocks', 'ProductRef') IS NOT NULL
        EXEC(N'UPDATE [dbo].[Stocks] SET [ProductRef] = '''' WHERE [ProductRef] IS NULL');

    IF COL_LENGTH('dbo.Stocks', 'Location') IS NOT NULL
        EXEC(N'UPDATE [dbo].[Stocks] SET [Location] = '''' WHERE [Location] IS NULL');

    IF COL_LENGTH('dbo.Stocks', 'Quantity') IS NOT NULL
        EXEC(N'UPDATE [dbo].[Stocks] SET [Quantity] = 0 WHERE [Quantity] IS NULL');

    IF COL_LENGTH('dbo.Stocks', 'LastReceiptDate') IS NOT NULL
        EXEC(N'UPDATE [dbo].[Stocks] SET [LastReceiptDate] = GETDATE() WHERE [LastReceiptDate] IS NULL');

    IF COL_LENGTH('dbo.Stocks', 'CreatedAt') IS NOT NULL
        EXEC(N'UPDATE [dbo].[Stocks] SET [CreatedAt] = GETDATE() WHERE [CreatedAt] IS NULL');

    IF COL_LENGTH('dbo.Stocks', 'UpdatedAt') IS NOT NULL
        EXEC(N'UPDATE [dbo].[Stocks] SET [UpdatedAt] = GETDATE() WHERE [UpdatedAt] IS NULL');
END
");
        }
    }
}
