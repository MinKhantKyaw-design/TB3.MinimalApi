using TB3.Database.AppDbContextModels;
using TB3.MinimalApi.Dtos;
using TB3.MinimalApi.Endpoints.Product;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();

app.MapGet("/product", () =>
{
    AppDbContext db = new AppDbContext();
    var lst = db.TblProducts
            .OrderByDescending(x => x.ProductId)
            .Where(x => x.DeleteFlag == false)
            .ToList();
    return Results.Ok(lst);
})
.WithName("GetProductList")
.WithOpenApi();

app.MapGet("/product/{id}", (int id) =>
{
    AppDbContext db = new AppDbContext();
    var item = db.TblProducts.FirstOrDefault(x => x.ProductId == id);
    if (item is null)
    {
        return Results.NotFound("Product not found.");
    }

    var response = new ProductGetResponseDto
    {
        ProductName = item.ProductName
    };
    return Results.Ok(response);
})
.WithName("GetProduct")
.WithOpenApi();

app.MapPost("/product", (ProductCreateRequestDto request) =>
{
    using var db = new AppDbContext();
    db.TblProducts.Add(new TblProduct
    {
        CreatedDateTime = DateTime.Now,
        Price = request.Price,
        DeleteFlag = false,
        ProductName = request.ProductName,
        Quantity = request.Quantity,
    });

    int result = db.SaveChanges();
    string message = result > 0 ? "Saving Successful." : "Saving Failed.";

    return Results.Ok(message);
})
.WithName("CreateProduct")
.WithOpenApi();

app.MapPut("/product/{id}", (int id, ProductUpdateRequestDto request) =>
{
    using var db = new AppDbContext();
    var item = db.TblProducts.FirstOrDefault(x => x.ProductId == id);
    if (item is null)
        return Results.NotFound("Product not found.");

    item.ProductName = request.ProductName;
    item.Price = request.Price;
    item.Quantity = request.Quantity;
    item.ModifiedDateTime = DateTime.Now;

    int result = db.SaveChanges();
    string message = result > 0 ? "Updating Successful." : "Updating Failed.";

    return Results.Ok(message);
})
.WithName("UpdateProduct")
.WithOpenApi();

app.MapPatch("/product/{id}", (int id, ProductPatchRequestDto request) =>
{
    using var db = new AppDbContext();
    var item = db.TblProducts.FirstOrDefault(x => x.ProductId == id);
    if (item is null)
        return Results.NotFound("Product not found.");

    if (!string.IsNullOrEmpty(request.ProductName))
        item.ProductName = request.ProductName;

    if (request.Price is not null && request.Price > 0)
        item.Price = request.Price.Value;

    if (request.Quantity is not null && request.Quantity > 0)
        item.Quantity = request.Quantity.Value;

    item.ModifiedDateTime = DateTime.Now;

    int result = db.SaveChanges();
    string message = result > 0 ? "Patching Successful." : "Patching Failed.";

    return Results.Ok(message);
})
.WithName("PatchProduct")
.WithOpenApi();

app.MapDelete("/product/{id}", (int id) =>
{
    using var db = new AppDbContext();
    var item = db.TblProducts.FirstOrDefault(x => x.ProductId == id);
    if (item is null)
        return Results.NotFound("Product not found.");

    item.DeleteFlag = true;

    int result = db.SaveChanges();
    string message = result > 0 ? "Deleting Successful." : "Deleting Failed.";

    return Results.Ok(message);
})
.WithName("DeleteProduct")
.WithOpenApi();

app.UseProductAdoDotNetEndpoint();

app.UseProductByDapperEndpoint();

app.Run();

internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
