# ?? Country Entity - Complete Documentation

## Overview

The **Country** entity represents a country in the system following **Domain-Driven Design (DDD)** principles with rich domain behavior and encapsulated business logic.

---

## ?? **Entity Structure**

### **Inheritance**
```csharp
public class Country : Auditable
```

**Inherits from:** `Auditable` base class
- Provides `Id` (primary key)
- Provides `CreatedAt` timestamp
- Provides `UpdatedAt` timestamp

---

## ??? **Properties**

### **Code** (ISO 3166-1 alpha-2)
```csharp
public string Code { get; private set; } = string.Empty;
```
- **Type:** `string`
- **Required:** Yes
- **Length:** Exactly 2 characters (fixed length)
- **Format:** Uppercase (e.g., "US", "GB", "EG")
- **Index:** Unique index
- **Description:** ISO 3166-1 alpha-2 country code

### **Name**
```csharp
public string Name { get; private set; } = string.Empty;
```
- **Type:** `string`
- **Required:** Yes
- **Min Length:** 2 characters
- **Max Length:** 100 characters
- **Index:** Non-unique index
- **Description:** Full country name
- **Examples:** "United States", "United Kingdom", "Egypt"

### **Iso3Code** (ISO 3166-1 alpha-3)
```csharp
public string Iso3Code { get; private set; } = string.Empty;
```
- **Type:** `string`
- **Required:** Yes
- **Length:** Exactly 3 characters (fixed length)
- **Format:** Uppercase (e.g., "USA", "GBR", "EGY")
- **Index:** Unique index
- **Description:** ISO 3166-1 alpha-3 country code

### **PhoneCode**
```csharp
public string PhoneCode { get; private set; } = string.Empty;
```
- **Type:** `string`
- **Required:** Yes
- **Max Length:** 10 characters
- **Format:** Must start with "+" (e.g., "+1", "+44", "+20")
- **Description:** International dialing code

### **IsActive**
```csharp
public bool IsActive { get; private set; }
```
- **Type:** `bool`
- **Required:** Yes
- **Default:** `true`
- **Index:** Non-unique index
- **Description:** Indicates if the country is currently active in the system

---

## ?? **Constructors**

### **Private Constructor (EF Core)**
```csharp
private Country() { }
```
**Purpose:** Required by Entity Framework Core for materialization  
**Access:** Private - cannot be called directly

### **Public Constructor**
```csharp
public Country(string code, string name, string iso3Code, string phoneCode)
```

**Parameters:**
- `code` - ISO 3166-1 alpha-2 country code (2 characters)
- `name` - Full country name
- `iso3Code` - ISO 3166-1 alpha-3 country code (3 characters)
- `phoneCode` - International dialing code (must start with "+")

**Validation Rules:**
1. **Code:**
   - Must not be null, empty, or whitespace
   - Must be exactly 2 characters long
   - Automatically converted to uppercase

2. **Name:**
   - Must not be null, empty, or whitespace
   - Must be at least 2 characters long

3. **Iso3Code:**
   - Must not be null, empty, or whitespace
   - Must be exactly 3 characters long
   - Automatically converted to uppercase

4. **PhoneCode:**
   - Must not be null, empty, or whitespace
   - Must start with "+"

**Behavior:**
- Sets `IsActive` to `true` by default
- Sets `CreatedAt` to `DateTime.UtcNow`

**Exceptions:**
- `ArgumentException` - When required fields are null/empty
- `DomainException` - When domain rules are violated

**Example:**
```csharp
var country = new Country(
    code: "US",
    name: "United States",
    iso3Code: "USA",
    phoneCode: "+1"
);
```

---

## ?? **Methods**

### **Update**
```csharp
public void Update(string code, string name, string iso3Code, string phoneCode)
```

**Purpose:** Updates all country properties with validation

**Parameters:**
- `code` - ISO 3166-1 alpha-2 country code
- `name` - Full country name
- `iso3Code` - ISO 3166-1 alpha-3 country code
- `phoneCode` - International dialing code

**Validation:** Same as constructor

