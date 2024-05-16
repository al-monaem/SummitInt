using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class STORED_PROCEDURES : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var query = @"CREATE TYPE dbo.CategoryType AS TABLE
                        (
                            Id INT NOT NULL,
                            NAME VARCHAR(50) NOT NULL,
                            CreatedBy VARCHAR(450) NOT NULL,
                            UpdatedBy VARCHAR(450) NOT NULL,
                            CreatedAt datetime2,
                            UpdatedAt datetime2
                        );
                        GO

                        CREATE PROCEDURE dbo.InsertMultipleCategory
                            @Category AS dbo.CategoryType READONLY
                        AS
                        BEGIN
                            SET NOCOUNT ON;
                            SET IDENTITY_INSERT Categories ON;

                            INSERT INTO dbo.Categories(Id, Name, CreatedBy, UpdatedBy, CreatedAt, UpdatedAt)
                            SELECT Id, Name, CreatedBy, UpdatedBy, CreatedAt, UpdatedAt FROM @Category;
                        END
                        GO";
            migrationBuilder.Sql(query);

            query = @"CREATE TYPE dbo.ItemType AS TABLE
                    (
                        Id INT NOT NULL,
                        ItemName VARCHAR(50) NOT NULL,
	                    ItemUnit VARCHAR(50) NOT NULL,
	                    ItemQuantity VARCHAR(50) NOT NULL,
	                    CategoryId INT NOT NULL,
	                    ItemDescription VARCHAR(450) NOT NULL,
                        CreatedAt datetime2,
                        UpdatedAt datetime2
                    );
                    GO

                    CREATE PROCEDURE dbo.InsertMultipleItems
                        @Item AS dbo.ItemType READONLY
                    AS
                    BEGIN
                        SET NOCOUNT ON;
                        SET IDENTITY_INSERT items ON;

                        INSERT INTO dbo.items(Id, ItemName, ItemUnit, ItemQuantity, CategoryId, ItemDescription, CreatedAt, UpdatedAt)
                        SELECT Id, ItemName, ItemUnit, ItemQuantity, CategoryId, ItemDescription, CreatedAt, UpdatedAt FROM @Item;
                    END
                    GO";
            migrationBuilder.Sql(query);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
