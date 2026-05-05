# 🛒 Marketplace Backend API

A scalable and clean architecture backend for a Marketplace system built with ASP.NET Core Web API.  
It supports authentication, product management, reviews, AI-powered review summaries, and admin operations.

---

## 🚀 Features

### 🔐 Authentication & Authorization
- JWT-based authentication
- User registration & login
- Role-based access (User / Admin)

### 🛍️ Marketplace Core
- Product management (CRUD)
- Category management
- Order handling

### ⭐ Reviews System
- Add product reviews
- Rating system (1–5 stars)
- Prevent duplicate reviews per user
- Fetch reviews per product

### 🤖 AI Integration
- AI-powered review summary using OpenAI
- Smart insights from customer feedback
- Graceful fallback when AI is unavailable

### 🧑‍💼 Admin Features
- Ban / unban users
- Manage reports
- Monitor platform activity

---

## 🏗️ Architecture

- Clean Architecture principles
- Separation of concerns:
  - API Layer
  - Application Layer
  - Domain Layer
  - Infrastructure Layer

---

## 🧰 Tech Stack

- ASP.NET Core Web API
- Entity Framework Core
- SQL Server
- JWT Authentication
- OpenAI API (AI features)
- Dependency Injection
- LINQ

---

## ⚙️ Setup Instructions

### 1. Clone Repository
```bash
git clone https://github.com/your-username/MarketplaceBackend.git
cd MarketplaceBackend
