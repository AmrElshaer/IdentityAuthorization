using System.Security.Claims;
using IdentityAuthorization.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;

namespace IdentityAuthorization.Features;

/// <summary>
/// Inventory Management Feature - Handles CRUD operations for inventory.
/// </summary>
public static class InventoryManagement
{
    #region Request/Response Models
    
    /// <summary>
    /// Request model for creating a new inventory record.
    /// </summary>
    /// <param name="ProductId">The product identifier.</param>
    /// <param name="InitialQuantity">The initial stock quantity.</param>
    /// <param name="Location">Optional storage location.</param>
    public record CreateInventoryRequest(
        string ProductId,
        int InitialQuantity,
        string? Location = null
    );
    
    /// <summary>
    /// Request model for updating an existing inventory record.
    /// </summary>
    /// <param name="ProductId">The product identifier.</param>
    /// <param name="CurrentQuantity">The current stock quantity.</param>
    /// <param name="Location">Optional storage location.</param>
    public record UpdateInventoryRequest(
        string ProductId,
        int CurrentQuantity,
        string? Location = null
    );
    
    /// <summary>
    /// Request model for adjusting stock quantity.
    /// </summary>
    /// <param name="Quantity">The quantity to add or adjust.</param>
    public record StockAdjustmentRequest(int Quantity);
    
    /// <summary>
    /// Request model for updating storage location.
    /// </summary>
    /// <param name="Location">The new storage location.</param>
    public record UpdateLocationRequest(string? Location);
    
    /// <summary>
    /// Response model for inventory data.
    /// </summary>
    public record InventoryResponse(
        int Id,
        string ProductId,
        int InitialQuantity,
        int CurrentQuantity,
        string? Location,
        string CreatedBy,
        string? UpdatedBy,
        DateTime CreatedAt,
        DateTime? UpdatedAt,
        bool IsLowStock,
        bool IsOutOfStock,
        int StockVariance
    );
    
    #endregion
    
    #region Endpoint Mapping
    
    /// <summary>
    /// Maps all inventory management endpoints to the application.
    /// </summary>
    /// <param name="app">The web application.</param>
    public static void MapEndpoint(WebApplication app)
    {
        var inventoryGroup = app.MapGroup("/api/inventory")
            .WithTags("Inventory Management")
            .RequireAuthorization();
        
        // CREATE - Create new inventory record
        inventoryGroup.MapPost("/", CreateInventory)
            .WithName("CreateInventory")
            .WithOpenApi()
            .Produces<InventoryResponse>(StatusCodes.Status201Created)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);
        
        // READ - Get all inventories
        inventoryGroup.MapGet("/", GetAllInventories)
            .WithName("GetAllInventories")
            .WithOpenApi()
            .Produces<List<InventoryResponse>>(StatusCodes.Status200OK);
        
        // READ - Get inventory by ID
        inventoryGroup.MapGet("/{id:int}", GetInventoryById)
            .WithName("GetInventoryById")
            .WithOpenApi()
            .Produces<InventoryResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
        
        // READ - Get inventory by product ID
        inventoryGroup.MapGet("/product/{productId}", GetInventoryByProductId)
            .WithName("GetInventoryByProductId")
            .WithOpenApi()
            .Produces<InventoryResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
        
        // READ - Get low stock inventories
        inventoryGroup.MapGet("/low-stock", GetLowStockInventories)
            .WithName("GetLowStockInventories")
            .WithOpenApi()
            .Produces<List<InventoryResponse>>(StatusCodes.Status200OK);
        
        // READ - Get out of stock inventories
        inventoryGroup.MapGet("/out-of-stock", GetOutOfStockInventories)
            .WithName("GetOutOfStockInventories")
            .WithOpenApi()
            .Produces<List<InventoryResponse>>(StatusCodes.Status200OK);
        
