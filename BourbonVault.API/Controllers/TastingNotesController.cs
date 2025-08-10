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
    public class TastingNotesController : ControllerBase
    {
        private readonly ITastingNoteRepository _tastingNoteRepository;
        private readonly IBottleRepository _bottleRepository;
        private readonly ILogger<TastingNotesController> _logger;

        public TastingNotesController(
            ITastingNoteRepository tastingNoteRepository, 
            IBottleRepository bottleRepository,
            ILogger<TastingNotesController> logger)
        {
            _tastingNoteRepository = tastingNoteRepository;
            _bottleRepository = bottleRepository;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TastingNoteDto>>> GetTastingNotes()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }
                
                var tastingNotes = await _tastingNoteRepository.GetTastingNotesByUserIdAsync(userId);
                var tastingNoteDtos = tastingNotes.Select(MapTastingNoteToDto).ToList();
                
                return Ok(tastingNoteDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving tasting notes");
                return StatusCode(500, "An error occurred while retrieving tasting notes");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TastingNoteDto>> GetTastingNote(int id)
        {
            try
            {
                var tastingNote = await _tastingNoteRepository.GetTastingNoteWithDetailsAsync(id);
                
                if (tastingNote == null)
                {
                    return NotFound($"Tasting note with ID {id} not found");
                }

                // In a real application, we would check if the tasting note belongs to the current user
                // or is public, before returning it
                
                return Ok(MapTastingNoteToDto(tastingNote));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving tasting note {id}");
                return StatusCode(500, $"An error occurred while retrieving tasting note {id}");
            }
        }

        [HttpGet("bottle/{bottleId}")]
        public async Task<ActionResult<IEnumerable<TastingNoteDto>>> GetTastingNotesByBottle(int bottleId)
        {
            try
            {
                // Verify the bottle exists and belongs to the current user
                var bottle = await _bottleRepository.GetByIdAsync(bottleId);
                
                if (bottle == null)
                {
                    return NotFound($"Bottle with ID {bottleId} not found");
                }
                
                // In a real application, we would check if the bottle belongs to the current user
                
                var tastingNotes = await _tastingNoteRepository.GetTastingNotesByBottleIdAsync(bottleId);
                var tastingNoteDtos = tastingNotes.Select(MapTastingNoteToDto).ToList();
                
                return Ok(tastingNoteDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving tasting notes for bottle {bottleId}");
                return StatusCode(500, $"An error occurred while retrieving tasting notes for bottle {bottleId}");
            }
        }

        [HttpGet("public")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<TastingNoteDto>>> GetPublicTastingNotes([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var tastingNotes = await _tastingNoteRepository.GetPublicTastingNotesAsync(page, pageSize);
                var tastingNoteDtos = tastingNotes.Select(MapTastingNoteToDto).ToList();
                
                return Ok(tastingNoteDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving public tasting notes");
                return StatusCode(500, "An error occurred while retrieving public tasting notes");
            }
        }

        [HttpPost]
        public async Task<ActionResult<TastingNoteDto>> CreateTastingNote(TastingNoteCreateDto tastingNoteCreateDto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("CreateTastingNote ModelState invalid: {Errors}", string.Join(" | ", ModelState.Select(kvp => $"{kvp.Key}: {string.Join(", ", kvp.Value.Errors.Select(e => e.ErrorMessage))}")));
                return ValidationProblem(ModelState);
            }

            try
            {
                // Verify the bottle exists
                var bottle = await _bottleRepository.GetByIdAsync(tastingNoteCreateDto.BottleId);
                
                if (bottle == null)
                {
                    return BadRequest($"Bottle with ID {tastingNoteCreateDto.BottleId} not found");
                }
                
                // In a real application, we would check if the bottle belongs to the current user
                
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }
                
                var tastingNote = new TastingNote
                {
                    BottleId = tastingNoteCreateDto.BottleId,
                    UserId = userId,
                    TastingDate = tastingNoteCreateDto.TastingDate,
                    AppearanceRating = tastingNoteCreateDto.AppearanceRating,
                    NoseRating = tastingNoteCreateDto.NoseRating,
                    TasteRating = tastingNoteCreateDto.TasteRating,
                    FinishRating = tastingNoteCreateDto.FinishRating,
                    OverallRating = tastingNoteCreateDto.OverallRating,
                    AppearanceNotes = tastingNoteCreateDto.AppearanceNotes,
                    NoseNotes = tastingNoteCreateDto.NoseNotes,
                    TasteNotes = tastingNoteCreateDto.TasteNotes,
                    FinishNotes = tastingNoteCreateDto.FinishNotes,
                    AdditionalNotes = tastingNoteCreateDto.AdditionalNotes,
                    IsPublic = tastingNoteCreateDto.IsPublic,
                    ImageUrl = tastingNoteCreateDto.ImageUrl
                };
                
                await _tastingNoteRepository.AddAsync(tastingNote);
                await _tastingNoteRepository.SaveChangesAsync();
                
                // Reload the tasting note with its relationships
                var createdTastingNote = await _tastingNoteRepository.GetTastingNoteWithDetailsAsync(tastingNote.Id);
                
                return CreatedAtAction(
                    nameof(GetTastingNote), 
                    new { id = createdTastingNote.Id }, 
                    MapTastingNoteToDto(createdTastingNote));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating tasting note");
                return StatusCode(500, "An error occurred while creating the tasting note");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTastingNote(int id, TastingNoteUpdateDto tastingNoteUpdateDto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("UpdateTastingNote ModelState invalid: {Errors}", string.Join(" | ", ModelState.Select(kvp => $"{kvp.Key}: {string.Join(", ", kvp.Value.Errors.Select(e => e.ErrorMessage))}")));
                return ValidationProblem(ModelState);
            }

            try
            {
                var existingTastingNote = await _tastingNoteRepository.GetByIdAsync(id);
                
                if (existingTastingNote == null)
                {
                    return NotFound($"Tasting note with ID {id} not found");
                }
                
                // In a real application, we would check if the tasting note belongs to the current user
                
                // Update tasting note properties
                existingTastingNote.TastingDate = tastingNoteUpdateDto.TastingDate;
                existingTastingNote.AppearanceRating = tastingNoteUpdateDto.AppearanceRating;
                existingTastingNote.NoseRating = tastingNoteUpdateDto.NoseRating;
                existingTastingNote.TasteRating = tastingNoteUpdateDto.TasteRating;
                existingTastingNote.FinishRating = tastingNoteUpdateDto.FinishRating;
                existingTastingNote.OverallRating = tastingNoteUpdateDto.OverallRating;
                existingTastingNote.AppearanceNotes = tastingNoteUpdateDto.AppearanceNotes;
                existingTastingNote.NoseNotes = tastingNoteUpdateDto.NoseNotes;
                existingTastingNote.TasteNotes = tastingNoteUpdateDto.TasteNotes;
                existingTastingNote.FinishNotes = tastingNoteUpdateDto.FinishNotes;
                existingTastingNote.AdditionalNotes = tastingNoteUpdateDto.AdditionalNotes;
                existingTastingNote.IsPublic = tastingNoteUpdateDto.IsPublic;
                existingTastingNote.ImageUrl = tastingNoteUpdateDto.ImageUrl;
                
                _tastingNoteRepository.Update(existingTastingNote);
                await _tastingNoteRepository.SaveChangesAsync();
                
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating tasting note {id}");
                return StatusCode(500, $"An error occurred while updating tasting note {id}");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTastingNote(int id)
        {
            try
            {
                var tastingNote = await _tastingNoteRepository.GetByIdAsync(id);
                
                if (tastingNote == null)
                {
                    return NotFound($"Tasting note with ID {id} not found");
                }
                
                // In a real application, we would check if the tasting note belongs to the current user
                
                _tastingNoteRepository.Remove(tastingNote);
                await _tastingNoteRepository.SaveChangesAsync();
                
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting tasting note {id}");
                return StatusCode(500, $"An error occurred while deleting tasting note {id}");
            }
        }
        
        // Helper method for mapping TastingNote entity to TastingNoteDto
        private TastingNoteDto MapTastingNoteToDto(TastingNote tastingNote)
        {
            if (tastingNote == null)
                return null;
                
            return new TastingNoteDto
            {
                Id = tastingNote.Id,
                BottleId = tastingNote.BottleId,
                BottleName = tastingNote.Bottle?.Name,
                DistilleryName = tastingNote.Bottle?.Distillery?.Name,
                TastingDate = tastingNote.TastingDate,
                AppearanceRating = tastingNote.AppearanceRating,
                NoseRating = tastingNote.NoseRating,
                TasteRating = tastingNote.TasteRating,
                FinishRating = tastingNote.FinishRating,
                OverallRating = tastingNote.OverallRating,
                AppearanceNotes = tastingNote.AppearanceNotes,
                NoseNotes = tastingNote.NoseNotes,
                TasteNotes = tastingNote.TasteNotes,
                FinishNotes = tastingNote.FinishNotes,
                AdditionalNotes = tastingNote.AdditionalNotes,
                IsPublic = tastingNote.IsPublic,
                ImageUrl = tastingNote.ImageUrl
            };
        }
    }
}
