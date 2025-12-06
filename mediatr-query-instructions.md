# MediatR Query Instructions for GitHub Copilot

## Overview
When generating MediatR queries, always follow these rules to ensure clean, maintainable CQRS query implementation.

---

## General Rules for Every Query

1. **Queries must be records** (immutable)
2. **Implement IRequest<TResponse>** from MediatR
3. **Queries represent data retrieval** (read-only operations)
4. **Include filter/pagination parameters** as needed
5. **Add XML documentation** for all public queries
6. **Name queries with "Get" or "Search"** prefixes
7. **No side effects** (queries should not modify state)

---

## Query Structure Template

```csharp
namespace ProjectName.Features.FeatureName;

/// <summary>
/// Query to retrieve [specific data].
/// </summary>
/// <param name="PropertyName">Description of the filter property.</param>
public record GetEntityQuery(
    int? Id = null,
    string? Filter = null
) : IRequest<TResponse>;
```

---

## Reference Examples

### Example 1: Get Course By ID Query

```csharp
using MediatR;

namespace IdentityAuthorization.Features.Courses;

/// <summary>
/// Query to retrieve a course by its identifier.
/// </summary>
/// <param name="Id">The course identifier.</param>
public record GetCourseByIdQuery(
    int Id
) : IRequest<CourseResponse?>;  // Nullable for not found case
```

### Example 2: Get All Courses Query (with Pagination)

```csharp
using MediatR;

namespace IdentityAuthorization.Features.Courses;

/// <summary>
/// Query to retrieve a paginated list of courses.
/// </summary>
/// <param name="Page">The page number (1-based).</param>
/// <param name="PageSize">The number of items per page.</param>
/// <param name="Category">Optional category filter.</param>
/// <param name="Level">Optional level filter.</param>
/// <param name="SearchTerm">Optional search term for title/description.</param>
public record GetCoursesQuery(
    int Page = 1,
    int PageSize = 20,
    string? Category = null,
    string? Level = null,
    string? SearchTerm = null
) : IRequest<PagedResponse<CourseListItemResponse>>;
```

### Example 3: Search Courses Query

```csharp
using MediatR;

namespace IdentityAuthorization.Features.Courses;

/// <summary>
/// Query to search courses by various criteria.
/// </summary>
/// <param name="SearchTerm">The search term to match against title, description, or instructor.</param>
/// <param name="Category">Optional category filter.</param>
/// <param name="Level">Optional level filter.</param>
/// <param name="MinPrice">Optional minimum price filter.</param>
/// <param name="MaxPrice">Optional maximum price filter.</param>
/// <param name="IncludeFree">Whether to include free courses.</param>
/// <param name="SortBy">Optional sort field (Title, Price, Duration).</param>
/// <param name="SortDescending">Whether to sort in descending order.</param>
public record SearchCoursesQuery(
    string SearchTerm,
    string? Category = null,
    string? Level = null,
    decimal? MinPrice = null,
    decimal? MaxPrice = null,
    bool IncludeFree = true,
    string SortBy = "Title",
    bool SortDescending = false
) : IRequest<List<CourseListItemResponse>>;
```

### Example 4: Get Course Details Query

```csharp
using MediatR;

namespace IdentityAuthorization.Features.Courses;

/// <summary>
/// Query to retrieve detailed course information including modules and statistics.
/// </summary>
/// <param name="Id">The course identifier.</param>
/// <param name="IncludeModules">Whether to include course modules.</param>
/// <param name="IncludeStats">Whether to include course statistics.</param>
public record GetCourseDetailsQuery(
    int Id,
    bool IncludeModules = true,
    bool IncludeStats = true
) : IRequest<CourseDetailResponse?>;
```

---

## Query Naming Conventions

### Retrieval Queries
- `Get{Entity}ByIdQuery` - Retrieve single entity by ID
- `Get{Entity}By{Property}Query` - Retrieve by specific property
- `Get{Entities}Query` - Retrieve list (with pagination)
- `GetAll{Entities}Query` - Retrieve all without pagination

### Search Queries
- `Search{Entities}Query` - Full-text search
- `Find{Entities}By{Criteria}Query` - Find by specific criteria
- `Filter{Entities}Query` - Filter with multiple criteria

### Count/Aggregate Queries
- `Count{Entities}Query` - Count entities
- `Get{Entity}StatisticsQuery` - Get aggregate statistics

---

## Response Types

### Common Response Patterns

```csharp
// Single entity (nullable for not found)
IRequest<CourseResponse?>

// List of entities
IRequest<List<CourseListItemResponse>>

// Paginated results
IRequest<PagedResponse<CourseListItemResponse>>

// Count/aggregate
IRequest<int>  // For count
IRequest<CourseStatisticsResponse>  // For statistics

// Check existence
IRequest<bool>
```

---

## Complex Query Examples

### Query with Multiple Filters

```csharp
using MediatR;

namespace IdentityAuthorization.Features.Courses;

/// <summary>
/// Query to get courses with advanced filtering options.
/// </summary>
/// <param name="Categories">List of categories to filter by.</param>
/// <param name="Levels">List of levels to filter by.</param>
/// <param name="MinDuration">Minimum duration in hours.</param>
/// <param name="MaxDuration">Maximum duration in hours.</param>
/// <param name="IsFree">Filter for free courses only.</param>
/// <param name="InstructorId">Filter by instructor identifier.</param>
/// <param name="CreatedAfter">Filter courses created after this date.</param>
/// <param name="CreatedBefore">Filter courses created before this date.</param>
/// <param name="Page">Page number.</param>
/// <param name="PageSize">Page size.</param>
public record FilterCoursesQuery(
    List<string>? Categories = null,
    List<string>? Levels = null,
    int? MinDuration = null,
    int? MaxDuration = null,
    bool? IsFree = null,
    string? InstructorId = null,
    DateTime? CreatedAfter = null,
    DateTime? CreatedBefore = null,
    int Page = 1,
    int PageSize = 20
) : IRequest<PagedResponse<CourseListItemResponse>>;
```

