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
                    Name = table.Column<string>(maxLength: 100, nullable: true),
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
                        onDelete: ReferentialAction.Restrict);
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
                    IsArchived = table.Column<bool>(nullable: false),
                    RegionCustomName = table.Column<string>(maxLength: 100, nullable: true),
                    DistrictCustomName = table.Column<string>(maxLength: 100, nullable: true),
                    VillageCustomName = table.Column<string>(maxLength: 100, nullable: true),
                    ZoneCustomName = table.Column<string>(maxLength: 100, nullable: true),
                    ContentLanguageId = table.Column<int>(nullable: true)
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
                });

            migrationBuilder.CreateTable(
                name: "HealthRiskLanguageContents",
                schema: "nyss",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
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
                        onDelete: ReferentialAction.Restrict);
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
                    DataManagerUserId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Users_DataManagerUserId",
                        column: x => x.DataManagerUserId,
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
                name: "IX_Users_DataManagerUserId",
                schema: "nyss",
                table: "Users",
                column: "DataManagerUserId");

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
        }
    }
}
