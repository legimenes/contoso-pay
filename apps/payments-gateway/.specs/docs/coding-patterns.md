# Coding Patterns & Implementation Guidelines

This document provides technical implementation patterns for endpoints, use-cases, domain entities, validators, and general C# coding rules.

## General Development Guideline (For Developers and LLMs)

When adding a new feature, follow this mental workflow:

1. **Entities (Domain)**: Does the business rule require changing the state of an entity? Modify the `Domain` first.
2. **Contracts (Application)**: If communication with an external system is required (database, queue, vendor), create the interface in Application.
3. **Use-Case (Application)**: Implement the feature following the Vertical Slice approach. Create a new directory named after the feature, then another directory named after the operation/use-case (e.g., `CreateOrder`), and add its respective files (e.g., Interface, UseCase, Request, Response, and Validator).
4. **Data Access (Persistence/Adapters)**: Implement the newly created interfaces in their respective adapters.
5. **Exposure (API)**: Create the endpoint by mapping the Minimal API to trigger the corresponding use-case in `Application` projects. Never implement business rules in the Minimal API.

## Results Pattern

Use the **Results Pattern** to represent the outcome of an operation explicitly, distinguishing successful execution from failure without relying on exceptions for expected business scenarios. A `Result` object should indicate whether the operation succeeded or failed and, when appropriate, carry the resulting value on success or error information on failure.

**Recommendation**: Prefer the **Results Pattern** for methods that return an object or value and may encounter an expected failure during execution. Examples include use-cases, repository operations, domain services, and external service integrations. Instead of returning `null`, using ambiguous return values, or throwing exceptions for expected failures, return a `Result<T>` that clearly communicates success or failure and provides the corresponding value or error information. Reserve exceptions for unexpected situations and exceptional conditions that cannot be reasonably represented as part of the operation's normal flow.

The application provides an internal **Results Pattern** implementation under `SharedKernel/Results`. Reuse the existing result types and abstractions whenever an operation needs to explicitly represent success or failure. Do not introduce external libraries or create alternative result abstractions unless explicitly required.

## Domain Entities

Domain entities must strictly adhere to the following rules:

- **Single constructor**: Each entity must have only one constructor, accepting all the properties required to fully initialize the instance.
- **Static factory method**: Each entity must provide a **static factory method** responsible for creating and returning a new instance of the entity by using the single constructor.

Conceptual example:

```cs
public sealed class Customer(
    Guid id,
    string name,
    string document,
    DateTime createdAtUtc)
{
    public Guid Id { get; } = id;
    public string Name { get; } = name;
    public string Document { get; } = document;
    public DateTime CreatedAtUtc { get; } = createdAtUtc;

    public static Customer New(
        string name,
        string document)
    { 
        return new(
            id: Guid.NewGuid(),
            name: name.Trim(),
            document: document.Trim(),
            createdAtUtc: DateTime.UtcNow);
    }
}
```

## Resource File

All user-facing messages must be defined in the **resource file** `Resource.resx` located in the **SharedKernel** project. Do not hardcode message strings throughout the codebase. Always reference the appropriate resource entry when displaying validation messages, error messages, or other messages that may be exposed to the user. Hardcoded strings are allowed only for one-off log messages that will not be exposed to the user.

## Features Coding Rules

- The capability folder MUST be named after the operation/use-case (`CreateCustomer`, `GetCustomerById`, `ListCustomers`), never after the verb alone (`Create`, `GetById`, `List`), and its namespace MUST mirror the folder path.
- Use the **`FluentValidation` NuGet package** to implement validators for all request objects. Do not use data annotations or custom validation logic.
- Each *Use-Case* MUST own its own *Request* and *Response* models within its respective directory. **NEVER reuse, reference, or share Request or Response models between different Use-Cases**, even if their definitions are structurally identical.
- Generic standardized contracts - such as pagination envelopes - MAY live under the `Abstractions` folder and be composed into any use-case's response models; per-use-case Request and Response models MUST still be owned by the slice and never shared.
- **Paginated requests** are positional records inheriting from `Application.Abstractions.Pagination.PaginationRequest`, restating Page/PageSize defaults only via the base's constants (`Page = PaginationRequest.PageDefault`, `PageSize = PaginationRequest.PageSizeDefault`); slice validators MUST inherit from `AbstractValidator<TRequest>` and execute the shared pagination rules through `Include(new PaginationRequestValidator())` (composition; FluentValidation's contravariant `IValidator<in T>` accepts the base-type validator), so the pagination rules run for every slice without duplication.

## Asynchronous Calls

**Never use `.ConfigureAwait()` in asynchronous code**. This applies to all await expressions throughout the application. Always use await directly on asynchronous operations without chaining `.ConfigureAwait(false)` or `.ConfigureAwait(true)`.

## General Coding Rules and Style

- **Nested classes are not allowed within the same file**: Classes must always be defined in separate files, in order to maintain clean navigation and appropriate granularity.
- **File-scoped namespaces**: All C# files must use file-scoped namespaces instead of block-scoped namespaces.
- **Primary constructors**: Use primary constructors whenever a class or record requires constructor parameters. Avoid declaring explicit constructors when the primary constructor syntax is applicable.
- **Expression-bodied member**: Não utilize expression-bodied members (`=>`) na declaração de métodos. Sempre utilize a sintaxe tradicional com blocos de código delimitados por `{}`. Métodos que retornam valores devem utilizar a instrução `return` explicitamente.
- **Never add a newline character at the end of a C# source file**: The file must end immediately after the final character of the last line of code, typically the closing brace `}`.
