using BourbonVault.Core.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BourbonVault.Web.Services
{
    public interface IBottleService
    {
        Task<IEnumerable<BottleDto>> GetAllBottlesAsync();
        Task<BottleDto> GetBottleByIdAsync(int id);
        Task<BottleDto> CreateBottleAsync(BottleCreateDto bottleDto);
        Task<bool> UpdateBottleAsync(BottleUpdateDto bottleDto);
        Task<bool> DeleteBottleAsync(int id);
    }
}
