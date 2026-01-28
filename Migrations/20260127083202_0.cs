using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Pawlio.Migrations
{
    /// <inheritdoc />
    public partial class _0 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Devices",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Flavor = table.Column<byte>(type: "smallint", nullable: false, defaultValue: (byte)0),
                    CreaterId = table.Column<int>(type: "integer", nullable: true),
                    Updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdaterId = table.Column<int>(type: "integer", nullable: true),
                    PNToken = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    AppName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PackageName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Version = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    BuildNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    BuildSignature = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Devices", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Firms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Flavor = table.Column<byte>(type: "smallint", nullable: false, defaultValue: (byte)0),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreaterId = table.Column<int>(type: "integer", nullable: true),
                    Updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdaterId = table.Column<int>(type: "integer", nullable: true),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Phone = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    Email = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Address = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Lat = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    Lon = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    TimeOut = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ImageId = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    FirmType = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Firms", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Symptoms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Symptoms", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Flavor = table.Column<byte>(type: "smallint", nullable: false, defaultValue: (byte)0),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreaterId = table.Column<int>(type: "integer", nullable: true),
                    Updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdaterId = table.Column<int>(type: "integer", nullable: true),
                    IdentityNumber = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    Nationality = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    BirthDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Phone = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    Email = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    UserTypeId = table.Column<byte>(type: "smallint", nullable: false),
                    JobId = table.Column<byte>(type: "smallint", nullable: false),
                    LoginType = table.Column<byte>(type: "smallint", nullable: false),
                    ExternalUserId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    AppleAuthCode = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    AppleIdToken = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    Password = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    ImageId = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: true),
                    About = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    AboutNew = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    AcceptCitizen = table.Column<bool>(type: "boolean", nullable: false),
                    IsAdmin = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Lat = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    Lon = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    SendSms = table.Column<bool>(type: "boolean", nullable: false),
                    SmsCode = table.Column<int>(type: "integer", nullable: false),
                    SmsEndTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SendEMail = table.Column<bool>(type: "boolean", nullable: false),
                    EMailCode = table.Column<int>(type: "integer", nullable: false),
                    EMailEndTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Token = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    SessionId = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    LastLogin = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Branches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Flavor = table.Column<byte>(type: "smallint", nullable: false, defaultValue: (byte)0),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreaterId = table.Column<int>(type: "integer", nullable: true),
                    Updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdaterId = table.Column<int>(type: "integer", nullable: true),
                    FirmId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    AppointmentStartTimeHour = table.Column<byte>(type: "smallint", nullable: false),
                    AppointmentStartTimeMinute = table.Column<byte>(type: "smallint", nullable: false),
                    AppointmentEndTimeHour = table.Column<byte>(type: "smallint", nullable: false),
                    AppointmentEndTimeMinute = table.Column<byte>(type: "smallint", nullable: false),
                    LunchBreak = table.Column<bool>(type: "boolean", nullable: false),
                    LunchBreakStartHour = table.Column<byte>(type: "smallint", nullable: false),
                    LunchBreakStartMinute = table.Column<byte>(type: "smallint", nullable: false),
                    LunchBreakEndHour = table.Column<byte>(type: "smallint", nullable: false),
                    LunchBreakEndMinute = table.Column<byte>(type: "smallint", nullable: false),
                    AppointmentTime = table.Column<byte>(type: "smallint", nullable: false),
                    AppointmentCount = table.Column<byte>(type: "smallint", nullable: false),
                    AppointmentNotifyTime = table.Column<byte>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Branches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Branches_Firms_FirmId",
                        column: x => x.FirmId,
                        principalTable: "Firms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Definitions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Flavor = table.Column<byte>(type: "smallint", nullable: false, defaultValue: (byte)0),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreaterId = table.Column<int>(type: "integer", nullable: true),
                    Updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdaterId = table.Column<int>(type: "integer", nullable: true),
                    Key = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    NameTr = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    NameEn = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ValueType = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    Value = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    DetailsTr = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    DetailsEn = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ParentId = table.Column<int>(type: "integer", nullable: true),
                    FirmId = table.Column<int>(type: "integer", nullable: false),
                    AddSubDefinitions = table.Column<bool>(type: "boolean", nullable: false),
                    Static = table.Column<bool>(type: "boolean", nullable: false),
                    Icon = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Definitions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Definitions_Definitions_ParentId",
                        column: x => x.ParentId,
                        principalTable: "Definitions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Definitions_Firms_FirmId",
                        column: x => x.FirmId,
                        principalTable: "Firms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Suppliers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Flavor = table.Column<byte>(type: "smallint", nullable: false, defaultValue: (byte)0),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreaterId = table.Column<int>(type: "integer", nullable: true),
                    Updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdaterId = table.Column<int>(type: "integer", nullable: true),
                    TypeId = table.Column<int>(type: "integer", nullable: false),
                    FirmId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IdNumber = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    TaxOffice = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    FirmName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Phone = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    Mobile = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    Email = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    WebSite = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Address = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Lat = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    Lon = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    Notes = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Suppliers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Suppliers_Firms_FirmId",
                        column: x => x.FirmId,
                        principalTable: "Firms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserFirms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Flavor = table.Column<byte>(type: "smallint", nullable: false, defaultValue: (byte)0),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreaterId = table.Column<int>(type: "integer", nullable: true),
                    Updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdaterId = table.Column<int>(type: "integer", nullable: true),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    FirmId = table.Column<int>(type: "integer", nullable: false),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                    IsAdmin = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserFirms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserFirms_Firms_FirmId",
                        column: x => x.FirmId,
                        principalTable: "Firms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserFirms_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Events",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Flavor = table.Column<byte>(type: "smallint", nullable: false, defaultValue: (byte)0),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreaterId = table.Column<int>(type: "integer", nullable: true),
                    Updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdaterId = table.Column<int>(type: "integer", nullable: true),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    FirmId = table.Column<int>(type: "integer", nullable: false),
                    BranchId = table.Column<int>(type: "integer", nullable: true),
                    CustomerId = table.Column<int>(type: "integer", nullable: true),
                    TypeId = table.Column<int>(type: "integer", nullable: true),
                    EventId = table.Column<int>(type: "integer", nullable: false),
                    EventSubId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Events", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Events_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Events_Firms_FirmId",
                        column: x => x.FirmId,
                        principalTable: "Firms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Flavor = table.Column<byte>(type: "smallint", nullable: false, defaultValue: (byte)0),
                    CreaterId = table.Column<int>(type: "integer", nullable: true),
                    Updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdaterId = table.Column<int>(type: "integer", nullable: true),
                    FirmId = table.Column<int>(type: "integer", nullable: false),
                    Personal = table.Column<bool>(type: "boolean", nullable: false),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IdNumber = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    Phone = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    PhoneVerified = table.Column<bool>(type: "boolean", nullable: false),
                    Email = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    EmailVerified = table.Column<bool>(type: "boolean", nullable: false),
                    BirthDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Address = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Notes = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    SendSms = table.Column<bool>(type: "boolean", nullable: false),
                    SendEmail = table.Column<bool>(type: "boolean", nullable: false),
                    GroupId = table.Column<int>(type: "integer", nullable: true),
                    SourceId = table.Column<int>(type: "integer", nullable: true),
                    Job = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ImageId = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: true),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Customers_Definitions_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Definitions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Customers_Definitions_SourceId",
                        column: x => x.SourceId,
                        principalTable: "Definitions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Customers_Firms_FirmId",
                        column: x => x.FirmId,
                        principalTable: "Firms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Flavor = table.Column<byte>(type: "smallint", nullable: false, defaultValue: (byte)0),
                    CreaterId = table.Column<int>(type: "integer", nullable: true),
                    Updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdaterId = table.Column<int>(type: "integer", nullable: true),
                    FirmId = table.Column<int>(type: "integer", nullable: false),
                    CategoryId = table.Column<int>(type: "integer", nullable: true),
                    MarkId = table.Column<int>(type: "integer", nullable: true),
                    UnitId = table.Column<int>(type: "integer", nullable: true),
                    PackContentId = table.Column<int>(type: "integer", nullable: true),
                    ConcentrationId = table.Column<int>(type: "integer", nullable: true),
                    TaxRateId = table.Column<int>(type: "integer", nullable: true),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Nots = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    PackContentAmount = table.Column<decimal>(type: "numeric(7,2)", precision: 7, scale: 2, nullable: false),
                    ConcentrationAmount = table.Column<decimal>(type: "numeric(7,2)", precision: 7, scale: 2, nullable: false),
                    Barcode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SerialNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TarbilSerialNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Buying = table.Column<decimal>(type: "numeric(10,4)", precision: 10, scale: 4, nullable: false),
                    Price = table.Column<decimal>(type: "numeric(10,4)", precision: 10, scale: 4, nullable: false),
                    CriticalAmount = table.Column<decimal>(type: "numeric(7,2)", precision: 7, scale: 2, nullable: false, defaultValue: 0m),
                    CriticalAmountAlert = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    ImageId = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: true),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Products_Definitions_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Definitions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Products_Definitions_ConcentrationId",
                        column: x => x.ConcentrationId,
                        principalTable: "Definitions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Products_Definitions_MarkId",
                        column: x => x.MarkId,
                        principalTable: "Definitions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Products_Definitions_PackContentId",
                        column: x => x.PackContentId,
                        principalTable: "Definitions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Products_Definitions_TaxRateId",
                        column: x => x.TaxRateId,
                        principalTable: "Definitions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Products_Definitions_UnitId",
                        column: x => x.UnitId,
                        principalTable: "Definitions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Products_Firms_FirmId",
                        column: x => x.FirmId,
                        principalTable: "Firms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserFirmsBranches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Flavor = table.Column<byte>(type: "smallint", nullable: false, defaultValue: (byte)0),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreaterId = table.Column<int>(type: "integer", nullable: true),
                    Updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdaterId = table.Column<int>(type: "integer", nullable: true),
                    BranchId = table.Column<int>(type: "integer", nullable: false),
                    UserFirmId = table.Column<int>(type: "integer", nullable: false),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                    PositionId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserFirmsBranches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserFirmsBranches_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserFirmsBranches_UserFirms_UserFirmId",
                        column: x => x.UserFirmId,
                        principalTable: "UserFirms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Animals",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Flavor = table.Column<byte>(type: "smallint", nullable: false, defaultValue: (byte)0),
                    CreaterId = table.Column<int>(type: "integer", nullable: true),
                    Updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdaterId = table.Column<int>(type: "integer", nullable: true),
                    FirmId = table.Column<int>(type: "integer", nullable: false),
                    OwnerId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CategoryId = table.Column<int>(type: "integer", nullable: true),
                    TypeId = table.Column<int>(type: "integer", nullable: true),
                    RaceId = table.Column<int>(type: "integer", nullable: true),
                    IdNumber = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    BirthDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Gender = table.Column<bool>(type: "boolean", nullable: true),
                    MotherIdNumber = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    FatherIdNumber = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ColorId = table.Column<int>(type: "integer", nullable: true),
                    Attacker = table.Column<bool>(type: "boolean", nullable: false),
                    Neutered = table.Column<bool>(type: "boolean", nullable: false),
                    Habit = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DistinctiveFeature = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Notes = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ImageId = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: true),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Animals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Animals_Customers_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Animals_Definitions_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Definitions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Animals_Definitions_ColorId",
                        column: x => x.ColorId,
                        principalTable: "Definitions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Animals_Definitions_RaceId",
                        column: x => x.RaceId,
                        principalTable: "Definitions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Animals_Definitions_TypeId",
                        column: x => x.TypeId,
                        principalTable: "Definitions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Animals_Firms_FirmId",
                        column: x => x.FirmId,
                        principalTable: "Firms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Balances",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Flavor = table.Column<byte>(type: "smallint", nullable: false, defaultValue: (byte)0),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreaterId = table.Column<int>(type: "integer", nullable: true),
                    Updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdaterId = table.Column<int>(type: "integer", nullable: true),
                    FirmId = table.Column<int>(type: "integer", nullable: false),
                    BranchId = table.Column<int>(type: "integer", nullable: false),
                    CustomerId = table.Column<int>(type: "integer", nullable: true),
                    SupplierId = table.Column<int>(type: "integer", nullable: true),
                    Balance = table.Column<decimal>(type: "numeric(10,4)", precision: 10, scale: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Balances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Balances_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Balances_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Balances_Firms_FirmId",
                        column: x => x.FirmId,
                        principalTable: "Firms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Balances_Suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Suppliers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Baskets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Flavor = table.Column<byte>(type: "smallint", nullable: false, defaultValue: (byte)0),
                    CreaterId = table.Column<int>(type: "integer", nullable: true),
                    Updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdaterId = table.Column<int>(type: "integer", nullable: true),
                    FirmId = table.Column<int>(type: "integer", nullable: false),
                    BranchId = table.Column<int>(type: "integer", nullable: false),
                    SupplierId = table.Column<int>(type: "integer", nullable: true),
                    CustomerId = table.Column<int>(type: "integer", nullable: true),
                    TotalAmount = table.Column<decimal>(type: "numeric(10,4)", precision: 10, scale: 4, nullable: false),
                    TotalProfit = table.Column<decimal>(type: "numeric(10,4)", precision: 10, scale: 4, nullable: false),
                    TotalTax = table.Column<decimal>(type: "numeric(10,4)", precision: 10, scale: 4, nullable: false),
                    InvoiceNo = table.Column<string>(type: "text", nullable: false),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Baskets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Baskets_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Baskets_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Baskets_Firms_FirmId",
                        column: x => x.FirmId,
                        principalTable: "Firms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Baskets_Suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Suppliers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ProductAmounts",
                columns: table => new
                {
                    BranchId = table.Column<int>(type: "integer", nullable: false),
                    ProductId = table.Column<int>(type: "integer", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(10,4)", precision: 10, scale: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductAmounts", x => new { x.BranchId, x.ProductId });
                    table.ForeignKey(
                        name: "FK_ProductAmounts_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductAmounts_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductPriceHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProductId = table.Column<int>(type: "integer", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(10,4)", precision: 10, scale: 4, nullable: false),
                    CreaterId = table.Column<int>(type: "integer", nullable: true),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductPriceHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductPriceHistories_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AnimalWeights",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Flavor = table.Column<byte>(type: "smallint", nullable: false, defaultValue: (byte)0),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreaterId = table.Column<int>(type: "integer", nullable: true),
                    Updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdaterId = table.Column<int>(type: "integer", nullable: true),
                    Weight = table.Column<decimal>(type: "numeric(7,2)", precision: 7, scale: 2, nullable: false),
                    AnimalId = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Notes = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnimalWeights", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AnimalWeights_Animals_AnimalId",
                        column: x => x.AnimalId,
                        principalTable: "Animals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Appointments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Flavor = table.Column<byte>(type: "smallint", nullable: false, defaultValue: (byte)0),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreaterId = table.Column<int>(type: "integer", nullable: true),
                    Updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdaterId = table.Column<int>(type: "integer", nullable: true),
                    FirmId = table.Column<int>(type: "integer", nullable: false),
                    BranchId = table.Column<int>(type: "integer", nullable: true),
                    CustomerId = table.Column<int>(type: "integer", nullable: true),
                    SupplierId = table.Column<int>(type: "integer", nullable: true),
                    SubjectId = table.Column<int>(type: "integer", nullable: true),
                    BasketId = table.Column<int>(type: "integer", nullable: true),
                    Notes = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    StartTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AllDay = table.Column<bool>(type: "boolean", nullable: false),
                    Lat = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    Lon = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    JobId = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Appointments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Appointments_Baskets_BasketId",
                        column: x => x.BasketId,
                        principalTable: "Baskets",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Appointments_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Appointments_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Appointments_Definitions_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "Definitions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Appointments_Firms_FirmId",
                        column: x => x.FirmId,
                        principalTable: "Firms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Appointments_Suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Suppliers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Flavor = table.Column<byte>(type: "smallint", nullable: false, defaultValue: (byte)0),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreaterId = table.Column<int>(type: "integer", nullable: true),
                    Updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdaterId = table.Column<int>(type: "integer", nullable: true),
                    FirmId = table.Column<int>(type: "integer", nullable: false),
                    BranchId = table.Column<int>(type: "integer", nullable: false),
                    BasketId = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    SupplierId = table.Column<int>(type: "integer", nullable: true),
                    CustomerId = table.Column<int>(type: "integer", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    PaymentType = table.Column<int>(type: "integer", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(10,4)", precision: 10, scale: 4, nullable: false),
                    Nots = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    PayDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AcquirerId = table.Column<int>(type: "integer", nullable: false),
                    RefNo = table.Column<string>(type: "text", nullable: false),
                    AuthCode = table.Column<string>(type: "text", nullable: false),
                    ResponseCode = table.Column<string>(type: "text", nullable: false),
                    BatchNo = table.Column<int>(type: "integer", nullable: false),
                    StanNo = table.Column<int>(type: "integer", nullable: false),
                    CreditCardNo = table.Column<string>(type: "text", nullable: false),
                    CreditCardName = table.Column<string>(type: "text", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Payments_Baskets_BasketId",
                        column: x => x.BasketId,
                        principalTable: "Baskets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Payments_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Payments_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Payments_Firms_FirmId",
                        column: x => x.FirmId,
                        principalTable: "Firms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Payments_Suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Suppliers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Accountings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Flavor = table.Column<byte>(type: "smallint", nullable: false, defaultValue: (byte)0),
                    CreaterId = table.Column<int>(type: "integer", nullable: true),
                    Updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdaterId = table.Column<int>(type: "integer", nullable: true),
                    FirmId = table.Column<int>(type: "integer", nullable: false),
                    BranchId = table.Column<int>(type: "integer", nullable: false),
                    BasketId = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    SupplierId = table.Column<int>(type: "integer", nullable: true),
                    CustomerId = table.Column<int>(type: "integer", nullable: true),
                    AppointmentId = table.Column<int>(type: "integer", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    EventId = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Detail = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(10,4)", precision: 10, scale: 4, nullable: false),
                    Buying = table.Column<decimal>(type: "numeric(10,4)", precision: 10, scale: 4, nullable: false),
                    Discount = table.Column<decimal>(type: "numeric(10,4)", precision: 10, scale: 4, nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(10,4)", precision: 10, scale: 4, nullable: false),
                    Profit = table.Column<decimal>(type: "numeric(10,4)", precision: 10, scale: 4, nullable: false),
                    Tax = table.Column<decimal>(type: "numeric(10,4)", precision: 10, scale: 4, nullable: false),
                    TaxRate = table.Column<decimal>(type: "numeric(7,2)", precision: 7, scale: 2, nullable: false),
                    DataVersion = table.Column<int>(type: "integer", nullable: false),
                    Data = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accountings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Accountings_Appointments_AppointmentId",
                        column: x => x.AppointmentId,
                        principalTable: "Appointments",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Accountings_Baskets_BasketId",
                        column: x => x.BasketId,
                        principalTable: "Baskets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Accountings_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Accountings_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Accountings_Firms_FirmId",
                        column: x => x.FirmId,
                        principalTable: "Firms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Accountings_Suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Suppliers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AnimalAppointments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Flavor = table.Column<byte>(type: "smallint", nullable: false, defaultValue: (byte)0),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreaterId = table.Column<int>(type: "integer", nullable: true),
                    Updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdaterId = table.Column<int>(type: "integer", nullable: true),
                    AnimalId = table.Column<int>(type: "integer", nullable: false),
                    AppointmentId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnimalAppointments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AnimalAppointments_Animals_AnimalId",
                        column: x => x.AnimalId,
                        principalTable: "Animals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AnimalAppointments_Appointments_AppointmentId",
                        column: x => x.AppointmentId,
                        principalTable: "Appointments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AnimalAccountings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Flavor = table.Column<byte>(type: "smallint", nullable: false, defaultValue: (byte)0),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreaterId = table.Column<int>(type: "integer", nullable: true),
                    Updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdaterId = table.Column<int>(type: "integer", nullable: true),
                    AccountingId = table.Column<int>(type: "integer", nullable: false),
                    AnimalId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnimalAccountings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AnimalAccountings_Accountings_AccountingId",
                        column: x => x.AccountingId,
                        principalTable: "Accountings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AnimalAccountings_Animals_AnimalId",
                        column: x => x.AnimalId,
                        principalTable: "Animals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExaminationSymptoms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Flavor = table.Column<byte>(type: "smallint", nullable: false, defaultValue: (byte)0),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreaterId = table.Column<int>(type: "integer", nullable: true),
                    Updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdaterId = table.Column<int>(type: "integer", nullable: true),
                    AccountingId = table.Column<int>(type: "integer", nullable: false),
                    SymptomId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExaminationSymptoms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExaminationSymptoms_Accountings_AccountingId",
                        column: x => x.AccountingId,
                        principalTable: "Accountings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExaminationSymptoms_Symptoms_SymptomId",
                        column: x => x.SymptomId,
                        principalTable: "Symptoms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Images",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    Flavor = table.Column<byte>(type: "smallint", nullable: false, defaultValue: (byte)0),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreaterId = table.Column<int>(type: "integer", nullable: true),
                    Updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdaterId = table.Column<int>(type: "integer", nullable: true),
                    ImageType = table.Column<int>(type: "integer", nullable: false),
                    FirmId = table.Column<int>(type: "integer", nullable: false),
                    BranchId = table.Column<int>(type: "integer", nullable: true),
                    CustomerId = table.Column<int>(type: "integer", nullable: true),
                    SupplierId = table.Column<int>(type: "integer", nullable: true),
                    ProductId = table.Column<int>(type: "integer", nullable: true),
                    AccountingId = table.Column<int>(type: "integer", nullable: true),
                    ExaminationId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Images", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Images_Accountings_AccountingId",
                        column: x => x.AccountingId,
                        principalTable: "Accountings",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AnimalImages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Flavor = table.Column<byte>(type: "smallint", nullable: false, defaultValue: (byte)0),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreaterId = table.Column<int>(type: "integer", nullable: true),
                    Updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdaterId = table.Column<int>(type: "integer", nullable: true),
                    ImageId = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    AnimalId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnimalImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AnimalImages_Animals_AnimalId",
                        column: x => x.AnimalId,
                        principalTable: "Animals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AnimalImages_Images_ImageId",
                        column: x => x.ImageId,
                        principalTable: "Images",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Accountings_AppointmentId",
                table: "Accountings",
                column: "AppointmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Accountings_BasketId",
                table: "Accountings",
                column: "BasketId");

            migrationBuilder.CreateIndex(
                name: "IX_Accountings_BranchId",
                table: "Accountings",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_Accountings_CustomerId",
                table: "Accountings",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Accountings_FirmId",
                table: "Accountings",
                column: "FirmId");

            migrationBuilder.CreateIndex(
                name: "IX_Accountings_SupplierId",
                table: "Accountings",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_AnimalAccountings_AccountingId",
                table: "AnimalAccountings",
                column: "AccountingId");

            migrationBuilder.CreateIndex(
                name: "IX_AnimalAccountings_AnimalId",
                table: "AnimalAccountings",
                column: "AnimalId");

            migrationBuilder.CreateIndex(
                name: "IX_AnimalAppointments_AnimalId",
                table: "AnimalAppointments",
                column: "AnimalId");

            migrationBuilder.CreateIndex(
                name: "IX_AnimalAppointments_AppointmentId",
                table: "AnimalAppointments",
                column: "AppointmentId");

            migrationBuilder.CreateIndex(
                name: "IX_AnimalImages_AnimalId",
                table: "AnimalImages",
                column: "AnimalId");

            migrationBuilder.CreateIndex(
                name: "IX_AnimalImages_ImageId",
                table: "AnimalImages",
                column: "ImageId");

            migrationBuilder.CreateIndex(
                name: "IX_Animals_CategoryId",
                table: "Animals",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Animals_ColorId",
                table: "Animals",
                column: "ColorId");

            migrationBuilder.CreateIndex(
                name: "IX_Animals_FirmId",
                table: "Animals",
                column: "FirmId");

            migrationBuilder.CreateIndex(
                name: "IX_Animals_OwnerId",
                table: "Animals",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Animals_RaceId",
                table: "Animals",
                column: "RaceId");

            migrationBuilder.CreateIndex(
                name: "IX_Animals_TypeId",
                table: "Animals",
                column: "TypeId");

            migrationBuilder.CreateIndex(
                name: "IX_AnimalWeights_AnimalId",
                table: "AnimalWeights",
                column: "AnimalId");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_BasketId",
                table: "Appointments",
                column: "BasketId");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_BranchId",
                table: "Appointments",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_CustomerId",
                table: "Appointments",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_FirmId",
                table: "Appointments",
                column: "FirmId");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_SubjectId",
                table: "Appointments",
                column: "SubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_SupplierId",
                table: "Appointments",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_Balances_BranchId",
                table: "Balances",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_Balances_CustomerId",
                table: "Balances",
                column: "CustomerId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Balances_FirmId",
                table: "Balances",
                column: "FirmId");

            migrationBuilder.CreateIndex(
                name: "IX_Balances_SupplierId",
                table: "Balances",
                column: "SupplierId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Baskets_BranchId",
                table: "Baskets",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_Baskets_CustomerId",
                table: "Baskets",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Baskets_FirmId",
                table: "Baskets",
                column: "FirmId");

            migrationBuilder.CreateIndex(
                name: "IX_Baskets_SupplierId",
                table: "Baskets",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_Branches_FirmId",
                table: "Branches",
                column: "FirmId");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_FirmId",
                table: "Customers",
                column: "FirmId");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_GroupId",
                table: "Customers",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_SourceId",
                table: "Customers",
                column: "SourceId");

            migrationBuilder.CreateIndex(
                name: "IX_Definitions_FirmId",
                table: "Definitions",
                column: "FirmId");

            migrationBuilder.CreateIndex(
                name: "IX_Definitions_ParentId",
                table: "Definitions",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_Events_BranchId",
                table: "Events",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_Events_FirmId",
                table: "Events",
                column: "FirmId");

            migrationBuilder.CreateIndex(
                name: "IX_ExaminationSymptoms_AccountingId",
                table: "ExaminationSymptoms",
                column: "AccountingId");

            migrationBuilder.CreateIndex(
                name: "IX_ExaminationSymptoms_SymptomId",
                table: "ExaminationSymptoms",
                column: "SymptomId");

            migrationBuilder.CreateIndex(
                name: "IX_Images_AccountingId",
                table: "Images",
                column: "AccountingId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_BasketId",
                table: "Payments",
                column: "BasketId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_BranchId",
                table: "Payments",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_CustomerId",
                table: "Payments",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_FirmId",
                table: "Payments",
                column: "FirmId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_SupplierId",
                table: "Payments",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductAmounts_ProductId",
                table: "ProductAmounts",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductPriceHistories_ProductId",
                table: "ProductPriceHistories",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_CategoryId",
                table: "Products",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_ConcentrationId",
                table: "Products",
                column: "ConcentrationId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_FirmId",
                table: "Products",
                column: "FirmId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_MarkId",
                table: "Products",
                column: "MarkId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_PackContentId",
                table: "Products",
                column: "PackContentId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_TaxRateId",
                table: "Products",
                column: "TaxRateId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_UnitId",
                table: "Products",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_Suppliers_FirmId",
                table: "Suppliers",
                column: "FirmId");

            migrationBuilder.CreateIndex(
                name: "IX_UserFirms_FirmId",
                table: "UserFirms",
                column: "FirmId");

            migrationBuilder.CreateIndex(
                name: "IX_UserFirms_UserId",
                table: "UserFirms",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserFirmsBranches_BranchId",
                table: "UserFirmsBranches",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_UserFirmsBranches_UserFirmId",
                table: "UserFirmsBranches",
                column: "UserFirmId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AnimalAccountings");

            migrationBuilder.DropTable(
                name: "AnimalAppointments");

            migrationBuilder.DropTable(
                name: "AnimalImages");

            migrationBuilder.DropTable(
                name: "AnimalWeights");

            migrationBuilder.DropTable(
                name: "Balances");

            migrationBuilder.DropTable(
                name: "Devices");

            migrationBuilder.DropTable(
                name: "Events");

            migrationBuilder.DropTable(
                name: "ExaminationSymptoms");

            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.DropTable(
                name: "ProductAmounts");

            migrationBuilder.DropTable(
                name: "ProductPriceHistories");

            migrationBuilder.DropTable(
                name: "UserFirmsBranches");

            migrationBuilder.DropTable(
                name: "Images");

            migrationBuilder.DropTable(
                name: "Animals");

            migrationBuilder.DropTable(
                name: "Symptoms");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "UserFirms");

            migrationBuilder.DropTable(
                name: "Accountings");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Appointments");

            migrationBuilder.DropTable(
                name: "Baskets");

            migrationBuilder.DropTable(
                name: "Branches");

            migrationBuilder.DropTable(
                name: "Customers");

            migrationBuilder.DropTable(
                name: "Suppliers");

            migrationBuilder.DropTable(
                name: "Definitions");

            migrationBuilder.DropTable(
                name: "Firms");
        }
    }
}
