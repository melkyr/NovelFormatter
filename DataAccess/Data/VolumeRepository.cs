using Dapper;
using DataAccess.Interfaces;
using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Data
{
    public class VolumeRepository : IVolumeRepository
    {
        private readonly DbConnectionFactory _dbFactory;

        public VolumeRepository(DbConnectionFactory dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public async Task<IEnumerable<VolumeModel>> GetByNovelIdAsync(int novelId)
        {
            using var db = _dbFactory.CreateConnection();
            return await db.QueryAsync<VolumeModel>("SELECT * FROM volumes WHERE novel_id = @NovelId", new { NovelId = novelId });
        }

        public async Task<VolumeModel?> GetByVolumeNumberAsync(int volumeNumber)
        {
            using var db = _dbFactory.CreateConnection();
            return await db.QueryFirstOrDefaultAsync<VolumeModel>("SELECT * FROM volumes WHERE volume_number = @volume_number", new { Volume_Number = volumeNumber });
        }

        public async Task<int> InsertAsync(VolumeModel volume)
        {
            using var db = _dbFactory.CreateConnection();
            string query = "INSERT INTO volumes (novel_id, title, volume_number, description) VALUES (@Novel_Id, @Title, @Volume_Number, @Description); SELECT LAST_INSERT_ID();";
            return await db.ExecuteScalarAsync<int>(query, volume);
        }

        public async Task<int> UpdateAsync(VolumeModel volume)
        {
            using var db = _dbFactory.CreateConnection();
            string query = "UPDATE volumes SET title = @Title, volume_number = @VolumeNumber, description = @Description WHERE volume_id = @VolumeId";
            return await db.ExecuteAsync(query, volume);
        }

        public async Task<int> DeleteAsync(int id)
        {
            using var db = _dbFactory.CreateConnection();
            return await db.ExecuteAsync("DELETE FROM volumes WHERE volume_id = @Id", new { Id = id });
        }
    }
}
