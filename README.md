# 🎫 Ticket Management API

<p align="center">
  <strong>A clean ASP.NET Core Web API for managing support tickets.</strong>
</p>

<p align="center">
  <img src="https://img.shields.io/badge/.NET-ASP.NET%20Core-512BD4?style=for-the-badge&logo=dotnet" />
  <img src="https://img.shields.io/badge/API-REST-02569B?style=for-the-badge" />
  <img src="https://img.shields.io/badge/Status-Training%20Project-orange?style=for-the-badge" />
  <img src="https://img.shields.io/badge/License-MIT-green?style=for-the-badge" />
</p>

---

## 📌 Context

This project was created as a training API to practice backend development with **ASP.NET Core Web API** in a professional context.

The main goal is to understand how to build a clean, structured, and maintainable REST API while applying good backend development habits.

---

## 🧾 Summary

**Ticket Management API** allows users to manage support tickets through REST endpoints.

A ticket can represent a technical issue, a user request, or an internal task that needs to be tracked.

---

## 🎯 What This API Is For

This API can be used to:

| Feature            | Description                   |
| ------------------ | ----------------------------- |
| 🎫 Create tickets  | Add a new support ticket      |
| 📋 List tickets    | Retrieve all existing tickets |
| 🔎 Get details     | Retrieve a ticket by ID       |
| ✏️ Update tickets  | Modify ticket information     |
| 🔄 Update status   | Change the ticket status      |
| 🗑️ Delete tickets | Remove or close a ticket      |

---

## 🚀 Getting Started

### Prerequisites

Before running the project, make sure you have:

* [.NET SDK](https://dotnet.microsoft.com/) installed
* A code editor such as **Visual Studio**, **Rider**, or **Visual Studio Code**
* A database configured if persistence is enabled

---

### Installation

Clone the repository:

```bash
git clone <repository-url>
```

Move into the project folder:

```bash
cd TicketManagementApi
```

Restore dependencies:

```bash
dotnet restore
```

Run the API:

```bash
dotnet run
```

---

## 📚 API Documentation

Once the API is running, Swagger should be available at:

```text
/swagger
```

Swagger allows you to explore and test the API directly from the browser.

---

## 🧪 Example Usage

### Create a ticket

```http
POST /tickets
```

Request body:

```json
{
  "title": "Cannot access my account",
  "description": "The user cannot log in with valid credentials.",
  "priority": "High"
}
```

Example response:

```json
{
  "id": 1,
  "title": "Cannot access my account",
  "description": "The user cannot log in with valid credentials.",
  "status": "Open",
  "priority": "High",
  "createdAt": "2026-07-08T10:30:00Z"
}
```

---

## 🏗️ Architecture

The project follows a simple layered architecture.

```text
TicketManagementApi
│
├── Controllers
│   └── TicketsController
│
├── Services
│   └── TicketService
│
├── DTOs
│   ├── CreateTicketRequest
│   ├── UpdateTicketRequest
│   └── TicketResponse
│
├── Models
│   └── Ticket
│
├── Enums
│   ├── TicketStatus
│   └── TicketPriority
│
└── Program.cs
```

---

## 🧱 Project Layers

| Layer           | Responsibility                                  |
| --------------- | ----------------------------------------------- |
| **Controllers** | Handle HTTP requests and responses              |
| **Services**    | Contain business logic                          |
| **DTOs**        | Define data exchanged with the API              |
| **Models**      | Represent internal data structures              |
| **Enums**       | Define fixed values such as status and priority |
| **Data**        | Handle database access and persistence          |

---

## 👤 Author

Created by **3thanole** as a backend development training project.

---

## 📄 License

This project is licensed under the **MIT License**.
