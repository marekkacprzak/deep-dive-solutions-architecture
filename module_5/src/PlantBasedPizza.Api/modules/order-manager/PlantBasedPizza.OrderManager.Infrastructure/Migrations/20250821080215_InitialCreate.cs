using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PlantBasedPizza.OrderManager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "orders",
                columns: table => new
                {
                    OrderIdentifier = table.Column<string>(type: "text", nullable: false),
                    OrderNumber = table.Column<string>(type: "text", nullable: false),
                    OrderDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AwaitingCollection = table.Column<bool>(type: "boolean", nullable: false),
                    OrderSubmittedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    OrderCompletedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    OrderType = table.Column<int>(type: "integer", nullable: false),
                    CustomerIdentifier = table.Column<string>(type: "text", nullable: false),
                    TotalPrice = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_orders", x => x.OrderIdentifier);
                });

            migrationBuilder.CreateTable(
                name: "orders_outboxitems",
                columns: table => new
                {
                    ItemId = table.Column<string>(type: "text", nullable: false),
                    EventType = table.Column<string>(type: "text", nullable: false),
                    EventData = table.Column<string>(type: "text", nullable: false),
                    EventTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Processed = table.Column<bool>(type: "boolean", nullable: false),
                    Failed = table.Column<bool>(type: "boolean", nullable: false),
                    FailureReason = table.Column<string>(type: "text", nullable: true),
                    TraceId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_orders_outboxitems", x => x.ItemId);
                });

            migrationBuilder.CreateTable(
                name: "DeliveryDetails",
                columns: table => new
                {
                    DeliveryDetailsId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AddressLine1 = table.Column<string>(type: "text", nullable: false),
                    AddressLine2 = table.Column<string>(type: "text", nullable: false),
                    AddressLine3 = table.Column<string>(type: "text", nullable: false),
                    AddressLine4 = table.Column<string>(type: "text", nullable: false),
                    AddressLine5 = table.Column<string>(type: "text", nullable: false),
                    Postcode = table.Column<string>(type: "text", nullable: false),
                    OrderIdentifier = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeliveryDetails", x => x.DeliveryDetailsId);
                    table.ForeignKey(
                        name: "FK_DeliveryDetails_orders_OrderIdentifier",
                        column: x => x.OrderIdentifier,
                        principalTable: "orders",
                        principalColumn: "OrderIdentifier");
                });

            migrationBuilder.CreateTable(
                name: "OrderHistory",
                columns: table => new
                {
                    OrderHistoryId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Description = table.Column<string>(type: "text", nullable: false),
                    HistoryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    OrderIdentifier = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderHistory", x => x.OrderHistoryId);
                    table.ForeignKey(
                        name: "FK_OrderHistory_orders_OrderIdentifier",
                        column: x => x.OrderIdentifier,
                        principalTable: "orders",
                        principalColumn: "OrderIdentifier",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrderItem",
                columns: table => new
                {
                    OrderItemId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RecipeIdentifier = table.Column<string>(type: "text", nullable: false),
                    ItemName = table.Column<string>(type: "text", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    Price = table.Column<decimal>(type: "numeric", nullable: false),
                    OrderIdentifier = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderItem", x => x.OrderItemId);
                    table.ForeignKey(
                        name: "FK_OrderItem_orders_OrderIdentifier",
                        column: x => x.OrderIdentifier,
                        principalTable: "orders",
                        principalColumn: "OrderIdentifier",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryDetails_OrderIdentifier",
                table: "DeliveryDetails",
                column: "OrderIdentifier",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderHistory_OrderIdentifier",
                table: "OrderHistory",
                column: "OrderIdentifier");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItem_OrderIdentifier",
                table: "OrderItem",
                column: "OrderIdentifier");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DeliveryDetails");

            migrationBuilder.DropTable(
                name: "OrderHistory");

            migrationBuilder.DropTable(
                name: "OrderItem");

            migrationBuilder.DropTable(
                name: "orders_outboxitems");

            migrationBuilder.DropTable(
                name: "orders");
        }
    }
}
