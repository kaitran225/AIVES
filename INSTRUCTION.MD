# AIVES - AI-Assisted Viva Examination System

## Project Context

We are developing a university-level software project called **AIVES (AI Viva Examination System)**.

The system is designed to support **oral examinations (viva/oral exams)** such as:

* Thesis/project defenses
* Final oral examinations
* Competency interviews
* Course-based oral assessments

Traditional viva examinations require significant instructor time, are difficult to standardize across different examination rooms, are difficult to scale when the number of students is large, and may lack objective evidence when students appeal their grades.

AIVES aims to assist lecturers by:

1. Generating and managing viva questions.
2. Organizing oral examination sessions.
3. Using AI as a virtual examiner that asks questions and generates adaptive follow-up questions.
4. Supporting AI-assisted grading based on predefined rubrics.
5. Recording the complete examination process, including questions, answers, transcripts, scores, and AI feedback.

The system must keep the **lecturer as the final decision-maker**. AI only provides assistance and recommendations.

---

# Technical Requirements

This is a **university-level project**, so the implementation should remain realistic, understandable, and maintainable rather than being unnecessarily enterprise-scale.

### Required technology

* **Language:** C#
* **Framework:** .NET / ASP.NET Core MVC
* **Architecture:** MVC + 3-Layer Architecture
* **Database:** Microsoft SQL Server
* **ORM:** Entity Framework Core
* **IDE:** Visual Studio
* **Project type:** ASP.NET Core MVC Web Application
* Use separate projects / DLLs where appropriate to demonstrate the 3-layer architecture.

Recommended solution structure:

```text
AIVES.sln

AIVES.Web
    Controllers
    Views
    ViewModels
    wwwroot

AIVES.Business
    Services
    Interfaces
    DTOs

AIVES.Data
    Entities
    DbContext
    Repositories
    Migrations
```

The exact structure can be adjusted if necessary, but the separation between presentation, business logic, and data access must be clear.

---

# Architecture Requirements

Implement both:

## MVC Pattern

```text
Controller
    ↓
Model / Service
    ↓
View
```

Controllers handle HTTP requests and coordinate application logic.

Views are responsible for presentation.

Models / ViewModels represent application data.

Business logic must not be placed directly inside Views.

---

## 3-Layer Architecture

### Presentation Layer

ASP.NET Core MVC:

* Controllers
* Views
* ViewModels
* Authentication / authorization UI

### Business Logic Layer

Responsible for:

* Question generation logic
* Exam management
* Adaptive follow-up logic
* Scoring logic
* Rubric evaluation
* Examination workflow
* AI service orchestration

### Data Access Layer

Responsible for:

* Entity Framework Core
* SQL Server
* Entities
* DbContext
* Repositories
* Database persistence

The Presentation Layer should not directly access the database.

---

# Main Project Requirement

The final project **must demonstrate Main Flow #3: AI Interview Core**.

This is the most important requirement of the project and must be included in the final demonstration.

The main demonstration flow should show a complete AI-powered viva examination:

```text
Student starts examination
        ↓
AI asks main question
        ↓
Student answers using microphone
        ↓
Speech-to-text converts answer into transcript
        ↓
AI analyzes the answer
        ↓
AI decides whether clarification/follow-up is necessary
        ↓
AI asks adaptive follow-up question
        ↓
Student answers again
        ↓
Repeat until follow-up limit is reached
        ↓
Move to next main question
        ↓
Exam ends
        ↓
Transcript and examination record are saved
        ↓
AI generates grading recommendation
        ↓
Lecturer reviews and adjusts final score
```

This flow should be treated as the core feature of the application.

---

# Functional Requirements

## Module 1 - Question Bank and Rubric Management

Lecturers must be able to manage the question bank.

### Question creation

The system should allow lecturers to:

* Create questions manually.
* Import questions.
* Generate questions using AI.
* Associate questions with a course.
* Associate questions with a topic.
* Store reference material for the question.

AI-generated questions may be generated from:

* Course textbooks
* Lecture slides
* Course documents
* Uploaded learning materials

A simplified RAG-based approach may be used if appropriate for the project scope.

### Bloom's Taxonomy

Each question should have a cognitive difficulty/category based on Bloom's taxonomy:

* Remember
* Understand
* Apply
* Analyze

### Rubric

Each question should be associated with a grading rubric.

A rubric should contain:

