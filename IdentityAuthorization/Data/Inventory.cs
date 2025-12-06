namespace IdentityAuthorization.Data;

/// <summary>
/// Represents an Inventory entity in the domain.
/// Follows Domain-Driven Design (DDD) principles with encapsulated business logic.
/// Manages product stock levels and inventory adjustments.
/// </summary>
public class Inventory : Auditable
{
    #region Properties
    
    /// <summary>
    /// Gets the product identifier associated with this inventory record.
    /// </summary>
    public string ProductId { get; private set; } = string.Empty;
    
    /// <summary>
    /// Gets the initial quantity when inventory was created.
    /// </summary>
    public int InitialQuantity { get; private set; }
    
    /// <summary>
    /// Gets the current quantity available in stock.
    /// </summary>
    public int CurrentQuantity { get; private set; }
    
    /// <summary>
    /// Gets the storage location for this inventory (optional).
    /// </summary>
    public string? Location { get; private set; }
    
    /// <summary>
    /// Gets the user ID who created this inventory record.
    /// </summary>
    public string CreatedBy { get; private set; } = string.Empty;
    
    /// <summary>
    /// Gets the user ID who last updated this inventory record.
    /// </summary>
    public string? UpdatedBy { get; private set; }
    
    #endregion

    #region Constructors
    
    /// <summary>
    /// Private parameterless constructor for Entity Framework Core.
    /// </summary>
    private Inventory() { }

    /// <summary>
    /// Creates a new Inventory record with required fields and validation.
    /// </summary>
    /// <param name="productId">The product identifier.</param>
    /// <param name="initialQuantity">The initial stock quantity.</param>
    /// <param name="createdBy">The user creating the inventory record.</param>
    /// <param name="location">Optional storage location.</param>
    /// <exception cref="ArgumentException">Thrown when required fields are invalid.</exception>
    /// <exception cref="DomainException">Thrown when domain rules are violated.</exception>
    public Inventory(string productId, int initialQuantity, string createdBy, string? location = null)
    {
        // Validate product ID
        if (string.IsNullOrWhiteSpace(productId))
            throw new ArgumentException("Product ID is required.", nameof(productId));
        
        // Validate initial quantity
        if (initialQuantity < 0)
            throw new DomainException("Initial quantity cannot be negative.");
        
        // Validate created by
        if (string.IsNullOrWhiteSpace(createdBy))
            throw new ArgumentException("Created by user is required.", nameof(createdBy));
        
        // Validate location if provided
        if (location != null && location.Length > 200)
            throw new DomainException("Location cannot exceed 200 characters.");
        
        // Set properties
        ProductId = productId;
        InitialQuantity = initialQuantity;
        CurrentQuantity = initialQuantity; // Current starts as initial
        Location = location;
        CreatedBy = createdBy;
        
        // Set audit timestamp
        CreatedAt = DateTime.UtcNow;
    }
    
    #endregion

    #region Methods
    
