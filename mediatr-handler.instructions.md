# MediatR Handler Instructions for GitHub Copilot

## Overview
When generating MediatR handlers, always follow these rules to ensure clean, maintainable command/query processing.

---

## General Rules for Every Handler

1. **Handlers must be classes** (not records)
2. **Implement IRequestHandler<TRequest, TResponse>** from MediatR
3. **Use dependency injection** for all dependencies
4. **Include comprehensive error handling**
5. **Add XML documentation** for all public handlers
6. **Use private methods** for complex operations
7. **Log all important operations** using ILogger
8. **Follow Single Responsibility Principle**

---

## Handler Structure Template

```csharp
namespace ProjectName.Features.FeatureName;

/// <summary>
/// Handles [command/query description].
/// </summary>
public class ActionEntityHandler : IRequestHandler<ActionEntityCommand, TResponse>
{
    private readonly IDependency _dependency;
    private readonly ILogger<ActionEntityHandler> _logger;
    
    public ActionEntityHandler(
        IDependency dependency,
        ILogger<ActionEntityHandler> _logger)
    {
        _dependency = dependency;
        _logger = logger;
    }
    
    public async Task<TResponse> Handle(
        ActionEntityCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling {CommandName}", nameof(ActionEntityCommand));
        
        try
        {
            // Business logic here
            
            _logger.LogInformation("Successfully handled {CommandName}", nameof(ActionEntityCommand));
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling {CommandName}", nameof(ActionEntityCommand));
            throw;
        }
    }
}
```

---

## Reference Examples

### Example 1: Create Course Command Handler

```csharp
using IdentityAuthorization.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IdentityAuthorization.Features.Courses;

/// <summary>
/// Handles the creation of a new course.
/// </summary>
public class CreateCourseHandler : IRequestHandler<CreateCourseCommand, CourseResponse>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<CreateCourseHandler> _logger;
    
    public CreateCourseHandler(
        ApplicationDbContext dbContext,
        ILogger<CreateCourseHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }
    
    public async Task<CourseResponse> Handle(
        CreateCourseCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating course with title: {Title}", request.Title);
        
        try
        {
            // Check for duplicate course title
            var exists = await CourseExistsByTitleAsync(request.Title, cancellationToken);
            if (exists)
            {
                _logger.LogWarning("Course with title {Title} already exists", request.Title);
                throw new DomainException($"Course with title '{request.Title}' already exists.");
            }
            
            // Create course entity
            var course = new Course(
                title: request.Title,
                description: request.Description,
                category: request.Category,
                duration: request.Duration,
                level: request.Level,
                createdBy: request.CreatedBy,
                instructor: request.Instructor,
                price: request.Price
            );
            
            _dbContext.Courses.Add(course);
            await _dbContext.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation("Successfully created course with ID: {CourseId}", course.Id);
            
            return MapToResponse(course);
        }
        catch (DomainException)
        {
            throw;  // Re-throw domain exceptions
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating course with title: {Title}", request.Title);
            throw new ApplicationException("An error occurred while creating the course.", ex);
        }
    }
    
    #region Private Methods
    
    /// <summary>
    /// Checks if a course with the given title already exists.
    /// </summary>
    private async Task<bool> CourseExistsByTitleAsync(
        string title,
        CancellationToken cancellationToken)
    {
        return await _dbContext.Courses
            .AnyAsync(c => c.Title == title, cancellationToken);
    }
    
    /// <summary>
    /// Maps a Course entity to CourseResponse DTO.
    /// </summary>
    private static CourseResponse MapToResponse(Course course)
    {
        return new CourseResponse(
            Id: course.Id,
            Title: course.Title,
            Description: course.Description,
            Category: course.Category,
            Duration: course.Duration,
            Level: course.Level,
            Instructor: course.Instructor,
            Price: course.Price,
            IsFree: course.IsFree(),
            CreatedBy: course.CreatedBy,
            CreatedAt: course.CreatedAt,
            UpdatedBy: course.UpdatedBy,
            UpdatedAt: course.UpdatedAt
        );
    }
    
    #endregion
}
```

### Example 2: Update Course Command Handler

