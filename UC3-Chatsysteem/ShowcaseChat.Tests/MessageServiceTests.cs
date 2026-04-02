using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Showcase_Chat.Data;
using Showcase_Chat.Models;
using Showcase_Chat.Services;
using Xunit;

namespace ShowcaseChat.Tests
{
    // Tests voor MessageService — alle happy paths en unhappy paths
    public class MessageServiceTests
    {
        /// <summary>
        /// Maakt een in-memory AppDbContext met een seed-gebruiker.
        /// Elke test krijgt een unieke database om interferentie te voorkomen.
        /// </summary>
        private static AppDbContext CreateDbContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(dbName)
                .Options;
            return new AppDbContext(options);
        }

        private static MessageService CreateService(AppDbContext db)
        {
            var logger = new Mock<ILogger<MessageService>>().Object;
            return new MessageService(db, logger);
        }

        // Hulpfunctie: voeg een testgebruiker toe aan de database
        private static ApplicationUser SeedUser(AppDbContext db, string id = "user-1")
        {
            var user = new ApplicationUser
            {
                Id = id,
                UserName = $"{id}@test.nl",
                Email = $"{id}@test.nl",
                DisplayName = $"TestUser {id}"
            };
            db.Users.Add(user);
            db.SaveChanges();
            return user;
        }

        // ─── Happy path tests ──────────────────────────────────────────────────

        [Fact]
        public async Task SendMessageAsync_SlaatBerichtOpInDatabase()
        {
            // Arrange
            using var db = CreateDbContext("test-send-1");
            var user = SeedUser(db);
            var service = CreateService(db);

            // Act
            var result = await service.SendMessageAsync(user.Id, "Hallo wereld!");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Hallo wereld!", result.Content);
            Assert.Equal(user.Id, result.SenderId);
            Assert.Equal(1, await db.Messages.CountAsync());
        }

        [Fact]
        public async Task SendMessageAsync_StriptWhitespaceUitContent()
        {
            // Arrange
            using var db = CreateDbContext("test-send-trim");
            var user = SeedUser(db);
            var service = CreateService(db);

            // Act
            var result = await service.SendMessageAsync(user.Id, "   Hallo   ");

            // Assert — content moet getrimd worden opgeslagen
            Assert.Equal("Hallo", result.Content);
        }

        [Fact]
        public async Task GetAllMessagesAsync_RetourneertAlleBerichtenMetSender()
        {
            // Arrange
            using var db = CreateDbContext("test-getall");
            var user = SeedUser(db);
            var service = CreateService(db);
            await service.SendMessageAsync(user.Id, "Bericht 1");
            await service.SendMessageAsync(user.Id, "Bericht 2");

            // Act
            var messages = (await service.GetAllMessagesAsync()).ToList();

            // Assert
            Assert.Equal(2, messages.Count);
            Assert.All(messages, m => Assert.NotNull(m.Sender));
        }

        [Fact]
        public async Task GetMessagesByUserAsync_RetourneertAlleenBerichtenVanDieGebruiker()
        {
            // Arrange
            using var db = CreateDbContext("test-filter");
            var user1 = SeedUser(db, "user-a");
            var user2 = SeedUser(db, "user-b");
            var service = CreateService(db);
            await service.SendMessageAsync(user1.Id, "Van user1");
            await service.SendMessageAsync(user2.Id, "Van user2");

            // Act
            var messages = (await service.GetMessagesByUserAsync(user1.Id)).ToList();

            // Assert — alleen berichten van user1
            Assert.Single(messages);
            Assert.Equal(user1.Id, messages[0].SenderId);
        }

        // ─── Unhappy path tests ───────────────────────────────────────────────

        [Fact]
        public async Task SendMessageAsync_GooidExceptionBijLegeContent()
        {
            // Arrange
            using var db = CreateDbContext("test-empty");
            var user = SeedUser(db);
            var service = CreateService(db);

            // Act + Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                service.SendMessageAsync(user.Id, ""));
        }

        [Fact]
        public async Task SendMessageAsync_GooidExceptionBijWhitespaceContent()
        {
            // Arrange
            using var db = CreateDbContext("test-whitespace");
            var user = SeedUser(db);
            var service = CreateService(db);

            // Act + Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                service.SendMessageAsync(user.Id, "   "));
        }

        [Fact]
        public async Task SendMessageAsync_GooidExceptionBijTeLangueContent()
        {
            // Arrange
            using var db = CreateDbContext("test-toolong");
            var user = SeedUser(db);
            var service = CreateService(db);
            var teVeelTekens = new string('A', 1001);

            // Act + Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                service.SendMessageAsync(user.Id, teVeelTekens));
        }

        [Fact]
        public async Task GetMessagesByUserAsync_GooidExceptionBijLegeSenderId()
        {
            // Arrange
            using var db = CreateDbContext("test-emptyid");
            var service = CreateService(db);

            // Act + Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                service.GetMessagesByUserAsync(""));
        }
    }
}
