using Microsoft.AspNetCore.Mvc;
using AIVES.Business.Interfaces;
using AIVES.Business.DTOs;
using AIVES.Web.ViewModels;

namespace AIVES.Web.Controllers
{
    /// <summary>
    /// Controller for Course Material CRUD operations.
    /// Maps Business DTOs → Web ViewModels at the boundary.
    /// </summary>
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
            IEnumerable<CourseMaterialDto> dtos;
            if (courseId.HasValue)
            {
                dtos = await _materialService.GetMaterialsByCourseAsync(courseId.Value);
            }
            else
            {
                dtos = await _materialService.GetAllMaterialsAsync();
            }
            var viewModel = dtos.Select(MapToIndex).ToList();
            ViewData["CourseId"] = courseId;
            return View(viewModel);
        }

        // GET: CourseMaterials/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var dto = await _materialService.GetMaterialByIdAsync(id);
            if (dto == null) return NotFound();
            var viewModel = MapToDetail(dto);
            return View(viewModel);
        }

        // GET: CourseMaterials/Create
        public IActionResult Create(int? courseId)
        {
            var viewModel = new CourseMaterialCreateViewModel { CourseId = courseId ?? 0 };
            ViewData["CourseId"] = courseId;
            return View(viewModel);
        }

        // POST: CourseMaterials/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CourseMaterialCreateViewModel viewModel, IFormFile? file)
        {
            if (ModelState.IsValid)
            {
                var dto = MapToCreateDto(viewModel);
                if (file != null && file.Length > 0)
                {
                    dto.FileUpload = file;
                }
                var result = await _materialService.ImportMaterialAsync(dto);
                TempData["Success"] = result.Message;
                return RedirectToAction(nameof(Details), new { id = result.MaterialId });
            }
            ViewData["CourseId"] = viewModel.CourseId;
            return View(viewModel);
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
            var dto = await _materialService.GetMaterialByIdAsync(id);
            if (dto == null) return NotFound();
            var viewModel = MapToDetail(dto);
            return View(viewModel);
        }

        // ===== Mapping Methods (DTO ↔ ViewModel) =====
        private CourseMaterialIndexViewModel MapToIndex(CourseMaterialDto dto)
        {
            return new CourseMaterialIndexViewModel
            {
                Id = dto.Id,
                Title = dto.Title,
                CourseName = dto.CourseName,
                SourceType = dto.SourceType,
                FileName = dto.FileName,
                ChunkCount = dto.ChunkCount,
                ImportedAt = dto.ImportedAt
            };
        }

        private CourseMaterialDetailViewModel MapToDetail(CourseMaterialDto dto)
        {
            return new CourseMaterialDetailViewModel
            {
                Id = dto.Id,
                Title = dto.Title,
                Content = dto.Content,
                CourseName = dto.CourseName,
                SourceType = dto.SourceType,
                FileName = dto.FileName,
                ChunkCount = dto.ChunkCount,
                ImportedAt = dto.ImportedAt
            };
        }

        private CourseMaterialCreateDto MapToCreateDto(CourseMaterialCreateViewModel vm)
        {
            return new CourseMaterialCreateDto
            {
                Title = vm.Title,
                Content = vm.Content,
                CourseId = vm.CourseId,
                SourceType = vm.SourceType,
                FileName = vm.FileName
            };
        }
    }
}
