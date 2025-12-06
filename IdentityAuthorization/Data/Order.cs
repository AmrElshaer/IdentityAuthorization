namespace IdentityAuthorization.Data;

/// <summary>
/// Represents an Order entity in the domain.
/// Follows Domain-Driven Design (DDD) principles with encapsulated business logic.
/// </summary>
public class Order : Auditable
{
    #region Properties
    
    /// <summary>
    /// Gets the unique order number.
    /// </summary>
    public string Number { get; private set; } = string.Empty;
    
    /// <summary>
    /// Gets the customer identifier associated with this order.
    /// </summary>
    public string CustomerId { get; private set; } = string.Empty;
    
    /// <summary>
    /// Gets the delivery address for this order.
    /// </summary>
    public string Address { get; private set; } = string.Empty;
    
    #endregion

    #region Constructors
    
    /// <summary>
    /// Private parameterless constructor for Entity Framework Core.
    /// </summary>
    private Order() { }

    /// <summary>
    /// Creates a new Order with required fields and validation.
    /// </summary>
    /// <param name="number">The unique order number.</param>
    /// <param name="customerId">The customer identifier.</param>
    /// <param name="address">The delivery address.</param>
    /// <exception cref="ArgumentException">Thrown when required fields are invalid.</exception>
    /// <exception cref="DomainException">Thrown when domain rules are violated.</exception>
    public Order(string number, string customerId, string address)
    {
        // Validate order number
        if (string.IsNullOrWhiteSpace(number))
            throw new ArgumentException("Order number is required.", nameof(number));
        
        if (number.Length < 3)
            throw new DomainException("Order number must be at least 3 characters long.");
        
        // Validate customer ID
        if (string.IsNullOrWhiteSpace(customerId))
            throw new ArgumentException("Customer ID is required.", nameof(customerId));
        
        // Validate address
        if (string.IsNullOrWhiteSpace(address))
            throw new ArgumentException("Address is required.", nameof(address));
        
        if (address.Length < 5)
            throw new DomainException("Address must be at least 5 characters long.");
        
        // Set properties
        Number = number;
        CustomerId = customerId;
        Address = address;
        
        // Set audit timestamp
        CreatedAt = DateTime.UtcNow;
    }
    
    #endregion

    #region Methods
    
    /// <summary>
    /// Updates the order with new values and enforces validation rules.
    /// </summary>
    /// <param name="number">The new order number.</param>
    /// <param name="customerId">The new customer identifier.</param>
    /// <param name="address">The new delivery address.</param>
    /// <exception cref="ArgumentException">Thrown when required fields are invalid.</exception>
    /// <exception cref="DomainException">Thrown when domain rules are violated.</exception>
    public void Update(string number, string customerId, string address)
    {
        // Validate order number
        if (string.IsNullOrWhiteSpace(number))
            throw new ArgumentException("Order number is required.", nameof(number));
        
        if (number.Length < 3)
            throw new DomainException("Order number must be at least 3 characters long.");
        
        // Validate customer ID
        if (string.IsNullOrWhiteSpace(customerId))
            throw new ArgumentException("Customer ID is required.", nameof(customerId));
        
        // Validate address
        if (string.IsNullOrWhiteSpace(address))
            throw new ArgumentException("Address is required.", nameof(address));
        
        if (address.Length < 5)
            throw new DomainException("Address must be at least 5 characters long.");
        
        // Set properties
        Number = number;
        CustomerId = customerId;
        Address = address;
        
        // Update audit timestamp
        UpdatedAt = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Updates only the delivery address for this order.
    /// </summary>
    /// <param name="address">The new delivery address.</param>
    /// <exception cref="ArgumentException">Thrown when address is invalid.</exception>
    /// <exception cref="DomainException">Thrown when domain rules are violated.</exception>
    public void UpdateAddress(string address)
    {
        if (string.IsNullOrWhiteSpace(address))
            throw new ArgumentException("Address is required.", nameof(address));
        
        if (address.Length < 5)
            throw new DomainException("Address must be at least 5 characters long.");
        
        Address = address;
        UpdatedAt = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Changes the customer associated with this order.
    /// </summary>
    /// <param name="customerId">The new customer identifier.</param>
    /// <exception cref="ArgumentException">Thrown when customer ID is invalid.</exception>
    public void ChangeCustomer(string customerId)
    {
        if (string.IsNullOrWhiteSpace(customerId))
            throw new ArgumentException("Customer ID is required.", nameof(customerId));
        
        CustomerId = customerId;
        UpdatedAt = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Validates if the order can be shipped.
    /// </summary>
    /// <returns>True if the order is valid for shipping; otherwise, false.</returns>
    public bool CanShip()
    {
        return !string.IsNullOrWhiteSpace(Number) &&
               !string.IsNullOrWhiteSpace(CustomerId) &&
               !string.IsNullOrWhiteSpace(Address) &&
               Address.Length >= 5;
    }
    
    #endregion
}
