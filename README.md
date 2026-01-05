# Online Courses Platform

Technical Assessment implementation using .NET 8 (Clean Architecture), PostgreSQL, and Vue.js (Vite + Tailwind).

## Features
- **Clean Architecture**: Domain, Application, Infrastructure, Api.
- **Docker Support**: Full stack orchestration with Docker Compose.
- **Admin Seeding**: Automated admin user creation on startup.
- **Course Management**: CRUD, Publish/Unpublish, Draft/Published states.
- **Lesson Management**: Add, Remove, Reorder (Drag & Drop UI simulation).
- **Soft Delete**: Entities are soft deleted.
- **Authentication**: JWT based auth.

## Prerequisites
- Docker & Docker Compose
- .NET 8 SDK (Optional, for local dev)
- Node.js (Optional, for local dev)

## Quick Start (Docker)

1. Open a terminal in the root directory.
2. Run:
   ```bash
   docker-compose up --build
   ```
3. Wait for the containers to start. The Database will initialize, API will start, and Frontend will build.
4. Access the application at: `http://localhost:3000`

### Default Admin Credentials
The system automatically seeds an Admin user if it doesn't exist.
- **Email**: `admin@example.com`
- **Password**: `AdminPassword123!`

## Project Structure
- `backend/`: .NET 8 Solution
    - `OnlineCourses.Domain`: Entities, Enums, Repo Interfaces.
    - `OnlineCourses.Application`: Use Cases, DTOs, Services (Business Logic).
    - `OnlineCourses.Infrastructure`: EF Core, Identity, Repositories Implementation.
    - `OnlineCourses.Api`: Controllers, Swagger, Auth Config.
    - `OnlineCourses.UnitTests`: xUnit tests for Business Rules.
- `frontend/`: Vue 3 + Vite application.

## Running Locally (Manual)

### Backend
1. Navigate to `backend/`.
2. Update `appsettings.json` connection string to point to a local PostgreSQL instance.
3. **Important**: Run the following command to create the initial migration (since it wasn't pre-generated):
   ```bash
   dotnet ef migrations add InitialCreate --project OnlineCourses.Infrastructure --startup-project OnlineCourses.Api
   ```
4. Run:
   ```bash
   dotnet restore
   dotnet run --project OnlineCourses.Api
   ```
   API will run on `http://localhost:8080` (or configured port).

### Frontend
1. Navigate to `frontend/`.
2. Install dependencies:
   ```bash
   npm install
   ```
3. Run dev server:
   ```bash
   npm run dev
   ```
   Access at `http://localhost:5173`.
   *Note: Ensure `vite.config.js` proxy points to your local API port.*

## Testing
To run unit tests:
```bash
cd backend
dotnet test
```
Tests cover:
- Publishing rules (Must have lessons).
- Lesson ordering (Must be unique).
- Soft delete verification.