**Side Effects:**
- Updates `UpdatedAt` timestamp to `DateTime.UtcNow`

**Exceptions:**
- `ArgumentException` - When required fields are null/empty
- `DomainException` - When domain rules are violated

**Example:**
```csharp
country.Update("GB", "United Kingdom", "GBR", "+44");
```

---

### **UpdateName**
```csharp
public void UpdateName(string name)
```

**Purpose:** Updates only the country name

**Parameters:**
- `name` - The new country name

**Validation:**
- Must not be null, empty, or whitespace
- Must be at least 2 characters long

**Side Effects:**
- Updates `UpdatedAt` timestamp to `DateTime.UtcNow`

**Exceptions:**
- `ArgumentException` - When name is null/empty
- `DomainException` - When name is too short

**Example:**
```csharp
country.UpdateName("United States of America");
```

---

### **Activate**
```csharp
public void Activate()
```

**Purpose:** Activates the country

**Side Effects:**
- Sets `IsActive` to `true`
- Updates `UpdatedAt` timestamp to `DateTime.UtcNow`

**Example:**
```csharp
country.Activate();
```

---

### **Deactivate**
```csharp
public void Deactivate()
```

**Purpose:** Deactivates the country

**Side Effects:**
- Sets `IsActive` to `false`
- Updates `UpdatedAt` timestamp to `DateTime.UtcNow`

**Example:**
```csharp
country.Deactivate();
```

---

### **HasCode**
```csharp
public bool HasCode(string code)
```

**Purpose:** Checks if the country code matches (case-insensitive)

**Parameters:**
- `code` - The country code to check

**Returns:** `bool`
- `true` - Codes match
- `false` - Codes don't match or code is null/empty

**Example:**
```csharp
if (country.HasCode("us"))  // Case-insensitive
{
    // Code matches
}
```

---

### **ValidatePhoneNumber**
```csharp
public bool ValidatePhoneNumber(string phoneNumber)
```

**Purpose:** Validates if a phone number belongs to this country

**Parameters:**
- `phoneNumber` - The phone number to validate

**Returns:** `bool`
- `true` - Phone number starts with country's phone code
- `false` - Phone number doesn't match or is null/empty

**Example:**
```csharp
var isValid = country.ValidatePhoneNumber("+1-555-1234");  // Returns true for US
```

---

## ??? **Database Configuration**

### **Table Information**
- **Schema:** `Domain`
- **Table Name:** `Countries`
- **Primary Key:** `Id` (inherited from `Auditable`)

### **Columns**

| Column Name | Type | Nullable | Max Length | Fixed Length | Default | Description |
|-------------|------|----------|------------|--------------|---------|-------------|
| Id | int | No | - | No | IDENTITY | Primary key |
| Code | nchar | No | 2 | Yes | - | ISO 3166-1 alpha-2 code |
| Name | nvarchar | No | 100 | No | - | Country name |
| Iso3Code | nchar | No | 3 | Yes | - | ISO 3166-1 alpha-3 code |
| PhoneCode | nvarchar | No | 10 | No | - | International dialing code |
| IsActive | bit | No | - | No | 1 (true) | Active status |
| CreatedAt | datetime2 | No | - | No | - | Creation timestamp |
| UpdatedAt | datetime2 | Yes | - | No | NULL | Last update timestamp |

### **Indexes**

| Index Name | Columns | Unique | Purpose |
|------------|---------|--------|---------|
| PK_Countries | Id | Yes | Primary key |
| IX_Countries_Code | Code | Yes | Unique country code lookup |
| IX_Countries_Iso3Code | Iso3Code | Yes | Unique ISO3 code lookup |
| IX_Countries_Name | Name | No | Country name searches |
| IX_Countries_IsActive | IsActive | No | Active country queries |

---

## ?? **Usage Examples**

### **Creating a New Country**

```csharp
try
{
    var country = new Country(
        code: "EG",
        name: "Egypt",
        iso3Code: "EGY",
        phoneCode: "+20"
    );
    
    dbContext.Countries.Add(country);
    await dbContext.SaveChangesAsync();
}
catch (ArgumentException ex)
{
    // Handle validation error
    Console.WriteLine($"Validation Error: {ex.Message}");
}
catch (DomainException ex)
{
    // Handle domain rule violation
    Console.WriteLine($"Domain Error: {ex.Message}");
}
```

