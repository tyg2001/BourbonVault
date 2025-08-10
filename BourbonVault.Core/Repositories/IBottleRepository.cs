using BourbonVault.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BourbonVault.Core.Repositories
{
    public interface IBottleRepository : IRepository<Bottle>
    {
        Task<IEnumerable<Bottle>> GetBottlesByUserIdAsync(string userId);
        Task<Bottle> GetBottleWithDetailsAsync(int id);
        Task<IEnumerable<Bottle>> GetBottlesByDistilleryIdAsync(int distilleryId);
        Task<IEnumerable<Bottle>> GetBottlesByTagIdAsync(int tagId);
        Task UpdateBottleTagsAsync(int bottleId, IEnumerable<int> tagIds);
    }
}
