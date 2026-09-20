using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BikeRepairShop.Database.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "service_types",
                columns: table => new
                {
                    code = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    display_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_service_types", x => x.code);
                });

            migrationBuilder.CreateTable(
                name: "orders",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    customer_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    phone_number = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    email = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: false),
                    bike_brand = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    service_type = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    due_date = table.Column<DateOnly>(type: "date", nullable: false),
                    notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    placed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_orders", x => x.id);
                    table.ForeignKey(
                        name: "fk_orders_service_types_service_type",
                        column: x => x.service_type,
                        principalTable: "service_types",
                        principalColumn: "code",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "service_types",
                columns: new[] { "code", "display_name", "sort_order" },
                values: new object[,]
                {
                    { "BrakeMaintenance", "Brake maintenance", 3 },
                    { "ChainReplacement", "Chain replacement", 2 },
                    { "WheelAdjustment", "Wheel adjustment", 1 }
                });

            migrationBuilder.CreateIndex(
                name: "ix_orders_service_type",
                table: "orders",
                column: "service_type");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "orders");

            migrationBuilder.DropTable(
                name: "service_types");
        }
    }
}
