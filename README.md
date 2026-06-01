# StudyGroupApi — C# Whitebox API Testing

A portfolio project demonstrating white-box testing of an ASP.NET Core REST API using NUnit, Moq, and EF Core in-memory SQLite. The emphasis is on test architecture, test readability, and multi-layer verification across unit and component tests.

---

## Projects

| Project | Type | Description |
|---|---|---|
| `StudyGroupApi` | REST API | ASP.NET Core REST API — study group management (create, join, leave, search) |
| `StudyGroupApi.UnitTests` | Unit Tests | Domain logic + controller behavior via Moq |
| `StudyGroupApi.ComponentTests` | Component Tests | Controller + repository wired with SQLite in-memory DB |

---

## Public Test Report

The latest published HTML test report is available here:

- Detailed HTML report: `https://aliboztemir.github.io/csharp-whitebox-api-testing/details.html`
- Report landing page: `https://aliboztemir.github.io/csharp-whitebox-api-testing/`

Because the report is published through GitHub Pages, it can be viewed by public/guest users without signing in, as long as the repository Pages site remains public.

---

## CI/CD with GitHub Actions

This repository includes an automated CI/CD workflow using GitHub Actions.

### What the workflow does

On every push, pull request, or manual trigger:

1. Checks out the repository
2. Sets up .NET 8
3. Restores NuGet dependencies
4. Builds the solution in Release mode
5. Runs all tests and generates TRX test results
6. Publishes parsed test output to GitHub Checks
7. Generates a customer-friendly HTML report and a detailed HTML report from TRX results
8. Uploads TRX and HTML artifacts
9. Deploys the HTML report to GitHub Pages when tests complete successfully

### Workflow location

- `.github/workflows/tests.yml`

### Manual execution

You can manually run the workflow from the **Actions** tab using the **Run workflow** button.

### Published output

After a successful run:

- GitHub Pages summary page is published as the landing page
- Detailed per-test HTML output is published at the detailed report link above
- Raw `.trx` files remain available in workflow artifacts for technical traceability

This gives both a stakeholder-friendly report view and a developer-friendly raw execution record.

---

## White-Box Testing Approach

This project applies white-box testing principles across two test layers:

- **Unit Tests** — test individual classes in isolation, with dependencies replaced by Moq mocks. Includes domain entity validation and controller logic.
- **Component Tests** — test the controller and repository together using a real SQLite in-memory database. Exercises realistic dependency wiring, database persistence, and HTTP response behavior.

---

## Unit Testing

Located in `StudyGroupApi.UnitTests/`.

### Domain Tests (`Domain/StudyGroupDomainTests.cs`)
Tests `StudyGroup` entity construction and business rules directly:
- ID, name, subject, and date validation
- Boundary conditions (min/max name length, invalid enum values)
- `AddUser` / `RemoveUser` behavior including null and duplicate handling

### Controller Tests (`Controllers/StudyGroupControllerUnitTests.cs`)
Tests controller methods with a mocked `IStudyGroupRepository`:
- Verifies correct HTTP responses for all endpoints
- Verifies repository method invocation and invocation count using `Moq.Verify`
- Covers happy path and negative cases

---

## Component Testing

Located in `StudyGroupApi.ComponentTests/`.

### Controller + Repository Tests (`Controllers/StudyGroupControllerDBComponentTests.cs`)
Tests the full controller → repository → SQLite pipeline:
- Realistic dependency wiring (no mocks)
- Verifies data is persisted to and read from the database
- Tests sorting, filtering, join, and leave behavior with real DB state
- Covers boundary and negative cases end-to-end

---

## Test Builders

Located in shared file `TestSupport/Builders/Builders.cs` and linked by both test projects.

Builders provide sensible defaults and fluent configuration to reduce test setup duplication:

```csharp
var user = new UserBuilder()
    .WithId(1)
    .WithName("Alice")
    .Build();

var studyGroup = new StudyGroupBuilder()
    .WithId(1)
    .WithName("Math Club")
    .WithSubject(Subject.Math)
    .WithUser(user)
    .Build();
```

---

## Mock Verification

Controller unit tests use explicit `Moq.Verify` to confirm repository interactions:

```csharp
_mockRepo.Verify(repo => repo.CreateStudyGroup(studyGroup), Times.Once);
_mockRepo.Verify(repo => repo.JoinStudyGroup(1, 1), Times.Once);
_mockRepo.Verify(repo => repo.CreateStudyGroup(It.IsAny<StudyGroup>()), Times.Never);
```

This makes white-box intent explicit — not just *what* the controller returns, but *how* it interacts with its dependencies.

---

## NUnit Categories

Tests are tagged with relevant categories for selective execution:

| Category | Meaning |
|---|---|
| `Unit` | Isolated unit test (mocks, no I/O) |
| `Component` | Wired test with real dependencies |
| `Domain` | Tests on entity logic |
| `Controller` | Tests on controller behavior |
| `Validation` | Boundary or input validation scenarios |
| `Negative` | Tests for rejection, error, or failure cases |

Run by category:
```bash
dotnet test --filter TestCategory=Unit
dotnet test --filter TestCategory=Negative
dotnet test --filter TestCategory=Component
```

---

## Test Coverage Matrix

| Scenario | Unit Tests | Component Tests |
|---|---|---|
| Create Study Group | ✓ | ✓ |
| Validate Study Group Name | ✓ | ✓ |
| Validate Subject | ✓ | ✓ |
| Join Study Group | ✓ | ✓ |
| Leave Study Group | ✓ | ✓ |
| List Study Groups | ✓ | ✓ |
| Search by Subject | ✓ | ✓ |
| Repository Persistence | — | ✓ |

---

## Running Tests

```bash
# All tests
dotnet test StudyGroupApi.sln

# Unit tests only
dotnet test StudyGroupApi.UnitTests/

# Component tests only
dotnet test StudyGroupApi.ComponentTests/
```

---

## Tech Stack

- .NET 8 / ASP.NET Core Web API
- Entity Framework Core (SQLite / SQL Server)
- NUnit 3
- Moq
- GitHub Actions CI/CD
