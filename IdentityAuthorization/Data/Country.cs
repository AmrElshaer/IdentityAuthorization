namespace IdentityAuthorization.Data;

/// <summary>
/// Represents a Country entity in the domain.
/// Follows Domain-Driven Design (DDD) principles with encapsulated business logic.
/// </summary>
public class Country : Auditable
{
    #region Properties
    
    /// <summary>
    /// Gets the country code (ISO 3166-1 alpha-2).
    /// </summary>
    public string Code { get; private set; } = string.Empty;
    
    /// <summary>
    /// Gets the country name.
    /// </summary>
    public string Name { get; private set; } = string.Empty;
    
    /// <summary>
    /// Gets the country's ISO 3166-1 alpha-3 code.
    /// </summary>
    public string Iso3Code { get; private set; } = string.Empty;
    
    /// <summary>
    /// Gets the country's phone code (e.g., +1, +20, +44).
    /// </summary>
    public string PhoneCode { get; private set; } = string.Empty;
    
    /// <summary>
    /// Gets whether the country is currently active.
    /// </summary>
    public bool IsActive { get; private set; }
    
    #endregion

    #region Constructors
    
    /// <summary>
    /// Private parameterless constructor for Entity Framework Core.
    /// </summary>
    private Country() { }

    /// <summary>
    /// Creates a new Country with required fields and validation.
    /// </summary>
    /// <param name="code">The ISO 3166-1 alpha-2 country code.</param>
    /// <param name="name">The country name.</param>
    /// <param name="iso3Code">The ISO 3166-1 alpha-3 country code.</param>
    /// <param name="phoneCode">The country's phone code.</param>
    /// <exception cref="ArgumentException">Thrown when required fields are invalid.</exception>
    /// <exception cref="DomainException">Thrown when domain rules are violated.</exception>
    public Country(string code, string name, string iso3Code, string phoneCode)
    {
        // Validate country code
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Country code is required.", nameof(code));
        
        if (code.Length != 2)
            throw new DomainException("Country code must be exactly 2 characters (ISO 3166-1 alpha-2).");
        
        // Validate country name
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Country name is required.", nameof(name));
        
        if (name.Length < 2)
            throw new DomainException("Country name must be at least 2 characters long.");
        
        // Validate ISO3 code
        if (string.IsNullOrWhiteSpace(iso3Code))
            throw new ArgumentException("ISO3 code is required.", nameof(iso3Code));
        
        if (iso3Code.Length != 3)
            throw new DomainException("ISO3 code must be exactly 3 characters (ISO 3166-1 alpha-3).");
        
        // Validate phone code
        if (string.IsNullOrWhiteSpace(phoneCode))
            throw new ArgumentException("Phone code is required.", nameof(phoneCode));
        
        if (!phoneCode.StartsWith("+"))
            throw new DomainException("Phone code must start with '+'.");
        
        // Set properties
        Code = code.ToUpperInvariant();
        Name = name;
        Iso3Code = iso3Code.ToUpperInvariant();
        PhoneCode = phoneCode;
        IsActive = true;
        
        // Set audit timestamp
        CreatedAt = DateTime.UtcNow;
    }
    
    #endregion

    #region Methods
    
    /// <summary>
    /// Updates the country with new values and enforces validation rules.
    /// </summary>
    /// <param name="code">The ISO 3166-1 alpha-2 country code.</param>
    /// <param name="name">The country name.</param>
    /// <param name="iso3Code">The ISO 3166-1 alpha-3 country code.</param>
    /// <param name="phoneCode">The country's phone code.</param>
    /// <exception cref="ArgumentException">Thrown when required fields are invalid.</exception>
    /// <exception cref="DomainException">Thrown when domain rules are violated.</exception>
    public void Update(string code, string name, string iso3Code, string phoneCode)
    {
        // Validate country code
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Country code is required.", nameof(code));
        
        if (code.Length != 2)
            throw new DomainException("Country code must be exactly 2 characters (ISO 3166-1 alpha-2).");
        
        // Validate country name
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Country name is required.", nameof(name));
        
        if (name.Length < 2)
            throw new DomainException("Country name must be at least 2 characters long.");
        
        // Validate ISO3 code
        if (string.IsNullOrWhiteSpace(iso3Code))
            throw new ArgumentException("ISO3 code is required.", nameof(iso3Code));
        
        if (iso3Code.Length != 3)
            throw new DomainException("ISO3 code must be exactly 3 characters (ISO 3166-1 alpha-3).");
        
        // Validate phone code
        if (string.IsNullOrWhiteSpace(phoneCode))
            throw new ArgumentException("Phone code is required.", nameof(phoneCode));
        
        if (!phoneCode.StartsWith("+"))
            throw new DomainException("Phone code must start with '+'.");
        
        // Set properties
        Code = code.ToUpperInvariant();
        Name = name;
        Iso3Code = iso3Code.ToUpperInvariant();
        PhoneCode = phoneCode;
        
        // Update audit timestamp
        UpdatedAt = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Updates only the country name.
    /// </summary>
    /// <param name="name">The new country name.</param>
    /// <exception cref="ArgumentException">Thrown when name is invalid.</exception>
    /// <exception cref="DomainException">Thrown when domain rules are violated.</exception>
    public void UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Country name is required.", nameof(name));
        
        if (name.Length < 2)
            throw new DomainException("Country name must be at least 2 characters long.");
        
        Name = name;
        UpdatedAt = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Activates the country.
    /// </summary>
    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Deactivates the country.
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Checks if the country code matches the given code (case-insensitive).
    /// </summary>
    /// <param name="code">The country code to check.</param>
    /// <returns>True if codes match; otherwise, false.</returns>
    public bool HasCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return false;
        
        return Code.Equals(code, StringComparison.OrdinalIgnoreCase);
    }
    
    /// <summary>
    /// Validates if the country's phone code matches a given phone number prefix.
    /// </summary>
    /// <param name="phoneNumber">The phone number to validate.</param>
    /// <returns>True if phone number starts with country's phone code; otherwise, false.</returns>
    public bool ValidatePhoneNumber(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return false;
        
        return phoneNumber.StartsWith(PhoneCode);
    }
    
    #endregion
}
