using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BourbonVault.Core.Models;
using BourbonVault.Core.Repositories;
using BourbonVault.Core.DTOs;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace BourbonVault.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class BottlesController : ControllerBase
    {
        private readonly IBottleRepository _bottleRepository;
        private readonly ILogger<BottlesController> _logger;
        
        // In a real application, we would use AutoMapper for mapping between entities and DTOs
        // For simplicity, we're doing manual mapping here

        public BottlesController(IBottleRepository bottleRepository, ILogger<BottlesController> logger)
        {
            _bottleRepository = bottleRepository;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<BottleDto>>> GetBottles([FromQuery] string search = null)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }
                
                var bottles = await _bottleRepository.GetBottlesByUserIdAsync(userId);
                if (!string.IsNullOrWhiteSpace(search))
                {
                    var term = search.Trim();
                    bottles = bottles.Where(b =>
                        (!string.IsNullOrEmpty(b.Name) && b.Name.Contains(term, StringComparison.OrdinalIgnoreCase)) ||
                        (b.Distillery != null && !string.IsNullOrEmpty(b.Distillery.Name) && b.Distillery.Name.Contains(term, StringComparison.OrdinalIgnoreCase))
                    );
                }
                var bottleDtos = bottles.Select(MapBottleToDto).ToList();
                
                return Ok(bottleDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving bottles");
                return StatusCode(500, "An error occurred while retrieving bottles");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<BottleDto>> GetBottle(int id)
        {
            try
            {
                var bottle = await _bottleRepository.GetBottleWithDetailsAsync(id);
                
                if (bottle == null)
                {
                    return NotFound($"Bottle with ID {id} not found");
                }

                // In a real application, we would check if the bottle belongs to the current user
                // For demo purposes, we'll skip that check

                return Ok(MapBottleToDto(bottle));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving bottle {id}");
                return StatusCode(500, $"An error occurred while retrieving bottle {id}");
            }
        }

        [HttpPost]
        public async Task<ActionResult<BottleDto>> CreateBottle(BottleCreateDto bottleCreateDto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("CreateBottle ModelState invalid: {Errors}", string.Join(" | ", ModelState.Select(kvp => $"{kvp.Key}: {string.Join(", ", kvp.Value.Errors.Select(e => e.ErrorMessage))}")));
                return ValidationProblem(ModelState);
            }

            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }
                
                var bottle = new Bottle
                {
                    Name = bottleCreateDto.Name,
                    DistilleryId = bottleCreateDto.DistilleryId,
                    Type = bottleCreateDto.Type,
                    AgeYears = bottleCreateDto.AgeYears,
                    Proof = bottleCreateDto.Proof,
                    PurchasePrice = bottleCreateDto.PurchasePrice ?? 0m,
                    PurchaseDate = bottleCreateDto.PurchaseDate,
                    PurchaseLocation = bottleCreateDto.PurchaseLocation,
                    CurrentEstimatedValue = bottleCreateDto.CurrentEstimatedValue,
                    Status = bottleCreateDto.Status,
                    Notes = bottleCreateDto.Notes,
                    ImageUrl = bottleCreateDto.ImageUrl,
                    UserId = userId
                };

                await _bottleRepository.AddAsync(bottle);
                await _bottleRepository.SaveChangesAsync();
                
                // If tags are provided, update the bottle tags
                if (bottleCreateDto.TagIds != null && bottleCreateDto.TagIds.Any())
                {
                    await _bottleRepository.UpdateBottleTagsAsync(bottle.Id, bottleCreateDto.TagIds);
                }
                
                // Reload the bottle with its relationships
                var createdBottle = await _bottleRepository.GetBottleWithDetailsAsync(bottle.Id);
                
                return CreatedAtAction(
                    nameof(GetBottle), 
                    new { id = createdBottle.Id }, 
                    MapBottleToDto(createdBottle));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating bottle");
                return StatusCode(500, "An error occurred while creating the bottle");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBottle(int id, BottleUpdateDto bottleUpdateDto)
        {
            if (id != bottleUpdateDto.Id)
            {
                _logger.LogWarning("UpdateBottle ID mismatch: route {RouteId} vs body {BodyId}", id, bottleUpdateDto.Id);
                return BadRequest("ID mismatch");
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("UpdateBottle ModelState invalid: {Errors}", string.Join(" | ", ModelState.Select(kvp => $"{kvp.Key}: {string.Join(", ", kvp.Value.Errors.Select(e => e.ErrorMessage))}")));
                return ValidationProblem(ModelState);
            }

            try
            {
                var existingBottle = await _bottleRepository.GetByIdAsync(id);
                
                if (existingBottle == null)
                {
                    return NotFound($"Bottle with ID {id} not found");
                }
                
                // In a real application, we would check if the bottle belongs to the current user
                // For demo purposes, we'll skip that check
                
                // Update bottle properties
                existingBottle.Name = bottleUpdateDto.Name;
                existingBottle.DistilleryId = bottleUpdateDto.DistilleryId;
                existingBottle.Type = bottleUpdateDto.Type;
                existingBottle.AgeYears = bottleUpdateDto.AgeYears;
                existingBottle.Proof = bottleUpdateDto.Proof;
                existingBottle.PurchasePrice = bottleUpdateDto.PurchasePrice ?? 0m;
                existingBottle.PurchaseDate = bottleUpdateDto.PurchaseDate;
                existingBottle.PurchaseLocation = bottleUpdateDto.PurchaseLocation;
                existingBottle.CurrentEstimatedValue = bottleUpdateDto.CurrentEstimatedValue;
                existingBottle.Status = bottleUpdateDto.Status;
                existingBottle.Notes = bottleUpdateDto.Notes;
                existingBottle.ImageUrl = bottleUpdateDto.ImageUrl;
                
                _bottleRepository.Update(existingBottle);
                await _bottleRepository.SaveChangesAsync();
                
                // Update bottle tags if provided
                if (bottleUpdateDto.TagIds != null)
                {
                    await _bottleRepository.UpdateBottleTagsAsync(id, bottleUpdateDto.TagIds);
                }
                
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating bottle {id}");
                return StatusCode(500, $"An error occurred while updating bottle {id}");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBottle(int id)
        {
            try
            {
                var bottle = await _bottleRepository.GetByIdAsync(id);
                
                if (bottle == null)
                {
                    return NotFound($"Bottle with ID {id} not found");
                }
                
                // In a real application, we would check if the bottle belongs to the current user
                // For demo purposes, we'll skip that check
                
                _bottleRepository.Remove(bottle);
                await _bottleRepository.SaveChangesAsync();
                
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting bottle {id}");
                return StatusCode(500, $"An error occurred while deleting bottle {id}");
            }
        }
        
        // Helper method for mapping Bottle entity to BottleDto
        private BottleDto MapBottleToDto(Bottle bottle)
        {
            if (bottle == null)
                return null;
                
            return new BottleDto
            {
                Id = bottle.Id,
                Name = bottle.Name,
                DistilleryId = bottle.DistilleryId,
                DistilleryName = bottle.Distillery?.Name,
                Type = bottle.Type,
                AgeYears = bottle.AgeYears,
                Proof = bottle.Proof,
                PurchasePrice = bottle.PurchasePrice,
                PurchaseDate = bottle.PurchaseDate,
                PurchaseLocation = bottle.PurchaseLocation,
                CurrentEstimatedValue = bottle.CurrentEstimatedValue,
                Status = bottle.Status,
                Notes = bottle.Notes,
                ImageUrl = bottle.ImageUrl,
                Tags = bottle.BottleTags?.Select(bt => bt.Tag?.Name)?.Where(n => n != null)?.ToList() ?? new List<string>(),
                TastingNotesCount = bottle.TastingNotes?.Count ?? 0,
                AverageRating = bottle.TastingNotes?.Any() == true 
                    ? bottle.TastingNotes.Average(tn => tn.OverallRating) 
                    : null
            };
        }
    }
}
