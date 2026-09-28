using Microsoft.EntityFrameworkCore;
using AIVES.Data.Entities;

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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // ==================== User ====================
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(200);
                entity.Property(e => e.PasswordHash).IsRequired();
                entity.Property(e => e.Role).IsRequired().HasMaxLength(50);
                entity.Property(e => e.StudentNumber).HasMaxLength(50);

                // User → Course (optional, User has CourseId FK)
                entity.HasOne(e => e.Course)
                      .WithMany(c => c.Lecturers)
                      .HasForeignKey(e => e.CourseId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ==================== Course ====================
            modelBuilder.Entity<Course>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Code).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).HasMaxLength(1000);
            });

            // ==================== QuestionTopic ====================
            modelBuilder.Entity<QuestionTopic>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).HasMaxLength(1000);
            });

            // ==================== Question ====================
            modelBuilder.Entity<Question>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Text).IsRequired().HasMaxLength(2000);
                entity.Property(e => e.ReferenceAnswer).HasMaxLength(4000);
                entity.Property(e => e.BloomLevel).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
                entity.Property(e => e.SourceType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.ReferenceMaterial).HasMaxLength(2000);

                // Question → Course (required)
                entity.HasOne(e => e.Course)
                      .WithMany(c => c.Questions)
                      .HasForeignKey(e => e.CourseId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Question → QuestionTopic (optional)
                entity.HasOne(e => e.Topic)
                      .WithMany(t => t.Questions)
                      .HasForeignKey(e => e.TopicId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Question → QuestionRubric (optional)
                entity.HasOne(e => e.Rubric)
                      .WithMany(r => r.Questions)
                      .HasForeignKey(e => e.RubricId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ==================== QuestionRubric ====================
            modelBuilder.Entity<QuestionRubric>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).HasMaxLength(1000);
            });

            // ==================== RubricCriterion ====================
            modelBuilder.Entity<RubricCriterion>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Criterion).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).IsRequired().HasMaxLength(1000);
                entity.Property(e => e.ScoringGuidance).HasMaxLength(2000);

                // RubricCriterion → QuestionRubric (required)
                entity.HasOne(e => e.Rubric)
                      .WithMany(r => r.Criteria)
                      .HasForeignKey(e => e.RubricId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ==================== ExamSession ====================
            modelBuilder.Entity<ExamSession>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
                entity.HasOne(e => e.Course)
                      .WithMany(c => c.ExamSessions)
                      .HasForeignKey(e => e.CourseId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ==================== ExamParticipant ====================
            modelBuilder.Entity<ExamParticipant>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.StudentNumber).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
                entity.HasOne(e => e.ExamSession)
                      .WithMany(s => s.Participants)
                      .HasForeignKey(e => e.ExamSessionId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Student)
                      .WithMany(u => u.Participants)
                      .HasForeignKey(e => e.StudentId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ==================== ExamQuestion ====================
            modelBuilder.Entity<ExamQuestion>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.ExamSession)
                      .WithMany(s => s.ExamQuestions)
                      .HasForeignKey(e => e.ExamSessionId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Question)
                      .WithMany(q => q.ExamQuestions)
                      .HasForeignKey(e => e.QuestionId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ==================== InterviewSession ====================
            modelBuilder.Entity<InterviewSession>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
                entity.HasOne(e => e.ExamParticipant)
                      .WithOne(p => p.InterviewSession)
                      .HasForeignKey<InterviewSession>(e => e.ExamParticipantId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ==================== InterviewQuestion ====================
            modelBuilder.Entity<InterviewQuestion>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.QuestionText).IsRequired().HasMaxLength(2000);
                entity.Property(e => e.AIAnalysis).HasMaxLength(4000);
                entity.HasOne(e => e.ExamQuestion)
                      .WithMany(eq => eq.InterviewQuestions)
                      .HasForeignKey(e => e.ExamQuestionId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.PreviousQuestion)
                      .WithMany()
                      .HasForeignKey(e => e.PreviousQuestionId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ==================== StudentAnswer ====================
            modelBuilder.Entity<StudentAnswer>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Transcript).IsRequired();
                entity.HasOne(e => e.InterviewQuestion)
                      .WithMany(iq => iq.Answers)
                      .HasForeignKey(e => e.InterviewQuestionId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ==================== AIScoreSuggestion ====================
            modelBuilder.Entity<AIScoreSuggestion>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Strengths).IsRequired();
                entity.Property(e => e.Weaknesses).IsRequired();
                entity.Property(e => e.MissingInformation).IsRequired();
                entity.Property(e => e.Explanation).IsRequired();
                entity.HasOne(e => e.InterviewQuestion)
                      .WithOne(iq => iq.ScoreSuggestion)
                      .HasForeignKey<AIScoreSuggestion>(e => e.InterviewQuestionId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ==================== LecturerEvaluation ====================
            modelBuilder.Entity<LecturerEvaluation>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.LecturerComment).HasMaxLength(2000);
                entity.HasOne(e => e.InterviewQuestion)
                      .WithOne()
                      .HasForeignKey<LecturerEvaluation>(e => e.InterviewQuestionId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Lecturer)
                      .WithMany(u => u.LecturerEvaluations)
                      .HasForeignKey(e => e.LecturerId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ==================== AuditLog ====================
            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Action).IsRequired().HasMaxLength(200);
                entity.Property(e => e.EntityName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Details).HasMaxLength(4000);
                entity.HasOne(e => e.User)
                      .WithMany()
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}