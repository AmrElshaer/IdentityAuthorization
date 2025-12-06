# FluentValidation Validator Instructions for GitHub Copilot

## Overview
When generating FluentValidation validators, always follow these rules to ensure consistent, comprehensive validation.

---

## General Rules for Every Validator

1. **Validators must be classes** inheriting from `AbstractValidator<T>`
2. **Define all rules in the constructor**
3. **Use fluent API** for rule configuration
4. **Include custom error messages** for all rules
5. **Add XML documentation** for validator purpose
6. **Group related rules** with comments
7. **Use async validation** when needed
8. **Follow DRY principle** with custom validators

---

## Validator Structure Template

```csharp
using FluentValidation;

namespace ProjectName.Features.FeatureName;

/// <summary>
/// Validator for [Request/Command/Query name].
/// </summary>
public class RequestValidator : AbstractValidator<Request>
{
    public RequestValidator()
    {
        // Property validation rules
        RuleFor(x => x.Property)
            .NotEmpty().WithMessage("Property is required.")
            .MaximumLength(100).WithMessage("Property cannot exceed 100 characters.");
    }
}
```

---

## Reference Examples

### Example 1: Create Course Command Validator

```csharp
using FluentValidation;

namespace IdentityAuthorization.Features.Courses;

/// <summary>
/// Validator for CreateCourseCommand.
/// </summary>
public class CreateCourseCommandValidator : AbstractValidator<CreateCourseCommand>
{
    public CreateCourseCommandValidator()
    {
        // Title validation
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Course title is required.")
            .MinimumLength(3).WithMessage("Course title must be at least 3 characters.")
            .MaximumLength(200).WithMessage("Course title cannot exceed 200 characters.");
        
        // Description validation
        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Course description is required.")
            .MinimumLength(10).WithMessage("Course description must be at least 10 characters.")
            .MaximumLength(2000).WithMessage("Course description cannot exceed 2000 characters.");
        
        // Category validation
        RuleFor(x => x.Category)
            .NotEmpty().WithMessage("Course category is required.")
            .MinimumLength(2).WithMessage("Course category must be at least 2 characters.")
            .MaximumLength(50).WithMessage("Course category cannot exceed 50 characters.");
        
        // Duration validation
        RuleFor(x => x.Duration)
            .GreaterThan(0).WithMessage("Course duration must be greater than zero.")
            .LessThanOrEqualTo(1000).WithMessage("Course duration cannot exceed 1000 hours.");
        
        // Level validation
        RuleFor(x => x.Level)
            .NotEmpty().WithMessage("Course level is required.")
            .Must(BeValidLevel).WithMessage("Course level must be Beginner, Intermediate, or Advanced.");
        
        // CreatedBy validation
        RuleFor(x => x.CreatedBy)
            .NotEmpty().WithMessage("Created by user is required.")
            .MaximumLength(450).WithMessage("User ID cannot exceed 450 characters.");
        
        // Instructor validation (optional field)
        When(x => !string.IsNullOrWhiteSpace(x.Instructor), () =>
        {
            RuleFor(x => x.Instructor)
                .MaximumLength(200).WithMessage("Instructor information cannot exceed 200 characters.");
        });
        
        // Price validation (optional field)
        When(x => x.Price.HasValue, () =>
        {
            RuleFor(x => x.Price)
                .GreaterThanOrEqualTo(0).WithMessage("Course price cannot be negative.")
                .LessThanOrEqualTo(10000).WithMessage("Course price cannot exceed 10,000.");
        });
    }
    
    /// <summary>
    /// Validates if the level is one of the allowed values.
    /// </summary>
    private static bool BeValidLevel(string level)
    {
        var validLevels = new[] { "Beginner", "Intermediate", "Advanced" };
        return validLevels.Contains(level, StringComparer.OrdinalIgnoreCase);
    }
}
```

### Example 2: Update Course Command Validator

