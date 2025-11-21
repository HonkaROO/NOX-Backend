using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NOX_Backend.Migrations
{
    public partial class Add_UserTaskProgress : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop old FKs only if the old table exists
            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'UserTaskProgress')
BEGIN
    ALTER TABLE UserTaskProgress DROP CONSTRAINT FK_UserTaskProgress_AspNetUsers_UserId;
    ALTER TABLE UserTaskProgress DROP CONSTRAINT FK_UserTaskProgress_OnboardingTasks_TaskId;
END
");

            // Drop PK if old table exists
            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'UserTaskProgress')
BEGIN
    ALTER TABLE UserTaskProgress DROP CONSTRAINT PK_UserTaskProgress;
END
");

            // SAFE rename
            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'UserTaskProgress')
BEGIN
    EXEC sp_rename 'UserTaskProgress', 'UserOnboardingTaskProgress';
END
");

            // SAFE index rename
            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'UserOnboardingTaskProgress')
BEGIN
    IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_UserTaskProgress_UserId')
        EXEC sp_rename 'UserTaskProgress.IX_UserTaskProgress_UserId', 'IX_UserOnboardingTaskProgress_UserId', 'INDEX';

    IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_UserTaskProgress_TaskId')
        EXEC sp_rename 'UserTaskProgress.IX_UserTaskProgress_TaskId', 'IX_UserOnboardingTaskProgress_TaskId', 'INDEX';
END
");

            // Add PK on new table
            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'UserOnboardingTaskProgress')
BEGIN
    ALTER TABLE UserOnboardingTaskProgress 
        ADD CONSTRAINT PK_UserOnboardingTaskProgress PRIMARY KEY (Id);
END
");

            // Add FKs safely
            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'UserOnboardingTaskProgress')
BEGIN
    ALTER TABLE UserOnboardingTaskProgress 
        ADD CONSTRAINT FK_UserOnboardingTaskProgress_AspNetUsers_UserId 
        FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id) ON DELETE CASCADE;

    ALTER TABLE UserOnboardingTaskProgress 
        ADD CONSTRAINT FK_UserOnboardingTaskProgress_OnboardingTasks_TaskId 
        FOREIGN KEY (TaskId) REFERENCES OnboardingTasks(Id) ON DELETE CASCADE;
END
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Reverse operations safely…

            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'UserOnboardingTaskProgress')
BEGIN
    ALTER TABLE UserOnboardingTaskProgress DROP CONSTRAINT FK_UserOnboardingTaskProgress_AspNetUsers_UserId;
    ALTER TABLE UserOnboardingTaskProgress DROP CONSTRAINT FK_UserOnboardingTaskProgress_OnboardingTasks_TaskId;
    ALTER TABLE UserOnboardingTaskProgress DROP CONSTRAINT PK_UserOnboardingTaskProgress;

    EXEC sp_rename 'UserOnboardingTaskProgress', 'UserTaskProgress';
END
");
        }
    }
}
