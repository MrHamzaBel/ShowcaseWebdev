using Microsoft.EntityFrameworkCore;
using ShowcaseWebsite.Data;
using ShowcaseWebsite.Models;
using ShowcaseWebsite.Services;
using Xunit;

namespace ShowcaseWebsite.Tests;

/// <summary>
/// Unit tests voor MessageService met SQLite in-memory database.
/// Controleert: opslaan, ophalen (recent + per gebruiker), XSS-input bewaren zonder te wijzigen.
/// </summary>
public class MessageServiceTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly MessageService _sut; // System Under Test

    public MessageServiceTests()
    {
        // Gebruik een unieke in-memory SQLite database per test-run (geen cross-test vervuiling)
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _db = new AppDbContext(options);
        _sut = new MessageService(_db);
    }

    [Fact]
    public async Task SaveAsync_PersistsMessageInDatabase()
    {
        // Act
        var msg = await _sut.SaveAsync("alice@test.nl", "Hallo wereld");

        // Assert
        Assert.NotEqual(0, msg.Id);
        Assert.Equal("alice@test.nl", msg.UserName);
        Assert.Equal("Hallo wereld", msg.Content);
        Assert.True(msg.SentAt <= DateTime.UtcNow);
    }

    [Fact]
    public async Task GetRecentAsync_ReturnsMessagesInChronologicalOrder()
    {
        // Arrange – voeg berichten toe op verschillende tijdstippen
        await _sut.SaveAsync("userA", "Eerste");
        await Task.Delay(5); // klein tijdsverschil
        await _sut.SaveAsync("userB", "Tweede");
        await Task.Delay(5);
        await _sut.SaveAsync("userA", "Derde");

        // Act
        var result = await _sut.GetRecentAsync(50);

        // Assert – chronologische volgorde (oudste eerst)
        Assert.Equal(3, result.Count);
        Assert.Equal("Eerste", result[0].Content);
        Assert.Equal("Derde", result[2].Content);
    }

    [Fact]
    public async Task GetRecentAsync_RespectsCountLimit()
    {
        // Arrange – voeg meer berichten toe dan het limiet
        for (int i = 1; i <= 10; i++)
            await _sut.SaveAsync("user", $"Bericht {i}");

        // Act
        var result = await _sut.GetRecentAsync(count: 5);

        // Assert – maximaal 5 terug (de meest recente)
        Assert.Equal(5, result.Count);
    }

    [Fact]
    public async Task GetByUserAsync_ReturnsOnlyMessagesForThatUser()
    {
        // Arrange
        await _sut.SaveAsync("alice@test.nl", "Van Alice");
        await _sut.SaveAsync("bob@test.nl", "Van Bob");
        await _sut.SaveAsync("alice@test.nl", "Nog een van Alice");

        // Act
        var result = await _sut.GetByUserAsync("alice@test.nl");

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, m => Assert.Equal("alice@test.nl", m.UserName));
    }

    [Fact]
    public async Task SaveAsync_StoresRawContent_WithoutModification()
    {
        // XSS-input wordt opgeslagen zoals ingevoerd;
        // Razor encode bij weergave (dit is de verantwoordelijkheid van de view, niet de service).
        string xssInput = "<script>alert('xss')</script>";
        var msg = await _sut.SaveAsync("tester", xssInput);

        Assert.Equal(xssInput, msg.Content);
    }

    [Fact]
    public async Task GetByUserAsync_ReturnsEmpty_WhenUserHasNoMessages()
    {
        await _sut.SaveAsync("someone", "Bericht");

        var result = await _sut.GetByUserAsync("nobody@test.nl");

        Assert.Empty(result);
    }

    public void Dispose() => _db.Dispose();
}
