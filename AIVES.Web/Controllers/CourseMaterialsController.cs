using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using AIVES.Business.Interfaces;
using AIVES.Business.DTOs;
using AIVES.Web.ViewModels;

namespace AIVES.Web.Controllers
{
    /// <summary>
    /// Controller for Course Material CRUD operations, and the single place where
    /// courses and their topics are created, edited and deleted.
    /// Maps Business DTOs → Web ViewModels at the boundary.
    /// </summary>
    public class CourseMaterialsController : Controller
    {
        private readonly ICourseMaterialService _materialService;
        private readonly ICourseService _courseService;
        private readonly IQuestionService _questionService;

        public CourseMaterialsController(
            ICourseMaterialService materialService,
            ICourseService courseService,
            IQuestionService questionService)
        {
            _materialService = materialService;
            _courseService = courseService;
            _questionService = questionService;
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
            return View("Preview", viewModel);
        }

        // GET: CourseMaterials/Create
        public async Task<IActionResult> Create(int? courseId)
        {
            var viewModel = new CourseMaterialCreateViewModel { CourseId = courseId ?? 0 };
            await PopulateCoursesAsync(viewModel);
            ViewData["CourseId"] = courseId;
            return View(viewModel);
        }

        // POST: CourseMaterials/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CourseMaterialCreateViewModel viewModel, IFormFile? file)
        {
            if (!await CourseExistsAsync(viewModel.CourseId))
                ModelState.AddModelError("CourseId", "Please select a valid course.");

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
            await PopulateCoursesAsync(viewModel);
            return View(viewModel);
        }

        // POST: CourseMaterials/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            if (await _materialService.GetMaterialByIdAsync(id) == null)
            {
                TempData["Error"] = $"Material {id} no longer exists.";
                return RedirectToAction(nameof(Index));
            }

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

        // ==================== Course & Topic Manager ====================

        // GET: CourseMaterials/Manage
        public async Task<IActionResult> Manage()
        {
            var courses = (await _courseService.GetAllCoursesAsync()).ToList();
            var topics = (await _courseService.GetAllTopicsAsync()).ToList();
            var questions = (await _questionService.GetAllQuestionsAsync()).ToList();
            var materials = (await _materialService.GetAllMaterialsAsync()).ToList();

            var viewModel = new CourseManagerViewModel
            {
                Courses = courses.Select(c => new CourseWithTopicsViewModel
                {
                    Id = c.Id,
                    Code = c.Code,
                    Name = c.Name,
                    Description = c.Description,
                    QuestionCount = questions.Count(q => q.CourseId == c.Id),
                    MaterialCount = materials.Count(m => m.CourseId == c.Id),
                    Topics = topics
                        .Where(t => t.CourseId == c.Id)
                        .Select(t => new TopicRowViewModel
                        {
                            Id = t.Id,
                            Name = t.Name,
                            Description = t.Description,
                            QuestionCount = questions.Count(q => q.TopicId == t.Id)
                        })
                        .ToList()
                }).ToList()
            };

            return View(viewModel);
        }

        // GET: CourseMaterials/CourseCreate
        public IActionResult CourseCreate()
        {
            return View(new CourseFormViewModel());
        }

        // POST: CourseMaterials/CourseCreate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CourseCreate(CourseFormViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var course = await _courseService.CreateCourseAsync(new CourseCreateDto
                    {
                        Code = viewModel.Code,
                        Name = viewModel.Name,
                        Description = viewModel.Description,
                        InitialTopics = viewModel.InitialTopics
                    });