```text
Criterion
Description
Maximum Score
Scoring Guidance
```

Example:

```text
Criterion: Technical Understanding
Maximum Score: 4

4 = Complete and technically correct explanation
3 = Mostly correct with minor omissions
2 = Partial understanding
1 = Significant misunderstanding
0 = No valid answer
```

### Lecturer review

AI-generated questions must not automatically become official questions.

Lecturers must be able to:

* Review
* Edit
* Approve
* Reject
* Delete

AI-generated questions before they enter the official question bank.

---

# Module 2 - Exam and Schedule Management

Lecturers can create viva examination sessions.

An examination session should contain:

* Course
* Examination name
* Date
* Start/end time
* Student list
* Time limit per student
* Number of main questions
* Maximum follow-up questions per main question

Example:

```text
Exam:
Software Engineering Final Viva

Students:
Student A
Student B
Student C

Main questions:
5

Maximum follow-ups per question:
2

Time per student:
15 minutes
```

---

## Question Selection

The system should support randomized or adaptive question selection.

The objective is to prevent students taking the exam consecutively from receiving exactly the same question sequence.

The system should consider:

* Question topic
* Bloom level
* Previously used questions
* Number of required questions
* Exam configuration

For a university project, a simple randomized selection strategy is acceptable.

A sophisticated machine-learning recommendation engine is not required.

---

# Module 3 - AI Interview Core

## This module is mandatory.

This is the key differentiating feature of AIVES.

The AI acts as a **virtual examiner**.

### Step 1 - AI asks the question

The system displays the current question and optionally uses **Text-to-Speech (TTS)** to read the question aloud.

Example:

```text
AI Examiner:

"Explain the difference between MVC and a three-layer architecture."
```

The student listens and prepares an answer.

---

## Step 2 - Student answers using voice

The student answers through a microphone.

The system uses **Speech-to-Text (STT)** to convert the student's speech into text.

Example:

```text
Student speaks:

"MVC separates the controller, model and view..."

Transcript:

"MVC separates the controller, model and view..."
```

The transcript should be stored as part of the examination record.

Near-real-time transcription is preferred, but a simplified implementation is acceptable if real-time streaming is too complex for the university project.

---

# Step 3 - AI analyzes the answer

The AI receives:

```text
Original Question
Expected Answer / Reference Answer
Rubric
Student Transcript
Previous Follow-up Questions
```

The AI evaluates whether the answer is:

* Correct
* Partially correct
* Incorrect
* Missing important information
* Ambiguous
* Contradictory

---

# Step 4 - Adaptive Follow-Up

This is one of the most important features of the system.

If the student's answer is incomplete, vague, or contradictory, the AI should generate a follow-up question.

Example:

```text
Main Question:

"What is the purpose of dependency injection?"

Student:

"It helps classes use other classes."

AI analysis:

The answer is partially correct but does not explain
why dependency injection is useful or how it reduces coupling.

Follow-up:

"Can you explain how dependency injection helps reduce
coupling between two classes?"
```

The follow-up question must be based on the student's actual answer.

The system should NOT simply ask another random predefined question.

---

## Follow-Up Limits

Each main question must have a configurable maximum number of follow-up questions.

Example:

```text
Maximum follow-ups = 2
```

The system must stop generating follow-up questions after the configured limit.

This prevents an AI conversation from continuing indefinitely, because apparently even software needs boundaries.

---

# Time Limits

The examination system should support:

* Overall examination time limit
* Time limit per answer
* Maximum number of follow-up questions

Example:

```text
Exam duration: 15 minutes
Answer time: 60 seconds
Maximum follow-ups: 2
```

When the answer time expires, the system should automatically end the response phase and continue the examination workflow.

---

# Module 4 - AI-Assisted Grading

After the examination, the system should analyze the student's answers.

The AI should compare:

```text
Question
Reference Answer
Rubric
Student Transcript
```

and produce:

```text
Suggested Score
Strengths
Weaknesses
Missing Information
Explanation
```

Example:

```text
Score: 3 / 4

Strengths:
- Correctly explained dependency injection.
- Correctly identified reduced coupling.

Missing:
- Did not explain how dependencies are provided externally.

Recommendation:
Mostly correct with a minor omission.
```

---

# Human-in-the-Loop Requirement

AI must NOT determine the final grade automatically.

The lecturer must be able to:

