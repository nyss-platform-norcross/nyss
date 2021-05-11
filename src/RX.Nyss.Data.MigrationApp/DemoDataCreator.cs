using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Data;

namespace RX.Nyss.Data.MigrationApp
{
    public class DemoDataCreator
    {
        public static void CreateDemoData(string dbConnectionString, string password = "P@ssw0rd")
        {
            var usersToCreate = new (string roleName, string id, string name, string phone, int organizationId)[]
            {
                ("DataConsumer", "6be5c384-6675-a7be-4c45-3cba40f7cd13", "Jane DataConsumer", "+1234561", 1),
                ("GlobalCoordinator", "5f259e96-4200-0cbc-4fe6-90024dcb770a", "Kim GlobalCoordinator", "+1234562", 1),
                ("Coordinator", "4f8cb17f-5851-4156-9997-e85324717129", "Corey Coordinator", "+123456278", 1),
                ("Supervisor", "a076dd25-30d5-cbb2-4174-f73fda1524fa", "Bob Supervisor", "+1234563", 1),
                ("Supervisor", "93485951-13fd-42e1-a98c-e5b54ce60073", "Simon Supervisor", "+1234566", 1),
                ("TechnicalAdvisor", "7c2d7baf-4400-6baf-497a-997a7ca18597", "Todd TechnicalAdvisor", "+1234564", 1),
                ("Manager", "9cfad185-f353-7c8f-4cf7-b2d3498eb715", "Mary Manager", "+1234565", 1),
                ("Manager", "00043634-6ffd-48f3-98a5-0067b6500166", "Martin Manager", "+12345653", 2),
                ("Supervisor", "899CE4F4-F712-4C84-B5A0-4812E03A6312", "Siri Supervisor", "+1234536", 2),
                ("Supervisor", "2005BA30-DEB1-44C5-8575-03C629CDBF51", "Kenny Supervisor", "+12345667", 2),
                ("DataConsumer", "B7218E3A-4F5B-4164-A21A-3D2B9FA2A0D4", "Adam DataConsumer", "+1234561", 3),
                ("Coordinator", "5B36CF2F-E119-40A0-BCF1-9B57D95597C7", "Charlotte Coordinator", "+123456278", 3),
                ("Supervisor", "9828F324-0513-4DC1-A8F6-8AC76D24B211", "Serge Supervisor", "+1234563", 3),
                ("Supervisor", "51CF49DD-235B-4B43-9C1E-CF781310ABB7", "Sophie Supervisor", "+1234566", 3),
                ("TechnicalAdvisor", "CA06B9BB-A93D-4AB7-A7D4-AC77955C947A", "Thierry TechnicalAdvisor", "+1234564", 3),
                ("Manager", "BF23D073-A543-4C26-B83C-3BFF7227E65C", "Marcelle Manager", "+1234565", 3),
                ("Manager", "6608C52B-9677-43CD-A4BB-CDECF1B05DDE", "Marc Manager", "+12345653", 4),
                ("Supervisor", "E2E5B1DB-C31A-41CB-87A2-FE4BD892A2D9", "Sylvain Supervisor", "+1234536", 4),
                ("Supervisor", "9FEA556F-8965-4588-9009-BF70700D8FA3", "Stéphanie Supervisor", "+12345667", 4),
                ("DataConsumer", "D4657D86-6C86-46F9-ABF0-01209678D308", "Andrés DataConsumer", "+2234561", 5),
                ("Coordinator", "118B1B0A-AEF2-4D0D-B199-A9B8B1B459F4", "Carlos Coordinator", "+223456278", 5),
                ("Supervisor", "09F153AB-E0D6-4315-91CE-29F3969A81CA", "Sergio Supervisor", "+2234563", 5),
                ("Supervisor", "E5C0A4BC-AEB3-4995-BA46-1DC81F4F9686", "Sofia Supervisor", "+2234566", 5),
                ("TechnicalAdvisor", "4E87746D-925F-46A0-8515-E00CA8A1961E", "Juan TechnicalAdvisor", "+2234564", 5),
                ("Manager", "92C804DD-A3DC-43EF-8CD5-6532AFA827FE", "Juana Manager", "+2234565", 5),
                ("Manager", "6A842190-1FF4-4CCF-97A4-41CB85F1B534", "Maria Manager", "+22345653", 6),
                ("Supervisor", "528D807B-4DC2-4F42-A85D-BEDCE7FCF160", "Paola Supervisor", "+2234536", 6),
                ("Supervisor", "12A420F5-BF48-479D-A391-4EA32118ADFE", "Silvia Supervisor", "+22345667", 6)
            };

            var englishUsersToCreate = usersToCreate.Take(10).ToArray();
            var frenchUsersToCreate = usersToCreate.Skip(10).Take(9).ToArray();
            var spanishUsersToCreate = usersToCreate.TakeLast(9).ToArray();

            SeedIdentityUsers(dbConnectionString, usersToCreate, password);
            SeedEnglishNyssDemoData(dbConnectionString, englishUsersToCreate);
            SeedFrenchNyssDemoData(dbConnectionString, frenchUsersToCreate);
            SeedSpanishNyssDemoData(dbConnectionString, spanishUsersToCreate);
        }

