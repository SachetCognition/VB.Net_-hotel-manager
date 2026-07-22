using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace HotelManager.Infrastructure.Migrations.Postgres
{
    /// <inheritdoc />
    public partial class InitialCreatePostgres : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Beer",
                columns: table => new
                {
                    ID = table.Column<string>(type: "text", nullable: false),
                    BeerName = table.Column<string>(type: "text", nullable: true),
                    Rate = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Beer", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "CurrencySet",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CS_Currency = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CurrencySet", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Dish",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DishName = table.Column<string>(type: "text", nullable: true),
                    Category = table.Column<string>(type: "text", nullable: true),
                    Rate = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dish", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "EmployeeRegistration",
                columns: table => new
                {
                    EmployeeID = table.Column<string>(type: "text", nullable: false),
                    EmployeeName = table.Column<string>(type: "text", nullable: true),
                    Address = table.Column<string>(type: "text", nullable: true),
                    MobileNo = table.Column<string>(type: "text", nullable: true),
                    Email = table.Column<string>(type: "text", nullable: true),
                    Bloodgroup = table.Column<string>(type: "text", nullable: true),
                    Department = table.Column<string>(type: "text", nullable: true),
                    Designation = table.Column<string>(type: "text", nullable: true),
                    DateOfJoining = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Salary = table.Column<int>(type: "integer", nullable: true),
                    BasicWorkingTime = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeRegistration", x => x.EmployeeID);
                });

            migrationBuilder.CreateTable(
                name: "ExtraBed",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Charges = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExtraBed", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Garden",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Charges = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Garden", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Guest",
                columns: table => new
                {
                    GuestID = table.Column<string>(type: "text", nullable: false),
                    GuestName = table.Column<string>(type: "text", nullable: true),
                    Address = table.Column<string>(type: "text", nullable: true),
                    City = table.Column<string>(type: "text", nullable: true),
                    ContactNo = table.Column<string>(type: "text", nullable: true),
                    IDType = table.Column<string>(type: "text", nullable: true),
                    IDNumber = table.Column<string>(type: "text", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Guest", x => x.GuestID);
                });

            migrationBuilder.CreateTable(
                name: "Hall",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Charges = table.Column<int>(type: "integer", nullable: true),
                    HallName = table.Column<string>(name: "Hall Name", type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Hall", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "HotelInfo",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    HotelName = table.Column<string>(type: "text", nullable: true),
                    Address = table.Column<string>(type: "text", nullable: true),
                    ContactNo = table.Column<string>(type: "text", nullable: true),
                    ContactNo1 = table.Column<string>(type: "text", nullable: true),
                    Email = table.Column<string>(type: "text", nullable: true),
                    TIN = table.Column<string>(type: "text", nullable: true),
                    STNo = table.Column<string>(type: "text", nullable: true),
                    Logo = table.Column<byte[]>(type: "bytea", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HotelInfo", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Liquor_master",
                columns: table => new
                {
                    LiquorName = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Liquor_master", x => x.LiquorName);
                });

            migrationBuilder.CreateTable(
                name: "PurchasedInventory",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProductName = table.Column<string>(type: "text", nullable: true),
                    Category = table.Column<string>(type: "text", nullable: true),
                    TransactionType = table.Column<string>(type: "text", nullable: true),
                    PartyName = table.Column<string>(type: "text", nullable: true),
                    PurchaseDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Quantity = table.Column<double>(type: "double precision", nullable: true),
                    Unit = table.Column<string>(type: "text", nullable: true),
                    Price = table.Column<int>(type: "integer", nullable: true),
                    TotalPrice = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchasedInventory", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Registration",
                columns: table => new
                {
                    UserName = table.Column<string>(type: "text", nullable: false),
                    UserType = table.Column<string>(type: "text", nullable: true),
                    User_Password = table.Column<string>(type: "text", nullable: true),
                    NameOfuser = table.Column<string>(type: "text", nullable: true),
                    ContactNo = table.Column<string>(type: "text", nullable: true),
                    Email = table.Column<string>(type: "text", nullable: true),
                    JoiningDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Registration", x => x.UserName);
                });

            migrationBuilder.CreateTable(
                name: "Room",
                columns: table => new
                {
                    RoomNo = table.Column<string>(type: "text", nullable: false),
                    RoomType = table.Column<string>(type: "text", nullable: true),
                    RoomCharges = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Room", x => x.RoomNo);
                });

            migrationBuilder.CreateTable(
                name: "Taxinfo",
                columns: table => new
                {
                    Salray = table.Column<string>(type: "text", nullable: false),
                    taxinP = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Taxinfo", x => x.Salray);
                });

            migrationBuilder.CreateTable(
                name: "Trans",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Employee_Party_Name = table.Column<string>(type: "text", nullable: true),
                    TransactionType = table.Column<string>(type: "text", nullable: true),
                    TransactionDetails = table.Column<string>(type: "text", nullable: true),
                    TransactionMonth = table.Column<string>(type: "text", nullable: true),
                    TransactionDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    TransactionAmount = table.Column<int>(type: "integer", nullable: true),
                    AmountReceived = table.Column<int>(type: "integer", nullable: true),
                    DueAmount = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Trans", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Stock_Beer",
                columns: table => new
                {
                    StockID = table.Column<string>(type: "text", nullable: false),
                    BeerID = table.Column<string>(type: "text", nullable: true),
                    NoOfBottles = table.Column<int>(type: "integer", nullable: true),
                    StockDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stock_Beer", x => x.StockID);
                    table.ForeignKey(
                        name: "FK_Stock_Beer_Beer_BeerID",
                        column: x => x.BeerID,
                        principalTable: "Beer",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "AdvanceEntry",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EmployeeID = table.Column<string>(type: "text", nullable: true),
                    Amount = table.Column<int>(type: "integer", nullable: true),
                    Deduction = table.Column<int>(type: "integer", nullable: true),
                    WorkingDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdvanceEntry", x => x.ID);
                    table.ForeignKey(
                        name: "FK_AdvanceEntry_EmployeeRegistration_EmployeeID",
                        column: x => x.EmployeeID,
                        principalTable: "EmployeeRegistration",
                        principalColumn: "EmployeeID");
                });

            migrationBuilder.CreateTable(
                name: "EmployeeAttendance",
                columns: table => new
                {
                    AttendanceID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    WorkingDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    EmployeeID = table.Column<string>(type: "text", nullable: true),
                    BasicWorkingTime = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: true),
                    InTime = table.Column<string>(type: "text", nullable: true),
                    OutTime = table.Column<string>(type: "text", nullable: true),
                    Overtime = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeAttendance", x => x.AttendanceID);
                    table.ForeignKey(
                        name: "FK_EmployeeAttendance_EmployeeRegistration_EmployeeID",
                        column: x => x.EmployeeID,
                        principalTable: "EmployeeRegistration",
                        principalColumn: "EmployeeID");
                });

            migrationBuilder.CreateTable(
                name: "EmployeePayment",
                columns: table => new
                {
                    PaymentID = table.Column<string>(type: "text", nullable: false),
                    DateFrom = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    DateTo = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    EmployeeID = table.Column<string>(type: "text", nullable: true),
                    PresentDays = table.Column<int>(type: "integer", nullable: true),
                    Salary = table.Column<int>(type: "integer", nullable: true),
                    Advance = table.Column<int>(type: "integer", nullable: true),
                    Deduction = table.Column<int>(type: "integer", nullable: true),
                    Overtime = table.Column<string>(type: "text", nullable: true),
                    OvertimeRate = table.Column<int>(type: "integer", nullable: true),
                    OverTimeAmount = table.Column<int>(type: "integer", nullable: true),
                    PaymentDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ModeOfPayment = table.Column<string>(type: "text", nullable: true),
                    PaymentModeDetails = table.Column<string>(type: "text", nullable: true),
                    NetPay = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeePayment", x => x.PaymentID);
                    table.ForeignKey(
                        name: "FK_EmployeePayment_EmployeeRegistration_EmployeeID",
                        column: x => x.EmployeeID,
                        principalTable: "EmployeeRegistration",
                        principalColumn: "EmployeeID");
                });

            migrationBuilder.CreateTable(
                name: "Reservation_HallandGarden",
                columns: table => new
                {
                    ID = table.Column<string>(type: "text", nullable: false),
                    GuestID = table.Column<string>(type: "text", nullable: true),
                    CurrencyID = table.Column<int>(type: "integer", nullable: true),
                    HotelID = table.Column<int>(type: "integer", nullable: true),
                    Hall = table.Column<string>(type: "text", nullable: true),
                    DateFrom_Hall = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    DateTo_Hall = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Days_Hall = table.Column<double>(type: "double precision", nullable: true),
                    Rate_Hall = table.Column<double>(type: "double precision", nullable: true),
                    TotalCharges_Hall = table.Column<double>(type: "double precision", nullable: true),
                    Garden = table.Column<string>(type: "text", nullable: true),
                    DateFrom_Garden = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    DateTo_Garden = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Days_Garden = table.Column<int>(type: "integer", nullable: true),
                    Rate_Garden = table.Column<double>(type: "double precision", nullable: true),
                    TotalCharges_Garden = table.Column<double>(type: "double precision", nullable: true),
                    OtherCharges = table.Column<double>(type: "double precision", nullable: true),
                    SubTotal = table.Column<double>(type: "double precision", nullable: true),
                    ServiceTaxPer = table.Column<double>(type: "double precision", nullable: true),
                    ServiceTaxAmount = table.Column<double>(type: "double precision", nullable: true),
                    LuxuryTaxPer = table.Column<double>(type: "double precision", nullable: true),
                    LuxuryTaxAmount = table.Column<double>(type: "double precision", nullable: true),
                    DiscountPer = table.Column<double>(type: "double precision", nullable: true),
                    Discount = table.Column<double>(type: "double precision", nullable: true),
                    GrandTotal = table.Column<double>(type: "double precision", nullable: true),
                    TotalPaid = table.Column<double>(type: "double precision", nullable: true),
                    Balance = table.Column<double>(type: "double precision", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reservation_HallandGarden", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Reservation_HallandGarden_CurrencySet_CurrencyID",
                        column: x => x.CurrencyID,
                        principalTable: "CurrencySet",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_Reservation_HallandGarden_Guest_GuestID",
                        column: x => x.GuestID,
                        principalTable: "Guest",
                        principalColumn: "GuestID");
                    table.ForeignKey(
                        name: "FK_Reservation_HallandGarden_HotelInfo_HotelID",
                        column: x => x.HotelID,
                        principalTable: "HotelInfo",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "Reservation_HallorGarden",
                columns: table => new
                {
                    ID = table.Column<string>(type: "text", nullable: false),
                    GuestID = table.Column<string>(type: "text", nullable: true),
                    Type = table.Column<string>(type: "text", nullable: true),
                    DateFrom = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    DateTo = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Days = table.Column<int>(type: "integer", nullable: true),
                    Rate = table.Column<double>(type: "double precision", nullable: true),
                    TotalCharges = table.Column<double>(type: "double precision", nullable: true),
                    OtherCharges = table.Column<double>(type: "double precision", nullable: true),
                    SubTotal = table.Column<double>(type: "double precision", nullable: true),
                    ServiceTaxPer = table.Column<double>(type: "double precision", nullable: true),
                    ServiceTaxAmount = table.Column<double>(type: "double precision", nullable: true),
                    LuxuryTaxPer = table.Column<double>(type: "double precision", nullable: true),
                    LuxuryTaxAmount = table.Column<double>(type: "double precision", nullable: true),
                    DiscountPer = table.Column<double>(type: "double precision", nullable: true),
                    Discount = table.Column<double>(type: "double precision", nullable: true),
                    GrandTotal = table.Column<double>(type: "double precision", nullable: true),
                    TotalPaid = table.Column<double>(type: "double precision", nullable: true),
                    Balance = table.Column<double>(type: "double precision", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    CurrencyID = table.Column<int>(type: "integer", nullable: true),
                    HotelID = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reservation_HallorGarden", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Reservation_HallorGarden_CurrencySet_CurrencyID",
                        column: x => x.CurrencyID,
                        principalTable: "CurrencySet",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_Reservation_HallorGarden_Guest_GuestID",
                        column: x => x.GuestID,
                        principalTable: "Guest",
                        principalColumn: "GuestID");
                    table.ForeignKey(
                        name: "FK_Reservation_HallorGarden_HotelInfo_HotelID",
                        column: x => x.HotelID,
                        principalTable: "HotelInfo",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "Restaurant_OrderInfo",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrderNo = table.Column<string>(type: "text", nullable: true),
                    CurrencyID = table.Column<int>(type: "integer", nullable: true),
                    HotelID = table.Column<int>(type: "integer", nullable: true),
                    OrderDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    SubTotal = table.Column<int>(type: "integer", nullable: true),
                    VATPer = table.Column<double>(type: "double precision", nullable: true),
                    STPer = table.Column<double>(type: "double precision", nullable: true),
                    VATAmount = table.Column<double>(type: "double precision", nullable: true),
                    STAmount = table.Column<double>(type: "double precision", nullable: true),
                    GrandTotal = table.Column<int>(type: "integer", nullable: true),
                    TotalPayment = table.Column<int>(type: "integer", nullable: true),
                    PaymentDue = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Restaurant_OrderInfo", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Restaurant_OrderInfo_CurrencySet_CurrencyID",
                        column: x => x.CurrencyID,
                        principalTable: "CurrencySet",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_Restaurant_OrderInfo_HotelInfo_HotelID",
                        column: x => x.HotelID,
                        principalTable: "HotelInfo",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "Liquor",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LiquorName = table.Column<string>(type: "text", nullable: true),
                    Volume = table.Column<int>(type: "integer", nullable: true),
                    Rate = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Liquor", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Liquor_Liquor_master_LiquorName",
                        column: x => x.LiquorName,
                        principalTable: "Liquor_master",
                        principalColumn: "LiquorName");
                });

            migrationBuilder.CreateTable(
                name: "Stock",
                columns: table => new
                {
                    StockID = table.Column<string>(type: "text", nullable: false),
                    LiquorName = table.Column<string>(type: "text", nullable: true),
                    NoOfBottles = table.Column<int>(type: "integer", nullable: true),
                    Volume = table.Column<int>(type: "integer", nullable: true),
                    TotalVolume = table.Column<int>(type: "integer", nullable: true),
                    StockDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stock", x => x.StockID);
                    table.ForeignKey(
                        name: "FK_Stock_Liquor_master_LiquorName",
                        column: x => x.LiquorName,
                        principalTable: "Liquor_master",
                        principalColumn: "LiquorName");
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Username = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Username);
                    table.ForeignKey(
                        name: "FK_Users_Registration_Username",
                        column: x => x.Username,
                        principalTable: "Registration",
                        principalColumn: "UserName",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CheckIN_Room",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GuestID = table.Column<string>(type: "text", nullable: true),
                    CurrencyID = table.Column<int>(type: "integer", nullable: true),
                    RoomNo = table.Column<string>(type: "text", nullable: true),
                    RoomCharges = table.Column<int>(type: "integer", nullable: true),
                    DateIN = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    DateOUT = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    NoOfAdults = table.Column<int>(type: "integer", nullable: true),
                    NoOfKids = table.Column<int>(type: "integer", nullable: true),
                    NoOfDays = table.Column<int>(type: "integer", nullable: true),
                    ExtraBed = table.Column<string>(type: "text", nullable: true),
                    TotalRoomCharges = table.Column<double>(type: "double precision", nullable: true),
                    OtherCharges = table.Column<double>(type: "double precision", nullable: true),
                    SubTotal = table.Column<double>(type: "double precision", nullable: true),
                    ServiceTaxPer = table.Column<double>(type: "double precision", nullable: true),
                    ServiceTaxAmount = table.Column<double>(type: "double precision", nullable: true),
                    LuxuryTaxPer = table.Column<double>(type: "double precision", nullable: true),
                    LuxuryTaxAmount = table.Column<double>(type: "double precision", nullable: true),
                    DiscountPer = table.Column<double>(type: "double precision", nullable: true),
                    Discount = table.Column<double>(type: "double precision", nullable: true),
                    GrandTotal = table.Column<double>(type: "double precision", nullable: true),
                    TotalPaid = table.Column<double>(type: "double precision", nullable: true),
                    Balance = table.Column<double>(type: "double precision", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CheckIN_Room", x => x.ID);
                    table.ForeignKey(
                        name: "FK_CheckIN_Room_CurrencySet_CurrencyID",
                        column: x => x.CurrencyID,
                        principalTable: "CurrencySet",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_CheckIN_Room_Guest_GuestID",
                        column: x => x.GuestID,
                        principalTable: "Guest",
                        principalColumn: "GuestID");
                    table.ForeignKey(
                        name: "FK_CheckIN_Room_Room_RoomNo",
                        column: x => x.RoomNo,
                        principalTable: "Room",
                        principalColumn: "RoomNo");
                });

            migrationBuilder.CreateTable(
                name: "Reservation",
                columns: table => new
                {
                    ReservationID = table.Column<string>(type: "text", nullable: false),
                    GuestID = table.Column<string>(type: "text", nullable: true),
                    RoomNo = table.Column<string>(type: "text", nullable: true),
                    DateIN = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    DateOUT = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reservation", x => x.ReservationID);
                    table.ForeignKey(
                        name: "FK_Reservation_Guest_GuestID",
                        column: x => x.GuestID,
                        principalTable: "Guest",
                        principalColumn: "GuestID");
                    table.ForeignKey(
                        name: "FK_Reservation_Room_RoomNo",
                        column: x => x.RoomNo,
                        principalTable: "Room",
                        principalColumn: "RoomNo");
                });

            migrationBuilder.CreateTable(
                name: "Temp_Reservation",
                columns: table => new
                {
                    ReservationID = table.Column<string>(type: "text", nullable: false),
                    GuestID = table.Column<string>(type: "text", nullable: true),
                    RoomNo = table.Column<string>(type: "text", nullable: true),
                    DateIN = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    DateOUT = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Temp_Reservation", x => x.ReservationID);
                    table.ForeignKey(
                        name: "FK_Temp_Reservation_Guest_GuestID",
                        column: x => x.GuestID,
                        principalTable: "Guest",
                        principalColumn: "GuestID");
                    table.ForeignKey(
                        name: "FK_Temp_Reservation_Room_RoomNo",
                        column: x => x.RoomNo,
                        principalTable: "Room",
                        principalColumn: "RoomNo");
                });

            migrationBuilder.CreateTable(
                name: "Tax_ReservationHallandGarden",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ReservationID = table.Column<string>(type: "text", nullable: true),
                    HEduTax = table.Column<double>(type: "double precision", nullable: true),
                    HEduTaxAmount = table.Column<double>(type: "double precision", nullable: true),
                    EducationalTax = table.Column<double>(type: "double precision", nullable: true),
                    EducationalTaxAmount = table.Column<double>(type: "double precision", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tax_ReservationHallandGarden", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Tax_ReservationHallandGarden_Reservation_HallandGarden_Rese~",
                        column: x => x.ReservationID,
                        principalTable: "Reservation_HallandGarden",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "Tax_ReservationHallorGarden",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ReservationID = table.Column<string>(type: "text", nullable: true),
                    HEduTax = table.Column<double>(type: "double precision", nullable: true),
                    HEduTaxAmount = table.Column<double>(type: "double precision", nullable: true),
                    EducationalTax = table.Column<double>(type: "double precision", nullable: true),
                    EducationalTaxAmount = table.Column<double>(type: "double precision", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tax_ReservationHallorGarden", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Tax_ReservationHallorGarden_Reservation_HallorGarden_Reserv~",
                        column: x => x.ReservationID,
                        principalTable: "Reservation_HallorGarden",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "RestaurantOrderedProduct",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrderID = table.Column<int>(type: "integer", nullable: true),
                    ProductID = table.Column<string>(type: "text", nullable: true),
                    ProductName = table.Column<string>(type: "text", nullable: true),
                    Rate = table.Column<int>(type: "integer", nullable: true),
                    Quantity = table.Column<int>(type: "integer", nullable: true),
                    Amount = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RestaurantOrderedProduct", x => x.ID);
                    table.ForeignKey(
                        name: "FK_RestaurantOrderedProduct_Restaurant_OrderInfo_OrderID",
                        column: x => x.OrderID,
                        principalTable: "Restaurant_OrderInfo",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "Tax_Restaurantorder",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrderID = table.Column<int>(type: "integer", nullable: true),
                    HEduTax = table.Column<double>(type: "double precision", nullable: true),
                    HEduTaxAmount = table.Column<double>(type: "double precision", nullable: true),
                    EducationalTax = table.Column<double>(type: "double precision", nullable: true),
                    EducationalTaxAmount = table.Column<double>(type: "double precision", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tax_Restaurantorder", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Tax_Restaurantorder_Restaurant_OrderInfo_OrderID",
                        column: x => x.OrderID,
                        principalTable: "Restaurant_OrderInfo",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "Checkout_Room",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BillNo = table.Column<string>(type: "text", nullable: true),
                    CheckInID = table.Column<int>(type: "integer", nullable: true),
                    CurrencyID = table.Column<int>(type: "integer", nullable: true),
                    HotelID = table.Column<int>(type: "integer", nullable: true),
                    CheckOutDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Checkout_Room", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Checkout_Room_CheckIN_Room_CheckInID",
                        column: x => x.CheckInID,
                        principalTable: "CheckIN_Room",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_Checkout_Room_CurrencySet_CurrencyID",
                        column: x => x.CurrencyID,
                        principalTable: "CurrencySet",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_Checkout_Room_HotelInfo_HotelID",
                        column: x => x.HotelID,
                        principalTable: "HotelInfo",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "Order_Info",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CurrencyID = table.Column<int>(type: "integer", nullable: true),
                    HotelID = table.Column<int>(type: "integer", nullable: true),
                    OrderNo = table.Column<string>(type: "text", nullable: true),
                    OrderDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CheckInID = table.Column<int>(type: "integer", nullable: true),
                    SubTotal = table.Column<int>(type: "integer", nullable: true),
                    VATPer = table.Column<double>(type: "double precision", nullable: true),
                    VATAmount = table.Column<double>(type: "double precision", nullable: true),
                    STPer = table.Column<double>(type: "double precision", nullable: true),
                    STAmount = table.Column<double>(type: "double precision", nullable: true),
                    GrandTotal = table.Column<int>(type: "integer", nullable: true),
                    TotalPayment = table.Column<int>(type: "integer", nullable: true),
                    PaymentDue = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Order_Info", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Order_Info_CheckIN_Room_CheckInID",
                        column: x => x.CheckInID,
                        principalTable: "CheckIN_Room",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_Order_Info_CurrencySet_CurrencyID",
                        column: x => x.CurrencyID,
                        principalTable: "CurrencySet",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_Order_Info_HotelInfo_HotelID",
                        column: x => x.HotelID,
                        principalTable: "HotelInfo",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "Tax_Room",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BillID = table.Column<int>(type: "integer", nullable: true),
                    HEduTax = table.Column<double>(type: "double precision", nullable: true),
                    HEduTaxAmount = table.Column<double>(type: "double precision", nullable: true),
                    EducationalTax = table.Column<double>(type: "double precision", nullable: true),
                    EducationalTaxAmount = table.Column<double>(type: "double precision", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tax_Room", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Tax_Room_Checkout_Room_BillID",
                        column: x => x.BillID,
                        principalTable: "Checkout_Room",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "OrderedProduct",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrderID = table.Column<int>(type: "integer", nullable: true),
                    ProductID = table.Column<string>(type: "text", nullable: true),
                    ProductName = table.Column<string>(type: "text", nullable: true),
                    Volume = table.Column<int>(type: "integer", nullable: true),
                    Rate = table.Column<int>(type: "integer", nullable: true),
                    Quantity = table.Column<int>(type: "integer", nullable: true),
                    Amount = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderedProduct", x => x.ID);
                    table.ForeignKey(
                        name: "FK_OrderedProduct_Order_Info_OrderID",
                        column: x => x.OrderID,
                        principalTable: "Order_Info",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "Tax_order",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrderID = table.Column<int>(type: "integer", nullable: true),
                    HEduTax = table.Column<double>(type: "double precision", nullable: true),
                    HEduTaxAmount = table.Column<double>(type: "double precision", nullable: true),
                    EducationalTax = table.Column<double>(type: "double precision", nullable: true),
                    EducationalTaxAmount = table.Column<double>(type: "double precision", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tax_order", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Tax_order_Order_Info_OrderID",
                        column: x => x.OrderID,
                        principalTable: "Order_Info",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AdvanceEntry_EmployeeID",
                table: "AdvanceEntry",
                column: "EmployeeID");

            migrationBuilder.CreateIndex(
                name: "IX_CheckIN_Room_CurrencyID",
                table: "CheckIN_Room",
                column: "CurrencyID");

            migrationBuilder.CreateIndex(
                name: "IX_CheckIN_Room_GuestID",
                table: "CheckIN_Room",
                column: "GuestID");

            migrationBuilder.CreateIndex(
                name: "IX_CheckIN_Room_RoomNo",
                table: "CheckIN_Room",
                column: "RoomNo");

            migrationBuilder.CreateIndex(
                name: "IX_Checkout_Room_CheckInID",
                table: "Checkout_Room",
                column: "CheckInID");

            migrationBuilder.CreateIndex(
                name: "IX_Checkout_Room_CurrencyID",
                table: "Checkout_Room",
                column: "CurrencyID");

            migrationBuilder.CreateIndex(
                name: "IX_Checkout_Room_HotelID",
                table: "Checkout_Room",
                column: "HotelID");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeAttendance_EmployeeID",
                table: "EmployeeAttendance",
                column: "EmployeeID");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeePayment_EmployeeID",
                table: "EmployeePayment",
                column: "EmployeeID");

            migrationBuilder.CreateIndex(
                name: "IX_Liquor_LiquorName",
                table: "Liquor",
                column: "LiquorName");

            migrationBuilder.CreateIndex(
                name: "IX_Order_Info_CheckInID",
                table: "Order_Info",
                column: "CheckInID");

            migrationBuilder.CreateIndex(
                name: "IX_Order_Info_CurrencyID",
                table: "Order_Info",
                column: "CurrencyID");

            migrationBuilder.CreateIndex(
                name: "IX_Order_Info_HotelID",
                table: "Order_Info",
                column: "HotelID");

            migrationBuilder.CreateIndex(
                name: "IX_OrderedProduct_OrderID",
                table: "OrderedProduct",
                column: "OrderID");

            migrationBuilder.CreateIndex(
                name: "IX_Reservation_GuestID",
                table: "Reservation",
                column: "GuestID");

            migrationBuilder.CreateIndex(
                name: "IX_Reservation_RoomNo",
                table: "Reservation",
                column: "RoomNo");

            migrationBuilder.CreateIndex(
                name: "IX_Reservation_HallandGarden_CurrencyID",
                table: "Reservation_HallandGarden",
                column: "CurrencyID");

            migrationBuilder.CreateIndex(
                name: "IX_Reservation_HallandGarden_GuestID",
                table: "Reservation_HallandGarden",
                column: "GuestID");

            migrationBuilder.CreateIndex(
                name: "IX_Reservation_HallandGarden_HotelID",
                table: "Reservation_HallandGarden",
                column: "HotelID");

            migrationBuilder.CreateIndex(
                name: "IX_Reservation_HallorGarden_CurrencyID",
                table: "Reservation_HallorGarden",
                column: "CurrencyID");

            migrationBuilder.CreateIndex(
                name: "IX_Reservation_HallorGarden_GuestID",
                table: "Reservation_HallorGarden",
                column: "GuestID");

            migrationBuilder.CreateIndex(
                name: "IX_Reservation_HallorGarden_HotelID",
                table: "Reservation_HallorGarden",
                column: "HotelID");

            migrationBuilder.CreateIndex(
                name: "IX_Restaurant_OrderInfo_CurrencyID",
                table: "Restaurant_OrderInfo",
                column: "CurrencyID");

            migrationBuilder.CreateIndex(
                name: "IX_Restaurant_OrderInfo_HotelID",
                table: "Restaurant_OrderInfo",
                column: "HotelID");

            migrationBuilder.CreateIndex(
                name: "IX_RestaurantOrderedProduct_OrderID",
                table: "RestaurantOrderedProduct",
                column: "OrderID");

            migrationBuilder.CreateIndex(
                name: "IX_Stock_LiquorName",
                table: "Stock",
                column: "LiquorName");

            migrationBuilder.CreateIndex(
                name: "IX_Stock_Beer_BeerID",
                table: "Stock_Beer",
                column: "BeerID");

            migrationBuilder.CreateIndex(
                name: "IX_Tax_order_OrderID",
                table: "Tax_order",
                column: "OrderID");

            migrationBuilder.CreateIndex(
                name: "IX_Tax_ReservationHallandGarden_ReservationID",
                table: "Tax_ReservationHallandGarden",
                column: "ReservationID");

            migrationBuilder.CreateIndex(
                name: "IX_Tax_ReservationHallorGarden_ReservationID",
                table: "Tax_ReservationHallorGarden",
                column: "ReservationID");

            migrationBuilder.CreateIndex(
                name: "IX_Tax_Restaurantorder_OrderID",
                table: "Tax_Restaurantorder",
                column: "OrderID");

            migrationBuilder.CreateIndex(
                name: "IX_Tax_Room_BillID",
                table: "Tax_Room",
                column: "BillID");

            migrationBuilder.CreateIndex(
                name: "IX_Temp_Reservation_GuestID",
                table: "Temp_Reservation",
                column: "GuestID");

            migrationBuilder.CreateIndex(
                name: "IX_Temp_Reservation_RoomNo",
                table: "Temp_Reservation",
                column: "RoomNo");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdvanceEntry");

            migrationBuilder.DropTable(
                name: "Dish");

            migrationBuilder.DropTable(
                name: "EmployeeAttendance");

            migrationBuilder.DropTable(
                name: "EmployeePayment");

            migrationBuilder.DropTable(
                name: "ExtraBed");

            migrationBuilder.DropTable(
                name: "Garden");

            migrationBuilder.DropTable(
                name: "Hall");

            migrationBuilder.DropTable(
                name: "Liquor");

            migrationBuilder.DropTable(
                name: "OrderedProduct");

            migrationBuilder.DropTable(
                name: "PurchasedInventory");

            migrationBuilder.DropTable(
                name: "Reservation");

            migrationBuilder.DropTable(
                name: "RestaurantOrderedProduct");

            migrationBuilder.DropTable(
                name: "Stock");

            migrationBuilder.DropTable(
                name: "Stock_Beer");

            migrationBuilder.DropTable(
                name: "Tax_order");

            migrationBuilder.DropTable(
                name: "Tax_ReservationHallandGarden");

            migrationBuilder.DropTable(
                name: "Tax_ReservationHallorGarden");

            migrationBuilder.DropTable(
                name: "Tax_Restaurantorder");

            migrationBuilder.DropTable(
                name: "Tax_Room");

            migrationBuilder.DropTable(
                name: "Taxinfo");

            migrationBuilder.DropTable(
                name: "Temp_Reservation");

            migrationBuilder.DropTable(
                name: "Trans");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "EmployeeRegistration");

            migrationBuilder.DropTable(
                name: "Liquor_master");

            migrationBuilder.DropTable(
                name: "Beer");

            migrationBuilder.DropTable(
                name: "Order_Info");

            migrationBuilder.DropTable(
                name: "Reservation_HallandGarden");

            migrationBuilder.DropTable(
                name: "Reservation_HallorGarden");

            migrationBuilder.DropTable(
                name: "Restaurant_OrderInfo");

            migrationBuilder.DropTable(
                name: "Checkout_Room");

            migrationBuilder.DropTable(
                name: "Registration");

            migrationBuilder.DropTable(
                name: "CheckIN_Room");

            migrationBuilder.DropTable(
                name: "HotelInfo");

            migrationBuilder.DropTable(
                name: "CurrencySet");

            migrationBuilder.DropTable(
                name: "Guest");

            migrationBuilder.DropTable(
                name: "Room");
        }
    }
}
