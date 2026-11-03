# FU NEWS MANAGEMENT SYSTEM - ASSIGNMENT 02 UPGRADE
**Course:** PRN232 - Advanced .NET Programming  
**Student Identifier:** `DangMinhHieu_SE1933_NET`  
**Baseline Reference:** `16_DangMinhHieu_Assignment1_PRN232`  

---

## 1. PROJECT ARCHITECTURE OVERVIEW

The system is developed using the **3-Tier Architecture (Layered Pattern)** organized under a master solution `16_DangMinhHieu_Assignment2.sln` containing **2 dedicated C# Projects**:

```
16_DangMinhHieu_Assignment2_PRN232/
├── 16_DangMinhHieu_Assignment2.sln      # Master Solution (Chứa trực tiếp 2 Project BE & FE)
│
├── DangMinhHieu_SE1933_NET_A02_BE/      # Back-End Project (Web API + OData + JWT + 3-Tier Layered)
│   ├── DangMinhHieu_SE1933_NET_A02_BE.csproj
│   ├── DangMinhHieu_SE1933_NET_A02_BE.sln
│   ├── Controllers/                  # Presentation Layer (API Controllers & Endpoints)
│   ├── Services/                     # Business Logic Layer (Services & Logic)
│   ├── DTOs/                         # Data Transfer Objects
│   ├── Repositories/                 # Repository Pattern (IRepository & Implementations)
│   ├── DAOs/                         # Singleton Data Access Objects
│   ├── Entities/                     # EF Core DbContext & Models
│   ├── ODataQueryOptionsFilter.cs    # OData Swagger Filter
│   ├── Program.cs                    # Dependency Injection & Middleware
│   └── appsettings.json              # Connection String & JWT Config
│
└── DangMinhHieu_SE1933_NET_A02_FE/      # Front-End Project (ASP.NET Core MVC Client)
    ├── DangMinhHieu_SE1933_NET_A02_FE.csproj
    ├── DangMinhHieu_SE1933_NET_A02_FE.sln
    ├── Controllers/                  # MVC Controllers (Auth, News, Categories, Accounts, Profile)
    ├── Filters/                      # ApiExceptionFilter (Handles 401/403 redirects)
    ├── Models/                       # ViewModels
    ├── Services/                     # IApiService & HttpClient with JWT injection
    ├── Views/                        # Razor Views with Modals & SweetAlert2
    ├── wwwroot/                      # Static assets (Bootstrap, SweetAlert2, CSS, JS)
    ├── Program.cs                    # MVC Pipeline, Session, Cookie Auth
    └── appsettings.json              # API Base URL Config
```

### Key Design Patterns & Technical Upgrades
- **Repository Pattern & Singleton Pattern**:
  - All database queries strictly flow through thread-safe Singleton DAOs (`CategoryDAO`, `NewsArticleDAO`, `SystemAccountDAO`, `TagDAO`) and their respective Repositories (`CategoryRepository`, `NewsArticleRepository`, `SystemAccountRepository`, `TagRepository`).
  - Controllers in `FUNewsManagement_API` never communicate directly with `DbContext`.
- **JWT Authentication & Role-Based Authorization**:
  - `POST /api/auth/login`: Authenticates either Admin (from `appsettings.json`) or Staff/Lecturer (from database `SystemAccount`).
  - Generates signed JWT tokens with claims (`NameIdentifier`, `Email`, `Name`, `Role`).
  - API endpoints are protected using `[Authorize(Roles = "Admin")]` and `[Authorize(Roles = "Staff")]`.
- **ASP.NET Core OData Integration**:
  - Fully configured OData Entity Data Model (EDM) on route prefix `odata`.
  - Supports standard OData query options: `$filter`, `$select`, `$orderby`, `$expand`, `$top`, `$skip`, `$count`.
  - Applied `[EnableQuery]` on `NewsArticles`, `Categories`, and `SystemAccounts`.