        private static void SeedIdentityUsers(string dbConnectionString, (string roleName, string id, string name, string phone, int organizationId)[] usersToCreate, string password)
        {
            var hasher = new PasswordHasher<IdentityUser>();
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseSqlServer(dbConnectionString, x => x.UseNetTopologySuite());

            var identityUsers = usersToCreate.Select(user => new IdentityUser
            {
                Id = user.id,
                AccessFailedCount = 0,
                ConcurrencyStamp = Guid.NewGuid().ToString(),
                Email = $"{user.name.Replace(" ", "_")}@example.com".ToLower(),
                EmailConfirmed = true,
                LockoutEnabled = false,
                LockoutEnd = null,
                NormalizedEmail = $"{user.name.Replace(" ", "_")}@example.com".ToUpper(),
                NormalizedUserName = $"{user.name.Replace(" ", "_")}@example.com".ToUpper(),
                PasswordHash = hasher.HashPassword(null, password),
                PhoneNumber = null,
                PhoneNumberConfirmed = false,
                SecurityStamp = Guid.NewGuid().ToString(),
                TwoFactorEnabled = false,
                UserName = $"{user.name.Replace(" ", "_")}@example.com".ToLower()
            });

            using (var context = new ApplicationDbContext(optionsBuilder.Options))
            {
                if (context.Users.Count() > 1)
                {
                    throw new Exception("Database is not empty! More than just the admin user exists.");
                }

                context.Users.AddRange(identityUsers);
                context.SaveChanges();
                context.UserRoles.AddRange(usersToCreate.Select(user => new IdentityUserRole<string>
                {
                    RoleId = context.Roles.First(r => r.Name == user.roleName).Id,
                    UserId = user.id
                }));

                context.SaveChanges();
            }

            Console.WriteLine("Successfully added ApplicationDbContext demo data");
        }

