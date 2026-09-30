using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var ordersFolder = Path.Combine(AppContext.BaseDirectory, "orders");
Directory.CreateDirectory(ordersFolder);

app.MapGet("/", () => Results.Ok(new
{
    status = "ok",
    service = "Integrity Rising Gumroad webhook receiver",
    message = "Use POST /api/gumroad to receive Gumroad webhook events."
}));

app.MapGet("/health", () => Results.Ok(new
{
    status = "healthy",
    timestamp = DateTime.UtcNow
}));

app.MapPost("/api/gumroad", async (HttpContext context) =>
{
    using var reader = new StreamReader(context.Request.Body);
    var payload = await reader.ReadToEndAsync();

    if (string.IsNullOrWhiteSpace(payload))
    {
        return Results.BadRequest(new
        {
            success = false,
            message = "Empty payload received."
        });
    }

    var fileName = Path.Combine(ordersFolder, $"order-{DateTime.UtcNow:yyyyMMdd-HHmmssfff}.json");

    try
    {
        using var jsonDocument = JsonDocument.Parse(payload);
        var root = jsonDocument.RootElement;

        var customerEmail = root.TryGetProperty("email", out var emailElement)
            ? emailElement.GetString()
            : "unknown";

        var productName = root.TryGetProperty("product_name", out var productElement)
            ? productElement.GetString()
            : "unknown";

        var price = root.TryGetProperty("price", out var priceElement)
            ? priceElement.ToString()
            : "unknown";

        await File.WriteAllTextAsync(fileName, payload);

        Console.WriteLine($"[Gumroad] New order saved: {fileName}");
        Console.WriteLine($"[Gumroad] Customer: {customerEmail}");
        Console.WriteLine($"[Gumroad] Product: {productName}");
        Console.WriteLine($"[Gumroad] Price: {price}");

        return Results.Ok(new
        {
            success = true,
            message = "Order saved successfully.",
            file = Path.GetFileName(fileName),
            customerEmail,
            productName,
            price
        });
    }
    catch (Exception ex)
    {
        await File.WriteAllTextAsync(fileName, payload);
        Console.WriteLine($"[Gumroad] Payload saved but parsing failed: {ex.Message}");

        return Results.Ok(new
        {
            success = true,
            message = "Payload was saved even though parsing failed.",
            error = ex.Message
        });
    }
});

app.Run();
