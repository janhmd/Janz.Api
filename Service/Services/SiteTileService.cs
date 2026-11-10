using Domain.Constants;
using Domain.Entities;
using FluentValidation;
using Repository.Repositories.Interfaces;
using Service.Helpers.DTOs.Tiles;
using Service.Helpers.Exceptions;
using Service.Helpers.Validators;
using Service.Services.Interfaces;

namespace Service.Services
{
    // Home və kataloq səhifələrindəki şəkilli kartlar. Admin ad, keçid, şəkil, sıra və görünüşü dəyişir
    public class SiteTileService : ISiteTileService
    {
        public const string ImageFolder = "tiles";

        private readonly IBaseRepository<SiteTile> _tileRepo;
        private readonly IFileService _fileService;
        private readonly IValidator<SaveSiteTileDto> _validator;

        public SiteTileService(IBaseRepository<SiteTile> tileRepo, IFileService fileService, IValidator<SaveSiteTileDto> validator)
        {
            _tileRepo = tileRepo;
            _fileService = fileService;
            _validator = validator;
        }

        public async Task<IEnumerable<SiteTileDto>> GetVisibleAsync(string placement)
        {
            var tiles = (await _tileRepo.GetAllAsync()).Where(m => m.IsVisible && m.Placement == placement);

            return tiles.OrderBy(m => m.SortOrder).ThenBy(m => m.Id).Select(ToDto).ToList();
        }

        public async Task<IEnumerable<SiteTileDto>> GetAllAsync()
        {
            var tiles = await _tileRepo.GetAllAsync();

            return tiles
                .OrderBy(m => Array.IndexOf(TilePlacements.All, m.Placement))
                .ThenBy(m => m.SortOrder).ThenBy(m => m.Id)
                .Select(ToDto).ToList();
        }

        public async Task<SiteTileDto> GetByIdAsync(int id)
        {
            var tile = await _tileRepo.GetByIdAsync(id);
            if (tile is null)
            {
                throw new NotFoundException();
            }

            return ToDto(tile);
        }

        public async Task<SiteTileDto> CreateAsync(SaveSiteTileDto model)
        {
            await _validator.EnsureValidAsync(model);
            if (model.Image is null || model.Image.Length == 0)
            {
                throw new BadRequestException("Choose an image for the tile.");
            }

            var tile = new SiteTile
            {
                Placement = model.Placement!,
                Title = model.Title!.Trim(),
                Link = model.Link!.Trim(),
                ImagePath = await _fileService.UploadImageAsync(model.Image, ImageFolder),
                SortOrder = model.SortOrder,
                IsVisible = model.IsVisible
            };
            await _tileRepo.AddAsync(tile);

            return ToDto(tile);
        }

        public async Task UpdateAsync(int id, SaveSiteTileDto model)
        {
            await _validator.EnsureValidAsync(model);

            var tile = await _tileRepo.GetByIdAsync(id);
            if (tile is null)
            {
                throw new NotFoundException();
            }

            var oldImage = tile.ImagePath;
            var replaced = model.Image is not null && model.Image.Length > 0;
            if (replaced)
            {
                tile.ImagePath = await _fileService.UploadImageAsync(model.Image!, ImageFolder);
                tile.VideoPath = null;       // köhnə video yeni şəkilə uyğun gəlmir
            }

            tile.Placement = model.Placement!;
            tile.Title = model.Title!.Trim();
            tile.Link = model.Link!.Trim();
            tile.SortOrder = model.SortOrder;
            tile.IsVisible = model.IsVisible;
            await _tileRepo.UpdateAsync(tile);

            if (replaced)
            {
                await _fileService.DeleteFileAsync(oldImage);
            }
        }

        public async Task DeleteAsync(int id)
        {
            var tile = await _tileRepo.GetByIdAsync(id);
            if (tile is null)
            {
                throw new NotFoundException();
            }

            await _tileRepo.DeleteAsync(tile);
            await _fileService.DeleteFileAsync(tile.ImagePath);
        }

        private static SiteTileDto ToDto(SiteTile tile) => new()
        {
            Id = tile.Id,
            Placement = tile.Placement,
            Title = tile.Title,
            Link = tile.Link,
            ImagePath = tile.ImagePath,
            VideoPath = tile.VideoPath,
            SortOrder = tile.SortOrder,
            IsVisible = tile.IsVisible
        };
    }
}
