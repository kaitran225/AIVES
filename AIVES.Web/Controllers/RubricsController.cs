using Microsoft.AspNetCore.Mvc;
using AIVES.Business.Interfaces;
using AIVES.Business.DTOs;
using AIVES.Web.ViewModels;

namespace AIVES.Web.Controllers
{
    /// <summary>
        /// Controller for Rubric CRUD operations (matrix structure).
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
                var viewModel = new RubricCreateViewModel
                {
                    Step = 1,
                    MaxScore = 4,
                    CriteriaCount = 3
                };
                return View(viewModel);
            }

            // POST: Rubrics/Create - Step 1: Define dimensions
            [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> Create(RubricCreateViewModel viewModel)
            {
                if (viewModel.Step == 1)
                {
                    return HandleStep1(viewModel);
                }
                else
                {
                    return await HandleStep2(viewModel);
                }
            }

private IActionResult HandleStep1(RubricCreateViewModel viewModel)
        {
            viewModel.Name = viewModel.Name?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(viewModel.Name))
            {
                ModelState.AddModelError(nameof(viewModel.Name), "Rubric name is required.");
            }

            if (viewModel.MaxScore < 2)
            {
                ModelState.AddModelError(nameof(viewModel.MaxScore), "Max score must be at least 2.");
            }

            if (viewModel.CriteriaCount < 1)
            {
                ModelState.AddModelError(nameof(viewModel.CriteriaCount), "Criteria count must be at least 1.");
            }

            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            // Initialize criteria list with empty entries
            viewModel.Criteria = new List<RubricCriterionFormViewModel>();
            for (int i = 0; i < viewModel.CriteriaCount; i++)
            {
                viewModel.Criteria.Add(new RubricCriterionFormViewModel
                {
                    SortOrder = i,
                    LevelDescriptions = Enumerable.Range(1, viewModel.MaxScore).Select(l => new CriterionLevelDescriptionFormViewModel
                    {
                        PerformanceLevelId = l,
                        Description = ""
                    }).ToList()
                });
            }

            viewModel.Step = 2;
            return View(viewModel);
        }

        private async Task<IActionResult> HandleStep2(RubricCreateViewModel viewModel)
        {
            viewModel.Name = viewModel.Name?.Trim() ?? string.Empty;

            // Model binding should populate LevelLabels and CriterionNames automatically
            // But we need to ensure they match the expected counts
            if (viewModel.LevelLabels.Count != viewModel.MaxScore)
            {
                // Initialize with defaults if missing
                while (viewModel.LevelLabels.Count < viewModel.MaxScore)
                {
                    var level = viewModel.LevelLabels.Count + 1;
                    viewModel.LevelLabels.Add(level == 1 ? "Beginning" : level == 2 ? "Developing" : level == 3 ? "Proficient" : level == 4 ? "Exemplary" : "Level " + level);
                }
            }

            if (viewModel.CriterionNames.Count != viewModel.CriteriaCount)
            {
                // Initialize with empty strings if missing
                while (viewModel.CriterionNames.Count < viewModel.CriteriaCount)
                {
                    viewModel.CriterionNames.Add("");
                }
            }

            // Update criteria with names from CriterionNames
            for (int i = 0; i < viewModel.Criteria.Count; i++)
            {
                viewModel.Criteria[i].Criterion = viewModel.CriterionNames.Count > i ? viewModel.CriterionNames[i]?.Trim() ?? "" : "";
                if (string.IsNullOrWhiteSpace(viewModel.Criteria[i].Criterion))
                {
                    ModelState.AddModelError("CriterionNames", $"Criterion {i + 1} name is required.");
                }
            }

            // Bind descriptions - they should be in Criteria[row].LevelDescriptions[col].Description
            // Model binding handles this automatically if the form names match
            for (int row = 0; row < viewModel.Criteria.Count; row++)
            {
                for (int col = 0; col < viewModel.MaxScore; col++)
                {
                    if (viewModel.Criteria[row].LevelDescriptions.Count > col)
                    {
                        var desc = viewModel.Criteria[row].LevelDescriptions[col].Description?.Trim() ?? "";
                        if (string.IsNullOrWhiteSpace(desc))
                        {
                            ModelState.AddModelError("Matrix", $"Criterion {row + 1}, Level {col + 1} description is required.");
                        }
                    }
                }
            }

            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            // Create DTO and save
            var dto = MapToCreateDto(viewModel);
            return await SaveRubric(dto, viewModel);
        }

            private async Task<IActionResult> SaveRubric(RubricCreateDto dto, RubricFormViewModel viewModel)
            {
                try
                {
                    var rubric = await _rubricService.CreateRubricAsync(dto);
                    TempData["Success"] = "Rubric created successfully.";
                    return RedirectToAction(nameof(Details), new { id = rubric.Id });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error saving rubric: " + ex.Message);
                    return View(viewModel);
                }
            }

            // GET: Rubrics/Edit/5
            public async Task<IActionResult> Edit(int id)
            {
                var dto = await _rubricService.GetRubricByIdAsync(id);
                if (dto == null) return NotFound();
                var viewModel = MapToUpdateViewModel(dto);
                viewModel.Step = 2; // Skip to matrix edit
                return View(viewModel);
            }

            // POST: Rubrics/Edit/5
            [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> Edit(RubricUpdateViewModel viewModel)
            {
                if (!ModelState.IsValid)
                {
                    return View(viewModel);
                }

                var dto = MapToUpdateDto(viewModel);
                try
                {
                    var rubric = await _rubricService.UpdateRubricAsync(dto);
                    TempData["Success"] = "Rubric updated successfully.";
                    return RedirectToAction(nameof(Details), new { id = rubric.Id });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error updating rubric: " + ex.Message);
                    return View(viewModel);
                }
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
                PerformanceLevelsCount = dto.PerformanceLevels?.Count ?? 0
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
                PerformanceLevels = dto.PerformanceLevels?.Select(p => new PerformanceLevelDetailViewModel
                {
                    Id = p.Id,
                    Level = p.Level,
                    Label = p.Label,
                    SortOrder = p.SortOrder
                }).ToList() ?? new(),
                Criteria = dto.Criteria?.Select(c => new RubricCriterionDetailViewModel
                {
                    Id = c.Id,
                    Criterion = c.Criterion,
                    SortOrder = c.SortOrder,
                    LevelDescriptions = c.LevelDescriptions?.Select(d => new CriterionLevelDescriptionDetailViewModel
                    {
                        Id = d.Id,
                        PerformanceLevelId = d.PerformanceLevelId,
                        Description = d.Description
                    }).ToList() ?? new()
                }).ToList() ?? new()
            };
        }

        private RubricCreateDto MapToCreateDto(RubricCreateViewModel vm)
        {
            var levelLabels = vm.LevelLabels.Count > 0 ? vm.LevelLabels : new List<string>();
            var performanceLevels = new List<PerformanceLevelDto>();
            for (int i = 0; i < vm.MaxScore; i++)
            {
                performanceLevels.Add(new PerformanceLevelDto
                {
                    Level = i + 1,
                    Label = levelLabels.Count > i ? levelLabels[i] : $"Level {i + 1}",
                    SortOrder = i
                });
            }

            return new RubricCreateDto
            {
                Name = vm.Name,
                Description = vm.Description,
                MaxScore = vm.MaxScore,
                PerformanceLevels = performanceLevels,
                Criteria = vm.Criteria?.Select((c, idx) => new CriterionDto
                {
                    Id = c.Id,
                    Criterion = c.Criterion,
                    SortOrder = c.SortOrder,
                    LevelDescriptions = c.LevelDescriptions?.Select((d, dIdx) => new CriterionLevelDescriptionDto
                    {
                        Id = d.Id,
                        PerformanceLevelId = dIdx + 1, // Map by position
                        Description = d.Description
                    }).ToList() ?? new()
                }).ToList() ?? new()
            };
        }

        private RubricUpdateViewModel MapToUpdateViewModel(RubricDto dto)
        {
            var viewModel = new RubricUpdateViewModel
            {
                Id = dto.Id,
                Name = dto.Name,
                Description = dto.Description,
                MaxScore = dto.MaxScore,
                PerformanceLevels = dto.PerformanceLevels?.Select(p => new PerformanceLevelFormViewModel
                {
                    Id = p.Id,
                    Level = p.Level,
                    Label = p.Label,
                    SortOrder = p.SortOrder
                }).ToList() ?? new(),
                Criteria = dto.Criteria?.Select(c => new RubricCriterionFormViewModel
                {
                    Id = c.Id,
                    Criterion = c.Criterion,
                    SortOrder = c.SortOrder,
                    LevelDescriptions = c.LevelDescriptions?.Select(d => new CriterionLevelDescriptionFormViewModel
                    {
                        Id = d.Id,
                        PerformanceLevelId = d.PerformanceLevelId,
                        Description = d.Description
                    }).ToList() ?? new()
                }).ToList() ?? new(),
                Step = 2,
                CriteriaCount = dto.Criteria?.Count ?? 0,
                LevelLabels = dto.PerformanceLevels?.Select(p => p.Label).ToList() ?? new(),
                CriterionNames = dto.Criteria?.Select(c => c.Criterion).ToList() ?? new()
            };
            return viewModel;
        }

        private RubricUpdateDto MapToUpdateDto(RubricUpdateViewModel vm)
        {
            var levelLabels = vm.LevelLabels.Count > 0 ? vm.LevelLabels : new List<string>();
            var performanceLevels = new List<PerformanceLevelDto>();
            for (int i = 0; i < vm.MaxScore; i++)
            {
                performanceLevels.Add(new PerformanceLevelDto
                {
                    Id = vm.PerformanceLevels.Count > i ? vm.PerformanceLevels[i].Id : 0,
                    Level = i + 1,
                    Label = levelLabels.Count > i ? levelLabels[i] : $"Level {i + 1}",
                    SortOrder = i
                });
            }

            var criterionNames = vm.CriterionNames.Count > 0 ? vm.CriterionNames : new List<string>();

            return new RubricUpdateDto
            {
                Id = vm.Id,
                Name = vm.Name,
                Description = vm.Description,
                MaxScore = vm.MaxScore,
                PerformanceLevels = performanceLevels,
                Criteria = vm.Criteria?.Select((c, idx) => new CriterionDto
                {
                    Id = c.Id,
                    Criterion = criterionNames.Count > idx ? criterionNames[idx]?.Trim() ?? c.Criterion : c.Criterion,
                    SortOrder = c.SortOrder,
                    LevelDescriptions = c.LevelDescriptions?.Select((d, dIdx) => new CriterionLevelDescriptionDto
                    {
                        Id = d.Id,
                        PerformanceLevelId = dIdx + 1,
                        Description = d.Description
                    }).ToList() ?? new()
                }).ToList() ?? new()
            };
        }
    }
}