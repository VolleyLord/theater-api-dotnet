using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TheaterAPI.Migrations
{
    /// <inheritdoc />
    public partial class FixNegativeValues : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Исправляем отрицательные значения в таблице Spectacles
            migrationBuilder.Sql(@"
                UPDATE ""Spectacles"" 
                SET seats_booked = 0 
                WHERE seats_booked < 0;
            ");
            
            migrationBuilder.Sql(@"
                UPDATE ""Spectacles"" 
                SET seats_bought = 0 
                WHERE seats_bought < 0;
            ");
            
            migrationBuilder.Sql(@"
                UPDATE ""Spectacles"" 
                SET money_earned = 0 
                WHERE money_earned < 0;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
