using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Data;
using NyssUser = RX.Nyss.Data.Models.User;

namespace RX.Nyss.Data.MigrationApp
{
    public class TrainingDataCreator
    {
        private class User
        {
            public required string RoleName { get; init; }
            public required string Id { get; init; }
            public required string Name { get; init; }
            public required string Phone { get; init; }
            public int OrganizationId { get; init; }

            public string Email => $"{Name.Replace(" ", "_")}@test.com".ToLower();
        }

        public static void CreateTrainingData(string dbConnectionString, int groupCount, string password = "nysstraining")
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(dbConnectionString);
            ArgumentOutOfRangeException.ThrowIfLessThan(groupCount, 1);

            var usersToCreate = new User[groupCount];

            for (var i = 0; i < groupCount; i++)
            {
                usersToCreate[i] = new User
                {
                    RoleName = "GlobalCoordinator",
                    Id = Guid.NewGuid().ToString(),
                    Name = $"Global Coordinator {i + 1}",
                    Phone = $"+123456{i + 1}",
                    OrganizationId = 1
                };
            }

            SeedIdentityUsers(dbConnectionString, usersToCreate, password);
            SeedTrainingData(dbConnectionString, groupCount, usersToCreate);
        }

        private static void SeedIdentityUsers(string dbConnectionString, User[] usersToCreate, string password)
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

            var userRoles = usersToCreate.Select(user =>
            {
                if (!roleIds.TryGetValue(user.RoleName, out var roleId))
                {
                    throw new InvalidOperationException($"Role '{user.RoleName}' not found in the database.");
                }

                return new IdentityUserRole<string>
                {
                    RoleId = roleId,
                    UserId = user.Id
                };
            });

            context.UserRoles.AddRange(userRoles);
            context.SaveChanges();

            Console.WriteLine("Successfully added ApplicationDbContext demo data");
        }

        private static void SeedTrainingData(string dbConnectionString, int numberOfGroups, IEnumerable<User> usersToCreate)
        {
            var optionsBuilder = new DbContextOptionsBuilder<NyssContext>();
            optionsBuilder.UseSqlServer(dbConnectionString, options => options.UseNetTopologySuite());

            using var context = new NyssContext(optionsBuilder.Options);

            if (context.NationalSocieties.Any())
            {
                throw new InvalidOperationException("Database is not empty, existing national societies!");
            }

            var contentLanguage = context.ContentLanguages.FirstOrDefault()
                ?? throw new InvalidOperationException("No content languages found in the database.");

            var applicationLanguage = context.ApplicationLanguages.FirstOrDefault()
                ?? throw new InvalidOperationException("No application languages found in the database.");

            var nationalSocieties = Enumerable.Range(1, numberOfGroups).Select(i => new NationalSociety
            {
                ContentLanguage = contentLanguage,
                Country = new Country
                {
                    Name = "Mandawi",
                    CountryCode = "MI"
                },
                Name = $"Training group {i}",
                StartDate = DateTime.UtcNow,
                Organizations = new List<Organization>
                {
                    new Organization { Name = "Demo Organization" }
                }
            }).ToList();

            context.NationalSocieties.AddRange(nationalSocieties);
            context.SaveChanges();

            foreach (var ns in context.NationalSocieties.ToList())
            {
                ns.DefaultOrganization = ns.Organizations!.FirstOrDefault()
                    ?? throw new InvalidOperationException($"National society '{ns.Name}' has no organizations.");
            }

            context.SaveChanges();

            var defaultProject = context.Projects.FirstOrDefault();

            var nyssUsers = usersToCreate.Select(user =>
            {
                var nyssUser = CreateNyssUser(user.RoleName, defaultProject)
                    ?? throw new InvalidOperationException($"Unsupported role: '{user.RoleName}'.");

                nyssUser.Name = user.Name;
                nyssUser.PhoneNumber = user.Phone;
                nyssUser.IdentityUserId = user.Id;
                nyssUser.ApplicationLanguage = applicationLanguage;
                nyssUser.EmailAddress = user.Email;
                nyssUser.IsFirstLogin = false;
                return nyssUser;
            });

            context.Users.AddRange(nyssUsers);
            context.SaveChanges();

            Console.WriteLine("Successfully added NyssContext training data");
        }

        private static NyssUser? CreateNyssUser(string roleName, Project? defaultProject) =>
            roleName switch
            {
                "Supervisor" => new SupervisorUser
                {
                    Role = Role.Supervisor,
                    CurrentProject = defaultProject ?? throw new InvalidOperationException("No project found for Supervisor."),
                    Sex = Sex.Other,
                    DecadeOfBirth = 1980
                },
                "HeadSupervisor" => new HeadSupervisorUser
                {
                    Role = Role.HeadSupervisor,
                    CurrentProject = defaultProject ?? throw new InvalidOperationException("No project found for HeadSupervisor."),
                    Sex = Sex.Other,
                    DecadeOfBirth = 1990
                },
                "DataConsumer" => new DataConsumerUser(),
                "GlobalCoordinator" => new GlobalCoordinatorUser(),
                "Coordinator" => new CoordinatorUser(),
                "Manager" => new ManagerUser(),
                "TechnicalAdvisor" => new TechnicalAdvisorUser(),
                _ => null
            };
    }
}