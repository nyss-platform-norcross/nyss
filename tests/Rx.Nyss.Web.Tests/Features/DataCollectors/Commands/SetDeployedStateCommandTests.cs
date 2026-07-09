using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MockQueryable.NSubstitute;
using NSubstitute;
using RX.Nyss.Common.Utils;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Data;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Features.DataCollectors.Commands;
using Shouldly;
using Xunit;

namespace RX.Nyss.Web.Tests.Features.DataCollectors.Commands
{
    public class SetDeployedStateCommandTests
    {
        private readonly INyssContext _nyssContextMock;
        private readonly IDateTimeProvider _dateTimeProviderMock;
        private readonly SetDeployedStateCommand.Handler _handler;

        public SetDeployedStateCommandTests()
        {
            _nyssContextMock = Substitute.For<INyssContext>();
            _dateTimeProviderMock = Substitute.For<IDateTimeProvider>();

            ArrangeData();

            _handler = new SetDeployedStateCommand.Handler(_nyssContextMock, _dateTimeProviderMock);
        }

        [Fact]
        public async Task SetDeployedState_WhenSetToNotDeployed_ShouldReturnCorrectMessage()
        {
            // Arrange
            var command = new SetDeployedStateCommand
            {
                Deployed = false,
                DataCollectorIds = new List<int> { 1 }
            };

            // Act
            var res = await _handler.Handle(command, CancellationToken.None);

            // Assert
            res.IsSuccess.ShouldBeTrue();
            res.Message.Key.ShouldBe(ResultKey.DataCollector.SetToNotDeployedSuccess);
        }

        [Fact]
        public async Task SetDeployedState_WhenSetToDeployed_ShouldReturnCorrectMessage()
        {
            // Arrange
            var command = new SetDeployedStateCommand
            {
                Deployed = true,
                DataCollectorIds = new List<int> { 2 }
            };

            // Act
            var res = await _handler.Handle(command, CancellationToken.None);

            // Assert
            res.IsSuccess.ShouldBeTrue();
            res.Message.Key.ShouldBe(ResultKey.DataCollector.SetToDeployedSuccess);
        }

        private void ArrangeData()
        {
            var dataCollectors = new List<DataCollector>
            {
                new DataCollector
                {
                    Id = 1,
                    Deployed = true,
                    DatesNotDeployed = new List<DataCollectorNotDeployed>()
                },
                new DataCollector
                {
                    Id = 2,
                    Deployed = false,
                    DatesNotDeployed = new List<DataCollectorNotDeployed> { new DataCollectorNotDeployed { StartDate = new DateTime(2021, 12, 10) } }
                }
            };

            var dataCollectorsMockDbSet = dataCollectors.AsQueryable().BuildMockDbSet();
            _nyssContextMock.DataCollectors.Returns(dataCollectorsMockDbSet);
        }
    }
}
