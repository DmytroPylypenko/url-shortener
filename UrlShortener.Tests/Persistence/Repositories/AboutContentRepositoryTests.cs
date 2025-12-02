using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using UrlShortener.Web.Domain.Entities;
using UrlShortener.Web.Persistence;
using UrlShortener.Web.Persistence.Repositories;

namespace UrlShortener.Tests.Persistence.Repositories;

public class AboutContentRepositoryTests
{
    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()) // isolated DB per test
            .Options;

        return new ApplicationDbContext(options);
    }

    // -----------------------------------------------------------
    // GET 
    // -----------------------------------------------------------
    [Fact]
    public async Task GetAsync_WhenNoRecordExists_ShouldCreateDefaultContent()
    {
        // Arrange
        var context = CreateContext();
        var sut = new AboutContentRepository(context);

        // Act
        var result = await sut.GetAsync(CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Content.Should().Contain("Base62 encoding");
        result.LastUpdatedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

        var dbRecord = await context.AboutContents.FirstOrDefaultAsync();
        dbRecord.Should().NotBeNull();
    }
    
    [Fact]
    public async Task GetAsync_WhenRecordExists_ShouldReturnExistingContent()
    {
        // Arrange
        var context = CreateContext();
        var existing = new AboutContent
        {
            Content = "Existing content",
            LastUpdatedAtUtc = DateTime.UtcNow.AddDays(-1)
        };

        context.AboutContents.Add(existing);
        await context.SaveChangesAsync();

        var sut = new AboutContentRepository(context);

        // Act
        var result = await sut.GetAsync(CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Content.Should().Be("Existing content");
        result.LastUpdatedAtUtc.Should().Be(existing.LastUpdatedAtUtc);

        // Ensure repository did NOT create a second record
        (await context.AboutContents.CountAsync()).Should().Be(1);
    }

    // -----------------------------------------------------------
    // UPDATE
    // -----------------------------------------------------------
    [Fact]
    public async Task UpdateAsync_ShouldMarkEntityAsModified()
    {
        // Arrange
        var context = CreateContext();
        var sut = new AboutContentRepository(context);

        var entity = new AboutContent
        {
            Content = "Original",
            LastUpdatedAtUtc = DateTime.UtcNow
        };

        context.AboutContents.Add(entity);
        await context.SaveChangesAsync();

        // Modify content
        entity.Content = "Updated";
        await sut.UpdateAsync(entity);

        // Act
        var state = context.Entry(entity).State;

        // Assert
        state.Should().Be(EntityState.Modified);
    }

    // -----------------------------------------------------------
    // SAVE CHANGES
    // -----------------------------------------------------------
    [Fact]
    public async Task SaveChangesAsync_ShouldPersistChangesToDatabase()
    {
        // Arrange
        var context = CreateContext();
        var sut = new AboutContentRepository(context);

        var entity = new AboutContent
        {
            Content = "Original content",
            LastUpdatedAtUtc = DateTime.UtcNow
        };

        context.AboutContents.Add(entity);
        await context.SaveChangesAsync();

        // Update
        entity.Content = "New content";
        await sut.UpdateAsync(entity);

        // Act
        await sut.SaveChangesAsync(CancellationToken.None);

        // Assert
        var refreshed = await context.AboutContents.FirstAsync();
        refreshed.Content.Should().Be("New content");
    }
}
