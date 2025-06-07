# User Data Application

A full-stack application for managing and displaying user data with filtering and pagination capabilities.

## Tech Stack

### Backend
- .NET 7 Web API
- Parquet file handling
- In-memory caching
- CORS enabled

### Frontend
- React with TypeScript
- Vite
- Material-UI
- React Query
- React Select

## Setup

### Backend
1. Navigate to `UserDataApp.API` directory
2. Update `appsettings.json` with your Parquet file path
3. Run:
```bash
dotnet restore
dotnet run
```

### Frontend
1. Navigate to `userdataapp-frontend-vite` directory
2. Install dependencies:
```bash
npm install
```
3. Start development server:
```bash
npm run dev
```

## Features
- User data display with pagination
- Advanced filtering (country, gender, salary range, dates)
- Responsive design
- Real-time search
- Data caching 