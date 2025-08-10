using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using BourbonVault.Core.DTOs;
using FluentAssertions;
using Xunit;

namespace BourbonVault.Tests.Integration
{
    public class TastingNoteApiTests : IClassFixture<ApiTestFixture>
    {
        private readonly ApiTestFixture _fixture;
        private readonly HttpClient _client;
        private int _bottleId;

        public TastingNoteApiTests(ApiTestFixture fixture)
        {
            _fixture = fixture;
            _client = fixture.CreateClient();
        }

        private async Task AuthenticateAndCreateBottleAsync()
        {
            // Authenticate with unique username for each test run
            var authResult = await _fixture.RegisterTestUserAsync(_client);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authResult.Token);
            
            // Create a bottle to associate with tasting notes
            var bottle = new BottleCreateDto
            {
                Name = "Tasting Note Test Bottle",
                DistilleryId = 1,
                Type = "Bourbon",
                AgeYears = 8,
                Proof = 100,
                PurchasePrice = 49.99m,
                PurchaseDate = DateTime.Now,
                PurchaseLocation = "Test Store",
                Status = "Unopened",
                // Make sure all required fields are set
                Notes = "Test notes",
                ImageUrl = "test-image.jpg",
                TagIds = new List<int>(), // Initialize with empty list instead of null
                CurrentEstimatedValue = 55.99m
            };
            
            Console.WriteLine($"BOTTLE CREATE DTO: {System.Text.Json.JsonSerializer.Serialize(bottle)}");
            
