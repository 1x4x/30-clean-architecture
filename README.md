<div align="center">

# 🏛️ Clean Architecture

**Keep business rules at the center. Keep implementation details at the edges.**

A visual, practical guide to building software that stays maintainable as technology changes.

![Clean Architecture](https://img.shields.io/badge/ARCHITECTURE-CLEAN-8b5cf6?style=for-the-badge&labelColor=0d1117)
![Dependency Rule](https://img.shields.io/badge/DEPENDENCIES-INWARD-22d3ee?style=for-the-badge&labelColor=0d1117)
![Design](https://img.shields.io/badge/DESIGN-FRAMEWORK--INDEPENDENT-34d399?style=for-the-badge&labelColor=0d1117)

[Overview](#-overview) • [Layers](#-the-four-layers) • [Dependency Rule](#-the-dependency-rule) • [Example](#-putting-it-together) • [Trade-offs](#-trade-offs)

</div>

---

## 🌌 Overview

**Clean Architecture** organizes software around the business capabilities it provides rather than the frameworks, databases, or transport protocols it uses.

At the center are **domain rules** and **application use cases**. Infrastructure—such as HTTP, SQL, queues, and UI frameworks—belongs at the edges. Boundaries make those details replaceable.

> **The guiding rule:** Source-code dependencies point **inward**, toward business policy.

| Goal | How Clean Architecture helps |
| --- | --- |
| 🧠 Business focus | Core logic models business concepts rather than framework APIs. |
| 🔄 Changeability | Swap a database or delivery mechanism without rewriting use cases. |
| 🧪 Testability | Test application behavior using in-memory fakes. |
| 🔌 Explicit boundaries | Interfaces (ports) specify what the core needs from the outside world. |
| 🧭 Clear ownership | Each layer has a distinct responsibility. |

## 🎨 Architecture map

The Mermaid diagram uses a dark palette inspired by GitHub's dark theme. **Arrows represent source-code dependencies**, not necessarily runtime execution order.

```mermaid
%%{init: {"theme": "base", "themeVariables": {"background": "#0d1117", "primaryColor": "#161b22", "primaryTextColor": "#e6edf3", "primaryBorderColor": "#8b5cf6", "lineColor": "#a78bfa", "fontFamily": "ui-sans-serif, system-ui"}}}%%
flowchart TB
    F["🌐 Frameworks & Drivers<br/>Web · Database · UI · Queues"]
    A["🔌 Interface Adapters<br/>Controllers · Presenters · Repositories"]
    U["⚙️ Application / Use Cases<br/>Business workflows · Ports"]
    E["💎 Enterprise / Domain<br/>Entities · Value Objects · Invariants"]

    F -->|"depends on"| A
    A -->|"depends on"| U
    U -->|"depends on"| E

    classDef outer fill:#17212b,stroke:#64748b,color:#e2e8f0,stroke-width:2px;
    classDef adapter fill:#172036,stroke:#60a5fa,color:#dbeafe,stroke-width:2px;
    classDef usecase fill:#251b3b,stroke:#a78bfa,color:#ede9fe,stroke-width:2px;
    classDef domain fill:#112a25,stroke:#34d399,color:#d1fae5,stroke-width:3px;
    class F outer;
    class A adapter;
    class U usecase;
    class E domain;
```

> The diagram shows a simplified hierarchy of conceptual layers. Your source tree does not need to mirror the four rings exactly. **The direction of dependencies matters more than folder names.**

## 🧱 The four layers

### 01 · 💎 Entities — domain

The **innermost layer** represents durable business concepts and rules: entities, value objects, and invariants. It should not import HTTP libraries, ORMs, or database clients.

**Example:** An order must contain at least one item before it can be placed.

### 02 · ⚙️ Use cases — application

Use cases coordinate actions the system can perform: `PlaceOrder`, `RegisterUser`, or `CancelSubscription`.

They invoke domain behavior and depend on **abstract ports** for services such as persistence. A use case should not know whether the order is stored in PostgreSQL, Redis, or memory.

### 03 · 🔌 Interface adapters

Adapters translate between external representations and the application's boundary:

- Controllers turn HTTP requests into use-case inputs.
- Presenters turn use-case outputs into responses.
- Repository adapters implement persistence ports using a real database.

Their job is **translation**, not the ownership of core business rules.

### 04 · 🌐 Frameworks & drivers

These are concrete delivery and infrastructure details: routers, web servers, database engines, third-party SDKs, messaging tools, and UI frameworks.

> **Frameworks are tools, not the foundation of your business logic.**

## 🧭 The dependency rule

**Code in an inner layer must not depend on code in an outer layer.**

For example, an application use case can define a `UserRepository` interface, while a database adapter implements it. This is **dependency inversion**: the abstraction lives close to the policy that needs it.

```mermaid
%%{init: {"theme": "base", "themeVariables": {"background": "#0d1117", "primaryTextColor": "#e6edf3", "lineColor": "#a78bfa", "fontFamily": "ui-sans-serif, system-ui"}}}%%
flowchart LR
    H["HTTP Controller"] --> C["CreateUser Use Case"]
    P["PostgresUserRepository"] -. "implements" .-> R["UserRepository Port"]
    C --> R
    C --> D["User Domain Model"]
    P --> D

    classDef entry fill:#172036,stroke:#60a5fa,color:#dbeafe,stroke-width:2px;
    classDef core fill:#251b3b,stroke:#a78bfa,color:#ede9fe,stroke-width:2px;
    classDef domain fill:#112a25,stroke:#34d399,color:#d1fae5,stroke-width:2px;
    class H,P entry;
    class C,R core;
    class D domain;
```

The **runtime request flow** can travel from a controller to a use case to a repository implementation and then to a database. That does **not** mean the application imports the repository implementation: the implementation is supplied at startup by composition or dependency injection.

## 💻 Putting it together

Below is a deliberately small **TypeScript example**, for illustration rather than a claim about this repository's implementation. It registers a user without coupling the use case to a database or web framework.

**1. Domain model** — `src/domain/user.ts`

```ts
export type User = {
  id: string;
  email: string;
};
```

**2. Application port and use case** — `src/application/register-user.ts`

```ts
import type { User } from "../domain/user";

export interface UserRepository {
  findByEmail(email: string): Promise<User | null>;
  save(user: User): Promise<void>;
}

export interface IdGenerator {
  generate(): string;
}

export class RegisterUser {
  constructor(
    private readonly users: UserRepository,
    private readonly ids: IdGenerator
  ) {}

  async execute(email: string): Promise<User> {
    const normalized = email.trim().toLowerCase();

    if (!normalized.includes("@")) {
      throw new Error("Invalid email");
    }

    if (await this.users.findByEmail(normalized)) {
      throw new Error("Email already registered");
    }

    const user = { id: this.ids.generate(), email: normalized };
    await this.users.save(user);
    return user;
  }
}
```

**3. Infrastructure adapter** — `src/infrastructure/in-memory-user-repository.ts`

```ts
import type { User } from "../domain/user";
import type { UserRepository } from "../application/register-user";

export class InMemoryUserRepository implements UserRepository {
  private readonly items = new Map<string, User>();

  async findByEmail(email: string): Promise<User | null> {
    return [...this.items.values()].find(u => u.email === email) ?? null;
  }

  async save(user: User): Promise<void> {
    this.items.set(user.id, user);
  }
}
```

**4. Composition root** — `src/main.ts`

```ts
import { randomUUID } from "node:crypto";
import { RegisterUser } from "./application/register-user";
import { InMemoryUserRepository } from "./infrastructure/in-memory-user-repository";

const registerUser = new RegisterUser(
  new InMemoryUserRepository(),
  { generate: randomUUID }
);

const newUser = await registerUser.execute("hello@example.com");
console.log(newUser);
```

The use case can now be tested without HTTP or SQL. A production adapter can replace the in-memory repository at the composition root without changing `RegisterUser`.

> For a real production system, use a robust email validator and handle concurrent registration with a database uniqueness constraint. The sample intentionally focuses on **dependency direction**.

## 📂 Example project structure

```text
src/
├── domain/
│   ├── user.ts                     # Business model
│   └── ...                         # Domain policies
├── application/
│   ├── register-user.ts            # Use case + required ports
│   └── ...                         # Application workflows
├── adapters/
│   ├── http/                       # Request/response translation
│   └── persistence/                # SQL/ORM implementations
├── infrastructure/
│   └── ...                         # External integrations
└── main.ts                         # Dependency wiring
```

This structure is a **suggestion**, not an existing repository inventory. Organize by feature or bounded context when the application becomes large enough to benefit from it.

## 🧪 Testing strategy

| Level | What to test | Usually requires |
| --- | --- | --- |
| **Domain** | Business invariants and decisions | Plain objects |
| **Application** | Use-case behavior, errors, orchestration | Fakes / mocks of ports |
| **Adapters** | Serialization, database queries, SDK mapping | Integration test infrastructure |
| **End-to-end** | Critical flows across the deployed system | Running services |

A healthy test suite tends to put most fast tests near domain and application rules, while using integration tests to verify that real adapters honor their contracts.

## ⚖️ Trade-offs

| ✅ Benefits | ⚠️ Costs |
| --- | --- |
| Business logic survives framework changes | More interfaces and wiring |
| Dependencies are deliberate and visible | Additional abstraction can slow a tiny project |
| Easy-to-isolate use cases | Poorly designed boundaries can add boilerplate |
| Infrastructure is replaceable | Requires discipline about import direction |

**Use Clean Architecture when** business rules are substantial, the application will evolve, multiple integrations are expected, or independent testing is important.

**Keep it simpler when** building a small one-off script, a throwaway prototype, or a very basic CRUD service where extra layering brings little value. It is a design tool—not a requirement for every project.

## 🚫 Common pitfalls

- **Layering by folder name only:** Having a `domain/` directory means little if it imports ORM models.
- **Putting business rules in controllers:** Transport concerns should not decide domain behavior.
- **Creating interfaces for everything:** Introduce ports where they protect a meaningful boundary.
- **Leaking framework types:** Avoid passing framework request objects into your use cases.
- **Confusing call flow with import flow:** Runtime calls can reach outward through ports; source-code dependencies still point inward.

## 📚 Further reading

- [Robert C. Martin — The Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [Martin Fowler — Inversion of Control](https://martinfowler.com/bliki/InversionOfControl.html)
- [Martin Fowler — Repository](https://martinfowler.com/eaaCatalog/repository.html)

---

<div align="center">

### 💜 Keep the core clean.

**Domain first · Dependencies inward · Details at the edges**

Made for developers who want software that stays understandable.

</div>
