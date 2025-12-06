# MediatR Command Instructions for GitHub Copilot

## Overview
When generating MediatR commands, always follow these rules to ensure clean, maintainable CQRS implementation.

---

## General Rules for Every Command

1. **Commands must be records** (immutable)
2. **Implement IRequest<TResponse>** from MediatR
3. **Commands represent intent** (create, update, delete operations)
4. **Include all required data** as constructor parameters
5. **Add XML documentation** for all public commands
6. **Name commands with action verbs** (Create, Update, Delete, etc.)
7. **No business logic** in commands (pure data structures)

---

## Command Structure Template

```csharp
namespace ProjectName.Features.FeatureName;

/// <summary>
/// Command to [perform specific action].
/// </summary>
/// <param name="PropertyName">Description of the property.</param>
public record ActionEntityCommand(
    string PropertyName,
    int AnotherProperty,
    decimal? OptionalProperty = null
) : IRequest<TResponse>;
```

---

## Reference Examples

### Example 1: Create Course Command

```csharp
using MediatR;

namespace IdentityAuthorization.Features.Courses;

/// <summary>
/// Command to create a new course.
/// </summary>
/// <param name="Title">The course title.</param>
/// <param name="Description">The course description.</param>
/// <param name="Category">The course category.</param>
/// <param name="Duration">The course duration in hours.</param>
/// <param name="Level">The course level (Beginner/Intermediate/Advanced).</param>
/// <param name="CreatedBy">The user creating the course.</param>
/// <param name="Instructor">Optional instructor information.</param>
/// <param name="Price">Optional course price (null for free courses).</param>
public record CreateCourseCommand(
    string Title,
    string Description,
    string Category,
    int Duration,
    string Level,
    string CreatedBy,
    string? Instructor = null,
    decimal? Price = null
) : IRequest<CourseResponse>;
```

### Example 2: Update Course Command

```csharp
using MediatR;

namespace IdentityAuthorization.Features.Courses;

/// <summary>
/// Command to update an existing course.
/// </summary>
/// <param name="Id">The course identifier.</param>
/// <param name="Title">The course title.</param>
/// <param name="Description">The course description.</param>
/// <param name="Category">The course category.</param>
/// <param name="Duration">The course duration in hours.</param>
/// <param name="Level">The course level.</param>
/// <param name="UpdatedBy">The user updating the course.</param>
/// <param name="Instructor">Optional instructor information.</param>
/// <param name="Price">Optional course price.</param>
public record UpdateCourseCommand(
    int Id,
    string Title,
    string Description,
    string Category,
    int Duration,
    string Level,
    string UpdatedBy,
    string? Instructor = null,
    decimal? Price = null
) : IRequest<CourseResponse>;
```

### Example 3: Delete Course Command

```csharp
using MediatR;

namespace IdentityAuthorization.Features.Courses;

/// <summary>
/// Command to delete a course.
/// </summary>
/// <param name="Id">The course identifier.</param>
/// <param name="DeletedBy">The user deleting the course.</param>
public record DeleteCourseCommand(
    int Id,
    string DeletedBy
) : IRequest<Unit>;  // Unit for void/no return value
```

### Example 4: Update Course Price Command

```csharp
using MediatR;

namespace IdentityAuthorization.Features.Courses;

/// <summary>
/// Command to update only the course price.
/// </summary>
/// <param name="Id">The course identifier.</param>
/// <param name="Price">The new price (null for free courses).</param>
/// <param name="UpdatedBy">The user updating the price.</param>
public record UpdateCoursePriceCommand(
    int Id,
    decimal? Price,
    string UpdatedBy
) : IRequest<CourseResponse>;
```

---

## Command Naming Conventions

### Standard Actions
- `Create{Entity}Command` - Creates a new entity
- `Update{Entity}Command` - Updates an entire entity
- `Delete{Entity}Command` - Deletes an entity
- `Update{Entity}{Property}Command` - Updates specific property

### Specific Actions
- `Publish{Entity}Command` - Changes state to published
- `Activate{Entity}Command` - Activates an entity
- `Deactivate{Entity}Command` - Deactivates an entity
- `Enroll{Entity}Command` - Enrollment action
- `Cancel{Entity}Command` - Cancellation action

---

## Response Types

### Common Response Patterns

