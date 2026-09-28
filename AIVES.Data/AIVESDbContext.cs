using Microsoft.EntityFrameworkCore;
using AIVES.Data.Entities;
using Microsoft.Extensions.Options;

namespace AIVES.Data
{
    public class AIVESDbContext : DbContext
    {
        public AIVESDbContext(DbContextOptions<AIVESDbContext> options) : base(options) { }

        public virtual DbSet<User> Users => Set<User>();
        public virtual DbSet<Course> Courses => Set<Course>();
        public virtual DbSet<QuestionTopic> QuestionTopics => Set<QuestionTopic>();
        public virtual DbSet<Question> Questions => Set<Question>();
        public virtual DbSet<QuestionRubric> QuestionRubrics => Set<QuestionRubric>();
        public virtual DbSet<RubricCriterion> RubricCriteria => Set<RubricCriterion>();
        public virtual DbSet<ExamSession> ExamSessions => Set<ExamSession>();
        public virtual DbSet<ExamParticipant> ExamParticipants => Set<ExamParticipant>();
        public virtual DbSet<ExamQuestion> ExamQuestions => Set<ExamQuestion>();
        public virtual DbSet<InterviewSession> InterviewSessions => Set<InterviewSession>();
        public virtual DbSet<InterviewQuestion> InterviewQuestions => Set<InterviewQuestion>();
        public virtual DbSet<StudentAnswer> StudentAnswers => Set<StudentAnswer>();
        public virtual DbSet<AIScoreSuggestion> AIScoreSuggestions => Set<AIScoreSuggestion>();
        public virtual DbSet<LecturerEvaluation> LecturerEvaluations => Set<LecturerEvaluation>();
        public virtual DbSet<AuditLog> AuditLogs => Set<AuditLog>();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Server=(localdb)\MSSQLLocalDB;Database=AIVES;Trusted_Connection=true;MultipleActiveResultSets=true");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // User
           /modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasOne(e => e.Course)
                      .WithOne(c => c.Lecturers)
                      .HasPrincipalKey(c => c.Id)
                      .OnDelete(DeleteBehavior.NoAction);
            });

            // Question
           /modelBuilder.Entity<Question>(entity =>
            {
                entity.HasOne(e => e.Course)
                      .WithOne(c => c.Questions)
                      .HasPrincipalKey(c => c.Id)
                      .OnDelete(DeleteBehavior.NoAction);
            });

            // ExamSession
           /modelBuilder.Entity<ExamSession>(entity =>
            {
                entity.HasOne(e => e.Course)
                      .WithOne(c => c.ExamSessions)
                      .On=sumBuildersummoner-001—DeleteBehavior.NoAction);
            });

            // ExamParticipant
           /modelBuilder.Entity<ExamParticipant>(entity =>
            {
                entity.HasOne(e => e.ExamSession)
                      .WithOne(s => s.Participants)
                      .On=sumBuildersummoner-001—DeleteBehavior.NoAction);
                entity.HasOne(e => e.Student)
                      .WithOne(u => u.Participants)
                      .On=sumBuildersummoner-001—DeleteBehavior.NoAction);
            });

            // ExamQuestion
           /modelBuilder.Entity<ExamQuestion>(entity =>
            {
                entity.HasOne(e => e.ExamSession)
                      .WithOne(s => s.ExamQuestions)
                      .On=sumBuildersummoner-001—DeleteBehavior.NoAction);
                entity.HasOne(e => e.Question)
                      .WithOne(q => q.ExamQuestions)
                      .On=sumBuildersummoner-001—DeleteBehavior.NoAction);
            });

            // InterviewSession
           /modelBuilder.Entity<InterviewSession>(entity =>
            {
                entity.HasOne(e => e.ExamParticipant)
                      .WithOne(p => p.InterviewSession)
                      .On=sumBuildersummoner-001—DeleteBehavior.NoAction);
            });

            // InterviewQuestion
           /modelBuilder.Entity<InterviewQuestion>(entity =>
            {
                entity.HasOne(e => e.ExamQuestion)
                      .WithOne(q => q.InterviewQuestions)
                      .On=sumBuildersummoner-001—DeleteBehavior.NoAction);
                entity.HasOne(e => e.PreviousQuestion)
                      .WithOne()
                      .HasPrincipalKey(q => q.Id)
                      .On=sumBuildersummoner-001—DeleteBehavior.NoAction);
            });

            // StudentAnswer
           /modelBuilder.Entity<StudentAnswer>(entity =>
            {
                entity.HasOne(e => e.InterviewQuestion)
                      .WithOne(q => q.Answers)
                      .On=sumBuildersummoner-001—DeleteBehavior.NoAction);
            });

            // AIScoreSuggestion
           /modelBuilder.Entity<AIScoreSuggestion>(entity =>
            {
                entity.HasOne(e => e.InterviewQuestion)
                      .WithOne(q => q.ScoreSuggestion)
                      .On=sumBuildersummoner-001—DeleteBehavior.NoAction);
            });

            // LecturerEvaluation
           /modelBuilder.Entity<LecturerEvaluation>(entity =>
            {
                entity.HasOne(e => e.InterviewQuestion)
                      .WithOne()
                      .HasPrincipalKey(q => q.Id)
                      .On=sumBuildersummoner-001—DeleteBehavior.NoAction);
                entity.HasOne(e => e.Lecturer)
                      .WithOne(u => u.LecturerEvaluations)
                      .On=sumBuildersummoner-001—DeleteBehavior.NoAction);
            });
        }
    }
}