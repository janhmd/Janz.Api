using Domain.Entities;
using FluentValidation;
using Repository.Repositories.Interfaces;
using Service.Helpers.DTOs.Pages;
using Service.Helpers.Exceptions;
using Service.Helpers.Validators;
using Service.Services.Interfaces;

namespace Service.Services
{
    public class PageService : IPageService
    {
        private readonly IBaseRepository<SitePage> _pageRepo;
        private readonly IValidator<UpdateSitePageDto> _updateValidator;

        public PageService(IBaseRepository<SitePage> pageRepo, IValidator<UpdateSitePageDto> updateValidator)
        {
            _pageRepo = pageRepo;
            _updateValidator = updateValidator;
        }

        public async Task<IEnumerable<SitePageLinkDto>> GetLinksAsync()
        {
            var pages = await _pageRepo.GetAllAsync();

            return pages.OrderBy(m => m.Id).Select(m => new SitePageLinkDto { Slug = m.Slug, Title = m.Title, InTerms = m.InTerms, InFooter = m.InFooter });
        }

        public async Task<SitePageDto> GetBySlugAsync(string slug)
        {
            var page = (await _pageRepo.GetAllAsync()).FirstOrDefault(m => m.Slug == slug);
            if (page is null)
            {
                throw new NotFoundException();
            }

            return ToDto(page);
        }

        public async Task<IEnumerable<SitePageDto>> GetAllAsync()
        {
            var pages = await _pageRepo.GetAllAsync();

            return pages.OrderBy(m => m.Id).Select(ToDto);
        }

        public async Task<SitePageDto> GetByIdAsync(int id)
        {
            var page = await _pageRepo.GetByIdAsync(id);
            if (page is null)
            {
                throw new NotFoundException();
            }

            return ToDto(page);
        }

        public async Task UpdateAsync(int id, UpdateSitePageDto model)
        {
            await _updateValidator.EnsureValidAsync(model);

            var page = await _pageRepo.GetByIdAsync(id);
            if (page is null)
            {
                throw new NotFoundException();
            }

            page.Title = model.Title!.Trim();
            page.Body = model.Body!.Trim();
            page.InTerms = model.InTerms;
            page.InFooter = model.InFooter;
            page.UpdatedAt = DateTime.UtcNow;
            await _pageRepo.UpdateAsync(page);
        }

        private static SitePageDto ToDto(SitePage page) => new()
        {
            Id = page.Id,
            Slug = page.Slug,
            Title = page.Title,
            Body = page.Body,
            InTerms = page.InTerms,
            InFooter = page.InFooter,
            UpdatedAt = page.UpdatedAt
        };
    }
}