* View every question.
* View the student's transcript.
* View AI analysis.
* View the AI suggested score.
* Modify the score.
* Add lecturer comments.
* Confirm the final score.

The final score belongs to the lecturer.

Example:

```text
AI Suggested Score: 3 / 4

Lecturer Adjustment:
4 / 4

Lecturer Comment:
"Student demonstrated sufficient understanding
during the follow-up discussion."

Final Score:
4 / 4
```

---

# Examination Audit Trail

The system must preserve evidence of the examination.

Store information such as:

```text
Exam Session
Student
Question
Question Order
Question Type
Student Transcript
Follow-up Questions
AI Analysis
AI Suggested Score
Lecturer Final Score
Lecturer Comments
Timestamps
Answer Duration
```

The purpose is to provide an objective examination record if a student later questions the result.

---

# Optional Additional Signals

The system may record additional signals such as:

* Answer duration
* Number of follow-up questions
* Speech duration
* Transcript length
* Response timestamps

These signals should be treated as supporting information only and should not automatically determine the student's grade.

---

# Suggested Database Entities

A reasonable initial database design could include:

```text
User
Course
Student
Lecturer

Question
QuestionTopic
QuestionRubric
RubricCriterion

ExamSession
ExamParticipant
ExamQuestion

InterviewSession
InterviewQuestion
StudentAnswer
Transcript

FollowUpQuestion
AIAnalysis
AIScoreSuggestion

FinalScore
LecturerEvaluation
AuditLog
```

The database design may be simplified if necessary for the university project.

Avoid unnecessary microservices or distributed architecture.

A single ASP.NET Core MVC application with separate class-library projects for the business and data layers is sufficient.

---

# Base Interface Pattern

All services should follow a base interface pattern to keep the business layer consistent and reusable.

## Base Interface

```csharp
namespace AIVES.Business.Interfaces;

public interface IBaseService<T> where T : class
{
    Task<T> GetByIdAsync(int id);
    Task<List<T>> GetAllAsync();
    Task<T> CreateAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
}
```

This base interface ensures every service has the same basic CRUD contract, making it easier to:

- Write generic controllers or helpers
- Swap implementations for testing
- Keep the codebase consistent

---

# Service Interfaces

Each service should extend `IBaseService<T>` or implement a specific interface when it has custom behavior.

## 1. IQuestionService

Manages the question bank.

```csharp
namespace AIVES.Business.Interfaces;

public interface IQuestionService : IBaseService<AIVES.Data.Entities.Question>
{
    Task<List<AIVES.Data.Entities.Question>> GetByCourseAsync(int courseId);
    Task<List<AIVES.Data.Entities.Question>> GetByTopicAsync(int topicId);
    Task<List<AIVES.Data.Entities.Question>> GetByBloomLevelAsync(string bloomLevel);
    Task<List<AIVES.Data.Entities.Question>> GetApprovedQuestionsAsync(int courseId, int count);
    Task ApproveQuestionAsync(int questionId, int lecturerId);
    Task RejectQuestionAsync(int questionId, int lecturerId, string reason);
    Task GenerateQuestionAsync(string courseCode, string topic, string bloomLevel);
}
```

## 2. IExamService

Manages exam sessions and participants.

```csharp
namespace AIVES.Business.Interfaces;

public interface IExamService : IBaseService<AIVES.Data.Entities.ExamSession>
{
    Task<List<AIVES.Data.Entities.ExamSession>> GetByCourseAsync(int courseId);
    Task<List<AIVES.Data.Entities.ExamSession>> GetByStatusAsync(string status);
    Task AddStudentToExamAsync(int examId, int studentId);
    Task RemoveStudentFromExamAsync(int examId, int studentId);
    Task<List<AIVES.Data.Entities.ExamParticipant>> GetParticipantsAsync(int examId);
    Task AssignQuestionsToExamAsync(int examId);
    Task StartExamSessionAsync(int examId);
    Task CompleteExamSessionAsync(int examId);
}
```

## 3. IInterviewService

Manages the live interview session during the exam.

```csharp
namespace AIVES.Business.Interfaces;

public interface IInterviewService
{
    Task CreateInterviewSessionAsync(int examParticipantId);
    Task<AIVES.Data.Entities.InterviewSession> GetInterviewSessionAsync(int participantId);
    Task<AIVES.Data.Entities.InterviewQuestion> GetCurrentQuestionAsync(int sessionId);
    Task SaveAnswerAsync(int questionId, string transcript, int durationSeconds);
    Task<bool> HasRemainingFollowUpsAsync(int examQuestionId);
    Task CompleteInterviewSessionAsync(int sessionId);
}
```

