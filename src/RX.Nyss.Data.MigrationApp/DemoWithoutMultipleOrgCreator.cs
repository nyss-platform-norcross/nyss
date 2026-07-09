using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Data;

namespace RX.Nyss.Data.MigrationApp
{
    public class DemoWithoutMultipleOrgCreator
    {
        private record UserDefinition(
            string RoleName,
            string Id,
            string Name,
            string Phone,
            int OrganizationId)
        {
            public string Email => $"{Name.Replace(" ", "_")}@test.com".ToLower();
        }

        private record NationalSocietyDefinition(
            string LanguageCode,
            string SocietyName,
            string OrganizationName,
            string ProjectName,
            string HeadManagerEmailFragment,
            int NationalSocietyId,
            int OrganizationId);

        private static readonly UserDefinition[] AllUsers =
        [
            new("DataConsumer",       "6be5c384-6675-a7be-4c45-3cba40f7cd13", "Jane DataConsumer",      "+1234561", 1),
            new("HeadSupervisor",     "5c3b91e6-9788-41a8-908c-bf4c3131b14a", "Daniel HeadSupervisor",  "+1234555", 1),
            new("GlobalCoordinator",  "5f259e96-4200-0cbc-4fe6-90024dcb770a", "Kim GlobalCoordinator",  "+1234562", 1),
            new("Supervisor",         "a076dd25-30d5-cbb2-4174-f73fda1524fa", "Bob Supervisor",          "+1234563", 1),
            new("Supervisor",         "93485951-13fd-42e1-a98c-e5b54ce60073", "Simon Supervisor",        "+1234566", 1),
            new("TechnicalAdvisor",   "7c2d7baf-4400-6baf-497a-997a7ca18597", "Todd TechnicalAdvisor",  "+1234564", 1),
            new("Manager",            "9cfad185-f353-7c8f-4cf7-b2d3498eb715", "Mary Manager",            "+1234565", 1),
            new("DataConsumer",       "B7218E3A-4F5B-4164-A21A-3D2B9FA2A0D4", "Adam DataConsumer",      "+1234561", 2),
            new("Supervisor",         "9828F324-0513-4DC1-A8F6-8AC76D24B211", "Serge Supervisor",        "+1234563", 2),
            new("Supervisor",         "51CF49DD-235B-4B43-9C1E-CF781310ABB7", "Sophie Supervisor",       "+1234566", 2),
            new("TechnicalAdvisor",   "CA06B9BB-A93D-4AB7-A7D4-AC77955C947A", "Thierry TechnicalAdvisor", "+1234564", 2),
            new("Manager",            "BF23D073-A543-4C26-B83C-3BFF7227E65C", "Marcelle Manager",        "+1234565", 2),
            new("DataConsumer",       "0582597c-77fa-441c-97fd-d8759cf786d0", "Andres DataConsumer",     "+2234561", 3),
            new("Supervisor",         "fac99c32-cceb-4abc-9561-6af84c1cb7b3", "Sergio Supervisor",       "+2234563", 3),
            new("Supervisor",         "48b09322-9401-4d8f-8b6e-1eaaf2cc7c8b", "Sofia Supervisor",        "+2234566", 3),
            new("TechnicalAdvisor",   "869cec24-e2fd-436a-820f-8d854670e682", "Juan TechnicalAdvisor",   "+2234564", 3),
            new("Manager",            "b4c20415-a9a6-47e5-97aa-e6cbd98a38ef", "Juana Manager",           "+2234565", 3),
        ];

        private static readonly NationalSocietyDefinition[] SocietyDefinitions =
        [
            new("en", "English-Mandawi test National Society",       "Demo Organization",                   "Mandawi test project",          "mary",     1, 1),
            new("fr", "Français-Mandawi exemple Société Nationale",      "Organisation de démonstration",        "Mandawi - Projet test",         "marcelle", 2, 2),
            new("es", "Español-Mandawi ejemplo Sociedad Nacional",      "Organización de la demostración",      "Mandawi - Proyecto de test",    "juana",    3, 3),
        ];

