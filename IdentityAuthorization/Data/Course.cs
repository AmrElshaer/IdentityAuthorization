namespace IdentityAuthorization.Data;

/// <summary>
/// Represents a Course entity in the domain.
/// Follows Domain-Driven Design (DDD) principles with encapsulated business logic.
/// </summary>
public class Course : Auditable
{
    #region Properties
    
    /// <summary>
    /// Gets the course title.
    /// </summary>
    public string Title { get; private set; } = string.Empty;
    
    /// <summary>
    /// Gets the course description.
    /// </summary>
    public string Description { get; private set; } = string.Empty;
    
    /// <summary>
    /// Gets the course category.
    /// </summary>
    public string Category { get; private set; } = string.Empty;
    
    /// <summary>
    /// Gets the course duration in hours.
    /// </summary>
    public int Duration { get; private set; }
    
    /// <summary>
    /// Gets the course level.
    /// </summary>
    public string Level { get; private set; } = string.Empty;
    
    /// <summary>
    /// Gets the instructor information (optional).
    /// </summary>
    public string? Instructor { get; private set; }
    
    /// <summary>
    /// Gets the course price (optional, nullable for free courses).
    /// </summary>
    public decimal? Price { get; private set; }
    
    /// <summary>
    /// Gets the user ID who created this course.
    /// </summary>
    public string CreatedBy { get; private set; } = string.Empty;
    
    /// <summary>
    /// Gets the user ID who last updated this course.
    /// </summary>
    public string? UpdatedBy { get; private set; }
    
    #endregion

    #region Constructors
    
    /// <summary>
    /// Private parameterless constructor for Entity Framework Core.
    /// </summary>
    private Course() { }

    /// <summary>
    /// Creates a new Course with required fields and validation.
    /// </summary>
    /// <param name="title">The course title.</param>
    /// <param name="description">The course description.</param>
    /// <param name="category">The course category.</param>
    /// <param name="duration">The course duration in hours.</param>
    /// <param name="level">The course level (Beginner/Intermediate/Advanced).</param>
    /// <param name="createdBy">The user creating the course.</param>
    /// <param name="instructor">Optional instructor information.</param>
    /// <param name="price">Optional course price (null for free courses).</param>
    /// <exception cref="ArgumentException">Thrown when required fields are invalid.</exception>
    /// <exception cref="DomainException">Thrown when domain rules are violated.</exception>
    public Course(string title, string description, string category, int duration, string level, string createdBy, string? instructor = null, decimal? price = null)
    {
        // Validate title
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Course title is required.", nameof(title));
        
        if (title.Length < 3)
            throw new DomainException("Course title must be at least 3 characters long.");
        
        if (title.Length > 200)
            throw new DomainException("Course title cannot exceed 200 characters.");
        
        // Validate description
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Course description is required.", nameof(description));
        
        if (description.Length < 10)
            throw new DomainException("Course description must be at least 10 characters long.");
        
        // Validate category
        if (string.IsNullOrWhiteSpace(category))
            throw new ArgumentException("Course category is required.", nameof(category));
        
        if (category.Length < 2)
            throw new DomainException("Course category must be at least 2 characters long.");
        
        // Validate duration
        if (duration <= 0)
            throw new DomainException("Course duration must be greater than zero.");
        
        // Validate level
        if (string.IsNullOrWhiteSpace(level))
            throw new ArgumentException("Course level is required.", nameof(level));
        
        var validLevels = new[] { "Beginner", "Intermediate", "Advanced" };
        if (!validLevels.Contains(level, StringComparer.OrdinalIgnoreCase))
            throw new DomainException("Course level must be Beginner, Intermediate, or Advanced.");
        
        // Validate created by
        if (string.IsNullOrWhiteSpace(createdBy))
            throw new ArgumentException("Created by user is required.", nameof(createdBy));
        
        // Validate price if provided
        if (price.HasValue && price.Value < 0)
            throw new DomainException("Course price cannot be negative.");
        
        // Set properties
        Title = title;
        Description = description;
        Category = category;
        Duration = duration;
        Level = level;
        Instructor = instructor;
        CreatedBy = createdBy;
        Price = price;
        
        // Set audit timestamp
        CreatedAt = DateTime.UtcNow;
    }
    
    #endregion

    #region Methods
    
