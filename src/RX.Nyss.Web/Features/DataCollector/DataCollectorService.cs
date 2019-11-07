using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using RX.Nyss.Data;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Utils.DataContract;
using RX.Nyss.Web.Utils.Logging;
using static RX.Nyss.Web.Utils.DataContract.Result;

namespace RX.Nyss.Web.Features.DataCollector
{
    public interface IDataCollectorService
    {
        Task<Result<int>> CreateDataCollector(int projectId, CreateDataCollectorRequestDto createDataCollectorDto);
        Task<Result> EditDataCollector(int projectId, EditDataCollectorRequestDto editDataCollectorDto);
        Task<Result> RemoveDataCollector(int dataCollectorId);
        Task<Result<GetDataCollectorResponseDto>> GetDataCollector(int dataCollectorId);
        Task<Result<IEnumerable<DataCollectorResponseDto>>> ListDataCollectors();
    }

    public class DataCollectorService : IDataCollectorService
    {
        private readonly INyssContext _nyssContext;
        private readonly ILoggerAdapter _logger;

        public DataCollectorService(ILoggerAdapter logger, INyssContext nyssContext)
        {
            _nyssContext = nyssContext;
            _logger = logger;
        }

        public async Task<Result<GetDataCollectorResponseDto>> GetDataCollector(int dataCollectorId)
        {
            try
            {
                var dataCollector = await _nyssContext.DataCollectors
                    .Include(dc => dc.Village).ThenInclude(v => v.District).ThenInclude(d => d.Region)
                    .Select(dc => new GetDataCollectorResponseDto
                    {
                        Id = dc.Id,
                        Name = dc.Name,
                        DisplayName = dc.DisplayName,
                        DataCollectorType = dc.DataCollectorType,
                        Sex = dc.Sex,
                        BirthYearGroup = dc.BirthYearGroup,
                        PhoneNumber = dc.PhoneNumber,
                        AdditionalPhoneNumber = dc.AdditionalPhoneNumber,
                        Latitude = dc.Location.X,
                        Longitude = dc.Location.Y,
                        SupervisorId = dc.Supervisor.Id,
                        Region = dc.Village.District.Region.Name,
                        District = dc.Village.District.Name,
                        Village = dc.Village.Name,
                        Zone = dc.Zone != null ? dc.Zone.Name : null
                    }).FirstOrDefaultAsync(dc => dc.Id == dataCollectorId);

                if (dataCollector == null)
                {
                    return Error(ResultKey.DataCollector.DataCollectorNotFound).Cast<GetDataCollectorResponseDto>();
                }

                return Success(dataCollector);
            }
            catch (Exception e)
            {
                _logger.Debug(e.Message);
                return Error(ResultKey.Shared.GeneralErrorMessage).Cast<GetDataCollectorResponseDto>();
            }
        }

        public async Task<Result<IEnumerable<DataCollectorResponseDto>>> ListDataCollectors()
        {
            try
            {
                var dataCollectors = await _nyssContext.DataCollectors.Select(dc => new DataCollectorResponseDto
                {
                    Name = dc.Name,
                    DisplayName = dc.DisplayName,
                    PhoneNumber = dc.PhoneNumber,
                    Village = dc.Village.Name,
                    District = dc.Village.District.Name,
                    Region = dc.Village.District.Region.Name
                }).ToListAsync();

                return Success<IEnumerable<DataCollectorResponseDto>>(dataCollectors);
            }
            catch (Exception e)
            {
                _logger.Debug(e);
                return Error(ResultKey.Shared.GeneralErrorMessage).Cast<IEnumerable<DataCollectorResponseDto>>();
            }
        }

