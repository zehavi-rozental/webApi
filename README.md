# Ice Cream Management System - Web API

A professional, multi-layered .NET Web API designed for managing ice cream inventories with real-time synchronization and asynchronous logging. This project demonstrates modern backend architecture, security best practices, and real-time communication.

## 🚀 Key Features

* **Clean Architecture:** Separated into Core, Infrastructure, and WebAPI layers for high maintainability.
* **Real-Time Synchronization:** Integrated **SignalR** to sync data across multiple devices/tabs for the same user.
* **Asynchronous Logging:** High-performance logging using **Serilog** with a background worker pattern, including file rotation (50MB limit).
* **Hybrid Authentication:** Supports standard **JWT-based** authentication and **Google OAuth** integration.
* **Role-Based Access Control (RBAC):** Strict separation between Administrator and Regular User permissions.
* **Optimized Grid Updates:** Frontend grid refreshes only via SignalR notifications to ensure data consistency across active connections.

## 🛠 Tech Stack

* **Backend:** ASP.NET Core 8.0 (Targeting 2026 Standards)
* **Real-Time:** SignalR Hubs
* **Logging:** Serilog (File Sink with Rotation)
* **Storage:** JSON-based persistence (Interface-driven for easy DB migration)
* **Security:** JWT Bearer & Google Authentication

## 📂 Project Structure

* **Core:** Contains Domain Models, Interfaces, and Business Logic.
* **Infrastructure:** Implementation of Data Access (JSON), Logging services, and external integrations.
* **WebApi:** Controllers, Middlewares, SignalR Hubs, and Dependency Injection configurations.

## 📋 API Documentation

### Item Management
| Method | URL | Auth | Description |
| :--- | :--- | :--- | :--- |
| GET | `/api/item` | User/Admin | Get all items for the logged-in user |
| POST | `/api/item` | User/Admin | Add a new item |
| PUT | `/api/item/{id}` | User/Admin | Update an existing item |
| DELETE | `/api/item/{id}` | User/Admin | Delete an item |

### User Management
| Method | URL | Auth | Description |
| :--- | :--- | :--- | :--- |
| GET | `/api/user/me` | User/Admin | Get current user profile |
| GET | `/api/user` | Admin | List all registered users |
| DELETE | `/api/user/{id}` | Admin | Delete user and their associated items |

## ⚙️ Setup & Configuration

1.  **Clone the repository.**
2.  **Configuration:** Update `appsettings.json` for custom log paths and Google Client IDs.
3.  **Run:** Execute `dotnet run` from the WebApi directory.
4.  **Logging:** Logs are automatically generated in the configured path with a 50MB rotation policy.

## 🌟 Quality Assurance
This project adheres to the highest coding standards, utilizing Scoped services for active user tracking and Extension methods for clean `IServiceCollection` registration.

---
*Developed as a professional grade assignment - 2026.*