                    var topicCount = (await _courseService.GetTopicsByCourseAsync(course.Id)).Count();
                    TempData["Success"] = $"Course {course.Code} created with {topicCount} topic(s).";
                    return RedirectToAction(nameof(Manage));
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError("Code", ex.Message);
                }
            }

            return View(viewModel);
        }

        // GET: CourseMaterials/CourseEdit/3
        public async Task<IActionResult> CourseEdit(int id)
        {
            var course = await _courseService.GetCourseByIdAsync(id);
            if (course == null) return NotFound();

            return View(new CourseFormViewModel
            {
                Id = course.Id,
                Code = course.Code,
                Name = course.Name,
                Description = course.Description
            });
        }

        // POST: CourseMaterials/CourseEdit/3
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CourseEdit(CourseFormViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var course = await _courseService.UpdateCourseAsync(new CourseUpdateDto
                    {
                        Id = viewModel.Id,
                        Code = viewModel.Code,
                        Name = viewModel.Name,
                        Description = viewModel.Description
                    });

                    TempData["Success"] = $"Course {course.Code} updated.";
                    return RedirectToAction(nameof(Manage));
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError("Code", ex.Message);
                }
                catch (KeyNotFoundException)
                {
                    return NotFound();
                }
            }

            return View(viewModel);
        }

        // POST: CourseMaterials/CourseDelete/3
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CourseDelete(int id)
        {
            var result = await _courseService.DeleteCourseAsync(id);

            if (result.Success)
            {
                TempData["Success"] = result.Message;
            }
            else
            {
                TempData["Error"] = result.Blockers.Count > 0
                    ? $"{result.Message} Still referenced by: {string.Join(", ", result.Blockers)}."
                    : result.Message;
            }

            return RedirectToAction(nameof(Manage));
        }

        // GET: CourseMaterials/TopicCreate?courseId=3
        [HttpGet("~/CourseMaterials/TopicCreate")]
        [HttpGet("~/CourseMaterials/TopicCreate/{courseId:int}")]
        public async Task<IActionResult> TopicCreate(int courseId = 0)
        {
            var viewModel = new TopicFormViewModel { CourseId = courseId };
            await PopulateCourseOptionsAsync(viewModel);
            await SetCourseLabelAsync(viewModel);
            return View(viewModel);
        }

        // POST: CourseMaterials/TopicCreate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TopicCreate(TopicFormViewModel viewModel)
        {
            await PopulateCourseOptionsAsync(viewModel);
            await SetCourseLabelAsync(viewModel);

            if (ModelState.IsValid)
            {
                try
                {
                    var topic = await _courseService.CreateTopicAsync(new QuestionTopicCreateDto
                    {
                        Name = viewModel.Name,
                        Description = viewModel.Description,
                        CourseId = viewModel.CourseId
                    });

                    TempData["Success"] = $"Topic '{topic.Name}' added.";
                    return RedirectToAction(nameof(Manage));
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError("Name", ex.Message);
                }
            }

            return View(viewModel);
        }

        // GET: CourseMaterials/TopicEdit/5
        public async Task<IActionResult> TopicEdit(int id)
        {
            var topic = await _courseService.GetTopicByIdAsync(id);
            if (topic == null) return NotFound();

            var viewModel = new TopicFormViewModel
            {
                Id = topic.Id,
                CourseId = topic.CourseId,
                Name = topic.Name,
                Description = topic.Description
            };

            await PopulateCourseOptionsAsync(viewModel);
            await SetCourseLabelAsync(viewModel);
            return View(viewModel);
        }

        // POST: CourseMaterials/TopicEdit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TopicEdit(TopicFormViewModel viewModel)
        {
            await PopulateCourseOptionsAsync(viewModel);
            await SetCourseLabelAsync(viewModel);

            if (ModelState.IsValid)
            {
                try
                {
                    var topic = await _courseService.UpdateTopicAsync(new QuestionTopicUpdateDto
                    {
                        Id = viewModel.Id,
                        Name = viewModel.Name,
                        Description = viewModel.Description,
                        CourseId = viewModel.CourseId
                    });

                    TempData["Success"] = $"Topic '{topic.Name}' updated.";
                    return RedirectToAction(nameof(Manage));
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError("Name", ex.Message);
                }
                catch (KeyNotFoundException)
                {
                    return NotFound();
                }
            }

            return View(viewModel);
        }

        // POST: CourseMaterials/TopicDelete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TopicDelete(int id)
        {
            var result = await _courseService.DeleteTopicAsync(id);

            if (result.Success)
            {
                TempData["Success"] = result.Message;
            }
            else
            {
                TempData["Error"] = result.Blockers.Count > 0
                    ? $"{result.Message} Still referenced by: {string.Join(", ", result.Blockers)}."
                    : result.Message;
            }

            return RedirectToAction(nameof(Manage));
        }

        // ===== Helpers =====
        private async Task PopulateCoursesAsync(CourseMaterialCreateViewModel viewModel)
        {
            var courses = await _courseService.GetAllCoursesAsync();
            viewModel.Courses = courses
                .Select(c => new SelectListItem($"{c.Code} - {c.Name}", c.Id.ToString()))
                .ToList();
        }

        private async Task PopulateCourseOptionsAsync(TopicFormViewModel viewModel)
        {
            var courses = await _courseService.GetAllCoursesAsync();
            viewModel.Courses = courses
                .Select(c => new SelectListItem($"{c.Code} - {c.Name}", c.Id.ToString()))
                .ToList();
        }

        private async Task SetCourseLabelAsync(TopicFormViewModel viewModel)
        {
            if (viewModel.CourseId <= 0) return;
            var course = await _courseService.GetCourseByIdAsync(viewModel.CourseId);
            if (course != null) viewModel.CourseLabel = $"{course.Code} - {course.Name}";
        }

        private async Task<bool> CourseExistsAsync(int courseId)
        {
            if (courseId <= 0) return false;
            return await _courseService.GetCourseByIdAsync(courseId) != null;
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
