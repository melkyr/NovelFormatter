using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Interfaces
{
    public interface IChapterRepository
    {
        Task<IEnumerable<ChapterModel>> GetByNovelIdAsync(int novelId);
        Task<IEnumerable<ChapterModel>> GetByVolumeIdAsync(int volumeId);
        Task<int> InsertAsync(ChapterModel chapter);
        Task<int> UpdateAsync(ChapterModel chapter);
        Task<int> DeleteAsync(int id);
    }

}
