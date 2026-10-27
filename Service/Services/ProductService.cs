using Domain.Constants;
using Domain.Entities;
using FluentValidation;
using Repository.Repositories.Interfaces;
using Service.Helpers;
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
        private readonly IBaseRepository<ProductVariant> _variantRepo;
        private readonly IBaseRepository<ProductVariantImage> _variantImageRepo;
        private readonly IBaseRepository<ProductReview> _reviewRepo;
        private readonly IBaseRepository<CartItem> _cartItemRepo;
        private readonly IFileService _fileService;
        private readonly IValidator<CreateProductDto> _createValidator;
        private readonly IValidator<UpdateProductDto> _updateValidator;

        public ProductService(IBaseRepository<Product> productRepo,
                              IBaseRepository<Category> categoryRepo,
                              IBaseRepository<Tag> tagRepo,
                              IBaseRepository<ProductTag> productTagRepo,
                              IBaseRepository<ProductImage> productImageRepo,
                              IBaseRepository<OrderItem> orderItemRepo,
                              IBaseRepository<ProductVariant> variantRepo,
                              IBaseRepository<ProductVariantImage> variantImageRepo,
                              IBaseRepository<CartItem> cartItemRepo,
                              IBaseRepository<ProductReview> reviewRepo,
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
            _variantRepo = variantRepo;
            _variantImageRepo = variantImageRepo;
            _reviewRepo = reviewRepo;
            _cartItemRepo = cartItemRepo;
            _fileService = fileService;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
        }

        // Sayt yalnız görünən məhsulları alır, admin panel hamısını (includeHidden)
        public async Task<IEnumerable<ProductDto>> GetAllAsync(bool includeHidden = false)
        {
            var products = await _productRepo.GetAllAsync();
            return await MapProductsAsync(products.Where(m => includeHidden || m.IsVisible));
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
            return await MapProductsAsync(products.Where(m => m.CategoryId == category.Id && m.IsVisible));
        }

        // Saytdakı kataloq: yalnız görünən məhsullar, filtrlər ProductCatalogFilter-dadır
        public async Task<IEnumerable<ProductDto>> SearchAsync(ProductFilterDto filter)
        {
            var products = (await _productRepo.GetAllAsync()).Where(m => m.IsVisible);
            return ProductCatalogFilter.Apply(await MapProductsAsync(products), filter);
        }

        public async Task<ProductDto> GetByIdAsync(int id, bool includeHidden = false)
        {
            var product = await _productRepo.GetByIdAsync(id);
            if (product is null || (!product.IsVisible && !includeHidden))
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

                var galleryPaths = uploaded.Skip(1).ToList();
                var variantPaths = await UploadVariantImagesAsync(model.Variants, uploaded);

                var product = new Product
                {
                    Name = model.Name!.Trim(),
                    Description = model.Description!.Trim(),
                    Price = model.Price,
                    Stock = model.Stock,
                    CategoryId = model.CategoryId,
                    ImagePath = imagePath,
                    OldPrice = model.OldPrice,
                    Badge = Clean(model.Badge),
                    IsVisible = model.IsVisible,
                    Roast = Clean(model.Roast),
                    Region = Clean(model.Region),
                    Processing = Clean(model.Processing),
                    Caffeine = Clean(model.Caffeine),
                    Brewing = Clean(model.Brewing),
                    Storage = Clean(model.Storage),
                    Use = Clean(model.Use),
                    Taste = Clean(model.Taste),
                    Notes = CleanNotes(model.Notes),
                    Density = model.Density,
                    Acidity = model.Acidity,
                    TasteGroup = Clean(model.TasteGroup),
                    BrewMethods = JoinBrewMethods(model.BrewMethods)
                };
                ApplySummary(product, model.Variants);
                await _productRepo.AddAsync(product);

                await AddGalleryAsync(product.Id, galleryPaths, 0);
                await SetTagsAsync(product.Id, model.TagIds);
                await SaveVariantsAsync(product.Id, model.Variants, variantPaths);

                return await GetByIdAsync(product.Id, includeHidden: true);
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
            var newVariantFiles = new List<string>();
            var removedVariantFiles = new List<string>();
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

                var variantPaths = await UploadVariantImagesAsync(model.Variants, newVariantFiles);

                product.Name = model.Name!.Trim();
                product.Description = model.Description!.Trim();
                product.Price = model.Price;
                product.Stock = model.Stock;
                product.CategoryId = model.CategoryId;
                product.OldPrice = model.OldPrice;
                product.Badge = Clean(model.Badge);
                product.IsVisible = model.IsVisible;
                product.Roast = Clean(model.Roast);
                product.Region = Clean(model.Region);
                product.Processing = Clean(model.Processing);
                product.Caffeine = Clean(model.Caffeine);
                product.Brewing = Clean(model.Brewing);
                product.Storage = Clean(model.Storage);
                product.Use = Clean(model.Use);
                product.Taste = Clean(model.Taste);
                product.Notes = CleanNotes(model.Notes);
                product.Density = model.Density;
                product.Acidity = model.Acidity;
                product.TasteGroup = Clean(model.TasteGroup);
                product.BrewMethods = JoinBrewMethods(model.BrewMethods);
                ApplySummary(product, model.Variants);
                if (newImage is not null)
                {
                    product.ImagePath = newImage;
                }
                await _productRepo.UpdateAsync(product);

                var nextOrder = gallery.Count == 0 ? 0 : gallery.Max(m => m.Order) + 1;
                await AddGalleryAsync(id, newGallery, nextOrder);
                await SetTagsAsync(id, model.TagIds);
                removedVariantFiles = await SaveVariantsAsync(id, model.Variants, variantPaths);
            }
            catch
            {
                if (newImage is not null)
                {
                    await _fileService.DeleteFileAsync(newImage);
                }
                foreach (var path in newGallery.Concat(newVariantFiles))
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

            // Variantlardan çıxarılan şəkillərin faylları
            foreach (var path in removedVariantFiles)
            {
                await _fileService.DeleteFileAsync(path);
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

            var variantIds = (await _variantRepo.GetAllAsync()).Where(m => m.ProductId == id).Select(m => m.Id).ToHashSet();
            var images = (await _productImageRepo.GetAllAsync())
                .Where(m => m.ProductId == id)
                .Select(m => m.ImagePath)
                .Concat((await _variantImageRepo.GetAllAsync()).Where(m => variantIds.Contains(m.VariantId)).Select(m => m.ImagePath))
                .Append(product.ImagePath)
                .ToList();

            // Səbət sətirləri variantla da bağlıdır (NoAction): məhsuldan əvvəl silinir
            foreach (var item in (await _cartItemRepo.GetAllAsync()).Where(m => m.ProductId == id))
            {
                await _cartItemRepo.DeleteAsync(item);
            }

            // Əvvəl baza (qalereya, teqlər və variantlar cascade ilə silinir), sonra fayllar
            await _productRepo.DeleteAsync(product);
            foreach (var path in images)
            {
                await _fileService.DeleteFileAsync(path);
            }
        }

        // Variant varsa kartda ən ucuz variantın qiyməti görünür, stok variantların cəmidir
        private static void ApplySummary(Product product, List<ProductVariantFormDto> variants)
        {
            if (variants.Count == 0)
            {
                return;
            }

            var cheapest = variants.OrderBy(m => m.Price).First();
            product.Price = cheapest.Price;
            product.OldPrice = cheapest.OldPrice;
            product.Stock = variants.Sum(m => m.Stock);
        }

        // Variantların yeni şəkilləri bazaya yazmazdan əvvəl yüklənir; uploaded siyahısı xəta olanda silmək üçündür
        private async Task<List<List<string>>> UploadVariantImagesAsync(List<ProductVariantFormDto> variants, List<string> uploaded)
        {
            var result = new List<List<string>>();
            foreach (var variant in variants)
            {
                var paths = new List<string>();
                foreach (var file in variant.NewImages.Where(m => m.Length > 0))
                {
                    var path = await _fileService.UploadImageAsync(file, ProductRules.ImageFolder);
                    uploaded.Add(path);
                    paths.Add(path);
                }
                result.Add(paths);
            }
            return result;
        }

        // Formdakı variantlar bazaya yazılır: Id-si olanlar dəyişir, yenilər əlavə olunur, formada olmayanlar silinir.
        // Qaytarılan siyahı: artıq lazım olmayan şəkil faylları (baza yazılandan sonra silinir)
        private async Task<List<string>> SaveVariantsAsync(int productId, List<ProductVariantFormDto> forms, List<List<string>> newPaths)
        {
            var existing = (await _variantRepo.GetAllAsync()).Where(m => m.ProductId == productId).ToList();
            var existingIds = existing.Select(m => m.Id).ToHashSet();
            var images = (await _variantImageRepo.GetAllAsync()).Where(m => existingIds.Contains(m.VariantId)).ToList();

            var filesToDelete = new List<string>();
            var keptIds = new HashSet<int>();

            for (var i = 0; i < forms.Count; i++)
            {
                var form = forms[i];
                // Başqa məhsulun variant Id-si göndərilsə yeni variant kimi qəbul olunur
                var variant = form.Id is int formId ? existing.FirstOrDefault(m => m.Id == formId) : null;
                var isNew = variant is null;
                variant ??= new ProductVariant { ProductId = productId };

                variant.Format = form.Format!.Trim();
                variant.Size = Clean(form.Size);
                variant.Price = form.Price;
                variant.OldPrice = form.OldPrice;
                variant.Stock = form.Stock;
                variant.Order = i;

                if (isNew)
                {
                    await _variantRepo.AddAsync(variant);
                }
                else
                {
                    await _variantRepo.UpdateAsync(variant);
                    keptIds.Add(variant.Id);
                }

                // Saxlanılan şəkillər yalnız bu variantın öz şəkilləri arasından seçilir, göndərilən sıra ilə
                var current = images.Where(m => m.VariantId == variant.Id).ToList();
                var keep = form.KeepImages.Distinct().Where(path => current.Any(m => m.ImagePath == path)).ToList();

                foreach (var image in current.Where(m => !keep.Contains(m.ImagePath)))
                {
                    await _variantImageRepo.DeleteAsync(image);
                    filesToDelete.Add(image.ImagePath);
                }

                var order = 0;
                foreach (var path in keep)
                {
                    var image = current.First(m => m.ImagePath == path);
                    image.Order = order++;
                    await _variantImageRepo.UpdateAsync(image);
                }

                foreach (var path in newPaths[i])
                {
                    await _variantImageRepo.AddAsync(new ProductVariantImage { VariantId = variant.Id, ImagePath = path, Order = order++ });
                }
            }

            foreach (var variant in existing.Where(m => !keptIds.Contains(m.Id)))
            {
                await DetachVariantAsync(variant.Id);
                filesToDelete.AddRange(images.Where(m => m.VariantId == variant.Id).Select(m => m.ImagePath));
                await _variantRepo.DeleteAsync(variant);
            }

            return filesToDelete;
        }

        // Silinən variant: səbətdəki sətirləri silinir, sifarişlərdə isə yazısı qalır, əlaqə boşalır
        private async Task DetachVariantAsync(int variantId)
        {
            foreach (var item in (await _cartItemRepo.GetAllAsync()).Where(m => m.VariantId == variantId))
            {
                await _cartItemRepo.DeleteAsync(item);
            }

            foreach (var item in (await _orderItemRepo.GetAllAsync()).Where(m => m.VariantId == variantId))
            {
                item.VariantId = null;
                await _orderItemRepo.UpdateAsync(item);
            }
        }

        // Notlar vergüllə saxlanılır: boşluqlar və boş notlar atılır
        private static string? CleanNotes(string? value)
        {
            var notes = (value ?? string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            return notes.Length == 0 ? null : string.Join(", ", notes);
        }

        // Üsullar siyahıdakı sıra ilə saxlanılır ki, səhifədə həmişə eyni sıra ilə görünsün
        private static string? JoinBrewMethods(List<string> values)
        {
            var methods = BrewMethods.All.Where(m => values.Contains(m)).ToList();
            return methods.Count == 0 ? null : string.Join(",", methods);
        }

        // Boş mətn null kimi saxlanılır ki, məhsul səhifəsi boş blok göstərməsin
        private static string? Clean(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
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
            var variants = (await _variantRepo.GetAllAsync()).ToLookup(m => m.ProductId);
            var variantImages = (await _variantImageRepo.GetAllAsync()).ToLookup(m => m.VariantId);
            var reviews = (await _reviewRepo.GetAllAsync()).Where(m => !m.IsHidden).ToLookup(m => m.ProductId);

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

                var dto = ToDto(product, category, productTagList, images[product.Id]);
                dto.Variants = variants[product.Id]
                    .OrderBy(m => m.Order)
                    .Select(m => ToDto(m, variantImages[m.Id]))
                    .ToList();
                var productReviews = reviews[product.Id].ToList();
                dto.ReviewCount = productReviews.Count;
                dto.Rating = productReviews.Count == 0 ? null : Math.Round((decimal)productReviews.Average(m => m.Rating), 1);
                result.Add(dto);
            }

            return result;
        }

        private static ProductVariantDto ToDto(ProductVariant variant, IEnumerable<ProductVariantImage> images)
        {
            return new ProductVariantDto
            {
                Id = variant.Id,
                Format = variant.Format,
                Size = variant.Size,
                Label = variant.Label,
                Price = variant.Price,
                OldPrice = variant.OldPrice,
                Stock = variant.Stock,
                Images = images.OrderBy(m => m.Order).Select(m => m.ImagePath).ToList()
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
            dto.OldPrice = product.OldPrice;
            dto.Badge = product.Badge;
            dto.IsVisible = product.IsVisible;
            dto.Roast = product.Roast;
            dto.Region = product.Region;
            dto.Processing = product.Processing;
            dto.Caffeine = product.Caffeine;
            dto.Brewing = product.Brewing;
            dto.Storage = product.Storage;
            dto.BrewMethods = string.IsNullOrEmpty(product.BrewMethods)
                ? new List<string>()
                : product.BrewMethods.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();
            dto.CreatedAt = product.CreatedAt;
            dto.Use = product.Use;
            dto.Taste = product.Taste;
            dto.Notes = string.IsNullOrEmpty(product.Notes)
                ? new List<string>()
                : product.Notes.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
            dto.Density = product.Density;
            dto.Acidity = product.Acidity;
            dto.TasteGroup = product.TasteGroup;
            dto.CategoryId = category.Id;
            dto.CategoryName = category.Name;
            dto.CategorySlug = category.Slug;
            dto.Images = images.OrderBy(m => m.Order).Select(m => m.ImagePath).ToList();
            dto.Tags = tags.Select(ToDto).ToList();
        }
    }
}
