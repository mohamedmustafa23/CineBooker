# 🎬 CineMax - Advanced Cinema Booking System

**CineMax** is a comprehensive, full-stack web application designed to manage cinema operations and facilitate online ticket booking. Built with **ASP.NET Core MVC**, this project demonstrates a robust backend architecture capable of handling complex booking logic, concurrency, and real-time seat management.

> **Note:** This project focuses primarily on **Backend Engineering** and **System Architecture**. The User Interface (UI) and Frontend design were generated using AI tools to ensure a modern look while allowing the development effort to concentrate on server-side logic, database optimization, and security.

---

## 🚀 Key Backend Features

### 1. Robust Booking Engine
*   **Seat Locking Mechanism:** Implemented a concurrency-safe system where seats are temporarily "Locked" for 10 minutes during the booking process to prevent double-booking.
*   **Dynamic Seat Availability:** Real-time validation of seat status (Available, Locked, Booked) using server-side logic.
*   **Complex Validation:** Prevents overlapping showtimes within the same hall and validates booking constraints.

### 2. Architecture & Design Patterns
*   **Repository Pattern:** Decoupled business logic from data access using Generic Repositories (`IRepository<T>`) to ensure clean code and testability.
*   **ViewModel Pattern:** Used ViewModels (`VMs`) extensively to separate Domain Models from View data, ensuring security and proper data shaping.
*   **Dependency Injection (DI):** Heavy use of DI for managing services, repositories, and managers.

### 3. Identity & Security
*   **ASP.NET Core Identity:** Fully integrated authentication system.
*   **Role-Based Authorization:** Distinct access levels for **Admins** (Dashboard, Management) and **Customers** (Booking, History).

### 4. Admin Dashboard & Management
*   **Data Visualization:** Backend logic aggregates data for charts (Revenue, Occupancy rates, Daily bookings).
*   **CRUD Operations:** Full management for Movies, Cinemas, Halls, and Shows.

### 5. Advanced Data Handling
*   **AJAX Integration:** efficient API endpoints within controllers to serve JSON data for dynamic UI elements (e.g., changing dates, loading seat maps) without page reloads.
*   **LINQ & EF Core:** Complex queries with `Include`, `GroupBy`, and aggregations to generate reports and filter movies.

---

## 🛠️ Tech Stack

*   **Framework:** .NET 8 (ASP.NET Core MVC)
*   **Language:** C#
*   **Database:** SQL Server
*   **ORM:** Entity Framework Core (Code-First Approach)
*   **Authentication:** ASP.NET Core Identity
*   **Frontend:** HTML5, CSS3, Bootstrap 5, JavaScript (AI-Assisted Design)

---

## 📂 Database Schema Overview

![Database Schema](ScreenShots/Mermaid%20Chart%20-%20Create%20complex,%20visual%20diagrams%20with%20text.-2025-12-11-194308.png)


---

## ⚙️ Getting Started

To run this project locally:

1.  **Clone the repository:**
    ```bash
    git clone https://github.com/YourUsername/CineMax.git
    ```
2.  **Configure Database:**
    Update the connection string in `appsettings.json` to point to your local SQL Server instance.
3.  **Apply Migrations:**
    Open Package Manager Console and run:
    ```powershell
    update-database
    ```
4.  **Run the Application:**
    ```bash
    dotnet run
    ```

---

## 📸 Screenshots

![Database Schema](ScreenShots/Screenshot%202025-12-11%20212845.png)
![Database Schema](ScreenShots/Screenshot%202025-12-11%20212913.png)
![Database Schema](ScreenShots/Screenshot%202025-12-11%20212927.png)
![Database Schema](ScreenShots/Screenshot%202025-12-11%20213000.png)
![Database Schema](ScreenShots/Screenshot%202025-12-11%20213019.png)
![Database Schema](ScreenShots/Screenshot%202025-12-11%20213113.png)

---

## 👨‍💻 Developer

Developed by **[Mohamed Mustafa]**.
*Focusing on building scalable, secure, and efficient backend solutions with .NET.*
