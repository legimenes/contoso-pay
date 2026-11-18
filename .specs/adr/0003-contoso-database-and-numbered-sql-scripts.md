---
Status: accepted
---

# One database, a schema per concern, numbered SQL scripts as the source of truth

The repository forbids EF Core migrations and names a single SQL file as the schema source of truth. We keep that principle but split it by concern: the existing `contoso` database holds Contoso Pay objects, the OpenIddict tables live in the `contosopay` schema, and their DDL is tracked in `scripts/postgresql/02-openiddict-objects.sql`, separate from the application schema file. Scripts are executed manually and never by the Docker init hook. Inside the schema the OpenIddict prefix is dropped and names are lowercase: `applications`, `authorizations`, `scopes`, `tokens`, mapped through `IEntityTypeConfiguration` classes.

## Considered Options

- **One database per bounded context** (`contoso_gateway`, `contoso_backoffice`, `contoso_authorization`). Rejected: isolation is already achieved by the schema, and separate databases would triple the bootstrap (roles, connection strings, health checks) for an educational project.
- **Everything in the `public` schema**, keeping the single `01-objects.sql`. Rejected: it mixes framework-owned tables with application tables and preserves a rule that the second schema already contradicts.
- **Mounting the scripts into `/docker-entrypoint-initdb.d/`.** Rejected: it hides schema evolution behind volume state, and the documented workflow forbids dropping volumes to apply changes.

## Consequences

- "One SQL file" now means "one numbered SQL file per concern, run in order". A fresh environment must run `01` then `02`; an existing one receives only the new delta.
- The application still needs a second schema for a different bounded context; this decision is scoped to `contosopay`, not a general convention.
- Table names no longer match the OpenIddict documentation examples, so anyone comparing the model to upstream docs must translate; keeping lowercase names also means no quoted identifiers in PostgreSQL.
