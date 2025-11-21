using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NOX_Backend.Migrations
{
    /// <inheritdoc />
    public partial class Add_UserTaskProgress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserTaskProgress_AspNetUsers_UserId",
                table: "UserTaskProgress");

            migrationBuilder.DropForeignKey(
                name: "FK_UserTaskProgress_OnboardingTasks_TaskId",
                table: "UserTaskProgress");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserTaskProgress",
                table: "UserTaskProgress");

            migrationBuilder.RenameTable(
                name: "UserTaskProgress",
                newName: "UserOnboardingTaskProgress");

            migrationBuilder.RenameIndex(
                name: "IX_UserTaskProgress_UserId",
                table: "UserOnboardingTaskProgress",
                newName: "IX_UserOnboardingTaskProgress_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_UserTaskProgress_TaskId",
                table: "UserOnboardingTaskProgress",
                newName: "IX_UserOnboardingTaskProgress_TaskId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserOnboardingTaskProgress",
                table: "UserOnboardingTaskProgress",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserOnboardingTaskProgress_AspNetUsers_UserId",
                table: "UserOnboardingTaskProgress",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserOnboardingTaskProgress_OnboardingTasks_TaskId",
                table: "UserOnboardingTaskProgress",
                column: "TaskId",
                principalTable: "OnboardingTasks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserOnboardingTaskProgress_AspNetUsers_UserId",
                table: "UserOnboardingTaskProgress");

            migrationBuilder.DropForeignKey(
                name: "FK_UserOnboardingTaskProgress_OnboardingTasks_TaskId",
                table: "UserOnboardingTaskProgress");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserOnboardingTaskProgress",
                table: "UserOnboardingTaskProgress");

            migrationBuilder.RenameTable(
                name: "UserOnboardingTaskProgress",
                newName: "UserTaskProgress");

            migrationBuilder.RenameIndex(
                name: "IX_UserOnboardingTaskProgress_UserId",
                table: "UserTaskProgress",
                newName: "IX_UserTaskProgress_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_UserOnboardingTaskProgress_TaskId",
                table: "UserTaskProgress",
                newName: "IX_UserTaskProgress_TaskId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserTaskProgress",
                table: "UserTaskProgress",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserTaskProgress_AspNetUsers_UserId",
                table: "UserTaskProgress",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserTaskProgress_OnboardingTasks_TaskId",
                table: "UserTaskProgress",
                column: "TaskId",
                principalTable: "OnboardingTasks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
