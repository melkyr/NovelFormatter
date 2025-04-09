using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Mysqlx.Notice.Warning.Types;

namespace DataAccess.Interfaces
{
    public interface INovelRepository
    {
        Task<IEnumerable<NovelModel>> GetAllAsync();
        Task<NovelModel> GetByIdAsync(int id);
        Task<int> InsertAsync(NovelModel novel);
        Task<int> UpdateAsync(NovelModel novel);
        Task<int> DeleteAsync(int id);
    }

}
