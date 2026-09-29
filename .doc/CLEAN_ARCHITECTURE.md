# Clean Architecture Implementation Guide

## Table of Contents
1. [Architecture Overview](#architecture-overview)
2. [Layer Responsibilities](#layer-responsibilities)
3. [Data Flow](#data-flow)
4. [DTO vs ViewModel](#dto-vs-viewmodel)
5. [File Structure](#file-structure)
6. [Mapping Patterns](#mapping-patterns)
7. [Implementation Checklist](#implementation-checklist)
8. [Benefits](#benefits)

---

## Architecture Overview

This project follows the **Clean Architecture** (also known as Onion Architecture or Hexagonal Architecture) pattern, adapted for ASP.NET Core MVC.

### The Dependency Rule

```
Web (Outer) --> depends on --> Business (Middle) --> depends on --> Data (Inner)
```

**Key Principle:** Dependencies point INWARD. Outer layers depend on inner layers, never the reverse.

### Visual Diagram

```
+---------------------------------------------------------------------------+
|                        USER / BROWSER                                      |
+---------------------------------------------------------------------------+
                                              |
                                              v
+---------------------------------------------------------------------------+
|                   WEB LAYER (Presentation)                                 |
|                                                                           |
|   View (.cshtml)  <-- ViewModel --> Controller <-- Service               |
|      |              |           |          |                              |
|   HTML          CourseIndex   Maps to    ICourseService                  |
|                 ViewModel     DTOs       (interface)                     |
|                                                                           |
|   * ViewModels live HERE. NEVER in Business.                              |
+---------------------------------------------------------------------------+
                                              | depends on
                                              v
+---------------------------------------------------------------------------+
|                BUSINESS LAYER (Logic)                                      |
|                                                                           |
|   ICourseService  <-- CourseService --> CourseDto                        |
|         |                |              |                                 |
|   Interface       Business      DTO (layer boundary)                     |
|                   Rules                         * DTOs                   |
|                                             live HERE.                   |
+---------------------------------------------------------------------------+
                                              | depends on
                                              v
+---------------------------------------------------------------------------+
|                 DATA LAYER (Persistence)                                   |
|                                                                           |
|   DbContext  <--> CourseEntity  <-->  Database                           |
|                                                                           |
|   * Entities live HERE. Plain POCOs. No logic.                           |
+---------------------------------------------------------------------------+
                                              |
                                              v
+---------------------------------------------------------------------------+
|                   DATABASE (SQL Server)                                    |
+---------------------------------------------------------------------------+
```

---

## Layer Responsibilities

### 1. Data Layer (`AIVES.Data`)

**Purpose:** Database access and persistence.

**Contains:**
- `Entities/` - Plain POCO classes that mirror database tables
- `DbContext/` - EF Core DbContext
- `Repositories/` - Data access methods (optional)
- `Migrations/` - EF Core migrations

**Does NOT contain:**
- Business logic
- Validation rules
- UI-specific code

**Dependencies:** None (or configuration only)

### 2. Business Layer (`AIVES.Business`)

**Purpose:** Business rules, validation, and domain logic.

**Contains:**
- `Interfaces/` - Service contracts (e.g., `ICourseService`)
- `Services/` - Business logic implementations
- `DTOs/` - Data Transfer Objects (layer boundary contracts)
- `Validators/` - Business rule validators

**Does NOT contain:**
- UI code
- HTTP handling
- ViewModels

**Dependencies:** Data layer (via interfaces or direct)


---

## Data Flow

### Read Path (Database -> View)

```
Step 1: Data Layer
   DbContext --> queries --> List<CourseEntity>

Step 2: Business Layer
   Service --> transforms --> List<CourseDto>
   (Entities --> DTOs)

Step 3: Controller
   Controller --> maps --> List<CourseViewModel>
   (DTOs --> ViewModels)

Step 4: View
   Razor --> renders --> HTML
   (ViewModel --> HTML)
```

### Write Path (View -> Database)

```
Step 1: View
   User submits --> CourseViewModel

Step 2: Controller
   Controller --> validates --> CourseViewModel --> maps --> CourseCreateDto

Step 3: Business Layer
   Service --> validates business rules --> CourseCreateDto --> transforms --> CourseEntity

Step 4: Data Layer
   DbContext --> inserts --> CourseEntity --> Database
```

---

## File Structure

```
AIVES/
+-- AIVES.Data/                    <-- DATA LAYER
|   +-- Entities/                  <-- Database table classes
|   |   +-- Course.cs
|   |   +-- Question.cs
|   |   +-- Rubric.cs
|   |   +-- CourseMaterial.cs
|   +-- DbContext/                 <-- EF Core context
|   |   +-- AIVESDbContext.cs
|   +-- Migrations/                <-- EF migrations
|   +-- AIVES.Data.csproj
|
+-- AIVES.Business/                <-- BUSINESS LAYER
|   +-- Interfaces/                <-- Service contracts
|   |   +-- ICourseService.cs
|   |   +-- IQuestionService.cs
|   |   +-- IRubricService.cs
|   |   +-- ICourseMaterialService.cs
|   +-- Services/                  <-- Business logic
|   |   +-- CourseService.cs
|   |   +-- QuestionService.cs
|   |   +-- RubricService.cs
|   |   +-- CourseMaterialService.cs
|   |   +-- SimpleRAGService.cs
|   +-- DTOs/                      <-- Layer boundary contracts
|   |   +-- CourseDto.cs
|   |   +-- QuestionDTOs.cs
|   |   +-- RubricDTOs.cs
|   |   +-- MaterialDTOs.cs
|   +-- AIVES.Business.csproj
|
```

### Web Layer

```
+-- AIVES.Web/                     <-- WEB LAYER
|   +-- Controllers/               <-- Thin orchestrators
|   |   +-- HomeController.cs
|   |   +-- CoursesController.cs
|   |   +-- QuestionsController.cs
|   |   +-- RubricsController.cs
|   |   +-- CourseMaterialsController.cs
|   +-- ViewModels/                <-- UI models (NEW!)
|   |   +-- QuestionIndexViewModel.cs
|   |   +-- QuestionFormViewModel.cs
|   |   +-- RubricViewModels.cs
|   |   +-- CourseMaterialViewModels.cs
|   |   +-- AIGenerateViewModel.cs
|   |   +-- HomeDashboardViewModel.cs
|   +-- Views/                     <-- Razor views
|   |   +-- Home/
|   |   |   +-- Index.cshtml       <-- @model HomeDashboardViewModel
|   |   |   +-- Privacy.cshtml
|   |   +-- Questions/
|   |   |   +-- Index.cshtml       <-- @model IEnumerable<QuestionIndexViewModel>
|   |   |   +-- Details.cshtml     <-- @model QuestionDetailViewModel
|   |   |   +-- Create.cshtml      <-- @model QuestionCreateViewModel
|   |   |   +-- Pending.cshtml     <-- @model IEnumerable<QuestionIndexViewModel>
|   |   |   +-- AIGenerate.cshtml  <-- @model AIGenerateViewModel
|   |   +-- Rubrics/
|   |   |   +-- Index.cshtml       <-- @model IEnumerable<RubricIndexViewModel>
|   |   |   +-- Details.cshtml     <-- @model RubricDetailViewModel
|   |   |   +-- Create.cshtml      <-- @model RubricCreateViewModel
|   |   |   +-- Edit.cshtml        <-- @model RubricUpdateViewModel
|   |   +-- CourseMaterials/
|   |   |   +-- Index.cshtml       <-- @model IEnumerable<CourseMaterialIndexViewModel>
|   |   |   +-- Details.cshtml     <-- @model CourseMaterialDetailViewModel
|   |   |   +-- Create.cshtml      <-- @model CourseMaterialCreateViewModel
|   |   |   +-- Preview.cshtml     <-- @model CourseMaterialDetailViewModel
|   |   +-- Shared/
|   |       +-- _Layout.cshtml
|   |       +-- _ViewImports.cshtml
|   |       +-- Error.cshtml
|   +-- Middleware/                <-- Exception handling
|   |   +-- ExceptionHandlingMiddleware.cs
|   +-- wwwroot/                   <-- Static files
|   +-- Program.cs                 <-- Startup
|   +-- AIVES.Web.csproj
|
+-- CLEAN_ARCHITECTURE.md          <-- This documentation
+-- README.md                      <-- Project README
```

---

## Mapping Patterns

### Controller Mapping Method

Each controller contains private mapping methods that translate between DTOs and ViewModels:

```csharp
public class QuestionsController : Controller
{
    private readonly IQuestionService _questionService;

    // GET returns DTOs from service
    public async Task<IActionResult> Index()
    {
        var dtos = await _questionService.GetAllQuestionsAsync();
        var viewModel = dtos.Select(MapToIndex).ToList();  // DTO -> ViewModel
        return View(viewModel);
    }

    // POST receives ViewModel from form
    [HttpPost]
    public async Task<IActionResult> Create(QuestionCreateViewModel viewModel)
    {
        if (ModelState.IsValid)
        {
            var dto = MapToCreateDto(viewModel);  // ViewModel -> DTO
            await _questionService.CreateQuestionAsync(dto);
            return RedirectToAction(nameof(Index));
        }
        return View(viewModel);
    }

    // ===== Mapping Methods =====
    private QuestionIndexViewModel MapToIndex(QuestionDto dto)
    {
        return new QuestionIndexViewModel
        {
            Id = dto.Id,
            Text = dto.Text,
            BloomLevel = dto.BloomLevel,
            Status = dto.Status,
            CourseName = dto.CourseName,
            // ... map all relevant fields
        };
    }

    private QuestionCreateDto MapToCreateDto(QuestionCreateViewModel vm)
    {
        return new QuestionCreateDto
        {
            Text = vm.Text,
            BloomLevel = vm.BloomLevel,
            // ... map all relevant fields
        };
    }
}
```

### ViewModel with UI Helpers

ViewModels can include computed properties for UI purposes:

```csharp
public class QuestionIndexViewModel
{
    // Core data (mapped from DTO)
    public int Id { get; set; }
    public string Text { get; set; }
    public string BloomLevel { get; set; }
    public string Status { get; set; }

    // UI helpers (computed in ViewModel)
    public string StatusBadge => Status switch
    {
        "Approved" => "bg-success",
        "Pending" => "bg-warning text-dark",
        "Rejected" => "bg-danger",
        _ => "bg-secondary"
    };

    public string EditUrl => $"/Questions/Edit/{Id}";
    public string DeleteUrl => $"/Questions/Delete/{Id}";
}
```

### View Usage

```html
@model IEnumerable<QuestionIndexViewModel>

@foreach (var q in Model)
{
    <tr>
        <td>@q.Id</td>
        <td>@q.Text</td>
        <td>
            <span class="badge @q.StatusBadge">@q.Status</span>
        </td>
        <td>
            <a href="@q.EditUrl" class="btn btn-sm btn-primary">Edit</a>
            <a href="@q.DeleteUrl" class="btn btn-sm btn-danger">Delete</a>
        </td>
    </tr>
}
```

---

## Implementation Checklist

### When Adding a New Entity

- [ ] **Data Layer:** Create entity class in `AIVES.Data/Entities/`
- [ ] **Data Layer:** Add DbSet to DbContext
- [ ] **Data Layer:** Create EF migration
- [ ] **Business Layer:** Create DTOs in `AIVES.Business/DTOs/`
- [ ] **Business Layer:** Create interface in `AIVES.Business/Interfaces/`
- [ ] **Business Layer:** Create service in `AIVES.Business/Services/`
- [ ] **Web Layer:** Create ViewModels in `AIVES.Web/ViewModels/`
- [ ] **Web Layer:** Create controller with mapping methods
- [ ] **Web Layer:** Create Views with `@model` pointing to ViewModels
- [ ] **Web Layer:** Update `_ViewImports.cshtml` to include ViewModels namespace
- [ ] **Test:** Verify build succeeds
- [ ] **Test:** Verify data flows correctly through all layers

### When Modifying an Existing Entity

1. **If changing database schema:** Update entity -> add migration
2. **If changing business rules:** Update service -> update DTOs if needed
3. **If changing UI:** Update ViewModel -> update Views
4. **NEVER:** Change a DTO and expect Views to still work without updating

---

## Benefits

### 1. Loose Coupling
- Views don't know about Business layer DTOs
- Business layer doesn't know about Web layer
- Each layer can change independently

### 2. Testability
- Business services can be tested without UI
- Controllers can be tested with mock services
- DTOs are simple POCOs, easy to test

### 3. Maintainability
- UI changes don't affect business logic
- Business rule changes don't break Views
- Clear separation of concerns

### 4. Security
- DTOs can exclude sensitive fields
- ViewModels can sanitize data for display
- No direct entity exposure to Views

### 5. Flexibility
- Can swap UI framework without changing business logic
- Can add new UI (API, mobile) without duplicating business rules
- Can replace database without changing business logic

---

## Before vs After Comparison

### Before (WRONG - Tight Coupling)

```csharp
// Controller
public IActionResult Index()
{
    var dtos = _service.GetAllAsync();  // Returns DTOs
    return View(dtos);                  // View gets DTOs directly
}

// View (Index.cshtml)
@model IEnumerable<AIVES.Business.DTOs.QuestionDto>  // View depends on Business layer

@foreach (var q in Model)
{
    <td>@q.Text</td>
    <td>@q.CourseName</td>
}
```

**Problems:**
- View directly references `AIVES.Business.DTOs` namespace
- Changing a DTO breaks every View that uses it
- Can't add UI-specific computed properties
- Tight coupling between Web and Business layers

### After (CORRECT - Clean Architecture)

```csharp
// Controller
public IActionResult Index()
{
    var dtos = _service.GetAllAsync();           // Returns DTOs
    var viewModel = dtos.Select(MapToIndex);     // Map to ViewModels
    return View(viewModel);                      // View gets ViewModels
}

// View (Index.cshtml)
@model IEnumerable<QuestionIndexViewModel>  // View depends only on Web layer

@foreach (var q in Model)
{
    <td>@q.Text</td>
    <td>@q.CourseName</td>
    <td><span class="badge @q.StatusBadge">@q.Status</span></td>
    <td><a href="@q.EditUrl">Edit</a></td>
}
```

**Benefits:**
- View only knows about `AIVES.Web.ViewModels`
- DTO changes don't affect Views (mapping absorbs the change)
- Can add UI helpers (badges, URLs, formatted fields) to ViewModels
- Clean dependency boundary between layers

---

## Quick Reference

| Question | Answer |
|----------|--------|
| Where do DTOs live? | `AIVES.Business.DTOs/` |
| Where do ViewModels live? | `AIVES.Web.ViewModels/` |
| Where is business logic? | `AIVES.Business.Services/` |
| Where is database access? | `AIVES.Data/` |
| Who maps DTOs to ViewModels? | Controllers |
| Who maps ViewModels to DTOs? | Controllers |
| What does the View see? | ViewModels only |
| What does the Service return? | DTOs only |
| What does the Entity look like? | Plain POCO, mirrors DB table |

---

*Generated for AIVES (AI-Powered Viva Examiner System)*
*Architecture: Clean Architecture (Onion Architecture)*
*Framework: ASP.NET Core MVC*