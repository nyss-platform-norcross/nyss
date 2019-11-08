using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

namespace RX.Nyss.Data.Migrations
{
    public partial class InitialMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "nyss");

            migrationBuilder.CreateTable(
                name: "AlertRules",
                schema: "nyss",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CountThreshold = table.Column<int>(nullable: false),
                    HoursThreshold = table.Column<int>(nullable: true),
                    MetersThreshold = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlertRules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ApplicationLanguages",
                schema: "nyss",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DisplayName = table.Column<string>(maxLength: 100, nullable: false),
                    LanguageCode = table.Column<string>(maxLength: 10, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationLanguages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ContentLanguages",
                schema: "nyss",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DisplayName = table.Column<string>(maxLength: 100, nullable: false),
                    LanguageCode = table.Column<string>(maxLength: 10, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContentLanguages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Countries",
                schema: "nyss",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(maxLength: 100, nullable: true),
                    CountryCode = table.Column<string>(maxLength: 10, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Countries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AlertRecipients",
                schema: "nyss",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmailAddress = table.Column<string>(maxLength: 100, nullable: true),
                    AlertRuleId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlertRecipients", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AlertRecipients_AlertRules_AlertRuleId",
                        column: x => x.AlertRuleId,
                        principalSchema: "nyss",
                        principalTable: "AlertRules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HealthRisks",
                schema: "nyss",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HealthRiskType = table.Column<string>(maxLength: 20, nullable: false),
                    HealthRiskCode = table.Column<int>(nullable: false),
                    AlertRuleId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HealthRisks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HealthRisks_AlertRules_AlertRuleId",
                        column: x => x.AlertRuleId,
                        principalSchema: "nyss",
                        principalTable: "AlertRules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Localizations",
                schema: "nyss",
                columns: table => new
                {
                    Key = table.Column<string>(nullable: false),
                    ApplicationLanguageId = table.Column<int>(nullable: false),
                    Value = table.Column<string>(maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Localizations", x => new { x.ApplicationLanguageId, x.Key });
                    table.ForeignKey(
                        name: "FK_Localizations_ApplicationLanguages_ApplicationLanguageId",
                        column: x => x.ApplicationLanguageId,
                        principalSchema: "nyss",
                        principalTable: "ApplicationLanguages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LocalizedTemplates",
                schema: "nyss",
                columns: table => new
                {
                    Key = table.Column<string>(nullable: false),
                    ApplicationLanguageId = table.Column<int>(nullable: false),
                    Value = table.Column<string>(maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocalizedTemplates", x => new { x.ApplicationLanguageId, x.Key });
                    table.ForeignKey(
                        name: "FK_LocalizedTemplates_ApplicationLanguages_ApplicationLanguageId",
                        column: x => x.ApplicationLanguageId,
                        principalSchema: "nyss",
                        principalTable: "ApplicationLanguages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "NationalSocieties",
                schema: "nyss",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(maxLength: 100, nullable: true),
                    StartDate = table.Column<DateTime>(nullable: false),
                    IsArchived = table.Column<bool>(nullable: false),
                    RegionCustomName = table.Column<string>(maxLength: 100, nullable: true),
                    DistrictCustomName = table.Column<string>(maxLength: 100, nullable: true),
                    VillageCustomName = table.Column<string>(maxLength: 100, nullable: true),
                    ZoneCustomName = table.Column<string>(maxLength: 100, nullable: true),
                    ContentLanguageId = table.Column<int>(nullable: true),
                    CountryId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NationalSocieties", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NationalSocieties_ContentLanguages_ContentLanguageId",
                        column: x => x.ContentLanguageId,
                        principalSchema: "nyss",
                        principalTable: "ContentLanguages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NationalSocieties_Countries_CountryId",
                        column: x => x.CountryId,
                        principalSchema: "nyss",
                        principalTable: "Countries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HealthRiskLanguageContents",
                schema: "nyss",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(maxLength: 100, nullable: false),
                    CaseDefinition = table.Column<string>(maxLength: 500, nullable: false),
                    FeedbackMessage = table.Column<string>(maxLength: 160, nullable: false),
                    HealthRiskId = table.Column<int>(nullable: false),
                    ContentLanguageId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HealthRiskLanguageContents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HealthRiskLanguageContents_ContentLanguages_ContentLanguageId",
                        column: x => x.ContentLanguageId,
                        principalSchema: "nyss",
                        principalTable: "ContentLanguages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HealthRiskLanguageContents_HealthRisks_HealthRiskId",
                        column: x => x.HealthRiskId,
                        principalSchema: "nyss",
                        principalTable: "HealthRisks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GatewaySettings",
                schema: "nyss",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(maxLength: 100, nullable: false),
                    ApiKey = table.Column<string>(maxLength: 100, nullable: false),
                    GatewayType = table.Column<string>(maxLength: 100, nullable: false),
                    NationalSocietyId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GatewaySettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GatewaySettings_NationalSocieties_NationalSocietyId",
                        column: x => x.NationalSocietyId,
                        principalSchema: "nyss",
                        principalTable: "NationalSocieties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Projects",
                schema: "nyss",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(maxLength: 200, nullable: true),
                    TimeZone = table.Column<string>(maxLength: 50, nullable: true),
                    State = table.Column<string>(maxLength: 50, nullable: false),
                    ContentLanguageId = table.Column<int>(nullable: false),
                    NationalSocietyId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Projects_ContentLanguages_ContentLanguageId",
                        column: x => x.ContentLanguageId,
                        principalSchema: "nyss",
                        principalTable: "ContentLanguages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Projects_NationalSocieties_NationalSocietyId",
                        column: x => x.NationalSocietyId,
                        principalSchema: "nyss",
                        principalTable: "NationalSocieties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Regions",
                schema: "nyss",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(maxLength: 100, nullable: true),
                    NationalSocietyId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Regions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Regions_NationalSocieties_NationalSocietyId",
                        column: x => x.NationalSocietyId,
                        principalSchema: "nyss",
                        principalTable: "NationalSocieties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProjectHealthRisks",
                schema: "nyss",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FeedbackMessage = table.Column<string>(maxLength: 160, nullable: true),
                    ProjectId = table.Column<int>(nullable: false),
                    HealthRiskId = table.Column<int>(nullable: false),
                    AlertRuleId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectHealthRisks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectHealthRisks_AlertRules_AlertRuleId",
                        column: x => x.AlertRuleId,
                        principalSchema: "nyss",
                        principalTable: "AlertRules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProjectHealthRisks_HealthRisks_HealthRiskId",
                        column: x => x.HealthRiskId,
                        principalSchema: "nyss",
                        principalTable: "HealthRisks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProjectHealthRisks_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalSchema: "nyss",
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Districts",
                schema: "nyss",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(maxLength: 100, nullable: true),
                    RegionId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Districts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Districts_Regions_RegionId",
                        column: x => x.RegionId,
                        principalSchema: "nyss",
                        principalTable: "Regions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Alerts",
                schema: "nyss",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Status = table.Column<string>(nullable: false),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    Comments = table.Column<string>(maxLength: 500, nullable: true),
                    ProjectHealthRiskId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Alerts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Alerts_ProjectHealthRisks_ProjectHealthRiskId",
                        column: x => x.ProjectHealthRiskId,
                        principalSchema: "nyss",
                        principalTable: "ProjectHealthRisks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Villages",
                schema: "nyss",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(maxLength: 100, nullable: true),
                    DistrictId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Villages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Villages_Districts_DistrictId",
                        column: x => x.DistrictId,
                        principalSchema: "nyss",
                        principalTable: "Districts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Zones",
                schema: "nyss",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(maxLength: 100, nullable: true),
                    VillageId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Zones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Zones_Villages_VillageId",
                        column: x => x.VillageId,
                        principalSchema: "nyss",
                        principalTable: "Villages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                schema: "nyss",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdentityUserId = table.Column<string>(nullable: true),
                    Name = table.Column<string>(maxLength: 100, nullable: false),
                    Role = table.Column<string>(maxLength: 50, nullable: false),
                    EmailAddress = table.Column<string>(maxLength: 100, nullable: false),
                    PhoneNumber = table.Column<string>(maxLength: 20, nullable: false),
                    AdditionalPhoneNumber = table.Column<string>(maxLength: 20, nullable: true),
                    Organization = table.Column<string>(maxLength: 100, nullable: true),
                    IsFirstLogin = table.Column<bool>(nullable: false),
                    ApplicationLanguageId = table.Column<int>(nullable: true),
                    IsDataOwner = table.Column<bool>(nullable: true),
                    HasConsented = table.Column<bool>(nullable: true),
                    ConsentedAt = table.Column<DateTime>(nullable: true),
                    Sex = table.Column<string>(maxLength: 20, nullable: true),
                    VillageId = table.Column<int>(nullable: true),
                    ZoneId = table.Column<int>(nullable: true),
                    ManagerUserId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Users_ManagerUserId",
                        column: x => x.ManagerUserId,
                        principalSchema: "nyss",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Users_Villages_VillageId",
                        column: x => x.VillageId,
                        principalSchema: "nyss",
                        principalTable: "Villages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Users_Zones_ZoneId",
                        column: x => x.ZoneId,
                        principalSchema: "nyss",
                        principalTable: "Zones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Users_ApplicationLanguages_ApplicationLanguageId",
                        column: x => x.ApplicationLanguageId,
                        principalSchema: "nyss",
                        principalTable: "ApplicationLanguages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AlertEvents",
                schema: "nyss",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Operation = table.Column<string>(maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    AlertId = table.Column<int>(nullable: false),
                    UserId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlertEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AlertEvents_Alerts_AlertId",
                        column: x => x.AlertId,
                        principalSchema: "nyss",
                        principalTable: "Alerts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AlertEvents_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "nyss",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DataCollectors",
                schema: "nyss",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(maxLength: 100, nullable: false),
                    DataCollectorType = table.Column<string>(maxLength: 20, nullable: false),
                    DisplayName = table.Column<string>(maxLength: 100, nullable: false),
                    PhoneNumber = table.Column<string>(maxLength: 20, nullable: false),
                    AdditionalPhoneNumber = table.Column<string>(maxLength: 20, nullable: true),
                    Location = table.Column<Point>(nullable: false),
                    SupervisorId = table.Column<int>(nullable: false),
                    ProjectId = table.Column<int>(nullable: false),
                    VillageId = table.Column<int>(nullable: false),
                    ZoneId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataCollectors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DataCollectors_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalSchema: "nyss",
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DataCollectors_Users_SupervisorId",
                        column: x => x.SupervisorId,
                        principalSchema: "nyss",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DataCollectors_Villages_VillageId",
                        column: x => x.VillageId,
                        principalSchema: "nyss",
                        principalTable: "Villages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DataCollectors_Zones_ZoneId",
                        column: x => x.ZoneId,
                        principalSchema: "nyss",
                        principalTable: "Zones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                schema: "nyss",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IsOpened = table.Column<bool>(nullable: false),
                    NotificationType = table.Column<string>(maxLength: 20, nullable: false),
                    Content = table.Column<string>(maxLength: 500, nullable: false),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    UserId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notifications_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "nyss",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserNationalSocieties",
                schema: "nyss",
                columns: table => new
                {
                    UserId = table.Column<int>(nullable: false),
                    NationalSocietyId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserNationalSocieties", x => new { x.UserId, x.NationalSocietyId });
                    table.ForeignKey(
                        name: "FK_UserNationalSocieties_NationalSocieties_NationalSocietyId",
                        column: x => x.NationalSocietyId,
                        principalSchema: "nyss",
                        principalTable: "NationalSocieties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserNationalSocieties_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "nyss",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Reports",
                schema: "nyss",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReportType = table.Column<string>(maxLength: 20, nullable: false),
                    RawContent = table.Column<string>(maxLength: 160, nullable: false),
                    ReceivedAt = table.Column<DateTime>(nullable: false),
                    IsValid = table.Column<bool>(nullable: false),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    ModifiedAt = table.Column<DateTime>(nullable: true),
                    ModifiedBy = table.Column<string>(maxLength: 100, nullable: true),
                    Status = table.Column<string>(maxLength: 20, nullable: false),
                    IsTraining = table.Column<bool>(nullable: false),
                    Location = table.Column<Point>(nullable: false),
                    ReportedCase_CountFemalesBelowFive = table.Column<int>(nullable: true),
                    ReportedCase_CountFemalesAtLeastFive = table.Column<int>(nullable: true),
                    ReportedCase_CountMalesBelowFive = table.Column<int>(nullable: true),
                    ReportedCase_CountMalesAtLeastFive = table.Column<int>(nullable: true),
                    KeptCase_CountFemalesBelowFive = table.Column<int>(nullable: true),
                    KeptCase_CountFemalesAtLeastFive = table.Column<int>(nullable: true),
                    KeptCase_CountMalesBelowFive = table.Column<int>(nullable: true),
                    KeptCase_CountMalesAtLeastFive = table.Column<int>(nullable: true),
                    DataCollectionPointCase_ReferredCount = table.Column<int>(nullable: true),
                    DataCollectionPointCase_DeathCount = table.Column<int>(nullable: true),
                    DataCollectionPointCase_FromOtherVillagesCount = table.Column<int>(nullable: true),
                    ProjectHealthRiskId = table.Column<int>(nullable: false),
                    DataCollectorId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reports_DataCollectors_DataCollectorId",
                        column: x => x.DataCollectorId,
                        principalSchema: "nyss",
                        principalTable: "DataCollectors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Reports_ProjectHealthRisks_ProjectHealthRiskId",
                        column: x => x.ProjectHealthRiskId,
                        principalSchema: "nyss",
                        principalTable: "ProjectHealthRisks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AlertReports",
                schema: "nyss",
                columns: table => new
                {
                    AlertId = table.Column<int>(nullable: false),
                    ReportId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlertReports", x => new { x.AlertId, x.ReportId });
                    table.ForeignKey(
                        name: "FK_AlertReports_Alerts_AlertId",
                        column: x => x.AlertId,
                        principalSchema: "nyss",
                        principalTable: "Alerts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AlertReports_Reports_ReportId",
                        column: x => x.ReportId,
                        principalSchema: "nyss",
                        principalTable: "Reports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                schema: "nyss",
                table: "ContentLanguages",
                columns: new[] { "Id", "DisplayName", "LanguageCode" },
                values: new object[,]
                {
                    { 1, "English", "EN" },
                    { 2, "Français", "FR" }
                });

            migrationBuilder.InsertData(
                schema: "nyss",
                table: "Countries",
                columns: new[] { "Id", "CountryCode", "Name" },
                values: new object[,]
                {
                    { 122, "LS", "Lesotho" },
                    { 3, "DZ", "Algeria" },
                    { 4, "AS", "American Samoa" },
                    { 5, "AD", "AndorrA" },
                    { 6, "AO", "Angola" },
                    { 7, "AI", "Anguilla" },
                    { 8, "AQ", "Antarctica" },
                    { 9, "AG", "Antigua and Barbuda" },
                    { 10, "AR", "Argentina" },
                    { 11, "AM", "Armenia" },
                    { 12, "AW", "Aruba" },
                    { 13, "AU", "Australia" },
                    { 14, "AT", "Austria" },
                    { 15, "AZ", "Azerbaijan" },
                    { 16, "BS", "Bahamas" },
                    { 17, "BH", "Bahrain" },
                    { 18, "BD", "Bangladesh" },
                    { 19, "BB", "Barbados" },
                    { 20, "BY", "Belarus" },
                    { 21, "BE", "Belgium" },
                    { 22, "BZ", "Belize" },
                    { 23, "BJ", "Benin" },
                    { 24, "BM", "Bermuda" },
                    { 25, "BT", "Bhutan" },
                    { 26, "BO", "Bolivia" },
                    { 27, "BA", "Bosnia and Herzegovina" },
                    { 28, "BW", "Botswana" },
                    { 2, "AL", "Albania" },
                    { 29, "BV", "Bouvet Island" },
                    { 31, "IO", "British Indian Ocean Territory" },
                    { 59, "DJ", "Djibouti" },
                    { 33, "BG", "Bulgaria" },
                    { 34, "BF", "Burkina Faso" },
                    { 35, "BI", "Burundi" },
                    { 36, "KH", "Cambodia" },
                    { 37, "CM", "Cameroon" },
                    { 38, "CA", "Canada" },
                    { 39, "CV", "Cape Verde" },
                    { 40, "KY", "Cayman Islands" },
                    { 41, "CF", "Central African Republic" },
                    { 42, "TD", "Chad" },
                    { 43, "CL", "Chile" },
                    { 44, "CN", "China" },
                    { 32, "BN", "Brunei Darussalam" },
                    { 45, "CX", "Christmas Island" },
                    { 47, "CO", "Colombia" },
                    { 48, "KM", "Comoros" },
                    { 49, "CG", "Congo" },
                    { 50, "CD", "Congo, The Democratic Republic of the" },
                    { 51, "CK", "Cook Islands" },
                    { 52, "CR", "Costa Rica" },
                    { 53, "CI", "Cote D\"Ivoire" },
                    { 54, "HR", "Croatia" },
                    { 55, "CU", "Cuba" },
                    { 56, "CY", "Cyprus" },
                    { 57, "CZ", "Czech Republic" },
                    { 58, "DK", "Denmark" },
                    { 46, "CC", "Cocos (Keeling) Islands" },
                    { 30, "BR", "Brazil" },
                    { 241, "ZM", "Zambia" },
                    { 60, "DM", "Dominica" },
                    { 62, "EC", "Ecuador" },
                    { 64, "SV", "El Salvador" },
                    { 65, "GQ", "Equatorial Guinea" },
                    { 66, "ER", "Eritrea" },
                    { 67, "EE", "Estonia" },
                    { 68, "ET", "Ethiopia" },
                    { 69, "FK", "Falkland Islands (Malvinas)" },
                    { 70, "FO", "Faroe Islands" },
                    { 71, "FJ", "Fiji" },
                    { 72, "FI", "Finland" },
                    { 73, "FR", "France" },
                    { 74, "GF", "French Guiana" },
                    { 75, "PF", "French Polynesia" },
                    { 63, "EG", "Egypt" },
                    { 76, "TF", "French Southern Territories" },
                    { 78, "GM", "Gambia" },
                    { 79, "GE", "Georgia" },
                    { 80, "DE", "Germany" },
                    { 81, "GH", "Ghana" },
                    { 82, "GI", "Gibraltar" },
                    { 83, "GR", "Greece" },
                    { 84, "GL", "Greenland" },
                    { 85, "GD", "Grenada" },
                    { 86, "GP", "Guadeloupe" },
                    { 87, "GU", "Guam" },
                    { 88, "GT", "Guatemala" },
                    { 89, "GG", "Guernsey" },
                    { 77, "GA", "Gabon" },
                    { 90, "GN", "Guinea" },
                    { 91, "GW", "Guinea-Bissau" },
                    { 92, "GY", "Guyana" },
                    { 94, "HM", "Heard Island and Mcdonald Islands" },
                    { 95, "VA", "Holy See (Vatican City State)" },
                    { 96, "HN", "Honduras" },
                    { 97, "HK", "Hong Kong" },
                    { 98, "HU", "Hungary" },
                    { 99, "IS", "Iceland" },
                    { 100, "IN", "India" },
                    { 101, "ID", "Indonesia" },
                    { 102, "IR", "Iran, Islamic Republic Of" },
                    { 103, "IQ", "Iraq" },
                    { 104, "IE", "Ireland" },
                    { 105, "IM", "Isle of Man" },
                    { 93, "HT", "Haiti" },
                    { 106, "IL", "Israel" },
                    { 108, "JM", "Jamaica" },
                    { 109, "JP", "Japan" },
                    { 110, "JE", "Jersey" },
                    { 111, "JO", "Jordan" },
                    { 112, "KZ", "Kazakhstan" },
                    { 113, "KE", "Kenya" },
                    { 114, "KI", "Kiribati" },
                    { 115, "KP", "Korea, Democratic People\"S Republic of" },
                    { 116, "KR", "Korea, Republic of" },
                    { 117, "KW", "Kuwait" },
                    { 118, "KG", "Kyrgyzstan" },
                    { 119, "LA", "Lao People\"S Democratic Republic" },
                    { 107, "IT", "Italy" },
                    { 120, "LV", "Latvia" },
                    { 121, "LB", "Lebanon" },
                    { 242, "ZW", "Zimbabwe" },
                    { 123, "LR", "Liberia" },
                    { 124, "LY", "Libyan Arab Jamahiriya" },
                    { 125, "LI", "Liechtenstein" },
                    { 126, "LT", "Lithuania" },
                    { 127, "LU", "Luxembourg" },
                    { 128, "MO", "Macao" },
                    { 129, "MK", "Macedonia, The Former Yugoslav Republic of" },
                    { 130, "MG", "Madagascar" },
                    { 131, "MW", "Malawi" },
                    { 132, "MY", "Malaysia" },
                    { 133, "MV", "Maldives" },
                    { 134, "ML", "Mali" },
                    { 135, "MT", "Malta" },
                    { 150, "NA", "Namibia" },
                    { 151, "NR", "Nauru" },
                    { 138, "MR", "Mauritania" },
                    { 139, "MU", "Mauritius" },
                    { 140, "YT", "Mayotte" },
                    { 141, "MX", "Mexico" },
                    { 142, "FM", "Micronesia, Federated States of" },
                    { 143, "MD", "Moldova, Republic of" },
                    { 144, "MC", "Monaco" },
                    { 145, "MN", "Mongolia" },
                    { 146, "MS", "Montserrat" },
                    { 147, "MA", "Morocco" },
                    { 148, "MZ", "Mozambique" },
                    { 149, "MM", "Myanmar" },
                    { 136, "MH", "Marshall Islands" },
                    { 152, "NP", "Nepal" },
                    { 180, "RU", "Russian Federation" },
                    { 167, "PS", "Palestinian Territory, Occupied" },
                    { 1, "AX", "Åland Islands" },
                    { 154, "AN", "Netherlands Antilles" },
                    { 155, "NC", "New Caledonia" },
                    { 156, "NZ", "New Zealand" },
                    { 157, "NI", "Nicaragua" },
                    { 158, "NE", "Niger" },
                    { 159, "NG", "Nigeria" },
                    { 160, "NU", "Niue" },
                    { 161, "NF", "Norfolk Island" },
                    { 162, "MP", "Northern Mariana Islands" },
                    { 163, "NO", "Norway" },
                    { 164, "OM", "Oman" },
                    { 165, "PK", "Pakistan" },
                    { 153, "NL", "Netherlands" },
                    { 166, "PW", "Palau" },
                    { 168, "PA", "Panama" },
                    { 169, "PG", "Papua New Guinea" },
                    { 170, "PY", "Paraguay" },
                    { 171, "PE", "Peru" },
                    { 172, "PH", "Philippines" },
                    { 173, "PN", "Pitcairn" },
                    { 174, "PL", "Poland" },
                    { 175, "PT", "Portugal" },
                    { 176, "PR", "Puerto Rico" },
                    { 177, "QA", "Qatar" },
                    { 178, "RE", "Reunion" },
                    { 179, "RO", "Romania" },
                    { 137, "MQ", "Martinique" },
                    { 61, "DO", "Dominican Republic" },
                    { 181, "RW", "RWANDA" },
                    { 183, "KN", "Saint Kitts and Nevis" },
                    { 185, "PM", "Saint Pierre and Miquelon" },
                    { 186, "VC", "Saint Vincent and the Grenadines" },
                    { 187, "WS", "Samoa" },
                    { 188, "SM", "San Marino" },
                    { 189, "ST", "Sao Tome and Principe" },
                    { 190, "SA", "Saudi Arabia" },
                    { 191, "SN", "Senegal" },
                    { 192, "CS", "Serbia and Montenegro" },
                    { 193, "SC", "Seychelles" },
                    { 194, "SL", "Sierra Leone" },
                    { 195, "SG", "Singapore" },
                    { 196, "SK", "Slovakia" },
                    { 184, "LC", "Saint Lucia" },
                    { 197, "SI", "Slovenia" },
                    { 199, "SO", "Somalia" },
                    { 200, "ZA", "South Africa" },
                    { 201, "GS", "South Georgia and the South Sandwich Islands" },
                    { 202, "ES", "Spain" },
                    { 203, "LK", "Sri Lanka" },
                    { 204, "SD", "Sudan" },
                    { 205, "SR", "Suriname" },
                    { 206, "SJ", "Svalbard and Jan Mayen" },
                    { 207, "SZ", "Swaziland" },
                    { 208, "SE", "Sweden" },
                    { 209, "CH", "Switzerland" },
                    { 210, "SY", "Syrian Arab Republic" },
                    { 198, "SB", "Solomon Islands" },
                    { 211, "TW", "Taiwan, Province of China" },
                    { 212, "TJ", "Tajikistan" },
                    { 213, "TZ", "Tanzania, United Republic of" },
                    { 215, "TL", "Timor-Leste" },
                    { 216, "TG", "Togo" },
                    { 217, "TK", "Tokelau" },
                    { 218, "TO", "Tonga" },
                    { 219, "TT", "Trinidad and Tobago" },
                    { 220, "TN", "Tunisia" },
                    { 221, "TR", "Turkey" },
                    { 222, "TM", "Turkmenistan" },
                    { 223, "TC", "Turks and Caicos Islands" },
                    { 224, "TV", "Tuvalu" },
                    { 225, "UG", "Uganda" },
                    { 226, "UA", "Ukraine" },
                    { 214, "TH", "Thailand" },
                    { 227, "AE", "United Arab Emirates" },
                    { 229, "US", "United States" },
                    { 230, "UM", "United States Minor Outlying Islands" },
                    { 231, "UY", "Uruguay" },
                    { 232, "UZ", "Uzbekistan" },
                    { 233, "VU", "Vanuatu" },
                    { 234, "VE", "Venezuela" },
                    { 235, "VN", "Viet Nam" },
                    { 236, "VG", "Virgin Islands, British" },
                    { 237, "VI", "Virgin Islands, U.S." },
                    { 238, "WF", "Wallis and Futuna" },
                    { 239, "EH", "Western Sahara" },
                    { 240, "YE", "Yemen" },
                    { 228, "GB", "United Kingdom" },
                    { 182, "SH", "Saint Helena" }
                });

            migrationBuilder.InsertData(
                schema: "nyss",
                table: "Users",
                columns: new[] { "Id", "AdditionalPhoneNumber", "ApplicationLanguageId", "EmailAddress", "IdentityUserId", "IsFirstLogin", "Name", "Organization", "PhoneNumber", "Role" },
                values: new object[] { 1, null, null, "admin@domain.com", "9c1071c1-fa69-432a-9cd0-2c4baa703a67", false, "Administrator", null, "", "Administrator" });

            migrationBuilder.CreateIndex(
                name: "IX_AlertEvents_AlertId",
                schema: "nyss",
                table: "AlertEvents",
                column: "AlertId");

            migrationBuilder.CreateIndex(
                name: "IX_AlertEvents_UserId",
                schema: "nyss",
                table: "AlertEvents",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AlertRecipients_AlertRuleId",
                schema: "nyss",
                table: "AlertRecipients",
                column: "AlertRuleId");

            migrationBuilder.CreateIndex(
                name: "IX_AlertReports_ReportId",
                schema: "nyss",
                table: "AlertReports",
                column: "ReportId");

            migrationBuilder.CreateIndex(
                name: "IX_Alerts_ProjectHealthRiskId",
                schema: "nyss",
                table: "Alerts",
                column: "ProjectHealthRiskId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationLanguages_DisplayName",
                schema: "nyss",
                table: "ApplicationLanguages",
                column: "DisplayName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContentLanguages_DisplayName",
                schema: "nyss",
                table: "ContentLanguages",
                column: "DisplayName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DataCollectors_ProjectId",
                schema: "nyss",
                table: "DataCollectors",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_DataCollectors_SupervisorId",
                schema: "nyss",
                table: "DataCollectors",
                column: "SupervisorId");

            migrationBuilder.CreateIndex(
                name: "IX_DataCollectors_VillageId",
                schema: "nyss",
                table: "DataCollectors",
                column: "VillageId");

            migrationBuilder.CreateIndex(
                name: "IX_DataCollectors_ZoneId",
                schema: "nyss",
                table: "DataCollectors",
                column: "ZoneId");

            migrationBuilder.CreateIndex(
                name: "IX_Districts_RegionId",
                schema: "nyss",
                table: "Districts",
                column: "RegionId");

            migrationBuilder.CreateIndex(
                name: "IX_GatewaySettings_NationalSocietyId",
                schema: "nyss",
                table: "GatewaySettings",
                column: "NationalSocietyId");

            migrationBuilder.CreateIndex(
                name: "IX_HealthRiskLanguageContents_ContentLanguageId",
                schema: "nyss",
                table: "HealthRiskLanguageContents",
                column: "ContentLanguageId");

            migrationBuilder.CreateIndex(
                name: "IX_HealthRiskLanguageContents_HealthRiskId",
                schema: "nyss",
                table: "HealthRiskLanguageContents",
                column: "HealthRiskId");

            migrationBuilder.CreateIndex(
                name: "IX_HealthRisks_AlertRuleId",
                schema: "nyss",
                table: "HealthRisks",
                column: "AlertRuleId");

            migrationBuilder.CreateIndex(
                name: "IX_NationalSocieties_ContentLanguageId",
                schema: "nyss",
                table: "NationalSocieties",
                column: "ContentLanguageId");

            migrationBuilder.CreateIndex(
                name: "IX_NationalSocieties_CountryId",
                schema: "nyss",
                table: "NationalSocieties",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_NationalSocieties_Name",
                schema: "nyss",
                table: "NationalSocieties",
                column: "Name",
                unique: true,
                filter: "[Name] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId",
                schema: "nyss",
                table: "Notifications",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectHealthRisks_AlertRuleId",
                schema: "nyss",
                table: "ProjectHealthRisks",
                column: "AlertRuleId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectHealthRisks_HealthRiskId",
                schema: "nyss",
                table: "ProjectHealthRisks",
                column: "HealthRiskId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectHealthRisks_ProjectId",
                schema: "nyss",
                table: "ProjectHealthRisks",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_ContentLanguageId",
                schema: "nyss",
                table: "Projects",
                column: "ContentLanguageId");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_NationalSocietyId",
                schema: "nyss",
                table: "Projects",
                column: "NationalSocietyId");

            migrationBuilder.CreateIndex(
                name: "IX_Regions_NationalSocietyId",
                schema: "nyss",
                table: "Regions",
                column: "NationalSocietyId");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_DataCollectorId",
                schema: "nyss",
                table: "Reports",
                column: "DataCollectorId");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_ProjectHealthRiskId",
                schema: "nyss",
                table: "Reports",
                column: "ProjectHealthRiskId");

            migrationBuilder.CreateIndex(
                name: "IX_UserNationalSocieties_NationalSocietyId",
                schema: "nyss",
                table: "UserNationalSocieties",
                column: "NationalSocietyId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_ManagerUserId",
                schema: "nyss",
                table: "Users",
                column: "ManagerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_VillageId",
                schema: "nyss",
                table: "Users",
                column: "VillageId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_ZoneId",
                schema: "nyss",
                table: "Users",
                column: "ZoneId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_ApplicationLanguageId",
                schema: "nyss",
                table: "Users",
                column: "ApplicationLanguageId");

            migrationBuilder.CreateIndex(
                name: "IX_Villages_DistrictId",
                schema: "nyss",
                table: "Villages",
                column: "DistrictId");

            migrationBuilder.CreateIndex(
                name: "IX_Zones_VillageId",
                schema: "nyss",
                table: "Zones",
                column: "VillageId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AlertEvents",
                schema: "nyss");

            migrationBuilder.DropTable(
                name: "AlertRecipients",
                schema: "nyss");

            migrationBuilder.DropTable(
                name: "AlertReports",
                schema: "nyss");

            migrationBuilder.DropTable(
                name: "GatewaySettings",
                schema: "nyss");

            migrationBuilder.DropTable(
                name: "HealthRiskLanguageContents",
                schema: "nyss");

            migrationBuilder.DropTable(
                name: "Localizations",
                schema: "nyss");

            migrationBuilder.DropTable(
                name: "LocalizedTemplates",
                schema: "nyss");

            migrationBuilder.DropTable(
                name: "Notifications",
                schema: "nyss");

            migrationBuilder.DropTable(
                name: "UserNationalSocieties",
                schema: "nyss");

            migrationBuilder.DropTable(
                name: "Alerts",
                schema: "nyss");

            migrationBuilder.DropTable(
                name: "Reports",
                schema: "nyss");

            migrationBuilder.DropTable(
                name: "DataCollectors",
                schema: "nyss");

            migrationBuilder.DropTable(
                name: "ProjectHealthRisks",
                schema: "nyss");

            migrationBuilder.DropTable(
                name: "Users",
                schema: "nyss");

            migrationBuilder.DropTable(
                name: "HealthRisks",
                schema: "nyss");

            migrationBuilder.DropTable(
                name: "Projects",
                schema: "nyss");

            migrationBuilder.DropTable(
                name: "Zones",
                schema: "nyss");

            migrationBuilder.DropTable(
                name: "ApplicationLanguages",
                schema: "nyss");

            migrationBuilder.DropTable(
                name: "AlertRules",
                schema: "nyss");

            migrationBuilder.DropTable(
                name: "Villages",
                schema: "nyss");

            migrationBuilder.DropTable(
                name: "Districts",
                schema: "nyss");

            migrationBuilder.DropTable(
                name: "Regions",
                schema: "nyss");

            migrationBuilder.DropTable(
                name: "NationalSocieties",
                schema: "nyss");

            migrationBuilder.DropTable(
                name: "ContentLanguages",
                schema: "nyss");

            migrationBuilder.DropTable(
                name: "Countries",
                schema: "nyss");
        }
    }
}
