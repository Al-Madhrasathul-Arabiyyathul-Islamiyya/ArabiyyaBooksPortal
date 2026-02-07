using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BooksPortal.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddModule9Entities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ReferenceNo",
                table: "TeacherIssues",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AddColumn<string>(
                name: "PdfFilePath",
                table: "TeacherIssues",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ReferenceNo",
                table: "ReturnSlips",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AddColumn<string>(
                name: "PdfFilePath",
                table: "ReturnSlips",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Key",
                table: "ReferenceCounters",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "ReferenceNo",
                table: "DistributionSlips",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AddColumn<string>(
                name: "PdfFilePath",
                table: "DistributionSlips",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ReferenceNumberFormats",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SlipType = table.Column<int>(type: "int", nullable: false),
                    AcademicYearId = table.Column<int>(type: "int", nullable: false),
                    FormatTemplate = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PaddingWidth = table.Column<int>(type: "int", nullable: false, defaultValue: 6),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReferenceNumberFormats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReferenceNumberFormats_AcademicYears_AcademicYearId",
                        column: x => x.AcademicYearId,
                        principalTable: "AcademicYears",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SlipTemplateSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Key = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SlipTemplateSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TeacherReturnSlips",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReferenceNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TeacherIssueId = table.Column<int>(type: "int", nullable: false),
                    ReceivedById = table.Column<int>(type: "int", nullable: false),
                    ReceivedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PdfFilePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeacherReturnSlips", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeacherReturnSlips_TeacherIssues_TeacherIssueId",
                        column: x => x.TeacherIssueId,
                        principalTable: "TeacherIssues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TeacherReturnSlipItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TeacherReturnSlipId = table.Column<int>(type: "int", nullable: false),
                    TeacherIssueItemId = table.Column<int>(type: "int", nullable: false),
                    BookId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeacherReturnSlipItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeacherReturnSlipItems_Books_BookId",
                        column: x => x.BookId,
                        principalTable: "Books",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TeacherReturnSlipItems_TeacherIssueItems_TeacherIssueItemId",
                        column: x => x.TeacherIssueItemId,
                        principalTable: "TeacherIssueItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TeacherReturnSlipItems_TeacherReturnSlips_TeacherReturnSlipId",
                        column: x => x.TeacherReturnSlipId,
                        principalTable: "TeacherReturnSlips",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReferenceNumberFormats_AcademicYearId",
                table: "ReferenceNumberFormats",
                column: "AcademicYearId");

            migrationBuilder.CreateIndex(
                name: "IX_ReferenceNumberFormats_SlipType_AcademicYearId",
                table: "ReferenceNumberFormats",
                columns: new[] { "SlipType", "AcademicYearId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SlipTemplateSettings_Category",
                table: "SlipTemplateSettings",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_SlipTemplateSettings_Category_Key",
                table: "SlipTemplateSettings",
                columns: new[] { "Category", "Key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TeacherReturnSlipItems_BookId",
                table: "TeacherReturnSlipItems",
                column: "BookId");

            migrationBuilder.CreateIndex(
                name: "IX_TeacherReturnSlipItems_TeacherIssueItemId",
                table: "TeacherReturnSlipItems",
                column: "TeacherIssueItemId");

            migrationBuilder.CreateIndex(
                name: "IX_TeacherReturnSlipItems_TeacherReturnSlipId",
                table: "TeacherReturnSlipItems",
                column: "TeacherReturnSlipId");

            migrationBuilder.CreateIndex(
                name: "IX_TeacherReturnSlips_ReferenceNo",
                table: "TeacherReturnSlips",
                column: "ReferenceNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TeacherReturnSlips_TeacherIssueId",
                table: "TeacherReturnSlips",
                column: "TeacherIssueId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReferenceNumberFormats");

            migrationBuilder.DropTable(
                name: "SlipTemplateSettings");

            migrationBuilder.DropTable(
                name: "TeacherReturnSlipItems");

            migrationBuilder.DropTable(
                name: "TeacherReturnSlips");

            migrationBuilder.DropColumn(
                name: "PdfFilePath",
                table: "TeacherIssues");

            migrationBuilder.DropColumn(
                name: "PdfFilePath",
                table: "ReturnSlips");

            migrationBuilder.DropColumn(
                name: "PdfFilePath",
                table: "DistributionSlips");

            migrationBuilder.AlterColumn<string>(
                name: "ReferenceNo",
                table: "TeacherIssues",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "ReferenceNo",
                table: "ReturnSlips",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Key",
                table: "ReferenceCounters",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "ReferenceNo",
                table: "DistributionSlips",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);
        }
    }
}
