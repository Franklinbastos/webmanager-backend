using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddFixedFinanceIdToFinance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FixedFinanceId",
                table: "Finances",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Finances_FixedFinanceId",
                table: "Finances",
                column: "FixedFinanceId");

            migrationBuilder.AddForeignKey(
                name: "FK_Finances_FixedFinances_FixedFinanceId",
                table: "Finances",
                column: "FixedFinanceId",
                principalTable: "FixedFinances",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Finances_FixedFinances_FixedFinanceId",
                table: "Finances");

            migrationBuilder.DropIndex(
                name: "IX_Finances_FixedFinanceId",
                table: "Finances");

            migrationBuilder.DropColumn(
                name: "FixedFinanceId",
                table: "Finances");
        }
    }
}
