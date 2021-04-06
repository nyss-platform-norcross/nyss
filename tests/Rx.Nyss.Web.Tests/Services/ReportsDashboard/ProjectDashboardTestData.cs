using System;
using System.Collections.Generic;
using System.Linq;
using MockQueryable.NSubstitute;
using NSubstitute;
using RX.Nyss.Common.Utils;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;

namespace RX.Nyss.Web.Tests.Services.ReportsDashboard
{
    public class ReportsDashboardTestData
    {
        private static readonly DateTime BaseDate = new DateTime(2019, 01, 01);
        private readonly IDateTimeProvider _dateTimeProvider;
        public int ProjectId { get; set; } = 1;

        public List<NationalSociety> NationalSocieties { get; set; }
        public List<HealthRisk> HealthRisks { get; set; }
        public List<AlertRule> AlertRules { get; set; }
        public List<Project> Projects { get; set; }
        public List<ProjectHealthRisk> ProjectHealthRisks { get; set; }
        public List<DataCollector> DataCollectors { get; set; }
        public List<Report> Reports { get; set; }
        public List<RawReport> RawReports { get; set; }
        public List<User> Users { get; set; }
        public List<SupervisorUserProject> SupervisorUserProjects { get; set; }
        public List<UserNationalSociety> UserNationalSocieties { get; set; }
        public List<Region> Regions { get; set; }
        public List<District> Districts { get; set; }
        public List<Village> Villages { get; set; }
        public List<Zone> Zones { get; set; }
        public List<Alert> Alerts { get; set; }

        public ReportsDashboardTestData(IDateTimeProvider dateTimeProvider)
        {
            _dateTimeProvider = dateTimeProvider;


            NationalSocieties = new List<NationalSociety> { new NationalSociety { Id = 1 } };

            Projects = new List<Project>
            {
                new Project
                {
                    Id = ProjectId,
                    NationalSocietyId = NationalSocieties[0].Id,
                    NationalSociety = NationalSocieties[0]
                }
            };

            Users = new List<User> { new SupervisorUser { Id = 1 } };

            UserNationalSocieties = new List<UserNationalSociety>
            {
                new UserNationalSociety
                {
                    NationalSociety = NationalSocieties[0],
                    NationalSocietyId = NationalSocieties[0].Id,
                    User = Users[0],
                    UserId = Users[0].Id
                }
            };

            SupervisorUserProjects = new List<SupervisorUserProject>
            {
                new SupervisorUserProject
                {
                    SupervisorUser = (SupervisorUser)Users[0],
                    SupervisorUserId = Users[0].Id,
                    Project = Projects[0],
                    ProjectId = Projects[0].Id
                }
            };

            AlertRules = new List<AlertRule>
            {
                new AlertRule
                {
                    Id = 1,
                    CountThreshold = 1
                },
                new AlertRule
                {
                    Id = 2,
                    CountThreshold = 1
                }
            };

            HealthRisks = new List<HealthRisk>
            {
                new HealthRisk
                {
                    Id = 1,
                    AlertRule = AlertRules[0],
                    HealthRiskType = HealthRiskType.Human
                },
                new HealthRisk
                {
                    Id = 2,
                    AlertRule = AlertRules[1],
                    HealthRiskType = HealthRiskType.Human
                }
            };

            ProjectHealthRisks = new List<ProjectHealthRisk>
            {
                new ProjectHealthRisk
                {
                    Id = 1,
                    AlertRule = AlertRules[0],
                    Project = Projects[0],
                    HealthRisk = HealthRisks[0],
                    HealthRiskId = HealthRisks[0].Id,
                    Reports = new List<Report>()
                },
                new ProjectHealthRisk
                {
                    Id = 2,
                    AlertRule = AlertRules[1],
                    Project = Projects[0],
                    HealthRisk = HealthRisks[1],
                    HealthRiskId = HealthRisks[1].Id,
                    Reports = new List<Report>()
                }
            };

            Alerts = new List<Alert>();

            GenerateGeographicalStructure();
            GenerateDataCollectorsWithReports();
        }