## 4. IAIInterviewService

Handles all AI interactions. Isolated behind an interface so the AI provider can be swapped.

```csharp
namespace AIVES.Business.Interfaces;

public interface IAIInterviewService
{
    // Analyze a student's answer against the rubric
    Task<ScoreAnalysis> AnalyzeAnswerAsync(
        string question,
        string transcript,
        string rubric);

    // Generate an adaptive follow-up based on the student's actual answer
    Task<string> GenerateFollowUpQuestionAsync(
        string question,
        string transcript,
        string analysis);

    // Produce a grading recommendation
    Task<ScoreSuggestion> EvaluateAnswerAsync(
        string question,
        string transcript,
        string rubric);

    // Generate a question from course material (RAG-based)
    Task<string> GenerateQuestionFromMaterialAsync(
        string courseCode,
        string topic,
        string bloomLevel);
}

// Supporting types
public class ScoreAnalysis
{
    public bool IsCorrect { get; set; }
    public string Summary { get; set; } = string.Empty;
    public string MissingInformation { get; set; } = string.Empty;
}

public class ScoreSuggestion
{
    public int SuggestedScore { get; set; }
    public string Strengths { get; set; } = string.Empty;
    public string Weaknesses { get; set; } = string.Empty;
    public string MissingInformation { get; set; } = string.Empty;
    public string Explanation { get; set; } = string.Empty;
}
```

## 5. ISpeechToTextService

Converts student voice input to text.

```csharp
namespace AIVES.Business.Interfaces;

public interface ISpeechToTextService
{
    Task<string> TranscribeAsync(byte[] audioData);
    Task<string> TranscribeAsync(string audioFilePath);
}
```

## 6. ITextToSpeechService

Converts AI questions to speech.

```csharp
namespace AIVES.Business.Interfaces;

public interface ITextToSpeechService
{
    Task<byte[]> SynthesizeAsync(string text);
    Task<string> SynthesizeToTempFileAsync(string text);
}
```

## 7. IRubricService

Manages grading rubrics and criteria.

```csharp
namespace AIVES.Business.Interfaces;

public interface IRubricService : IBaseService<AIVES.Data.Entities.QuestionRubric>
{
    Task<List<AIVES.Data.Entities.RubricCriterion>> GetCriteriaAsync(int rubricId);
    Task AddCriterionAsync(int rubricId, string criterion, string description, int maxScore, string guidance);
    Task UpdateCriterionAsync(AIVES.Data.Entities.RubricCriterion criterion);
    Task DeleteCriterionAsync(int criterionId);
}
```

## 8. IScoringService

Orchestrates the scoring process.

```csharp
namespace AIVES.Business.Interfaces;

public interface IScoringService
{
    Task<ScoreSuggestion> CalculateScoreAsync(int interviewQuestionId);
    Task ConfirmScoreAsync(int interviewQuestionId, int finalScore, int lecturerId, string comment);
    Task<List<ScoreSuggestion>> CalculateAllScoresAsync(int examParticipantId);
}
```

## 9. IAuditService

Tracks all important actions for the audit trail.

```csharp
namespace AIVES.Business.Interfaces;

public interface IAuditService
{
    Task LogAsync(
        string action,
        string entityName,
        int? entityId,
        string details,
        int userId);
}
```

---

# How Interfaces Are Used

```
AIVES.Web (Controllers)
    ↓  depends on  ↓
AIVES.Business (Interfaces + Services)
    ↓  depends on  ↓
AIVES.Data (Entities + DbContext)
```

- **Controllers** depend on **interfaces**, not concrete classes
- **Services** implement the interfaces and use **AIVES.Data** to access the database
- This makes the system **testable** and **flexible**

Example:

```csharp
// Controller depends on interface, not implementation
public class ExamController : Controller
{
    private readonly IExamService _examService;

    public ExamController(IExamService examService)
    {
        _examService = examService;
    }

    public IActionResult Details(int id)
    {
        var exam = _examService.GetByIdAsync(id);
        return View(exam);
    }
}
```

---

# Summary of Interface Structure

