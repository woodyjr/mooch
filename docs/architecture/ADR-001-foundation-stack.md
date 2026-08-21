# ADR-001: Foundation Stack

- Status: Accepted
- Date: 2026-08-14

## Context

Mooch is a social fitness app for dogs. A user creates an account, creates one or more dogs, connects activity sources, follows friends, and eventually joins dog exercise challenges.

The project goals are twofold:

1. Build a real product with room to grow.
2. Use the app as a portfolio project that demonstrates practical full-stack skills.

The initial stack under consideration is:

- TypeScript
- React
- C#
- ASP.NET Core
- Entity Framework Core
- PostgreSQL
- REST APIs
- Authentication
- Relational database design

## Product Assumptions

These assumptions drive the recommendation:

- v1 is web-first.
- v1 needs standard account creation, dog profiles, a social graph, activity ingestion, and an activity feed.
- Strava is a realistic early integration.
- Apple Health is desirable, but not required for v1 launch.
- The app should stay small enough for a portfolio project while still showing thoughtful architecture.

## Decision

Adopt the proposed stack with one important scope adjustment:

- Use `TypeScript + React` for the frontend.
- Use `C# + ASP.NET Core` for the backend API.
- Use `Entity Framework Core` with the `Npgsql` provider.
- Use `PostgreSQL` as the primary relational database.
- Use `REST` for the public application API in v1.
- Use app-managed authentication, starting with ASP.NET Core Identity backed by PostgreSQL.
- Treat `Strava` as the first external fitness integration.
- Defer `Apple Health` to a later mobile-native phase instead of making it a launch dependency.

## Why This Fits Mooch

### TypeScript + React

This is a strong fit for a feed-driven social product:

- React is well suited for profile pages, activity feeds, settings, challenges, and incremental UI growth.
- TypeScript adds a visible layer of engineering rigor that is valuable in a portfolio project.
- Typed frontend models pair well with a typed backend and reduce contract drift.

### C# + ASP.NET Core

This is a strong fit for a portfolio app that needs auth, APIs, and integrations:

- ASP.NET Core is mature for authentication, validation, middleware, background jobs, and API design.
- C# gives you strong domain modeling for concepts like `User`, `Dog`, `Friendship`, `Activity`, and `Challenge`.
- The ecosystem is good for production-style backend structure without forcing microservice complexity.

### Entity Framework Core

EF Core is a good default here because:

- your domain is relational by nature
- it keeps the learning curve manageable while still teaching migrations, mapping, querying, and aggregate boundaries
- it integrates cleanly with ASP.NET Core and PostgreSQL

Use raw SQL only for targeted hotspots later if needed.

### PostgreSQL

PostgreSQL is a particularly good fit for this app:

- core data is relational: users, dogs, friendships, memberships, and challenges
- it is reliable and widely respected in hiring contexts
- it gives you room for advanced features later such as `jsonb`, full-text search, and geospatial extensions if you add route or map features

### REST APIs

REST is the right default for v1:

- it is simple to explain in a portfolio
- it matches the resource model of the app well
- it works cleanly with React, mobile clients, and third-party integrations

GraphQL is not necessary early on. Use REST first and add realtime features separately only if the product needs them.

### Authentication

Authentication belongs in the stack from day one because it affects almost every feature:

- user accounts
- dog ownership
- friend relationships
- privacy boundaries
- connected external accounts

For a first version, the most practical choice is:

- ASP.NET Core Identity
- PostgreSQL storage
- secure cookie-based auth if the frontend and backend are served together or from the same top-level app

If the app later becomes multi-client or mobile-heavy, reassess token-based auth or a dedicated identity provider.

## Weighted Evaluation

Scoring scale:

- 1 = poor fit
- 3 = acceptable
- 5 = strong fit

| Criterion | Weight | Stack Score | Notes |
| --- | ---: | ---: | --- |
| Portfolio value | 5 | 5 | Shows real frontend, backend, auth, data modeling, and integration work. |
| Fit for social product workflows | 5 | 5 | Feed, profiles, friendships, and challenges map cleanly to the stack. |
| Development speed for one developer | 4 | 4 | Productive, but still substantial enough to learn from. |
| Long-term extensibility | 4 | 4 | Good room for mobile clients, background jobs, and richer data features. |
| Integration friendliness | 4 | 4 | Strong for Strava and general OAuth-based APIs. |
| Operational complexity | 3 | 3 | More moving parts than a single-framework full-stack app, but still manageable. |
| Apple Health readiness | 2 | 2 | Weak for web-only v1 because HealthKit is native-app oriented. |
| Total | 27 | 111 / 135 | Strong overall recommendation. |

