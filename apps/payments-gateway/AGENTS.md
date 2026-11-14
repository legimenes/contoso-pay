# AGENTS.md

This file provides project guidance for coding agents working in this source repository.

## Persona

You are a Senior Software Engineer.

## Project Structure (Solution)

The solution is divided into the following logical projects, following a strict dependency direction rule (from the outside in):

- `SharedKernel`: Contains a small set of domain concepts, abstractions, and building blocks that are shared across different parts of the application. It can be used in both Single Bounded Context and Multiple Bounded Contexts architectures.
- `Application`: Contains technical abstractions, contracts, and building blocks shared across `Application` projects. Its purpose is to provide common application-level capabilities without introducing dependencies between Bounded Contexts.
- `Gateway.Domain`: It contains the entities and domain rules inherent to the **Gateway Context** business.
- `Gateway.Application`: This is where the **Gateway Context** application flow is orchestrated. It contains the application's use-cases and domain services.
- `Gateway.API`: The **Gateway Context** application's entry point. It uses the ASP.NET Core Minimal APIs pattern. It is responsible only for routing, initial dependency injection, and mapping HTTP requests to use-cases.
- `Gateway.Adapters.Persistence`: The concrete implementation of data access to **Gateway Context**. It contains repositories, ORM configurations (such as Entity Framework Core), and database migrations. It acts as an **outbound Adapter**.
- `Adapters.Authorization`: The concrete implementation of OAuth 2.0 identity. It owns `OpenIddict`, its EF Core entities and `AuthorizationDbContext`, and the credential/token ports. It acts as an **outbound Adapter**.
- `Adapters.Identity`: The concrete implementation of user identity management. It owns `ASP.NET Core Identity`, its EF Core entities, user and role stores, and `IdentityDbContext`. It implements the user management and identity-related ports. It acts as an **outbound Adapter**.
- `Backoffice.Domain`. Soon...
- `Tests`: Soon... Automated testing setup covering both unit and integration tests.

## Progressive Documentation Loading

**IMPORTANT**: Only load documents relevant to your current task. Do NOT load all documentation at once.

### Decision Tree: What to Read

**IF** task involves understanding architecture:
➜ READ `backend/.specs/docs/architecture-guide.md`

**IF** task involves creating/modifying entities, use cases, services, endpoints:
➜ READ `backend/.specs/docs/coding-patterns.md`

**IF** task involves persistence, creating/modifying repositories, EF Core mapping conventions:
➜ READ `backend/.specs/docs/data-access-and-persistence.md`

**IF** task involves automate tests:
➜ READ `backend/.specs/docs/automated-tests.md`

## HIGH PRIORITY

- When you need to search docs, use `context7` tools.
- When a skill must be loaded or executed to obtain additional instructions, delegate the task to a subagent running in a separate context window whenever the environment supports it, and the agent’s final output should be passed to the main context window without interfering with the context with internal execution details.
