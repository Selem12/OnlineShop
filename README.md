# AuthApp - React + ASP.NET Core (.NET 10) + PostgreSQL

A secure authentication system featuring Argon2id password hashing, session-based authentication, and email/phone verification.

## 🛠️ Tech Stack
- **Frontend**: React (Vite), Axios, React Router
- **Backend**: ASP.NET Core WebAPI (.NET 10), EF Core
- **Database**: PostgreSQL
- **Security**: Argon2id, Cookie-based Sessions

## 🚀 Getting Started

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js (LTS)](https://nodejs.org/)
- [PostgreSQL](https://www.postgresql.org/download/)

### 1. Database Setup
1. Create a PostgreSQL database named `authdb`.
2. Update the connection string in `backend/AuthApi/appsettings.json`:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Host=localhost;Database=authdb;Username=YOUR_USER;Password=YOUR_PASSWORD"
   }
   ```

### 2. Backend Setup
```bash
# Navigate to backend
cd backend/AuthApi

# Install EF Core tools (if not installed)
dotnet tool install --global dotnet-ef

# Apply migrations to create tables
dotnet ef database update

# Run the API
dotnet run
```
The API will typically run at `http://localhost:5000` or `https://localhost:5001`.

### 3. Frontend Setup
```bash
# Navigate to frontend
cd frontend

# Install dependencies
npm install

# Run development server
npm run dev
```
The frontend will typically run at `http://localhost:5173`.

## 🔐 Authentication Flow
1. **Register**: Creates a user and logs a 6-digit verification code to the **backend console**.
2. **Verify**: Enter the code from the console into the frontend to verify email/phone.
3. **Login**: Creates a short-lived `HttpOnly` session cookie.
4. **Authorization**: `ProtectedRoute` guards prevent access to the dashboard until email is verified.

## 🛡️ Security Features
- **Argon2id**: Passwords are hashed using Argon2id (Memory: 64MB, Iterations: 3, Parallelism: 4).
- **Session Management**: Short-lived sliding expiration (15m idle / 1h absolute).
- **Cookie Security**: `HttpOnly`, `Secure`, and `SameSite=Strict` flags enabled.
