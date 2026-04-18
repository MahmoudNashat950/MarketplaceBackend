# ?? Quick Start Guide - Marketplace Backend

## Prerequisites

- .NET 8 SDK
- SQL Server (LocalDB or Express)
- Visual Studio 2022 or VS Code
- Postman or similar API testing tool

---

## ? Getting Started (5 minutes)

### 1. Clone & Open Project
```bash
git clone <repository>
cd Marketplace.API
```

### 2. Update Database Connection (if needed)
Edit `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=MarketplaceDB;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

### 3. Run the Application
```bash
dotnet run
```

**Output:**
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5213
      Now listening on: https://localhost:7283
```

### 4. Access Swagger UI
Open browser: **http://localhost:5213/swagger**

---

## ?? Test Endpoints (Postman)

### 1. Register a Buyer
```
POST http://localhost:5213/api/Auth/register
Content-Type: application/json

{
  "name": "John Buyer",
  "email": "buyer@test.com",
  "password": "Password123",
  "role": "buyer"
}
```

**Response:**
```json
{
  "message": "Registration successful."
}
```

### 2. Register a Seller
```
POST http://localhost:5213/api/Auth/register
Content-Type: application/json

{
  "name": "Jane Seller",
  "email": "seller@test.com",
  "password": "Password123",
  "role": "seller"
}
```

### 3. Login
```
POST http://localhost:5213/api/Auth/login
Content-Type: application/json

{
  "email": "buyer@test.com",
  "password": "Password123"
}
```

**Response:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "user": {
    "id": 1,
    "name": "John Buyer",
    "email": "buyer@test.com",
    "role": "buyer"
  }
}
```

**Copy the token for next requests!**

### 4. Get All Products
```
GET http://localhost:5213/api/Product
```

**Response:**
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
    "imageUrl": null,
    "discount": null,
    "rating": 0,
    "reviewsCount": 0
  }
]
```

### 5. Create Product (Seller)
```
POST http://localhost:5213/api/Product
Authorization: Bearer <your_seller_token>
Content-Type: application/json

{
  "name": "Gaming Monitor",
  "price": 599.99,
  "stock": 5,
  "categoryId": 1,
  "deliveryTimeInDays": 2,
  "discount": 50,
  "imageUrl": "https://example.com/monitor.jpg"
}
```

### 6. Create Order (Buyer)
```
POST http://localhost:5213/api/Order
Authorization: Bearer <your_buyer_token>
Content-Type: application/json

{
  "items": [
    {
      "productId": 1,
      "quantity": 2
    }
  ]
}
```

**Response:**
```json
{
  "id": 1,
  "status": "Pending",
  "createdAt": "2024-01-15T10:30:00Z",
  "totalPrice": 1999.98,
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

### 7. View My Orders (Buyer)
```
GET http://localhost:5213/api/Order/my
Authorization: Bearer <your_buyer_token>
```

### 8. Add Review (Buyer)
```
POST http://localhost:5213/api/Review
Authorization: Bearer <your_buyer_token>
Content-Type: application/json

{
  "productId": 1,
  "rating": 5,
  "comment": "Excellent product!"
}
```

### 9. Get Review Summary
```
GET http://localhost:5213/api/Review/summary/1
```

---

## ?? Authentication Tips

### Get Your Token
1. Login and copy the `token` from response
2. In Postman: Click "Authorization" tab
3. Select "Bearer Token"
4. Paste token in the "Token" field
5. All requests will include: `Authorization: Bearer <token>`

### Token Expiration
- Tokens expire after **7 days**
- After expiration, login again to get new token

### Common Auth Errors
```json
{
  "message": "Invalid credentials."
}
```
? Check email and password

```json
{
  "message": "Email already in use."
}
```
? Use different email

---

## ?? Database Structure

### Users Table
```
Id (PK)
Name
Email (Unique)
PasswordHash (Hashed with BCrypt)
Role (0=Buyer, 1=Seller)
```

### Products Table
```
Id (PK)
Name
Price
Stock
SellerId (FK)
CategoryId (FK)
DeliveryTimeInDays
Discount (nullable)
ImageUrl (nullable)
```

### Orders Table
```
Id (PK)
BuyerId (FK)
Status (0=Pending, 1=Processing, 2=Shipped, 3=Delivered, 4=Cancelled)
CreatedAt
```

### Ratings Table
```
Id (PK)
ProductId (FK)
UserId (FK)
Value (1-5)
CreatedAt
UpdatedAt
Unique: (ProductId, UserId)
```

---

## ?? Troubleshooting

### Error: "Address already in use"
```bash
# Kill process using port 5213
netstat -ano | findstr :5213
taskkill /PID <pid> /F
```

### Error: "Cannot connect to database"
1. Check SQL Server is running
2. Verify connection string in `appsettings.json`
3. Try `Server=localhost` instead of `.`

### Error: "401 Unauthorized"
1. Make sure token is copied correctly
2. Check token hasn't expired (7 days)
3. Verify "Authorization" header is set
4. Check "Bearer " prefix (with space)

### Error: "403 Forbidden"
You tried to access a seller-only endpoint as a buyer (or vice versa)
- Check your user role in login response
- Use correct token

### Error: "Duplicate review"
Buyer tried to review same product twice
```bash
# Clear old reviews:
DELETE FROM Ratings WHERE ProductId = 1 AND UserId = 1
```

---

## ?? Project Structure Reference

```
Marketplace.API/
??? Controllers/           # HTTP endpoints
?   ??? AuthController.cs
?   ??? ProductController.cs
?   ??? OrderController.cs
?   ??? ReviewController.cs
?   ??? CategoriesController.cs
?   ??? FlagController.cs
??? Services/             # Business logic
?   ??? AuthService.cs
?   ??? ProductService.cs
?   ??? ... (5 more services)
??? Interfaces/           # Service contracts
??? DTOs/                 # Data transfer objects
??? Enums/                # UserRole, OrderStatus
??? Middleware/           # ExceptionHandling
??? Utils/                # JsonConverters
??? Program.cs            # Configuration & DI
??? appsettings.json      # App config
```

---

## ?? Common Workflows

### Workflow 1: Seller Creates & Sells Product

```
1. Register as Seller
   POST /api/Auth/register with role="seller"