            var response = await _client.PostAsJsonAsync("/api/bottles", bottle);
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Failed to create bottle: {response.StatusCode}");
                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error response: {content}");
                await _fixture.LogResponseError(response, "AuthenticateAndCreateBottleAsync");
            }
            response.EnsureSuccessStatusCode();
            var createdBottle = await response.Content.ReadFromJsonAsync<BottleDto>();
            _bottleId = createdBottle.Id;
        }

        [Fact]
        public async Task GetAllTastingNotes_WithoutAuth_ShouldReturnUnauthorized()
        {
            // Act
            var response = await _client.GetAsync("/api/tastingnotes");
            
            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetAllTastingNotes_WithAuth_ShouldReturnEmptyList()
        {
            // Arrange
            await AuthenticateAndCreateBottleAsync();
            
            // Act
            var response = await _client.GetAsync("/api/tastingnotes");
            
            // Assert
            response.EnsureSuccessStatusCode();
            var tastingNotes = await response.Content.ReadFromJsonAsync<List<TastingNoteDto>>();
            tastingNotes.Should().NotBeNull();
            tastingNotes.Should().BeEmpty();
        }

        [Fact]
        public async Task CreateTastingNote_WithValidData_ShouldSucceed()
        {
            // Arrange
            await AuthenticateAndCreateBottleAsync();
            
            var tastingNote = new TastingNoteCreateDto
            {
                BottleId = _bottleId,
                TastingDate = DateTime.Now.Date,
                AppearanceRating = 4,
                NoseRating = 4,
                TasteRating = 4,
                FinishRating = 4,
                OverallRating = 4,
                AppearanceNotes = "Deep amber",
                NoseNotes = "Caramel, vanilla, oak",
                TasteNotes = "Rich, full-bodied with notes of toffee and spice",
                FinishNotes = "Long with lingering warmth",
                AdditionalNotes = "Excellent bourbon for the price",
                IsPublic = true
            };
            
            // Act
            var response = await _client.PostAsJsonAsync("/api/tastingnotes", tastingNote);
            
            // Assert
            response.EnsureSuccessStatusCode();
            var createdTastingNote = await response.Content.ReadFromJsonAsync<TastingNoteDto>();
            
            createdTastingNote.Should().NotBeNull();
            createdTastingNote.Id.Should().NotBe(0);
            createdTastingNote.BottleId.Should().Be(tastingNote.BottleId);
            createdTastingNote.AppearanceRating.Should().Be(tastingNote.AppearanceRating);
            createdTastingNote.NoseRating.Should().Be(tastingNote.NoseRating);
            createdTastingNote.TasteRating.Should().Be(tastingNote.TasteRating);
            createdTastingNote.FinishRating.Should().Be(tastingNote.FinishRating);
            createdTastingNote.OverallRating.Should().Be(tastingNote.OverallRating);
            createdTastingNote.AppearanceNotes.Should().Be(tastingNote.AppearanceNotes);
            createdTastingNote.NoseNotes.Should().Be(tastingNote.NoseNotes);
            createdTastingNote.TasteNotes.Should().Be(tastingNote.TasteNotes);
            createdTastingNote.FinishNotes.Should().Be(tastingNote.FinishNotes);
            createdTastingNote.AdditionalNotes.Should().Be(tastingNote.AdditionalNotes);
            createdTastingNote.IsPublic.Should().Be(tastingNote.IsPublic);
        }

        [Fact]
        public async Task GetTastingNoteById_WithExistingNote_ShouldReturnNote()
        {
            // Arrange
            await AuthenticateAndCreateBottleAsync();
            
            var tastingNote = new TastingNoteCreateDto
            {
                BottleId = _bottleId,
                TastingDate = DateTime.Now.Date,
                AppearanceRating = 5,
                NoseRating = 5,
                TasteRating = 5,
                FinishRating = 5,
                OverallRating = 5,
                AppearanceNotes = "Dark amber",
                NoseNotes = "Vanilla, caramel, oak",
                TasteNotes = "Sweet with hints of dark fruit",
                FinishNotes = "Long, warm finish",
                AdditionalNotes = "Notes",
                IsPublic = true
            };
            
            var createResponse = await _client.PostAsJsonAsync("/api/tastingnotes", tastingNote);
            createResponse.EnsureSuccessStatusCode();
            var createdTastingNote = await createResponse.Content.ReadFromJsonAsync<TastingNoteDto>();
            
            // Act
            var response = await _client.GetAsync($"/api/tastingnotes/{createdTastingNote.Id}");
            
            // Assert
            response.EnsureSuccessStatusCode();
            var retrievedNote = await response.Content.ReadFromJsonAsync<TastingNoteDto>();
            
            retrievedNote.Should().NotBeNull();
            retrievedNote.Id.Should().Be(createdTastingNote.Id);
            retrievedNote.BottleId.Should().Be(tastingNote.BottleId);
            retrievedNote.OverallRating.Should().Be(tastingNote.OverallRating);
            retrievedNote.AppearanceRating.Should().Be(tastingNote.AppearanceRating);
            retrievedNote.NoseRating.Should().Be(tastingNote.NoseRating);
            retrievedNote.TasteRating.Should().Be(tastingNote.TasteRating);
            retrievedNote.FinishRating.Should().Be(tastingNote.FinishRating);
        }

        [Fact]
        public async Task UpdateTastingNote_WithValidData_ShouldSucceed()
        {
            // Arrange
            await AuthenticateAndCreateBottleAsync();
            
            var tastingNote = new TastingNoteCreateDto
            {
                BottleId = _bottleId,
                TastingDate = DateTime.Now.Date,
                AppearanceRating = 3,
                NoseRating = 3,
                TasteRating = 3,
                FinishRating = 3,
                OverallRating = 3,
                AppearanceNotes = "Light amber",
                NoseNotes = "Light vanilla, oak",
                TasteNotes = "Sweet corn, light spice",
                FinishNotes = "Medium finish",
                AdditionalNotes = "Notes",
                IsPublic = false
            };
            
            var createResponse = await _client.PostAsJsonAsync("/api/tastingnotes", tastingNote);
            createResponse.EnsureSuccessStatusCode();
            var createdTastingNote = await createResponse.Content.ReadFromJsonAsync<TastingNoteDto>();
            
            var updateDto = new TastingNoteUpdateDto
            {
                TastingDate = DateTime.Now.Date,
                AppearanceRating = 4, // Changed from 3
                NoseRating = 4, // Changed from 3
                TasteRating = 4, // Changed from 3
                FinishRating = 4, // Changed from 3
                OverallRating = 4, // Changed from 3
                AppearanceNotes = "Light amber",
                NoseNotes = "Stronger vanilla, oak, with hints of caramel", // Changed
                TasteNotes = "Sweet corn, light spice, with more complexity", // Changed
                FinishNotes = "Longer finish with pleasant warmth", // Changed
                AdditionalNotes = "Better on second tasting", // Added
                IsPublic = true // Changed
            };
            
            // Act
            var response = await _client.PutAsJsonAsync($"/api/tastingnotes/{createdTastingNote.Id}", updateDto);
            
            // Assert
            response.EnsureSuccessStatusCode();
            
            // Verify the update by getting the tasting note
            var getResponse = await _client.GetAsync($"/api/tastingnotes/{createdTastingNote.Id}");
            getResponse.EnsureSuccessStatusCode();
            var updatedNote = await getResponse.Content.ReadFromJsonAsync<TastingNoteDto>();
            
            updatedNote.Should().NotBeNull();
            updatedNote.Id.Should().Be(createdTastingNote.Id);
            updatedNote.AppearanceRating.Should().Be(updateDto.AppearanceRating);
            updatedNote.NoseRating.Should().Be(updateDto.NoseRating);
            updatedNote.TasteRating.Should().Be(updateDto.TasteRating);
            updatedNote.FinishRating.Should().Be(updateDto.FinishRating);
            updatedNote.OverallRating.Should().Be(updateDto.OverallRating);
            updatedNote.AppearanceNotes.Should().Be(updateDto.AppearanceNotes);
            updatedNote.NoseNotes.Should().Be(updateDto.NoseNotes);
            updatedNote.TasteNotes.Should().Be(updateDto.TasteNotes);
            updatedNote.FinishNotes.Should().Be(updateDto.FinishNotes);
            updatedNote.AdditionalNotes.Should().Be(updateDto.AdditionalNotes);
            updatedNote.IsPublic.Should().Be(updateDto.IsPublic);
        }

        [Fact]
        public async Task DeleteTastingNote_WithExistingNote_ShouldSucceed()
        {
            // Arrange
            await AuthenticateAndCreateBottleAsync();

            var tastingNote = new TastingNoteCreateDto
            {
                BottleId = _bottleId,
                TastingDate = DateTime.Now.Date,
                AppearanceRating = 4,
                NoseRating = 4,
                TasteRating = 4,
                FinishRating = 4,
                OverallRating = 4,
                AppearanceNotes = "Deep amber",
                NoseNotes = "Caramel, vanilla, oak",
                TasteNotes = "Rich, full-bodied with notes of toffee and spice",
                FinishNotes = "Long with lingering warmth",
                AdditionalNotes = "Excellent bourbon for the price",
                IsPublic = true
            };

            var createResponse = await _client.PostAsJsonAsync("/api/tastingnotes", tastingNote);
            createResponse.EnsureSuccessStatusCode();
            var createdTastingNote = await createResponse.Content.ReadFromJsonAsync<TastingNoteDto>();
            
            // Act
            var response = await _client.DeleteAsync($"/api/tastingnotes/{createdTastingNote.Id}");
            
            // Assert
            response.EnsureSuccessStatusCode();
            
            // Verify the note is deleted
            var getResponse = await _client.GetAsync($"/api/tastingnotes/{createdTastingNote.Id}");
            getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetTastingNotesByBottle_ShouldReturnMatchingNotes()
        {
            // Arrange
            await AuthenticateAndCreateBottleAsync();
            
            // Create multiple tasting notes for the same bottle
            var tastingNote1 = new TastingNoteCreateDto
            {
                BottleId = _bottleId,
                TastingDate = DateTime.Now.AddDays(-10).Date,
                AppearanceRating = 3,
                NoseRating = 3,
                TasteRating = 3,
                FinishRating = 3,
                OverallRating = 3,
                AppearanceNotes = "Amber",
                NoseNotes = "First tasting notes",
                TasteNotes = "First tasting notes",
                FinishNotes = "First tasting notes",
                AdditionalNotes = "Notes",
                IsPublic = true
            };
            
            var tastingNote2 = new TastingNoteCreateDto
            {
                BottleId = _bottleId,
                TastingDate = DateTime.Now.AddDays(-5).Date,
                AppearanceRating = 4,
                NoseRating = 4,
                TasteRating = 4,
                FinishRating = 4,
                OverallRating = 4,
                AppearanceNotes = "Gold",
                NoseNotes = "Second tasting notes",
                TasteNotes = "Second tasting notes",
                FinishNotes = "Second tasting notes",
                AdditionalNotes = "Notes",
                IsPublic = true
            };
            
            await _client.PostAsJsonAsync("/api/tastingnotes", tastingNote1);
            await _client.PostAsJsonAsync("/api/tastingnotes", tastingNote2);
            
            // Act
            var response = await _client.GetAsync($"/api/tastingnotes/bottle/{_bottleId}");
            
            // Assert
            response.EnsureSuccessStatusCode();
            var notes = await response.Content.ReadFromJsonAsync<List<TastingNoteDto>>();
            
            notes.Should().NotBeNull();
            notes.Should().HaveCount(2);
            notes.Should().Contain(n => n.NoseNotes == "First tasting notes");
            notes.Should().Contain(n => n.NoseNotes == "Second tasting notes");
        }

        [Fact]
        public async Task GetPublicTastingNotes_ShouldReturnOnlyPublicNotes()
        {
            // Arrange
            await AuthenticateAndCreateBottleAsync();
            
            // Create one public and one private tasting note
            var publicNote = new TastingNoteCreateDto
            {
                BottleId = _bottleId,
                TastingDate = DateTime.Now.AddDays(-10).Date,
                AppearanceRating = 4,
                NoseRating = 4,
                TasteRating = 4,
                FinishRating = 4,
                OverallRating = 4,
                AppearanceNotes = "Gold",
                NoseNotes = "Public tasting notes",
                TasteNotes = "Public tasting notes",
                FinishNotes = "Public tasting notes",
                AdditionalNotes ="Notes",
                IsPublic = true
            };
            
            var privateNote = new TastingNoteCreateDto
            {
                BottleId = _bottleId,
                TastingDate = DateTime.Now.AddDays(-5).Date,
                AppearanceRating = 3,
                NoseRating = 3,
                TasteRating = 3,
                FinishRating = 3,
                OverallRating = 3,
                AppearanceNotes = "Amber",
                NoseNotes = "Private tasting notes",
                TasteNotes = "Private tasting notes",
                FinishNotes = "Private tasting notes",
                AdditionalNotes = "Notes",
                IsPublic = false
            };
            
            await _client.PostAsJsonAsync("/api/tastingnotes", publicNote);
            await _client.PostAsJsonAsync("/api/tastingnotes", privateNote);
            
            // Act
            var response = await _client.GetAsync("/api/tastingnotes/public");
            
            // Assert
            response.EnsureSuccessStatusCode();
            var notes = await response.Content.ReadFromJsonAsync<List<TastingNoteDto>>();
            
            notes.Should().NotBeNull();
            notes.Should().Contain(n => n.NoseNotes == "Public tasting notes");
            notes.Should().NotContain(n => n.NoseNotes == "Private tasting notes");
        }
    }
}
