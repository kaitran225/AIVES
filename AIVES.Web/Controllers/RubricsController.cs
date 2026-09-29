using Microsoft.AspNetCore.Mvc;
using AIVES.Business.Interfaces;
using AIVES.Business.DTOs;
using AIVES.Web.ViewModels;

namespace AIVES.Web.Controllers
{
    /// <summary>
    /// Controller for Rubric CRUD operations.
    /// Maps Business DTOs to Web ViewModels at the boundary.
    /// </summary>
    public class RubricsController : Controller
    {
        private readonly IRubricService _rubricService;

        public RubricsController(IRubricService rubricService)
        {
            _rubricService = rubricService;
        }

        // GET: Rubrics
        public async Task<IActionResult> Index()
        {
            var dtos = await _rubricService.GetAllRubricsAsync();
            var viewModel = dtos.Select(MapToIndex).ToList();
            return View(viewModel);
        }

        // GET: Rubrics/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var dto = await _rubricService.GetRubricByIdAsync(id);
            if (dto == null) return NotFound();
            var viewModel = MapToDetail(dto);
            return View(viewModel);
        }

        // GET: Rubrics/Create
        public IActionResult Create()
        {
            return View(new RubricCreateViewModel());
        }

        // POST: Rubrics/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RubricCreateViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var dto = MapToCreateDto(viewModel);
                var rubric = await _rubricService.CreateRubricAsync(dto);
                TempData["Success"] = "Rubric created successfully.";
                return RedirectToAction(nameof(Details), new { id = rubric.Id });
            }
            viewModel.ErrorMessage = "Invalid data. Please check your input.";
            return View(viewModel);
        }

        // GET: Rubrics/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var dto = await _rubricService.GetRubricByIdAsync(id);
            if (dto == null) return NotFound();
            var viewModel = MapToUpdateViewModel(dto);
            return View(viewModel);
        }

        // POST: Rubrics/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(RubricUpdateViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var dto = MapToUpdateDto(viewModel);
                var rubric = await _rubricService.UpdateRubricAsync(dto);
                TempData["Success"] = "Rubric updated successfully.";
                return RedirectToAction(nameof(Details), new { id = rubric.Id });
            }
            viewModel.ErrorMessage = "Invalid data. Please check your input.";
            return View(viewModel);
        }

        // POST: Rubrics/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await _rubricService.DeleteRubricAsync(id);
            TempData["Success"] = "Rubric deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
        // ===== Mapping Methods (DTO to ViewModel) =====
        private RubricIndexViewModel MapToIndex(RubricDto dto)
        {
            return new RubricIndexViewModel
            {
                
                Name = dto.Name,
                Description = dto.Description,
                MaxScore = dto.MaxScore,
                CriteriaCount = dto.Criteria?.Count ?? 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        private RubricDetailViewModel MapToDetail(RubricDto dto)
        {
            return new RubricDetailViewModel
            {
                
                Name = dto.Name,
                Description = dto.Description,
                MaxScore = dto.MaxScore,
                Criteria = dto.Criteria?.Select(c => new RubricCriterionDetailViewModel
                {
                    Id = c.Id,
                    Criterion = c.Criterion,
                    Description = c.Description,
                    MaxScore = c.MaxScore,
                    ScoringGuidance = c.ScoringGuidance
                }).ToList() ?? new(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        private RubricCreateViewModel MapToCreateViewModel(RubricCreateDto dto)
        {
            return new RubricCreateViewModel
            {
                
                Name = dto.Name,
                Description = dto.Description,
                MaxScore = dto.MaxScore,
                Criteria = dto.Criteria?.Select(c => new RubricCriterionFormViewModel
                {
                    Id = c.Id,
                    Criterion = c.Criterion,
                    Description = c.Description,
                    MaxScore = c.MaxScore,
                    ScoringGuidance = c.ScoringGuidance
                }).ToList() ?? new()
            };
        }

        private RubricCreateDto MapToCreateDto(RubricCreateViewModel vm)
        {
            return new RubricCreateDto
            {
                Name = vm.Name,
                Description = vm.Description,
                MaxScore = vm.MaxScore,
                Criteria = vm.Criteria?.Select(c => new CriterionDto
                {
                    Id = c.Id,
                    Criterion = c.Criterion,
                    Description = c.Description,
                    MaxScore = c.MaxScore,
                    ScoringGuidance = c.ScoringGuidance
                }).ToList() ?? new()
            };
        }

        private RubricUpdateViewModel MapToUpdateViewModel(RubricDto dto)
        {
            return new RubricUpdateViewModel
            {
                
                Name = dto.Name,
                Description = dto.Description,
                MaxScore = dto.MaxScore,
                Criteria = dto.Criteria?.Select(c => new RubricCriterionFormViewModel
                {
                    Id = c.Id,
                    Criterion = c.Criterion,
                    Description = c.Description,
                    MaxScore = c.MaxScore,
                    ScoringGuidance = c.ScoringGuidance
                }).ToList() ?? new()
            };
        }

        private RubricUpdateDto MapToUpdateDto(RubricUpdateViewModel vm)
        {
            return new RubricUpdateDto
            {
                Id = vm.Id,
                Name = vm.Name,
                Description = vm.Description,
                MaxScore = vm.MaxScore,
                Criteria = vm.Criteria?.Select(c => new CriterionDto
                {
                    Id = c.Id,
                    Criterion = c.Criterion,
                    Description = c.Description,
                    MaxScore = c.MaxScore,
                    ScoringGuidance = c.ScoringGuidance
                }).ToList() ?? new()
            };
        }
    }
}
