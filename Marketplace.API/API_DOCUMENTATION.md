# ?? Marketplace API Documentation

**ASP.NET Core 8 | Clean Architecture | Database-First EF Core | JWT Authentication**

---

## ?? Table of Contents

1. [Overview](#overview)
2. [Architecture](#architecture)
3. [Authentication](#authentication)
4. [API Endpoints](#api-endpoints)
5. [Response Format](#response-format)
6. [Error Handling](#error-handling)
7. [Role-Based Access](#role-based-access)

---

## ?? Overview

This is a **two-sided marketplace** backend supporting:
- **Buyers**: Browse products, create orders, review products, flag sellers
- **Sellers**: Manage products, fulfill orders, flag buyers

**Stack:**
- ASP.NET Core 8
- Entity Framework Core (Database-First)
- SQL Server
- JWT Authentication
- Swagger/OpenAPI

---

## ??? Architecture

### Layers

```
??? API Layer (Controllers)
?   ??? No business logic, only request/response handling
??? Application Layer (Services & DTOs)
?   ??? Services (Business Logic)
?   ??? Interfaces (Contracts)
?   ??? DTOs (Data Transfer Objects)
??? Infrastructure Layer (EF Core)
?   ??? MarketplaceDbContext
??? Domain Layer (Entities)
    ??? User, Product, Category, Order, OrderItem
    ??? Rating, Comment, Flag, OrderComment
    ??? Enums (UserRole, OrderStatus)
```

### Key Principles

- ? **Single Responsibility**: Each service handles one domain
- ? **Dependency Injection**: All services registered in Program.cs
- ? **Validation**: Business logic validated in services
- ? **Ownership Checks**: Sellers can only manage their own products
- ? **Never null arrays**: Empty arrays return `[]`

---

## ?? Authentication

### JWT Token Structure

```json
{
  "sub": "1",                    // User ID
  "NameIdentifier": "1",         // User ID (claim)
  "role": "buyer",               // User Role
  "name": "John Doe",            // User Name
  "exp": 1700000000
}
```

### Token Generation

- **Algorithm**: HS256 (HMAC SHA256)
- **Key**: From `appsettings.json` -> `Jwt:Key`
- **Expiration**: 7 days
- **Fallback Key** (dev): `VerySecretKey12345888888888`

### Usage

```http
Authorization: Bearer <token>
```

---

## ?? API Endpoints

### 1. Authentication

#### Register
```http
POST /api/Auth/register
Content-Type: application/json

{
  "name": "John Doe",
  "email": "john@example.com",
  "password": "SecurePassword123",
  "role": "buyer"              // "buyer" or "seller"
}
```

**Response (200):**
```json
{
  "message": "Registration successful."
}
```

**Errors (400):**
```json
{
  "message": "Email already in use."
}
```

---

#### Login
```http
POST /api/Auth/login
Content-Type: application/json

{
  "email": "john@example.com",
  "password": "SecurePassword123"
}
```

**Response (200):**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "user": {
    "id": 1,
    "name": "John Doe",
    "email": "john@example.com",
    "role": "buyer"
  }
}
```

**Errors (401):**
```json
{
  "message": "Invalid credentials."
}
```

---

### 2. Products

#### Get All Products
```http
GET /api/Product
```

**Response (200):**
```json
[
  {
    "id": 1,
    "name": "Laptop",
    "price": 999.99,
    "stock": 10,
    "deliveryTimeInDays": 3,
    "category": "Electronics",
    "categoryId": 1,
    "imageUrl": "https://...",
    "discount": 50.00,
    "rating": 4.5,
    "reviewsCount": 12
  }
]
```

---

#### Search Products
```http
GET /api/Product/search?query=laptop
```

**Response (200):**
```json
[
  { /* product */ }
]
```

---

#### Get Product by ID
```http
GET /api/Product/{id}
```

**Response (200):**
```json
{
  "id": 1,
  "name": "Laptop",
  /* ... other fields ... */
}
```

**Errors (404):**
```json
{
  "message": "Product not found."
}
```

---

#### Create Product (Seller Only)
```http
POST /api/Product
Authorization: Bearer <token>
Content-Type: application/json

{
  "name": "Laptop",
  "price": 999.99,
  "stock": 10,
  "categoryId": 1,
  "deliveryTimeInDays": 3,
  "discount": 50.00,
  "imageUrl": "https://..."
}
```

**Response (200):**
```json
{
  "id": 1,
  "name": "Laptop",
  /* ... full product ... */
}
```

**Errors:**
- `401` - Unauthorized (not seller)
- `400` - Invalid data

---

#### Update Product (Seller Only)
```http
PUT /api/Product/{id}
Authorization: Bearer <token>
Content-Type: application/json

{
  "name": "Laptop Pro",
  "price": 1299.99,
  /* ... other fields ... */
}
```

**Response (200):**
```json
{
  "id": 1,
  "name": "Laptop Pro",
  /* ... updated product ... */
}
```

**Errors:**
- `400` - "You can only update your own products."
- `404` - Product not found

---

#### Delete Product (Seller Only)
```http
DELETE /api/Product/{id}
Authorization: Bearer <token>
```

**Response (200):**
```json
{
  "message": "Product deleted successfully."
}
```

**Errors:**
- `400` - "You can only delete your own products."
- `404` - Product not found

---

### 3. Categories

#### Get All Categories
```http
GET /api/Categories
```

**Response (200):**
```json
[
  {
    "id": 1,
    "name": "Electronics"
  },
  {
    "id": 2,
    "name": "Clothing"
  }
]
```

---

#### Create Category (Seller Only)
```http
POST /api/Categories
Authorization: Bearer <token>
Content-Type: application/json

{
  "name": "New Category"
}
```

**Response (200):**
```json
{
  "id": 3,
  "name": "New Category",
  "message": "Category created successfully."
}
```

---

### 4. Orders

#### Create Order (Buyer Only)
```http
POST /api/Order
Authorization: Bearer <token>
Content-Type: application/json

{
  "items": [
    {
      "productId": 1,
      "quantity": 2
    },
    {
      "productId": 3,
      "quantity": 1
    }
  ]
}
```

**Response (200):**
```json
{
  "id": 1,
  "status": "Pending",
  "createdAt": "2024-01-15T10:30:00Z",
  "totalPrice": 2149.98,
  "items": [
    {
      "productId": 1,
      "productName": "Laptop",
      "quantity": 2,
      "price": 999.99
    }
  ]
}
```

**Errors:**
- `400` - Insufficient stock
- `401` - Not authorized

---

#### Get My Orders (Buyer Only)
```http
GET /api/Order/my
Authorization: Bearer <token>
```

**Response (200):**
```json
[
  {
    "id": 1,
    "status": "Pending",
    "createdAt": "2024-01-15T10:30:00Z",
    "totalPrice": 2149.98,
    "items": [
      /* ... order items ... */
    ]
  }
]
```

---

#### Get Seller Orders (Seller Only)
```http
GET /api/Order/seller
Authorization: Bearer <token>
```

**Response (200):**
```json
[
  {
    "id": 1,
    "status": "Processing",
    /* ... order details ... */
  }
]
```

---

#### Update Order Status (Seller Only)
```http
PUT /api/Order/{id}/status
Authorization: Bearer <token>
Content-Type: application/json

{
  "status": "Shipped"
}
```

**Valid Statuses:**
- `Pending`
- `Processing`
- `Shipped`
- `Delivered`
- `Cancelled`

**Response (200):**
```json
{
  "message": "Order status updated."
}
```

**Errors:**
- `400` - "You can only update orders containing your products."
- `404` - Order not found

---

#### Add Order Comment (Buyer Only)
```http
POST /api/Order/{id}/comments
Authorization: Bearer <token>
Content-Type: application/json

{
  "text": "Please ship ASAP"
}
```

**Response (200):**
```json
{
  "id": 1,
  "text": "Please ship ASAP",
  "createdAt": "2024-01-15T10:30:00Z"
}
```

---

#### Get Order Comments
```http
GET /api/Order/{id}/comments
```

**Response (200):**
```json
[
  {
    "id": 1,
    "text": "Please ship ASAP",
    "createdAt": "2024-01-15T10:30:00Z"
  }
]
```

---

### 5. Reviews

#### Add Review (Buyer Only)
```http
POST /api/Review
Authorization: Bearer <token>
Content-Type: application/json

{
  "productId": 1,
  "rating": 5,
  "comment": "Excellent product!"
}
```

**Response (200):**
```json
{
  "id": 1,
  "productId": 1,
  "rating": 5,
  "comment": "Excellent product!",
  "createdAt": "2024-01-15T10:30:00Z"
}
```

**Errors:**
- `400` - Rating must be 1-5
- `400` - "You have already reviewed this product."

---

#### Get Product Reviews
```http
GET /api/Review/product/{productId}
```

**Response (200):**
```json
[
  {
    "id": 1,
    "rating": 5,
    "comment": "Great!",
    "createdAt": "2024-01-15T10:30:00Z"
  }
]
```

---

#### Get Review Summary
```http
GET /api/Review/summary/{productId}
```

**Response (200):**
```json
{
  "totalReviews": 15,
  "averageRating": 4.73
}
```

---

### 6. Flags

#### Flag Seller (Buyer Only)
```http
POST /api/Flag/seller
Authorization: Bearer <token>
Content-Type: application/json

{
  "sellerId": 2,
  "reason": "Fraudulent product"
}
```

**Response (200):**
```json
{
  "message": "Seller flagged successfully."
}
```

---

#### Flag Buyer (Seller Only)
```http
POST /api/Flag/buyer
Authorization: Bearer <token>
Content-Type: application/json

{
  "buyerId": 1,
  "reason": "Harassment via comments"
}
```

**Response (200):**
```json
{
  "message": "Buyer flagged successfully."
}
```

---

## ?? Response Format

### Success Response
```json
{
  "data": { /* response body */ }
}
```
or direct object/array

### Error Response
```json
{
  "message": "Error description"
}
```

### Array Responses
- **Empty arrays always return** `[]` (never `null`)

---

## ?? Error Handling

### HTTP Status Codes

| Status | Usage |
|--------|-------|
| `200` | Success |
| `400` | Bad Request (validation error) |
| `401` | Unauthorized (invalid token) |
| `403` | Forbidden (insufficient role) |
| `404` | Not Found |
| `500` | Server Error |

### Error Response Format
```json
{
  "message": "Specific error message"
}
```

---

## ??? Role-Based Access

### Buyer (Role = 0)
- ? Create orders
- ? View own orders
- ? Add order comments
- ? Create reviews
- ? Flag sellers

### Seller (Role = 1)
- ? Create/Update/Delete own products
- ? View seller orders
- ? Update order status
- ? Create categories
- ? Flag buyers

### Anonymous (No Token)
- ? Get all products
- ? Search products
- ? Get categories
- ? View reviews
- ? View order comments

---

## ?? Integration Notes

### Request Headers
```http
Content-Type: application/json
Authorization: Bearer <jwt-token>
```

### CORS Policy
- **Allowed Origins**: `http://localhost:3000`
- **Allowed Methods**: GET, POST, PUT, DELETE, OPTIONS
- **Allowed Headers**: Any

### Database
- **Engine**: SQL Server
- **Connection**: `DefaultConnection` (appsettings.json)
- **Migrations**: Database-First (DO NOT regenerate)

---

## ?? Deployment Checklist

- [ ] Update `Jwt:Key` in production appsettings
- [ ] Set `ASPNETCORE_ENVIRONMENT=Production`
- [ ] Update `DefaultConnection` to production database
- [ ] Update CORS `WithOrigins` to frontend domain
- [ ] Enable HTTPS redirection
- [ ] Disable Swagger in production (`app.Environment.IsProduction()`)
- [ ] Set up SQL Server backups
- [ ] Configure JWT key as environment variable

---

## ?? Example: Complete Workflow

### 1. Register
```bash
curl -X POST http://localhost:5213/api/Auth/register \
  -H "Content-Type: application/json" \
  -d '{"name":"Jane","email":"jane@test.com","password":"Pass123","role":"buyer"}'
```

### 2. Login
```bash
curl -X POST http://localhost:5213/api/Auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"jane@test.com","password":"Pass123"}'
```

### 3. Browse Products
```bash
curl http://localhost:5213/api/Product
```

### 4. Create Order (with token)
```bash
curl -X POST http://localhost:5213/api/Order \
  -H "Authorization: Bearer <token>" \
  -H "Content-Type: application/json" \
  -d '{"items":[{"productId":1,"quantity":2}]}'
```

### 5. Review Product
```bash
curl -X POST http://localhost:5213/api/Review \
  -H "Authorization: Bearer <token>" \
  -H "Content-Type: application/json" \
  -d '{"productId":1,"rating":5,"comment":"Great product!"}'
```

---

## ?? Support

For API issues, check:
1. **Swagger UI**: `http://localhost:5213/swagger`
2. **Build Errors**: `dotnet build`
3. **Database**: Verify connection string
4. **Token**: Ensure JWT key matches across all instances

---

**Version**: 1.0  
**Last Updated**: 2024  
**Status**: Production Ready ?
