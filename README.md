# 🎬 Movies.Api

A simple RESTful API built with **ASP.NET Core Web API** to manage movies, genres, and user authentication.  
This project is part of my backend learning journey with a focus on clean architecture and practical API design.

---

## 📸 Preview

> You can test the API endpoints using Swagger UI  
> `https://localhost:{port}/swagger`

![image](https://github.com/user-attachments/assets/75fd7e62-ae4e-4397-9328-7d1765156894)

---

## 🧠 Features

- 🔐 JWT Authentication & Authorization
- 📁 CRUD operations for Movies and Genres
- 👤 User Registration & Login
- 🧼 Clean code structure with separation of concerns
- 🧪 Swagger for API documentation and testing

---

## 🧰 Technologies Used

- ✅ ASP.NET Core Web API
- ✅ Entity Framework Core
- ✅ SQL Server
- ✅ Swagger (Swashbuckle)
- ✅ AutoMapper
- ✅ JWT Authentication
- ✅ Repository Pattern

---

## 🚀 Getting Started

### 1️⃣ Clone the Repository
```bash
git clone https://github.com/Alber-Ashraf/Movies.Api.git
cd Movies.Api
```
### 2️⃣ Update the Database
Make sure you have SQL Server running.
Configure the connection string in appsettings.json: 
```bash
"ConnectionStrings": {
  "DefaultConnection": "Server=.;Database=MoviesApiDb;Trusted_Connection=True;MultipleActiveResultSets=true"
}
```
Apply migrations and update the database:
```bash
dotnet ef database update
```
### 3️⃣ Run the Application
```bash
dotnet run
```
Then navigate to:
```bash
https://localhost:{port}/swagger
```
---

## 🛡️ Authentication
Use the /api/Auth/register and /api/Auth/login endpoints to register or log in.
Once logged in, copy the JWT token and add it to Swagger via Authorize button for protected routes.

---

## 📁 Folder Structure
```
📦 Movies.Api.sln
├── 📂 Movies.Api                                            # Main ASP.NET Core Web API project
│   ├── 📂 Auth                                              # JWT authentication logic
│   ├── 📂 Controllers                                       # API Controllers (Movies, Auth, etc.)
│   ├── 📂 Endpoints                                         # Endpoint group definitions for minimal APIs
│   ├── 📂 Health                                            # Health check endpoints and setup
│   ├── 📂 Mapping                                           # AutoMapper profiles and configurations
│   ├── 📂 Swagger                                           # Swagger documentation and customization
│   ├── 📂 Properties                                        # LaunchSettings for local dev
│   ├── ApiEndpoints.cs                                     # Defines static API route strings
│   ├── Movies.Api.csproj                                   # Project file
│   ├── Movies.Api.http                                     # HTTP request collection for testing APIs
│   ├── Program.cs                                          # Entry point & configuration
│   ├── appsettings.json                                    # App configuration
│   └── appsettings.Development.json                        # Dev-specific configuration

├── 📂 Movies.Application                                    # Application logic layer
│   ├── 📂 DataBase                                          # DB context, migrations, and seeding
│   ├── 📂 Models                                            # Entity models (Movie, Genre, etc.)
│   ├── 📂 Repositories                                      # Repository interfaces and implementations
│   ├── 📂 Services                                          # Business logic & domain services
│   ├── 📂 Validators                                        # FluentValidation rules for requests
│   ├── ApplicationServiceCollectionExtensions.cs           # Service registration
│   ├── IApplicationMarker.cs                               # Marker interface for assembly scanning
│   └── Movies.Application.csproj                           # Project file

├── 📂 Movies.Contracts                                      # DTOs for requests & responses
│   ├── 📂 Requests                                          # Request models used by the API
│   ├── 📂 Responses                                         # Response models returned from API
│   └── Movies.Contracts.csproj                             # Project file

├── 📂 Movies.Api.Sdk                                       # Interface definitions for consumers
│   ├── ApiEndpoints.cs                                     # Static endpoint URIs for SDK
│   ├── IMoviesApi.cs                                       # Interface defining API client methods
│   └── Movies.Api.Sdk.csproj                               # Project file

├── 📂 Movies.Api.Sdk.Consumer                               # SDK implementation for consuming Movies.Api
│   ├── AuthHeaderHandler.cs                                # Adds Authorization header to HTTP client
│   ├── AuthTokenProvider.cs                                # Provides tokens for authenticated requests
│   ├── Movies.Api.Sdk.Consumer.csproj # Project file
│   └── Program.cs                                          # Sample app for using the SDK

├── .gitattributes                                          # Git settings for line endings etc.
├── .gitignore                                              # Files/folders to ignore in git
└── Movies.Api.sln                                           # Visual Studio solution file
```
---

## 💡 Author
Developed with ❤️ by Alber Ashraf
