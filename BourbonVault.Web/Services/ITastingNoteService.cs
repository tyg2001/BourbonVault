using BourbonVault.Core.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BourbonVault.Web.Services
{
    public interface ITastingNoteService
    {
        Task<IEnumerable<TastingNoteDto>> GetAllTastingNotesAsync();
        Task<IEnumerable<TastingNoteDto>> GetTastingNotesByBottleIdAsync(int bottleId);
        Task<TastingNoteDto> GetTastingNoteByIdAsync(int id);
        Task<TastingNoteDto> CreateTastingNoteAsync(TastingNoteCreateDto tastingNoteDto);
        Task<bool> UpdateTastingNoteAsync(TastingNoteUpdateDto tastingNoteDto);
        Task<bool> DeleteTastingNoteAsync(int id);
        Task<IEnumerable<TastingNoteDto>> GetPublicTastingNotesAsync(int page = 1, int pageSize = 10);
    }
}
