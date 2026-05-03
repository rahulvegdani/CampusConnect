# CampusConnect 🎓🛒

CampusConnect is a campus-based student marketplace platform where students can buy and sell products within their college community.

The platform helps students connect, communicate, and trade products in a trusted campus environment.

---

## Project Overview

CampusConnect is designed to simplify campus buying and selling.

Students can:

* Sell products
* Buy products
* Add products to wishlist
* Chat with sellers in real time
* Receive instant notifications
* Manage their profiles

This creates a safe and organized student marketplace.

---

## Features

### User Features

✅ User Registration
✅ User Login
✅ Profile Management
✅ Seller Profile View
✅ Buyer Profile View
✅ Wishlist Management
✅ Real-time Chat
✅ Notification System

---

### Product Features

✅ Add Product
✅ Edit Product
✅ Delete Product
✅ Browse Products
✅ Search Products
✅ Filter by Category
✅ Product Status Tracking

---

### Admin Features

✅ Admin Dashboard
✅ Product Approval
✅ User Management
✅ Category Management
✅ Pending Product Management
✅ Rejected Product Management
✅ Sold Product Management

---

## Real-time Features

* Real-time messaging using SignalR
* Chat message notifications
* Wishlist sold product notifications

---

## Technology Stack

### Backend

* ASP.NET Core MVC
* Entity Framework Core

### Frontend

* HTML
* CSS
* Bootstrap
* JavaScript

### Database

* SQL Server

### Authentication

* ASP.NET Identity

### Real-time Communication

* SignalR

---

## System Workflow

1. User registers
2. User logs in
3. User completes profile
4. User explores marketplace
5. User lists products for selling
6. Buyer searches products
7. Buyer adds products to wishlist
8. Buyer chats with seller
9. Notifications are triggered
10. Product gets sold

---

## Project Structure

```text id="mjlwmc"
CampusConnect/
├── Controllers/
├── Models/
├── Views/
├── Data/
├── Services/
├── Hubs/
├── Helpers/
├── Migrations/
├── wwwroot/
├── screenshots/
├── Program.cs
├── CampusConnect.csproj
├── README.md
└── .gitignore
```

---

## Project Screenshots

### Home Page

![Home Page](./screenshots/Home-Page.png)

### Login Page

![Login Page](./screenshots/Login-Page.png)

### Register Page

![Register Page](./screenshots/Register-Page.png)

### Market Page

![Market Page](./screenshots/Market-Page.png)

### Add Product Page

![Add Product Page](./screenshots/AddProduct-Page.png)

### My Profile Page

![My Profile Page](./screenshots/MyProfile-Page.png)

### Profile Review Page

![Profile Review Page](./screenshots/MyProfile-Review.png)

### My Product Page

![My Product Page](./screenshots/MyProduct-Page.png)

### Chat Page

![Chat Page](./screenshots/Message-Page.png)

### Wishlist Page

![Wishlist Page](./screenshots/WishList-Page.png)

### Notification Page

![Notification Page](./screenshots/Notification-Page.png)

### Admin Dashboard Page

![Admin Dashboard](./screenshots/Admin-Dashboard-Page.png)

### Admin All Product Page

![Admin All Product](./screenshots/Admin-AllProduct-Page.png)

### Admin Pending Product Page

![Admin Pending Product](./screenshots/Admin-PendingProduct-Page.png)

### Admin Rejected Product Page

![Admin Rejected Product](./screenshots/Admin-RejectedProduct-Page.png)

### Admin Sold Product Page

![Admin Sold Product](./screenshots/Admin-SoldProduct-Page.png)

### Admin User Management Page

![Admin User View](./screenshots/Admin-AllUserView-Page.png)

### Admin Category Management Page

![Admin Category Management](./screenshots/Admin-CategoryManagement-Page.png)

---

## Installation

Clone the repository:

```bash id="dclbks"
git clone https://github.com/rahulvegdani/CampusConnect.git
```

Open the project in Visual Studio.

Configure SQL Server connection string in:

```text id="isdbdn"
appsettings.json
```

Run database migration:

```powershell id="97gks0"
Update-Database
```

Run project:

```text id="s9m70f"
Ctrl + F5
```

---

## Future Enhancements

* AI-based product recommendations
* Mobile application
* Online payment integration
* Review and rating system

---

## Author

Rahul Vegdani

---

## Status

🚀 Active Development