    /// <summary>
    /// Updates the inventory with new values and enforces validation rules.
    /// </summary>
    /// <param name="productId">The product identifier.</param>
    /// <param name="currentQuantity">The current stock quantity.</param>
    /// <param name="updatedBy">The user updating the record.</param>
    /// <param name="location">Optional storage location.</param>
    /// <exception cref="ArgumentException">Thrown when required fields are invalid.</exception>
    /// <exception cref="DomainException">Thrown when domain rules are violated.</exception>
    public void Update(string productId, int currentQuantity, string updatedBy, string? location = null)
    {
        // Validate product ID
        if (string.IsNullOrWhiteSpace(productId))
            throw new ArgumentException("Product ID is required.", nameof(productId));
        
        // Validate current quantity
        if (currentQuantity < 0)
            throw new DomainException("Current quantity cannot be negative.");
        
        // Validate updated by
        if (string.IsNullOrWhiteSpace(updatedBy))
            throw new ArgumentException("Updated by user is required.", nameof(updatedBy));
        
        // Validate location if provided
        if (location != null && location.Length > 200)
            throw new DomainException("Location cannot exceed 200 characters.");
        
        // Set properties
        ProductId = productId;
        CurrentQuantity = currentQuantity;
        Location = location;
        UpdatedBy = updatedBy;
        
        // Update audit timestamp
        UpdatedAt = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Adds stock to the current inventory.
    /// </summary>
    /// <param name="quantity">The quantity to add.</param>
    /// <param name="updatedBy">The user making the adjustment.</param>
    /// <exception cref="ArgumentException">Thrown when user is invalid.</exception>
    /// <exception cref="DomainException">Thrown when quantity is invalid.</exception>
    public void AddStock(int quantity, string updatedBy)
    {
        if (quantity <= 0)
            throw new DomainException("Quantity to add must be positive.");
        
        if (string.IsNullOrWhiteSpace(updatedBy))
            throw new ArgumentException("Updated by user is required.", nameof(updatedBy));
        
        CurrentQuantity += quantity;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Removes stock from the current inventory.
    /// </summary>
    /// <param name="quantity">The quantity to remove.</param>
    /// <param name="updatedBy">The user making the adjustment.</param>
    /// <exception cref="ArgumentException">Thrown when user is invalid.</exception>
    /// <exception cref="DomainException">Thrown when quantity is invalid or insufficient stock.</exception>
    public void RemoveStock(int quantity, string updatedBy)
    {
        if (quantity <= 0)
            throw new DomainException("Quantity to remove must be positive.");
        
        if (string.IsNullOrWhiteSpace(updatedBy))
            throw new ArgumentException("Updated by user is required.", nameof(updatedBy));
        
        if (CurrentQuantity < quantity)
            throw new DomainException($"Insufficient stock. Current: {CurrentQuantity}, Requested: {quantity}");
        
        CurrentQuantity -= quantity;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Sets the current quantity directly (for corrections/adjustments).
    /// </summary>
    /// <param name="quantity">The new quantity.</param>
    /// <param name="updatedBy">The user making the adjustment.</param>
    /// <exception cref="ArgumentException">Thrown when user is invalid.</exception>
    /// <exception cref="DomainException">Thrown when quantity is invalid.</exception>
    public void AdjustQuantity(int quantity, string updatedBy)
    {
        if (quantity < 0)
            throw new DomainException("Quantity cannot be negative.");
        
        if (string.IsNullOrWhiteSpace(updatedBy))
            throw new ArgumentException("Updated by user is required.", nameof(updatedBy));
        
        CurrentQuantity = quantity;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Updates the storage location for this inventory.
    /// </summary>
    /// <param name="location">The new storage location.</param>
    /// <param name="updatedBy">The user updating the location.</param>
    /// <exception cref="ArgumentException">Thrown when user is invalid.</exception>
    /// <exception cref="DomainException">Thrown when location is invalid.</exception>
    public void UpdateLocation(string? location, string updatedBy)
    {
        if (string.IsNullOrWhiteSpace(updatedBy))
            throw new ArgumentException("Updated by user is required.", nameof(updatedBy));
        
        if (location != null && location.Length > 200)
            throw new DomainException("Location cannot exceed 200 characters.");
        
        Location = location;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Checks if the inventory is low on stock.
    /// </summary>
    /// <param name="threshold">The low stock threshold (default: 10).</param>
    /// <returns>True if stock is below threshold; otherwise, false.</returns>
    public bool IsLowStock(int threshold = 10)
    {
        return CurrentQuantity < threshold;
    }
    
    /// <summary>
    /// Checks if the inventory is out of stock.
    /// </summary>
    /// <returns>True if no stock available; otherwise, false.</returns>
    public bool IsOutOfStock()
    {
        return CurrentQuantity == 0;
    }
    
    /// <summary>
    /// Calculates the stock variance from initial quantity.
    /// </summary>
    /// <returns>The difference between current and initial quantity.</returns>
    public int GetStockVariance()
    {
        return CurrentQuantity - InitialQuantity;
    }
    
    /// <summary>
    /// Validates if stock can be reserved for an order.
    /// </summary>
    /// <param name="requestedQuantity">The quantity to reserve.</param>
    /// <returns>True if sufficient stock; otherwise, false.</returns>
    public bool CanReserve(int requestedQuantity)
    {
        return CurrentQuantity >= requestedQuantity && requestedQuantity > 0;
    }
    
    #endregion
}
