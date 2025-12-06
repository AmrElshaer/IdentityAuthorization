# Minimal API Endpoint Instructions for GitHub Copilot

## Overview
When generating Minimal API endpoints, always follow these rules to ensure clean, maintainable RESTful APIs.

---

## General Rules for Every Endpoint

1. **Use MapGroup** for related endpoints
2. **Follow RESTful conventions** for HTTP methods and URLs
3. **Include OpenAPI/Swagger metadata** with `.WithName()` and `.WithOpenApi()`
4. **Add proper status code responses** with `.Produces()` and `.ProducesValidationProblem()`
5. **Use parameter binding** attributes appropriately (`[FromBody]`, `[FromQuery]`, `[FromRoute]`)
6. **Include authorization** with `.RequireAuthorization()`
7. **Extract user context** from ClaimsPrincipal
8. **Use MediatR** for all business logic
9. **Return appropriate IResult** types
10. **Add XML documentation** for endpoint purpose

---

## Endpoint Structure Template

```csharp
namespace ProjectName.Features.FeatureName;

public static class EntityEndpoints
{
    public static void MapEndpoints(WebApplication app)
    {
        var group = app.MapGroup("/api/entity")
            .WithTags("Entity")
            .RequireAuthorization();
        
        group.MapPost("/", CreateEntity)
            .WithName("CreateEntity")
            .WithOpenApi()
            .Produces<Response>(StatusCodes.Status201Created)
            .ProducesValidationProblem();
        
        group.MapGet("/{id:int}", GetEntity)
            .WithName("GetEntity")
            .WithOpenApi()
            .Produces<Response>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
    }
    
    private static async Task<IResult> CreateEntity(
        Request request,
        ClaimsPrincipal user,
        IMediator mediator)
    {
        // Implementation
    }
}
```

---

## Reference Examples

### Example 1: Complete Course Endpoints

