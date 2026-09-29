using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using AIVES.Business.Interfaces;
using AIVES.Business.DTOs;
using AIVES.Web.ViewModels;

namespace AIVES.Web.Controllers
{
    /// <summary>
    /// Controller for Question CRUD operations.
    /// Maps Business DTOs → Web ViewModels at the boundary.
    /// </summary>
    public class QuestionsController : Controller
    {
        private readonly IQuestionService _questionService;
        private readonly IRubricService _rubricService;

        public QuestionsController(IQuestionService questionService, IRubricService rubricService)
        {
            _questionService = questionService;
            _rubricService = rubricService;
        }

        private async Task PopulateDropdownsAsync(QuestionCreateViewModel viewModel)
        {
            var courses = await _questionService.GetAllCoursesAsync();
            var topics = await _questionService.GetAllTopicsAsync();
            var rubrics = await _rubricService.GetAllRubricsAsync();

            viewModel.Courses = courses.Select(c => new SelectListItem($"{c.Code} - {c.Name}", c.Id.ToString())).ToList();
            viewModel.Topics = topics.Select(t => new SelectListItem(t.Name, t.Id.ToString())).ToList();
            viewModel.Rubrics = rubrics.Select(r => new SelectListItem(r.Name, r.Id.ToString())).ToList();
        }

        // GET: Questions
        public async Task<IActionResult> Index()
        {
            var dtos = await _questionService.GetAllQuestionsAsync();
            var viewModel = MapToIndexList(dtos);
            return View(viewModel);
        }

        // GET: Questions/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var dto = await _questionService.GetQuestionByIdAsync(id);
            if (dto == null) return NotFound();
            var viewModel = MapToDetail(dto);
            return View(viewModel);
        }

        // GET: Questions/Create
        public async Task<IActionResult> Create()
        {
            var viewModel = new QuestionCreateViewModel();
            await PopulateDropdownsAsync(viewModel);
            return View(viewModel);
        }

        // POST: Questions/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(QuestionCreateViewModel viewModel)
        {
            var courses = await _questionService.GetAllCoursesAsync();
            var courseIds = courses.Select(c => c.Id).ToHashSet();
            if (!courseIds.Contains(viewModel.CourseId))
            {
                ModelState.AddModelError("CourseId", "Invalid course selection.");
            }

            if (ModelState.IsValid)
            {
                var dto = MapToCreateDto(viewModel);
                var question = await _questionService.CreateQuestionAsync(dto);
                TempData["Success"] = "Question created successfully.";
                return RedirectToAction(nameof(Details), new { id = question.Id });
            }
            await PopulateDropdownsAsync(viewModel);
            return View(viewModel);
        }

