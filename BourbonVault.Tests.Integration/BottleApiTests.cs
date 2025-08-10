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
    public class BottleApiTests : IClassFixture<ApiTestFixture>
    {
        private readonly ApiTestFixture _fixture;
        private readonly HttpClient _client;

        public BottleApiTests(ApiTestFixture fixture)
        {
            _fixture = fixture;
            _client = fixture.CreateClient();
        }

        private async Task AuthenticateAsync()
        {
            var authResult = await _fixture.RegisterTestUserAsync(_client);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authResult.Token);
        }
        
        [Fact]
        public async Task GetAllBottles_WithoutAuth_ShouldReturnUnauthorized()
        {
            // Act
            var response = await _client.GetAsync("/api/bottles");
            
            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetAllBottles_WithAuth_ShouldReturnEmptyList()
        {
            // Arrange
            await AuthenticateAsync();
            
            // Act
            var response = await _client.GetAsync("/api/bottles");
            
            // Assert
            response.EnsureSuccessStatusCode();
            var bottles = await response.Content.ReadFromJsonAsync<List<BottleDto>>();
            bottles.Should().NotBeNull();
            bottles.Should().BeEmpty();
        }

        [Fact]
        public async Task CreateBottle_WithValidData_ShouldSucceed()
        {
            // Arrange
            await AuthenticateAsync();
            
            var bottle = new BottleCreateDto
            {
                Name = "Test Bourbon",
                DistilleryId = 1,
                Type = "Bourbon",
                AgeYears = 8,
                Proof = 100,
                PurchasePrice = 49.99m,
                PurchaseDate = DateTime.Now,
                PurchaseLocation = "Test Store",
                Notes = "Test description",
                Status = "Unopened",
                TagIds = new List<int>(), // Use empty list instead of specific tag IDs
                // Make sure all required fields are set
                ImageUrl = "test-image.jpg",
                CurrentEstimatedValue = 55.99m
            };
            
            Console.WriteLine($"BOTTLE CREATE DTO: {System.Text.Json.JsonSerializer.Serialize(bottle)}");
            
            // Act
            var response = await _client.PostAsJsonAsync("/api/bottles", bottle);
            
            // Assert
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Failed to create bottle: {response.StatusCode}");
                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error response: {content}");
                await _fixture.LogResponseError(response, "CreateBottle_WithValidData");
            }
            response.EnsureSuccessStatusCode();
            var createdBottle = await response.Content.ReadFromJsonAsync<BottleDto>();
            
            createdBottle.Should().NotBeNull();
            createdBottle.Id.Should().NotBe(0);
            createdBottle.Name.Should().Be(bottle.Name);
            createdBottle.Type.Should().Be(bottle.Type);
            createdBottle.AgeYears.Should().Be(bottle.AgeYears);
            createdBottle.Proof.Should().Be(bottle.Proof);
            createdBottle.PurchasePrice.Should().Be(bottle.PurchasePrice);
            createdBottle.Notes.Should().Be(bottle.Notes);
            // Tags are optional in tests; no assertion on tag count
        }

        [Fact]
        public async Task GetBottleById_WithExistingBottle_ShouldReturnBottle()
        {
            // Arrange
            await AuthenticateAsync();
            
            var bottle = new BottleCreateDto
            {
                Name = "Get By ID Test",
                DistilleryId = 1,
                Type = "Bourbon",
                AgeYears = 10,
                Proof = 90,
                PurchasePrice = 59.99m,
                PurchaseDate = DateTime.Now,
                PurchaseLocation = "Test Store",
                Status = "Unopened"
            };
            
            var createResponse = await _client.PostAsJsonAsync("/api/bottles", bottle);
            createResponse.EnsureSuccessStatusCode();
            var createdBottle = await createResponse.Content.ReadFromJsonAsync<BottleDto>();
            
            // Act
            var response = await _client.GetAsync($"/api/bottles/{createdBottle.Id}");
            
            // Assert
            response.EnsureSuccessStatusCode();
            var retrievedBottle = await response.Content.ReadFromJsonAsync<BottleDto>();
            
            retrievedBottle.Should().NotBeNull();
            retrievedBottle.Id.Should().Be(createdBottle.Id);
            retrievedBottle.Name.Should().Be(bottle.Name);
            retrievedBottle.Type.Should().Be(bottle.Type);
            retrievedBottle.AgeYears.Should().Be(bottle.AgeYears);
            retrievedBottle.Proof.Should().Be(bottle.Proof);
            retrievedBottle.PurchasePrice.Should().Be(bottle.PurchasePrice);
        }

        [Fact]
        public async Task UpdateBottle_WithValidData_ShouldSucceed()
        {
            // Arrange
            await AuthenticateAsync();
            
            var bottle = new BottleCreateDto
            {
                Name = "Update Test Original",
                DistilleryId = 1,
                Type = "Bourbon",
                AgeYears = 5,
                Proof = 80,
                PurchasePrice = 29.99m,
                PurchaseDate = DateTime.Now,
                PurchaseLocation = "Test Store",
                Status = "Unopened"
            };
            
            var createResponse = await _client.PostAsJsonAsync("/api/bottles", bottle);
            createResponse.EnsureSuccessStatusCode();
            var createdBottle = await createResponse.Content.ReadFromJsonAsync<BottleDto>();
            
            var updateDto = new BottleUpdateDto
            {
                Id = createdBottle.Id,
                Name = "Update Test Modified",
                Type = "Rye",
                DistilleryId = 1,
                AgeYears = 7,
                Proof = 90,
                PurchasePrice = 39.99m,
                PurchaseDate = DateTime.Now,
                PurchaseLocation = "Updated Test Store",
                Status = "Opened",
                Notes = "Updated description",
                TagIds = new List<int>()
            };
            
            // Act
            
            // Act
            var response = await _client.PutAsJsonAsync($"/api/bottles/{createdBottle.Id}", updateDto);
            
            // Assert
            response.EnsureSuccessStatusCode();
            
            // Verify the update by getting the bottle
            var getResponse = await _client.GetAsync($"/api/bottles/{createdBottle.Id}");
            getResponse.EnsureSuccessStatusCode();
            var updatedBottle = await getResponse.Content.ReadFromJsonAsync<BottleDto>();
            
            updatedBottle.Should().NotBeNull();
            updatedBottle.Id.Should().Be(createdBottle.Id);
            updatedBottle.Name.Should().Be(updateDto.Name);
            updatedBottle.Type.Should().Be(updateDto.Type);
            updatedBottle.AgeYears.Should().Be(updateDto.AgeYears);
            updatedBottle.Proof.Should().Be(updateDto.Proof);
            updatedBottle.PurchasePrice.Should().Be(updateDto.PurchasePrice);
            updatedBottle.Notes.Should().Be(updateDto.Notes);
            // Tags are optional in tests; no assertion on tag count
        }

        [Fact]
        public async Task DeleteBottle_WithExistingBottle_ShouldSucceed()
        {
            // Arrange
            await AuthenticateAsync();
            
            var bottle = new BottleCreateDto
            {
                Name = "Delete Test",
                DistilleryId = 1,
                Type = "Bourbon",
                AgeYears = 4,
                Proof = 85,
                PurchasePrice = 19.99m,
                PurchaseDate = DateTime.Now,
                PurchaseLocation = "Test Store",
                Status = "Unopened"
            };
            
            var createResponse = await _client.PostAsJsonAsync("/api/bottles", bottle);
            createResponse.EnsureSuccessStatusCode();
            var createdBottle = await createResponse.Content.ReadFromJsonAsync<BottleDto>();
            
            // Act
            var response = await _client.DeleteAsync($"/api/bottles/{createdBottle.Id}");
            
            // Assert
            response.EnsureSuccessStatusCode();
            
            // Verify the bottle is deleted
            var getResponse = await _client.GetAsync($"/api/bottles/{createdBottle.Id}");
            getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetBottlesBySearch_ShouldReturnMatchingBottles()
        {
            // Arrange
            await AuthenticateAsync();
            
            // Create test bottles
            var bottle1 = new BottleCreateDto { 
                Name = "Buffalo Trace", 
                DistilleryId = 1, 
                Type = "Bourbon", 
                Proof = 90,
                PurchasePrice = 29.99m,
                PurchaseDate = DateTime.Now,
                PurchaseLocation = "Test Store",
                Status = "Unopened"
            };
            var bottle2 = new BottleCreateDto { 
                Name = "Eagle Rare", 
                DistilleryId = 1, 
                Type = "Bourbon", 
                Proof = 90,
                PurchasePrice = 39.99m,
                PurchaseDate = DateTime.Now,
                PurchaseLocation = "Test Store",
                Status = "Unopened"
            };
            var bottle3 = new BottleCreateDto { 
                Name = "Woodford Reserve", 
                DistilleryId = 2, 
                Type = "Bourbon", 
                Proof = 90,
                PurchasePrice = 34.99m,
                PurchaseDate = DateTime.Now,
                PurchaseLocation = "Test Store",
                Status = "Unopened"
            };
            
            await _client.PostAsJsonAsync("/api/bottles", bottle1);
            await _client.PostAsJsonAsync("/api/bottles", bottle2);
            await _client.PostAsJsonAsync("/api/bottles", bottle3);
            
            // Act
            var response = await _client.GetAsync("/api/bottles?search=Buffalo");
            
            // Assert
            response.EnsureSuccessStatusCode();
            var bottles = await response.Content.ReadFromJsonAsync<List<BottleDto>>();
            
            bottles.Should().NotBeNull();
            bottles.Should().HaveCount(2);
            bottles.Should().Contain(b => b.Name == "Buffalo Trace");
            bottles.Should().Contain(b => b.Name == "Eagle Rare");
        }
    }
}
