using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ControleDeGasto.API.Migrations
{
    /// <inheritdoc />
    public partial class AddSocialWalletsAndPlanning : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DueDate",
                table: "Transactions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "InstallmentNumber",
                table: "Transactions",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "InstallmentPlanId",
                table: "Transactions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SettledAt",
                table: "Transactions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Transactions",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Settled");

            migrationBuilder.AddColumn<Guid>(
                name: "WalletId",
                table: "Transactions",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Friendships",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RequesterId = table.Column<Guid>(type: "uuid", nullable: false),
                    AddresseeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    RequestedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RespondedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    BlockedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Friendships", x => x.Id);
                    table.CheckConstraint("CK_Friendships_DifferentUsers", "\"RequesterId\" <> \"AddresseeId\"");
                    table.ForeignKey(
                        name: "FK_Friendships_AspNetUsers_AddresseeId",
                        column: x => x.AddresseeId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Friendships_AspNetUsers_RequesterId",
                        column: x => x.RequesterId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SavingsGoalMembers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SavingsGoalId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Role = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SavingsGoalMembers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SavingsGoalMembers_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SavingsGoalMembers_SavingsGoals_SavingsGoalId",
                        column: x => x.SavingsGoalId,
                        principalTable: "SavingsGoals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tags",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Color = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tags", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tags_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TransactionShares",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TransactionId = table.Column<Guid>(type: "uuid", nullable: false),
                    FriendUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    SettledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionShares", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TransactionShares_AspNetUsers_FriendUserId",
                        column: x => x.FriendUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TransactionShares_Transactions_TransactionId",
                        column: x => x.TransactionId,
                        principalTable: "Transactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Wallets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    Kind = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Color = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    Icon = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    InitialBalance = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CreditLimit = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    StatementClosingDay = table.Column<int>(type: "integer", nullable: true),
                    PaymentDueDay = table.Column<int>(type: "integer", nullable: true),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                    Active = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Wallets", x => x.Id);
                    table.CheckConstraint("CK_Wallets_PaymentDueDay", "\"PaymentDueDay\" IS NULL OR (\"PaymentDueDay\" BETWEEN 1 AND 31)");
                    table.CheckConstraint("CK_Wallets_StatementClosingDay", "\"StatementClosingDay\" IS NULL OR (\"StatementClosingDay\" BETWEEN 1 AND 31)");
                    table.ForeignKey(
                        name: "FK_Wallets_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TransactionTags",
                columns: table => new
                {
                    TransactionId = table.Column<Guid>(type: "uuid", nullable: false),
                    TagId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionTags", x => new { x.TransactionId, x.TagId });
                    table.ForeignKey(
                        name: "FK_TransactionTags_Tags_TagId",
                        column: x => x.TagId,
                        principalTable: "Tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TransactionTags_Transactions_TransactionId",
                        column: x => x.TransactionId,
                        principalTable: "Transactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FixedEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Kind = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: true),
                    WalletId = table.Column<Guid>(type: "uuid", nullable: true),
                    Description = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    DayOfMonth = table.Column<int>(type: "integer", nullable: false),
                    StartsOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndsOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Active = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FixedEntries", x => x.Id);
                    table.CheckConstraint("CK_FixedEntries_DayOfMonth", "\"DayOfMonth\" BETWEEN 1 AND 31");
                    table.ForeignKey(
                        name: "FK_FixedEntries_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FixedEntries_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FixedEntries_Wallets_WalletId",
                        column: x => x.WalletId,
                        principalTable: "Wallets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InstallmentPlans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    WalletId = table.Column<Guid>(type: "uuid", nullable: true),
                    Description = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    TotalAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    InstallmentCount = table.Column<int>(type: "integer", nullable: false),
                    FirstDueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PaymentMethod = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InstallmentPlans", x => x.Id);
                    table.CheckConstraint("CK_InstallmentPlans_InstallmentCount", "\"InstallmentCount\" BETWEEN 2 AND 360");
                    table.ForeignKey(
                        name: "FK_InstallmentPlans_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InstallmentPlans_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InstallmentPlans_Wallets_WalletId",
                        column: x => x.WalletId,
                        principalTable: "Wallets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WalletTransfers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    FromWalletId = table.Column<Guid>(type: "uuid", nullable: false),
                    ToWalletId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    OccurredOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Note = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WalletTransfers", x => x.Id);
                    table.CheckConstraint("CK_WalletTransfers_DifferentWallets", "\"FromWalletId\" <> \"ToWalletId\"");
                    table.ForeignKey(
                        name: "FK_WalletTransfers_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WalletTransfers_Wallets_FromWalletId",
                        column: x => x.FromWalletId,
                        principalTable: "Wallets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WalletTransfers_Wallets_ToWalletId",
                        column: x => x.ToWalletId,
                        principalTable: "Wallets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_InstallmentPlanId",
                table: "Transactions",
                column: "InstallmentPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_UserId_DueDate_Pending",
                table: "Transactions",
                columns: new[] { "UserId", "DueDate" },
                filter: "\"Status\" = 'Pending'");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_UserId_WalletId_OccurredOn",
                table: "Transactions",
                columns: new[] { "UserId", "WalletId", "OccurredOn" });

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_WalletId",
                table: "Transactions",
                column: "WalletId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Transactions_InstallmentNumber",
                table: "Transactions",
                sql: "\"InstallmentNumber\" IS NULL OR \"InstallmentNumber\" >= 1");

            migrationBuilder.CreateIndex(
                name: "IX_FixedEntries_CategoryId",
                table: "FixedEntries",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_FixedEntries_UserId_Active_Kind",
                table: "FixedEntries",
                columns: new[] { "UserId", "Active", "Kind" });

            migrationBuilder.CreateIndex(
                name: "IX_FixedEntries_WalletId",
                table: "FixedEntries",
                column: "WalletId");

            migrationBuilder.CreateIndex(
                name: "IX_Friendships_AddresseeId_Status",
                table: "Friendships",
                columns: new[] { "AddresseeId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Friendships_RequesterId_AddresseeId",
                table: "Friendships",
                columns: new[] { "RequesterId", "AddresseeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Friendships_RequesterId_Status",
                table: "Friendships",
                columns: new[] { "RequesterId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_InstallmentPlans_CategoryId",
                table: "InstallmentPlans",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_InstallmentPlans_UserId_CreatedAt",
                table: "InstallmentPlans",
                columns: new[] { "UserId", "CreatedAt" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_InstallmentPlans_WalletId",
                table: "InstallmentPlans",
                column: "WalletId");

            migrationBuilder.CreateIndex(
                name: "IX_SavingsGoalMembers_SavingsGoalId_Owner",
                table: "SavingsGoalMembers",
                column: "SavingsGoalId",
                unique: true,
                filter: "\"Role\" = 'Owner'");

            migrationBuilder.CreateIndex(
                name: "IX_SavingsGoalMembers_SavingsGoalId_UserId",
                table: "SavingsGoalMembers",
                columns: new[] { "SavingsGoalId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SavingsGoalMembers_UserId",
                table: "SavingsGoalMembers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Tags_UserId_Name",
                table: "Tags",
                columns: new[] { "UserId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TransactionShares_FriendUserId_SettledAt",
                table: "TransactionShares",
                columns: new[] { "FriendUserId", "SettledAt" });

            migrationBuilder.CreateIndex(
                name: "IX_TransactionShares_TransactionId_FriendUserId",
                table: "TransactionShares",
                columns: new[] { "TransactionId", "FriendUserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TransactionTags_TagId",
                table: "TransactionTags",
                column: "TagId");

            migrationBuilder.CreateIndex(
                name: "IX_Wallets_UserId_Active",
                table: "Wallets",
                columns: new[] { "UserId", "Active" });

            migrationBuilder.CreateIndex(
                name: "IX_Wallets_UserId_Default",
                table: "Wallets",
                column: "UserId",
                unique: true,
                filter: "\"IsDefault\" = true");

            migrationBuilder.CreateIndex(
                name: "IX_Wallets_UserId_Name",
                table: "Wallets",
                columns: new[] { "UserId", "Name" },
                unique: true,
                filter: "\"Active\" = true");

            migrationBuilder.CreateIndex(
                name: "IX_WalletTransfers_FromWalletId",
                table: "WalletTransfers",
                column: "FromWalletId");

            migrationBuilder.CreateIndex(
                name: "IX_WalletTransfers_ToWalletId",
                table: "WalletTransfers",
                column: "ToWalletId");

            migrationBuilder.CreateIndex(
                name: "IX_WalletTransfers_UserId_OccurredOn",
                table: "WalletTransfers",
                columns: new[] { "UserId", "OccurredOn" },
                descending: new[] { false, true });

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_InstallmentPlans_InstallmentPlanId",
                table: "Transactions",
                column: "InstallmentPlanId",
                principalTable: "InstallmentPlans",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Wallets_WalletId",
                table: "Transactions",
                column: "WalletId",
                principalTable: "Wallets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            // O acesso ao cofrinho passou a ser decidido pela participação. Sem esta carga, todo
            // cofrinho criado antes desta versão ficaria invisível até para quem o criou, porque
            // nenhuma consulta encontraria a linha de dono.
            migrationBuilder.Sql("""
                INSERT INTO "SavingsGoalMembers" ("Id", "SavingsGoalId", "UserId", "Role", "JoinedAt")
                SELECT gen_random_uuid(), g."Id", g."UserId", 'Owner', g."CreatedAt"
                FROM "SavingsGoals" g
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM "SavingsGoalMembers" m
                    WHERE m."SavingsGoalId" = g."Id"
                      AND m."UserId" = g."UserId"
                );
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_InstallmentPlans_InstallmentPlanId",
                table: "Transactions");

            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Wallets_WalletId",
                table: "Transactions");

            migrationBuilder.DropTable(
                name: "FixedEntries");

            migrationBuilder.DropTable(
                name: "Friendships");

            migrationBuilder.DropTable(
                name: "InstallmentPlans");

            migrationBuilder.DropTable(
                name: "SavingsGoalMembers");

            migrationBuilder.DropTable(
                name: "TransactionShares");

            migrationBuilder.DropTable(
                name: "TransactionTags");

            migrationBuilder.DropTable(
                name: "WalletTransfers");

            migrationBuilder.DropTable(
                name: "Tags");

            migrationBuilder.DropTable(
                name: "Wallets");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_InstallmentPlanId",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_UserId_DueDate_Pending",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_UserId_WalletId_OccurredOn",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_WalletId",
                table: "Transactions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Transactions_InstallmentNumber",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "DueDate",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "InstallmentNumber",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "InstallmentPlanId",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "SettledAt",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "WalletId",
                table: "Transactions");
        }
    }
}
