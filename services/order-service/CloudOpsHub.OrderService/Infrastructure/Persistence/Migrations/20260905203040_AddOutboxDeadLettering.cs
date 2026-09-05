using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CloudOpsHub.OrderService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddOutboxDeadLettering : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_OutboxMessages_ProcessedOnUtc_NextAttemptAtUtc",
                table: "OutboxMessages");

            migrationBuilder.AddColumn<DateTime>(
                name: "DeadLetteredOnUtc",
                table: "OutboxMessages",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_ProcessedOnUtc_DeadLetteredOnUtc_NextAttempt~",
                table: "OutboxMessages",
                columns: new[] { "ProcessedOnUtc", "DeadLetteredOnUtc", "NextAttemptAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_OutboxMessages_ProcessedOnUtc_DeadLetteredOnUtc_NextAttempt~",
                table: "OutboxMessages");

            migrationBuilder.DropColumn(
                name: "DeadLetteredOnUtc",
                table: "OutboxMessages");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_ProcessedOnUtc_NextAttemptAtUtc",
                table: "OutboxMessages",
                columns: new[] { "ProcessedOnUtc", "NextAttemptAtUtc" });
        }
    }
}
