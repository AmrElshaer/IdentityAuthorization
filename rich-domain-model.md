

Copilot Instructions for Creating Rich Domain Entities in C# (DDD Style)

When generating C# domain entities, always follow these rules and structure.
All entities must follow Domain-Driven Design (DDD) patterns and match the example template below.

General Rules for Every Entity

All properties must use private setters.

Include a private parameterless constructor for EF Core.

Include a public constructor that enforces required fields and validation.

Never expose public setters — use Update() methods to modify state.

Always add:

#region Properties

#region Constructors

#region Methods

Encapsulate all business logic inside the entity.

Maintain timestamps (CreatedAt, UpdatedAt) if the entity inherits from Auditable.

Entity Structure Template (Copilot MUST follow this)
public class EntityName : Auditable
{
    #region Properties
    // Add properties with private setters
    public string Example { get; private set; } = string.Empty;
    #endregion

    #region Constructors
    // EF Core private constructor
    private EntityName() { }

    // Public constructor (required fields only)
    public EntityName(string example)
    {
        // Add validation logic here
        Example = example;

        CreatedAt = DateTime.UtcNow;
    }
    #endregion

    #region Methods
    public void Update(string example)
    {
        // Add validation logic here
        Example = example;

        UpdatedAt = DateTime.UtcNow;
    }
    #endregion
}

Reference Example (Copilot Should Mirror This Style)
public class Contract : Auditable
{
    #region Properties
    public string Number { get; private set; } = string.Empty;
    public string Address { get; private set; } = string.Empty;
    #endregion

    #region Constructors
    private Contract() { }

    public Contract(string number, string address)
    {
        // add validation logic here
        Number = number;
        Address = address;
        CreatedAt = DateTime.UtcNow;
    }
    #endregion

    #region Methods
    public void Update(string number, string address)
    {
        // add validation logic here
        Number = number;
        Address = address;
        UpdatedAt = DateTime.UtcNow;
    }
    #endregion
}

Validation Rules Copilot Must Add

Validate required arguments in constructors:

if (string.IsNullOrWhiteSpace(number))
    throw new ArgumentException("Number is required.", nameof(number));


Enforce domain rules inside methods:

if (address.Length < 5)
    throw new DomainException("Address is too short.");

Copilot Summary

Always generate domain entities with private setters, rich behavior, validation in constructors, Update methods, timestamps, and the exact structure shown in the Contract example.