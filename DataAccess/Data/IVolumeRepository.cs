using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Interfaces
{
    public interface IVolumeRepository
    {
        Task<IEnumerable<VolumeModel>> GetByNovelIdAsync(int novelId);
        Task<VolumeModel?> GetByVolumeNumberAsync(int volumeNumber);
        Task<int> InsertAsync(VolumeModel volume);
        Task<int> UpdateAsync(VolumeModel volume);
        Task<int> DeleteAsync(int id);
    }

}
