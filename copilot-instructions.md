# Copilot Instructions - Table of Contents

This file contains references to all GitHub Copilot instruction files for generating consistent, high-quality code following Domain-Driven Design (DDD) and CQRS patterns.

---

## 📚 **Instruction Files**

### 1. **Domain Entities** (DDD Pattern)
**File:** [rich-domain-model.md](./rich-domain-model.md)

**Purpose:** Generate rich domain entities with encapsulated business logic

**When to Use:**
- Creating new domain entities (Order, Course, Inventory, etc.)
- Implementing business rules and validation
- Enforcing invariants and domain constraints

**Key Rules:**
- Private setters on all properties
- Validation in constructors
- Update() methods for state changes
- Timestamps (CreatedAt, UpdatedAt)

---

### 2. **Data Transfer Objects (DTOs)**
**File:** [dto-instructions.md](./dto-instructions.md)

**Purpose:** Generate immutable DTOs for data contracts between layers

**When to Use:**
- Creating request/response models for APIs
- Defining data contracts between layers
- Pagination and filtering models

**Key Rules:**
- Use records for immutability
- XML documentation
- Proper naming conventions (Request/Response suffixes)
- No business logic

---

### 3. **MediatR Commands**
**File:** [mediatr-command-instructions.md](./mediatr-command-instructions.md)

**Purpose:** Generate MediatR commands for write operations

**When to Use:**
- Creating entities (Create commands)
- Updating entities (Update commands)
- Deleting entities (Delete commands)
- Any state-changing operations

**Key Rules:**
- Implement IRequest<TResponse>
- Action verb naming (Create, Update, Delete)
- Include user context (CreatedBy, UpdatedBy)
- No business logic

---

### 4. **MediatR Queries**
**File:** [mediatr-query-instructions.md](./mediatr-query-instructions.md)

**Purpose:** Generate MediatR queries for read operations

**When to Use:**
- Retrieving single entities (GetById queries)
- Retrieving lists (Get, Search queries)
- Filtering and pagination
- Any read-only operations

**Key Rules:**
- Implement IRequest<TResponse>
- "Get" or "Search" naming prefixes
- Pagination for large datasets
- Nullable response types for single entities

---

### 5. **MediatR Handlers**
**File:** [mediatr-handler-instructions.md](./mediatr-handler-instructions.md)

**Purpose:** Generate MediatR handlers for processing commands and queries

**When to Use:**
- Implementing command handlers
- Implementing query handlers
- Processing business logic
- Database operations

**Key Rules:**
- Implement IRequestHandler<TRequest, TResponse>
- Dependency injection for all dependencies
- Comprehensive logging
- Private methods for complex operations
- Proper error handling

---

### 6. **FluentValidation Validators**
**File:** [fluentvalidation-instructions.md](./fluentvalidation-instructions.md)

**Purpose:** Generate FluentValidation validators for input validation

**When to Use:**
- Validating command/query inputs
- Enforcing data constraints
- Async validation (database checks)
- Custom validation rules

**Key Rules:**
- Inherit from AbstractValidator<T>
- Clear error messages
- Async validation when needed
- Conditional validation with When/Unless

---

### 7. **Minimal API Endpoints**
**File:** [minimal-api-endpoint-instructions.md](./minimal-api-endpoint-instructions.md)

**Purpose:** Generate Minimal API endpoints following RESTful conventions

**When to Use:**
- Exposing APIs to clients
- Mapping HTTP requests to MediatR
- Handling authentication/authorization
- Error handling and logging

**Key Rules:**
- Use MapGroup for organization
- OpenAPI metadata (.WithName, .WithOpenApi)
- Proper HTTP methods and status codes
- Extract user context from claims
- Comprehensive error handling

---

## 🎯 **Complete Feature Implementation Flow**

When implementing a complete feature (e.g., "Create Course"), follow this order:

### Step 1: Domain Entity
**File to reference:** `rich-domain-model.md`
```csharp
public class Course : Auditable
{
    // Private setters, validation, Update methods
}
```

### Step 2: DTOs
**File to reference:** `dto-instructions.md`
```csharp
public record CreateCourseRequest(...);
public record CourseResponse(...);
```

### Step 3: Command
**File to reference:** `mediatr-command-instructions.md`
```csharp
public record CreateCourseCommand(...) : IRequest<CourseResponse>;
```

### Step 4: Validator
**File to reference:** `fluentvalidation-instructions.md`
```csharp
public class CreateCourseCommandValidator : AbstractValidator<CreateCourseCommand> { }
```