        public static void CreateDemoWithoutMultipleOrgData(string dbConnectionString, string password = "P@ssw0rd")
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(dbConnectionString);

            SeedIdentityUsers(dbConnectionString, AllUsers, password);

            var usersByNs = new[]
            {
                AllUsers.Take(7).ToArray(),
                AllUsers.Skip(7).Take(5).ToArray(),
                AllUsers.TakeLast(5).ToArray(),
            };

            for (var i = 0; i < SocietyDefinitions.Length; i++)
            {
                SeedNyssData(dbConnectionString, usersByNs[i], SocietyDefinitions[i]);
            }
        }

        private static void SeedIdentityUsers(string dbConnectionString, UserDefinition[] usersToCreate, string password)
        {
            var hasher = new PasswordHasher<IdentityUser>();
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseSqlServer(dbConnectionString, x => x.UseNetTopologySuite());

            var identityUsers = usersToCreate.Select(user => new IdentityUser
            {
                Id = user.Id,
                AccessFailedCount = 0,
                ConcurrencyStamp = Guid.NewGuid().ToString(),
                Email = user.Email,
                EmailConfirmed = true,
                LockoutEnabled = false,
                LockoutEnd = null,
                NormalizedEmail = user.Email.ToUpper(),
                NormalizedUserName = user.Email.ToUpper(),
                PasswordHash = hasher.HashPassword(null!, password),
                PhoneNumber = null,
                PhoneNumberConfirmed = false,
                SecurityStamp = Guid.NewGuid().ToString(),
                TwoFactorEnabled = false,
                UserName = user.Email
            }).ToList();

            using var context = new ApplicationDbContext(optionsBuilder.Options);

            if (context.Users.Count() > 1)
            {
                throw new InvalidOperationException("Database is not empty! More than just the admin user exists.");
            }

            context.Users.AddRange(identityUsers);
            context.SaveChanges();

            var roleIds = context.Roles
                .Where(r => usersToCreate.Select(u => u.RoleName).Contains(r.Name))
                .ToDictionary(r => r.Name!, r => r.Id);

            context.UserRoles.AddRange(usersToCreate.Select(user =>
            {
                if (!roleIds.TryGetValue(user.RoleName, out var roleId))
                {
                    throw new InvalidOperationException($"Role '{user.RoleName}' not found in the database.");
                }

                return new IdentityUserRole<string> { RoleId = roleId, UserId = user.Id };
            }));

            context.SaveChanges();
            Console.WriteLine("Successfully added ApplicationDbContext demo data");
        }

