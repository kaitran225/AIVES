using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using AIVES.Business.Interfaces;
using AIVES.Business.DTOs;
using AIVES.Web.ViewModels;
using System.Text.RegularExpressions;

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
        private readonly ICourseService _courseService;

        public QuestionsController(
            IQuestionService questionService,
            IRubricService rubricService,
            ICourseService courseService)
        {
            _questionService = questionService;
            _rubricService = rubricService;
            _courseService = courseService;
        }

        private async Task PopulateDropdownsAsync(QuestionFormViewModel viewModel)
        {
            var courses = await _courseService.GetAllCoursesAsync();
            var rubrics = await _rubricService.GetAllRubricsAsync();

            viewModel.Courses = courses.Select(c => new SelectListItem($"{c.Code} - {c.Name}", c.Id.ToString())).ToList();
            viewModel.Rubrics = rubrics.Select(r => new SelectListItem(r.Name, r.Id.ToString())).ToList();

            // Topics belong to a course, so they are only loaded once a course is
            // known. The view re-loads them via GetTopicsByCourse when it changes.
            viewModel.Topics = viewModel.CourseId > 0
                ? await GetTopicItemsAsync(viewModel.CourseId)
                : new List<SelectListItem>();
        }

        private async Task<List<SelectListItem>> GetTopicItemsAsync(int courseId)
        {
            var topics = await _courseService.GetTopicsByCourseAsync(courseId);
            return topics.Select(t => new SelectListItem(t.Name, t.Id.ToString())).ToList();
        }

        // GET: Questions
        public async Task<IActionResult> Index(int? courseId)
        {
            var dtos = courseId.HasValue ? await _questionService.GetQuestionsByCourseAsync(courseId.Value) : await _questionService.GetAllQuestionsAsync();
            var courses = await _courseService.GetAllCoursesAsync();
            
            QuestionBankViewModel viewModel;
            if (courseId.HasValue)
            {
                viewModel = QuestionBankViewModel.ByCourse(MapToIndexList(dtos), courses, courseId.Value);
            }
            else
            {
                viewModel = QuestionBankViewModel.AllQuestions(MapToIndexList(dtos), courses);
            }
            
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
            var courses = await _courseService.GetAllCoursesAsync();
            var courseIds = courses.Select(c => c.Id).ToHashSet();
            if (!courseIds.Contains(viewModel.CourseId))
            {
                ModelState.AddModelError("CourseId", "Invalid course selection.");
            }
            else if (viewModel.TopicId is > 0 && !await TopicBelongsToCourseAsync(viewModel.TopicId.Value, viewModel.CourseId))
            {
                ModelState.AddModelError("TopicId", "The selected topic does not belong to the selected course.");
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
            await PopulateDropdownsAsync(viewModel);
            return View(viewModel);
        }

        // POST: Questions/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(QuestionUpdateViewModel viewModel)
        {
            if (viewModel.TopicId is > 0 && !await TopicBelongsToCourseAsync(viewModel.TopicId.Value, viewModel.CourseId))
            {
                ModelState.AddModelError("TopicId", "The selected topic does not belong to the selected course.");
            }

            if (ModelState.IsValid)
            {
                var dto = MapToUpdateDto(viewModel);
                await _questionService.UpdateQuestionAsync(dto);
                TempData["Success"] = "Question updated successfully.";
                return RedirectToAction(nameof(Details), new { id = dto.Id });
            }
            await PopulateDropdownsAsync(viewModel);
            return View(viewModel);
        }

        // POST: Questions/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            if (await _questionService.GetQuestionByIdAsync(id) == null)
            {
                TempData["Error"] = $"Question {id} no longer exists.";
                return RedirectToAction(nameof(Index));
            }

            await _questionService.DeleteQuestionAsync(id);
            TempData["Success"] = "Question deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Questions/Import
        public async Task<IActionResult> Import()
        {
            var viewModel = new QuestionCreateViewModel();
            await PopulateDropdownsAsync(viewModel);
            return View(viewModel);
        }

        // POST: Questions/Import
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Import(string content, int courseId)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                TempData["Error"] = "Please enter questions to import.";
                var viewModel = new QuestionCreateViewModel { CourseId = courseId };
                await PopulateDropdownsAsync(viewModel);
                return View(viewModel);
            }

            if (!await CourseExistsAsync(courseId))
            {
                TempData["Error"] = "Please select a valid course before importing.";
                var viewModel = new QuestionCreateViewModel { CourseId = 0 };
                await PopulateDropdownsAsync(viewModel);
                return View(viewModel);
            }

            var questions = await _questionService.ImportQuestionsAsync(content, courseId);
            TempData["Success"] = $"{questions.Count} questions imported successfully.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Questions/AI-Generate
        public async Task<IActionResult> AIGenerate()
        {
            var viewModel = new AIGenerateViewModel();
            await PopulateAIGenerateDropdownsAsync(viewModel);
            return View(viewModel);
        }

        // POST: Questions/AI-Generate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AIGenerate(AIGenerateViewModel viewModel)
        {
            await PopulateAIGenerateDropdownsAsync(viewModel);

            // Validate course is selected before allowing topic selection
            if (viewModel.CourseId <= 0)
            {
                viewModel.ErrorMessage = "Please select a course first to load topics.";
                return View(viewModel);
            }

            // Resolve CourseCode from selected CourseId
            if (string.IsNullOrWhiteSpace(viewModel.CourseCode))
            {
                var courses = await _courseService.GetAllCoursesAsync();
                var selectedCourse = courses.FirstOrDefault(c => c.Id == viewModel.CourseId);
                if (selectedCourse != null)
                {
                    viewModel.CourseCode = selectedCourse.Code;
                }
            }

            if (string.IsNullOrWhiteSpace(viewModel.CourseCode) || string.IsNullOrWhiteSpace(viewModel.Topic))
            {
                viewModel.ErrorMessage = "Please provide course and topic.";
                return View(viewModel);
            }

            // Get rubric description if a rubric is selected
            string? rubricDescription = null;
            if (viewModel.RubricId.HasValue && viewModel.RubricId > 0 && viewModel.Rubrics.Any())
            {
                var selectedRubric = viewModel.Rubrics.FirstOrDefault(r => int.TryParse(r.Value, out int id) && id == viewModel.RubricId);
                if (selectedRubric != null)
                {
                    rubricDescription = $"Rubric: {selectedRubric.Text}";
                }
            }

            var result = await _questionService.GenerateQuestionFromMaterialAsync(
                viewModel.CourseCode, viewModel.Topic, viewModel.BloomLevel, rubricDescription);

            ParseLLMResponse(result.GeneratedQuestion, out var questionText, out var referenceAnswer);

            viewModel.GeneratedQuestion = questionText;
            viewModel.ReferenceAnswer = referenceAnswer;
            viewModel.HasResult = true;
            return View(viewModel);
        }

        // GET: Questions/GetTopicsByCourse/3
        [HttpGet("~/Questions/GetTopicsByCourse/{courseId:int}")]
        public async Task<IActionResult> GetTopicsByCourse(int courseId)
        {
            if (courseId <= 0) return Json(new List<object>());
            var topics = await _courseService.GetTopicsByCourseAsync(courseId);
            var result = topics.Select(t => new { id = t.Id, name = t.Name }).ToList();
            return Json(result);
        }

        private async Task<bool> CourseExistsAsync(int courseId)
        {
            if (courseId <= 0) return false;
            return await _courseService.GetCourseByIdAsync(courseId) != null;
        }

        private async Task<bool> TopicBelongsToCourseAsync(int topicId, int courseId)
        {
            var topic = await _courseService.GetTopicByIdAsync(topicId);
            return topic != null && topic.CourseId == courseId;
        }

        private void ParseLLMResponse(string llmResponse, out string? questionText, out string? referenceAnswer)
        {
            questionText = llmResponse?.Trim();
            referenceAnswer = null;

            var questionMatch = Regex.Match(llmResponse ?? "", @"QUESTION:\s*(.*?)\s*(?:REFERENCE ANSWER:|$)",
                RegexOptions.Singleline | RegexOptions.IgnoreCase);
            if (questionMatch.Success && questionMatch.Groups.Count > 1)
                questionText = CleanResponse(questionMatch.Groups[1].Value);

            var referenceMatch = Regex.Match(llmResponse ?? "", @"REFERENCE ANSWER:\s*(.*?)\s*(?:FEEDBACK:|SCORE:|FOLLOW-UP:|$)",
                RegexOptions.Singleline | RegexOptions.IgnoreCase);
            if (referenceMatch.Success && referenceMatch.Groups.Count > 1)
                referenceAnswer = CleanResponse(referenceMatch.Groups[1].Value);
        }

        private string CleanResponse(string input)
        {
            var html = System.Net.WebUtility.HtmlDecode(input);
            return html.Trim();
        }

        private async Task PopulateAIGenerateDropdownsAsync(AIGenerateViewModel viewModel)
        {
            var courses = await _courseService.GetAllCoursesAsync();
            viewModel.Courses = courses
                .Select(c => new SelectListItem($"{c.Code} - {c.Name}", c.Id.ToString()))
                .ToList();

            if (viewModel.CourseId > 0)
            {
                viewModel.Topics = await GetTopicItemsAsync(viewModel.CourseId);
            }
            else
            {
                viewModel.Topics = new List<SelectListItem>();
            }
        }

        // POST: Questions/Review
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Review(QuestionReviewDto dto)
        {
            if (dto.QuestionId <= 0)
            {
                TempData["Error"] = "No question was specified for review.";
                return RedirectToAction(nameof(Index));
            }

            if (await _questionService.GetQuestionByIdAsync(dto.QuestionId) == null)
            {
                TempData["Error"] = $"Question {dto.QuestionId} no longer exists.";
                return RedirectToAction(nameof(Index));
            }

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
        [HttpGet("~/Questions/ByCourse")]
        [HttpGet("~/Questions/ByCourse/{courseId:int}")]
        public async Task<IActionResult> ByCourse(int courseId = 0)
        {
            var dtos = await _questionService.GetQuestionsByCourseAsync(courseId);
            var courses = await _courseService.GetAllCoursesAsync();
            var viewModel = QuestionBankViewModel.ByCourse(MapToIndexList(dtos), courses, courseId);
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
                Id = dto.Id,
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
                Id = dto.Id,
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
                Id = dto.Id,
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