### Aggregation Query

```csharp
using MediatR;

namespace IdentityAuthorization.Features.Courses;

/// <summary>
/// Query to get course statistics by category.
/// </summary>
/// <param name="Category">Optional category filter (null for all categories).</param>
public record GetCourseStatisticsByCategoryQuery(
    string? Category = null
) : IRequest<List<CategoryStatisticsResponse>>;

/// <summary>
/// Response containing category-level statistics.
/// </summary>
public record CategoryStatisticsResponse(
    string Category,
    int TotalCourses,
    int FreeCourses,
    int PaidCourses,
    decimal AveragePrice,
    int TotalEnrollments
);
```

### Query with Joins

```csharp
using MediatR;

namespace IdentityAuthorization.Features.Courses;

/// <summary>
/// Query to get courses with instructor information.
/// </summary>
/// <param name="IncludeInactive">Whether to include inactive courses.</param>
/// <param name="Page">Page number.</param>
/// <param name="PageSize">Page size.</param>
public record GetCoursesWithInstructorsQuery(
    bool IncludeInactive = false,
    int Page = 1,
    int PageSize = 20
) : IRequest<PagedResponse<CourseWithInstructorResponse>>;

/// <summary>
/// Response containing course with instructor details.
/// </summary>
public record CourseWithInstructorResponse(
    int CourseId,
    string Title,
    string Category,
    InstructorSummary Instructor
);

public record InstructorSummary(
    string Id,
    string Name,
    string Email
);
```

---

## Query Optimization Patterns

### Projection Query (Select Only Needed Fields)

```csharp
using MediatR;

namespace IdentityAuthorization.Features.Courses;

/// <summary>
/// Query to get course titles for dropdown list.
/// </summary>
/// <param name="Category">Optional category filter.</param>
public record GetCourseTitlesQuery(
    string? Category = null
) : IRequest<List<CourseTitleDto>>;

/// <summary>
/// Lightweight DTO containing only ID and title.
/// </summary>
public record CourseTitleDto(
    int Id,
    string Title
);
```

### Count Query (Efficient for Large Datasets)

```csharp
using MediatR;

namespace IdentityAuthorization.Features.Courses;

/// <summary>
/// Query to count courses matching specific criteria.
/// </summary>
/// <param name="Category">Optional category filter.</param>
/// <param name="Level">Optional level filter.</param>
/// <param name="IsFree">Optional free/paid filter.</param>
public record CountCoursesQuery(
    string? Category = null,
    string? Level = null,
    bool? IsFree = null
) : IRequest<int>;
```

### Exists Query (Check for Existence)

```csharp
using MediatR;

namespace IdentityAuthorization.Features.Courses;

/// <summary>
/// Query to check if a course exists by title.
/// </summary>
/// <param name="Title">The course title to check.</param>
/// <param name="ExcludeId">Optional course ID to exclude from check (for updates).</param>
public record CourseExistsByTitleQuery(
    string Title,
    int? ExcludeId = null
) : IRequest<bool>;
```

---

## Validation Rules for Queries

### DO:
? Use records for immutability  
? Add XML documentation  
? Use nullable types for optional filters  
? Include pagination for large result sets  
? Use descriptive query names  
? Return nullable for single entity queries  

### DON'T:
? Modify state in query handlers  
? Include commands within queries  
? Return full entities when projections suffice  
? Skip pagination for potentially large datasets  
? Use generic names like "DataQuery"  

---

## Query Organization

### File Structure
```
Features/
  Courses/
    Queries/
      GetCourseByIdQuery.cs
      GetCoursesQuery.cs
      SearchCoursesQuery.cs
    Handlers/
      GetCourseByIdHandler.cs
      GetCoursesHandler.cs
      SearchCoursesHandler.cs
```

### Alternative (Single File Pattern)
```csharp
namespace IdentityAuthorization.Features.Courses;

#region Queries

public record GetCourseByIdQuery(int Id) : IRequest<CourseResponse?>;
public record GetCoursesQuery(...) : IRequest<PagedResponse<CourseListItemResponse>>;
public record SearchCoursesQuery(...) : IRequest<List<CourseListItemResponse>>;

#endregion
```

---

## Integration with API

### Mapping from Request Parameters to Query

```csharp
// In Minimal API endpoint
app.MapGet("/courses", async (
    [AsParameters] GetCoursesRequest request,
    IMediator mediator) =>
{
    var query = new GetCoursesQuery(
        Page: request.Page,
        PageSize: request.PageSize,
        Category: request.Category,
        Level: request.Level,
        SearchTerm: request.SearchTerm
    );
    
    var response = await mediator.Send(query);
    return Results.Ok(response);
});

app.MapGet("/courses/{id:int}", async (
    int id,
    IMediator mediator) =>
{
    var query = new GetCourseByIdQuery(id);
    var response = await mediator.Send(query);
    
    return response is not null
        ? Results.Ok(response)
        : Results.NotFound();
});
```

---

## Copilot Summary

**Always generate queries as records with:**
- XML documentation for all public queries
- IRequest<TResponse> implementation
- "Get" or "Search" naming prefixes
- Nullable types for optional filters
- Pagination for lists
- Nullable response types for single entities

**Never include:**
- State modifications
- Business logic beyond filtering
- Commands mixed with queries
- Non-nullable single entity responses
- Unpaginated large result sets
