# Contribution Guide

This scaffold is meant to help you learn by building with it, not around it.

## How To Work With This Repo

Use a vertical-slice mindset:

1. pick one user-facing feature
2. model the data
3. add the API contract
4. add the backend endpoint
5. connect the frontend
6. test the slice end-to-end

For MVP, prefer adding small slices over building framework-heavy abstractions.

## Good First Tasks

These are intentionally left as learning-friendly follow-ups:

1. Replace the mocked dog list in the frontend with live API calls.
2. Add `GET /api/dogs/{id}`.
3. Add validation rules for dog creation.
4. Add user registration and login with ASP.NET Core Identity.
5. Add EF Core migrations and create the initial schema locally.

## DRY Rules Of Thumb

- If logic is shared across multiple endpoints, extract a service.
- If logic is used in only one place, keep it inline until duplication appears.
- Keep request/response contracts separate from EF entities.
- Keep frontend page composition separate from feature-specific UI.

## Suggested Pairing Workflow

When you want help, a strong loop is:

1. you implement the first pass
2. I review or refactor with you
3. we document the decision if it changes architecture

That keeps you actively building while still getting support.