        private static void SeedEnglishNyssDemoData(string dbConnectionString, (string roleName, string id, string name, string phone, int organizationId)[] usersToCreate)
        {
            var optionsBuilder = new DbContextOptionsBuilder<NyssContext>();
            optionsBuilder.UseSqlServer(dbConnectionString, x => x.UseNetTopologySuite());

            using (var context = new NyssContext(optionsBuilder.Options))
            {
                if (context.NationalSocieties.Any())
                {
                    throw new Exception("Database is not empty, existing national societies!");
                }

                context.NationalSocieties.Add(new NationalSociety
                {
                    ContentLanguage = context.ContentLanguages.First(),
                    Country = new Country
                    {
                        Name = "Mandawi",
                        CountryCode = "MI"
                    },
                    Name = "Mandawi example National Society",
                    StartDate = DateTime.UtcNow,
                    Organizations = new List<Organization>
                    {
                        new Organization { Name = "Demo Organization" },
                        new Organization { Name = "Another Organization" }
                    }
                });
                context.SaveChanges();

                context.NationalSocieties.First().DefaultOrganization = context.Organizations.First();

                context.Projects.Add(new Project
                {
                    Name = "Mandawi test project",
                    NationalSociety = context.NationalSocieties.First(),
                    StartDate = DateTime.UtcNow,
                    State = ProjectState.Open,
                    AllowMultipleOrganizations = true
                });
                context.SaveChanges();

                context.ProjectOrganizations.AddRange(context.Organizations.Select(o => new ProjectOrganization
                {
                    OrganizationId = o.Id,
                    ProjectId = context.Projects.First().Id
                }));

                context.Users.AddRange(usersToCreate.Select(user =>
                {
                    User nyssUser = null;
                    var (roleName, id, name, phone, organizationId) = user;

                    switch (roleName)
                    {
                        case "Supervisor":
                            nyssUser = new SupervisorUser
                            {
                                Role = Role.Supervisor,
                                CurrentProject = context.Projects.First(),
                                Sex = Sex.Other,
                                DecadeOfBirth = 1980
                            };
                            break;
                        case "DataConsumer":
                            nyssUser = new DataConsumerUser();
                            break;
                        case "GlobalCoordinator":
                            nyssUser = new GlobalCoordinatorUser();
                            break;
                        case "Coordinator":
                            nyssUser = new CoordinatorUser();
                            break;
                        case "Manager":
                            nyssUser = new ManagerUser();
                            break;
                        case "TechnicalAdvisor":
                            nyssUser = new TechnicalAdvisorUser();
                            break;
                    }

                    nyssUser.Name = $"{name}";
                    nyssUser.PhoneNumber = phone;
                    nyssUser.IdentityUserId = id;
                    nyssUser.ApplicationLanguage = context.ApplicationLanguages.First();
                    nyssUser.EmailAddress = $"{user.name.Replace(" ", "_")}@example.com".ToLower();
                    nyssUser.IsFirstLogin = false;

                    return nyssUser;
                }));

                context.SaveChanges();

                context.SupervisorUserProjects.AddRange(context.Users.Where(u => u.Role == Role.Supervisor)
                    .Select(sup => new SupervisorUserProject
                    {
                        Project = context.Projects.First(),
                        SupervisorUser = (SupervisorUser)sup
                    })
                );

                context.UserNationalSocieties.AddRange(usersToCreate.Where(x => x.roleName != "GlobalCoordinator").Select(user => new UserNationalSociety
                {
                    NationalSocietyId = 1,
                    UserId = context.Users.First(u => u.Name == user.name).Id,
                    Organization = user.roleName == "DataConsumer" ? null : context.Organizations.First(o => o.Id == user.organizationId)
                }));

                context.NationalSocieties.First().Organizations.First(o => o.Id == 1).HeadManager = context.Users.First(u => u.EmailAddress.Contains("mary"));
                context.NationalSocieties.First().Organizations.First(o => o.Id == 2).HeadManager = context.Users.First(u => u.EmailAddress.Contains("martin"));

                context.SaveChanges();
            }

            Console.WriteLine("Successfully added english NyssContext demo data");
        }

