using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using UrlShortener.Domain.Entities;
using UrlShortener.Infrastructure.Persistence;
using UrlShortener.Infrastructure.Persistence.Repositories;

namespace UrlShortener.Tests.Persistence.Repositories;

public class UserRepositoryTests
{
    private static ApplicationDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    // -----------------------------------------------------------
    // UserExistsAsync
    // -----------------------------------------------------------
    [Fact]
    public async Task UserExistsAsync_ShouldReturnTrue_WhenEmailExists()
    {
        // Arrange
        var db = CreateDb();
        db.Users.Add(new User { Email = "test@test.com", Name = "Test" });
        await db.SaveChangesAsync();

        var sut = new UserRepository(db);

        // Act
        var exists = await sut.UserExistsAsync("test@test.com");

        // Assert
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task UserExistsAsync_ShouldReturnFalse_WhenEmailDoesNotExist()
    {
        // Arrange
        var db = CreateDb();
        var sut = new UserRepository(db);

        // Act
        var exists = await sut.UserExistsAsync("missing@test.com");

        // Assert
        exists.Should().BeFalse();
    }

    // -----------------------------------------------------------
    // AddUserAsync
    // -----------------------------------------------------------
    [Fact]
    public async Task AddUserAsync_ShouldInsertUserIntoDatabase()
    {
        // Arrange
        var db = CreateDb();
        var sut = new UserRepository(db);

        var user = new User
        {
            Name = "John",
            Email = "john@example.com",
            PasswordHash = "hashed"
        };

        // Act
        await sut.AddUserAsync(user);

        // Assert
        var inserted = await db.Users.FirstOrDefaultAsync(u => u.Email == "john@example.com");
        inserted.Should().NotBeNull();
        inserted!.Name.Should().Be("John");
        inserted.PasswordHash.Should().Be("hashed");
    }

    // -----------------------------------------------------------
    // FindByEmailAsync
    // -----------------------------------------------------------
    [Fact]
    public async Task FindByEmailAsync_ShouldReturnCorrectUser()
    {
        // Arrange
        var db = CreateDb();

        var user = new User
        {
            Name = "Anna",
            Email = "anna@example.com",
            PasswordHash = "pwh"
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();

        var sut = new UserRepository(db);

        // Act
        var result = await sut.FindByEmailAsync("anna@example.com");

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Anna");
    }

    [Fact]
    public async Task FindByEmailAsync_ShouldReturnNull_WhenNotFound()
    {
        // Arrange
        var db = CreateDb();
        var sut = new UserRepository(db);

        // Act
        var result = await sut.FindByEmailAsync("missing@example.com");

        // Assert
        result.Should().BeNull();
    }

    // -----------------------------------------------------------
    // FindByIdAsync
    // -----------------------------------------------------------
    [Fact]
    public async Task FindByIdAsync_ShouldReturnCorrectUser()
    {
        // Arrange
        var db = CreateDb();

        var user = new User
        {
            Name = "Olga",
            Email = "olga@example.com",
            PasswordHash = "olga-hash"
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();

        var sut = new UserRepository(db);

        // Act
        var result = await sut.FindByIdAsync(user.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be("olga@example.com");
    }

    [Fact]
    public async Task FindByIdAsync_ShouldReturnNull_WhenUserNotFound()
    {
        // Arrange
        var db = CreateDb();
        var sut = new UserRepository(db);

        // Act
        var result = await sut.FindByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }
}