### Step 5: Handler
**File to reference:** `mediatr-handler-instructions.md`
```csharp
public class CreateCourseHandler : IRequestHandler<CreateCourseCommand, CourseResponse> { }
```

### Step 6: Endpoint
**File to reference:** `minimal-api-endpoint-instructions.md`
```csharp
public static class CourseEndpoints
{
    public static void MapEndpoints(WebApplication app) { }
}
```

---

## 📋 **Quick Reference Table**

| Component | Instruction File | Key Pattern |
|-----------|-----------------|-------------|
| Domain Entity | rich-domain-model.md | `public class Entity : Auditable` |
| DTO | dto-instructions.md | `public record EntityRequest(...)` |
| Command | mediatr-command-instructions.md | `public record CreateEntityCommand(...) : IRequest<TResponse>` |
| Query | mediatr-query-instructions.md | `public record GetEntityQuery(...) : IRequest<TResponse>` |
| Handler | mediatr-handler-instructions.md | `public class Handler : IRequestHandler<TRequest, TResponse>` |
| Validator | fluentvalidation-instructions.md | `public class Validator : AbstractValidator<T>` |
| Endpoint | minimal-api-endpoint-instructions.md | `public static class EntityEndpoints` |

---

## 🔍 **Architecture Overview**

```
┌─────────────────────────────────────────────────────────────┐
│                      Minimal API Layer                      │
│  (Endpoints: HTTP Request → MediatR Command/Query)          │
└────────────────────┬────────────────────────────────────────┘
                     │
                     ↓
┌─────────────────────────────────────────────────────────────┐
│                    Application Layer                        │
│  Commands, Queries, Handlers, Validators (MediatR + FV)    │
└────────────────────┬────────────────────────────────────────┘
                     │
                     ↓
┌─────────────────────────────────────────────────────────────┐
│                      Domain Layer                           │
│  Rich Domain Entities (Business Logic & Validation)        │
└────────────────────┬────────────────────────────────────────┘
                     │
                     ↓
┌─────────────────────────────────────────────────────────────┐
│                   Infrastructure Layer                      │
│  (Database: Entity Framework Core + DbContext)             │
└─────────────────────────────────────────────────────────────┘
```

---

## ✅ **Code Quality Checklist**

Before finalizing any feature, ensure:

### Domain Entity
- ✅ Private setters on all properties
- ✅ Validation in constructor
- ✅ Update() method with validation
- ✅ Timestamps (CreatedAt, UpdatedAt)
- ✅ #region organization

### DTOs
- ✅ Records (immutable)
- ✅ XML documentation
- ✅ Proper naming (Request/Response)
- ✅ No business logic

### Commands/Queries
- ✅ Implements IRequest<TResponse>
- ✅ Descriptive naming
- ✅ User context included
- ✅ XML documentation

### Handlers
- ✅ Dependency injection
- ✅ Comprehensive logging
- ✅ Error handling
- ✅ Private methods for complexity

### Validators
- ✅ Clear error messages
- ✅ Async validation when needed
- ✅ All required fields validated

### Endpoints
- ✅ OpenAPI metadata
- ✅ Authorization
- ✅ Error handling
- ✅ Logging

---

## 📚 **Additional Resources**

- **DDD Patterns:** Domain-Driven Design by Eric Evans
- **CQRS:** CQRS Journey by Microsoft
- **MediatR:** https://github.com/jbogard/MediatR
- **FluentValidation:** https://docs.fluentvalidation.net/
- **Minimal APIs:** https://learn.microsoft.com/aspnet/core/fundamentals/minimal-apis

---

## 🎓 **Learning Path**

For new team members, study instruction files in this order:

1. **rich-domain-model.md** - Understand domain entities
2. **dto-instructions.md** - Learn data contracts
3. **mediatr-command-instructions.md** - Commands for writes
4. **mediatr-query-instructions.md** - Queries for reads
5. **mediatr-handler-instructions.md** - Processing logic
6. **fluentvalidation-instructions.md** - Input validation
7. **minimal-api-endpoint-instructions.md** - API layer

---

## 💡 **Tips for Using These Instructions**

1. **Always start with the domain entity** - It's the foundation
2. **Follow the implementation flow** - Don't skip steps
3. **Reference the examples** - They show the complete pattern
4. **Use the checklist** - Ensure quality before PR
5. **Keep instructions updated** - As patterns evolve

---

**Last Updated:** January 2025  
**Version:** 1.0  
**Status:** ✅ Production Ready
