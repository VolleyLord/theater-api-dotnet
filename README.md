### 🎭 TheaterAPI

An ASP.NET Core 8 Web API for managing a theater: halls, spectacles, tickets, users, and admin operations. Built with Entity Framework Core (PostgreSQL), secured with JWT, and documented via Swagger.

---

### ✨ Features
- **Authentication & Authorization 🔐**: JWT-based auth with role-based endpoints (admin/user)
- **Entity Framework Core 🐘**: PostgreSQL provider, code-first migrations
- **Swagger/OpenAPI 📚**: Interactive docs at `/swagger`
- **Background Jobs ⏱️**: Cleanup and unblock workers (`ReservationCleanupWorker`, `SpectacleCleanupWorker`, `UserUnblockWorker`)
- **Auditing 🧾**: Action history tracking for key operations

---

### 🧱 Project Structure
```
Controllers/
  AdminController.cs        # Admin operations
  AuthController.cs         # Login / token
  HallsController.cs        # Halls CRUD
  SpectaclesController.cs   # Spectacles CRUD & stats
  TicketsController.cs      # Ticketing & reservations
  UserController.cs         # User profile & management
  UserVisitsController.cs   # Visit logging
Data/
  AppDbContext.cs
Migrations/                 # EF Core migrations
Models/                     # Entities & DTOs
Services/                   # Business logic & background workers
Program.cs                  # Host & middleware setup
```

---

### 🚀 Quickstart
1) **Prerequisites**
- .NET SDK 8.x
- PostgreSQL 14+ (or compatible)

2) **Clone & Configure**
```
git clone https://github.com/VolleyLord/theater-api-dotnet.git
cd theater-api-dotnet
```

Create `appsettings.json` (or `appsettings.Development.json`) using the template below. These files are intentionally git-ignored.

```json
{
  "ConnectionStrings": {
    "Default": "Host=localhost;Port=5432;Database=theaterdb;Username=user;Password=yourpassword"
  },
  "Jwt": {
    "Issuer": "TheaterAPI",
    "Audience": "TheaterAPI.Clients",
    "Key": "replace-with-a-long-random-secret-key",
    "TokenLifetimeMinutes": 120
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

3) **Restore, Migrate, Run**
```
dotnet restore
dotnet ef database update
dotnet run
```

The API will start (by default on `http://localhost:5197` or similar). Visit `http://localhost:<port>/swagger` for interactive docs.

---

### 🔑 Authentication
- Obtain a JWT via `AuthController` (e.g., `/api/auth/login`).
- Send the token in authorization window.
- Admin-only endpoints are guarded by role checks.

---

### 🧪 Testing with the provided HTTP file
Use `TheaterAPI.http` (VS Code/Visual Studio REST client) to quickly test endpoints locally.

---

### 🗃️ Database & Migrations
- Create a new migration:
```
dotnet ef migrations add <MigrationName>
```
- Apply migrations:
```
dotnet ef database update
```

Ensure your `ConnectionStrings:Default` is valid before applying migrations.

---

### 📡 API Surface (high level)
- `AuthController` — login, token issuance
- `AdminController` — administrative actions (e.g., blocking/unblocking users, audits)
- `HallsController` — CRUD for halls
- `SpectaclesController` — CRUD, scheduling, and statistics
- `TicketsController` — ticket reservation, purchase, cleanup
- `UserController` — user management, password updates
- `UserVisitsController` — logging and viewing user visits

See Swagger for full request/response schemas.

---

### ⚙️ Configuration Notes
- `appsettings.json` and `appsettings.*.json` are ignored by git. 
- Important keys:
  - `ConnectionStrings:Default`
  - `Jwt:Issuer`, `Jwt:Audience`, `Jwt:Key`, `Jwt:TokenLifetimeMinutes`

---
