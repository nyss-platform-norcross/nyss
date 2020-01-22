using System;
using System.Collections.Generic;
using System.Linq;
using NSubstitute;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.TestData.TestDataGeneration;
using RX.Nyss.Web.Features.SmsGateways;
using RX.Nyss.Web.Features.SmsGateways.Dto;

namespace RX.Nyss.Web.Tests.Features.NationalSocieties.TestData
{
    public class NationalSocietyServiceTestData
    {
        private readonly ISmsGatewayService _smsGatewayServiceMock;

        public struct ArchiveNationalSocietyAdditionalData
        {
            public NationalSociety NationalSocietyBeingArchived { get; set; }
            public List<int> SmsGatewaysIds { get; set; }
            public User HeadManager { get; set; }
        }

        public struct RestoreNationalSocietyAdditionalData
        {
            public NationalSociety NationalSocietyBeingReopened { get; set; }
        }


        private readonly TestCaseDataProvider _testCaseDataProvider;
        private readonly EntityNumerator _nationalSocietyNumerator = new EntityNumerator();
        private readonly EntityNumerator _userNumerator = new EntityNumerator();

        public BasicNationalSocietyServiceTestData BasicData { get; set; }

        public NationalSocietyServiceTestData(INyssContext nyssContextMock, ISmsGatewayService smsGatewayServiceMock)
        {
            _smsGatewayServiceMock = smsGatewayServiceMock;
            _testCaseDataProvider = new TestCaseDataProvider(nyssContextMock);
            
            BasicData = new BasicNationalSocietyServiceTestData(nyssContextMock, _nationalSocietyNumerator);
        }

        public TestCaseData<ArchiveNationalSocietyAdditionalData> ArchiveWhenHasProjects =>
            _testCaseDataProvider.GetOrCreate(nameof(ArchiveWhenHasProjects), (data) =>
            {
                var nationalSocietyWithProjects = new NationalSociety { Id = _nationalSocietyNumerator.Next , NationalSocietyUsers = new List<UserNationalSociety>()};
                data.NationalSocieties.Add(nationalSocietyWithProjects);
                data.Projects.Add(new Project{ NationalSociety = nationalSocietyWithProjects, NationalSocietyId = nationalSocietyWithProjects .Id});

                return new ArchiveNationalSocietyAdditionalData { NationalSocietyBeingArchived = nationalSocietyWithProjects };
            });

        public TestCaseData<ArchiveNationalSocietyAdditionalData> WhenHasNoProjectsAndNoUsersExceptHeadManagerWithRoleManager =>
            _testCaseDataProvider.GetOrCreate(nameof(WhenHasNoProjectsAndNoUsersExceptHeadManagerWithRoleManager), (data) =>
            {
                var nationalSociety = new NationalSociety { Id = _nationalSocietyNumerator.Next, NationalSocietyUsers = new List<UserNationalSociety>() };
                var headManagerUser = new ManagerUser { Id = _userNumerator.Next, Role = Role.Manager};
                var userNationalSociety = new UserNationalSociety
                {
                    User = headManagerUser, UserId = headManagerUser.Id, NationalSocietyId = nationalSociety.Id, NationalSociety = nationalSociety
                };
                nationalSociety.NationalSocietyUsers = new List<UserNationalSociety> { userNationalSociety };
                nationalSociety.HeadManager = headManagerUser;
                headManagerUser.UserNationalSocieties = new List<UserNationalSociety>{userNationalSociety};

                data.Users.Add(headManagerUser);
                data.UserNationalSocieties.Add(userNationalSociety);
                data.NationalSocieties.Add(nationalSociety);

                _smsGatewayServiceMock.GetSmsGateways(nationalSociety.Id).Returns(Result.Success(new List<GatewaySettingResponseDto>() ));

                return new ArchiveNationalSocietyAdditionalData { NationalSocietyBeingArchived = nationalSociety, HeadManager = headManagerUser};
            });

