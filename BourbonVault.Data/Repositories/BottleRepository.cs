using BourbonVault.Core.Models;
using BourbonVault.Core.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BourbonVault.Data.Repositories
{
    public class BottleRepository : Repository<Bottle>, IBottleRepository
    {
        public BottleRepository(BourbonVaultContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Bottle>> GetBottlesByUserIdAsync(string userId)
        {
            return await _context.Bottles
                .Include(b => b.Distillery)
                .Include(b => b.BottleTags)
                    .ThenInclude(bt => bt.Tag)
                .Where(b => b.UserId == userId)
                .ToListAsync();
        }

        public async Task<Bottle> GetBottleWithDetailsAsync(int id)
        {
            return await _context.Bottles
                .Include(b => b.Distillery)
                .Include(b => b.BottleTags)
                    .ThenInclude(bt => bt.Tag)
                .Include(b => b.TastingNotes)
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task<IEnumerable<Bottle>> GetBottlesByDistilleryIdAsync(int distilleryId)
        {
            return await _context.Bottles
                .Include(b => b.Distillery)
                .Include(b => b.BottleTags)
                    .ThenInclude(bt => bt.Tag)
                .Where(b => b.DistilleryId == distilleryId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Bottle>> GetBottlesByTagIdAsync(int tagId)
        {
            return await _context.Bottles
                .Include(b => b.Distillery)
                .Include(b => b.BottleTags)
                    .ThenInclude(bt => bt.Tag)
                .Where(b => b.BottleTags.Any(bt => bt.TagId == tagId))
                .ToListAsync();
        }

        public async Task UpdateBottleTagsAsync(int bottleId, IEnumerable<int> tagIds)
        {
            var bottle = await _context.Bottles
                .Include(b => b.BottleTags)
                .FirstOrDefaultAsync(b => b.Id == bottleId);

            if (bottle != null)
            {
                // Remove existing tags
                _context.BottleTags.RemoveRange(bottle.BottleTags);

                // Add new tags
                if (tagIds != null && tagIds.Any())
                {
                    foreach (var tagId in tagIds)
                    {
                        bottle.BottleTags.Add(new BottleTag
                        {
                            BottleId = bottleId,
                            TagId = tagId
                        });
                    }
                }

                await _context.SaveChangesAsync();
            }
        }
    }
}
