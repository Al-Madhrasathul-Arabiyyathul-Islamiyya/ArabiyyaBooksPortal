using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BooksPortal.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTeacherIssueEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TeacherIssues",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReferenceNo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    AcademicYearId = table.Column<int>(type: "int", nullable: false),
                    TeacherId = table.Column<int>(type: "int", nullable: false),
                    IssuedById = table.Column<int>(type: "int", nullable: false),
                    IssuedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpectedReturnDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeacherIssues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeacherIssues_AcademicYears_AcademicYearId",
                        column: x => x.AcademicYearId,
                        principalTable: "AcademicYears",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TeacherIssues_Teachers_TeacherId",
                        column: x => x.TeacherId,
                        principalTable: "Teachers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TeacherIssueItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TeacherIssueId = table.Column<int>(type: "int", nullable: false),
                    BookId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    ReturnedQuantity = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    ReturnedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReceivedById = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeacherIssueItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeacherIssueItems_Books_BookId",
                        column: x => x.BookId,
                        principalTable: "Books",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TeacherIssueItems_TeacherIssues_TeacherIssueId",
                        column: x => x.TeacherIssueId,
                        principalTable: "TeacherIssues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TeacherIssueItems_BookId",
                table: "TeacherIssueItems",
                column: "BookId");

            migrationBuilder.CreateIndex(
                name: "IX_TeacherIssueItems_TeacherIssueId",
                table: "TeacherIssueItems",
                column: "TeacherIssueId");

            migrationBuilder.CreateIndex(
                name: "IX_TeacherIssues_AcademicYearId",
                table: "TeacherIssues",
                column: "AcademicYearId");

            migrationBuilder.CreateIndex(
                name: "IX_TeacherIssues_ReferenceNo",
                table: "TeacherIssues",
                column: "ReferenceNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TeacherIssues_Status",
                table: "TeacherIssues",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_TeacherIssues_TeacherId",
                table: "TeacherIssues",
                column: "TeacherId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TeacherIssueItems");

            migrationBuilder.DropTable(
                name: "TeacherIssues");
        }
    }
}
