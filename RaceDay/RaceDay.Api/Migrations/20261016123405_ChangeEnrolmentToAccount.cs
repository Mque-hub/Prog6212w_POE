using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RaceDay.Api.Migrations
{
    /// <inheritdoc />
    public partial class ChangeEnrolmentToAccount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Enrolments_Participants_ParticipantId",
                table: "Enrolments");

            migrationBuilder.DropColumn(
                name: "EnrolmentType",
                table: "Enrolments");

            migrationBuilder.RenameColumn(
                name: "ParticipantId",
                table: "Enrolments",
                newName: "AccountID");

            migrationBuilder.RenameIndex(
                name: "IX_Enrolments_ParticipantId",
                table: "Enrolments",
                newName: "IX_Enrolments_AccountID");

            migrationBuilder.AddColumn<DateTime>(
                name: "EnrolmentDate",
                table: "Enrolments",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddForeignKey(
                name: "FK_Enrolments_Accounts_AccountID",
                table: "Enrolments",
                column: "AccountID",
                principalTable: "Accounts",
                principalColumn: "AccountId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Enrolments_Accounts_AccountID",
                table: "Enrolments");

            migrationBuilder.DropColumn(
                name: "EnrolmentDate",
                table: "Enrolments");

            migrationBuilder.RenameColumn(
                name: "AccountID",
                table: "Enrolments",
                newName: "ParticipantId");

            migrationBuilder.RenameIndex(
                name: "IX_Enrolments_AccountID",
                table: "Enrolments",
                newName: "IX_Enrolments_ParticipantId");

            migrationBuilder.AddColumn<string>(
                name: "EnrolmentType",
                table: "Enrolments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_Enrolments_Participants_ParticipantId",
                table: "Enrolments",
                column: "ParticipantId",
                principalTable: "Participants",
                principalColumn: "ParticipantId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