```csharp
using System.Security.Claims;
using IdentityAuthorization.Features.Courses;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;

namespace IdentityAuthorization.Features.Courses;

/// <summary>
/// Minimal API endpoints for course management.
/// </summary>
public static class CourseEndpoints
{
    /// <summary>
    /// Maps all course-related endpoints to the application.
    /// </summary>
    public static void MapEndpoints(WebApplication app)
    {
        var coursesGroup = app.MapGroup("/api/courses")
            .WithTags("Courses")
            .RequireAuthorization();
        
        // CREATE - Create new course
        coursesGroup.MapPost("/", CreateCourse)
            .WithName("CreateCourse")
            .WithOpenApi()
            .WithDescription("Creates a new course")
            .Produces<CourseResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);
        
        // READ - Get all courses (paginated)
        coursesGroup.MapGet("/", GetCourses)
            .WithName("GetCourses")
            .WithOpenApi()
            .WithDescription("Gets a paginated list of courses")
            .Produces<PagedResponse<CourseListItemResponse>>(StatusCodes.Status200OK);
        
        // READ - Get course by ID
        coursesGroup.MapGet("/{id:int}", GetCourseById)
            .WithName("GetCourseById")
            .WithOpenApi()
            .WithDescription("Gets a course by its identifier")
            .Produces<CourseResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
        
        // UPDATE - Update course
        coursesGroup.MapPut("/{id:int}", UpdateCourse)
            .WithName("UpdateCourse")
            .WithOpenApi()
            .WithDescription("Updates an existing course")
            .Produces<CourseResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest);
        
        // DELETE - Delete course
        coursesGroup.MapDelete("/{id:int}", DeleteCourse)
            .WithName("DeleteCourse")
            .WithOpenApi()
            .WithDescription("Deletes a course")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);
        
        // SEARCH - Search courses
        coursesGroup.MapGet("/search", SearchCourses)
            .WithName("SearchCourses")
            .WithOpenApi()
            .WithDescription("Searches for courses by various criteria")
            .Produces<List<CourseListItemResponse>>(StatusCodes.Status200OK);
    }
    
    /// <summary>
    /// Creates a new course.
    /// </summary>
    private static async Task<IResult> CreateCourse(
        [FromBody] CreateCourseRequest request,
        ClaimsPrincipal user,
        IMediator mediator,
        ILogger<CreateCourseRequest> logger)
    {
        try
        {
            // Extract user ID from JWT claims
            var userId = user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            if (string.IsNullOrWhiteSpace(userId))
            {
                logger.LogWarning("Create course attempt without valid user claim");
                return Results.Unauthorized();
            }
            
            // Create command
            var command = new CreateCourseCommand(
                Title: request.Title,
                Description: request.Description,
                Category: request.Category,
                Duration: request.Duration,
                Level: request.Level,
                CreatedBy: userId,
                Instructor: request.Instructor,
                Price: request.Price
            );
            
            // Send command through MediatR
            var response = await mediator.Send(command);
            
            logger.LogInformation("Course created with ID: {CourseId} by user: {UserId}",
                response.Id, userId);
            
            return Results.Created($"/api/courses/{response.Id}", response);
        }
        catch (DomainException ex)
        {
            logger.LogWarning(ex, "Domain error creating course: {Message}", ex.Message);
            return Results.BadRequest(new ProblemDetails
            {
                Title = "Domain Rule Violation",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating course");
            return Results.Problem(
                title: "An error occurred",
                detail: "An unexpected error occurred while creating the course.",
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }
    
    /// <summary>
    /// Gets a paginated list of courses.
    /// </summary>
    private static async Task<IResult> GetCourses(
        [AsParameters] GetCoursesRequest request,
        IMediator mediator,
        ILogger<GetCoursesRequest> logger)
    {
        try
        {
            var query = new GetCoursesQuery(
                Page: request.Page,
                PageSize: request.PageSize,
                Category: request.Category,
                Level: request.Level,
                SearchTerm: request.SearchTerm
            );
            
            var response = await mediator.Send(query);
            
            logger.LogInformation("Retrieved {Count} courses (page {Page}, size {PageSize})",
                response.Items.Count, request.Page, request.PageSize);
            
            return Results.Ok(response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving courses");
            return Results.Problem(
                title: "An error occurred",
                detail: "An unexpected error occurred while retrieving courses.",
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }
    
    /// <summary>
    /// Gets a course by its identifier.
    /// </summary>
    private static async Task<IResult> GetCourseById(
        int id,
        IMediator mediator,
        ILogger<GetCourseByIdQuery> logger)
    {
        try
        {
            var query = new GetCourseByIdQuery(id);
            var response = await mediator.Send(query);
            
            if (response is null)
            {
                logger.LogWarning("Course not found: {CourseId}", id);
                return Results.NotFound(new ProblemDetails
                {
                    Title = "Course Not Found",
                    Detail = $"Course with ID {id} was not found.",
                    Status = StatusCodes.Status404NotFound
                });
            }
            
            return Results.Ok(response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving course: {CourseId}", id);
            return Results.Problem(
                title: "An error occurred",
                detail: "An unexpected error occurred while retrieving the course.",
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }
    
    /// <summary>
    /// Updates an existing course.
    /// </summary>
    private static async Task<IResult> UpdateCourse(
        int id,
        [FromBody] UpdateCourseRequest request,
        ClaimsPrincipal user,
        IMediator mediator,
        ILogger<UpdateCourseRequest> logger)
    {
        try
        {
            // Extract user ID from JWT claims
            var userId = user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            if (string.IsNullOrWhiteSpace(userId))
            {
                logger.LogWarning("Update course attempt without valid user claim");
                return Results.Unauthorized();
            }
            
            // Create command
            var command = new UpdateCourseCommand(
                Id: id,
                Title: request.Title,
                Description: request.Description,
                Category: request.Category,
                Duration: request.Duration,
                Level: request.Level,
                UpdatedBy: userId,
                Instructor: request.Instructor,
                Price: request.Price
            );
            
            // Send command through MediatR
            var response = await mediator.Send(command);
            
            logger.LogInformation("Course updated: {CourseId} by user: {UserId}",
                id, userId);
            
            return Results.Ok(response);
        }
        catch (NotFoundException ex)
        {
            logger.LogWarning(ex, "Course not found: {CourseId}", id);
            return Results.NotFound(new ProblemDetails
            {
                Title = "Course Not Found",
                Detail = ex.Message,
                Status = StatusCodes.Status404NotFound
            });
        }
        catch (DomainException ex)
        {
            logger.LogWarning(ex, "Domain error updating course: {Message}", ex.Message);
            return Results.BadRequest(new ProblemDetails
            {
                Title = "Domain Rule Violation",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating course: {CourseId}", id);
            return Results.Problem(
                title: "An error occurred",
                detail: "An unexpected error occurred while updating the course.",
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }
    
    /// <summary>
    /// Deletes a course.
    /// </summary>
    private static async Task<IResult> DeleteCourse(
        int id,
        ClaimsPrincipal user,
        IMediator mediator,
        ILogger<DeleteCourseCommand> logger)
    {
        try
        {
            // Extract user ID from JWT claims
            var userId = user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            if (string.IsNullOrWhiteSpace(userId))
            {
                logger.LogWarning("Delete course attempt without valid user claim");
                return Results.Unauthorized();
            }
            
            var command = new DeleteCourseCommand(id, userId);
            await mediator.Send(command);
            
            logger.LogInformation("Course deleted: {CourseId} by user: {UserId}",
                id, userId);
            
            return Results.NoContent();
        }
        catch (NotFoundException ex)
        {
            logger.LogWarning(ex, "Course not found: {CourseId}", id);
            return Results.NotFound(new ProblemDetails
            {
                Title = "Course Not Found",
                Detail = ex.Message,
                Status = StatusCodes.Status404NotFound
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting course: {CourseId}", id);
            return Results.Problem(
                title: "An error occurred",
                detail: "An unexpected error occurred while deleting the course.",
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }
    
    /// <summary>
    /// Searches for courses by various criteria.
    /// </summary>
    private static async Task<IResult> SearchCourses(
        [AsParameters] SearchCoursesRequest request,
        IMediator mediator,
        ILogger<SearchCoursesRequest> logger)
    {
        try
        {
            var query = new SearchCoursesQuery(
                SearchTerm: request.SearchTerm,
                Category: request.Category,
                Level: request.Level,
                MinPrice: request.MinPrice,
                MaxPrice: request.MaxPrice,
                IncludeFree: request.IncludeFree,
                SortBy: request.SortBy,
                SortDescending: request.SortDescending
            );
            
            var response = await mediator.Send(query);
            
            logger.LogInformation("Search returned {Count} courses for term: {SearchTerm}",
                response.Count, request.SearchTerm);
            
            return Results.Ok(response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error searching courses");
            return Results.Problem(
                title: "An error occurred",
                detail: "An unexpected error occurred while searching courses.",
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }
}
```

