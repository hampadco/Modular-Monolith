# .NET 9 Modular Monolith Template 🚀

[![.NET 9](https://img.shields.io/badge/.NET-9.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![FastEndpoints](https://img.shields.io/badge/FastEndpoints-5.0-yellow)](https://fast-endpoints.com/)
[![Architecture](https://img.shields.io/badge/Architecture-Modular%20Monolith-blue)](https://github.com/khalidabuhakmeh/modular-monolith-with-dotnet)
[![License](https://img.shields.io/badge/License-MIT-green)](LICENSE)

A professional, production-ready template for building scalable applications using **Modular Monolith** architecture with **Vertical Slices** on **.NET 9**.

This project demonstrates high-cohesion, low-coupling design principles, making it easy to maintain, test, and eventually transition to Microservices if needed.

---

## 🏗 Architecture Overview

The solution follows the **Modular Monolith** approach, where the application is deployed as a single unit but internally structured into independent modules. Each module encapsulates a specific business domain (e.g., Identity, Sales) and communicates with others via well-defined interfaces or integration events.

### Key Architectural Patterns
-   **Vertical Slices**: Code is organized by "Features" rather than technical layers (Controller/Service/Repository).
-   **REPR Pattern**: (Request-Endpoint-Response) replaces traditional MVC Controllers, implemented using **FastEndpoints**.
-   **Domain-Driven Design (DDD)**: Rich domain models (`AggregateRoot`, `ValueObject`) ensure business logic stays in the core.
-   **Event-Driven Communication**: Modules communicate asynchronously via Domain/Integration Events to decouple write operations (using `MassTransit` patterns).
-   **Public APIs**: Synchronous inter-module communication is restricted to read-only operations via defined `IModuleApi` contracts.

> 📘 **Deep Dive**: For a detailed Persian guide on the architectural decisions, see [OPTIMIZED_ARCHITECTURE_GUIDE.md](./OPTIMIZED_ARCHITECTURE_GUIDE.md).

---

## 🛠 Technology Stack

-   **Core**: .NET 9.0
-   **API**: [FastEndpoints](https://fast-endpoints.com/) (Minimal APIs wrapper)
-   **Data Access**: Entity Framework Core (Writes), Dapper/EF Core (Reads)
-   **Validation**: FluentValidation
-   **Testing**: 
    -   xUnit / NUnit
    -   [NetArchTest](https://github.com/BenMorris/NetArchTest) (Architecture enforcement)
-   **Messaging**: Abstracted Event Bus (ready for MassTransit/RabbitMQ)

---

## 📂 Project Structure

```text
src/
├── App.Host/                   # Application Entry Point (Web API)
│   └── Program.cs              # DI Composition Root
│
├── App.Modules.*/              # Business Modules (Independent Projects)
│   ├── Features/               # Vertical Slices (Endpoints, Commands, Handlers)
│   ├── Domain/                 # Domain Entities & Logic
│   ├── Infrastructure/         # Persistence & External Services
│   └── PublicApi/              # Contracts exposed to other modules
│
└── App.Shared.Kernel/          # Shared Building Blocks (DDD base classes, Interfaces)
    ├── Abstractions/           # AggregateRoot, IDomainEvent, IModuleApi
    └── Common/                 # Common Utilities
```

---

## 🚀 Getting Started

### Prerequisites
-   [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
-   IDE: Visual Studio 2022, JetBrains Rider, or VS Code.

### Running the Application

1.  **Clone the repository:**
    ```bash
    git clone https://github.com/your-username/your-repo.git
    cd your-repo
    ```

2.  **Restore dependencies:**
    ```bash
    dotnet restore
    ```

3.  **Run the Host:**
    ```bash
    cd templete/src/App.Host
    dotnet run
    ```
    The API will be available at `https://localhost:7197` (or similar, check console output).
    Swagger UI: `https://localhost:7197/swagger`

---

## 🧩 Modules

### 👤 Identity Module
Handles user authentication, registration, and profile management.
-   **Path**: `src/Modules/App.Modules.Identity`

### 🛍 Sales Module
Manages orders, products, and sales processing.
-   **Path**: `src/Modules/App.Modules.Sales`

---

## 🧪 Testing

We strictly enforce architectural rules using **Architecture Tests**.

Run tests to ensure compliance:
```bash
dotnet test
```

**Key rules checked:**
-   Modules must not reference each other directly (except via `PublicApi`).
-   Domain layer must not depend on Infrastructure.
-   Controllers/Endpoints must not contain business logic.

---

## 🤝 Contributing

1.  Create a branch for your feature (`feature/amazing-feature`).
2.  Follow the **Vertical Slice** pattern: create a new folder in `Features/` for your use case.
3.  Ensure all Architecture Tests pass.
4.  Submit a Pull Request.

---

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