        public TestCaseData<ArchiveNationalSocietyAdditionalData> WhenHasNoProjectsAndNoUsersExceptHeadManagerWithRoleTechnicalAdvisor =>
            _testCaseDataProvider.GetOrCreate(nameof(WhenHasNoProjectsAndNoUsersExceptHeadManagerWithRoleTechnicalAdvisor), (data) =>
            {
                data.Include(WhenHasNoProjectsAndNoUsersExceptHeadManagerWithRoleManager.GenerateData().EntityData);
                var nationalSociety = WhenHasNoProjectsAndNoUsersExceptHeadManagerWithRoleManager.AdditionalData.NationalSocietyBeingArchived;
                var headManagerUser = WhenHasNoProjectsAndNoUsersExceptHeadManagerWithRoleManager.AdditionalData.HeadManager;

                headManagerUser.Role = Role.TechnicalAdvisor;

                return new ArchiveNationalSocietyAdditionalData { NationalSocietyBeingArchived = nationalSociety, HeadManager = headManagerUser };
            });

        public TestCaseData<ArchiveNationalSocietyAdditionalData> WhenHasNoProjectsAndSomeUsersExceptHeadManager =>
            _testCaseDataProvider.GetOrCreate(nameof(WhenHasNoProjectsAndSomeUsersExceptHeadManager), (data) =>
            {
                data.Include(WhenHasNoProjectsAndNoUsersExceptHeadManagerWithRoleManager.GenerateData().EntityData);

                var nationalSociety = WhenHasNoProjectsAndNoUsersExceptHeadManagerWithRoleManager.AdditionalData.NationalSocietyBeingArchived;

                var additionalUser = new ManagerUser{ Id = _userNumerator.Next };
                var userNationalSociety = new UserNationalSociety
                {
                    User = additionalUser,
                    UserId = additionalUser.Id,
                    NationalSocietyId = nationalSociety.Id,
                    NationalSociety = nationalSociety
                };

                additionalUser.UserNationalSocieties = new List<UserNationalSociety>{userNationalSociety};
                nationalSociety.NationalSocietyUsers.Add(userNationalSociety);

                return new ArchiveNationalSocietyAdditionalData { NationalSocietyBeingArchived = nationalSociety };
            });

        public TestCaseData<ArchiveNationalSocietyAdditionalData> WhenSuccessfullyArchivingNationalSocietyWith2SmsGateways =>
            _testCaseDataProvider.GetOrCreate(nameof(WhenSuccessfullyArchivingNationalSocietyWith2SmsGateways), (data) =>
            {
                data.Include(WhenHasNoProjectsAndNoUsersExceptHeadManagerWithRoleManager.GenerateData().EntityData);
                var nationalSociety = WhenHasNoProjectsAndNoUsersExceptHeadManagerWithRoleManager.AdditionalData.NationalSocietyBeingArchived;

                var smsGateway1 = new GatewaySettingResponseDto { Id = 1 };
                var smsGateway2 = new GatewaySettingResponseDto { Id = 2 };

                _smsGatewayServiceMock.GetSmsGateways(nationalSociety.Id).Returns(Result.Success(new List<GatewaySettingResponseDto> { smsGateway1, smsGateway2 } ));
                _smsGatewayServiceMock.DeleteSmsGateway(Arg.Any<int>()).Returns(Result.Success());

                return new ArchiveNationalSocietyAdditionalData { NationalSocietyBeingArchived = nationalSociety, SmsGatewaysIds = new List<int>{ smsGateway1.Id, smsGateway2.Id} };
            });

        public TestCaseData<RestoreNationalSocietyAdditionalData> WhenReopeningNationalSociety =>
            _testCaseDataProvider.GetOrCreate(nameof(WhenReopeningNationalSociety), (data) =>
            {
                var nationalSociety = new NationalSociety { Id = _nationalSocietyNumerator.Next, NationalSocietyUsers = new List<UserNationalSociety>() , IsArchived = true };
                data.NationalSocieties.Add(nationalSociety);

                data.NyssContextMockedMethods = (nyssContext) =>
                {
                    nyssContext.NationalSocieties.FindAsync(nationalSociety.Id).Returns(nationalSociety);
                };
                    
                return new RestoreNationalSocietyAdditionalData { NationalSocietyBeingReopened = nationalSociety };
            });

    }
}