```csharp
using FluentValidation;

namespace IdentityAuthorization.Features.Courses;

/// <summary>
/// Validator for UpdateCourseCommand.
/// </summary>
public class UpdateCourseCommandValidator : AbstractValidator<UpdateCourseCommand>
{
    public UpdateCourseCommandValidator()
    {
        // ID validation
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Course ID must be greater than zero.");
        
        // Title validation
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Course title is required.")
            .MinimumLength(3).WithMessage("Course title must be at least 3 characters.")
            .MaximumLength(200).WithMessage("Course title cannot exceed 200 characters.");
        
        // Description validation
        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Course description is required.")
            .MinimumLength(10).WithMessage("Course description must be at least 10 characters.")
            .MaximumLength(2000).WithMessage("Course description cannot exceed 2000 characters.");
        
        // Category validation
        RuleFor(x => x.Category)
            .NotEmpty().WithMessage("Course category is required.")
            .MinimumLength(2).WithMessage("Course category must be at least 2 characters.")
            .MaximumLength(50).WithMessage("Course category cannot exceed 50 characters.");
        
        // Duration validation
        RuleFor(x => x.Duration)
            .GreaterThan(0).WithMessage("Course duration must be greater than zero.")
            .LessThanOrEqualTo(1000).WithMessage("Course duration cannot exceed 1000 hours.");
        
        // Level validation
        RuleFor(x => x.Level)
            .NotEmpty().WithMessage("Course level is required.")
            .Must(BeValidLevel).WithMessage("Course level must be Beginner, Intermediate, or Advanced.");
        
        // UpdatedBy validation
        RuleFor(x => x.UpdatedBy)
            .NotEmpty().WithMessage("Updated by user is required.")
            .MaximumLength(450).WithMessage("User ID cannot exceed 450 characters.");
        
        // Instructor validation (optional field)
        When(x => !string.IsNullOrWhiteSpace(x.Instructor), () =>
        {
            RuleFor(x => x.Instructor)
                .MaximumLength(200).WithMessage("Instructor information cannot exceed 200 characters.");
        });
        
        // Price validation (optional field)
        When(x => x.Price.HasValue, () =>
        {
            RuleFor(x => x.Price)
                .GreaterThanOrEqualTo(0).WithMessage("Course price cannot be negative.")
                .LessThanOrEqualTo(10000).WithMessage("Course price cannot exceed 10,000.");
        });
    }
    
    /// <summary>
    /// Validates if the level is one of the allowed values.
    /// </summary>
    private static bool BeValidLevel(string level)
    {
        var validLevels = new[] { "Beginner", "Intermediate", "Advanced" };
        return validLevels.Contains(level, StringComparer.OrdinalIgnoreCase);
    }
}
```

### Example 3: Get Courses Query Validator

```csharp
using FluentValidation;

namespace IdentityAuthorization.Features.Courses;

/// <summary>
/// Validator for GetCoursesQuery.
/// </summary>
public class GetCoursesQueryValidator : AbstractValidator<GetCoursesQuery>
{
    public GetCoursesQueryValidator()
    {
        // Page validation
        RuleFor(x => x.Page)
            .GreaterThan(0).WithMessage("Page number must be greater than zero.");
        
        // PageSize validation
        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("Page size must be greater than zero.")
            .LessThanOrEqualTo(100).WithMessage("Page size cannot exceed 100.");
        
        // Category validation (optional)
        When(x => !string.IsNullOrWhiteSpace(x.Category), () =>
        {
            RuleFor(x => x.Category)
                .MaximumLength(50).WithMessage("Category cannot exceed 50 characters.");
        });
        
        // Level validation (optional)
        When(x => !string.IsNullOrWhiteSpace(x.Level), () =>
        {
            RuleFor(x => x.Level)
                .Must(BeValidLevel).WithMessage("Level must be Beginner, Intermediate, or Advanced.");
        });
        
        // SearchTerm validation (optional)
        When(x => !string.IsNullOrWhiteSpace(x.SearchTerm), () =>
        {
            RuleFor(x => x.SearchTerm)
                .MinimumLength(2).WithMessage("Search term must be at least 2 characters.")
                .MaximumLength(100).WithMessage("Search term cannot exceed 100 characters.");
        });
    }
    
    /// <summary>
    /// Validates if the level is one of the allowed values.
    /// </summary>
    private static bool BeValidLevel(string level)
    {
        var validLevels = new[] { "Beginner", "Intermediate", "Advanced" };
        return validLevels.Contains(level, StringComparer.OrdinalIgnoreCase);
    }
}
```

