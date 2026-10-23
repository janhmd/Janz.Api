using Domain.Entities;
using FluentValidation;
using Repository.Repositories.Interfaces;
using Service.Helpers;
using Service.Helpers.DTOs.Categories;
using Service.Helpers.DTOs.Products;
using Service.Helpers.DTOs.Tags;
using Service.Helpers.Exceptions;
using Service.Helpers.Validators;
using Service.Services.Interfaces;

namespace Service.Services
{
    public class ProductService : IProductService
    {
        private readonly IBaseRepository<Product> _productRepo;
        private readonly IBaseRepository<Category> _categoryRepo;
        private readonly IBaseRepository<Tag> _tagRepo;
        private readonly IBaseRepository<ProductTag> _productTagRepo;
        private readonly IBaseRepository<ProductImage> _productImageRepo;
        private readonly IBaseRepository<OrderItem> _orderItemRepo;
        private readonly IFileService _fileService;
        private readonly IValidator<CreateProductDto> _createValidator;
        private readonly IValidator<UpdateProductDto> _updateValidator;

        public ProductService(IBaseRepository<Product> productRepo,
                              IBaseRepository<Category> categoryRepo,
                              IBaseRepository<Tag> tagRepo,
                              IBaseRepository<ProductTag> productTagRepo,
                              IBaseRepository<ProductImage> productImageRepo,
                              IBaseRepository<OrderItem> orderItemRepo,
                              IFileService fileService,
                              IValidator<CreateProductDto> createValidator,
                              IValidator<UpdateProductDto> updateValidator)
        {
            _productRepo = productRepo;
            _categoryRepo = categoryRepo;
            _tagRepo = tagRepo;
            _productTagRepo = productTagRepo;
            _productImageRepo = productImageRepo;
            _orderItemRepo = orderItemRepo;
            _fileService = fileService;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
        }

        public async Task<IEnumerable<CategoryDto>> GetCategoriesAsync()
        {
            var categories = await _categoryRepo.GetAllAsync();
            return categories.Select(ToDto);
        }

        public async Task<IEnumerable<ProductDto>> GetAllAsync()
        {
            var products = await _productRepo.GetAllAsync();
            return await MapProductsAsync(products);
        }

        public async Task<IEnumerable<ProductDto>> GetByCategorySlugAsync(string slug)
        {
            var categories = await _categoryRepo.GetAllAsync();
            var category = categories.FirstOrDefault(m =>
                m.Slug.Equals(slug.Trim(), StringComparison.OrdinalIgnoreCase));

            if (category is null)
            {
                return new List<ProductDto>();
            }

            var products = await _productRepo.GetAllAsync();
            return await MapProductsAsync(products.Where(m => m.CategoryId == category.Id));
        }

        public async Task<ProductDto> GetByIdAsync(int id)
        {
            var product = await _productRepo.GetByIdAsync(id);
            if (product is null)
            {
                throw new NotFoundException();
            }

            var result = await MapProductsAsync(new List<Product> { product });
            var dto = result.FirstOrDefault();
            if (dto is null)
            {
                throw new NotFoundException();
            }

            return dto;
        }

        public async Task<ProductDto> CreateAsync(CreateProductDto model)
        {
            // Ad, təsvir, qiymət, stok və kateqoriya CreateProductDtoValidator-da yoxlanılır
            await _createValidator.EnsureValidAsync(model);
            await EnsureCategoryAndTagsAsync(model.CategoryId, model.TagIds);

            // Əvvəl şəkillər yüklənir: yanlış fayl olsa bazaya heç nə yazılmır
            var uploaded = new List<string>();
            try
            {
                var imagePath = await _fileService.UploadImageAsync(model.Image!, ProductRules.ImageFolder);
                uploaded.Add(imagePath);

                foreach (var file in model.Gallery)
                {
                    uploaded.Add(await _fileService.UploadImageAsync(file, ProductRules.ImageFolder));
                }

                var product = new Product
                {
                    Name = model.Name!.Trim(),
                    Description = model.Description!.Trim(),
                    Price = model.Price,
                    Stock = model.Stock,
                    CategoryId = model.CategoryId,
                    ImagePath = imagePath
                };
                await _productRepo.AddAsync(product);

                await AddGalleryAsync(product.Id, uploaded.Skip(1), 0);
                await SetTagsAsync(product.Id, model.TagIds);

                return await GetByIdAsync(product.Id);
            }
            catch
            {
                // Bazaya yazılmadısa yüklənmiş fayllar yetim qalmasın
                foreach (var path in uploaded)
                {
                    await _fileService.DeleteFileAsync(path);
                }
                throw;
            }
        }

