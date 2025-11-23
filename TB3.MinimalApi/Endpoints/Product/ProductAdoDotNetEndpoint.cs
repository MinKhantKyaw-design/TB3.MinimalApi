using Microsoft.Data.SqlClient;
using TB3.MinimalApi.Dtos;

namespace TB3.MinimalApi.Endpoints.Product;

public static class ProductAdoDotNetEndpoint
{
    private static readonly string _connectionString = "Server=.;Database=Batch3MiniPOS;User ID=sa;Password=sasa@123;TrustServerCertificate=True;";
    
    public static void UseProductAdoDotNetEndpoint(this IEndpointRouteBuilder app)
    {
        
        app.MapGet("/product", () =>
        {
            SqlConnection connection = new SqlConnection(_connectionString);
            connection.Open();
            
            List<ProductGetListResponseDto> lts = new List<ProductGetListResponseDto>();

            string query = @"
                SELECT ProductId, ProductName, Quantity, Price, DeleteFlag, CreatedDateTime, ModifiedDateTime FROM Tbl_Product
                WHERE DeleteFlag = 0
                ORDER BY ProductId DESC";

            SqlCommand cmd = new SqlCommand(query, connection);
            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var item = new ProductGetListResponseDto()
                {
                    ProductId = Convert.ToInt32(reader["ProductId"]),
                    ProductName = Convert.ToString(reader["ProductName"])!,
                    Quantity = Convert.ToInt32(reader["Quantity"]),
                    Price = Convert.ToDecimal(reader["Price"]),
                    DeleteFlag = Convert.ToBoolean(reader["DeleteFlag"]),
                    CreatedDateTime = Convert.ToDateTime(reader["CreatedDateTime"]),
                    ModifiedDateTime = Convert.ToDateTime(reader["ModifiedDateTime"])
                };

                lts.Add(item);
            }

            connection.Close();

            return Results.Ok(lts);
        })
        .WithName("GetProducts")
        .WithOpenApi();

        app.MapGet("/product/{id}", (int id) =>
        {
            var item = new ProductGetResponseDto();
            SqlConnection connection = new SqlConnection(_connectionString);
            connection.Open();

            string query = @"
                SELECT ProductId, ProductName, Quantity, Price, DeleteFlag, CreatedDateTime, ModifiedDateTime FROM Tbl_Product
                WHERE DeleteFlag = 0 AND ProductId = @ProductId";

            SqlCommand cmd = new SqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@ProductId", id);

            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                item = new ProductGetResponseDto()
                {
                    ProductId = Convert.ToInt32(reader["ProductId"]),
                    ProductName = Convert.ToString(reader["ProductName"])!,
                    Quantity = Convert.ToInt32(reader["Quantity"]),
                    Price = Convert.ToDecimal(reader["Price"]),
                    DeleteFlag = Convert.ToBoolean(reader["DeleteFlag"]),
                    CreatedDateTime = Convert.ToDateTime(reader["CreatedDateTime"]),
                    ModifiedDateTime = Convert.ToDateTime(reader["ModifiedDateTime"])
                };
            }

            connection.Close();

            return Results.Ok(item);
        })
        .WithName("GetProduct")
        .WithOpenApi();

        app.MapPost("/product", (ProductCreateRequestDto request) =>
        {
            SqlConnection connection = new SqlConnection(_connectionString);
            connection.Open();

            string query = @"
                INSERT INTO Tbl_Product (ProductName, Quantity, Price, DeleteFlag, CreatedDateTime, ModifiedDateTime)
                VALUES (@ProductName, @Quantity, @Price, 0, @CreatedDateTime, NULL)";

            SqlCommand cmd = new SqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@ProductName", request.ProductName);
            cmd.Parameters.AddWithValue("@Quantity", request.Quantity);
            cmd.Parameters.AddWithValue("@Price", request.Price);
            cmd.Parameters.AddWithValue("@CreatedDateTime", DateTime.Now);

            int result = cmd.ExecuteNonQuery();

            connection.Close();

            string message = result > 0 ? "Saving Successful." : "Saving Failed.";
            return Results.Ok(message);
        })
        .WithName("CreateProduct")
        .WithOpenApi();

        app.MapPut("product/{id}", (int id, ProductUpdateRequestDto request) =>
        {
            SqlConnection connection = new SqlConnection(_connectionString);
            connection.Open();

            string query = @"
                UPDATE Tbl_Product SET ProductName = @ProductName, Quantity = @Quantity, Price = @Price, DeleteFlag = 0, ModifiedDateTime = @ModifiedDateTime
                WHERE ProductId = @ProductId";

            SqlCommand cmd = new SqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@ProductId", id);
            cmd.Parameters.AddWithValue("@ProductName", request.ProductName);
            cmd.Parameters.AddWithValue("@Quantity", request.Quantity);
            cmd.Parameters.AddWithValue("@Price", request.Price);
            cmd.Parameters.AddWithValue("@ModifiedDateTime", DateTime.Now);

            int result = cmd.ExecuteNonQuery();

            connection.Close();

            string message = result > 0 ? "Updating Successful." : "Updating Failed.";
            return Results.Ok(message);
        })
        .WithName("UpdateProduct")
        .WithOpenApi();

        app.MapPatch("/product/{id}", (int id, ProductPatchRequestDto request) =>
        {
            string conditions = "";
            if (!string.IsNullOrEmpty(request.ProductName))
                conditions += "ProductName = @ProductName,";
            if (request.Quantity is not null && request.Quantity > 0)
                conditions += "Quantity = @Quantity,";
            if (request.Price is not null && request.Price > 0)
                conditions += "Price = @Price,";
            conditions += "ModifiedDateTime = @ModifiedDateTime";

            if (conditions.Length == 0)
                return Results.BadRequest("Invalid Request");

            conditions = conditions.Substring(0, conditions.Length - 1);

            SqlConnection connection = new SqlConnection(_connectionString);
            connection.Open();

            string query = $@"
                UPDATE Tbl_Product SET {conditions}
                WHERE ProductId = @ProductId";


            SqlCommand cmd = new SqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@ProductId", id);

            if (!string.IsNullOrEmpty(request.ProductName))
                cmd.Parameters.AddWithValue("@ProductName", request.ProductName);
            if (request.Quantity is not null && request.Quantity > 0)
                cmd.Parameters.AddWithValue("@Quantity", request.Quantity);
            if (request.Price is not null && request.Price > 0)
                cmd.Parameters.AddWithValue("@Price", request.Price);
            cmd.Parameters.AddWithValue("@ModifiedDateTime", DateTime.Now);

            int result = cmd.ExecuteNonQuery();

            connection.Close();

            string message = result > 0 ? "Patching Successful." : "Patching Failed.";
            return Results.Ok(message);
        })
        .WithName("PatchProduct")
        .WithOpenApi();

        app.MapDelete("/product/{id}", (int id) =>
        {
            SqlConnection connection = new SqlConnection(_connectionString);
            connection.Open();

            string query = @"
                UPDATE Tbl_Product SET DeleteFlag = 1
                WHERE ProductId = @ProductId";

            SqlCommand cmd = new SqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@ProductId", id);

            int result = cmd.ExecuteNonQuery();

            connection.Close();

            string message = result > 0 ? "Deleting Successful." : "Deleting Failed.";
            return Results.Ok(message);
        })
        .WithName("DeleteProduct")
        .WithOpenApi();
    }
}