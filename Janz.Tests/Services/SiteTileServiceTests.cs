using Domain.Entities;
using Janz.Tests.Fakes;
using Microsoft.AspNetCore.Http;
using Service.Helpers.DTOs.Tiles;
using Service.Helpers.Exceptions;
using Service.Helpers.Validators.Tiles;
using Service.Services;
using Xunit;

namespace Janz.Tests.Services
{
    public class SiteTileServiceTests
    {
        private readonly FakeRepository<SiteTile> _repo = new();
        private readonly FakeFileService _files = new();

        private SiteTileService Create() => new(_repo, _files, new SaveSiteTileDtoValidator());

        private static IFormFile Image() => new FormFile(new MemoryStream(new byte[] { 1, 2, 3 }), 0, 3, "Image", "tile.png");

        private SiteTile Add(string placement, string title, int order, bool visible = true, string image = "/images/tiles/a.jpg", string? video = "/videos/a.mp4")
        {
            var tile = new SiteTile { Placement = placement, Title = title, Link = "/Products", SortOrder = order, IsVisible = visible, ImagePath = image, VideoPath = video };
            _repo.AddAsync(tile).Wait();
            return tile;
        }

        private static SaveSiteTileDto Valid() => new()
        {
            Placement = "tea",
            Title = "  Matcha  ",
            Link = " ?tag=matcha ",
            SortOrder = 6,
            IsVisible = true
        };

        [Fact]
        public async Task GetVisible_ReturnsOnlyVisibleTilesOfThePlacement_InOrder()
        {
            Add("tea", "Black", 2);
            Add("tea", "Green", 1);
            Add("tea", "Hidden", 3, visible: false);
            Add("home", "Coffee", 1);

            var tiles = (await Create().GetVisibleAsync("tea")).ToList();

            Assert.Equal(new[] { "Green", "Black" }, tiles.Select(m => m.Title));
        }

        [Fact]
        public async Task GetAll_GroupsByPlacementOrder_ThenSortOrder()
        {
            Add("tea", "Green", 1);
            Add("home", "Coffee", 2);
            Add("home", "Tea", 1);

            var tiles = (await Create().GetAllAsync()).ToList();

            Assert.Equal(new[] { "Tea", "Coffee", "Green" }, tiles.Select(m => m.Title));
        }

        [Fact]
        public async Task Create_SavesTrimmedTile_WithUploadedImage()
        {
            var dto = Valid();
            dto.Image = Image();

            var created = await Create().CreateAsync(dto);

            var tile = Assert.Single(_repo.Rows);
            Assert.Equal("Matcha", tile.Title);
            Assert.Equal("?tag=matcha", tile.Link);
            Assert.Equal("/uploads/tiles/new-1.png", tile.ImagePath);
            Assert.Null(tile.VideoPath);
            Assert.Equal(tile.Id, created.Id);
        }

        [Fact]
        public async Task Create_Throws_WithoutImage()
        {
            await Assert.ThrowsAsync<BadRequestException>(() => Create().CreateAsync(Valid()));
            Assert.Empty(_repo.Rows);
        }

        [Theory]
        [InlineData("nowhere", "Title", "/Products")]
        [InlineData("tea", "  ", "/Products")]
        [InlineData("tea", "Title", "")]
        [InlineData("tea", "Title", "https://evil.example/x")]
        [InlineData("tea", "Title", "//evil.example/x")]
        [InlineData("tea", "Title", "javascript:alert(1)")]
        [InlineData("tea", "Title", "/Products /x")]
        public async Task Create_RejectsInvalidInput(string placement, string title, string link)
        {
            var dto = new SaveSiteTileDto { Placement = placement, Title = title, Link = link, Image = Image() };

            await Assert.ThrowsAsync<BadRequestException>(() => Create().CreateAsync(dto));
            Assert.Empty(_repo.Rows);
            Assert.Empty(_files.Uploaded);
        }

        [Fact]
        public async Task Create_RejectsTooLongTitle_AndBadOrder()
        {
            var longTitle = Valid();
            longTitle.Title = new string('a', 41);
            longTitle.Image = Image();
            var badOrder = Valid();
            badOrder.SortOrder = 1000;
            badOrder.Image = Image();

            await Assert.ThrowsAsync<BadRequestException>(() => Create().CreateAsync(longTitle));
            await Assert.ThrowsAsync<BadRequestException>(() => Create().CreateAsync(badOrder));
        }

        [Fact]
        public async Task Update_ChangesFields_AndKeepsImageAndVideo_WhenNoNewImage()
        {
            var tile = Add("tea", "Green", 1);

            await Create().UpdateAsync(tile.Id, new SaveSiteTileDto { Placement = "sets", Title = "Gift", Link = "/Products/Sets", SortOrder = 4, IsVisible = false });

            Assert.Equal("sets", tile.Placement);
            Assert.Equal("Gift", tile.Title);
            Assert.False(tile.IsVisible);
            Assert.Equal("/images/tiles/a.jpg", tile.ImagePath);
            Assert.Equal("/videos/a.mp4", tile.VideoPath);
            Assert.Empty(_files.Deleted);
        }

        [Fact]
        public async Task Update_WithNewImage_ReplacesImage_ClearsVideo_AndDeletesOldFile()
        {
            var tile = Add("tea", "Green", 1, image: "/uploads/tiles/old.png");

            await Create().UpdateAsync(tile.Id, new SaveSiteTileDto { Placement = "tea", Title = "Green", Link = "?tag=green-tea", Image = Image() });

            Assert.Equal("/uploads/tiles/new-1.png", tile.ImagePath);
            Assert.Null(tile.VideoPath);
            Assert.Equal(new[] { "/uploads/tiles/old.png" }, _files.Deleted);
        }

        [Fact]
        public async Task Update_Throws_ForUnknownTile_OrInvalidInput()
        {
            var tile = Add("tea", "Green", 1);

            await Assert.ThrowsAsync<NotFoundException>(() => Create().UpdateAsync(99, new SaveSiteTileDto { Placement = "tea", Title = "A", Link = "/x" }));
            await Assert.ThrowsAsync<BadRequestException>(() => Create().UpdateAsync(tile.Id, new SaveSiteTileDto { Placement = "tea", Title = "A", Link = "http://x" }));
            Assert.Equal("Green", tile.Title);
        }

        [Fact]
        public async Task Delete_RemovesTile_AndItsImage()
        {
            var tile = Add("tea", "Green", 1, image: "/uploads/tiles/old.png");

            await Create().DeleteAsync(tile.Id);

            Assert.Empty(_repo.Rows);
            Assert.Equal(new[] { "/uploads/tiles/old.png" }, _files.Deleted);
            await Assert.ThrowsAsync<NotFoundException>(() => Create().DeleteAsync(tile.Id));
        }
    }
}