---

## Common Validation Rules

### String Validation
```csharp
// Required
RuleFor(x => x.Property)
    .NotEmpty().WithMessage("Property is required.");

// Length constraints
RuleFor(x => x.Property)
    .MinimumLength(3).WithMessage("Property must be at least 3 characters.")
    .MaximumLength(200).WithMessage("Property cannot exceed 200 characters.");

// Exact length
RuleFor(x => x.Property)
    .Length(10).WithMessage("Property must be exactly 10 characters.");

// Email format
RuleFor(x => x.Email)
    .NotEmpty().WithMessage("Email is required.")
    .EmailAddress().WithMessage("Email format is invalid.");

// URL format
RuleFor(x => x.Website)
    .Must(BeValidUrl).WithMessage("Website URL is invalid.");

// Regular expression
RuleFor(x => x.PhoneNumber)
    .Matches(@"^\+?\d{10,15}$").WithMessage("Phone number format is invalid.");
```

### Numeric Validation
```csharp
// Greater than
RuleFor(x => x.Age)
    .GreaterThan(0).WithMessage("Age must be greater than zero.");

// Range
RuleFor(x => x.Price)
    .InclusiveBetween(0, 10000).WithMessage("Price must be between 0 and 10,000.");

// Must be one of specific values
RuleFor(x => x.Status)
    .Must(x => new[] { 1, 2, 3 }.Contains(x)).WithMessage("Status must be 1, 2, or 3.");
```

### Date Validation
```csharp
// Not in future
RuleFor(x => x.BirthDate)
    .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Birth date cannot be in the future.");

// Not in past
RuleFor(x => x.EventDate)
    .GreaterThanOrEqualTo(DateTime.UtcNow).WithMessage("Event date cannot be in the past.");

// Date range
RuleFor(x => x.StartDate)
    .LessThan(x => x.EndDate).WithMessage("Start date must be before end date.");
```

### Collection Validation
```csharp
// Not empty collection
RuleFor(x => x.Items)
    .NotEmpty().WithMessage("Items list cannot be empty.");

// Max count
RuleFor(x => x.Items)
    .Must(x => x.Count <= 100).WithMessage("Cannot exceed 100 items.");

// Validate each item
RuleForEach(x => x.Items)
    .SetValidator(new ItemValidator());
```

### Conditional Validation
```csharp
// When condition
When(x => x.Type == "Premium", () =>
{
    RuleFor(x => x.Price)
        .GreaterThan(100).WithMessage("Premium courses must cost more than 100.");
});

// Unless condition
Unless(x => x.IsFree, () =>
{
    RuleFor(x => x.Price)
        .GreaterThan(0).WithMessage("Paid courses must have a price greater than zero.");
});
```

---

## Async Validation

### Database Uniqueness Check

