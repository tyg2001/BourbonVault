using BourbonVault.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BourbonVault.Core.Repositories
{
    public interface ITastingNoteRepository : IRepository<TastingNote>
    {
        Task<IEnumerable<TastingNote>> GetTastingNotesByUserIdAsync(string userId);
        Task<IEnumerable<TastingNote>> GetTastingNotesByBottleIdAsync(int bottleId);
        Task<TastingNote> GetTastingNoteWithDetailsAsync(int id);
        Task<IEnumerable<TastingNote>> GetPublicTastingNotesAsync(int pageNumber = 1, int pageSize = 10);
    }
}
