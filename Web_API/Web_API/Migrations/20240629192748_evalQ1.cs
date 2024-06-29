using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Web_API.Migrations
{
    /// <inheritdoc />
    public partial class evalQ1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Score",
                table: "EvaluationQuestions");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "EvaluationQuestions",
                newName: "HRId");

            migrationBuilder.AddColumn<int>(
                name: "DefaultScore",
                table: "EvaluationQuestions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "UserQuestionScores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EvaluationQuestionId = table.Column<int>(type: "int", nullable: false),
                    Score = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserQuestionScores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserQuestionScores_EvaluationQuestions_EvaluationQuestionId",
                        column: x => x.EvaluationQuestionId,
                        principalTable: "EvaluationQuestions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserQuestionScores_EvaluationQuestionId",
                table: "UserQuestionScores",
                column: "EvaluationQuestionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserQuestionScores");

            migrationBuilder.DropColumn(
                name: "DefaultScore",
                table: "EvaluationQuestions");

            migrationBuilder.RenameColumn(
                name: "HRId",
                table: "EvaluationQuestions",
                newName: "UserId");

            migrationBuilder.AddColumn<double>(
                name: "Score",
                table: "EvaluationQuestions",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }
    }
}