```csharp
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace IdentityAuthorization.Features.Courses;

/// <summary>
/// Validator with async database check for CreateCourseCommand.
/// </summary>
public class CreateCourseCommandValidator : AbstractValidator<CreateCourseCommand>
{
    private readonly ApplicationDbContext _dbContext;
    
    public CreateCourseCommandValidator(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
        
        // Standard validation rules
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Course title is required.")
            .MinimumLength(3).WithMessage("Course title must be at least 3 characters.")
            .MaximumLength(200).WithMessage("Course title cannot exceed 200 characters.")
            .MustAsync(BeUniqueTitleAsync).WithMessage("Course title already exists.");
    }
    
    /// <summary>
    /// Checks if course title is unique in the database.
    /// </summary>
    private async Task<bool> BeUniqueTitleAsync(
        string title,
        CancellationToken cancellationToken)
    {
        return !await _dbContext.Courses
            .AnyAsync(c => c.Title == title, cancellationToken);
    }
}
```

### External API Validation

```csharp
public class CreateCourseCommandValidator : AbstractValidator<CreateCourseCommand>
{
    private readonly IExternalApiService _apiService;
    
    public CreateCourseCommandValidator(IExternalApiService apiService)
    {
        _apiService = apiService;
        
        RuleFor(x => x.InstructorId)
            .NotEmpty().WithMessage("Instructor ID is required.")
            .MustAsync(InstructorExistsAsync).WithMessage("Instructor not found in external system.");
    }
    
    /// <summary>
    /// Validates instructor exists in external system.
    /// </summary>
    private async Task<bool> InstructorExistsAsync(
        string instructorId,
        CancellationToken cancellationToken)
    {
        return await _apiService.InstructorExistsAsync(instructorId, cancellationToken);
    }
}
```

---

## Custom Validators

### Reusable Custom Validator

```csharp
using FluentValidation;

namespace IdentityAuthorization.Validation;

/// <summary>
/// Custom validator for course levels.
/// </summary>
public static class CourseLevelValidator
{
    private static readonly string[] ValidLevels = { "Beginner", "Intermediate", "Advanced" };
    
    public static IRuleBuilderOptions<T, string> MustBeValidCourseLevel<T>(
        this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .Must(level => ValidLevels.Contains(level, StringComparer.OrdinalIgnoreCase))
            .WithMessage("Course level must be Beginner, Intermediate, or Advanced.");
    }
}

// Usage
public class CreateCourseCommandValidator : AbstractValidator<CreateCourseCommand>
{
    public CreateCourseCommandValidator()
    {
        RuleFor(x => x.Level)
            .NotEmpty()
            .MustBeValidCourseLevel();  // Custom extension
    }
}
```

---

## Validation Rules Best Practices

### DO:
? Provide clear, user-friendly error messages  
? Use async validation for database/API checks  
? Group related validations with comments  
? Create custom validators for reusable rules  
? Use When/Unless for conditional validation  
? Validate both presence and format  
? Include XML documentation  

### DON'T:
? Duplicate validation between validator and entity  
? Perform business logic in validators  
? Use synchronous I/O operations  
? Create validators without error messages  
? Skip validation for optional fields entirely  

---

## Registration in DI Container

```csharp
// Program.cs
using FluentValidation;

builder.Services.AddValidatorsFromAssemblyContaining<CreateCourseCommandValidator>();

// Or manually
builder.Services.AddScoped<IValidator<CreateCourseCommand>, CreateCourseCommandValidator>();
```

---

## Integration with MediatR

### Using Pipeline Behavior

```csharp
using FluentValidation;
using MediatR;

namespace IdentityAuthorization.Behaviors;

/// <summary>
/// MediatR pipeline behavior for FluentValidation.
/// </summary>
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;
    
    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }
    
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
        {
            return await next();
        }
        
        var context = new ValidationContext<TRequest>(request);
        
        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));
        
        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();
        
        if (failures.Any())
        {
            throw new ValidationException(failures);
        }
        
        return await next();
    }
}

// Register in Program.cs
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
```

---

## Copilot Summary

**Always generate validators with:**
- XML documentation
- Clear error messages for all rules
- Async validation when needed
- Custom validators for reusable logic
- Conditional validation with When/Unless
- Proper dependency injection

**Never include:**
- Business logic
- Synchronous I/O operations
- Generic error messages
- Duplicate validation logic
