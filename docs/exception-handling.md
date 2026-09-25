# Exception Handling

WebServiceToolkit provides standardized exception types that automatically map to HTTP status codes when used with `ControllerUtils`.

## Overview

All toolkit exceptions derive from `WebServiceException`, which carries the HTTP status code:

| Exception | HTTP Status | Use Case |
|-----------|-------------|----------|
| `BadRequestException` | 400 | Invalid request data (optionally names the property) |
| `UnauthorizedException` | 401 | Authentication failed |
| `ForbiddenException` | 403 | Authenticated, but not allowed |
| `RecordNotFoundException` | 404 | Resource not found |
| `RecordConflictException` | 409 | Resource conflict |
| `UnprocessableEntityException` | 422 | Business rule violated (optionally names the property) |
| your own `WebServiceException` subclass | any | Domain-specific status |

Every error response has a `WebServiceError` JSON body — `{ errorType, message, propertyName }` —
wire-compatible with DevInstance.BlazorToolkit's `ServiceActionError`. `errorType` is `Validation`
(3) for 400/422, `General` (2) for other handled errors, and `Exception` (1) for 500.

## Using ControllerUtils

### HandleWebRequestAsync

Wrap your controller actions with `HandleWebRequestAsync` to automatically convert exceptions to HTTP responses:

```csharp
using DevInstance.WebServiceToolkit.Controllers;
using DevInstance.WebServiceToolkit.Exceptions;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    [HttpGet("{id}")]
    public Task<ActionResult<Product>> GetProduct(string id)
    {
        return this.HandleWebRequestAsync<Product>(async () =>
        {
            var product = await _productService.GetByIdAsync(id);
            if (product == null)
                throw new RecordNotFoundException(id);  // Returns 404
            return Ok(product);
        });
    }
}
```

### HandleWebRequest (synchronous)

For synchronous operations:

```csharp
[HttpGet("health")]
public ActionResult<HealthStatus> GetHealth()
{
    return this.HandleWebRequest<HealthStatus>(() =>
    {
        return Ok(new HealthStatus { Status = "Healthy" });
    });
}
```

## Exception Types

### RecordNotFoundException

Use when a requested resource doesn't exist:

```csharp
public async Task<Product> GetByIdAsync(string id)
{
    var product = await _repository.FindByIdAsync(id);
    if (product == null)
        throw new RecordNotFoundException(id);
    return product;
}
```

**HTTP Response:** 404 Not Found

### BadRequestException

Use when the client request is invalid:

```csharp
public async Task<Product> CreateAsync(CreateProductRequest request)
{
    if (string.IsNullOrWhiteSpace(request.Name))
        throw new BadRequestException("Product name is required");

    if (request.Price < 0)
        throw new BadRequestException("Price cannot be negative");

    // ... create product
}
```

**HTTP Response:** 400 Bad Request with message

### RecordConflictException

Use when the request conflicts with existing data:

```csharp
public async Task<User> CreateAsync(CreateUserRequest request)
{
    var existing = await _repository.FindByEmailAsync(request.Email);
    if (existing != null)
        throw new RecordConflictException($"Email {request.Email} is already registered");

    // ... create user
}
```

**HTTP Response:** 409 Conflict

### UnauthorizedException

Use when authentication fails:

```csharp
public async Task<User> AuthenticateAsync(string token)
{
    var user = await _tokenService.ValidateAsync(token);
    if (user == null)
        throw new UnauthorizedException("Invalid or expired token");
    return user;
}
```

**HTTP Response:** 401 Unauthorized with message

## Exception Handling Flow

```
Controller Action
       │
       ▼
HandleWebRequestAsync
       │
       ├─── Success ──────────► Returns ActionResult
       │
       └─── Exception ────────► Catch and Convert
                                      │
            ┌─────────────────────────┼─────────────────────────┐
            │                         │                         │
            ▼                         ▼                         ▼
    RecordNotFound            BadRequest              RecordConflict
         │                         │                         │
         ▼                         ▼                         ▼
      404 Not Found          400 Bad Request          409 Conflict

            │                         │                         │
            ▼                         ▼                         ▼
    Unauthorized /            Other Exception
    Forbidden / 422 /              │
    other WebServiceException      ▼
         │                   500 (no stack trace)
         ▼
    its StatusCode

    Every error response body is a WebServiceError.
```

## Unhandled Exceptions

Exceptions that are not a `WebServiceException` are logged as errors and converted to a 500
response. The body never contains the stack trace; the exception message is included only when
the host environment is Development, otherwise it is `"An unexpected error occurred."`:

```json
{ "errorType": 1, "message": "An unexpected error occurred.", "propertyName": null }
```

## Model Validation Errors

`[ApiController]` rejects invalid models before the action runs. Register
`AddWebServiceToolkitErrors()` so those 400 responses also use `WebServiceError`
(the first invalid property becomes `propertyName`):

```csharp
builder.Services.AddControllers().AddWebServiceToolkitErrors();
```

## Global Exception Handlers

For exceptions thrown outside `HandleWebRequestAsync` (middleware, filters), an
`IExceptionHandler` can produce the identical body:

```csharp
var (status, error) = ControllerUtils.ToWebServiceError(exception, env.IsDevelopment());
httpContext.Response.StatusCode = status;
await httpContext.Response.WriteAsJsonAsync(error);
```

## Best Practices

### 1. Throw from Services, Not Controllers

Keep exception logic in your service layer:

```csharp
// Good: Service throws the exception
public class ProductService : IProductService
{
    public async Task<Product> GetByIdAsync(string id)
    {
        var product = await _repository.FindByIdAsync(id);
        if (product == null)
            throw new RecordNotFoundException(id);
        return product;
    }
}

// Controller just wraps with HandleWebRequestAsync
[HttpGet("{id}")]
public Task<ActionResult<Product>> GetProduct(string id)
{
    return this.HandleWebRequestAsync<Product>(async () =>
    {
        return Ok(await _productService.GetByIdAsync(id));
    });
}
```

### 2. Include Helpful Messages

```csharp
// Good: Descriptive message
throw new BadRequestException("Email address is invalid. Please provide a valid email.");

// Avoid: Generic message
throw new BadRequestException("Invalid input");
```

### 3. Use Specific Exception Types

```csharp
// Good: Specific exception
if (user == null)
    throw new RecordNotFoundException($"User with ID {id}");

// Avoid: Wrong exception type
if (user == null)
    throw new BadRequestException("User not found");
```

### 4. Validate Early

```csharp
public async Task<Product> UpdateAsync(string id, UpdateProductRequest request)
{
    // Validate input first
    if (request.Price < 0)
        throw new BadRequestException("Price cannot be negative");

    // Then check existence
    var product = await _repository.FindByIdAsync(id);
    if (product == null)
        throw new RecordNotFoundException(id);

    // Proceed with update
    // ...
}
```

## Custom Exception Handling

If you need to handle additional exception types, you can create your own wrapper:

```csharp
public static class CustomControllerUtils
{
    public static async Task<ActionResult<T>> HandleWithCustomExceptions<T>(
        this ControllerBase controller,
        Func<Task<ActionResult<T>>> handler)
    {
        try
        {
            return await handler();
        }
        catch (PaymentRequiredException)
        {
            return controller.StatusCode(402, "Payment required");
        }
        catch (RateLimitException ex)
        {
            return controller.StatusCode(429, ex.Message);
        }
        catch (Exception ex)
        {
            // Fall back to standard handling
            return await controller.HandleWebRequestAsync(async () => await handler());
        }
    }
}
```

## See Also

- [Getting Started](getting-started.md)
- [Service Registration](service-registration.md)
