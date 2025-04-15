using System;

namespace EmployeeManagement.Infrastructure.SqlScripts.StoredProcedures
{
    public static class GetFilteredEmployees
    {
        public const string CreateGetFilteredEmployeesProcedure = @"
ALTER PROCEDURE [dbo].[GetFilteredEmployees]
    @Name NVARCHAR(100) = NULL,
    @Department NVARCHAR(50) = NULL,
    @MinSalary DECIMAL(18,2) = NULL,
    @MaxSalary DECIMAL(18,2) = NULL,
    @SortBy NVARCHAR(50) = 'CreatedAt',
    @SortOrder NVARCHAR(4) = 'DESC',
    @PageNumber INT = 1,
    @PageSize INT = 10
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Sql NVARCHAR(MAX) = N'
SELECT 
    Id, 
    FullName, 
    Department, 
    Salary, 
    CreatedAt
FROM Employees
WHERE IsDeleted = 0';

    IF @Name IS NOT NULL AND LTRIM(RTRIM(@Name)) <> ''
        SET @Sql += ' AND FullName LIKE ''%'' + REPLACE(@Name, ''''''', '''''''') + ''%''';

    IF @Department IS NOT NULL AND LTRIM(RTRIM(@Department)) <> ''
        SET @Sql += ' AND Department = ''' + REPLACE(@Department, ''''''', '''''''') + '''';

    IF @MinSalary IS NOT NULL AND @MinSalary > 0
        SET @Sql += ' AND Salary >= ' + CAST(@MinSalary AS NVARCHAR);

    IF @MaxSalary IS NOT NULL AND @MaxSalary > 0
        SET @Sql += ' AND Salary <= ' + CAST(@MaxSalary AS NVARCHAR);

    IF @SortBy NOT IN (''FullName'', ''Salary'', ''Department'', ''CreatedAt'')
        SET @SortBy = ''CreatedAt'';

    IF UPPER(@SortOrder) NOT IN (''ASC'', ''DESC'')
        SET @SortOrder = ''DESC'';

    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;

    SET @Sql += ' ORDER BY ' + QUOTENAME(@SortBy) + ' ' + @SortOrder;
    SET @Sql += ' OFFSET ' + CAST(@Offset AS NVARCHAR) + ' ROWS';
    SET @Sql += ' FETCH NEXT ' + CAST(@PageSize AS NVARCHAR) + ' ROWS ONLY;';

    SELECT @Sql AS QueryToExecute;
END
";
    }
}