## Key Caveat: Apple Health vs. Strava

This is the one place where the original plan needs adjustment.

### Strava fits the proposed stack well

Strava provides OAuth-based authentication and a REST API, which lines up naturally with:

- React initiating connect flows
- ASP.NET Core handling OAuth callbacks and token storage
- background sync jobs importing activity data

### Apple Health does not fit a web-first v1 in the same way

Apple Health access is built around HealthKit, which is part of Apple's native app platform. In practice, that means:

- it is not a normal server-to-server web integration like Strava
- it should be treated as an iOS-native capability
- a pure React web app should not depend on it for initial launch scope

Decision:

- `Strava` is in scope for v1.
- `Apple Health` is a phase-2 or phase-3 feature that likely requires a native iOS client or companion app.

## Recommended v1 Architecture

### Frontend

- React
- TypeScript
- React Router
- a data fetching library such as TanStack Query

### Backend

- ASP.NET Core Web API
- controller-based REST endpoints for clarity and portfolio readability
- FluentValidation or built-in validation
- ASP.NET Core Identity
- EF Core with migrations

Note:
Microsoft currently recommends Minimal APIs for many new HTTP APIs, but controller-based APIs are still a perfectly valid choice here and may read more clearly in a portfolio that emphasizes explicit resource structure and auth boundaries.

### Database

Start relational-first with tables such as:

- `users`
- `dogs`
- `user_dogs`
- `friendships`
- `connected_accounts`
- `activities`
- `activity_sources`
- `challenges`
- `challenge_participants`

This gives you a strong base for demonstrating relational design rather than forcing everything into document-style storage.

## Suggested Delivery Phases

### Phase 1

- user registration and login
- create and edit dog profiles
- manual activity logging
- basic friend/follow system
- personal feed

### Phase 2

- Strava connect
- activity import pipeline
- normalized external account and source tracking
- challenge creation and participation

### Phase 3

- Apple Health support through a native mobile strategy
- richer challenge mechanics
- notifications
- route, map, or leaderboard features

## Consequences

Positive:

- The stack is credible, practical, and portfolio-friendly.
- It shows strong full-stack breadth without being trendy for its own sake.
- It leaves room for richer product ideas later.

Costs:

- You will manage two main codebases, frontend and backend.
- Authentication and integration work will add meaningful complexity.
- Apple Health should not be promised too early if the app begins as web-only.

## Alternatives Considered

### React + Node/NestJS + PostgreSQL

A good alternative, especially for all-JavaScript development, but weaker if your goal is explicitly to showcase C# and ASP.NET Core.

### Next.js full-stack

Faster for a simpler social app MVP, but it would reduce your direct exposure to a dedicated C# API and EF Core, which are part of your stated learning goals.

### React + ASP.NET Core + SQL Server

Also viable, but PostgreSQL is a better portfolio fit for modern product-style apps and gives you stronger flexibility for future data features.

## Implementation Guidance for Future Decisions

Unless a later ADR replaces this decision:

- prefer relational modeling over premature document storage
- prefer REST over GraphQL
- keep auth first-party and simple in v1
- integrate Strava before Apple Health
- treat Apple Health as a mobile-native concern

## Sources

- Strava authentication docs: https://developers.strava.com/docs/authentication/
- Strava getting started docs: https://developers.strava.com/docs/getting-started/
- Strava rate limit docs: https://developers.strava.com/docs/rate-limits/
- ASP.NET Core authentication overview: https://learn.microsoft.com/en-us/aspnet/core/security/authentication/?view=aspnetcore-10.0
- ASP.NET Core API overview: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/apis?view=aspnetcore-10.0
- Npgsql EF Core provider docs: https://www.npgsql.org/efcore/
- PostgreSQL overview: https://www.postgresql.org/about/
- PostgreSQL JSON types docs: https://www.postgresql.org/docs/16/datatype-json.html
- Apple HealthKit overview: https://developer.apple.com/documentation/healthkit
- Apple HealthKit setup: https://developer.apple.com/documentation/healthkit/setting-up-healthkit
- Apple Health authorization: https://developer.apple.com/documentation/healthkit/authorizing-access-to-health-data
