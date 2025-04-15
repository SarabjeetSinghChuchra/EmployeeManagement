using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmployeeManagement.API.Migrations
{
    /// <inheritdoc />
    public partial class InsertInitialUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
        INSERT INTO [dbo].[Users] 
            ([Id], [Username], [PasswordHash], [Role], [RefreshToken], [RefreshTokenExpiry], [CreatedAt])
        VALUES
            (NEWID(), 'Sarabjeet', 'oRoZ7ZP4FsMsiV52O8lzRg==', 'Admin', NULL, NULL, SYSDATETIME()),
            (NEWID(), 'Priya', 'jFMP6IkdrRKG9H+Mh8Ozqg==', 'HR', NULL, NULL, SYSDATETIME()),
            (NEWID(), 'Aman', 'AtplmeeBOFR60hWfsXvzhA==', 'Employee', NULL, NULL, SYSDATETIME());
    ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
        DELETE FROM [dbo].[Users]
        WHERE Username IN ('Sarabjeet', 'Priya', 'Aman');
    ");
        }
    }
}
