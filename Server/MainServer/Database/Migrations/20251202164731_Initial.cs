using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Server.MainServer.Database.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CoordinatorInstances",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Ip = table.Column<string>(type: "TEXT", nullable: false),
                    Port = table.Column<int>(type: "INTEGER", nullable: false),
                    CreateTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FirstLaunchTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CurrentLaunchTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    MaxOnlineUsers = table.Column<int>(type: "INTEGER", nullable: false),
                    MaxUsers = table.Column<int>(type: "INTEGER", nullable: false),
                    IsMOTDEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsTurnEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    TurnAddress = table.Column<string>(type: "TEXT", nullable: true),
                    TurnPort = table.Column<ushort>(type: "INTEGER", nullable: true),
                    TurnUsername = table.Column<string>(type: "TEXT", nullable: true),
                    TurnPassword = table.Column<string>(type: "TEXT", nullable: true),
                    IsStunEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    StunAddress = table.Column<string>(type: "TEXT", nullable: true),
                    StunPort = table.Column<ushort>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoordinatorInstances", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OrchestratorInstance",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ServerVersion = table.Column<string>(type: "TEXT", nullable: false),
                    LicenseKey = table.Column<string>(type: "TEXT", nullable: true),
                    Ip = table.Column<string>(type: "TEXT", nullable: false),
                    DnsName = table.Column<string>(type: "TEXT", nullable: true),
                    Port = table.Column<int>(type: "INTEGER", nullable: false),
                    MaxCoordinators = table.Column<int>(type: "INTEGER", nullable: false),
                    MaxOnlineUsers = table.Column<int>(type: "INTEGER", nullable: false),
                    MaxUsers = table.Column<int>(type: "INTEGER", nullable: false),
                    TimeZoneId = table.Column<string>(type: "TEXT", nullable: true),
                    FirstLaunchTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CurrentLaunchTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    OperatingSystem = table.Column<string>(type: "TEXT", nullable: true),
                    CpuName = table.Column<string>(type: "TEXT", nullable: true),
                    DbProvider = table.Column<string>(type: "TEXT", nullable: true),
                    GpuName = table.Column<string>(type: "TEXT", nullable: true),
                    RamSizeMb = table.Column<int>(type: "INTEGER", nullable: false),
                    MaxDbSizeMb = table.Column<int>(type: "INTEGER", nullable: false),
                    RegionName = table.Column<string>(type: "TEXT", nullable: true),
                    RegionCode = table.Column<string>(type: "TEXT", nullable: true),
                    CityName = table.Column<string>(type: "TEXT", nullable: true),
                    Locale = table.Column<string>(type: "TEXT", nullable: true),
                    ActiveSessionsCount = table.Column<int>(type: "INTEGER", nullable: false),
                    ConnectedUsersCount = table.Column<int>(type: "INTEGER", nullable: false),
                    RegisteredUsersCount = table.Column<int>(type: "INTEGER", nullable: false),
                    CpuLoadPercent = table.Column<double>(type: "REAL", nullable: false),
                    MemoryConsumption = table.Column<double>(type: "REAL", nullable: false),
                    IsFirstLaunch = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrchestratorInstance", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserEntities",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", maxLength: 36, nullable: false),
                    UserIdentity = table.Column<string>(type: "TEXT", maxLength: 36, nullable: false),
                    Username = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Ip = table.Column<string>(type: "TEXT", maxLength: 45, nullable: false),
                    CoordinatorInstanceId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    RegionName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    RegionCode = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    DisplayName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Prefix = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    AvatarUrl = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: true),
                    State = table.Column<int>(type: "INTEGER", nullable: false),
                    FirstConnectionTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastConnectionTime = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserEntities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VideoSessionsEntities",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", maxLength: 36, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    CoordinatorInstanceId = table.Column<string>(type: "TEXT", maxLength: 36, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    ConnectedUsersCount = table.Column<int>(type: "INTEGER", nullable: false),
                    Capacity = table.Column<int>(type: "INTEGER", nullable: false),
                    HostId = table.Column<string>(type: "TEXT", maxLength: 36, nullable: true),
                    HostPeerId = table.Column<string>(type: "TEXT", maxLength: 36, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VideoSessionsEntities", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CoordinatorInstances");

            migrationBuilder.DropTable(
                name: "OrchestratorInstance");

            migrationBuilder.DropTable(
                name: "UserEntities");

            migrationBuilder.DropTable(
                name: "VideoSessionsEntities");
        }
    }
}
