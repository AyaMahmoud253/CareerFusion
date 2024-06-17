using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Web_API.Migrations
{
    /// <inheritdoc />
    public partial class AddTelephoneInterviewQuestions1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TelephoneInterviewQuestionEntity_JobForms_JobFormEntityId",
                table: "TelephoneInterviewQuestionEntity");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TelephoneInterviewQuestionEntity",
                table: "TelephoneInterviewQuestionEntity");

            migrationBuilder.RenameTable(
                name: "TelephoneInterviewQuestionEntity",
                newName: "TelephoneInterviewQuestions");

            migrationBuilder.RenameIndex(
                name: "IX_TelephoneInterviewQuestionEntity_JobFormEntityId",
                table: "TelephoneInterviewQuestions",
                newName: "IX_TelephoneInterviewQuestions_JobFormEntityId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TelephoneInterviewQuestions",
                table: "TelephoneInterviewQuestions",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TelephoneInterviewQuestions_JobForms_JobFormEntityId",
                table: "TelephoneInterviewQuestions",
                column: "JobFormEntityId",
                principalTable: "JobForms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TelephoneInterviewQuestions_JobForms_JobFormEntityId",
                table: "TelephoneInterviewQuestions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TelephoneInterviewQuestions",
                table: "TelephoneInterviewQuestions");

            migrationBuilder.RenameTable(
                name: "TelephoneInterviewQuestions",
                newName: "TelephoneInterviewQuestionEntity");

            migrationBuilder.RenameIndex(
                name: "IX_TelephoneInterviewQuestions_JobFormEntityId",
                table: "TelephoneInterviewQuestionEntity",
                newName: "IX_TelephoneInterviewQuestionEntity_JobFormEntityId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TelephoneInterviewQuestionEntity",
                table: "TelephoneInterviewQuestionEntity",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TelephoneInterviewQuestionEntity_JobForms_JobFormEntityId",
                table: "TelephoneInterviewQuestionEntity",
                column: "JobFormEntityId",
                principalTable: "JobForms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