- **Front-End Client with JWT Token Injection & Exception Filter**:
  - `IApiService` automatically retrieves the JWT token from the user session and attaches `Authorization: Bearer <token>` to all HTTP requests.
  - Custom `ApiExceptionFilter` catches HTTP 401 (`Unauthorized`) and redirects to `/Account/Login`; catches HTTP 403 (`Forbidden`) and redirects to `/Account/AccessDenied`.
  - **Create/Update via Modal Popups**: All forms open inside sleek Bootstrap 5 modal dialogs without full page reloads.
  - **Delete via Confirmation Dialogs**: SweetAlert2 confirmation dialogs prompt the user before deletion, enforcing business integrity rules (cannot delete Category with articles, cannot delete Account with authored articles).
  - **Admin Report & Statistics**: Filter articles by `StartDate` and `EndDate` (based on `CreatedDate`), ordered descending with KPI summary cards and CSV/PDF export.

---

## 2. SYSTEM ACCOUNTS & CREDENTIALS

| Role | Account Email | Password | Source / Access Rights |
| :--- | :--- | :--- | :--- |
| **Admin** | `admin@FUNewsManagementSystem.org` | `@@abc123@@` | Configured in `appsettings.json`. Full management of Accounts, Categories, News Articles, and Period Reports. |
| **Staff** | `staff@fpt.edu.vn` | `@1` | Database (`SystemAccount` Role = 1). Manage Articles (with Tags), Categories, Profile, and Personal Article History. |
| **Lecturer** | `lecturer@fpt.edu.vn` | `@1` | Database (`SystemAccount` Role = 2). View articles and manage personal profile. |
| **Guest** | *No login required* | *None* | Access Home feed to read published articles (`NewsStatus == true`). |

*(Note: Quick-fill buttons are provided on the Login page for rapid grading!)*

---

## 3. HOW TO RUN THE PROJECT

### Prerequisites
- .NET 8.0 SDK or higher
- Microsoft SQL Server (LocalDB or SQL Server Express)
- Database `FUNewsManagement` initialized (use database script from Assignment 1)

### Connection String Configuration
Back-End connection string is configured in `DangMinhHieu_SE1933_NET_A02_BE/appsettings.json`:
```json
"ConnectionStrings": {
  "DefaultConnectionString": "Server=(local);Database=FUNewsManagement;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True;"
}
```

### Running via Command Line (Terminal / PowerShell)

**Step 1: Start Back-End Web API (Port 5173)**
```powershell
cd DangMinhHieu_SE1933_NET_A02_BE
dotnet run
```
- API Base URL: `http://localhost:5173`
- Swagger UI Documentation: `http://localhost:5173/swagger`
- OData Metadata Endpoint: `http://localhost:5173/odata/$metadata`

**Step 2: Start Front-End Client (Port 5076)**
Open a second terminal window:
```powershell
cd DangMinhHieu_SE1933_NET_A02_FE
dotnet run
```
- Front-End URL: `http://localhost:5076`

---

## 4. ASSIGNMENT 02 FEATURES CHECKLIST

- [x] **2 Independent Solutions**: `DangMinhHieu_SE1933_NET_A02_BE.sln` and `DangMinhHieu_SE1933_NET_A02_FE.sln`
- [x] **3-Tier Architecture**: DAL (EF Core + Singleton DAOs + Repositories) -> BLL (Services + DTOs) -> API / Client
- [x] **JWT Authentication**: Token generation with SecretKey, Issuer, Audience, ExpireTime; claims-based authorization
- [x] **OData Support**: EDM model builder, query options (`$filter`, `$orderby`, `$select`, `$expand`, `$top`, `$skip`)
- [x] **Client Token Injection**: `HttpClient` Bearer token header handling and session storage
- [x] **Client 401/403 Handling**: Automatic redirect to `/Account/Login` and `/Account/AccessDenied`
- [x] **Modal Dialogs (Popups)**: Create/Update with Bootstrap 5 modals; Delete with SweetAlert2 confirmation
- [x] **Referential Integrity Constraints**: Prevent deletion of Category with articles; prevent deletion of Account with articles
- [x] **Admin Statistics & Reports**: Date-range filtering by `CreatedDate`, sorted descending, summary metric cards, CSV export
