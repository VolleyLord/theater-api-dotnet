using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TheaterAPI.Migrations
{
    /// <inheritdoc />
    public partial class FixHallIdConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Spectacles_Halls_hallid",
                table: "Spectacles");

            migrationBuilder.DropIndex(
                name: "IX_Spectacles_hallid",
                table: "Spectacles");

            migrationBuilder.DropColumn(
                name: "hallid",
                table: "Spectacles");

            migrationBuilder.AlterColumn<int>(
                name: "hall_id",
                table: "Spectacles",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Spectacles_hall_id",
                table: "Spectacles",
                column: "hall_id");

            migrationBuilder.AddForeignKey(
                name: "FK_Spectacles_Halls_hall_id",
                table: "Spectacles",
                column: "hall_id",
                principalTable: "Halls",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Spectacles_Halls_hall_id",
                table: "Spectacles");

            migrationBuilder.DropIndex(
                name: "IX_Spectacles_hall_id",
                table: "Spectacles");

            migrationBuilder.AlterColumn<int>(
                name: "hall_id",
                table: "Spectacles",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<int>(
                name: "hallid",
                table: "Spectacles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Spectacles_hallid",
                table: "Spectacles",
                column: "hallid");

            migrationBuilder.AddForeignKey(
                name: "FK_Spectacles_Halls_hallid",
                table: "Spectacles",
                column: "hallid",
                principalTable: "Halls",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
