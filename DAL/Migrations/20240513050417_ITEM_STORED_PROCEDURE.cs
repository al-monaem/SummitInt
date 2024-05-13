using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class ITEM_STORED_PROCEDURE : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //var queyr = "USE [summit_interview] " +
            //    "GO " +
            //    "SET ANSI_NULLS ON " +
            //    "GO " +
            //    "SET QUOTED_IDENTIFIER ON " +
            //    "GO " +
            //    "CREATE PROCEDURE [dbo].[InsertMultipleItems] " +
            //        "@Item AS dbo.ItemType READONLY " +
            //    "AS " +
            //    "BEGIN " +
            //        "SET NOCOUNT ON; " +
            //        "SET IDENTITY_INSERT dbo.Items ON; " +
            //        "BEGIN TRY " +
            //            "INSERT INTO dbo.Items(Id, ItemName, ItemUnit, ItemQuantity, CategoryId, CreatedAt, UpdatedAt) " +
            //            "SELECT Id, ItemName, ItemUnit, ItemQuantity, CategoryId, CreatedAt, UpdatedAt FROM @Item; " +
            //            "END TRY " +
            //        "BEGIN CATCH " +
            //        "END CATCH " +
            //    "END " +
            //    "GO";
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