        private static void SeedNyssData(
            string dbConnectionString,
            UserDefinition[] usersToCreate,
            NationalSocietyDefinition def)
        {
            var optionsBuilder = new DbContextOptionsBuilder<NyssContext>();
            optionsBuilder.UseSqlServer(dbConnectionString, x => x.UseNetTopologySuite());

            using var context = new NyssContext(optionsBuilder.Options);

            // --- Guard: only check for the English (first) society ---
            if (def.NationalSocietyId == 1 && context.NationalSocieties.Any())
            {
                throw new InvalidOperationException("Database is not empty, existing national societies!");
            }

            // --- Lookups ---
            var contentLanguage = context.ContentLanguages.FirstOrDefault(cl => cl.LanguageCode == def.LanguageCode)
                ?? throw new InvalidOperationException($"Content language '{def.LanguageCode}' not found.");

            var applicationLanguage = context.ApplicationLanguages.FirstOrDefault(al => al.LanguageCode == def.LanguageCode)
                ?? throw new InvalidOperationException($"Application language '{def.LanguageCode}' not found.");

            // --- National Society ---
            var nationalSociety = new NationalSociety
            {
                ContentLanguage = contentLanguage,
                Country = new Country { Name = "Mandawi", CountryCode = "MI" },
                Name = def.SocietyName,
                StartDate = DateTime.UtcNow,
                Organizations = new List<Organization>
                {
                    new Organization { Name = def.OrganizationName }
                }
            };

            context.NationalSocieties.Add(nationalSociety);
            context.SaveChanges();

            var organization = context.Organizations.FirstOrDefault(o => o.Id == def.OrganizationId)
                ?? throw new InvalidOperationException($"Organization {def.OrganizationId} not found.");

            nationalSociety.DefaultOrganization = organization;

            // --- Project ---
            var project = new Project
            {
                Name = def.ProjectName,
                NationalSociety = nationalSociety,
                StartDate = DateTime.UtcNow,
                State = ProjectState.Open,
                AllowMultipleOrganizations = false
            };

            context.Projects.Add(project);
            context.SaveChanges();

            context.ProjectOrganizations.Add(new ProjectOrganization
            {
                OrganizationId = organization.Id,
                ProjectId = project.Id
            });

            // --- Users ---
            var nyssUsers = usersToCreate.Select(user =>
            {
                var nyssUser = CreateNyssUser(user.RoleName, project)
                    ?? throw new InvalidOperationException($"Unsupported role: '{user.RoleName}'.");

                nyssUser.Name = user.Name;
                nyssUser.PhoneNumber = user.Phone;
                nyssUser.IdentityUserId = user.Id;
                nyssUser.ApplicationLanguage = applicationLanguage;
                nyssUser.EmailAddress = user.Email;
                nyssUser.IsFirstLogin = false;
                return nyssUser;
            }).ToList();

            context.Users.AddRange(nyssUsers);
            context.SaveChanges();

            // --- Supervisor projects ---
            var existingSupervisorIds = context.SupervisorUserProjects
                .Select(s => s.SupervisorUserId)
                .ToHashSet();

            context.SupervisorUserProjects.AddRange(
                nyssUsers
                    .OfType<SupervisorUser>()
                    .Where(s => !existingSupervisorIds.Contains(s.Id))
                    .Select(s => new SupervisorUserProject { Project = project, SupervisorUser = s })
            );

            // --- User–National Society links ---
            context.UserNationalSocieties.AddRange(usersToCreate
                .Where(u => u.RoleName != "GlobalCoordinator")
                .Select(user =>
                {
                    var nyssUser = context.Users.FirstOrDefault(u => u.Name == user.Name)
                        ?? throw new InvalidOperationException($"Nyss user '{user.Name}' not found after save.");

                    return new UserNationalSociety
                    {
                        NationalSocietyId = def.NationalSocietyId,
                        UserId = nyssUser.Id,
                        Organization = user.RoleName == "DataConsumer"
                            ? null
                            : context.Organizations.FirstOrDefault(o => o.Id == user.OrganizationId)
                              ?? throw new InvalidOperationException($"Organization {user.OrganizationId} not found.")
                    };
                }));

            // --- Head manager ---
            var headManager = context.Users.FirstOrDefault(u => u.EmailAddress!.Contains(def.HeadManagerEmailFragment))
                ?? throw new InvalidOperationException($"Head manager with email containing '{def.HeadManagerEmailFragment}' not found.");

            organization.HeadManager = headManager;

            context.SaveChanges();
            Console.WriteLine($"Successfully added {def.LanguageCode} NyssContext demo data");
        }

        private static User? CreateNyssUser(string roleName, Project project) =>
            roleName switch
            {
                "HeadSupervisor" => new HeadSupervisorUser
                {
                    Role = Role.HeadSupervisor,
                    CurrentProject = project,
                    Sex = Sex.Other,
                    DecadeOfBirth = 1990
                },
                "Supervisor" => new SupervisorUser
                {
                    Role = Role.Supervisor,
                    CurrentProject = project,
                    Sex = Sex.Other,
                    DecadeOfBirth = 1980
                },
                "DataConsumer"      => new DataConsumerUser(),
                "GlobalCoordinator" => new GlobalCoordinatorUser(),
                "Coordinator"       => new CoordinatorUser(),
                "Manager"           => new ManagerUser(),
                "TechnicalAdvisor"  => new TechnicalAdvisorUser(),
                _                   => null
            };
    }
}