```csharp
using IdentityAuthorization.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IdentityAuthorization.Features.Courses;

/// <summary>
/// Handles updating an existing course.
/// </summary>
public class UpdateCourseHandler : IRequestHandler<UpdateCourseCommand, CourseResponse>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<UpdateCourseHandler> _logger;
    
    public UpdateCourseHandler(
        ApplicationDbContext dbContext,
        ILogger<UpdateCourseHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }
    
    public async Task<CourseResponse> Handle(
        UpdateCourseCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating course with ID: {CourseId}", request.Id);
        
        try
        {
            // Find existing course
            var course = await FindCourseByIdAsync(request.Id, cancellationToken);
            if (course is null)
            {
                _logger.LogWarning("Course not found with ID: {CourseId}", request.Id);
                throw new NotFoundException($"Course with ID {request.Id} not found.");
            }
            
            // Check for duplicate title (excluding current course)
            var titleExists = await IsTitleDuplicateAsync(
                request.Title,
                request.Id,
                cancellationToken);
            
            if (titleExists)
            {
                _logger.LogWarning("Course title {Title} already exists", request.Title);
                throw new DomainException($"Course with title '{request.Title}' already exists.");
            }
            
            // Update course entity
            course.Update(
                title: request.Title,
                description: request.Description,
                category: request.Category,
                duration: request.Duration,
                level: request.Level,
                updatedBy: request.UpdatedBy,
                instructor: request.Instructor,
                price: request.Price
            );
            
            await _dbContext.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation("Successfully updated course with ID: {CourseId}", course.Id);
            
            return MapToResponse(course);
        }
        catch (NotFoundException)
        {
            throw;  // Re-throw not found exceptions
        }
        catch (DomainException)
        {
            throw;  // Re-throw domain exceptions
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating course with ID: {CourseId}", request.Id);
            throw new ApplicationException("An error occurred while updating the course.", ex);
        }
    }
    
    #region Private Methods
    
    /// <summary>
    /// Finds a course by its identifier.
    /// </summary>
    private async Task<Course?> FindCourseByIdAsync(
        int id,
        CancellationToken cancellationToken)
    {
        return await _dbContext.Courses
            .FindAsync(new object[] { id }, cancellationToken);
    }
    
    /// <summary>
    /// Checks if a course title is duplicate (excluding the specified course ID).
    /// </summary>
    private async Task<bool> IsTitleDuplicateAsync(
        string title,
        int excludeId,
        CancellationToken cancellationToken)
    {
        return await _dbContext.Courses
            .AnyAsync(c => c.Title == title && c.Id != excludeId, cancellationToken);
    }
    
    /// <summary>
    /// Maps a Course entity to CourseResponse DTO.
    /// </summary>
    private static CourseResponse MapToResponse(Course course)
    {
        return new CourseResponse(
            Id: course.Id,
            Title: course.Title,
            Description: course.Description,
            Category: course.Category,
            Duration: course.Duration,
            Level: course.Level,
            Instructor: course.Instructor,
            Price: course.Price,
            IsFree: course.IsFree(),
            CreatedBy: course.CreatedBy,
            CreatedAt: course.CreatedAt,
            UpdatedBy: course.UpdatedBy,
            UpdatedAt: course.UpdatedAt
        );
    }
    
    #endregion
}
```

### Example 3: Get Course By ID Query Handler

```csharp
using IdentityAuthorization.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IdentityAuthorization.Features.Courses;

/// <summary>
/// Handles retrieving a course by its identifier.
/// </summary>
public class GetCourseByIdHandler : IRequestHandler<GetCourseByIdQuery, CourseResponse?>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<GetCourseByIdHandler> _logger;
    
    public GetCourseByIdHandler(
        ApplicationDbContext dbContext,
        ILogger<GetCourseByIdHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }
    
    public async Task<CourseResponse?> Handle(
        GetCourseByIdQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving course with ID: {CourseId}", request.Id);
        
        try
        {
            var course = await _dbContext.Courses
                .AsNoTracking()  // No tracking for read-only operations
                .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);
            
            if (course is null)
            {
                _logger.LogWarning("Course not found with ID: {CourseId}", request.Id);
                return null;
            }
            
            _logger.LogInformation("Successfully retrieved course with ID: {CourseId}", request.Id);
            
            return MapToResponse(course);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving course with ID: {CourseId}", request.Id);
            throw new ApplicationException("An error occurred while retrieving the course.", ex);
        }
    }
    
    #region Private Methods
    
    /// <summary>
    /// Maps a Course entity to CourseResponse DTO.
    /// </summary>
    private static CourseResponse MapToResponse(Course course)
    {
        return new CourseResponse(
            Id: course.Id,
            Title: course.Title,
            Description: course.Description,
            Category: course.Category,
            Duration: course.Duration,
            Level: course.Level,
            Instructor: course.Instructor,
            Price: course.Price,
            IsFree: course.IsFree(),
            CreatedBy: course.CreatedBy,
            CreatedAt: course.CreatedAt,
            UpdatedBy: course.UpdatedBy,
            UpdatedAt: course.UpdatedAt
        );
    }
    
    #endregion
}
```

### Example 4: Get Courses Query Handler (with Pagination)

