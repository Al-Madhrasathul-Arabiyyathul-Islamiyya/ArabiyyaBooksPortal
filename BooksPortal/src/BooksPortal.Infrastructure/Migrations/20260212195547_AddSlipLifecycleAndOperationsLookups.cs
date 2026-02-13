using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BooksPortal.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSlipLifecycleAndOperationsLookups : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CancelledAt",
                table: "TeacherIssues",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CancelledById",
                table: "TeacherIssues",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FinalizedAt",
                table: "TeacherIssues",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FinalizedById",
                table: "TeacherIssues",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LifecycleStatus",
                table: "TeacherIssues",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "CancelledAt",
                table: "DistributionSlips",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CancelledById",
                table: "DistributionSlips",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FinalizedAt",
                table: "DistributionSlips",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FinalizedById",
                table: "DistributionSlips",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LifecycleStatus",
                table: "DistributionSlips",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_TeacherIssues_LifecycleStatus",
                table: "TeacherIssues",
                column: "LifecycleStatus");

            migrationBuilder.CreateIndex(
                name: "IX_DistributionSlips_LifecycleStatus",
                table: "DistributionSlips",
                column: "LifecycleStatus");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TeacherIssues_LifecycleStatus",
                table: "TeacherIssues");

            migrationBuilder.DropIndex(
                name: "IX_DistributionSlips_LifecycleStatus",
                table: "DistributionSlips");

            migrationBuilder.DropColumn(
                name: "CancelledAt",
                table: "TeacherIssues");

            migrationBuilder.DropColumn(
                name: "CancelledById",
                table: "TeacherIssues");

            migrationBuilder.DropColumn(
                name: "FinalizedAt",
                table: "TeacherIssues");

            migrationBuilder.DropColumn(
                name: "FinalizedById",
                table: "TeacherIssues");

            migrationBuilder.DropColumn(
                name: "LifecycleStatus",
                table: "TeacherIssues");

            migrationBuilder.DropColumn(
                name: "CancelledAt",
                table: "DistributionSlips");

            migrationBuilder.DropColumn(
                name: "CancelledById",
                table: "DistributionSlips");

            migrationBuilder.DropColumn(
                name: "FinalizedAt",
                table: "DistributionSlips");

            migrationBuilder.DropColumn(
                name: "FinalizedById",
                table: "DistributionSlips");

            migrationBuilder.DropColumn(
                name: "LifecycleStatus",
                table: "DistributionSlips");
        }
    }
}