### **Updating a Country**

```csharp
var country = await dbContext.Countries
    .FirstOrDefaultAsync(c => c.Code == "EG");

if (country != null)
{
    country.Update(
        code: "EG",
        name: "Arab Republic of Egypt",
        iso3Code: "EGY",
        phoneCode: "+20"
    );
    
    await dbContext.SaveChangesAsync();
}
```

### **Deactivating a Country**

```csharp
var country = await dbContext.Countries.FindAsync(countryId);

if (country != null)
{
    country.Deactivate();
    await dbContext.SaveChangesAsync();
}
```

### **Finding Active Countries**

```csharp
var activeCountries = await dbContext.Countries
    .Where(c => c.IsActive)
    .OrderBy(c => c.Name)
    .ToListAsync();
```

### **Finding Country by Code**

```csharp
var country = await dbContext.Countries
    .FirstOrDefaultAsync(c => c.Code == "US");
```

### **Validating Phone Number**

```csharp
var country = await dbContext.Countries
    .FirstOrDefaultAsync(c => c.Code == "US");

if (country != null)
{
    var isValid = country.ValidatePhoneNumber("+1-555-1234");
    if (isValid)
    {
        // Phone number belongs to this country
    }
}
```

---

## ? **Validation Rules Summary**

### **Constructor & Update Method**

| Field | Validation Rule | Exception Type |
|-------|-----------------|----------------|
| Code | Not null/empty/whitespace | `ArgumentException` |
| Code | Exactly 2 characters | `DomainException` |
| Name | Not null/empty/whitespace | `ArgumentException` |
| Name | Min length: 2 characters | `DomainException` |
| Iso3Code | Not null/empty/whitespace | `ArgumentException` |
| Iso3Code | Exactly 3 characters | `DomainException` |
| PhoneCode | Not null/empty/whitespace | `ArgumentException` |
| PhoneCode | Must start with "+" | `DomainException` |

---

## ??? **DDD Principles Applied**

### 1. ? **Encapsulation**
- All setters are `private`
- State changes only through methods
- Validation enforced at boundaries

### 2. ? **Rich Domain Model**
- Business logic inside entity
- `HasCode()` method for code comparison
- `ValidatePhoneNumber()` for phone validation
- `Activate()`/`Deactivate()` for status management

### 3. ? **Invariants Protection**
- Constructor enforces ISO standards
- Code/Iso3Code automatically uppercased
- IsActive defaults to true
- Timestamps automatically managed

### 4. ? **Explicit State Changes**
- `Update()` for full updates
- `UpdateName()` for name changes
- `Activate()`/`Deactivate()` for status changes

### 5. ? **Self-Validation**
- Entity validates its own state
- Throws domain-specific exceptions
- Clear error messages

---

## ?? **Sample Data (Common Countries)**

```csharp
// United States
new Country("US", "United States", "USA", "+1");

// United Kingdom
new Country("GB", "United Kingdom", "GBR", "+44");

// Egypt
new Country("EG", "Egypt", "EGY", "+20");

// Germany
new Country("DE", "Germany", "DEU", "+49");

// France
new Country("FR", "France", "FRA", "+33");

// Japan
new Country("JP", "Japan", "JPN", "+81");

// China
new Country("CN", "China", "CHN", "+86");

// India
new Country("IN", "India", "IND", "+91");

// Canada
new Country("CA", "Canada", "CAN", "+1");

// Australia
new Country("AU", "Australia", "AUS", "+61");
```

---

## ?? **Anti-Patterns Avoided**

? **Anemic Domain Model** - Entity has rich behavior, not just getters/setters  
? **Public Setters** - All properties have private setters  
? **Missing Validation** - Validation in constructor and all update methods  
? **Data Bags** - Entity contains business logic  
? **No Invariants** - Invariants always protected (ISO standards enforced)  

---

