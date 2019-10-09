using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

namespace RX.Nyss.Data.Migrations
{
    public partial class InitialMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AlertRules",
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
                        principalTable: "AlertRules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HealthRisks",
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
                        principalTable: "AlertRules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Localizations",
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
                        principalTable: "ApplicationLanguages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LocalizedTemplates",
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
                        principalTable: "ApplicationLanguages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "NationalSocieties",
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
                        principalTable: "ContentLanguages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HealthRiskLanguageContents",
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
                        principalTable: "ContentLanguages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HealthRiskLanguageContents_HealthRisks_HealthRiskId",
                        column: x => x.HealthRiskId,
                        principalTable: "HealthRisks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GatewaySettings",
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
                        principalTable: "NationalSocieties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Projects",
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
                        principalTable: "ContentLanguages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Projects_NationalSocieties_NationalSocietyId",
                        column: x => x.NationalSocietyId,
                        principalTable: "NationalSocieties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Regions",
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
                        principalTable: "NationalSocieties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProjectHealthRisks",
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
                        principalTable: "AlertRules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProjectHealthRisks_HealthRisks_HealthRiskId",
                        column: x => x.HealthRiskId,
                        principalTable: "HealthRisks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProjectHealthRisks_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Districts",
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
                        principalTable: "Regions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Alerts",
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
                        principalTable: "ProjectHealthRisks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Villages",
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
                        principalTable: "Districts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Zones",
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
                        principalTable: "Villages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Users",
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
                    NationalSocietyId = table.Column<int>(nullable: true),
                    Sex = table.Column<string>(maxLength: 20, nullable: true),
                    SupervisorUser_NationalSocietyId = table.Column<int>(nullable: true),
                    VillageId = table.Column<int>(nullable: true),
                    ZoneId = table.Column<int>(nullable: true),
                    DataManagerUserId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_NationalSocieties_NationalSocietyId",
                        column: x => x.NationalSocietyId,
                        principalTable: "NationalSocieties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Users_Users_DataManagerUserId",
                        column: x => x.DataManagerUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Users_NationalSocieties_SupervisorUser_NationalSocietyId",
                        column: x => x.SupervisorUser_NationalSocietyId,
                        principalTable: "NationalSocieties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Users_Villages_VillageId",
                        column: x => x.VillageId,
                        principalTable: "Villages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Users_Zones_ZoneId",
                        column: x => x.ZoneId,
                        principalTable: "Zones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Users_ApplicationLanguages_ApplicationLanguageId",
                        column: x => x.ApplicationLanguageId,
                        principalTable: "ApplicationLanguages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AlertEvents",
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
                        principalTable: "Alerts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AlertEvents_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DataCollectors",
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
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DataCollectors_Users_SupervisorId",
                        column: x => x.SupervisorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DataCollectors_Villages_VillageId",
                        column: x => x.VillageId,
                        principalTable: "Villages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DataCollectors_Zones_ZoneId",
                        column: x => x.ZoneId,
                        principalTable: "Zones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
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
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserNationalSocieties",
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
                        principalTable: "NationalSocieties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserNationalSocieties_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Reports",
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
                        principalTable: "DataCollectors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Reports_ProjectHealthRisks_ProjectHealthRiskId",
                        column: x => x.ProjectHealthRiskId,
                        principalTable: "ProjectHealthRisks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AlertReports",
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
                        principalTable: "Alerts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AlertReports_Reports_ReportId",
                        column: x => x.ReportId,
                        principalTable: "Reports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AlertEvents_AlertId",
                table: "AlertEvents",
                column: "AlertId");

            migrationBuilder.CreateIndex(
                name: "IX_AlertEvents_UserId",
                table: "AlertEvents",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AlertRecipients_AlertRuleId",
                table: "AlertRecipients",
                column: "AlertRuleId");

            migrationBuilder.CreateIndex(
                name: "IX_AlertReports_ReportId",
                table: "AlertReports",
                column: "ReportId");

            migrationBuilder.CreateIndex(
                name: "IX_Alerts_ProjectHealthRiskId",
                table: "Alerts",
                column: "ProjectHealthRiskId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationLanguages_DisplayName",
                table: "ApplicationLanguages",
                column: "DisplayName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContentLanguages_DisplayName",
                table: "ContentLanguages",
                column: "DisplayName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DataCollectors_ProjectId",
                table: "DataCollectors",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_DataCollectors_SupervisorId",
                table: "DataCollectors",
                column: "SupervisorId");

            migrationBuilder.CreateIndex(
                name: "IX_DataCollectors_VillageId",
                table: "DataCollectors",
                column: "VillageId");

            migrationBuilder.CreateIndex(
                name: "IX_DataCollectors_ZoneId",
                table: "DataCollectors",
                column: "ZoneId");

            migrationBuilder.CreateIndex(
                name: "IX_Districts_RegionId",
                table: "Districts",
                column: "RegionId");

            migrationBuilder.CreateIndex(
                name: "IX_GatewaySettings_NationalSocietyId",
                table: "GatewaySettings",
                column: "NationalSocietyId");

            migrationBuilder.CreateIndex(
                name: "IX_HealthRiskLanguageContents_ContentLanguageId",
                table: "HealthRiskLanguageContents",
                column: "ContentLanguageId");

            migrationBuilder.CreateIndex(
                name: "IX_HealthRiskLanguageContents_HealthRiskId",
                table: "HealthRiskLanguageContents",
                column: "HealthRiskId");

            migrationBuilder.CreateIndex(
                name: "IX_HealthRisks_AlertRuleId",
                table: "HealthRisks",
                column: "AlertRuleId");

            migrationBuilder.CreateIndex(
                name: "IX_NationalSocieties_ContentLanguageId",
                table: "NationalSocieties",
                column: "ContentLanguageId");

            migrationBuilder.CreateIndex(
                name: "IX_NationalSocieties_Name",
                table: "NationalSocieties",
                column: "Name",
                unique: true,
                filter: "[Name] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId",
                table: "Notifications",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectHealthRisks_AlertRuleId",
                table: "ProjectHealthRisks",
                column: "AlertRuleId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectHealthRisks_HealthRiskId",
                table: "ProjectHealthRisks",
                column: "HealthRiskId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectHealthRisks_ProjectId",
                table: "ProjectHealthRisks",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_ContentLanguageId",
                table: "Projects",
                column: "ContentLanguageId");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_NationalSocietyId",
                table: "Projects",
                column: "NationalSocietyId");

            migrationBuilder.CreateIndex(
                name: "IX_Regions_NationalSocietyId",
                table: "Regions",
                column: "NationalSocietyId");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_DataCollectorId",
                table: "Reports",
                column: "DataCollectorId");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_ProjectHealthRiskId",
                table: "Reports",
                column: "ProjectHealthRiskId");

            migrationBuilder.CreateIndex(
                name: "IX_UserNationalSocieties_NationalSocietyId",
                table: "UserNationalSocieties",
                column: "NationalSocietyId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_NationalSocietyId",
                table: "Users",
                column: "NationalSocietyId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_DataManagerUserId",
                table: "Users",
                column: "DataManagerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_SupervisorUser_NationalSocietyId",
                table: "Users",
                column: "SupervisorUser_NationalSocietyId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_VillageId",
                table: "Users",
                column: "VillageId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_ZoneId",
                table: "Users",
                column: "ZoneId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_ApplicationLanguageId",
                table: "Users",
                column: "ApplicationLanguageId");

            migrationBuilder.CreateIndex(
                name: "IX_Villages_DistrictId",
                table: "Villages",
                column: "DistrictId");

            migrationBuilder.CreateIndex(
                name: "IX_Zones_VillageId",
                table: "Zones",
                column: "VillageId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AlertEvents");

            migrationBuilder.DropTable(
                name: "AlertRecipients");

            migrationBuilder.DropTable(
                name: "AlertReports");

            migrationBuilder.DropTable(
                name: "GatewaySettings");

            migrationBuilder.DropTable(
                name: "HealthRiskLanguageContents");

            migrationBuilder.DropTable(
                name: "Localizations");

            migrationBuilder.DropTable(
                name: "LocalizedTemplates");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "UserNationalSocieties");

            migrationBuilder.DropTable(
                name: "Alerts");

            migrationBuilder.DropTable(
                name: "Reports");

            migrationBuilder.DropTable(
                name: "DataCollectors");

            migrationBuilder.DropTable(
                name: "ProjectHealthRisks");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "HealthRisks");

            migrationBuilder.DropTable(
                name: "Projects");

            migrationBuilder.DropTable(
                name: "Zones");

            migrationBuilder.DropTable(
                name: "ApplicationLanguages");

            migrationBuilder.DropTable(
                name: "AlertRules");

            migrationBuilder.DropTable(
                name: "Villages");

            migrationBuilder.DropTable(
                name: "Districts");

            migrationBuilder.DropTable(
                name: "Regions");

            migrationBuilder.DropTable(
                name: "NationalSocieties");

            migrationBuilder.DropTable(
                name: "ContentLanguages");
        }
    }
}
