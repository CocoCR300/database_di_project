using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Restify.API.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Perk",
                columns: table => new
                {
                    perkId = table.Column<uint>(type: "int unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Perk", x => x.perkId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PhoneNumber",
                columns: table => new
                {
                    Number = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Photo",
                columns: table => new
                {
                    Ordering = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    FileName = table.Column<string>(type: "CHAR(75)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "UserRole",
                columns: table => new
                {
                    userRoleId = table.Column<uint>(type: "int unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Type = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRole", x => x.userRoleId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    userName = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    userRoleId = table.Column<uint>(type: "int unsigned", nullable: false),
                    Password = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.userName);
                    table.ForeignKey(
                        name: "FK_User_UserRole_userRoleId",
                        column: x => x.userRoleId,
                        principalTable: "UserRole",
                        principalColumn: "userRoleId",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Person",
                columns: table => new
                {
                    personId = table.Column<uint>(type: "int unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    userName = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FirstName = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LastName = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EmailAddress = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Person", x => x.personId);
                    table.ForeignKey(
                        name: "FK_Person_User_userName",
                        column: x => x.userName,
                        principalTable: "User",
                        principalColumn: "userName",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Lodging",
                columns: table => new
                {
                    lodgingId = table.Column<uint>(type: "int unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ownerPersonId = table.Column<uint>(type: "int unsigned", nullable: false),
                    Address = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    lodgingType = table.Column<string>(type: "CHAR(50)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EmailAddress = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lodging", x => x.lodgingId);
                    table.ForeignKey(
                        name: "FK_Lodging_Person_ownerPersonId",
                        column: x => x.ownerPersonId,
                        principalTable: "Person",
                        principalColumn: "personId",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PersonPhoneNumber",
                columns: table => new
                {
                    phoneNumber = table.Column<string>(type: "CHAR(30)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PersonId = table.Column<uint>(type: "int unsigned", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonPhoneNumber", x => new { x.PersonId, x.phoneNumber });
                    table.ForeignKey(
                        name: "FK_PersonPhoneNumber_Person_PersonId",
                        column: x => x.PersonId,
                        principalTable: "Person",
                        principalColumn: "personId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Booking",
                columns: table => new
                {
                    bookingId = table.Column<uint>(type: "int unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    customerPersonId = table.Column<uint>(type: "int unsigned", nullable: false),
                    LodgingId = table.Column<uint>(type: "int unsigned", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Booking", x => x.bookingId);
                    table.ForeignKey(
                        name: "FK_Booking_Lodging_LodgingId",
                        column: x => x.LodgingId,
                        principalTable: "Lodging",
                        principalColumn: "lodgingId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Booking_Person_customerPersonId",
                        column: x => x.customerPersonId,
                        principalTable: "Person",
                        principalColumn: "personId",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "LodgingPerk",
                columns: table => new
                {
                    LodgingId = table.Column<uint>(type: "int unsigned", nullable: false),
                    PerkId = table.Column<uint>(type: "int unsigned", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LodgingPerk", x => new { x.LodgingId, x.PerkId });
                    table.ForeignKey(
                        name: "FK_LodgingPerk_Lodging_LodgingId",
                        column: x => x.LodgingId,
                        principalTable: "Lodging",
                        principalColumn: "lodgingId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LodgingPerk_Perk_PerkId",
                        column: x => x.PerkId,
                        principalTable: "Perk",
                        principalColumn: "perkId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "LodgingPhoneNumber",
                columns: table => new
                {
                    phoneNumber = table.Column<string>(type: "CHAR(30)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LodgingId = table.Column<uint>(type: "int unsigned", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LodgingPhoneNumber", x => new { x.LodgingId, x.phoneNumber });
                    table.ForeignKey(
                        name: "FK_LodgingPhoneNumber_Lodging_LodgingId",
                        column: x => x.LodgingId,
                        principalTable: "Lodging",
                        principalColumn: "lodgingId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "LodgingPhoto",
                columns: table => new
                {
                    FileName = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LodgingId = table.Column<uint>(type: "int unsigned", nullable: false),
                    Ordering = table.Column<byte>(type: "tinyint unsigned", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LodgingPhoto", x => x.FileName);
                    table.ForeignKey(
                        name: "FK_LodgingPhoto_Lodging_LodgingId",
                        column: x => x.LodgingId,
                        principalTable: "Lodging",
                        principalColumn: "lodgingId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "RoomType",
                columns: table => new
                {
                    RoomTypeId = table.Column<uint>(type: "int unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Fees = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    PerNightPrice = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    Capacity = table.Column<uint>(type: "int unsigned", nullable: false),
                    LodgingId = table.Column<uint>(type: "int unsigned", nullable: false),
                    Name = table.Column<string>(type: "varchar(75)", maxLength: 75, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoomType", x => x.RoomTypeId);
                    table.ForeignKey(
                        name: "FK_RoomType_Lodging_LodgingId",
                        column: x => x.LodgingId,
                        principalTable: "Lodging",
                        principalColumn: "lodgingId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Payment",
                columns: table => new
                {
                    paymentId = table.Column<uint>(type: "int unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    DateAndTime = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    InvoiceImageFileName = table.Column<string>(type: "varchar(75)", maxLength: 75, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BookingId = table.Column<uint>(type: "int unsigned", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payment", x => x.paymentId);
                    table.ForeignKey(
                        name: "FK_Payment_Booking_BookingId",
                        column: x => x.BookingId,
                        principalTable: "Booking",
                        principalColumn: "bookingId");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Room",
                columns: table => new
                {
                    LodgingId = table.Column<uint>(type: "int unsigned", nullable: false),
                    roomNumber = table.Column<uint>(type: "int unsigned", nullable: false),
                    roomTypeId = table.Column<uint>(type: "int unsigned", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Room", x => new { x.LodgingId, x.roomNumber });
                    table.ForeignKey(
                        name: "FK_Room_Lodging_LodgingId",
                        column: x => x.LodgingId,
                        principalTable: "Lodging",
                        principalColumn: "lodgingId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Room_RoomType_roomTypeId",
                        column: x => x.roomTypeId,
                        principalTable: "RoomType",
                        principalColumn: "RoomTypeId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "RoomTypePhoto",
                columns: table => new
                {
                    FileName = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RoomTypeId = table.Column<uint>(type: "int unsigned", nullable: false),
                    Ordering = table.Column<byte>(type: "tinyint unsigned", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoomTypePhoto", x => x.FileName);
                    table.ForeignKey(
                        name: "FK_RoomTypePhoto_RoomType_RoomTypeId",
                        column: x => x.RoomTypeId,
                        principalTable: "RoomType",
                        principalColumn: "RoomTypeId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "RoomBooking",
                columns: table => new
                {
                    roomBookingId = table.Column<uint>(type: "int unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Cost = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    Discount = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    Fees = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    Status = table.Column<string>(type: "CHAR(50)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BookingId = table.Column<uint>(type: "int unsigned", nullable: false),
                    LodgingId = table.Column<uint>(type: "int unsigned", nullable: false),
                    RoomNumber = table.Column<uint>(type: "int unsigned", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoomBooking", x => x.roomBookingId);
                    table.ForeignKey(
                        name: "FK_RoomBooking_Booking_BookingId",
                        column: x => x.BookingId,
                        principalTable: "Booking",
                        principalColumn: "bookingId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RoomBooking_Room_LodgingId_RoomNumber",
                        columns: x => new { x.LodgingId, x.RoomNumber },
                        principalTable: "Room",
                        principalColumns: new[] { "LodgingId", "roomNumber" },
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Booking_customerPersonId",
                table: "Booking",
                column: "customerPersonId");

            migrationBuilder.CreateIndex(
                name: "IX_Booking_LodgingId",
                table: "Booking",
                column: "LodgingId");

            migrationBuilder.CreateIndex(
                name: "IX_Lodging_ownerPersonId",
                table: "Lodging",
                column: "ownerPersonId");

            migrationBuilder.CreateIndex(
                name: "IX_LodgingPerk_PerkId",
                table: "LodgingPerk",
                column: "PerkId");

            migrationBuilder.CreateIndex(
                name: "IX_LodgingPhoto_LodgingId",
                table: "LodgingPhoto",
                column: "LodgingId");

            migrationBuilder.CreateIndex(
                name: "IX_Payment_BookingId",
                table: "Payment",
                column: "BookingId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Person_userName",
                table: "Person",
                column: "userName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Room_roomTypeId",
                table: "Room",
                column: "roomTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_RoomBooking_BookingId",
                table: "RoomBooking",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_RoomBooking_LodgingId_RoomNumber",
                table: "RoomBooking",
                columns: new[] { "LodgingId", "RoomNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_RoomType_LodgingId",
                table: "RoomType",
                column: "LodgingId");

            migrationBuilder.CreateIndex(
                name: "IX_RoomTypePhoto_RoomTypeId",
                table: "RoomTypePhoto",
                column: "RoomTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_User_userRoleId",
                table: "User",
                column: "userRoleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LodgingPerk");

            migrationBuilder.DropTable(
                name: "LodgingPhoneNumber");

            migrationBuilder.DropTable(
                name: "LodgingPhoto");

            migrationBuilder.DropTable(
                name: "Payment");

            migrationBuilder.DropTable(
                name: "PersonPhoneNumber");

            migrationBuilder.DropTable(
                name: "PhoneNumber");

            migrationBuilder.DropTable(
                name: "Photo");

            migrationBuilder.DropTable(
                name: "RoomBooking");

            migrationBuilder.DropTable(
                name: "RoomTypePhoto");

            migrationBuilder.DropTable(
                name: "Perk");

            migrationBuilder.DropTable(
                name: "Booking");

            migrationBuilder.DropTable(
                name: "Room");

            migrationBuilder.DropTable(
                name: "RoomType");

            migrationBuilder.DropTable(
                name: "Lodging");

            migrationBuilder.DropTable(
                name: "Person");

            migrationBuilder.DropTable(
                name: "User");

            migrationBuilder.DropTable(
                name: "UserRole");
        }
    }
}