```csharp
// Returns entity details
IRequest<CourseResponse>

// Returns void/no content (for delete operations)
IRequest<Unit>

// Returns success/failure indicator
IRequest<Result>  // Custom Result type

// Returns created entity ID
IRequest<int>

// Returns boolean success indicator
IRequest<bool>
```

---

## Complex Command Examples

### Bulk Operation Command

```csharp
using MediatR;

namespace IdentityAuthorization.Features.Courses;

/// <summary>
/// Command to enroll multiple students in a course.
/// </summary>
/// <param name="CourseId">The course identifier.</param>
/// <param name="StudentIds">List of student identifiers.</param>
/// <param name="EnrolledBy">The user performing the enrollment.</param>
public record BulkEnrollStudentsCommand(
    int CourseId,
    List<string> StudentIds,
    string EnrolledBy
) : IRequest<BulkEnrollmentResponse>;

/// <summary>
/// Response for bulk enrollment operation.
/// </summary>
public record BulkEnrollmentResponse(
    int SuccessCount,
    int FailureCount,
    List<string> FailedStudentIds
);
```

### Command with File Upload

```csharp
using MediatR;

namespace IdentityAuthorization.Features.Courses;

/// <summary>
/// Command to upload course materials.
/// </summary>
/// <param name="CourseId">The course identifier.</param>
/// <param name="FileName">The name of the file.</param>
/// <param name="FileContent">The file content as byte array.</param>
/// <param name="ContentType">The MIME type of the file.</param>
/// <param name="UploadedBy">The user uploading the file.</param>
public record UploadCourseMaterialCommand(
    int CourseId,
    string FileName,
    byte[] FileContent,
    string ContentType,
    string UploadedBy
) : IRequest<MaterialResponse>;
```

---

## Command with Nested Data

```csharp
using MediatR;

namespace IdentityAuthorization.Features.Courses;

/// <summary>
/// Command to create a course with modules.
/// </summary>
/// <param name="Title">The course title.</param>
/// <param name="Description">The course description.</param>
/// <param name="Modules">List of course modules.</param>
/// <param name="CreatedBy">The user creating the course.</param>
public record CreateCourseWithModulesCommand(
    string Title,
    string Description,
    string Category,
    int Duration,
    string Level,
    List<ModuleData> Modules,
    string CreatedBy
) : IRequest<CourseDetailResponse>;

/// <summary>
/// Module data for course creation.
/// </summary>
public record ModuleData(
    string Title,
    string Description,
    int Order
);
```

---

## Validation Rules for Commands

### DO:
? Use records for immutability  
? Add XML documentation  
? Include user context (CreatedBy, UpdatedBy, etc.)  
? Use descriptive action verbs in naming  
? Specify appropriate response types  
? Keep commands focused on single action  

### DON'T:
? Add business logic to commands  
? Mix multiple actions in one command  
? Include computed properties  
? Add validation logic (use FluentValidation instead)  
? Use generic names like "DataCommand"  

---

## Command Organization

### File Structure
```
Features/
  Courses/
    Commands/
      CreateCourseCommand.cs
      UpdateCourseCommand.cs
      DeleteCourseCommand.cs
    Handlers/
      CreateCourseHandler.cs
      UpdateCourseHandler.cs
      DeleteCourseHandler.cs
    Queries/
      GetCourseByIdQuery.cs
      GetCoursesQuery.cs
```

### Alternative (Single File Pattern)
```csharp
namespace IdentityAuthorization.Features.Courses;

#region Commands

public record CreateCourseCommand(...) : IRequest<CourseResponse>;
public record UpdateCourseCommand(...) : IRequest<CourseResponse>;
public record DeleteCourseCommand(...) : IRequest<Unit>;

#endregion
```

---

## Integration with API

### Mapping from Request DTO to Command

```csharp
// In Minimal API endpoint
app.MapPost("/courses", async (
    CreateCourseRequest request,
    ClaimsPrincipal user,
    IMediator mediator) =>
{
    var userId = user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value!;
    
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
    
    var response = await mediator.Send(command);
    return Results.Created($"/courses/{response.Id}", response);
});
```

---

## Copilot Summary

**Always generate commands as records with:**
- XML documentation for all public commands
- IRequest<TResponse> implementation
- Action verb naming (Create, Update, Delete, etc.)
- User context parameters (CreatedBy, UpdatedBy)
- Appropriate response types
- No business logic (pure data)

**Never include:**
- Mutable properties
- Business validation logic
- Computed properties
- Multiple responsibilities
- Generic or unclear names
