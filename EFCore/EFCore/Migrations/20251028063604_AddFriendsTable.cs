using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace EFCore.Migrations
{
    /// <inheritdoc />
    public partial class AddFriendsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "friends",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    friend_user_id = table.Column<long>(type: "bigint", nullable: false),
                    status = table.Column<short>(type: "smallint", nullable: false),
                    apply_message = table.Column<string>(type: "text", nullable: true),
                    remark_name = table.Column<string>(type: "text", nullable: true),
                    friend_group = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_friend", x => x.id);
                    table.ForeignKey(
                        name: "FK_friends_users_friend_user_id",
                        column: x => x.friend_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_friends_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_friends_created_at",
                table: "friends",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_friends_friend_user_id",
                table: "friends",
                column: "friend_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_friends_status",
                table: "friends",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_friends_user_id_friend_user_id",
                table: "friends",
                columns: new[] { "user_id", "friend_user_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "friends");
        }
    }
}
