using Microsoft.AspNetCore.Mvc;
using AIVES.Business.Interfaces;
using AIVES.Business.DTOs;

namespace AIVES.Web.Controllers
{
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
            var rubrics = await _rubricService.GetAllRubricsAsync();
            return View(rubrics);
        }

        // GET: Rubrics/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var rubric = await _rubricService.GetRubricByIdAsync(id);
            if (rubric == null) return NotFound();
            return View(rubric);
        }

        // GET: Rubrics/Create
        public IActionResult Create()
        {
            return View(new RubricCreateDto());
        }

        // POST: Rubrics/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RubricCreateDto dto)
        {
            if (ModelState.IsValid)
            {
                var rubric = await _rubricService.CreateRubricAsync(dto);
                TempData["Success"] = "Rubric created successfully.";
                return RedirectToAction(nameof(Details), new { id = rubric.Id });
            }
            return View(dto);
        }

        // GET: Rubrics/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var rubric = await _rubricService.GetRubricByIdAsync(id);
            if (rubric == null) return NotFound();
            var dto = new RubricUpdateDto
            {
                Id = rubric.Id,
                Name = rubric.Name,
                Description = rubric.Description,
                MaxScore = rubric.MaxScore,
                Criteria = rubric.Criteria.Select(c => new CriterionDto
                {
                    Id = c.Id,
                    Criterion = c.Criterion,
                    Description = c.Description,
                    MaxScore = c.MaxScore,
                    ScoringGuidance = c.ScoringGuidance
                }).ToList()
            };
            return View(dto);
        }

        // POST: Rubrics/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(RubricUpdateDto dto)
        {
            if (ModelState.IsValid)
            {
                var rubric = await _rubricService.UpdateRubricAsync(dto);
                TempData["Success"] = "Rubric updated successfully.";
                return RedirectToAction(nameof(Details), new { id = rubric.Id });
            }
            return View(dto);
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
    }
}
