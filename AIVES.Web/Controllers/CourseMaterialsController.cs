using Microsoft.AspNetCore.Mvc;
using AIVES.Business.Interfaces;
using AIVES.Business.DTOs;

namespace AIVES.Web.Controllers
{
    public class CourseMaterialsController : Controller
    {
        private readonly ICourseMaterialService _materialService;

        public CourseMaterialsController(ICourseMaterialService materialService)
        {
            _materialService = materialService;
        }

        // GET: CourseMaterials
        public async Task<IActionResult> Index(int? courseId)
        {
            IEnumerable<CourseMaterialDto> materials;
            if (courseId.HasValue)
            {
                materials = await _materialService.GetMaterialsByCourseAsync(courseId.Value);
            }
            else
            {
                materials = await _materialService.GetAllMaterialsAsync();
            }
            ViewData["CourseId"] = courseId;
            return View(materials);
        }

        // GET: CourseMaterials/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var material = await _materialService.GetMaterialByIdAsync(id);
            if (material == null) return NotFound();
            return View(material);
        }

        // GET: CourseMaterials/Create
        public IActionResult Create(int? courseId)
        {
            var dto = new CourseMaterialCreateDto { CourseId = courseId ?? 0 };
            ViewData["CourseId"] = courseId;
            return View(dto);
        }

        // POST: CourseMaterials/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CourseMaterialCreateDto dto, IFormFile? file)
        {
            if (ModelState.IsValid)
            {
                // Use file content if provided
                if (file != null && file.Length > 0)
                {
                    dto.FileUpload = file;
                }

                var result = await _materialService.ImportMaterialAsync(dto);
                TempData["Success"] = result.Message;
                return RedirectToAction(nameof(Details), new { id = result.MaterialId });
            }
            ViewData["CourseId"] = dto.CourseId;
            return View(dto);
        }

        // POST: CourseMaterials/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await _materialService.DeleteMaterialAsync(id);
            TempData["Success"] = "Material deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        // GET: CourseMaterials/Preview/5
        public async Task<IActionResult> Preview(int id)
        {
            var material = await _materialService.GetMaterialByIdAsync(id);
            if (material == null) return NotFound();
            return View(material);
        }
    }
}
