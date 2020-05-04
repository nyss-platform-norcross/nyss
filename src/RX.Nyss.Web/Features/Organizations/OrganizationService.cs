using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Common.Utils.Logging;
using RX.Nyss.Data;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Features.Organizations.Dto;
using static RX.Nyss.Common.Utils.DataContract.Result;

namespace RX.Nyss.Web.Features.Organizations
{
    public interface IOrganizationService
    {
        Task<Result<OrganizationResponseDto>> Get(int organizationId);
        Task<Result<List<OrganizationResponseDto>>> List(int nationalSocietyId);
        Task<Result<int>> Create(int nationalSocietyId, OrganizationRequestDto gatewaySettingRequestDto);
        Task<Result> Edit(int organizationId, OrganizationRequestDto gatewaySettingRequestDto);
        Task<Result> Delete(int organizationId);
    }

    public class OrganizationService : IOrganizationService
    {
        private readonly INyssContext _nyssContext;
        private readonly ILoggerAdapter _loggerAdapter;

        public OrganizationService(
            INyssContext nyssContext,
            ILoggerAdapter loggerAdapter)
        {
            _nyssContext = nyssContext;
            _loggerAdapter = loggerAdapter;
        }

        public async Task<Result<OrganizationResponseDto>> Get(int organizationId)
        {
            var gatewaySetting = await _nyssContext.Organizations
                .Select(gs => new OrganizationResponseDto
                {
                    Id = gs.Id,
                    Name = gs.Name
                })
                .FirstOrDefaultAsync(gs => gs.Id == organizationId);

            if (gatewaySetting == null)
            {
                return Error<OrganizationResponseDto>(ResultKey.NationalSociety.Organization.SettingDoesNotExist);
            }

            var result = Success(gatewaySetting);

            return result;
        }

        public async Task<Result<List<OrganizationResponseDto>>> List(int nationalSocietyId)
        {
            var gatewaySettings = await _nyssContext.Organizations
                .Where(gs => gs.NationalSocietyId == nationalSocietyId)
                .OrderBy(gs => gs.Id)
                .Select(gs => new OrganizationResponseDto
                {
                    Id = gs.Id,
                    Name = gs.Name,
                })
                .ToListAsync();

            var result = Success(gatewaySettings);

            return result;
        }

        public async Task<Result<int>> Create(int nationalSocietyId, OrganizationRequestDto gatewaySettingRequestDto)
        {
            try
            {
                var nationalSociety = await _nyssContext.NationalSocieties
                    .Include(x => x.Country)
                    .SingleOrDefaultAsync(ns => ns.Id == nationalSocietyId);

                if (nationalSociety == null)
                {
                    return Error<int>(ResultKey.NationalSociety.Organization.NationalSocietyDoesNotExist);
                }

                var gatewaySettingToAdd = new Organization
                {
                    Name = gatewaySettingRequestDto.Name,
                    NationalSocietyId = nationalSocietyId
                };

                await _nyssContext.Organizations.AddAsync(gatewaySettingToAdd);
                await _nyssContext.SaveChangesAsync();
                
                return Success(gatewaySettingToAdd.Id, ResultKey.NationalSociety.Organization.SuccessfullyAdded);
            }
            catch (ResultException exception)
            {
                _loggerAdapter.Debug(exception);
                return exception.GetResult<int>();
            }
        }

        public async Task<Result> Edit(int organizationId, OrganizationRequestDto gatewaySettingRequestDto)
        {
            try
            {
                var entity = await _nyssContext.Organizations
                    .Include(x => x.NationalSociety.Country)
                    .SingleOrDefaultAsync(x => x.Id == organizationId);

                if (entity == null)
                {
                    return Error(ResultKey.NationalSociety.Organization.SettingDoesNotExist);
                }

                entity.Name = gatewaySettingRequestDto.Name;

                await _nyssContext.SaveChangesAsync();

                return SuccessMessage(ResultKey.NationalSociety.Organization.SuccessfullyUpdated);
            }
            catch (ResultException exception)
            {
                _loggerAdapter.Debug(exception);
                return exception.Result;
            }
        }

        public async Task<Result> Delete(int organizationId)
        {
            try
            {
                var gatewaySettingToDelete = await _nyssContext.Organizations.FindAsync(organizationId);

                if (gatewaySettingToDelete == null)
                {
                    return Error(ResultKey.NationalSociety.Organization.SettingDoesNotExist);
                }

                _nyssContext.Organizations.Remove(gatewaySettingToDelete);

                await _nyssContext.SaveChangesAsync();
                
                return SuccessMessage(ResultKey.NationalSociety.Organization.SuccessfullyDeleted);
            }
            catch (ResultException exception)
            {
                _loggerAdapter.Debug(exception);
                return exception.Result;
            }
        }
    }
}
