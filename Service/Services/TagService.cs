using Domain.Entities;
using FluentValidation;
using Repository.Repositories.Interfaces;
using Service.Helpers;
using Service.Helpers.DTOs.Tags;
using Service.Helpers.Exceptions;
using Service.Helpers.Validators;
using Service.Services.Interfaces;

namespace Service.Services
{
    public class TagService : ITagService
    {
        private readonly IBaseRepository<Tag> _tagRepo;
        private readonly IBaseRepository<ProductTag> _productTagRepo;
        private readonly IValidator<CreateTagDto> _createValidator;
        private readonly IValidator<UpdateTagDto> _updateValidator;

        public TagService(IBaseRepository<Tag> tagRepo,
                          IBaseRepository<ProductTag> productTagRepo,
                          IValidator<CreateTagDto> createValidator,
                          IValidator<UpdateTagDto> updateValidator)
        {
            _tagRepo = tagRepo;
            _productTagRepo = productTagRepo;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
        }

        public async Task<IEnumerable<TagDto>> GetAllAsync()
        {
            var tags = await _tagRepo.GetAllAsync();
            var counts = (await _productTagRepo.GetAllAsync()).GroupBy(m => m.TagId).ToDictionary(m => m.Key, m => m.Count());

            return tags.OrderBy(m => m.Name).Select(m => ToDto(m, counts.GetValueOrDefault(m.Id)));
        }

        public async Task<TagDto> GetByIdAsync(int id)
        {
            var tag = await _tagRepo.GetByIdAsync(id);
            if (tag is null)
            {
                throw new NotFoundException();
            }

            var count = (await _productTagRepo.GetAllAsync()).Count(m => m.TagId == id);
            return ToDto(tag, count);
        }

        public async Task<TagDto> CreateAsync(CreateTagDto model)
        {
            await _createValidator.EnsureValidAsync(model);

            var name = model.Name!.Trim();
            var slug = await MakeUniqueSlugAsync(model.Slug, name, null);

            var tag = new Tag
            {
                Name = name,
                Slug = slug
            };
            await _tagRepo.AddAsync(tag);

            return ToDto(tag, 0);
        }

        public async Task UpdateAsync(int id, UpdateTagDto model)
        {
            await _updateValidator.EnsureValidAsync(model);

            var tag = await _tagRepo.GetByIdAsync(id);
            if (tag is null)
            {
                throw new NotFoundException();
            }

            tag.Name = model.Name!.Trim();
            tag.Slug = await MakeUniqueSlugAsync(model.Slug, tag.Name, id);
            await _tagRepo.UpdateAsync(tag);
        }

        public async Task DeleteAsync(int id)
        {
            var tag = await _tagRepo.GetByIdAsync(id);
            if (tag is null)
            {
                throw new NotFoundException();
            }

            // Teq məhsullardan da çıxır (ProductTag cascade ilə silinir), məhsulların özünə toxunulmur
            await _tagRepo.DeleteAsync(tag);
        }

        // Slug yazılmayıbsa addan düzəldilir. Başqa teqdə eyni slug varsa xəta qaytarılır
        private async Task<string> MakeUniqueSlugAsync(string? slug, string name, int? currentId)
        {
            var result = string.IsNullOrWhiteSpace(slug) ? SlugHelper.Make(name) : slug.Trim();
            if (string.IsNullOrEmpty(result))
            {
                throw new BadRequestException("Write a slug with latin letters.");
            }

            var tags = await _tagRepo.GetAllAsync();
            if (tags.Any(m => m.Id != currentId && m.Slug == result))
            {
                throw new BadRequestException("This slug is already used by another tag.");
            }

            return result;
        }

        private static TagDto ToDto(Tag tag, int productCount)
        {
            return new TagDto
            {
                Id = tag.Id,
                Name = tag.Name,
                Slug = tag.Slug,
                ProductCount = productCount
            };
        }
    }
}
