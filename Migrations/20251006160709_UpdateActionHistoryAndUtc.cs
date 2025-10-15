using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TheaterAPI.Migrations
{
    /// <inheritdoc />
    public partial class UpdateActionHistoryAndUtc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ActionHistory_Users_userid",
                table: "ActionHistory");

            migrationBuilder.DropIndex(
                name: "IX_ActionHistory_userid",
                table: "ActionHistory");

            migrationBuilder.DropColumn(
                name: "userid",
                table: "ActionHistory");

            migrationBuilder.AlterColumn<string>(
                name: "old_value",
                table: "ActionHistory",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "new_value",
                table: "ActionHistory",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "metadata",
                table: "ActionHistory",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.CreateIndex(
                name: "IX_ActionHistory_user_id",
                table: "ActionHistory",
                column: "user_id");

            migrationBuilder.AddForeignKey(
                name: "FK_ActionHistory_Users_user_id",
                table: "ActionHistory",
                column: "user_id",
                principalTable: "Users",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ActionHistory_Users_user_id",
                table: "ActionHistory");

            migrationBuilder.DropIndex(
                name: "IX_ActionHistory_user_id",
                table: "ActionHistory");

            migrationBuilder.AlterColumn<string>(
                name: "old_value",
                table: "ActionHistory",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "new_value",
                table: "ActionHistory",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "metadata",
                table: "ActionHistory",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "userid",
                table: "ActionHistory",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ActionHistory_userid",
                table: "ActionHistory",
                column: "userid");

            migrationBuilder.AddForeignKey(
                name: "FK_ActionHistory_Users_userid",
                table: "ActionHistory",
                column: "userid",
                principalTable: "Users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