        // UPDATE - Update inventory
        inventoryGroup.MapPut("/{id:int}", UpdateInventory)
            .WithName("UpdateInventory")
            .WithOpenApi()
            .Produces<InventoryResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);
        
        // UPDATE - Add stock
        inventoryGroup.MapPost("/{id:int}/add-stock", AddStock)
            .WithName("AddStock")
            .WithOpenApi()
            .Produces<InventoryResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);
        
        // UPDATE - Remove stock
        inventoryGroup.MapPost("/{id:int}/remove-stock", RemoveStock)
            .WithName("RemoveStock")
            .WithOpenApi()
            .Produces<InventoryResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);
        
        // UPDATE - Adjust quantity
        inventoryGroup.MapPost("/{id:int}/adjust", AdjustQuantity)
            .WithName("AdjustQuantity")
            .WithOpenApi()
            .Produces<InventoryResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);
        
        // UPDATE - Update location
        inventoryGroup.MapPatch("/{id:int}/location", UpdateLocation)
            .WithName("UpdateLocation")
            .WithOpenApi()
            .Produces<InventoryResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);
        
        // DELETE - Delete inventory
        inventoryGroup.MapDelete("/{id:int}", DeleteInventory)
            .WithName("DeleteInventory")
            .WithOpenApi()
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);
    }
    
    #endregion
    
    #region Endpoint Handlers
    
    /// <summary>
    /// Creates a new inventory record.
    /// </summary>
    private static async Task<IResult> CreateInventory(
        CreateInventoryRequest request,
        ClaimsPrincipal user,
        ApplicationDbContext dbContext,
        ILogger<CreateInventoryRequest> logger)
    {
        try
        {
            // Get user ID from claims
            var userId = user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            if (string.IsNullOrWhiteSpace(userId))
            {
                logger.LogWarning("Create inventory attempt without valid user claim");
                return Results.Unauthorized();
            }
            
            // Check if inventory already exists for this product
            var existingInventory = await dbContext.Inventories
                .FirstOrDefaultAsync(i => i.ProductId == request.ProductId);
            
            if (existingInventory != null)
            {
                logger.LogWarning("Inventory already exists for product: {ProductId}", request.ProductId);
                return Results.BadRequest(new ProblemDetails
                {
                    Title = "Inventory Already Exists",
                    Detail = $"Inventory for product '{request.ProductId}' already exists.",
                    Status = StatusCodes.Status400BadRequest
                });
            }
            
            // Create new inventory
            var inventory = new Inventory(
                productId: request.ProductId,
                initialQuantity: request.InitialQuantity,
                createdBy: userId,
                location: request.Location
            );
            
            dbContext.Inventories.Add(inventory);
            await dbContext.SaveChangesAsync();
            
            logger.LogInformation("Inventory created for product: {ProductId} by user: {UserId}", 
                request.ProductId, userId);
            
            var response = MapToResponse(inventory);
            return Results.Created($"/api/inventory/{inventory.Id}", response);
        }
        catch (ArgumentException ex)
        {
            logger.LogWarning(ex, "Validation error creating inventory: {Message}", ex.Message);
            return Results.BadRequest(new ProblemDetails
            {
                Title = "Validation Error",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
        catch (DomainException ex)
        {
            logger.LogWarning(ex, "Domain error creating inventory: {Message}", ex.Message);
            return Results.BadRequest(new ProblemDetails
            {
                Title = "Domain Rule Violation",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
    }
    
    /// <summary>
    /// Gets all inventory records.
    /// </summary>
    private static async Task<IResult> GetAllInventories(
        ApplicationDbContext dbContext,
        ILogger<CreateInventoryRequest> logger,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        var skip = (page - 1) * pageSize;
        
        var inventories = await dbContext.Inventories
            .OrderByDescending(i => i.CreatedAt)
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync();
        
        var response = inventories.Select(MapToResponse).ToList();
        
        logger.LogInformation("Retrieved {Count} inventories (page {Page}, size {PageSize})", 
            inventories.Count, page, pageSize);
        
        return Results.Ok(response);
    }
    
    /// <summary>
    /// Gets inventory by ID.
    /// </summary>
    private static async Task<IResult> GetInventoryById(
        int id,
        ApplicationDbContext dbContext,
        ILogger<CreateInventoryRequest> logger)
    {
        var inventory = await dbContext.Inventories.FindAsync(id);
        
        if (inventory == null)
        {
            logger.LogWarning("Inventory not found: {Id}", id);
            return Results.NotFound();
        }
        
        var response = MapToResponse(inventory);
        return Results.Ok(response);
    }
    
    /// <summary>
    /// Gets inventory by product ID.
    /// </summary>
    private static async Task<IResult> GetInventoryByProductId(
        string productId,
        ApplicationDbContext dbContext,
        ILogger<CreateInventoryRequest> logger)
    {
        var inventory = await dbContext.Inventories
            .FirstOrDefaultAsync(i => i.ProductId == productId);
        
        if (inventory == null)
        {
            logger.LogWarning("Inventory not found for product: {ProductId}", productId);
            return Results.NotFound();
        }
        
        var response = MapToResponse(inventory);
        return Results.Ok(response);
    }
    
    /// <summary>
    /// Gets low stock inventories.
    /// </summary>
    private static async Task<IResult> GetLowStockInventories(
        ApplicationDbContext dbContext,
        ILogger<CreateInventoryRequest> logger,
        [FromQuery] int threshold = 10)
    {
        var inventories = await dbContext.Inventories
            .Where(i => i.CurrentQuantity < threshold && i.CurrentQuantity > 0)
            .OrderBy(i => i.CurrentQuantity)
            .ToListAsync();
        
        var response = inventories.Select(MapToResponse).ToList();
        
        logger.LogInformation("Retrieved {Count} low stock inventories (threshold: {Threshold})", 
            inventories.Count, threshold);
        
        return Results.Ok(response);
    }
    
    /// <summary>
    /// Gets out of stock inventories.
    /// </summary>
    private static async Task<IResult> GetOutOfStockInventories(
        ApplicationDbContext dbContext,
        ILogger<CreateInventoryRequest> logger)
    {
        var inventories = await dbContext.Inventories
            .Where(i => i.CurrentQuantity == 0)
            .OrderByDescending(i => i.UpdatedAt ?? i.CreatedAt)
            .ToListAsync();
        
        var response = inventories.Select(MapToResponse).ToList();
        
        logger.LogInformation("Retrieved {Count} out of stock inventories", inventories.Count);
        
        return Results.Ok(response);
    }
    
    /// <summary>
    /// Updates an existing inventory record.
    /// </summary>
    private static async Task<IResult> UpdateInventory(
        int id,
        UpdateInventoryRequest request,
        ClaimsPrincipal user,
        ApplicationDbContext dbContext,
        ILogger<CreateInventoryRequest> logger)
    {
        try
        {
            var userId = user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            if (string.IsNullOrWhiteSpace(userId))
            {
                logger.LogWarning("Update inventory attempt without valid user claim");
                return Results.Unauthorized();
            }
            
            var inventory = await dbContext.Inventories.FindAsync(id);
            if (inventory == null)
            {
                logger.LogWarning("Inventory not found: {Id}", id);
                return Results.NotFound();
            }
            
            inventory.Update(
                productId: request.ProductId,
                currentQuantity: request.CurrentQuantity,
                updatedBy: userId,
                location: request.Location
            );
            
            await dbContext.SaveChangesAsync();
            
            logger.LogInformation("Inventory updated: {Id} by user: {UserId}", id, userId);
            
            var response = MapToResponse(inventory);
            return Results.Ok(response);
        }
        catch (ArgumentException ex)
        {
            logger.LogWarning(ex, "Validation error updating inventory: {Message}", ex.Message);
            return Results.BadRequest(new ProblemDetails
            {
                Title = "Validation Error",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
        catch (DomainException ex)
        {
            logger.LogWarning(ex, "Domain error updating inventory: {Message}", ex.Message);
            return Results.BadRequest(new ProblemDetails
            {
                Title = "Domain Rule Violation",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
    }
    
    /// <summary>
    /// Adds stock to an inventory.
    /// </summary>
    private static async Task<IResult> AddStock(
        int id,
        StockAdjustmentRequest request,
        ClaimsPrincipal user,
        ApplicationDbContext dbContext,
        ILogger<CreateInventoryRequest> logger)
    {
        try
        {
            var userId = user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            if (string.IsNullOrWhiteSpace(userId))
                return Results.Unauthorized();
            
            var inventory = await dbContext.Inventories.FindAsync(id);
            if (inventory == null)
                return Results.NotFound();
            
            inventory.AddStock(request.Quantity, userId);
            await dbContext.SaveChangesAsync();
            
            logger.LogInformation("Added {Quantity} stock to inventory {Id}", request.Quantity, id);
            
            return Results.Ok(MapToResponse(inventory));
        }
        catch (DomainException ex)
        {
            return Results.BadRequest(new ProblemDetails { Detail = ex.Message });
        }
    }
    
    /// <summary>
    /// Removes stock from an inventory.
    /// </summary>
    private static async Task<IResult> RemoveStock(
        int id,
        StockAdjustmentRequest request,
        ClaimsPrincipal user,
        ApplicationDbContext dbContext,
        ILogger<CreateInventoryRequest> logger)
    {
        try
        {
            var userId = user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            if (string.IsNullOrWhiteSpace(userId))
                return Results.Unauthorized();
            
            var inventory = await dbContext.Inventories.FindAsync(id);
            if (inventory == null)
                return Results.NotFound();
            
            inventory.RemoveStock(request.Quantity, userId);
            await dbContext.SaveChangesAsync();
            
            logger.LogInformation("Removed {Quantity} stock from inventory {Id}", request.Quantity, id);
            
            return Results.Ok(MapToResponse(inventory));
        }
        catch (DomainException ex)
        {
            return Results.BadRequest(new ProblemDetails { Detail = ex.Message });
        }
    }
    
    /// <summary>
    /// Adjusts the quantity of an inventory.
    /// </summary>
    private static async Task<IResult> AdjustQuantity(
        int id,
        StockAdjustmentRequest request,
        ClaimsPrincipal user,
        ApplicationDbContext dbContext,
        ILogger<CreateInventoryRequest> logger)
    {
        try
        {
            var userId = user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            if (string.IsNullOrWhiteSpace(userId))
                return Results.Unauthorized();
            
            var inventory = await dbContext.Inventories.FindAsync(id);
            if (inventory == null)
                return Results.NotFound();
            
            inventory.AdjustQuantity(request.Quantity, userId);
            await dbContext.SaveChangesAsync();
            
            logger.LogInformation("Adjusted inventory {Id} quantity to {Quantity}", id, request.Quantity);
            
            return Results.Ok(MapToResponse(inventory));
        }
        catch (DomainException ex)
        {
            return Results.BadRequest(new ProblemDetails { Detail = ex.Message });
        }
    }
    
    /// <summary>
    /// Updates the location of an inventory.
    /// </summary>
    private static async Task<IResult> UpdateLocation(
        int id,
        UpdateLocationRequest request,
        ClaimsPrincipal user,
        ApplicationDbContext dbContext,
        ILogger<CreateInventoryRequest> logger)
    {
        try
        {
            var userId = user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            if (string.IsNullOrWhiteSpace(userId))
                return Results.Unauthorized();
            
            var inventory = await dbContext.Inventories.FindAsync(id);
            if (inventory == null)
                return Results.NotFound();
            
            inventory.UpdateLocation(request.Location, userId);
            await dbContext.SaveChangesAsync();
            
            logger.LogInformation("Updated location for inventory {Id}", id);
            
            return Results.Ok(MapToResponse(inventory));
        }
        catch (DomainException ex)
        {
            return Results.BadRequest(new ProblemDetails { Detail = ex.Message });
        }
    }
    
    /// <summary>
    /// Deletes an inventory record.
    /// </summary>
    private static async Task<IResult> DeleteInventory(
        int id,
        ApplicationDbContext dbContext,
        ILogger<CreateInventoryRequest> logger)
    {
        var inventory = await dbContext.Inventories.FindAsync(id);
        if (inventory == null)
        {
            logger.LogWarning("Inventory not found for deletion: {Id}", id);
            return Results.NotFound();
        }
        
        dbContext.Inventories.Remove(inventory);
        await dbContext.SaveChangesAsync();
        
        logger.LogInformation("Inventory deleted: {Id}", id);
        
        return Results.NoContent();
    }
    
    #endregion
    
    #region Helper Methods
    
    /// <summary>
    /// Maps an Inventory entity to an InventoryResponse.
    /// </summary>
    private static InventoryResponse MapToResponse(Inventory inventory)
    {
        return new InventoryResponse(
            Id: inventory.Id,
            ProductId: inventory.ProductId,
            InitialQuantity: inventory.InitialQuantity,
            CurrentQuantity: inventory.CurrentQuantity,
            Location: inventory.Location,
            CreatedBy: inventory.CreatedBy,
            UpdatedBy: inventory.UpdatedBy,
            CreatedAt: inventory.CreatedAt,
            UpdatedAt: inventory.UpdatedAt,
            IsLowStock: inventory.IsLowStock(),
            IsOutOfStock: inventory.IsOutOfStock(),
            StockVariance: inventory.GetStockVariance()
        );
    }
    
    #endregion
}
