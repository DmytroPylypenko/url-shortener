using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using UrlShortener.Web.Domain.Entities;
using UrlShortener.Web.Persistence;
using UrlShortener.Web.Persistence.Repositories;

namespace UrlShortener.Tests.Persistence.Repositories;

public class UrlRecordRepositoryTests
{
    private static ApplicationDbContext CreateDb()
    {
        var opts = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()) // isolated DB per test
            .Options;

        return new ApplicationDbContext(opts);
    }

    // -----------------------------------------------------------
    // GET BY ID
    // -----------------------------------------------------------
    [Fact]
    public async Task GetByIdAsync_ShouldReturnRecordWithIncludedUser()
    {
        // Arrange
        var db = CreateDb();

        var user = new User { Name = "Dima", Email = "dima@test.com" };
        var record = new UrlRecord
        {
            OriginalUrl = "https://google.com",
            ShortCode = "abc",
            CreatedAtUtc = DateTime.UtcNow,
            CreatedByUser = user
        };

        db.UrlRecords.Add(record);
        await db.SaveChangesAsync();

        var sut = new UrlRecordRepository(db);

        // Act
        var result = await sut.GetByIdAsync(record.Id);

        // Assert
        result.Should().NotBeNull();
        result!.CreatedByUser.Should().NotBeNull();
        result.CreatedByUser.Name.Should().Be("Dima");
    }

    // -----------------------------------------------------------
    // GET BY SHORTCODE
    // -----------------------------------------------------------
    [Fact]
    public async Task GetByShortCodeAsync_ShouldReturnCorrectRecord()
    {
        // Arrange
        var db = CreateDb();

        var record = new UrlRecord
        {
            OriginalUrl = "https://sample.com",
            ShortCode = "xyz",
            CreatedAtUtc = DateTime.UtcNow,
            CreatedByUser = new User { Name = "User123" }
        };

        db.UrlRecords.Add(record);
        await db.SaveChangesAsync();

        var sut = new UrlRecordRepository(db);

        // Act
        var result = await sut.GetByShortCodeAsync("xyz");

        // Assert
        result.Should().NotBeNull();
        result!.ShortCode.Should().Be("xyz");
        result.CreatedByUser.Name.Should().Be("User123");
    }

    // -----------------------------------------------------------
    // GET ALL 
    // -----------------------------------------------------------
    [Fact]
    public async Task GetAllAsync_ShouldReturnRecordsOrderedByDateDescending()
    {
        // Arrange
        var db = CreateDb();

        var rec1 = new UrlRecord
        {
            OriginalUrl = "https://a.com",
            ShortCode = "a",
            CreatedAtUtc = DateTime.UtcNow.AddHours(-2),
            CreatedByUser = new User { Name = "UserA" }
        };
        var rec2 = new UrlRecord
        {
            OriginalUrl = "https://b.com",
            ShortCode = "b",
            CreatedAtUtc = DateTime.UtcNow,
            CreatedByUser = new User { Name = "UserB" }
        };

        db.UrlRecords.AddRange(rec1, rec2);
        await db.SaveChangesAsync();

        var sut = new UrlRecordRepository(db);

        // Act
        var list = (await sut.GetAllAsync()).ToList();

        // Assert
        list.Should().HaveCount(2);
        list[0].ShortCode.Should().Be("b"); // newest first
        list[1].ShortCode.Should().Be("a");
    }

    // -----------------------------------------------------------
    // ADD
    // -----------------------------------------------------------
    [Fact]
    public async Task AddAsync_ShouldAddRecordToDatabase()
    {
        // Arrange
        var db = CreateDb();
        var sut = new UrlRecordRepository(db);

        var record = new UrlRecord
        {
            OriginalUrl = "https://dotnet.com",
            ShortCode = "dn",
            CreatedAtUtc = DateTime.UtcNow,
            CreatedByUser = new User { Name = "Admin" }
        };

        // Act
        await sut.AddAsync(record);
        await sut.SaveChangesAsync();

        var exists = await db.UrlRecords.AnyAsync(r => r.ShortCode == "dn");

        // Assert
        exists.Should().BeTrue();
    }

    // -----------------------------------------------------------
    // DELETE
    // -----------------------------------------------------------
    [Fact]
    public async Task DeleteAsync_ShouldRemoveRecordFromDatabase()
    {
        // Arrange
        var db = CreateDb();

        var record = new UrlRecord
        {
            OriginalUrl = "https://delete-me.com",
            ShortCode = "del",
            CreatedAtUtc = DateTime.UtcNow,
            CreatedByUser = new User { Name = "DelUser" }
        };

        db.UrlRecords.Add(record);
        await db.SaveChangesAsync();

        var sut = new UrlRecordRepository(db);

        // Act
        await sut.DeleteAsync(record);
        await sut.SaveChangesAsync();

        var exists = await db.UrlRecords.AnyAsync(r => r.ShortCode == "del");

        // Assert
        exists.Should().BeFalse();
    }

    // -----------------------------------------------------------
    // EXISTS CHECKS
    // -----------------------------------------------------------
    [Fact]
    public async Task UrlExistsAsync_ShouldReturnTrueIfUrlExists()
    {
        // Arrange
        var db = CreateDb();

        var record = new UrlRecord
        {
            OriginalUrl = "https://exists.com",
            ShortCode = "ex",
            CreatedAtUtc = DateTime.UtcNow,
            CreatedByUser = new User { Name = "ExistsUser" }
        };

        db.UrlRecords.Add(record);
        await db.SaveChangesAsync();

        var sut = new UrlRecordRepository(db);

        // Act
        var exists = await sut.UrlExistsAsync("https://exists.com");

        // Assert
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task UrlExistsAsync_ShouldReturnFalseIfUrlDoesNotExist()
    {
        // Arrange
        var db = CreateDb();
        var sut = new UrlRecordRepository(db);

        // Act
        var exists = await sut.UrlExistsAsync("https://missing.com");

        // Assert
        exists.Should().BeFalse();
    }

    // -----------------------------------------------------------
    // SHORT CODE CHECKS
    // -----------------------------------------------------------
    [Fact]
    public async Task ShortCodeExistsAsync_ShouldReturnTrueIfShortCodeExists()
    {
        // Arrange
        var db = CreateDb();

        var record = new UrlRecord
        {
            OriginalUrl = "https://short.com",
            ShortCode = "code",
            CreatedAtUtc = DateTime.UtcNow,
            CreatedByUser = new User { Name = "ShortUser" }
        };

        db.UrlRecords.Add(record);
        await db.SaveChangesAsync();

        var sut = new UrlRecordRepository(db);

        // Act
        var exists = await sut.ShortCodeExistsAsync("code");

        // Assert
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task ShortCodeExistsAsync_ShouldReturnFalseIfNotFound()
    {
        // Arrange
        var db = CreateDb();
        var sut = new UrlRecordRepository(db);

        // Act
        var exists = await sut.ShortCodeExistsAsync("nope");

        // Assert
        exists.Should().BeFalse();
    }
}
