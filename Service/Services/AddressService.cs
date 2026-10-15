using Domain.Entities;
using FluentValidation;
using Repository.Repositories.Interfaces;
using Service.Helpers.DTOs.Addresses;
using Service.Helpers.Responses;
using Service.Services.Interfaces;

namespace Service.Services
{
    public class AddressService : IAddressService
    {
        private readonly IBaseRepository<Address> _addressRepo;
        private readonly IValidator<CreateAddressDto> _createValidator;
        private readonly IValidator<UpdateAddressDto> _updateValidator;

        public AddressService(IBaseRepository<Address> addressRepo,
                              IValidator<CreateAddressDto> createValidator,
                              IValidator<UpdateAddressDto> updateValidator)
        {
            _addressRepo = addressRepo;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
        }

        public async Task<IEnumerable<AddressDto>> GetAllAsync(string userId)
        {
            var addresses = (await _addressRepo.GetAllAsync()).Where(m => m.UserId == userId);
            return addresses.Select(ToDto);
        }

        public async Task<ServiceResult<AddressDto>> GetByIdAsync(string userId, int id)
        {
            var address = await _addressRepo.GetByIdAsync(id);
            if (address is null || address.UserId != userId)
            {
                return ServiceResult<AddressDto>.NotFound();
            }

            return ServiceResult<AddressDto>.Ok(ToDto(address));
        }

        public async Task<ServiceResult<AddressDto>> CreateAsync(string userId, CreateAddressDto model)
        {
            var validation = await _createValidator.ValidateAsync(model);
            if (!validation.IsValid)
            {
                return ServiceResult<AddressDto>.Fail(validation.Errors.Select(e => e.ErrorMessage).ToArray());
            }

            var address = new Address { UserId = userId };
            Fill(address, model);
            await _addressRepo.AddAsync(address);

            return ServiceResult<AddressDto>.Ok(ToDto(address));
        }

        public async Task<ServiceResult<AddressDto>> UpdateAsync(string userId, int id, UpdateAddressDto model)
        {
            var validation = await _updateValidator.ValidateAsync(model);
            if (!validation.IsValid)
            {
                return ServiceResult<AddressDto>.Fail(validation.Errors.Select(e => e.ErrorMessage).ToArray());
            }

            var address = await _addressRepo.GetByIdAsync(id);
            if (address is null || address.UserId != userId)
            {
                return ServiceResult<AddressDto>.NotFound();
            }

            Fill(address, model);
            await _addressRepo.UpdateAsync(address);

            return ServiceResult<AddressDto>.Ok(ToDto(address));
        }

        public async Task<ServiceResult<bool>> DeleteAsync(string userId, int id)
        {
            var address = await _addressRepo.GetByIdAsync(id);
            if (address is null || address.UserId != userId)
            {
                return ServiceResult<bool>.NotFound();
            }

            await _addressRepo.DeleteAsync(address);
            return ServiceResult<bool>.Ok(true);
        }

        private static AddressDto ToDto(Address address)
        {
            var dto = new AddressDto();
            Fill(dto, address);
            return dto;
        }

        private static void Fill(AddressDto dto, Address address)
        {
            dto.Id = address.Id;
            dto.FullName = address.FullName;
            dto.Phone = address.Phone;
            dto.Line = address.Line;
            dto.City = address.City;
            dto.PostalCode = address.PostalCode;
        }

        private static void Fill(Address address, CreateAddressDto model)
        {
            address.FullName = model.FullName.Trim();
            address.Phone = model.Phone.Trim();
            address.Line = model.Line.Trim();
            address.City = model.City.Trim();
            address.PostalCode = model.PostalCode.Trim();
        }

        private static void Fill(Address address, UpdateAddressDto model)
        {
            address.FullName = model.FullName.Trim();
            address.Phone = model.Phone.Trim();
            address.Line = model.Line.Trim();
            address.City = model.City.Trim();
            address.PostalCode = model.PostalCode.Trim();
        }
    }
}
