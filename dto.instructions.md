# DTO (Data Transfer Object) Instructions for GitHub Copilot

## Overview
When generating DTOs, always follow these rules to ensure clean, maintainable data contracts between layers.

---

## General Rules for Every DTO

1. **DTOs must be records** (immutable by default in C# 10+)
2. **Use init-only properties** for immutability
3. **Group related DTOs in nested classes** when appropriate
4. **Add XML documentation** for all public DTOs
5. **Use nullable reference types** appropriately
6. **No business logic** in DTOs (pure data structures)
7. **Organize with #region blocks** for clarity

---

## DTO Categories

### 1. Request DTOs
Used for incoming data from API clients

### 2. Response DTOs
Used for outgoing data to API clients

### 3. Internal DTOs
Used for communication between application layers

---

## DTO Structure Template

### Request DTO Template
```csharp
namespace ProjectName.Features.FeatureName;

/// <summary>
/// Request DTO for [Operation] operation.
/// </summary>
/// <param name="PropertyName">Description of the property.</param>
public record OperationRequest(
    string PropertyName,
    int AnotherProperty,
    decimal? OptionalProperty = null
);
```

### Response DTO Template
```csharp
namespace ProjectName.Features.FeatureName;

/// <summary>
/// Response DTO for [Operation] operation.
/// </summary>
/// <param name="Id">The unique identifier.</param>
/// <param name="PropertyName">Description of the property.</param>
public record OperationResponse(
    int Id,
    string PropertyName,
    DateTime CreatedAt
);
```

---

## Reference Examples

### Example 1: Create Course DTOs

```csharp
namespace IdentityAuthorization.Features.Courses;

#region Request DTOs

/// <summary>
/// Request DTO for creating a new course.
/// </summary>
/// <param name="Title">The course title.</param>
/// <param name="Description">The course description.</param>
/// <param name="Category">The course category.</param>
/// <param name="Duration">The course duration in hours.</param>
/// <param name="Level">The course level (Beginner/Intermediate/Advanced).</param>
/// <param name="Instructor">Optional instructor information.</param>
/// <param name="Price">Optional course price (null for free courses).</param>
public record CreateCourseRequest(
    string Title,
    string Description,
    string Category,
    int Duration,
    string Level,
    string? Instructor = null,
    decimal? Price = null
);

/// <summary>
/// Request DTO for updating an existing course.
/// </summary>
/// <param name="Title">The course title.</param>
/// <param name="Description">The course description.</param>
/// <param name="Category">The course category.</param>
/// <param name="Duration">The course duration in hours.</param>
/// <param name="Level">The course level.</param>
/// <param name="Instructor">Optional instructor information.</param>
/// <param name="Price">Optional course price.</param>
public record UpdateCourseRequest(
    string Title,
    string Description,
    string Category,
    int Duration,
    string Level,
    string? Instructor = null,
    decimal? Price = null
);

#endregion

#region Response DTOs

/// <summary>
/// Response DTO containing course details.
/// </summary>
/// <param name="Id">The course identifier.</param>
/// <param name="Title">The course title.</param>
/// <param name="Description">The course description.</param>
/// <param name="Category">The course category.</param>
/// <param name="Duration">The course duration in hours.</param>
/// <param name="Level">The course level.</param>
/// <param name="Instructor">The instructor information.</param>
/// <param name="Price">The course price.</param>
/// <param name="IsFree">Indicates if the course is free.</param>
/// <param name="CreatedBy">The user who created the course.</param>
/// <param name="CreatedAt">The creation timestamp.</param>
/// <param name="UpdatedBy">The user who last updated the course.</param>
/// <param name="UpdatedAt">The last update timestamp.</param>
public record CourseResponse(
    int Id,
    string Title,
    string Description,
    string Category,
    int Duration,
    string Level,
    string? Instructor,
    decimal? Price,
    bool IsFree,
    string CreatedBy,
    DateTime CreatedAt,
    string? UpdatedBy,
    DateTime? UpdatedAt
);

/// <summary>
/// Simplified response DTO for course list items.
/// </summary>
/// <param name="Id">The course identifier.</param>
/// <param name="Title">The course title.</param>
/// <param name="Category">The course category.</param>
/// <param name="Duration">The course duration in hours.</param>
/// <param name="Level">The course level.</param>
/// <param name="Price">The course price.</param>
/// <param name="IsFree">Indicates if the course is free.</param>
public record CourseListItemResponse(
    int Id,
    string Title,
    string Category,
    int Duration,
    string Level,
    decimal? Price,
    bool IsFree
);

#endregion
```

---

## Validation Rules for DTOs

### DO:
? Use records for immutability  
? Add XML documentation  
? Use descriptive parameter names  
? Group with #region blocks  
? Use nullable types appropriately  
? Keep DTOs in the same file as their feature  

### DON'T:
? Add business logic to DTOs  
? Use classes instead of records (unless mutable is required)  
? Mix request and response DTOs without clear separation  
? Create DTOs without documentation  
? Use generic names like "Data" or "Info"  

---

## DTO Naming Conventions

### Request DTOs
- `Create{Entity}Request`
- `Update{Entity}Request`
- `Delete{Entity}Request`
- `Get{Entity}Request`

### Response DTOs
- `{Entity}Response` (full details)
- `{Entity}ListItemResponse` (summary)
- `{Entity}SummaryResponse` (minimal data)

### Query DTOs
- `Get{Entity}ByIdQuery`
- `Get{Entity}ListQuery`
- `Search{Entity}Query`

---

## Complex DTO Example

### Nested DTOs for Related Data

```csharp
namespace IdentityAuthorization.Features.Courses;

/// <summary>
/// Detailed course response with related entities.
/// </summary>
public record CourseDetailResponse(
    int Id,
    string Title,
    string Description,
    string Category,
    int Duration,
    string Level,
    InstructorDto? Instructor,
    decimal? Price,
    bool IsFree,
    List<ModuleDto> Modules,
    CourseStatsDto Stats,
    DateTime CreatedAt
);

/// <summary>
/// Instructor information DTO.
/// </summary>
public record InstructorDto(
    string Id,
    string Name,
    string Email,
    string Bio
);

/// <summary>
/// Course module DTO.
/// </summary>
public record ModuleDto(
    int Id,
    string Title,
    int LessonCount,
    int DurationMinutes
);

/// <summary>
/// Course statistics DTO.
/// </summary>
public record CourseStatsDto(
    int EnrollmentCount,
    decimal AverageRating,
    int ReviewCount
);
```

---

## Pagination DTO Pattern

```csharp
/// <summary>
/// Request DTO for paginated course list.
/// </summary>
/// <param name="Page">The page number (1-based).</param>
/// <param name="PageSize">The number of items per page.</param>
/// <param name="Category">Optional category filter.</param>
/// <param name="Level">Optional level filter.</param>
/// <param name="SearchTerm">Optional search term.</param>
public record GetCoursesRequest(
    int Page = 1,
    int PageSize = 20,
    string? Category = null,
    string? Level = null,
    string? SearchTerm = null
);

/// <summary>
/// Response DTO for paginated results.
/// </summary>
/// <typeparam name="T">The type of items in the page.</typeparam>
public record PagedResponse<T>(
    List<T> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages
);
```

---

## Copilot Summary

**Always generate DTOs as records with:**
- XML documentation for all public DTOs
- Descriptive parameter names
- Appropriate nullable types
- #region organization for clarity
- Proper naming conventions
- No business logic (pure data)

**Never include:**
- Mutable properties (unless specifically required)
- Business validation logic
- Database-specific attributes
- Entity Framework navigation properties