```csharp
using IdentityAuthorization.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IdentityAuthorization.Features.Courses;

/// <summary>
/// Handles retrieving a paginated list of courses.
/// </summary>
public class GetCoursesHandler : IRequestHandler<GetCoursesQuery, PagedResponse<CourseListItemResponse>>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<GetCoursesHandler> _logger;
    
    public GetCoursesHandler(
        ApplicationDbContext dbContext,
        ILogger<GetCoursesHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }
    
    public async Task<PagedResponse<CourseListItemResponse>> Handle(
        GetCoursesQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving courses - Page: {Page}, PageSize: {PageSize}",
            request.Page, request.PageSize);
        
        try
        {
            // Build query
            var query = BuildQuery(request);
            
            // Get total count
            var totalCount = await query.CountAsync(cancellationToken);
            
            // Get paged results
            var courses = await query
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);
            
            var items = courses.Select(MapToListItemResponse).ToList();
            
            var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);
            
            _logger.LogInformation("Retrieved {Count} courses out of {TotalCount}",
                courses.Count, totalCount);
            
            return new PagedResponse<CourseListItemResponse>(
                Items: items,
                TotalCount: totalCount,
                Page: request.Page,
                PageSize: request.PageSize,
                TotalPages: totalPages
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving courses");
            throw new ApplicationException("An error occurred while retrieving courses.", ex);
        }
    }
    
    #region Private Methods
    
    /// <summary>
    /// Builds the query with applied filters.
    /// </summary>
    private IQueryable<Course> BuildQuery(GetCoursesQuery request)
    {
        var query = _dbContext.Courses.AsNoTracking();
        
        // Apply category filter
        if (!string.IsNullOrWhiteSpace(request.Category))
        {
            query = query.Where(c => c.Category == request.Category);
        }
        
        // Apply level filter
        if (!string.IsNullOrWhiteSpace(request.Level))
        {
            query = query.Where(c => c.Level == request.Level);
        }
        
        // Apply search term filter
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            query = query.Where(c =>
                c.Title.Contains(request.SearchTerm) ||
                c.Description.Contains(request.SearchTerm));
        }
        
        // Order by title
        query = query.OrderBy(c => c.Title);
        
        return query;
    }
    
    /// <summary>
    /// Maps a Course entity to CourseListItemResponse DTO.
    /// </summary>
    private static CourseListItemResponse MapToListItemResponse(Course course)
    {
        return new CourseListItemResponse(
            Id: course.Id,
            Title: course.Title,
            Category: course.Category,
            Duration: course.Duration,
            Level: course.Level,
            Price: course.Price,
            IsFree: course.IsFree()
        );
    }
    
    #endregion
}
```

---

## Handler Best Practices

### DO:
? Use dependency injection for all dependencies  
? Log all important operations  
? Use private methods for complex logic  
? Handle all exceptions appropriately  
? Use AsNoTracking() for read-only queries  
? Include cancellation token support  
? Add XML documentation  
? Use descriptive method names  

### DON'T:
? Include business logic in handlers (put it in entities)  
? Catch and swallow exceptions  
? Use magic strings or numbers  
? Perform synchronous I/O operations  
? Return tracked entities from queries  
? Mix concerns (keep handlers focused)  

---

## Private Methods Guidelines

### Common Private Method Patterns

```csharp
#region Private Methods

// Entity retrieval
private async Task<Entity?> FindEntityByIdAsync(int id, CancellationToken cancellationToken)

// Existence checks
private async Task<bool> EntityExistsAsync(string criteria, CancellationToken cancellationToken)

// Duplicate checks
private async Task<bool> IsDuplicateAsync(string value, int? excludeId, CancellationToken cancellationToken)

// Mapping
private static ResponseDto MapToResponse(Entity entity)

// Query building
private IQueryable<Entity> BuildQuery(QueryRequest request)

// Validation
private void ValidateRequest(Request request)

// Business rules
private bool CheckBusinessRule(Entity entity)

#endregion
```

---

## Error Handling Patterns

```csharp
public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken)
{
    _logger.LogInformation("Handling {RequestName}", typeof(TRequest).Name);
    
    try
    {
        // Business logic
        
        _logger.LogInformation("Successfully handled {RequestName}", typeof(TRequest).Name);
        return response;
    }
    catch (NotFoundException ex)
    {
        _logger.LogWarning(ex, "Entity not found");
        throw;  // Let global exception handler deal with it
    }
    catch (DomainException ex)
    {
        _logger.LogWarning(ex, "Domain rule violation");
        throw;
    }
    catch (DbUpdateException ex)
    {
        _logger.LogError(ex, "Database update error");
        throw new ApplicationException("An error occurred while saving changes.", ex);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Unexpected error handling {RequestName}", typeof(TRequest).Name);
        throw new ApplicationException($"An error occurred while processing {typeof(TRequest).Name}.", ex);
    }
}
```

---

## Copilot Summary

**Always generate handlers with:**
- XML documentation
- Dependency injection
- Comprehensive logging
- Proper error handling
- Private methods for complex logic
- CancellationToken support
- AsNoTracking() for queries

**Never include:**
- Business logic (belongs in entities)
- Swallowed exceptions
- Synchronous I/O operations
- Mixed concerns
- Magic values
