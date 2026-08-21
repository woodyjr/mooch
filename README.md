# Mooch

Mooch is a social fitness app for dogs: part social network, part activity tracker, part challenge platform.

This repository is intentionally set up as a learning-friendly monorepo:

- `apps/web` contains the React + TypeScript frontend
- `apps/api` contains the ASP.NET Core API
- `docs/architecture` contains ADRs and architecture decisions
- `docs/getting-started` contains contribution and learning notes

## MVP Focus

The first milestone is intentionally small and clean:

- account creation and login
- dog profile management
- manual activity logging
- simple friend/follow relationships
- a lightweight activity feed

The goal is to stay DRY and understandable rather than prematurely broad.

## Getting Started

Before running the app locally, provide your own local config values:

- `apps/web/.env`
  - `VITE_GOOGLE_CLIENT_ID=your-google-client-id.apps.googleusercontent.com`
- environment variables for API / Docker when needed
  - `GOOGLE_CLIENT_ID`
  - `MOOCH_CONNECTION_STRING`
  - optional for EF tooling: `MOOCH_DESIGNTIME_CONNECTION`

### Frontend

```powershell
cd apps/web
npm install
npm run dev
```

### Backend

```powershell
cd apps/api
dotnet restore
dotnet ef database update
dotnet run
```

### PostgreSQL

You can run a local PostgreSQL instance with Docker:

```powershell
docker compose up -d
```

The compose files now use environment-variable placeholders with safe sample defaults. Override them locally instead of committing personal values.

## Suggested Build Order

1. Run the database.
2. Run the API and inspect the Swagger endpoints.
3. Run the React app and walk through the current routes.
4. Implement one vertical slice at a time, starting with dogs.

## Learning Notes

Start here if you want to keep contributing rather than just reviewing generated code:

- [Foundation stack ADR](./docs/architecture/ADR-001-foundation-stack.md)
- [Contribution guide](./docs/getting-started/contribution-guide.md)
