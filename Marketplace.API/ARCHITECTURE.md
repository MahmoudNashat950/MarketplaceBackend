# ??? Marketplace Backend - Architecture & Implementation Guide

## Project Structure

```
Marketplace.API/
??? Controllers/                 # HTTP request handlers
?   ??? AuthController.cs        # Auth endpoints
?   ??? ProductController.cs     # Product CRUD endpoints
?   ??? OrderController.cs       # Order management endpoints
?   ??? ReviewController.cs      # Review endpoints
?   ??? CategoriesController.cs  # Category endpoints
?   ??? FlagController.cs        # Flag endpoints
??? Services/                    # Business logic layer
?   ??? AuthService.cs           # User registration & authentication
?   ??? ProductService.cs        # Product operations
?   ??? OrderService.cs          # Order processing
?   ??? ReviewService.cs         # Review management
?   ??? CategoryService.cs       # Category operations
?   ??? CommentService.cs        # Order comments
?   ??? FlagService.cs           # User flagging
??? Interfaces/                  # Service contracts
?   ??? IAuthService.cs
?   ??? IProductService.cs
?   ??? IOrderService.cs
?   ??? IReviewService.cs
?   ??? ICategoryService.cs
?   ??? ICommentService.cs
?   ??? IFlagService.cs
??? DTOs/                        # Data Transfer Objects
?   ??? AuthDtos.cs
?   ??? ProductDtos.cs
?   ??? OrderDtos.cs
?   ??? ReviewDtos.cs
?   ??? CategoryDtos.cs
?   ??? CommentDtos.cs
?   ??? FlagDtos.cs
??? Enums/                       # Enumeration types
?   ??? UserRole.cs              # Buyer=0, Seller=1
?   ??? OrderStatus.cs           # Order statuses
??? Middleware/                  # Custom middleware
?   ??? ExceptionHandlingMiddleware.cs
??? Utils/                       # Utility functions
?   ??? JsonConverters/
?       ??? NullToEmptyEnumerableConverterFactory.cs
??? Program.cs                   # DI & app configuration
??? appsettings.json            # App config
??? Properties/
    ??? launchSettings.json      # Launch profiles
```

---

## ?? Clean Architecture Layers

### 1. **Presentation Layer** (API)
- **Location**: `Controllers/`
- **Responsibility**: Accept HTTP requests, route to services, format responses
- **Rules**:
  - ? Extract user ID from JWT claims
  - ? Call corresponding service
  - ? Return standardized JSON responses
  - ? Never contain business logic
  - ? Never directly access database

**Example (ProductController.cs):**
```csharp
[HttpPost]
[Authorize(Roles = "seller")]
public async Task<IActionResult> Create(
    [FromBody] CreateProductDto dto, 
    [FromServices] IProductService svc)
{
    var sellerId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
    try
    {
        var created = await svc.CreateAsync(dto, sellerId);
        return Ok(created);  // 200 OK
    }
    catch (ApplicationException ex)
    {
        return BadRequest(new { message = ex.Message });  // 400 Bad Request
    }
}
```

---

### 2. **Application Layer** (Services)
- **Location**: `Services/` + `Interfaces/`
- **Responsibility**: Business logic, validation, orchestration
- **Rules**:
  - ? Implement all business rules
  - ? Validate input data
  - ? Enforce role-based access
  - ? Handle exceptions gracefully
  - ? Never return HTTP responses
  - ? Never reference HttpContext

**Key Services:**

#### **AuthService**
```csharp
public class AuthService : IAuthService
{
    public async Task RegisterAsync(RegisterDto dto)
    {
        // 1. Validate input
        if (string.IsNullOrWhiteSpace(dto.email)) 
            throw new ApplicationException("Email is required.");
        
        // 2. Check duplicate
        if (await _db.Users.AnyAsync(u => u.Email == dto.email))
            throw new ApplicationException("Email already in use.");
        
        // 3. Create user
        var user = new User 
        { 
            Email = dto.email, 
            PasswordHash = BCrypt.HashPassword(dto.password),
            Role = dto.role == "seller" ? 1 : 0
        };
        _db.Users.Add(user);
        
        // 4. Persist
        await _db.SaveChangesAsync();
    }
    
    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        // 1. Find user
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == dto.email);
        if (user == null || !VerifyPassword(dto.password, user.PasswordHash))
            throw new ApplicationException("Invalid credentials.");
        
        // 2. Generate token
        var token = GenerateToken(user);
        
        // 3. Return response
        return new AuthResponseDto(token, new UserDto(...));
    }
}
```

