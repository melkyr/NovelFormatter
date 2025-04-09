using Dapper;
using DataAccess.Interfaces;
using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Mysqlx.Notice.Warning.Types;

namespace DataAccess.Data
{
    public class NovelRepository : INovelRepository
    {
        private readonly DbConnectionFactory _dbFactory;

        public NovelRepository(DbConnectionFactory dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public async Task<IEnumerable<NovelModel>> GetAllAsync()
        {
            using var db = _dbFactory.CreateConnection();
            return await db.QueryAsync<NovelModel>("SELECT * FROM novels");
        }

        public async Task<NovelModel> GetByIdAsync(int id)
        {
            using var db = _dbFactory.CreateConnection();
            return await db.QueryFirstOrDefaultAsync<NovelModel>("SELECT * FROM novels WHERE novel_id = @Id", new { Id = id });
        }

        public async Task<int> InsertAsync(NovelModel novel)
        {
            using var db = _dbFactory.CreateConnection();
            string query = "INSERT INTO novels (name, author, status, description) VALUES (@Name, @Author, @Status, @Description)";
            return await db.ExecuteAsync(query, novel);
        }

        public async Task<int> UpdateAsync(NovelModel novel)
        {
            using var db = _dbFactory.CreateConnection();
            string query = "UPDATE novels SET name = @Name, author = @Author, status = @Status, description = @Description WHERE novel_id = @NovelId";
            return await db.ExecuteAsync(query, novel);
        }

        public async Task<int> DeleteAsync(int id)
        {
            using var db = _dbFactory.CreateConnection();
            return await db.ExecuteAsync("DELETE FROM novels WHERE novel_id = @Id", new { Id = id });
        }
    }
}