---

## HTTP Method Conventions

### RESTful Mapping
```csharp
// CREATE
MapPost("/", CreateEntity)           // POST /api/entities

// READ
MapGet("/", GetAll)                  // GET /api/entities
MapGet("/{id:int}", GetById)         // GET /api/entities/5
MapGet("/search", Search)            // GET /api/entities/search?q=term

// UPDATE
MapPut("/{id:int}", Update)          // PUT /api/entities/5
MapPatch("/{id:int}", PartialUpdate) // PATCH /api/entities/5

// DELETE
MapDelete("/{id:int}", Delete)       // DELETE /api/entities/5
```

---

## Parameter Binding

### From Body
```csharp
private static async Task<IResult> Create(
    [FromBody] CreateRequest request,
    IMediator mediator)
```

### From Route
```csharp
private static async Task<IResult> GetById(
    int id,  // Automatically bound from route
    IMediator mediator)
```

### From Query
```csharp
private static async Task<IResult> Search(
    [FromQuery] string? searchTerm,
    [FromQuery] int page = 1,
    IMediator mediator)
```

### As Parameters (Complex Query Object)
```csharp
// Request DTO
public record GetCoursesRequest(
    int Page = 1,
    int PageSize = 20,
    string? Category = null,
    string? Level = null
);

// Endpoint
private static async Task<IResult> GetCourses(
    [AsParameters] GetCoursesRequest request,
    IMediator mediator)
```

---

## Response Types

