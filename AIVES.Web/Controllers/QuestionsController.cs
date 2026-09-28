using Microsoft.AspNetCore.Mvc;
using AIVES.Business.Interfaces;
using AIVES.Business.DTOs;

namespace AIVES.Web.Controllers
{
    public class QuestionsController : Controller
    {
        private readonly IQuestionService _questionService;

        public QuestionsController(IQuestionService questionService)
        {
            _questionService = questionService;
        }

        // GET: Questions
        public async Task<IActionResult> Index()
        {
            var questions = await _questionService.GetAllQuestionsAsync();
            return View(questions);
        }

        // GET: Questions/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var question = await _questionService.GetQuestionByIdAsync(id);
            if (question == null) return NotFound();
            return View(question);
        }

        // GET: Questions/Create
        public IActionResult Create()
        {
            return View(new QuestionCreateDto());
        }

        // POST: Questions/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(QuestionCreateDto dto)
        {
            if (ModelState.IsValid)
            {
                var question = await _questionService.CreateQuestionAsync(dto);
                TempData["Success"] = "Question created successfully.";
                return RedirectToAction(nameof(Details), new { id = question.Id });
            }
            return View(dto);
        }

        // GET: Questions/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var question = await _questionService.GetQuestionByIdAsync(id);
            if (question == null) return NotFound();
            var dto = new QuestionUpdateDto
            {
                Id = question.Id,
                Text = question.Text,
                ReferenceAnswer = question.ReferenceAnswer,
                BloomLevel = question.BloomLevel,
                Status = question.Status,
                CourseId = question.CourseId,
                TopicId = question.TopicId,
                RubricId = question.RubricId
            };
            return View(dto);
        }

        // POST: Questions/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(QuestionUpdateDto dto)
        {
            if (ModelState.IsValid)
            {
                await _questionService.UpdateQuestionAsync(dto);
                TempData["Success"] = "Question updated successfully.";
                return RedirectToAction(nameof(Details), new { id = dto.Id });
            }
            return View(dto);
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
            return View();
        }

        // POST: Questions/AI-Generate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AIGenerate(string courseCode, string topic, string bloomLevel)
        {
            if (string.IsNullOrWhiteSpace(courseCode) || string.IsNullOrWhiteSpace(topic))
            {
                TempData["Error"] = "Please provide course code and topic.";
                return View();
            }

            var result = await _questionService.GenerateQuestionFromMaterialAsync(courseCode, topic, bloomLevel);
            ViewData["GeneratedQuestion"] = result.GeneratedQuestion;
            ViewData["BloomLevel"] = result.BloomLevel;
            ViewData["CourseCode"] = result.CourseCode;
            ViewData["Topic"] = result.Topic;
            return View(result);
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
            var questions = await _questionService.GetPendingQuestionsAsync();
            return View(questions);
        }

        // GET: Questions/ByCourse/5
        public async Task<IActionResult> ByCourse(int courseId)
        {
            var questions = await _questionService.GetQuestionsByCourseAsync(courseId);
            return View("Index", questions);
        }
    }
}