        public async Task<Result<int>> CreateDataCollector(int projectId, CreateDataCollectorRequestDto createDataCollectorDto)
        {
            try
            {
                var phoneNumberExists = await _nyssContext.DataCollectors.AnyAsync(dc => dc.PhoneNumber == createDataCollectorDto.PhoneNumber);

                if (phoneNumberExists)
                {
                    return Error(ResultKey.DataCollector.PhoneNumberAlreadyExists).Cast<int>();
                }

                var project = await _nyssContext.Projects.Select(p => new Project
                {
                    Id = p.Id,
                    Name = p.Name,
                    NationalSociety = p.NationalSociety,
                    State = p.State,
                    TimeZone = p.TimeZone
                }).FirstOrDefaultAsync(p => p.Id == projectId);

                var village = await _nyssContext.Villages
                    .Include(v => v.District).ThenInclude(d => d.Region).ThenInclude(r => r.NationalSociety)
                    .FirstOrDefaultAsync(v => v.Name == createDataCollectorDto.Village && v.District.Region.NationalSociety.Id == project.NationalSociety.Id);

                var zone = string.IsNullOrEmpty(createDataCollectorDto.Zone) ? null : await _nyssContext.Zones.FirstOrDefaultAsync(z => z.Village.Id == village.Id);
                var supervisor = (SupervisorUser)await _nyssContext.Users.FindAsync(createDataCollectorDto.SupervisorId);

                var dataCollector = new Nyss.Data.Models.DataCollector
                {
                    Name = createDataCollectorDto.Name,
                    DisplayName = createDataCollectorDto.DisplayName,
                    PhoneNumber = createDataCollectorDto.PhoneNumber,
                    AdditionalPhoneNumber = createDataCollectorDto.AdditionalPhoneNumber,
                    BirthYearGroup = createDataCollectorDto.BirthYearGroup,
                    Sex = createDataCollectorDto.Sex,
                    DataCollectorType = createDataCollectorDto.DataCollectorType,
                    Location = new Point(createDataCollectorDto.Latitude, createDataCollectorDto.Longitude),
                    Village = village,
                    Supervisor = supervisor,
                    Project = project,
                    Zone = zone
                };

                var entity = await _nyssContext.AddAsync(dataCollector);
                await _nyssContext.SaveChangesAsync();
                return Success(entity.Entity.Id, ResultKey.DataCollector.CreateSuccess);
            }
            catch (Exception e)
            {
                _logger.Debug(e);
                return Error(ResultKey.DataCollector.CreateError).Cast<int>();
            }
        }

        public async Task<Result> EditDataCollector(int projectId, EditDataCollectorRequestDto editDataCollectorDto)
        {
            try
            {
                var dataCollector = await _nyssContext.DataCollectors
                    .Include(dc => dc.Project)
                    .Include(dc => dc.Supervisor)
                    .Include(dc => dc.Village).ThenInclude(v => v.District).ThenInclude(d => d.Region)
                    .Include(dc => dc.Zone)
                    .SingleOrDefaultAsync(dc => dc.Id == editDataCollectorDto.Id);

                if (dataCollector == null)
                {
                    return Error(ResultKey.DataCollector.DataCollectorNotFound);
                }

                dataCollector.Name = editDataCollectorDto.Name;
                dataCollector.DisplayName = editDataCollectorDto.DisplayName;
                dataCollector.PhoneNumber = editDataCollectorDto.PhoneNumber;
                dataCollector.AdditionalPhoneNumber = editDataCollectorDto.AdditionalPhoneNumber;
                dataCollector.Location = new Point(editDataCollectorDto.Latitude, editDataCollectorDto.Longitude);
                dataCollector.Sex = editDataCollectorDto.Sex;
                dataCollector.DataCollectorType = editDataCollectorDto.DataCollectorType;

                if (dataCollector.Project.Id != projectId)
                {
                    dataCollector.Project = await _nyssContext.Projects.FindAsync(projectId);
                }

                if (dataCollector.Supervisor.Id != editDataCollectorDto.SupervisorId)
                {
                    dataCollector.Supervisor = (SupervisorUser)await _nyssContext.Users.FindAsync(editDataCollectorDto.SupervisorId);
                }

                if (dataCollector.Village.Name != editDataCollectorDto.Village)
                {
                    dataCollector.Village = await _nyssContext.Villages
                        .FirstOrDefaultAsync(v => v.Name == editDataCollectorDto.Name && v.District.Region.NationalSociety.Id == dataCollector.Project.Id);
                }

                if (!string.IsNullOrEmpty(editDataCollectorDto.Zone) && (dataCollector.Zone == null || dataCollector.Zone.Name != editDataCollectorDto.Zone))
                {
                    dataCollector.Zone = await _nyssContext.Zones.FirstOrDefaultAsync(z => z.Name == editDataCollectorDto.Zone && z.Village.Id == dataCollector.Village.Id);
                } 

                await _nyssContext.SaveChangesAsync();
                return SuccessMessage(ResultKey.DataCollector.EditSuccess);
            }
            catch (Exception e)
            {
                _logger.Debug(e);
                return Error(ResultKey.DataCollector.EditError);
            }
        }

        public async Task<Result> RemoveDataCollector(int dataCollectorId)
        {
            try
            {
                var dataCollector = await _nyssContext.DataCollectors.FindAsync(dataCollectorId);

                if (dataCollector == null)
                {
                    return Error(ResultKey.DataCollector.DataCollectorNotFound);
                }

                _nyssContext.DataCollectors.Remove(dataCollector);
                await _nyssContext.SaveChangesAsync();
                return SuccessMessage(ResultKey.DataCollector.RemoveSuccess);
            }
            catch (Exception e)
            {
                _logger.Debug(e);
                return Error(ResultKey.DataCollector.RemoveError);
            }
        }
    }
}