        private static void SeedFrenchNyssDemoData(string dbConnectionString, (string roleName, string id, string name, string phone, int organizationId)[] usersToCreate)
        {
            var optionsBuilder = new DbContextOptionsBuilder<NyssContext>();
            optionsBuilder.UseSqlServer(dbConnectionString, x => x.UseNetTopologySuite());

            using (var context = new NyssContext(optionsBuilder.Options))
            {
                context.NationalSocieties.Add(new NationalSociety
                {
                    ContentLanguage = context.ContentLanguages.First(cl => cl.LanguageCode == "fr"),
                    Country = new Country
                    {
                        Name = "Mandawi",
                        CountryCode = "MI"
                    },
                    Name = "Mandawi exemple Société Nationale",
                    StartDate = DateTime.UtcNow,
                    Organizations = new List<Organization>
                    {
                        new Organization { Name = "Organisation de démonstration" },
                        new Organization { Name = "Une autre organisation" }
                    }
                });
                context.SaveChanges();

                context.NationalSocieties.First(ns => ns.Id == 2).DefaultOrganization = context.Organizations.First(o => o.Id == 3);

                context.Projects.Add(new Project
                {
                    Name = "Mandawi - Projet test",
                    NationalSociety = context.NationalSocieties.First(ns => ns.Id == 2),
                    StartDate = DateTime.UtcNow,
                    State = ProjectState.Open,
                    AllowMultipleOrganizations = true
                });
                context.SaveChanges();

                context.ProjectOrganizations.AddRange(context.Organizations.Where(o => o.NationalSocietyId == 2).Select(o => new ProjectOrganization
                {
                    OrganizationId = o.Id,
                    ProjectId = context.Projects.First(p => p.NationalSocietyId == 2).Id
                }));

                context.Users.AddRange(usersToCreate.Select(user =>
                {
                    User nyssUser = null;
                    var (roleName, id, name, phone, organizationId) = user;

                    switch (roleName)
                    {
                        case "Supervisor":
                            nyssUser = new SupervisorUser
                            {
                                Role = Role.Supervisor,
                                CurrentProject = context.Projects.First(p => p.NationalSocietyId == 2),
                                Sex = Sex.Other,
                                DecadeOfBirth = 1980
                            };
                            break;
                        case "DataConsumer":
                            nyssUser = new DataConsumerUser();
                            break;
                        case "Coordinator":
                            nyssUser = new CoordinatorUser();
                            break;
                        case "Manager":
                            nyssUser = new ManagerUser();
                            break;
                        case "TechnicalAdvisor":
                            nyssUser = new TechnicalAdvisorUser();
                            break;
                    }

                    nyssUser.Name = $"{name}";
                    nyssUser.PhoneNumber = phone;
                    nyssUser.IdentityUserId = id;
                    nyssUser.ApplicationLanguage = context.ApplicationLanguages.First(al => al.LanguageCode == "fr");
                    nyssUser.EmailAddress = $"{user.name.Replace(" ", "_")}@example.com".ToLower();
                    nyssUser.IsFirstLogin = false;

                    return nyssUser;
                }));

                context.SaveChanges();

                context.SupervisorUserProjects.AddRange(context.Users.Where(u => u.Role == Role.Supervisor && !context.SupervisorUserProjects.Any(sup => sup.SupervisorUserId == u.Id))
                    .Select(sup => new SupervisorUserProject
                    {
                        Project = context.Projects.First(p => p.NationalSocietyId == 2),
                        SupervisorUser = (SupervisorUser)sup
                    })
                );

                context.UserNationalSocieties.AddRange(usersToCreate.Where(x => x.roleName != "GlobalCoordinator").Select(user => new UserNationalSociety
                {
                    NationalSocietyId = 2,
                    UserId = context.Users.First(u => u.Name == user.name).Id,
                    Organization = user.roleName == "DataConsumer" ? null : context.Organizations.First(o => o.Id == user.organizationId)
                }));

                context.NationalSocieties.First(ns => ns.Id == 2).Organizations.First(o => o.Id == 3).HeadManager = context.Users.First(u => u.EmailAddress.Contains("marcelle"));
                context.NationalSocieties.First(ns => ns.Id == 2).Organizations.First(o => o.Id == 4).HeadManager = context.Users.First(u => u.EmailAddress.Contains("marc"));

                context.SaveChanges();
            }

            Console.WriteLine("Successfully added french NyssContext demo data");
        }