        public async Task UpdateAsync(int id, UpdateProductDto model)
        {
            await _updateValidator.EnsureValidAsync(model);

            var product = await _productRepo.GetByIdAsync(id);
            if (product is null)
            {
                throw new NotFoundException();
            }

            await EnsureCategoryAndTagsAsync(model.CategoryId, model.TagIds);

            // Silinəcək şəkillər yalnız bu məhsulun bazadakı şəkilləri arasından seçilir (göndərilən yola etibar edilmir)
            var gallery = (await _productImageRepo.GetAllAsync()).Where(m => m.ProductId == id).ToList();
            var toRemove = gallery.Where(m => model.RemoveImages.Contains(m.ImagePath)).ToList();

            if (gallery.Count - toRemove.Count + model.Gallery.Count > ProductRules.MaxGallery)
            {
                throw new BadRequestException($"The gallery can have at most {ProductRules.MaxGallery} images.");
            }

            // Yeni şəkillər əvvəl yüklənir ki, yanlış fayl olsa köhnələr itməsin
            var oldImage = product.ImagePath;
            string? newImage = null;
            var newGallery = new List<string>();
            try
            {
                if (model.Image is not null && model.Image.Length > 0)
                {
                    newImage = await _fileService.UploadImageAsync(model.Image, ProductRules.ImageFolder);
                }

                foreach (var file in model.Gallery)
                {
                    newGallery.Add(await _fileService.UploadImageAsync(file, ProductRules.ImageFolder));
                }

                product.Name = model.Name!.Trim();
                product.Description = model.Description!.Trim();
                product.Price = model.Price;
                product.Stock = model.Stock;
                product.CategoryId = model.CategoryId;
                if (newImage is not null)
                {
                    product.ImagePath = newImage;
                }
                await _productRepo.UpdateAsync(product);

                var nextOrder = gallery.Count == 0 ? 0 : gallery.Max(m => m.Order) + 1;
                await AddGalleryAsync(id, newGallery, nextOrder);
                await SetTagsAsync(id, model.TagIds);
            }
            catch
            {
                if (newImage is not null)
                {
                    await _fileService.DeleteFileAsync(newImage);
                }
                foreach (var path in newGallery)
                {
                    await _fileService.DeleteFileAsync(path);
                }
                throw;
            }

            // Hər şey yazıldı: köhnə əsas şəkil və silinən qalereya şəkilləri silinir
            foreach (var image in toRemove)
            {
                await _productImageRepo.DeleteAsync(image);
                await _fileService.DeleteFileAsync(image.ImagePath);
            }

            if (newImage is not null)
            {
                await _fileService.DeleteFileAsync(oldImage);
            }
        }

        public async Task DeleteAsync(int id)
        {
            var product = await _productRepo.GetByIdAsync(id);
            if (product is null)
            {
                throw new NotFoundException();
            }

            // Sifarişlərdə olan məhsul silinmir: sifariş tarixçəsi pozulmasın. Onun yerinə stoku 0 etmək olar
            var orderItems = await _orderItemRepo.GetAllAsync();
            if (orderItems.Any(m => m.ProductId == id))
            {
                throw new BadRequestException("This product is in orders, so it can't be deleted. Set its stock to 0 instead.");
            }

            var images = (await _productImageRepo.GetAllAsync())
                .Where(m => m.ProductId == id)
                .Select(m => m.ImagePath)
                .Append(product.ImagePath)
                .ToList();

            // Əvvəl baza (qalereya, teqlər və səbət sətirləri cascade ilə silinir), sonra fayllar
            await _productRepo.DeleteAsync(product);
            foreach (var path in images)
            {
                await _fileService.DeleteFileAsync(path);
            }
        }

