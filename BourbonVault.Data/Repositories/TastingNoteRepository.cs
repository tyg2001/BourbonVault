using BourbonVault.Core.Models;
using BourbonVault.Core.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BourbonVault.Data.Repositories
{
    public class TastingNoteRepository : Repository<TastingNote>, ITastingNoteRepository
    {
        public TastingNoteRepository(BourbonVaultContext context) : base(context)
        {
        }

        public async Task<IEnumerable<TastingNote>> GetTastingNotesByUserIdAsync(string userId)
        {
            return await _context.TastingNotes
                .Include(tn => tn.Bottle)
                    .ThenInclude(b => b.Distillery)
                .Where(tn => tn.UserId == userId)
                .OrderByDescending(tn => tn.TastingDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<TastingNote>> GetTastingNotesByBottleIdAsync(int bottleId)
        {
            return await _context.TastingNotes
                .Include(tn => tn.Bottle)
                    .ThenInclude(b => b.Distillery)
                .Where(tn => tn.BottleId == bottleId)
                .OrderByDescending(tn => tn.TastingDate)
                .ToListAsync();
        }

        public async Task<TastingNote> GetTastingNoteWithDetailsAsync(int id)
        {
            return await _context.TastingNotes
                .Include(tn => tn.Bottle)
                    .ThenInclude(b => b.Distillery)
                .FirstOrDefaultAsync(tn => tn.Id == id);
        }

        public async Task<IEnumerable<TastingNote>> GetPublicTastingNotesAsync(int pageNumber = 1, int pageSize = 10)
        {
            return await _context.TastingNotes
                .Include(tn => tn.Bottle)
                    .ThenInclude(b => b.Distillery)
                .Where(tn => tn.IsPublic)
                .OrderByDescending(tn => tn.TastingDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
    }
}