        private static void SeedSpanishNyssDemoData(string dbConnectionString, (string roleName, string id, string name, string phone, int organizationId)[] usersToCreate)
        {
            var optionsBuilder = new DbContextOptionsBuilder<NyssContext>();
            optionsBuilder.UseSqlServer(dbConnectionString, x => x.UseNetTopologySuite());

            using (var context = new NyssContext(optionsBuilder.Options))
            {
                context.NationalSocieties.Add(new NationalSociety
                {
                    ContentLanguage = context.ContentLanguages.First(cl => cl.LanguageCode == "es"),
                    Country = new Country
                    {
                        Name = "Mandawi",
                        CountryCode = "MI"
                    },
                    Name = "Mandawi ejemplo Sociedad Nacional",
                    StartDate = DateTime.UtcNow,
                    Organizations = new List<Organization>
                    {
                        new Organization { Name = "Organización de la demostración" },
                        new Organization { Name = "Un otro organización" }
                    }
                });
                context.SaveChanges();

                context.NationalSocieties.First(ns => ns.Id == 3).DefaultOrganization = context.Organizations.First(o => o.Id == 5);

                context.Projects.Add(new Project
                {
                    Name = "Mandawi - Proyecto de test",
                    NationalSociety = context.NationalSocieties.First(ns => ns.Id == 3),
                    StartDate = DateTime.UtcNow,
                    State = ProjectState.Open,
                    AllowMultipleOrganizations = true
                });
                context.SaveChanges();

                context.ProjectOrganizations.AddRange(context.Organizations.Where(o => o.NationalSocietyId == 3).Select(o => new ProjectOrganization
                {
                    OrganizationId = o.Id,
                    ProjectId = context.Projects.First(p => p.NationalSocietyId == 3).Id
                }));

                context.Users.AddRange(usersToCreate.Select(user =>
                {
                    User nyssUser = null;
                    var (roleName, id, name, phone, organizationId) = user;

                    switch (roleName)
                    {
                        case "Supervisor":
                            nyssUser = new SupervisorUser
                            {
                                Role = Role.Supervisor,
                                CurrentProject = context.Projects.First(p => p.NationalSocietyId == 3),
                                Sex = Sex.Other,
                                DecadeOfBirth = 1980
                            };
                            break;
                        case "DataConsumer":
                            nyssUser = new DataConsumerUser();
                            break;
                        case "Coordinator":
                            nyssUser = new CoordinatorUser();
                            break;
                        case "Manager":
                            nyssUser = new ManagerUser();
                            break;
                        case "TechnicalAdvisor":
                            nyssUser = new TechnicalAdvisorUser();
                            break;
                    }

                    nyssUser.Name = $"{name}";
                    nyssUser.PhoneNumber = phone;
                    nyssUser.IdentityUserId = id;
                    nyssUser.ApplicationLanguage = context.ApplicationLanguages.First(al => al.LanguageCode == "es");
                    nyssUser.EmailAddress = $"{user.name.Replace(" ", "_")}@example.com".ToLower();
                    nyssUser.IsFirstLogin = false;

                    return nyssUser;
                }));

                context.SaveChanges();

                context.SupervisorUserProjects.AddRange(context.Users.Where(u => u.Role == Role.Supervisor && !context.SupervisorUserProjects.Any(sup => sup.SupervisorUserId == u.Id))
                    .Select(sup => new SupervisorUserProject
                    {
                        Project = context.Projects.First(p => p.NationalSocietyId == 3),
                        SupervisorUser = (SupervisorUser)sup
                    })
                );

                context.UserNationalSocieties.AddRange(usersToCreate.Where(x => x.roleName != "GlobalCoordinator").Select(user => new UserNationalSociety
                {
                    NationalSocietyId = 3,
                    UserId = context.Users.First(u => u.Name == user.name).Id,
                    Organization = user.roleName == "DataConsumer" ? null : context.Organizations.First(o => o.Id == user.organizationId)
                }));

                context.NationalSocieties.First(ns => ns.Id == 3).Organizations.First(o => o.Id == 5).HeadManager = context.Users.First(u => u.EmailAddress.Contains("juana"));
                context.NationalSocieties.First(ns => ns.Id == 3).Organizations.First(o => o.Id == 6).HeadManager = context.Users.First(u => u.EmailAddress.Contains("maria"));

                context.SaveChanges();
            }

            Console.WriteLine("Successfully added spanish NyssContext demo data");
        }
    }
}