#### **ProductService**
- **CreateAsync(dto, sellerId)**: Create product, set SellerId
- **UpdateAsync(id, dto, sellerId)**: Update only if seller owns product
- **DeleteAsync(id, sellerId)**: Delete only if seller owns product
- **GetAllAsync()**: Public listing
- **GetByIdAsync(id)**: Product detail

**Ownership Validation:**
```csharp
public async Task<ProductDto> UpdateAsync(int id, UpdateProductDto dto, int sellerId)
{
    var product = await _db.Products.FindAsync(id);
    if (product == null) 
        throw new ApplicationException("Product not found.");
    
    // ? Ownership check
    if (product.SellerId != sellerId) 
        throw new ApplicationException("You can only update your own products.");
    
    // Update fields and save
    product.Name = dto.Name;
    await _db.SaveChangesAsync();
    return Map(product);
}
```

#### **OrderService**
- **CreateAsync(dto, buyerId)**: Create order, reduce stock, calculate total
- **GetBuyerOrdersAsync(buyerId)**: Buyer's order history
- **GetSellerOrdersAsync(sellerId)**: Seller's received orders
- **UpdateStatusAsync(id, status, sellerId)**: Update order status with seller validation

**Stock Reduction:**
```csharp
foreach (var item in dto.items)
{
    var product = await _db.Products.FindAsync(item.productId);
    
    if (product.Stock < item.quantity) 
        throw new ApplicationException($"Insufficient stock for {product.Name}.");
    
    product.Stock -= item.quantity;  // Reduce stock
    
    var orderItem = new OrderItem 
    { 
        OrderId = order.Id, 
        ProductId = product.Id, 
        Quantity = item.quantity, 
        Price = product.Price 
    };
    _db.OrderItems.Add(orderItem);
}
await _db.SaveChangesAsync();
```

#### **ReviewService**
- **AddReviewAsync(dto, userId)**: Add review with duplicate check
- **GetReviewsByProductAsync(productId)**: Product review list
- **GetSummaryAsync(productId)**: Aggregate rating stats

**Duplicate Check (One review per user per product):**
```csharp
if (await _db.Ratings.AnyAsync(r => 
    r.ProductId == dto.productId && r.UserId == userId)) 
    throw new ApplicationException("You have already reviewed this product.");
```

---

### 3. **Infrastructure Layer** (Data Access)
- **Location**: `Marketplace.Domain/Entities/MarketplaceDbContext.cs`
- **Pattern**: Database-First with Entity Framework Core
- **Rules**:
  - ? Use DbContext for all data access
  - ? Leverage navigation properties
  - ? Configure relationships in OnModelCreating
  - ? Never modify generated entities or DbContext

**DbContext Setup (Program.cs):**
```csharp
builder.Services.AddDbContext<MarketplaceDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
```

**Connection String (appsettings.json):**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=MarketplaceDB;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

---

### 4. **Domain Layer** (Entities)
- **Location**: `Marketplace.Domain/Entities/`
- **Entities**:
  - `User`: id, name, email, passwordHash, role
  - `Product`: id, name, price, stock, sellerId, categoryId, discount, imageUrl
  - `Category`: id, name
  - `Order`: id, buyerId, status, createdAt
  - `OrderItem`: id, orderId, productId, quantity, price
  - `Rating`: id, productId, userId, value, createdAt
  - `Comment`: id, productId, userId, content, createdAt
  - `OrderComment`: id, orderId, userId, text, createdAt
  - `Flag`: id, reporterId, reportedId, reason, createdAt

**Enums:**
```csharp
public enum UserRole
{
    Buyer = 0,
    Seller = 1
}

public enum OrderStatus
{
    Pending = 0,
    Processing = 1,
    Shipped = 2,
    Delivered = 3,
    Cancelled = 4
}
```

---

## ?? Security Architecture

### JWT Authentication Flow

```
1. User registers/logs in
        ?
2. AuthService validates credentials & creates user
        ?
3. AuthService generates JWT token containing:
   - userId (NameIdentifier claim)
   - role (Role claim)
   - name (Name claim)
   - exp (expiration: 7 days)
        ?
4. Token returned to client
        ?
5. Client sends token in Authorization header
        ?
6. [Authorize] attribute validates token
        ?
7. User.FindFirst(ClaimTypes.NameIdentifier) extracts userId
        ?
8. [Authorize(Roles = "seller")] enforces role
```

### Token Generation (AuthService.cs)
```csharp
private string GenerateToken(User user)
{
    var jwtKey = _config["Jwt:Key"] ?? "VerySecretKey12345888888888";
    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    var claims = new[]
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),      // userId
        new Claim(ClaimTypes.Role, user.Role == 1 ? "seller" : "buyer"), // role
        new Claim(ClaimTypes.Name, user.Name)                            // name
    };

    var token = new JwtSecurityToken(
        claims: claims, 
        expires: DateTime.UtcNow.AddDays(7), 
        signingCredentials: creds);
    
    return new JwtSecurityTokenHandler().WriteToken(token);
}
```

