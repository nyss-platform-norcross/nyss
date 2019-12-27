using System;
using System.Collections.Generic;
using System.Text;
using RX.Nyss.Data.Models;

namespace RX.Nyss.Web.Tests.Features.ProjectDashboard
{
    public class ProjectDashboardTests
    {
    }

    public class ProjectDashboardTestData
    {
        private const int _nationalSocietyId = 1;
        private const int _projectId = 1;

        public List<NationalSociety> NationalSocieties { get; set; }
        public List<Project> Projects { get; set; }
        public List<DataCollector> DataCollectors { get; set; }

        public ProjectDashboardTestData()
        {
            NationalSocieties = new List<NationalSociety> { new NationalSociety { Id = _nationalSocietyId } };

            Projects = new List<Project>
            {
                new Project { Id = _projectId, NationalSocietyId = _nationalSocietyId, NationalSociety = NationalSocieties[0]}
            };

            DataCollectors = GenerateDataCollectorsWithReports();
        }

        private List<DataCollector> GenerateDataCollectorsWithReports()
        {
            throw new NotImplementedException();
        }
    }
}
