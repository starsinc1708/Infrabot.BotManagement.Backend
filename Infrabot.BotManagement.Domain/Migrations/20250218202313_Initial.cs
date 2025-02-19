using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrabot.BotManagement.Domain.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Bots",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    BotToken = table.Column<string>(type: "text", nullable: false),
                    FirstName = table.Column<string>(type: "text", nullable: true),
                    LastName = table.Column<string>(type: "text", nullable: true),
                    CanJoinGroups = table.Column<bool>(type: "boolean", nullable: false),
                    CanReadAllGroupMessages = table.Column<bool>(type: "boolean", nullable: false),
                    SupportsInlineQueries = table.Column<bool>(type: "boolean", nullable: false),
                    CanConnectToBusiness = table.Column<bool>(type: "boolean", nullable: false),
                    HasMainWebApp = table.Column<bool>(type: "boolean", nullable: false),
                    Enabled = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bots", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Modules",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: true),
                    IpAddress = table.Column<string>(type: "text", nullable: true),
                    Port = table.Column<int>(type: "integer", nullable: false),
                    HealthCheckEndpoint = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Modules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UpdateSettings",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UpdateSource = table.Column<int>(type: "integer", nullable: false),
                    UpdateType = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UpdateSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TgBotModules",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BotId = table.Column<long>(type: "bigint", nullable: false),
                    ModuleId = table.Column<long>(type: "bigint", nullable: false),
                    Enabled = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TgBotModules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TgBotModules_Bots_BotId",
                        column: x => x.BotId,
                        principalTable: "Bots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TgBotModules_Modules_ModuleId",
                        column: x => x.ModuleId,
                        principalTable: "Modules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TgModuleUpdateSettings",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UpdateSettingsId = table.Column<long>(type: "bigint", nullable: false),
                    ModuleId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TgModuleUpdateSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TgModuleUpdateSettings_Modules_ModuleId",
                        column: x => x.ModuleId,
                        principalTable: "Modules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TgModuleUpdateSettings_UpdateSettings_UpdateSettingsId",
                        column: x => x.UpdateSettingsId,
                        principalTable: "UpdateSettings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TgBotModules_BotId",
                table: "TgBotModules",
                column: "BotId");

            migrationBuilder.CreateIndex(
                name: "IX_TgBotModules_ModuleId",
                table: "TgBotModules",
                column: "ModuleId");

            migrationBuilder.CreateIndex(
                name: "IX_TgModuleUpdateSettings_ModuleId",
                table: "TgModuleUpdateSettings",
                column: "ModuleId");

            migrationBuilder.CreateIndex(
                name: "IX_TgModuleUpdateSettings_UpdateSettingsId",
                table: "TgModuleUpdateSettings",
                column: "UpdateSettingsId");

            migrationBuilder.CreateIndex(
                name: "IX_UpdateSettings_UpdateSource_UpdateType",
                table: "UpdateSettings",
                columns: new[] { "UpdateSource", "UpdateType" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TgBotModules");

            migrationBuilder.DropTable(
                name: "TgModuleUpdateSettings");

            migrationBuilder.DropTable(
                name: "Bots");

            migrationBuilder.DropTable(
                name: "Modules");

            migrationBuilder.DropTable(
                name: "UpdateSettings");
        }
    }
}
