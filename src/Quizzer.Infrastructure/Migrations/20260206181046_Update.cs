using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Quizzer.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsCorrect",
                table: "Options",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<long>(
                name: "PublishedAt",
                table: "ExamVersions",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "CreatedAt",
                table: "ExamVersions",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<long>(
                name: "CreatedAt",
                table: "Exams",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<long>(
                name: "StartedAt",
                table: "Attempts",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<long>(
                name: "FinishedAt",
                table: "Attempts",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "QuestionStats",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    QuestionKey = table.Column<Guid>(type: "TEXT", nullable: false),
                    LastSeenAt = table.Column<long>(type: "INTEGER", nullable: true),
                    LastCorrectAt = table.Column<long>(type: "INTEGER", nullable: true),
                    CorrectCount = table.Column<int>(type: "INTEGER", nullable: false),
                    WrongCount = table.Column<int>(type: "INTEGER", nullable: false),
                    EaseFactor = table.Column<double>(type: "REAL", nullable: false),
                    IntervalDays = table.Column<int>(type: "INTEGER", nullable: false),
                    DueAt = table.Column<long>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestionStats", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_QuestionStats_QuestionKey",
                table: "QuestionStats",
                column: "QuestionKey",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QuestionStats");

            migrationBuilder.DropColumn(
                name: "IsCorrect",
                table: "Options");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "PublishedAt",
                table: "ExamVersions",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "ExamVersions",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "Exams",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "StartedAt",
                table: "Attempts",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "FinishedAt",
                table: "Attempts",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "INTEGER",
                oldNullable: true);
        }
    }
}
