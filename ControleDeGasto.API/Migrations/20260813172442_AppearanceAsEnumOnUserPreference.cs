using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ControleDeGasto.API.Migrations
{
    /// <inheritdoc />
    public partial class AppearanceAsEnumOnUserPreference : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserPreferences_Appearances_AppearanceId",
                table: "UserPreferences");

            migrationBuilder.DropTable(
                name: "Appearances");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserPreferences",
                table: "UserPreferences");

            migrationBuilder.DropIndex(
                name: "IX_UserPreferences_AppearanceId",
                table: "UserPreferences");

            migrationBuilder.DropIndex(
                name: "IX_UserPreferences_UserId",
                table: "UserPreferences");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "UserPreferences");

            migrationBuilder.DropColumn(
                name: "AppearanceId",
                table: "UserPreferences");

            // Colunas novas entram como nullable, recebem backfill e só então viram NOT NULL.
            // O scaffold original preenchia Appearance com "" (valor inválido para o enum) e
            // CreatedAt com 0001-01-01, o que quebraria qualquer linha pré-existente.
            migrationBuilder.AddColumn<string>(
                name: "Appearance",
                table: "UserPreferences",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "UserPreferences",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.Sql(@"
                UPDATE ""UserPreferences""
                SET ""Appearance"" = 'Light'
                WHERE ""Appearance"" IS NULL;");

            migrationBuilder.Sql(@"
                UPDATE ""UserPreferences""
                SET ""CreatedAt"" = now() AT TIME ZONE 'UTC'
                WHERE ""CreatedAt"" IS NULL;");

            // Os parâmetros old* são obrigatórios aqui: sem eles o EF assume que a coluna já era
            // NOT NULL e não emite o SET NOT NULL.
            migrationBuilder.AlterColumn<string>(
                name: "Appearance",
                table: "UserPreferences",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "UserPreferences",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "UserPreferences",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserPreferences",
                table: "UserPreferences",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_UserPreferences",
                table: "UserPreferences");

            migrationBuilder.DropColumn(
                name: "Appearance",
                table: "UserPreferences");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "UserPreferences");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "UserPreferences");

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "UserPreferences",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "AppearanceId",
                table: "UserPreferences",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserPreferences",
                table: "UserPreferences",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "Appearances",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Key = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Label = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Appearances", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserPreferences_AppearanceId",
                table: "UserPreferences",
                column: "AppearanceId");

            migrationBuilder.CreateIndex(
                name: "IX_UserPreferences_UserId",
                table: "UserPreferences",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Appearances_Key",
                table: "Appearances",
                column: "Key",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_UserPreferences_Appearances_AppearanceId",
                table: "UserPreferences",
                column: "AppearanceId",
                principalTable: "Appearances",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
