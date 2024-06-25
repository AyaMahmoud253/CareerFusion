using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Web_API.Migrations
{
    /// <inheritdoc />
    public partial class UpdateJobFormCV : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "PhysicalInterviewDate",
                table: "JobFormCVs",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TechnicalAssessmentDate",
                table: "JobFormCVs",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PhysicalInterviewDate",
                table: "JobFormCVs");

            migrationBuilder.DropColumn(
                name: "TechnicalAssessmentDate",
                table: "JobFormCVs");
        }
    }
}
