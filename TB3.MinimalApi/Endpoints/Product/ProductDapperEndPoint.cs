using Azure.Core;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Numerics;
using TB3.MinimalApi.Dtos;

namespace TB3.MinimalApi.Endpoints.Product
{
    public static class ProductDapperEndPoint
    {
        private static readonly string _connectionString = "Server=.;Database=Batch3MiniPOS;User ID=sa;Password=sasa@123;TrustServerCertificate=True;";

        public static void UseProductByDapperEndpoint(this IEndpointRouteBuilder app)
        {
            app.MapGet("/productByDapper", () =>
            {
                using var connection = new SqlConnection(_connectionString);

                string query = @"SELECT ProductId, ProductName, Quantity, Price, DeleteFlag, CreatedDateTime, ModifiedDateTime FROM Tbl_Product
                WHERE DeleteFlag = 0
                ORDER BY ProductId DESC";

                IEnumerable<ProductGetListResponseDto> productGetListResponseDtos = connection.Query<ProductGetListResponseDto>(query);

                return Results.Ok(productGetListResponseDtos);
            })
            .WithName("GetProductsByDapper")
            .WithOpenApi();

            app.MapGet("/productByDapper/{id}", (int id) =>
            {
                using var connection = new SqlConnection(_connectionString);

                string query = @"
                SELECT ProductId, ProductName, Quantity, Price, DeleteFlag, CreatedDateTime, ModifiedDateTime FROM Tbl_Product
                WHERE DeleteFlag = 0 AND ProductId = @ID";

                IEnumerable<ProductGetResponseDto> productGetResponseDtos = connection.Query<ProductGetResponseDto>(query, new { ID = id });

                return Results.Ok(productGetResponseDtos);
            })
                .WithName("GetProductByDapper")
                .WithOpenApi();

            app.MapPost("/productByDapper", (ProductCreateRequestDto requestDto) =>
            {
                using var connection = new SqlConnection(_connectionString);

                string query = @"
                INSERT INTO Tbl_Product (ProductName, Quantity, Price, DeleteFlag, CreatedDateTime, ModifiedDateTime)
                VALUES (@ProductName, @Quantity, @Price, 0, @CreatedDateTime, NULL)";

                int isSuccess = connection.Execute(query,
                    new { ProductName = requestDto.ProductName, Quantity = requestDto.Quantity, Price = requestDto.Price, CreatedDateTime = DateTime.Now });

                string message = isSuccess > 0 ? "Saving Successful." : "Saving Failed.";

                return Results.Ok(message);

            })
            .WithName("CreateProductByDapper")
            .WithOpenApi();

            app.MapPatch("/productByDapper/{id}", (int id, ProductPatchRequestDto requestDto) =>
            {
                using var connection = new SqlConnection(_connectionString);
                string query = @"
                SELECT ProductId, ProductName, Quantity, Price, DeleteFlag, CreatedDateTime, ModifiedDateTime FROM Tbl_Product
                WHERE DeleteFlag = 0 AND ProductId = @ID";

                var responseDtos = connection.QueryFirstOrDefault<ProductGetResponseDto>(query, new { ID = id });

                if (responseDtos == null)
                {
                    return Results.NotFound("Product not found.");
                }

                if (!string.IsNullOrEmpty(requestDto.ProductName))
                    responseDtos.ProductName = requestDto.ProductName;

                if (requestDto.Price is not null && requestDto.Price > 0)
                    responseDtos.Price = requestDto.Price.Value;

                if (requestDto.Quantity is not null && requestDto.Quantity > 0)
                    responseDtos.Quantity = requestDto.Quantity.Value;

                responseDtos.ModifiedDateTime = DateTime.Now;

                string updateQuery = @"
                UPDATE Tbl_Product SET ProductName = @ProductName, Quantity = @Quantity, Price = @Price, ModifiedDateTime = @ModifiedDateTime
                WHERE ProductId = @ProductId";

                int result = connection.Execute(updateQuery, new { ProductName = responseDtos.ProductName, Quantity = responseDtos.Quantity, Price = responseDtos.Price, ModifiedDateTime = responseDtos.ModifiedDateTime, ProductId = id });

                string message = result > 0 ? "Update Successful." : "Update Failed.";
                return Results.Ok(message);
            })
                .WithName("PatchProductByDapper")
                .WithOpenApi();

            app.MapPut("/productByDapper/{id}", (int id, ProductUpdateRequestDto requestDto) =>
            {
                using var connection = new SqlConnection(_connectionString);

                // 1. Check existing product
                string selectQuery = @"
                SELECT ProductId, ProductName, Quantity, Price, DeleteFlag, CreatedDateTime, ModifiedDateTime
                FROM Tbl_Product
                WHERE DeleteFlag = 0 AND ProductId = @ID";

                var existingProduct = connection.QueryFirstOrDefault<ProductGetResponseDto>(selectQuery, new { ID = id });

                if (existingProduct is null)
                {
                    return Results.NotFound("Product not found.");
                }

                // 2. Full update (PUT means REPLACE all fields)
                string updateQuery = @"
                UPDATE Tbl_Product
                SET ProductName = @ProductName,
                    Quantity = @Quantity,
                    Price = @Price,
                    ModifiedDateTime = @ModifiedDateTime
               WHERE ProductId = @ProductId";

                int result = connection.Execute(updateQuery, new
                {
                    ProductId = id,
                    ProductName = requestDto.ProductName,
                    Quantity = requestDto.Quantity,
                    Price = requestDto.Price,
                    ModifiedDateTime = DateTime.Now
                });

                string message = result > 0 ? "Update Successful (PUT)." : "Update Failed.";
                return Results.Ok(message);

            })
            .WithName("PutProductByDapper")
            .WithOpenApi();

            app.MapDelete("/productByDapper/{id}", (int id) =>
            {
                using var connection = new SqlConnection(_connectionString);

                // 1. Check existing product
                string selectQuery = @"
                SELECT ProductId FROM Tbl_Product
                WHERE DeleteFlag = 0 AND ProductId = @ID";

                var product = connection.QueryFirstOrDefault<int?>(selectQuery, new { ID = id });
                if (product is null)
                {
                    return Results.NotFound("Product not found.");
                }

                // 2. Soft delete
                string deleteQuery = @"
                UPDATE Tbl_Product
                    SET DeleteFlag = 1,
                    ModifiedDateTime = @ModifiedDateTime
                WHERE ProductId = @ProductId";

                int result = connection.Execute(deleteQuery, new
                {
                    ProductId = id,
                    ModifiedDateTime = DateTime.Now
                });

                string message = result > 0 ? "Delete Successful." : "Delete Failed.";
                return Results.Ok(message);
            })
            .WithName("DeleteProductByDapper")
            .WithOpenApi();

        }
    }
}