        public INyssContext GetNyssContextMock()
        {
            var nyssContextMock = Substitute.For<INyssContext>();

            var nationalSocietiesDbSet = NationalSocieties.AsQueryable().BuildMockDbSet();
            var healthRisksDbSet = HealthRisks.AsQueryable().BuildMockDbSet();
            var alertRulesDbSet = AlertRules.AsQueryable().BuildMockDbSet();
            var projectsDbSet = Projects.AsQueryable().BuildMockDbSet();
            var projectHealthRisksDbSet = ProjectHealthRisks.AsQueryable().BuildMockDbSet();
            var dataCollectorsDbSet = DataCollectors.AsQueryable().BuildMockDbSet();
            var reportsDbSet = Reports.AsQueryable().BuildMockDbSet();
            var rawReportsDbSet = RawReports.AsQueryable().BuildMockDbSet();
            var usersDbSet = Users.AsQueryable().BuildMockDbSet();
            var supervisorUserProjectsDbSet = SupervisorUserProjects.AsQueryable().BuildMockDbSet();
            var userNationalSocietiesDbSet = UserNationalSocieties.AsQueryable().BuildMockDbSet();
            var regionsDbSet = Regions.AsQueryable().BuildMockDbSet();
            var districtsDbSet = Districts.AsQueryable().BuildMockDbSet();
            var villagesDbSet = Villages.AsQueryable().BuildMockDbSet();
            var zonesDbSet = Zones.AsQueryable().BuildMockDbSet();
            var alertsDbSet = Alerts.AsQueryable().BuildMockDbSet();

            nyssContextMock.NationalSocieties.Returns(nationalSocietiesDbSet);
            nyssContextMock.HealthRisks.Returns(healthRisksDbSet);
            nyssContextMock.AlertRules.Returns(alertRulesDbSet);
            nyssContextMock.Projects.Returns(projectsDbSet);
            nyssContextMock.ProjectHealthRisks.Returns(projectHealthRisksDbSet);
            nyssContextMock.DataCollectors.Returns(dataCollectorsDbSet);
            nyssContextMock.Reports.Returns(reportsDbSet);
            nyssContextMock.RawReports.Returns(rawReportsDbSet);
            nyssContextMock.Users.Returns(usersDbSet);
            nyssContextMock.SupervisorUserProjects.Returns(supervisorUserProjectsDbSet);
            nyssContextMock.UserNationalSocieties.Returns(userNationalSocietiesDbSet);
            nyssContextMock.Regions.Returns(regionsDbSet);
            nyssContextMock.Districts.Returns(districtsDbSet);
            nyssContextMock.Villages.Returns(villagesDbSet);
            nyssContextMock.Zones.Returns(zonesDbSet);
            nyssContextMock.Alerts.Returns(alertsDbSet);

            return nyssContextMock;
        }

