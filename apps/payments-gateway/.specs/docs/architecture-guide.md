# Architecture Guide

This document provides details about our overall architectural approach, layer responsibilities, and structural rules across solutions.

## Core Philosophy

This architecture blends concepts from **Ports and Adapters (Hexagonal Architecture)**, **Clean Architecture**, and **Vertical Slice Architecture**. Its primary goal is to isolate business rules from infrastructure and framework details, ensuring that the system is testable, maintainable, and ready to scale as its complexity increases.

## Ports and Adapters (Hexagonal Architecture)

The projects organize their code around three main concepts: **Application** (or Core), **Ports**, and **Adapters**.

```text
[ Driver Adapters (Inbound) ] (Minimal APIs, CLI, Kafka consumers)
        │
        ▼  (Uses Driver Port)
┌──────────────────────────────────────────────────────────┐
│                    APPLICATION (CORE)                    │
│                                                          │
│   ┌───────────────────────────────┐                      │
│   │          Application          │  <-- Flow            │
│   │    (Use Cases / Services)     │      Orchestration   │
│   └───────────────┬───────────────┘                      │
│                   │                                      │
│                   ▼                                      │
│   ┌───────────────────────────────┐                      │
│   │            Domain             │  <-- Pure Business   │
│   │   (Entities, Value Objects)   │      Rules           │
│   └───────────────┴───────────────┘                      │
└───────────────────┬──────────────────────────────────────┘
        │  (Implement Driven Port)
        ▼
[ Driven Adapters (Outbound) ] (Entity Framework, SQL Drivers, HTTP Client)
```

### Application (aka Core)

Contains the application's business logic and domain rules. It must remain independent of external technologies, frameworks, infrastructure, and delivery mechanisms.

In this repository, the term **Application** is used as an architectural concept. The architectural Application layer is composed of two distinct projects:

- **Domain**: Contains the domain model, business rules, entities, value objects, domain services, and domain contracts.
- **Application**: Contains application use cases, orchestration logic, application services, request/response models, validators, and inbound/outbound ports (interfaces).

> **Note**: Do not confuse **the architectural concept Application** with **the technical project named Application**.

### Vertical Slice and Screaming Architecture

The `Application` project is organized by **business capabilities** (Screaming Architecture), not by technical concerns. `BoundedContext.Application/UseCases/<Feature>/<Operation>/` keeps every file for an operation together; only cross-cutting contracts live under `BoundedContext.Application/Abstractions/`.

#### Features Folder Structure Example

For example, in a platform's Order feature, you find a feature-oriented structure (**Vertical Slice** + **Screaming**). All files related to an operation are kept together:

```text
Application
└── UseCases/
    └── Orders/
        └── CreateOrder/
            ├── ICreateOrder.cs
            ├── CreateOrder.cs
            ├── CreateOrderRequest.cs
            ├── CreateOrderResponse.cs
            └── CreateOrderRequestValidator.cs
```

### Ports

Define the contracts through which the **Application** communicates with the outside world. They must reside in the **Application** project.

### Driver Ports

Driver Port interfaces are placed inside the *operation use-case* folder (e.g., `CreateOrder/ICreateOrder`). Folder structure example:

```text
Application
└── UseCases/
    └── Orders/
        └── CreateOrder/
            └── ICreateOrder.cs
```

### Driven Ports

Driven Port interfaces are placed according to their scope following Vertical Slice Architecture:

- **Feature-specific interfaces**: Driven Port interfaces that belong to a specific feature must be created inside that feature's folder. (e.g., `IOrderQuery`, `IOrderRepository`)
- **Generic interfaces**: Driven Port interfaces that provide cross-cutting or reusable capabilities across multiple features must be created inside the `Abstractions` folder (e.g., `IExchangeToken`).

Folder structure example:

```text
Application
├── Abstractions/
│   └── IExchangeToken.cs/
└── UseCases/
    └── Orders/
        ├── CreateOrder/
        ├── IOrderQuery.cs
        └── IOrderRepository.cs
```

### Adapters

Implement or use the mechanisms required to connect external systems to the **Ports** (e.g., `Adapters.Persistence`, `Adapters.Identity`).

## Multiple Bounded Contexts

Projects in this repository adopt **Multiple Bounded Contexts**. Each *Bounded Context* must have its own `Application` and `Domain` projects, named using the corresponding business context as a prefix. Example:

```text
Solution.sln
├── Billing.Application/
│   └── UseCases/
│       └── Invoicing/
│           └── CreateInvoice/
│               ├── ICreateInvoice.cs
│               ├── CreateInvoice.cs
│               ├── CreateInvoiceRequest.cs
│               ├── CreateInvoiceResponse.cs
│               └── CreateInvoiceRequestValidator.cs
├── Billing.Domain/
│   └── Invoice.cs
├── Order.Application/
│   └── UseCases/
│       └── Orders/
│           └── CreateOrder/
│               ├── ICreateOrder.cs
│               ├── CreateOrder.cs
│               ├── CreateOrderRequest.cs
│               ├── CreateOrderResponse.cs
│               └── CreateOrderRequestValidator.cs
└── Order.Domain/
    └── Order.cs
```

## Strict Architectural Rules (Do Not Violate!)

- The **Domain** project must never reference **Application**, **Persistence**, **API**, or **Adapters**.
- Circular references between Slices within **Application** must be avoided. If two use-cases share a significant amount of logic, extract it into a **domain service within Application**.
- **Adapters** projects reference **Application** projects (to implement its interfaces), but **Application** projects must never reference the adapters.
- Composition and dependency injection take place exclusively at the **API** level (Composition Root).
