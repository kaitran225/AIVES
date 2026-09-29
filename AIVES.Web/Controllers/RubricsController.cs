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
            if (ValidateRubric(viewModel))
            {
                var dto = MapToCreateDto(viewModel);
                var rubric = await _rubricService.CreateRubricAsync(dto);
                TempData["Success"] = "Rubric created successfully.";
                return RedirectToAction(nameof(Details), new { id = rubric.Id });
            }

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
            if (ValidateRubric(viewModel))
            {
                var dto = MapToUpdateDto(viewModel);
                var rubric = await _rubricService.UpdateRubricAsync(dto);
                TempData["Success"] = "Rubric updated successfully.";
                return RedirectToAction(nameof(Details), new { id = rubric.Id });
            }

            return View(viewModel);
        }

        // POST: Rubrics/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            if (await _rubricService.GetRubricByIdAsync(id) == null)
            {
                TempData["Error"] = $"Rubric {id} no longer exists.";
                return RedirectToAction(nameof(Index));
            }

            await _rubricService.DeleteRubricAsync(id);
            TempData["Success"] = "Rubric deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
        // ===== Validation =====
        /// <summary>
        /// Validates a rubric and normalises its criteria. Blank rows the user
        /// may have added are dropped, and the criterion scores are required to
        /// add up to the rubric maximum score.
        /// </summary>
        private bool ValidateRubric(RubricFormViewModel viewModel)
        {
            viewModel.Name = viewModel.Name?.Trim() ?? string.Empty;

            viewModel.Criteria = (viewModel.Criteria ?? new List<RubricCriterionFormViewModel>())
                .Where(c => !string.IsNullOrWhiteSpace(c.Criterion))
                .ToList();

            foreach (var criterion in viewModel.Criteria)
            {
                criterion.Criterion = criterion.Criterion.Trim();
            }

            // An empty criterion input binds to null and trips the implicit
            // non-nullable "required" check during model binding. Blank rows are
            // dropped above, so their binder errors are stale - clear them and
            // rely on the explicit checks below.
            var staleKeys = ModelState.Keys
                .Where(k => k != null && k.StartsWith("Criteria[", StringComparison.OrdinalIgnoreCase))
                .ToList();

            foreach (var key in staleKeys)
            {
                ModelState.Remove(key);
            }

            if (string.IsNullOrWhiteSpace(viewModel.Name))
            {
                ModelState.AddModelError(nameof(viewModel.Name), "Rubric name is required.");
            }

            if (viewModel.MaxScore <= 0)
            {
                ModelState.AddModelError(nameof(viewModel.MaxScore), "Maximum score must be greater than 0.");
            }

            if (viewModel.Criteria.Count == 0)
            {
                ModelState.AddModelError("Criteria", "Add at least one criterion.");
                return false;
            }

            foreach (var criterion in viewModel.Criteria)
            {
                if (criterion.MaxScore <= 0)
                {
                    ModelState.AddModelError("Criteria",
                        $"Criterion \"{criterion.Criterion}\" needs a score greater than 0.");
                }
                else if (criterion.MaxScore > viewModel.MaxScore)
                {
                    ModelState.AddModelError("Criteria",
                        $"Criterion \"{criterion.Criterion}\" scores {criterion.MaxScore}, which exceeds the rubric maximum of {viewModel.MaxScore}.");
                }
            }

            var allocated = viewModel.Criteria.Sum(c => c.MaxScore);
            if (viewModel.MaxScore > 0 && allocated != viewModel.MaxScore)
            {
                var difference = allocated - viewModel.MaxScore;
                var detail = difference > 0
                    ? $"{difference} point(s) too many"
                    : $"{Math.Abs(difference)} point(s) missing";

                ModelState.AddModelError(nameof(viewModel.MaxScore),
                    $"The {viewModel.Criteria.Count} criteria allocate {allocated} of {viewModel.MaxScore} points ({detail}). Criterion scores must add up to the maximum score.");

                viewModel.ErrorMessage = "Criterion scores must add up to the rubric maximum score.";
            }

            return ModelState.IsValid;
        }

        // ===== Mapping Methods (DTO to ViewModel) =====
        private RubricIndexViewModel MapToIndex(RubricDto dto)
        {
            return new RubricIndexViewModel
            {
                Id = dto.Id,
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
                Id = dto.Id,
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
                Id = dto.Id,
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
