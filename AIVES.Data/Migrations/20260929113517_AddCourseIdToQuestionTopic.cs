using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AIVES.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCourseIdToQuestionTopic : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CourseId",
                table: "QuestionTopics",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_QuestionTopics_CourseId",
                table: "QuestionTopics",
                column: "CourseId");

            migrationBuilder.AddForeignKey(
                name: "FK_QuestionTopics_Courses_CourseId",
                table: "QuestionTopics",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QuestionTopics_Courses_CourseId",
                table: "QuestionTopics");

            migrationBuilder.DropIndex(
                name: "IX_QuestionTopics_CourseId",
                table: "QuestionTopics");

            migrationBuilder.DropColumn(
                name: "CourseId",
                table: "QuestionTopics");
        }
    }
}
