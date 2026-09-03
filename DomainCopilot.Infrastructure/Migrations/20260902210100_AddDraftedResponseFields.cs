using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DomainCopilot.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDraftedResponseFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DraftedResponseText",
                table: "CitizenRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EligibilityReason",
                table: "CitizenRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RequiredDocumentsSummary",
                table: "CitizenRequests",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DraftedResponseText",
                table: "CitizenRequests");

            migrationBuilder.DropColumn(
                name: "EligibilityReason",
                table: "CitizenRequests");

            migrationBuilder.DropColumn(
                name: "RequiredDocumentsSummary",
                table: "CitizenRequests");
        }
    }
}
