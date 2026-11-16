# Data Access and Persistence Patterns

This document details our persistence standards, repository split rules, EF Core mapping conventions, and database schema management workflows.

## Data Access Architecture

Use the **Repository Pattern** for data access, keeping persistence logic isolated from business logic. Use **EF Core** as the ORM for database operations. Do not access the `DbContext` directly from business layers; use repository/query interfaces and their implementations, following dependency injection principles.

### Repositories vs Queries (CQS / Read-Write Separation)

Split data access into two ports, classified by what the caller needs:

- **Repository** — the collection of aggregates. It creates aggregates, retrieves them by identity, and answers questions about the collection itself (existence by a natural key). It returns aggregates.
- **Queries** — the read side. It serves filtered and paginated reads shaped by the reader, returning the slice's own projection (its `Response` record) and never an aggregate.

#### Classification & Implementation Rules:

1. **Classification Rule**: A question about the aggregate collection belongs to the repository; a read for a reader, expressed as a projection, belongs to the queries. `GetByIdAsync` stays on the repository because it returns an aggregate; a filtered listing belongs to the queries.
2. **Contracts Ownership**: A queries port takes the slice's request and returns the slice's own response: both sides of the contract belong to the slice.
3. **Data Normalization**: The request arrives raw, so normalization (trimming filters) stays with the use-case, which hands the query a normalized copy — the adapter never normalizes.
4. **Collection Constraints**: A uniqueness rule that spans aggregates is a **collection constraint**, not an aggregate invariant — a single aggregate cannot guarantee it. It belongs to the repository (existence check by natural key), backed by a unique index, with the constraint violation mapped as a safety net for the race.
5. **No Extra Read Models**: Do not introduce a separate read model type when the slice's own `Response` already carries the projection, and do not add a generic `IQueries<,>` base before a second aggregate actually requires it.
6. **Location**: Both are driven ports: place each one in the folder of the feature that owns it (`Application/UseCases/<Feature>/`) and implement it in `Adapters.Persistence`, under `Repositories/` and `Queries/` respectively. A queries port named after an aggregate still lives in the feature folder, never in `Abstractions/`.

## EF Core Conventions

- **EntityTypeConfigurations**: Use dedicated entity configuration classes implementing `IEntityTypeConfiguration<TEntity>` to define entity mappings, including table names, column properties, primary keys, relationships, constraints, indexes, and other database-specific configurations. Keep these mappings separate from domain entities and apply them through the `DbContext` configuration.
- **DbContext Entity Access**: Do not declare `DbSet<TEntity>` properties in the `DbContext` to expose entities. To access entities in repositories, use `dbContext.Set<TEntity>()` directly.

## Schema Management & SQL Scripts

- **No EF Core Migrations**: Do not use *Migrations* in **Entity Framework Core** to manage database schema changes.
- **Single Source of Truth**: Maintain a single SQL file at `scripts/postgresql/01-objects.sql` containing the complete and up-to-date definition of all database objects. Whenever the data model changes, update this file to reflect the desired schema, including tables, columns, keys, relationships, indexes, constraints, and any other required objects.
- **Docker Environment Workflow**: Never drop Docker volumes to apply schema changes: fresh environments pick the file up via the init script, while existing databases receive only the new delta executed directly (e.g. via `psql`), with the file remaining the single source of truth.