        private void GenerateGeographicalStructure()
        {
            Regions = new List<Region>
            {
                new Region
                {
                    Id = 1,
                    Name = "Region 1",
                    NationalSociety = NationalSocieties[0]
                },
                new Region
                {
                    Id = 2,
                    Name = "Region 2",
                    NationalSociety = NationalSocieties[0]
                }
            };

            Districts = new List<District>
            {
                new District
                {
                    Id = 1,
                    Name = "District 1",
                    Region = Regions[0]
                },
                new District
                {
                    Id = 2,
                    Name = "District 2",
                    Region = Regions[0]
                },
                new District
                {
                    Id = 3,
                    Name = "District 3",
                    Region = Regions[1]
                },
                new District
                {
                    Id = 4,
                    Name = "District 4",
                    Region = Regions[1]
                }
            };

            Regions[0].Districts = Districts.GetRange(0, 2);
            Regions[1].Districts = Districts.GetRange(2, 2);

            Villages = new List<Village>
            {
                new Village
                {
                    Id = 1,
                    District = Districts[0],
                    Name = "Village 1"
                },
                new Village
                {
                    Id = 2,
                    District = Districts[0],
                    Name = "Village 2"
                },
                new Village
                {
                    Id = 3,
                    District = Districts[1],
                    Name = "Village 3"
                },
                new Village
                {
                    Id = 4,
                    District = Districts[1],
                    Name = "Village 4"
                },
                new Village
                {
                    Id = 5,
                    District = Districts[2],
                    Name = "Village 5"
                },
                new Village
                {
                    Id = 6,
                    District = Districts[2],
                    Name = "Village 6"
                },
                new Village
                {
                    Id = 7,
                    District = Districts[3],
                    Name = "Village 7"
                },
                new Village
                {
                    Id = 8,
                    District = Districts[3],
                    Name = "Village 8"
                }
            };

            Districts[0].Villages = Villages.GetRange(0, 2);
            Districts[1].Villages = Villages.GetRange(2, 2);
            Districts[2].Villages = Villages.GetRange(4, 2);
            Districts[3].Villages = Villages.GetRange(6, 2);

            Zones = new List<Zone>
            {
                new Zone
                {
                    Id = 1,
                    Village = Villages[0],
                    Name = "Zone 1"
                },
                new Zone
                {
                    Id = 2,
                    Village = Villages[0],
                    Name = "Zone 2"
                },
                new Zone
                {
                    Id = 3,
                    Village = Villages[1],
                    Name = "Zone 3"
                },
                new Zone
                {
                    Id = 4,
                    Village = Villages[1],
                    Name = "Zone 4"
                },
                new Zone
                {
                    Id = 5,
                    Village = Villages[2],
                    Name = "Zone 5"
                },
                new Zone
                {
                    Id = 6,
                    Village = Villages[2],
                    Name = "Zone 6"
                },
                new Zone
                {
                    Id = 7,
                    Village = Villages[3],
                    Name = "Zone 7"
                },
                new Zone
                {
                    Id = 8,
                    Village = Villages[3],
                    Name = "Zone 8"
                },
                new Zone
                {
                    Id = 9,
                    Village = Villages[4],
                    Name = "Zone 9"
                },
                new Zone
                {
                    Id = 10,
                    Village = Villages[4],
                    Name = "Zone 10"
                },
                new Zone
                {
                    Id = 11,
                    Village = Villages[5],
                    Name = "Zone 11"
                },
                new Zone
                {
                    Id = 12,
                    Village = Villages[5],
                    Name = "Zone 12"
                },
                new Zone
                {
                    Id = 13,
                    Village = Villages[6],
                    Name = "Zone 13"
                },
                new Zone
                {
                    Id = 14,
                    Village = Villages[6],
                    Name = "Zone 14"
                },
                new Zone
                {
                    Id = 15,
                    Village = Villages[7],
                    Name = "Zone 15"
                },
                new Zone
                {
                    Id = 16,
                    Village = Villages[7],
                    Name = "Zone 16"
                }
            };

            Villages[0].Zones = Zones.GetRange(0, 2);
            Villages[1].Zones = Zones.GetRange(2, 2);
            Villages[2].Zones = Zones.GetRange(4, 2);
            Villages[3].Zones = Zones.GetRange(6, 2);
            Villages[4].Zones = Zones.GetRange(8, 2);
            Villages[5].Zones = Zones.GetRange(10, 2);
            Villages[6].Zones = Zones.GetRange(12, 2);
            Villages[7].Zones = Zones.GetRange(14, 2);
        }


