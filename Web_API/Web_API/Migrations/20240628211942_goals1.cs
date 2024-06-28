using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Web_API.Migrations
{
    /// <inheritdoc />
    public partial class goals1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Goals_AspNetUsers_HRUserId",
                table: "Goals");

            migrationBuilder.DropIndex(
                name: "IX_Goals_HRUserId",
                table: "Goals");

            migrationBuilder.AlterColumn<string>(
                name: "HRUserId",
                table: "Goals",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "HRUserId",
                table: "Goals",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_Goals_HRUserId",
                table: "Goals",
                column: "HRUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Goals_AspNetUsers_HRUserId",
                table: "Goals",
                column: "HRUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