### Authorization Examples

**Seller Only:**
```csharp
[Authorize(Roles = "seller")]
public async Task<IActionResult> CreateProduct(...) { }
```

**Buyer Only:**
```csharp
[Authorize(Roles = "buyer")]
public async Task<IActionResult> CreateOrder(...) { }
```

**Public:**
```csharp
[AllowAnonymous]
[HttpGet]
public async Task<IActionResult> GetProducts(...) { }
```

---

## ?? Data Models & Relationships

### User-Product Relationship
```
User (Seller)
  ?? ProductSellers (ICollection<Product>)
  ?  ?? SellerId (FK)
  ?
  ?? ProductUsers (ICollection<Product>)
     ?? UserId (FK, optional)
```

### Order Flow
```
User (Buyer)
  ?? Orders (ICollection<Order>)
     ?? Order
        ?? OrderItems (ICollection<OrderItem>)
        ?  ?? OrderItem
        ?     ?? Product (FK)
        ?     ?? Quantity
        ?     ?? Price (snapshot)
        ?
        ?? OrderComments (ICollection<OrderComment>)
           ?? OrderComment
              ?? User (Commenter)
```

### Review System
```
Product
  ?? RatingProducts (ICollection<Rating>)
  ?  ?? Rating
  ?     ?? User (Reviewer)
  ?     ?? Value (1-5)
  ?     ?? CreatedAt
  ?     ?? UpdatedAt
  ?
  ?? Comments (ICollection<Comment>)
     ?? Comment
        ?? User (Commenter)
        ?? Content
```

---

## ?? Data Transfer Objects (DTOs)

DTOs decouple API contracts from entities, allowing safe API evolution.

### Auth DTOs
```csharp
public record RegisterDto(string name, string email, string password, string role);
public record LoginDto(string email, string password);
public record AuthResponseDto(string token, UserDto user);
```

### Product DTOs
```csharp
// Output DTO (includes computed fields)
public record ProductDto(
    int id, string name, decimal price, int stock, int deliveryTimeInDays,
    string category, int categoryId, string? imageUrl, decimal? discount,
    double rating, int reviewsCount);

// Input DTOs
public record CreateProductDto(
    string Name, decimal Price, int Stock, int CategoryId, 
    int DeliveryTimeInDays, decimal? Discount, string? ImageUrl);

public record UpdateProductDto(
    string Name, decimal Price, int Stock, int CategoryId,
    int DeliveryTimeInDays, decimal? Discount, string? ImageUrl);
```

### Order DTOs
```csharp
public record OrderDto(
    int id, string status, DateTime? createdAt, 
    decimal totalPrice, List<OrderItemDto> items);

public record OrderItemDto(
    int productId, string productName, 
    int quantity, decimal price);

public record CreateOrderDto(List<CreateOrderItemDto> items);
public record CreateOrderItemDto(int productId, int quantity);
```

---

## ?? Dependency Injection Setup (Program.cs)

```csharp
// Database
builder.Services.AddDbContext<MarketplaceDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Services (Scoped = one per request)
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddScoped<IFlagService, FlagService>();
```

**Lifetime Options:**
- **Transient**: New instance every time (stateless utilities)
- **Scoped**: One per HTTP request (services, DbContext)
- **Singleton**: One for entire application (config, logging)

---

## ?? Business Logic Examples

### Example 1: Creating an Order

```
Controller (ProductController.cs)
    ?
1. Extract buyerId from JWT token
2. Call IOrderService.CreateAsync(dto, buyerId)
    ?
Service (OrderService.cs)
    ?
3. Create Order entity with Pending status
4. For each item:
   a. Get Product from database
   b. Validate stock availability
   c. Reduce stock
   d. Create OrderItem with price snapshot
5. Save all changes
6. Return OrderDto with calculated total
    ?
Controller
    ?
7. Return 200 OK with order details
```

### Example 2: Updating a Product

```
Controller (ProductController.cs)
    ?
1. Extract sellerId from JWT token
2. Call IProductService.UpdateAsync(id, dto, sellerId)
    ?
Service (ProductService.cs)
    ?
3. Load Product from database
4. Verify product exists
5. Verify sellerId owns the product ? Ownership check
6. Update all fields
7. Save changes
8. Return updated ProductDto
    ?
Controller
    ?
9. Return 200 OK or 400 Bad Request
```

### Example 3: Adding a Review