    /// <summary>
    /// Updates the course with new values and enforces validation rules.
    /// </summary>
    /// <param name="title">The course title.</param>
    /// <param name="description">The course description.</param>
    /// <param name="category">The course category.</param>
    /// <param name="duration">The course duration in hours.</param>
    /// <param name="level">The course level.</param>
    /// <param name="updatedBy">The user updating the course.</param>
    /// <param name="instructor">Optional instructor information.</param>
    /// <param name="price">Optional course price.</param>
    /// <exception cref="ArgumentException">Thrown when required fields are invalid.</exception>
    /// <exception cref="DomainException">Thrown when domain rules are violated.</exception>
    public void Update(string title, string description, string category, int duration, string level, string updatedBy, string? instructor = null, decimal? price = null)
    {
        // Validate title
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Course title is required.", nameof(title));
        
        if (title.Length < 3)
            throw new DomainException("Course title must be at least 3 characters long.");
        
        if (title.Length > 200)
            throw new DomainException("Course title cannot exceed 200 characters.");
        
        // Validate description
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Course description is required.", nameof(description));
        
        if (description.Length < 10)
            throw new DomainException("Course description must be at least 10 characters long.");
        
        // Validate category
        if (string.IsNullOrWhiteSpace(category))
            throw new ArgumentException("Course category is required.", nameof(category));
        
        if (category.Length < 2)
            throw new DomainException("Course category must be at least 2 characters long.");
        
        // Validate duration
        if (duration <= 0)
            throw new DomainException("Course duration must be greater than zero.");
        
        // Validate level
        if (string.IsNullOrWhiteSpace(level))
            throw new ArgumentException("Course level is required.", nameof(level));
        
        var validLevels = new[] { "Beginner", "Intermediate", "Advanced" };
        if (!validLevels.Contains(level, StringComparer.OrdinalIgnoreCase))
            throw new DomainException("Course level must be Beginner, Intermediate, or Advanced.");
        
        // Validate updated by
        if (string.IsNullOrWhiteSpace(updatedBy))
            throw new ArgumentException("Updated by user is required.", nameof(updatedBy));
        
        // Validate price if provided
        if (price.HasValue && price.Value < 0)
            throw new DomainException("Course price cannot be negative.");
        
        // Set properties
        Title = title;
        Description = description;
        Category = category;
        Duration = duration;
        Level = level;
        Instructor = instructor;
        Price = price;
        UpdatedBy = updatedBy;
        
        // Update audit timestamp
        UpdatedAt = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Updates only the course title.
    /// </summary>
    /// <param name="title">The new course title.</param>
    /// <param name="updatedBy">The user updating the course.</param>
    /// <exception cref="ArgumentException">Thrown when title is invalid.</exception>
    /// <exception cref="DomainException">Thrown when domain rules are violated.</exception>
    public void UpdateTitle(string title, string updatedBy)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Course title is required.", nameof(title));
        
        if (title.Length < 3)
            throw new DomainException("Course title must be at least 3 characters long.");
        
        if (title.Length > 200)
            throw new DomainException("Course title cannot exceed 200 characters.");
        
        if (string.IsNullOrWhiteSpace(updatedBy))
            throw new ArgumentException("Updated by user is required.", nameof(updatedBy));
        
        Title = title;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Updates only the course price.
    /// </summary>
    /// <param name="price">The new course price (null for free courses).</param>
    /// <param name="updatedBy">The user updating the course.</param>
    /// <exception cref="ArgumentException">Thrown when user is invalid.</exception>
    /// <exception cref="DomainException">Thrown when price is invalid.</exception>
    public void UpdatePrice(decimal? price, string updatedBy)
    {
        if (string.IsNullOrWhiteSpace(updatedBy))
            throw new ArgumentException("Updated by user is required.", nameof(updatedBy));
        
        if (price.HasValue && price.Value < 0)
            throw new DomainException("Course price cannot be negative.");
        
        Price = price;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Marks the course as free by setting price to null.
    /// </summary>
    /// <param name="updatedBy">The user making the change.</param>
    /// <exception cref="ArgumentException">Thrown when user is invalid.</exception>
    public void MakeFree(string updatedBy)
    {
        if (string.IsNullOrWhiteSpace(updatedBy))
            throw new ArgumentException("Updated by user is required.", nameof(updatedBy));
        
        Price = null;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Checks if the course is free.
    /// </summary>
    /// <returns>True if the course is free; otherwise, false.</returns>
    public bool IsFree()
    {
        return !Price.HasValue || Price.Value == 0;
    }
    
    /// <summary>
    /// Validates if the course duration is short (less than 2 hours).
    /// </summary>
    /// <returns>True if duration is less than 2 hours; otherwise, false.</returns>
    public bool IsShortCourse()
    {
        return Duration < 2;
    }
    
    /// <summary>
    /// Validates if the course is in a specific category.
    /// </summary>
    /// <param name="category">The category to check.</param>
    /// <returns>True if course is in the specified category; otherwise, false.</returns>
    public bool IsInCategory(string category)
    {
        if (string.IsNullOrWhiteSpace(category))
            return false;
        
        return Category.Equals(category, StringComparison.OrdinalIgnoreCase);
    }
    
    #endregion
}
