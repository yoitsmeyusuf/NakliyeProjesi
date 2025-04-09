using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NakliyeProjesi.Migrations
{
    /// <inheritdoc />
    public partial class AddAcceptedBidToShipment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EmailConfirmationToken",
                table: "Users",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "EmailConfirmed",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PasswordResetToken",
                table: "Users",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "PasswordResetTokenExpires",
                table: "Users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AcceptedBidId",
                table: "Shipments",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AcceptedBidId1",
                table: "Shipments",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsCompleted",
                table: "Shipments",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Shipments_AcceptedBidId1",
                table: "Shipments",
                column: "AcceptedBidId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Shipments_Bids_AcceptedBidId1",
                table: "Shipments",
                column: "AcceptedBidId1",
                principalTable: "Bids",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Shipments_Bids_AcceptedBidId1",
                table: "Shipments");

            migrationBuilder.DropIndex(
                name: "IX_Shipments_AcceptedBidId1",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "EmailConfirmationToken",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "EmailConfirmed",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PasswordResetToken",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PasswordResetTokenExpires",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "AcceptedBidId",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "AcceptedBidId1",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "IsCompleted",
                table: "Shipments");
        }
    }
}
