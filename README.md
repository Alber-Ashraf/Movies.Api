# CineMovies API (`Movies.Api`)

Enterprise RESTful backend for the **CineMovies** platform, built with **ASP.NET Core 8 Minimal APIs**, **Dapper**, and **PostgreSQL**. Provides movie catalog management, official synopses and YouTube trailer links, community ratings, role-based access control (RBAC), output caching, API versioning, and automated database schema initialization and seeding.

- **Live Backend API**: [https://cinemovies.runasp.net](https://cinemovies.runasp.net)
- **Live Frontend App**: [https://cinemovies.tryasp.net](https://cinemovies.tryasp.net)

---

## Key Features

- **Minimal API Architecture**: High-performance endpoint routing organized by domain (`Movies`, `Ratings`, `Identity`) with API Versioning (`v1.0` & `v2.0`).
- **Authentication & Role-Based Authorization (RBAC)**:
  - Cryptographic password hashing via **PBKDF2 (`Rfc2898DeriveBytes`, SHA-256, 100,000 iterations, 16-byte salt)** and constant-time verification (`CryptographicOperations.FixedTimeEquals`).
  - **JWT Bearer** authentication (`HMACSHA256`) and dual **Admin API Key** (`x-api-key`) authentication.
  - Three hierarchical roles:
    - `Admin`: Full CRUD on movies, catalog seeding (`POST /api/movies/seed`), and ratings.
    - `TrustedMember` (Critic): Create/Update movies and manage personal ratings.
    - `Member`: Rate movies (`1–5` stars) and manage personal ratings history.
- **Zero-Migration Database Bootstrapping**:
  - `DbInitializer` idempotently creates and evolves PostgreSQL tables (`movies`, `genres`, `ratings`, `users`) and indexes on startup.
  - `DataSeeder` automatically seeds 3 enterprise role accounts, 24 acclaimed films with official plot synopses and YouTube trailers, and initial community ratings.
- **Server-Side Filtering, Sorting, & Pagination**:
  - Filter by `title`, `genre`, and `year`.
  - Sort ascending (`+`) or descending (`-`) by `title` or `yearofrelease`.
  - Tag-based **Output Caching** (`MovieCache`) with automatic cache eviction on mutations.
- **Validation & Resilience**:
  - **FluentValidation** request and domain validators with structured `ValidationFailureResponse` (`400 Bad Request`) middleware.
  - Database health check endpoint (`GET /_health`).
  - Typed **Refit SDK** (`Movies.Api.Sdk`) for strongly-typed .NET API consumers.

---

## Tech Stack

| Layer | Technology |
| :--- | :--- |
| **Runtime & Framework** | .NET 8, ASP.NET Core Minimal APIs |
| **Data Access** | Dapper, Npgsql (PostgreSQL) |
| **Authentication** | JWT Bearer (`Microsoft.AspNetCore.Authentication.JwtBearer`), PBKDF2 SHA-256 |
| **Validation** | FluentValidation |
| **API Versioning & Docs** | `Asp.Versioning.Http`, Swashbuckle / Swagger (OpenAPI) |
| **Caching** | ASP.NET Core Output Caching |
| **Client SDK** | Refit (`Movies.Api.Sdk`) |

---

## Solution Structure

```text
Movies.Api.sln
├── Movies.Api                          # Presentation & HTTP layer (Minimal APIs)
│   ├── Auth/                           # JWT & API Key handlers, auth constants
│   ├── Endpoints/
│   │   ├── Identity/                   # Register, Login, Current User (/me), Token endpoints
│   │   ├── Movies/                     # Movie CRUD, Genres list, and Admin Seeder endpoints
│   │   └── Ratings/                    # Rate movie, delete rating, and user ratings endpoints
│   ├── Health/                         # PostgreSQL database health check
│   ├── Mapping/                        # Contract <-> Domain mapping extensions + ValidationMiddleware
│   ├── Swagger/                        # Multi-version OpenAPI configuration
│   ├── Validators/                     # Auth request FluentValidation rules
│   ├── ApiEndpoints.cs                 # Canonical route templates
│   └── Program.cs                      # DI, CORS, security headers, pipeline & DB startup
│
├── Movies.Application                  # Domain & Application layer
│   ├── Auth/                           # PBKDF2 PasswordHasher
│   ├── DataBase/                       # NpgsqlConnectionFactory, DbInitializer, DataSeeder
│   ├── Models/                         # Movie, MovieRating, AppUser, GetAllMoviesOptions
│   ├── Repositories/                   # Dapper repositories (Movie, Rating, User)
│   ├── Services/                       # MovieService, RatingService business orchestration
│   └── Validators/                     # Movie & GetAllMoviesOptions validators
│
├── Movies.Contracts                    # Wire DTOs (Requests & Responses)
│   ├── Requests/                       # CreateMovie, UpdateMovie, RateMovie, Login, Register
│   └── Responses/                      # MovieResponse, MoviesResponse, AuthResponse, UserResponse
│
├── Movies.Api.Sdk                      # Refit client interface (IMoviesApi)
└── Movies.Api.Sdk.Consumer             # Sample console consumer demonstrating SDK usage
```

---

## API Endpoints Overview

### Identity & Authentication
| Method | Route | Auth | Description |
| :--- | :--- | :--- | :--- |
| `POST` | `/api/identity/register` | Public | Register a new `Member` account and receive a JWT |
| `POST` | `/api/identity/login` | Public | Authenticate with email & password and receive a JWT |
| `GET` | `/api/identity/me` | Bearer JWT | Retrieve current authenticated user profile & claims |
| `POST` | `/token` | Public | Legacy token generator endpoint for SDK testing |

### Movies & Genres
| Method | Route | Auth | Description |
| :--- | :--- | :--- | :--- |
| `GET` | `/api/movies` | Public (Optional JWT) | List movies with filtering, sorting, pagination & user ratings |
| `GET` | `/api/movies/{idOrSlug}` | Public (Optional JWT) | Get movie details by UUID or canonical slug |
| `GET` | `/api/genres` | Public | Get distinct sorted list of all movie genres |
| `POST` | `/api/movies` | `TrustedMember` / `Admin` | Create a new movie with synopsis, trailer URL, and genres |
| `PUT` | `/api/movies/{id:guid}` | `TrustedMember` / `Admin` | Update movie details, synopsis, trailer URL, and genres |
| `DELETE` | `/api/movies/{id:guid}` | `Admin` | Delete a movie and its associated ratings |
| `POST` | `/api/movies/seed` | `Admin` | Trigger idempotent seeding of 24 films & sample ratings |

### Ratings
| Method | Route | Auth | Description |
| :--- | :--- | :--- | :--- |
| `PUT` | `/api/movies/{id:guid}/ratings` | Authenticated | Rate a movie (`1` to `5` stars) |
| `DELETE` | `/api/movies/{id:guid}/ratings` | Authenticated | Remove current user's rating for a movie |
| `GET` | `/api/ratings/me` | Authenticated | List all ratings submitted by the current user |

---

## Seeded Demo Accounts

On startup, `DataSeeder` provisions three accounts for testing RBAC permissions:

| Role | Email | Password | Capabilities |
| :--- | :--- | :--- | :--- |
| **Admin** | `admin@cinemovies.io` | `Admin123!` | Create, edit, delete movies, trigger catalog seeder, rate films |
| **Trusted Critic** | `critic@cinemovies.io` | `Critic123!` | Create and edit movies, rate films |
| **Member** | `member@cinemovies.io` | `Member123!` | Rate films, manage personal rating history |

---

## Getting Started Locally

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [PostgreSQL 14+](https://www.postgresql.org/download/) (local instance or Docker container)

### 1. Start PostgreSQL (Optional Docker Command)
```bash
docker run --name cinemovies-db -e POSTGRES_USER=postgres -e POSTGRES_PASSWORD=postgres -e POSTGRES_DB=movies -p 5432:5432 -d postgres:16-alpine
```

### 2. Configure Application Settings
Configure your PostgreSQL connection string and JWT signing key in `Movies.Api/appsettings.Development.json` or via User Secrets:

```json
{
  "Database": {
    "ConnectionString": "Host=localhost;Port=5432;Database=movies;Username=postgres;Password=postgres"
  },
  "Jwt": {
    "Key": "PleaseProvideASecretKeyWithAtLeast32Characters!",
    "Issuer": "https://id.alber.com",
    "Audience": "https://movies.alber.com"
  },
  "Cors": {
    "AllowedOrigins": [
      "http://localhost:4200",
      "https://cinemovies.tryasp.net"
    ]
  }
}
```

### 3. Build and Run
```bash
dotnet build Movies.Api.sln
dotnet run --project Movies.Api/Movies.Api.csproj
```

On startup, the API automatically initializes the database schema, seeds the 24 movies and demo accounts, and serves Swagger UI in development mode at `https://localhost:7149/swagger` (or `http://localhost:5211/swagger`).

---

## Author

Developed by **Alber Ashraf**
