using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TheaterAPI.Migrations
{
    /// <inheritdoc />
    public partial class UpdateHallIdToNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Spectacles_Halls_hall_id",
                table: "Spectacles");

            migrationBuilder.AlterColumn<int>(
                name: "hall_id",
                table: "Spectacles",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddForeignKey(
                name: "FK_Spectacles_Halls_hall_id",
                table: "Spectacles",
                column: "hall_id",
                principalTable: "Halls",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Spectacles_Halls_hall_id",
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

            migrationBuilder.AddForeignKey(
                name: "FK_Spectacles_Halls_hall_id",
                table: "Spectacles",
                column: "hall_id",
                principalTable: "Halls",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
