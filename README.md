# Full-Stack ToDo Management System

A professional Task Management application featuring a secure authentication system, personalized user environments, and a modern React-based interface. This project demonstrates a clean separation of concerns between a high-performance .NET 9 Minimal API and a dynamic React frontend.

## Tech Stack

### Backend (Server)
* Framework: .NET 9.0 Minimal API for streamlined, high-performance routing.
* Database: MySQL integrated via Pomelo Entity Framework Core.
* Security:
    * JWT (JSON Web Tokens): Secure, stateless authentication for API access.
    * BCrypt.Net: Industry-standard password hashing for user data protection.
* Documentation: Swagger UI for interactive API testing and documentation.

### Frontend (Client)
* Library: React.js for a responsive, component-based UI.
* HTTP Client: Axios with custom Interceptors to automatically handle JWT injection and 401 (Unauthorized) session expiration.
* State Management: Modern React Hooks (useState, useEffect) for real-time UI updates.
* Authentication Logic: jwt-decode for client-side token parsing and user session management.

---

## Key Features
* Secure User Onboarding: Full Register/Login flow with encrypted passwords and token-based sessions.
* Personalized Data: Users can only view, create, or modify tasks linked to their unique User ID.
* Global Error Handling: Intelligent Axios interceptors that manage token persistence and logout on session expiration.
* API Standardization: Fully documented RESTful endpoints following standard HTTP methods (GET, POST, PUT, DELETE).

---

## Project Structure
* ToDoApi/ – The backend service containing the API logic, database context, and authentication middleware.
* ToDoListReact/ – The frontend application containing React components and the API service layer.

---

## Setup and Installation

### 1. Prerequisites
* .NET 9 SDK
* Node.js & npm
* MySQL Server

### 2. Database Configuration & Migrations
1. Open ToDoApi/appsettings.json and update the ToDoDB connection string with your local MySQL credentials.
2. Install the EF Core CLI tools:
   dotnet tool install --global dotnet-ef
3. Apply migrations to create the database schema:
   cd ToDoApi
   dotnet ef database update

### 3. Running the Backend
From the ToDoApi directory:
dotnet restore
dotnet run
The server will start at http://localhost:5142. API documentation is available at /swagger.

### 4. Running the Frontend
From the ToDoListReact directory:
npm install
npm start
The application will launch at http://localhost:3000.

---

## API Reference

All task-related endpoints require a valid JWT Token in the Authorization header.

| Method | Endpoint | Description | Auth Required |
| :--- | :--- | :--- | :--- |
| POST | /register | Create a new user account | No |
| POST | /login | Authenticate and receive JWT | No |
| GET | /items | Retrieve tasks for the current user | Yes |
| POST | /items | Add a new task linked to user | Yes |
| PUT | /items/{id} | Update task status or name | Yes |
| DELETE | /items/{id} | Remove a specific task | Yes |

---

Developed by Sara Volpo