### Success Responses
```csharp
// 200 OK
return Results.Ok(response);

// 201 Created
return Results.Created($"/api/entities/{id}", response);

// 204 No Content
return Results.NoContent();

// 202 Accepted
return Results.Accepted($"/api/entities/{id}/status", response);
```

### Error Responses
```csharp
// 400 Bad Request
return Results.BadRequest(new ProblemDetails
{
    Title = "Validation Error",
    Detail = "Invalid request data.",
    Status = StatusCodes.Status400BadRequest
});

// 401 Unauthorized
return Results.Unauthorized();

// 403 Forbidden
return Results.Forbid();

// 404 Not Found
return Results.NotFound(new ProblemDetails
{
    Title = "Entity Not Found",
    Detail = $"Entity with ID {id} was not found.",
    Status = StatusCodes.Status404NotFound
});

// 409 Conflict
return Results.Conflict(new ProblemDetails
{
    Title = "Conflict",
    Detail = "Entity already exists.",
    Status = StatusCodes.Status409Conflict
});

// 500 Internal Server Error
return Results.Problem(
    title: "An error occurred",
    detail: "An unexpected error occurred.",
    statusCode: StatusCodes.Status500InternalServerError
);
```

---

## Authorization Patterns

### Require Authorization (All Users)
```csharp
var group = app.MapGroup("/api/courses")
    .WithTags("Courses")
    .RequireAuthorization();
```

### Require Specific Role
```csharp
group.MapPost("/", CreateCourse)
    .RequireAuthorization(policy => policy.RequireRole("Admin"));
```

### Require Specific Policy
```csharp
group.MapPost("/", CreateCourse)
    .RequireAuthorization("CourseCreatePolicy");
```

### Allow Anonymous
```csharp
group.MapGet("/public", GetPublicCourses)
    .AllowAnonymous();
```

---

## Error Handling Pattern

```csharp
private static async Task<IResult> HandleRequest(
    Request request,
    IMediator mediator,
    ILogger logger)
{
    try
    {
        // Business logic
        var response = await mediator.Send(command);
        return Results.Ok(response);
    }
    catch (NotFoundException ex)
    {
        logger.LogWarning(ex, "Entity not found");
        return Results.NotFound(new ProblemDetails
        {
            Title = "Not Found",
            Detail = ex.Message,
            Status = StatusCodes.Status404NotFound
        });
    }
    catch (DomainException ex)
    {
        logger.LogWarning(ex, "Domain rule violation");
        return Results.BadRequest(new ProblemDetails
        {
            Title = "Domain Rule Violation",
            Detail = ex.Message,
            Status = StatusCodes.Status400BadRequest
        });
    }
    catch (ValidationException ex)
    {
        logger.LogWarning(ex, "Validation error");
        return Results.ValidationProblem(ex.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.ErrorMessage).ToArray()));
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Unexpected error");
        return Results.Problem(
            title: "An error occurred",
            detail: "An unexpected error occurred.",
            statusCode: StatusCodes.Status500InternalServerError
        );
    }
}
```

---

## Registration in Program.cs

```csharp
// Program.cs
using IdentityAuthorization.Features.Courses;

// ... other configuration

// Register endpoints
CourseEndpoints.MapEndpoints(app);
```

---

## Best Practices

### DO:
? Use MapGroup for related endpoints  
? Include OpenAPI metadata  
? Add proper status code documentation  
? Extract user context from claims  
? Use MediatR for business logic  
? Log all operations  
? Handle exceptions appropriately  
? Return ProblemDetails for errors  

### DON'T:
? Include business logic in endpoints  
? Swallow exceptions  
? Return raw exception messages  
? Skip authorization checks  
? Use synchronous operations  
? Hardcode user IDs  

---

## Copilot Summary

**Always generate endpoints with:**
- XML documentation
- MapGroup organization
- OpenAPI metadata (.WithName, .WithOpenApi, .WithDescription)
- Proper status code responses
- Authorization requirements
- Comprehensive error handling
- Logging for all operations
- MediatR for business logic

**Never include:**
- Business logic in endpoints
- Swallowed exceptions
- Missing authorization
- Undocumented endpoints
