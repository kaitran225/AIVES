using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AIVES.Data.Migrations
{
    /// <inheritdoc />
    public partial class MatrixRubric : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RubricCriteria_QuestionRubrics_RubricId",
                table: "RubricCriteria");

            migrationBuilder.DropIndex(
                name: "IX_RubricCriteria_RubricId",
                table: "RubricCriteria");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "RubricCriteria");

            migrationBuilder.DropColumn(
                name: "ScoringGuidance",
                table: "RubricCriteria");

            migrationBuilder.RenameColumn(
                name: "RubricId",
                table: "RubricCriteria",
                newName: "SortOrder");

            migrationBuilder.RenameColumn(
                name: "MaxScore",
                table: "RubricCriteria",
                newName: "QuestionRubricId");

            migrationBuilder.CreateTable(
                name: "PerformanceLevels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QuestionRubricId = table.Column<int>(type: "int", nullable: false),
                    Level = table.Column<int>(type: "int", nullable: false),
                    Label = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PerformanceLevels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PerformanceLevels_QuestionRubrics_QuestionRubricId",
                        column: x => x.QuestionRubricId,
                        principalTable: "QuestionRubrics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CriterionLevelDescriptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RubricCriterionId = table.Column<int>(type: "int", nullable: false),
                    PerformanceLevelId = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CriterionLevelDescriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CriterionLevelDescriptions_PerformanceLevels_PerformanceLevelId",
                        column: x => x.PerformanceLevelId,
                        principalTable: "PerformanceLevels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CriterionLevelDescriptions_RubricCriteria_RubricCriterionId",
                        column: x => x.RubricCriterionId,
                        principalTable: "RubricCriteria",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RubricCriteria_QuestionRubricId",
                table: "RubricCriteria",
                column: "QuestionRubricId");

            migrationBuilder.CreateIndex(
                name: "IX_CriterionLevelDescriptions_PerformanceLevelId",
                table: "CriterionLevelDescriptions",
                column: "PerformanceLevelId");

            migrationBuilder.CreateIndex(
                name: "IX_CriterionLevelDescriptions_RubricCriterionId",
                table: "CriterionLevelDescriptions",
                column: "RubricCriterionId");

            migrationBuilder.CreateIndex(
                name: "IX_PerformanceLevels_QuestionRubricId",
                table: "PerformanceLevels",
                column: "QuestionRubricId");

            migrationBuilder.AddForeignKey(
                name: "FK_RubricCriteria_QuestionRubrics_QuestionRubricId",
                table: "RubricCriteria",
                column: "QuestionRubricId",
                principalTable: "QuestionRubrics",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RubricCriteria_QuestionRubrics_QuestionRubricId",
                table: "RubricCriteria");

            migrationBuilder.DropTable(
                name: "CriterionLevelDescriptions");

            migrationBuilder.DropTable(
                name: "PerformanceLevels");

            migrationBuilder.DropIndex(
                name: "IX_RubricCriteria_QuestionRubricId",
                table: "RubricCriteria");

            migrationBuilder.RenameColumn(
                name: "SortOrder",
                table: "RubricCriteria",
                newName: "RubricId");

            migrationBuilder.RenameColumn(
                name: "QuestionRubricId",
                table: "RubricCriteria",
                newName: "MaxScore");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "RubricCriteria",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ScoringGuidance",
                table: "RubricCriteria",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_RubricCriteria_RubricId",
                table: "RubricCriteria",
                column: "RubricId");

            migrationBuilder.AddForeignKey(
                name: "FK_RubricCriteria_QuestionRubrics_RubricId",
                table: "RubricCriteria",
                column: "RubricId",
                principalTable: "QuestionRubrics",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