```
Controller (ReviewController.cs)
    ?
1. Extract userId (buyer) from JWT token
2. Validate rating is 1-5
3. Call IReviewService.AddReviewAsync(dto, userId)
    ?
Service (ReviewService.cs)
    ?
4. Check if user already reviewed this product
   (unique constraint: productId + userId)
5. Create Rating entity
6. Save to database
7. Return ReviewDto
    ?
Controller
    ?
8. Return 200 OK or 400 if duplicate review
```

---

## ?? Testing Recommendations

### Unit Tests (Services)
```csharp
[TestFixture]
public class ProductServiceTests
{
    private ProductService _service;
    private Mock<MarketplaceDbContext> _mockDb;
    
    [Test]
    public async Task UpdateAsync_WithDifferentSeller_ThrowsException()
    {
        var productOwnedBySeller1 = new Product { Id = 1, SellerId = 1 };
        
        // Act & Assert
        var ex = Assert.ThrowsAsync<ApplicationException>(
            () => _service.UpdateAsync(1, updateDto, sellerId: 2));
        
        Assert.That(ex.Message, Contains.Substring("own products"));
    }
}
```

### Integration Tests (API)
```csharp
[TestFixture]
public class ProductControllerTests
{
    private WebApplicationFactory<Program> _factory;
    private HttpClient _client;
    
    [Test]
    public async Task CreateProduct_WithoutToken_Returns401()
    {
        var response = await _client.PostAsJsonAsync("/api/Product", createDto);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }
}
```

---

## ?? Performance Considerations

### 1. **Database Queries**
```csharp
// ? N+1 problem - loads product for each item
var orders = await _db.Orders.ToListAsync();
foreach (var order in orders)
{
    var items = await _db.OrderItems.Where(oi => oi.OrderId == order.Id).ToListAsync();
}

// ? Eager loading - single query
var orders = await _db.Orders
    .Include(o => o.OrderItems)
    .ToListAsync();
```

### 2. **Index Usage**
```csharp
// Database has indexes on:
// - Comments: (ProductId, UserId) - unique
// - Ratings: (ProductId, UserId) - unique
// - Orders: BuyerId
// - Products: SellerId, CategoryId
```

### 3. **Asynchronous Operations**
```csharp
// ? Async all the way
public async Task<ProductDto> CreateAsync(CreateProductDto dto, int sellerId)
{
    // ... validation ...
    _db.Products.Add(product);
    await _db.SaveChangesAsync();  // Async I/O
    return Map(product);
}
```

---

## ?? Deployment Checklist

- [ ] Update `appsettings.Production.json`
- [ ] Set `Jwt:Key` as environment variable
- [ ] Update database connection string
- [ ] Update CORS origins to production domain
- [ ] Set `ASPNETCORE_ENVIRONMENT=Production`
- [ ] Disable Swagger in production
- [ ] Enable HTTPS redirection
- [ ] Configure SQL Server backups
- [ ] Set up application logging
- [ ] Configure CDN for product images
- [ ] Run database migrations if needed
- [ ] Load test the API

---

## ?? Code Style Guidelines

### Naming Conventions
```csharp
// Classes & Methods: PascalCase
public class ProductService { }
public async Task<ProductDto> GetByIdAsync(int id) { }

// Variables & Parameters: camelCase
var productId = 1;
foreach (var item in items) { }

// Constants: UPPER_SNAKE_CASE (if any)
private const int MAX_REVIEW_RATING = 5;

// Private fields: _camelCase
private readonly MarketplaceDbContext _db;
private readonly IConfiguration _config;
```

### Documentation
```csharp
/// <summary>
/// Creates a new product for the seller.
/// </summary>
/// <param name="dto">Product creation data</param>
/// <param name="sellerId">ID of the seller creating the product</param>
/// <returns>Created product DTO</returns>
/// <exception cref="ApplicationException">
/// Thrown when validation fails or database error occurs
/// </exception>
public async Task<ProductDto> CreateAsync(CreateProductDto dto, int sellerId)
{
    // Implementation...
}
```

---

## ? Summary

| Component | Location | Purpose |
|-----------|----------|---------|
| Controllers | `Controllers/` | HTTP routing & request handling |
| Services | `Services/` | Business logic & validation |
| Interfaces | `Interfaces/` | Service contracts |
| DTOs | `DTOs/` | API data contracts |
| Entities | `Marketplace.Domain/Entities/` | Database models |
| DbContext | `MarketplaceDbContext.cs` | EF Core data access |
| Enums | `Enums/` | Type-safe constants |
| Middleware | `Middleware/` | Cross-cutting concerns |

---

**Status**: ? Production Ready  
**Version**: 1.0  
**Last Updated**: 2024
