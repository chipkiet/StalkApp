using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChatApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPinboardConnections : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PinboardConnections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ConversationId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    Label = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PinboardConnections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PinboardConnections_PinboardItems_SourceItemId",
                        column: x => x.SourceItemId,
                        principalTable: "PinboardItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PinboardConnections_PinboardItems_TargetItemId",
                        column: x => x.TargetItemId,
                        principalTable: "PinboardItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PinboardConnections_ConversationId",
                table: "PinboardConnections",
                column: "ConversationId");

            migrationBuilder.CreateIndex(
                name: "IX_PinboardConnections_SourceItemId",
                table: "PinboardConnections",
                column: "SourceItemId");

            migrationBuilder.CreateIndex(
                name: "IX_PinboardConnections_TargetItemId",
                table: "PinboardConnections",
                column: "TargetItemId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PinboardConnections");
        }
    }
}
