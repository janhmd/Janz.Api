using Domain.Entities;
using FluentValidation;
using Repository.Repositories.Interfaces;
using Service.Helpers;
using Service.Helpers.DTOs.Categories;
using Service.Helpers.Exceptions;
using Service.Helpers.Validators;
using Service.Services.Interfaces;

namespace Service.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly IBaseRepository<Category> _categoryRepo;
        private readonly IBaseRepository<Product> _productRepo;
        private readonly IValidator<CreateCategoryDto> _createValidator;
        private readonly IValidator<UpdateCategoryDto> _updateValidator;

        public CategoryService(IBaseRepository<Category> categoryRepo,
                               IBaseRepository<Product> productRepo,
                               IValidator<CreateCategoryDto> createValidator,
                               IValidator<UpdateCategoryDto> updateValidator)
        {
            _categoryRepo = categoryRepo;
            _productRepo = productRepo;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
        }

        public async Task<IEnumerable<CategoryDto>> GetAllAsync()
        {
            var categories = await _categoryRepo.GetAllAsync();
            var counts = (await _productRepo.GetAllAsync()).GroupBy(m => m.CategoryId).ToDictionary(m => m.Key, m => m.Count());

            return categories.OrderBy(m => m.Name).Select(m => ToDto(m, counts.GetValueOrDefault(m.Id)));
        }

        public async Task<CategoryDto> GetByIdAsync(int id)
        {
            var category = await _categoryRepo.GetByIdAsync(id);
            if (category is null)
            {
                throw new NotFoundException();
            }

            var count = (await _productRepo.GetAllAsync()).Count(m => m.CategoryId == id);
            return ToDto(category, count);
        }

        public async Task<CategoryDto> CreateAsync(CreateCategoryDto model)
        {
            await _createValidator.EnsureValidAsync(model);

            var name = model.Name!.Trim();
            var slug = await MakeUniqueSlugAsync(model.Slug, name, null);

            var category = new Category
            {
                Name = name,
                Slug = slug
            };
            await _categoryRepo.AddAsync(category);

            return ToDto(category, 0);
        }

        public async Task UpdateAsync(int id, UpdateCategoryDto model)
        {
            await _updateValidator.EnsureValidAsync(model);

            var category = await _categoryRepo.GetByIdAsync(id);
            if (category is null)
            {
                throw new NotFoundException();
            }

            category.Name = model.Name!.Trim();
            category.Slug = await MakeUniqueSlugAsync(model.Slug, category.Name, id);
            await _categoryRepo.UpdateAsync(category);
        }

        public async Task DeleteAsync(int id)
        {
            var category = await _categoryRepo.GetByIdAsync(id);
            if (category is null)
            {
                throw new NotFoundException();
            }

            // Məhsulu olan kateqoriya silinmir (bazada da Restrict-dir): əvvəl məhsullar başqa kateqoriyaya keçirilməlidir
            var products = await _productRepo.GetAllAsync();
            if (products.Any(m => m.CategoryId == id))
            {
                throw new BadRequestException("This category has products. Move them to another category first.");
            }

            await _categoryRepo.DeleteAsync(category);
        }

        // Slug yazılmayıbsa addan düzəldilir. Başqa kateqoriyada eyni slug varsa xəta qaytarılır
        private async Task<string> MakeUniqueSlugAsync(string? slug, string name, int? currentId)
        {
            var result = string.IsNullOrWhiteSpace(slug) ? SlugHelper.Make(name) : slug.Trim();
            if (string.IsNullOrEmpty(result))
            {
                throw new BadRequestException("Write a slug with latin letters.");
            }

            var categories = await _categoryRepo.GetAllAsync();
            if (categories.Any(m => m.Id != currentId && m.Slug == result))
            {
                throw new BadRequestException("This slug is already used by another category.");
            }

            return result;
        }

        private static CategoryDto ToDto(Category category, int productCount)
        {
            return new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Slug = category.Slug,
                ProductCount = productCount
            };
        }
    }
}