## ?? **ISO 3166 Standards**

The Country entity follows **ISO 3166-1** standards:

- **Alpha-2 code:** Two-letter country codes (e.g., US, GB, EG)
- **Alpha-3 code:** Three-letter country codes (e.g., USA, GBR, EGY)
- **Numeric code:** Not implemented (can be added if needed)

**Reference:** https://www.iso.org/iso-3166-country-codes.html

---

## ?? **Related Entities**

Future relationships that could be added:
- **Customer** - Customers belong to countries
- **Address** - Addresses reference countries
- **Order** - Orders can have shipping countries
- **Currency** - Countries have currencies

---

## ?? **Migration**

### **Creating Migration**

```bash
# Add migration
dotnet ef migrations add AddCountryEntity --project IdentityAuthorization

# Update database
dotnet ef database update --project IdentityAuthorization
```

### **Generated SQL (Example)**

```sql
CREATE TABLE [Domain].[Countries] (
    [Id] int NOT NULL IDENTITY,
    [Code] nchar(2) NOT NULL,
    [Name] nvarchar(100) NOT NULL,
    [Iso3Code] nchar(3) NOT NULL,
    [PhoneCode] nvarchar(10) NOT NULL,
    [IsActive] bit NOT NULL DEFAULT 1,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_Countries] PRIMARY KEY ([Id])
);

CREATE UNIQUE INDEX [IX_Countries_Code] 
    ON [Domain].[Countries] ([Code]);

CREATE UNIQUE INDEX [IX_Countries_Iso3Code] 
    ON [Domain].[Countries] ([Iso3Code]);

CREATE INDEX [IX_Countries_Name] 
    ON [Domain].[Countries] ([Name]);

CREATE INDEX [IX_Countries_IsActive] 
    ON [Domain].[Countries] ([IsActive]);
```

---

## ?? **Unit Testing Examples**

### **Testing Constructor Validation**

```csharp
[Fact]
public void Constructor_WithInvalidCode_ThrowsDomainException()
{
    // Act & Assert
    var exception = Assert.Throws<DomainException>(() =>
        new Country("U", "United States", "USA", "+1"));
    
    Assert.Contains("exactly 2 characters", exception.Message);
}

[Fact]
public void Constructor_WithValidData_CreatesCountry()
{
    // Act
    var country = new Country("US", "United States", "USA", "+1");
    
    // Assert
    Assert.Equal("US", country.Code);
    Assert.Equal("United States", country.Name);
    Assert.Equal("USA", country.Iso3Code);
    Assert.Equal("+1", country.PhoneCode);
    Assert.True(country.IsActive);
    Assert.NotEqual(default, country.CreatedAt);
}

[Fact]
public void Constructor_ConvertsCodeToUppercase()
{
    // Act
    var country = new Country("us", "United States", "usa", "+1");
    
    // Assert
    Assert.Equal("US", country.Code);
    Assert.Equal("USA", country.Iso3Code);
}
```

### **Testing Methods**

```csharp
[Fact]
public void Deactivate_SetsIsActiveToFalse()
{
    // Arrange
    var country = new Country("US", "United States", "USA", "+1");
    
    // Act
    country.Deactivate();
    
    // Assert
    Assert.False(country.IsActive);
    Assert.NotNull(country.UpdatedAt);
}

[Fact]
public void ValidatePhoneNumber_WithMatchingCode_ReturnsTrue()
{
    // Arrange
    var country = new Country("US", "United States", "USA", "+1");
    
    // Act
    var isValid = country.ValidatePhoneNumber("+1-555-1234");
    
    // Assert
    Assert.True(isValid);
}

[Fact]
public void HasCode_WithMatchingCode_ReturnsTrue()
{
    // Arrange
    var country = new Country("US", "United States", "USA", "+1");
    
    // Act
    var hasCode = country.HasCode("us");  // Case-insensitive
    
    // Assert
    Assert.True(hasCode);
}
```

---

**Entity Status:** ? **Production Ready**  
**Last Updated:** January 2025  
**Follows:** DDD Principles + ISO 3166-1 Standards + EF Core Best Practices
