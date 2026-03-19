using AniLog.API.Data;
using AniLog.API.DTOs;
using AniLog.API.Models;
using AniLog.API.Services;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace AniLog.Tests;

public class AnimeLogServiceTests
{
    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private static Mock<IJikanService> CreateJikanMock(JikanAnimeData? data = null)
    {
        var mock = new Mock<IJikanService>();
        mock.Setup(j => j.GetAnimeByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(data ?? new JikanAnimeData
            {
                MalId = 20,
                Title = "Naruto",
                TitleEnglish = "Naruto",
                Episodes = 220,
                Score = 7.97m,
                Genres = [new JikanGenre { Name = "Action" }],
                Images = new JikanImages { Jpg = new JikanJpg { ImageUrl = "https://img.test/naruto.jpg" } }
            });
        return mock;
    }

    [Fact]
    public async Task GetAllAsync_EmptyDb_ReturnsEmptyList()
    {
        using var db = CreateDbContext();
        var service = new AnimeLogService(db, CreateJikanMock().Object);

        var result = await service.GetAllAsync(null, 1, 20);

        Assert.Empty(result.Data);
        Assert.Equal(0, result.Total);
    }

    [Fact]
    public async Task AddAnimeAsync_NewAnime_ReturnsDto()
    {
        using var db = CreateDbContext();
        var service = new AnimeLogService(db, CreateJikanMock().Object);

        var result = await service.AddAnimeAsync(new AddAnimeDto { MalId = 20, MyStatus = AnimeStatus.Watching, MyScore = 8 });

        Assert.Equal(20, result.MalId);
        Assert.Equal("Naruto", result.Title);
        Assert.Equal(AnimeStatus.Watching, result.MyStatus);
        Assert.Equal(1, await db.AnimeLogs.CountAsync());
    }

    [Fact]
    public async Task AddAnimeAsync_DuplicateMalId_ThrowsInvalidOperationException()
    {
        using var db = CreateDbContext();
        var service = new AnimeLogService(db, CreateJikanMock().Object);

        var dto = new AddAnimeDto { MalId = 20, MyStatus = AnimeStatus.Completed };
        await service.AddAnimeAsync(dto);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.AddAnimeAsync(dto));
    }

    [Fact]
    public async Task UpdateAnimeAsync_ExistingAnime_UpdatesFields()
    {
        using var db = CreateDbContext();
        var service = new AnimeLogService(db, CreateJikanMock().Object);

        var added = await service.AddAnimeAsync(new AddAnimeDto { MalId = 20, MyStatus = AnimeStatus.Watching, MyScore = 7 });
        var result = await service.UpdateAnimeAsync(added.Id, new UpdateAnimeDto { MyScore = 9.5m, MyStatus = AnimeStatus.Completed, EpisodesWatched = 220 });

        Assert.NotNull(result);
        Assert.Equal(9.5m, result!.MyScore);
        Assert.Equal(AnimeStatus.Completed, result.MyStatus);
        Assert.Equal(220, result.EpisodesWatched);
    }

    [Fact]
    public async Task UpdateAnimeAsync_NonExistentId_ReturnsNull()
    {
        using var db = CreateDbContext();
        var service = new AnimeLogService(db, CreateJikanMock().Object);

        var result = await service.UpdateAnimeAsync(999, new UpdateAnimeDto { MyScore = 8 });

        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteAnimeAsync_ExistingAnime_RemovesAndReturnsTrue()
    {
        using var db = CreateDbContext();
        var service = new AnimeLogService(db, CreateJikanMock().Object);

        var added = await service.AddAnimeAsync(new AddAnimeDto { MalId = 20, MyStatus = AnimeStatus.Completed });
        var deleted = await service.DeleteAnimeAsync(added.Id);

        Assert.True(deleted);
        Assert.Equal(0, await db.AnimeLogs.CountAsync());
    }

    [Fact]
    public async Task DeleteAnimeAsync_NonExistentId_ReturnsFalse()
    {
        using var db = CreateDbContext();
        var service = new AnimeLogService(db, CreateJikanMock().Object);

        var result = await service.DeleteAnimeAsync(999);

        Assert.False(result);
    }

    [Fact]
    public async Task GetAllAsync_FilterByStatus_ReturnsOnlyMatching()
    {
        using var db = CreateDbContext();
        var jikanMock = new Mock<IJikanService>();
        jikanMock.Setup(j => j.GetAnimeByIdAsync(20))
            .ReturnsAsync(new JikanAnimeData { MalId = 20, Title = "Naruto", Images = new JikanImages { Jpg = new JikanJpg { ImageUrl = "https://img.test/img.jpg" } } });
        jikanMock.Setup(j => j.GetAnimeByIdAsync(16498))
            .ReturnsAsync(new JikanAnimeData { MalId = 16498, Title = "Attack on Titan", Images = new JikanImages { Jpg = new JikanJpg { ImageUrl = "https://img.test/img.jpg" } } });

        var service = new AnimeLogService(db, jikanMock.Object);

        await service.AddAnimeAsync(new AddAnimeDto { MalId = 20, MyStatus = AnimeStatus.Watching });
        await service.AddAnimeAsync(new AddAnimeDto { MalId = 16498, MyStatus = AnimeStatus.Completed });

        var watching = await service.GetAllAsync(AnimeStatus.Watching, 1, 20);
        var completed = await service.GetAllAsync(AnimeStatus.Completed, 1, 20);

        Assert.Single(watching.Data);
        Assert.Equal("Naruto", watching.Data[0].Title);
        Assert.Single(completed.Data);
        Assert.Equal("Attack on Titan", completed.Data[0].Title);
    }
}