```
AIVES.Business/Interfaces/
├── IBaseService<T>.cs          — Base CRUD for all services
├── IQuestionService.cs
├── IExamService.cs
├── IInterviewService.cs
├── IAIInterviewService.cs
├── ISpeechToTextService.cs
├── ITextToSpeechService.cs
├── IRubricService.cs
├── IScoringService.cs
└── IAuditService.cs
```

All services should be registered in `Program.cs`:

```csharp
builder.Services.AddScoped<IBaseService<Question>, QuestionService>();
builder.Services.AddScoped<IQuestionService, QuestionService>();
builder.Services.AddScoped<IExamService, ExamService>();
builder.Services.AddScoped<IInterviewService, InterviewService>();
builder.Services.AddScoped<IAIInterviewService, AIInterviewService>();
builder.Services.AddScoped<ISpeechToTextService, SpeechToTextService>();
builder.Services.AddScoped<ITextToSpeechService, TextToSpeechService>();
builder.Services.AddScoped<IRubricService, RubricService>();
builder.Services.AddScoped<IScoringService, ScoringService>();
builder.Services.AddScoped<IAuditService, AuditService>();
```

---

# Required Demonstration

The final presentation should focus on the following end-to-end scenario:

## Lecturer

1. Creates a course.
2. Adds or generates questions.
3. Reviews and approves AI-generated questions.
4. Defines rubrics.
5. Creates an examination session.
6. Adds students.
7. Configures:

   * Number of questions
   * Answer time
   * Maximum follow-ups

## Student

1. Starts the examination.
2. AI reads the first question.
3. Student answers through microphone.
4. System converts speech to text.
5. AI analyzes the answer.
6. AI generates a follow-up question when appropriate.
7. Student answers the follow-up.
8. System continues until the configured limit.
9. System moves to the next question.
10. Examination ends.

## Lecturer Grading

1. Lecturer opens the completed examination.
2. Views the complete transcript.
3. Views AI analysis.
4. Views suggested scores.
5. Adjusts scores if necessary.
6. Adds comments.
7. Confirms the final grade.

---

# Assignment Requirements

The implementation must satisfy the following assignment requirements:

### Assignment 1

* Implement MVC architecture.
* Implement 3-layer architecture.
* Create the project using Visual Studio.
* Use ASP.NET Core MVC Web Application.
* Use separate class libraries / DLLs where appropriate.
* Demonstrate at least one complete main workflow.

### Mandatory Final Flow

The final project MUST demonstrate:

**Main Flow #3 - AI Interview Core**

This is the mandatory core feature for the group project.

---

# Scope Control

This is a university project, not a commercial enterprise product.

Prioritize:

1. Working end-to-end demonstration.
2. Clear MVC architecture.
3. Clear 3-layer architecture.
4. Functional SQL Server database.
5. Functional AI interview workflow.
6. Understandable source code.
7. Good UI sufficient for demonstration.
8. Complete examination audit trail.

Do NOT over-engineer the system with:

* Microservices
* Kubernetes
* Event-driven distributed architecture
* Complex ML model training
* Custom speech-recognition model training
* Complex recommendation algorithms
* Enterprise-scale infrastructure

Use existing APIs/services for AI, STT, and TTS where appropriate.

The goal is to demonstrate that the team understands **software architecture, database design, MVC, business logic, AI integration, and the complete viva examination workflow**.

---

# Development Strategy

Build the system incrementally.

Recommended implementation order:

```text
1. Create Visual Studio solution
2. Create MVC Web project
3. Create Business Layer
4. Create Data Access Layer
5. Configure SQL Server + EF Core
6. Create database entities
7. Implement authentication/basic roles
8. Implement Question Bank
9. Implement Rubric Management
10. Implement Exam Session Management
11. Implement Student Exam UI
12. Implement AI Interview Core
13. Integrate STT
14. Integrate TTS
15. Implement adaptive follow-up questions
16. Store complete transcripts
17. Implement AI-assisted scoring
18. Implement lecturer review
19. Implement audit trail
20. Polish UI and prepare final demonstration
```

The implementation should always maintain clean separation between:

```text
Controller
    ↓
Business Service
    ↓
Data Access
    ↓
SQL Server
```

AI integrations should be accessed through dedicated service interfaces rather than directly from controllers.

The final application should provide a clear, demonstrable end-to-end AI viva examination workflow.