        private void GenerateDataCollectorsWithReports()
        {
            var numberOfDataCollectors = 17;
            var numberOfHumanDataCollectors = 10;
            var numberOfDataCollectionPoints = 6;
            var humansStartIndex = 0;
            var collectionPointsStartIndex = numberOfHumanDataCollectors;
            var numberOfTrainingHumans = 2;
            var numberOfTrainingCollectionPoints = 2;
            var numberOfReports = (numberOfDataCollectors - 1) * 2;
            var numberOfErrorReports = 2;


            DataCollectors = Enumerable.Range(1, numberOfDataCollectors)
                .Select(i => new DataCollector
                {
                    Id = i,
                    Project = Projects[0],
                    Supervisor = (SupervisorUser)Users[0],
                    DataCollectorType = DataCollectorType.Human,
                    IsInTrainingMode = false,
                    Reports = new List<Report>(),
                    RawReports = new List<RawReport>(),
                    Zone = i == numberOfDataCollectors
                        ? Zones[i - 2]
                        : Zones[i - 1],
                    Village = i == numberOfDataCollectors
                        ? Zones[i - 2].Village
                        : Zones[i - 1].Village
                })
                .ToList();

            DataCollectors.GetRange(collectionPointsStartIndex, numberOfDataCollectionPoints).ForEach(dc => dc.DataCollectorType = DataCollectorType.CollectionPoint);
            DataCollectors.GetRange(humansStartIndex, numberOfTrainingHumans).ForEach(dc => dc.IsInTrainingMode = true);
            DataCollectors.GetRange(collectionPointsStartIndex, numberOfTrainingCollectionPoints).ForEach(dc => dc.IsInTrainingMode = true);

            Reports = Enumerable.Range(1, numberOfReports)
                .Select(i => new Report
                {
                    Id = i,
                    DataCollector = DataCollectors[(i - 1) / 2],
                    Status = ReportStatus.New
                })
                .ToList();

            Reports.ForEach(r =>
            {
                r.IsTraining = r.DataCollector.IsInTrainingMode;
                r.CreatedAt = BaseDate.AddDays(r.Id - 1);
                r.ReceivedAt = r.CreatedAt;
                r.EpiWeek = _dateTimeProvider.GetEpiDate(r.CreatedAt).EpiWeek;
                r.EpiYear = _dateTimeProvider.GetEpiDate(r.CreatedAt).EpiYear;
                r.ProjectHealthRisk = ProjectHealthRisks[(r.Id - 1) % 3 == 0
                    ? 0
                    : 1];
                r.RawReport = new RawReport
                {
                    Id = r.Id,
                    DataCollector = r.DataCollector,
                    NationalSociety = NationalSocieties[0],
                    IsTraining = r.IsTraining,
                    Village = r.DataCollector.Village,
                    Zone = r.DataCollector.Zone,
                    Report = r
                };

                r.DataCollector.Reports.Add(r);
                r.ProjectHealthRisk.Reports.Add(r);
            });

            Reports.Where(r => r.DataCollector.DataCollectorType == DataCollectorType.Human).ToList()
                .ForEach(r =>
                {
                    r.ReportedCaseCount = (r.Id % 5) switch
                    {
                        0 => 1,
                        1 => 1,
                        2 => 1,
                        3 => 1,
                        4 => 1,
                        _ => (r.ReportedCase.CountMalesBelowFive ?? 0) + (r.ReportedCase.CountFemalesAtLeastFive ?? 0) + (r.ReportedCase.CountFemalesBelowFive ?? 0) +
                        (r.ReportedCase.CountMalesAtLeastFive ?? 0) + (r.ReportedCase.CountUnspecifiedSexAndAge ?? 0) + (r.DataCollectionPointCase.DeathCount ?? 0) +
                        (r.DataCollectionPointCase.FromOtherVillagesCount ?? 0) + (r.DataCollectionPointCase.ReferredCount ?? 0)
                    };

                    r.ReportedCase = (r.Id % 5) switch
                    {
                        0 => new ReportCase
                        {
                            CountMalesBelowFive = 1,
                            CountMalesAtLeastFive = 0,
                            CountFemalesBelowFive = 0,
                            CountFemalesAtLeastFive = 0,
                            CountUnspecifiedSexAndAge = 0
                        },
                        1 => new ReportCase
                        {
                            CountMalesAtLeastFive = 1,
                            CountFemalesBelowFive = 0,
                            CountFemalesAtLeastFive = 0,
                            CountMalesBelowFive = 0,
                            CountUnspecifiedSexAndAge = 0
                        },
                        2 => new ReportCase
                        {
                            CountFemalesBelowFive = 1,
                            CountMalesBelowFive = 0,
                            CountMalesAtLeastFive = 0,
                            CountFemalesAtLeastFive = 0,
                            CountUnspecifiedSexAndAge = 0
                        },
                        3 => new ReportCase
                        {
                            CountFemalesAtLeastFive = 1,
                            CountMalesBelowFive = 0,
                            CountFemalesBelowFive = 0,
                            CountMalesAtLeastFive = 0,
                            CountUnspecifiedSexAndAge = 0
                        },
                        4 => new ReportCase
                        {
                            CountFemalesAtLeastFive = 0,
                            CountMalesBelowFive = 0,
                            CountFemalesBelowFive = 0,
                            CountMalesAtLeastFive = 0,
                            CountUnspecifiedSexAndAge = 1
                        },
                        _ => r.ReportedCase
                    };
                });

            Reports.Where(r => r.DataCollector.DataCollectorType == DataCollectorType.CollectionPoint).ToList()
                .ForEach(r =>
                {
                    r.ReportType = ReportType.DataCollectionPoint;
                    r.ReportedCase = (r.Id % 4) switch
                    {
                        0 => new ReportCase
                        {
                            CountMalesBelowFive = 1,
                            CountMalesAtLeastFive = 0,
                            CountFemalesBelowFive = 0,
                            CountFemalesAtLeastFive = 0,
                            CountUnspecifiedSexAndAge = 0
                        },
                        1 => new ReportCase
                        {
                            CountMalesAtLeastFive = 1,
                            CountFemalesBelowFive = 0,
                            CountFemalesAtLeastFive = 0,
                            CountMalesBelowFive = 0,
                            CountUnspecifiedSexAndAge = 0
                        },
                        2 => new ReportCase
                        {
                            CountFemalesBelowFive = 1,
                            CountMalesBelowFive = 0,
                            CountMalesAtLeastFive = 0,
                            CountFemalesAtLeastFive = 0,
                            CountUnspecifiedSexAndAge = 0
                        },
                        3 => new ReportCase
                        {
                            CountFemalesAtLeastFive = 1,
                            CountMalesBelowFive = 0,
                            CountFemalesBelowFive = 0,
                            CountMalesAtLeastFive = 0,
                            CountUnspecifiedSexAndAge = 0
                        },
                        _ => r.ReportedCase
                    };

                    r.DataCollectionPointCase = (r.Id % 3) switch
                    {
                        0 => new DataCollectionPointCase
                        {
                            FromOtherVillagesCount = 1,
                            ReferredCount = 0,
                            DeathCount = 0
                        },
                        1 => new DataCollectionPointCase
                        {
                            ReferredCount = 1,
                            FromOtherVillagesCount = 0,
                            DeathCount = 0
                        },
                        2 => new DataCollectionPointCase
                        {
                            DeathCount = 1,
                            FromOtherVillagesCount = 0,
                            ReferredCount = 0
                        },
                        _ => r.DataCollectionPointCase
                    };

                    r.ReportedCaseCount = (r.Id % 4) switch
                    {
                        0 => 1,
                        1 => 1,
                        2 => 1,
                        3 => 1,
                        _ => (r.ReportedCase.CountMalesBelowFive ?? 0) + (r.ReportedCase.CountFemalesAtLeastFive ?? 0) + (r.ReportedCase.CountFemalesBelowFive ?? 0) +
                        (r.ReportedCase.CountMalesAtLeastFive ?? 0) + (r.ReportedCase.CountUnspecifiedSexAndAge ?? 0) + (r.DataCollectionPointCase.DeathCount ?? 0) +
                        (r.DataCollectionPointCase.FromOtherVillagesCount ?? 0) + (r.DataCollectionPointCase.ReferredCount ?? 0)
                    };
                });

            RawReports = new List<RawReport>();
            Reports.ForEach(r =>
            {
                var rawReport = new RawReport
                {
                    Id = r.Id,
                    DataCollector = r.DataCollector,
                    ReceivedAt = r.ReceivedAt,
                    IsTraining = r.IsTraining,
                    Report = r
                };
                RawReports.Add(rawReport);
            });

            var errorReports = Enumerable.Range(1, numberOfErrorReports)
                .Select(i => new RawReport
                {
                    Id = i + numberOfReports,
                    DataCollector = DataCollectors[DataCollectors.Count - 1],
                    ReceivedAt = BaseDate.AddDays(i - 1),
                    IsTraining = false
                }).ToList();

            RawReports.AddRange(errorReports);

            RawReports.ForEach(r => r.DataCollector.RawReports.Add(r));
        }
    }
}