2. Create Category (Optional)
   POST /api/Categories (with token)

3. List Categories
   GET /api/Categories

4. Create Product
   POST /api/Product with token
   Include: name, price, stock, categoryId, etc.

5. View Created Product
   GET /api/Product/{id}

6. View All Products with Ratings
   GET /api/Product
```

### Workflow 2: Buyer Purchases & Reviews

```
1. Register as Buyer
   POST /api/Auth/register with role="buyer"

2. Browse Products
   GET /api/Product

3. Search Products
   GET /api/Product/search?query=laptop

4. Get Product Details
   GET /api/Product/{id}

5. View Reviews & Summary
   GET /api/Review/product/{productId}
   GET /api/Review/summary/{productId}

6. Create Order
   POST /api/Order with items and token

7. View My Orders
   GET /api/Order/my

8. Add Comment to Order
   POST /api/Order/{orderId}/comments

9. Add Review (After receiving)
   POST /api/Review with productId, rating, comment

10. Flag Seller (If needed)
    POST /api/Flag/seller
```

### Workflow 3: Seller Fulfills Order

```
1. Login as Seller
   POST /api/Auth/login

2. View Seller Orders
   GET /api/Order/seller

3. Update Order Status
   PUT /api/Order/{orderId}/status
   Status: Pending ? Processing ? Shipped ? Delivered

4. View Order Comments
   GET /api/Order/{orderId}/comments
```

---

## ?? Security Features You Should Know

? **Passwords are hashed** with BCrypt (not stored in plain text)  
? **JWT tokens** are signed and can't be tampered with  
? **Sellers can only edit** their own products  
? **Buyers can only review** each product once  
? **Role-based access** ensures proper permissions  

---

## ?? API Response Examples

### Successful Product Creation
```json
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
  "rating": 0.0,
  "reviewsCount": 0
}
```

### Error Response
```json
{
  "message": "You can only update your own products."
}
```

### Empty List (Not Null!)
```json
[]
```

---

## ?? Pro Tips

1. **Use Swagger UI** for interactive API testing
   - Go to http://localhost:5213/swagger
   - Click "Authorize" button
   - Paste your JWT token
   - Try endpoints directly from UI

2. **Save endpoints in Postman Collections**
   - Create folder structure: Auth, Products, Orders, etc.
   - Save commonly used requests
   - Reuse variables: `{{base_url}}`, `{{token}}`

3. **Test with Different Roles**
   - Create 1 buyer account + 1 seller account
   - Keep tokens for both
   - Switch tokens to test role-based access

4. **Monitor Database**
   - Use SQL Server Management Studio
   - Check tables: Users, Products, Orders, Ratings
   - Verify stock changes after orders

---

## ?? Next Steps

After getting started:

1. **Read full API documentation**: `API_DOCUMENTATION.md`
2. **Review architecture**: `ARCHITECTURE.md`
3. **Check implementation**: `IMPLEMENTATION_CHECKLIST.md`
4. **Explore code**: Start with `AuthService.cs`
5. **Connect frontend**: Update API base URL in your React app

---

## ?? Support

### Build Errors?
```bash
# Clean build
dotnet clean
dotnet build
```

### Need to reset database?
```bash
# Drop and recreate (careful!)
UPDATE [MarketplaceDbContext] SET NOCHECK
DELETE FROM Ratings
DELETE FROM Products
DELETE FROM Orders
DELETE FROM Users
DELETE FROM Categories
```

### Check logs?
Look at console output or Event Viewer (Windows) for error details

---

**You're ready to go! ??**

Next: Connect this API to your React frontend and start building!

For more details, see **API_DOCUMENTATION.md** and **ARCHITECTURE.md**.