        // GET: Questions/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var dto = await _questionService.GetQuestionByIdAsync(id);
            if (dto == null) return NotFound();
            var viewModel = MapToUpdateViewModel(dto);
            return View(viewModel);
        }

        // POST: Questions/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(QuestionUpdateViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var dto = MapToUpdateDto(viewModel);
                await _questionService.UpdateQuestionAsync(dto);
                TempData["Success"] = "Question updated successfully.";
                return RedirectToAction(nameof(Details), new { id = dto.Id });
            }
            return View(viewModel);
        }

        // POST: Questions/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await _questionService.DeleteQuestionAsync(id);
            TempData["Success"] = "Question deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Questions/Import
        public IActionResult Import()
        {
            return View();
        }

        // POST: Questions/Import
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Import(string content, int courseId)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                TempData["Error"] = "Please enter questions to import.";
                return View();
            }

            var questions = await _questionService.ImportQuestionsAsync(content, courseId);
            TempData["Success"] = $"{questions.Count} questions imported successfully.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Questions/AI-Generate
        public IActionResult AIGenerate()
        {
            return View(new AIGenerateViewModel());
        }

        // POST: Questions/AI-Generate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AIGenerate(AIGenerateViewModel viewModel)
        {
            if (string.IsNullOrWhiteSpace(viewModel.CourseCode) || string.IsNullOrWhiteSpace(viewModel.Topic))
            {
                viewModel.ErrorMessage = "Please provide course code and topic.";
                return View(viewModel);
            }

            var result = await _questionService.GenerateQuestionFromMaterialAsync(
                viewModel.CourseCode, viewModel.Topic, viewModel.BloomLevel);
            viewModel.GeneratedQuestion = result.GeneratedQuestion;
            viewModel.ReferenceAnswer = result.ReferenceAnswer;
            viewModel.HasResult = true;
            return View(viewModel);
        }

        // POST: Questions/Review
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Review(QuestionReviewDto dto)
        {
            await _questionService.ReviewQuestionAsync(dto);
            TempData["Success"] = $"Question {dto.Status.ToLower()}d successfully.";
            return RedirectToAction(nameof(Details), new { id = dto.QuestionId });
        }

        // GET: Questions/Pending
        public async Task<IActionResult> Pending()
        {
            var dtos = await _questionService.GetPendingQuestionsAsync();
            var viewModel = MapToIndexList(dtos);
            return View(viewModel);
        }

        // GET: Questions/ByCourse/5
        public async Task<IActionResult> ByCourse(int courseId)
        {
            var dtos = await _questionService.GetQuestionsByCourseAsync(courseId);
            var viewModel = MapToIndexList(dtos);
            return View("Index", viewModel);
        }

        // ===== Mapping Methods (DTO → ViewModel) =====
        private List<QuestionIndexViewModel> MapToIndexList(IEnumerable<QuestionDto> dtos)
        {
            return dtos.Select(MapToIndex).ToList();
        }

        private QuestionIndexViewModel MapToIndex(QuestionDto dto)
        {
            return new QuestionIndexViewModel
            {
                
                Text = dto.Text,
                BloomLevel = dto.BloomLevel,
                Status = dto.Status,
                SourceType = dto.SourceType,
                CourseName = dto.CourseName,
                TopicName = dto.TopicName,
                RubricName = dto.RubricName,
                CreatedAt = dto.CreatedAt
            };
        }

        private QuestionDetailViewModel MapToDetail(QuestionDto dto)
        {
            return new QuestionDetailViewModel
            {
                
                Text = dto.Text,
                ReferenceAnswer = dto.ReferenceAnswer,
                BloomLevel = dto.BloomLevel,
                Status = dto.Status,
                SourceType = dto.SourceType,
                ReferenceMaterial = dto.ReferenceMaterial,
                CreatedAt = dto.CreatedAt,
                CourseId = dto.CourseId,
                CourseName = dto.CourseName,
                TopicId = dto.TopicId,
                TopicName = dto.TopicName,
                RubricId = dto.RubricId,
                RubricName = dto.RubricName
            };
        }

        private QuestionCreateViewModel MapToCreateViewModel(QuestionCreateDto dto)
        {
            return new QuestionCreateViewModel
            {
                
                Text = dto.Text,
                ReferenceAnswer = dto.ReferenceAnswer,
                BloomLevel = dto.BloomLevel,
                SourceType = dto.SourceType,
                ReferenceMaterial = dto.ReferenceMaterial,
                CourseId = dto.CourseId,
                TopicId = dto.TopicId,
                RubricId = dto.RubricId
            };
        }

        private QuestionCreateDto MapToCreateDto(QuestionCreateViewModel vm)
        {
            return new QuestionCreateDto
            {
                Text = vm.Text,
                ReferenceAnswer = vm.ReferenceAnswer,
                BloomLevel = vm.BloomLevel,
                SourceType = vm.SourceType,
                ReferenceMaterial = vm.ReferenceMaterial,
                CourseId = vm.CourseId,
                TopicId = vm.TopicId,
                RubricId = vm.RubricId
            };
        }

        private QuestionUpdateViewModel MapToUpdateViewModel(QuestionDto dto)
        {
            return new QuestionUpdateViewModel
            {
                
                Text = dto.Text,
                ReferenceAnswer = dto.ReferenceAnswer,
                BloomLevel = dto.BloomLevel,
                Status = dto.Status,
                CourseId = dto.CourseId,
                TopicId = dto.TopicId,
                RubricId = dto.RubricId
            };
        }

        private QuestionUpdateDto MapToUpdateDto(QuestionUpdateViewModel vm)
        {
            return new QuestionUpdateDto
            {
                Id = vm.Id,
                Text = vm.Text,
                ReferenceAnswer = vm.ReferenceAnswer,
                BloomLevel = vm.BloomLevel,
                Status = vm.Status,
                CourseId = vm.CourseId,
                TopicId = vm.TopicId,
                RubricId = vm.RubricId
            };
        }
    }
}