        private async Task EnsureCategoryAndTagsAsync(int categoryId, List<int> tagIds)
        {
            if (await _categoryRepo.GetByIdAsync(categoryId) is null)
            {
                throw new BadRequestException("The category was not found.");
            }

            var tags = (await _tagRepo.GetAllAsync()).Select(m => m.Id).ToHashSet();
            if (tagIds.Any(m => !tags.Contains(m)))
            {
                throw new BadRequestException("One of the tags was not found.");
            }
        }

        private async Task AddGalleryAsync(int productId, IEnumerable<string> paths, int firstOrder)
        {
            var order = firstOrder;
            foreach (var path in paths)
            {
                await _productImageRepo.AddAsync(new ProductImage
                {
                    ProductId = productId,
                    ImagePath = path,
                    Order = order++
                });
            }
        }

        // Məhsulun teqləri göndərilən siyahı ilə eyni edilir: artıq olanlar silinir, yeniləri əlavə olunur
        private async Task SetTagsAsync(int productId, List<int> tagIds)
        {
            var wanted = tagIds.Distinct().ToList();
            var current = (await _productTagRepo.GetAllAsync()).Where(m => m.ProductId == productId).ToList();

            foreach (var link in current.Where(m => !wanted.Contains(m.TagId)))
            {
                await _productTagRepo.DeleteAsync(link);
            }

            foreach (var tagId in wanted.Where(m => current.All(c => c.TagId != m)))
            {
                await _productTagRepo.AddAsync(new ProductTag { ProductId = productId, TagId = tagId });
            }
        }

        private async Task<List<ProductDto>> MapProductsAsync(IEnumerable<Product> products)
        {
            var categories = (await _categoryRepo.GetAllAsync()).ToDictionary(m => m.Id);
            var tags = (await _tagRepo.GetAllAsync()).ToDictionary(m => m.Id);
            var productTags = (await _productTagRepo.GetAllAsync()).ToLookup(m => m.ProductId);
            var images = (await _productImageRepo.GetAllAsync()).ToLookup(m => m.ProductId);

            var result = new List<ProductDto>();
            foreach (var product in products)
            {
                if (!categories.TryGetValue(product.CategoryId, out var category))
                {
                    continue;
                }

                var productTagList = productTags[product.Id]
                    .Where(m => tags.ContainsKey(m.TagId))
                    .Select(m => tags[m.TagId]);

                result.Add(ToDto(product, category, productTagList, images[product.Id]));
            }

            return result;
        }

        private static CategoryDto ToDto(Category category)
        {
            return new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Slug = category.Slug
            };
        }

        private static TagDto ToDto(Tag tag)
        {
            return new TagDto
            {
                Id = tag.Id,
                Name = tag.Name,
                Slug = tag.Slug
            };
        }

        private static ProductDto ToDto(Product product, Category category, IEnumerable<Tag> tags, IEnumerable<ProductImage> images)
        {
            var dto = new ProductDto();
            Fill(dto, product, category, tags, images);
            return dto;
        }

        private static void Fill(ProductDto dto, Product product, Category category, IEnumerable<Tag> tags, IEnumerable<ProductImage> images)
        {
            dto.Id = product.Id;
            dto.Name = product.Name;
            dto.Description = product.Description;
            dto.Price = product.Price;
            dto.Stock = product.Stock;
            dto.ImagePath = product.ImagePath;
            dto.CategoryId = category.Id;
            dto.CategoryName = category.Name;
            dto.CategorySlug = category.Slug;
            dto.Images = images.OrderBy(m => m.Order).Select(m => m.ImagePath).ToList();
            dto.Tags = tags.Select(ToDto).ToList();
        }
    }
}
