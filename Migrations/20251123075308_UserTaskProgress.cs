using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NOX_Backend.Migrations
{
    /// <inheritdoc />
    public partial class UserTaskProgress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Conversations",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "UserOnboardingTaskProgress",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TaskId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserOnboardingTaskProgress", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserOnboardingTaskProgress_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserOnboardingTaskProgress_OnboardingTasks_TaskId",
                        column: x => x.TaskId,
                        principalTable: "OnboardingTasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserOnboardingTaskProgress_TaskId",
                table: "UserOnboardingTaskProgress",
                column: "TaskId");

            migrationBuilder.CreateIndex(
                name: "IX_UserOnboardingTaskProgress_UserId",
                table: "UserOnboardingTaskProgress",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserOnboardingTaskProgress");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Conversations",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
