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
    public class ChapterRepository : IChapterRepository
    {
        private readonly DbConnectionFactory _dbFactory;

        public ChapterRepository(DbConnectionFactory dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public async Task<IEnumerable<ChapterModel>> GetByNovelIdAsync(int novelId)
        {
            using var db = _dbFactory.CreateConnection();
            return await db.QueryAsync<ChapterModel>("SELECT * FROM chapters WHERE novel_id = @NovelId", new { NovelId = novelId });
        }

        public async Task<IEnumerable<ChapterModel>> GetByVolumeIdAsync(int volumeId)
        {
            using var db = _dbFactory.CreateConnection();
            return await db.QueryAsync<ChapterModel>("SELECT * FROM chapters WHERE volume_id = @VolumeId", new { VolumeId = volumeId });
        }

        public async Task<int> InsertAsync(ChapterModel chapter)
        {
            using var db = _dbFactory.CreateConnection();
            string query = "INSERT INTO chapters (novel_id, volume_id, chapter_number, title, content_plain, content_bulma, content_html) VALUES (@NovelId, @VolumeId, @ChapterNumber, @Title, @ContentPlain, @ContentBulma, @ContentHtml)";
            return await db.ExecuteAsync(query, chapter);
        }

        public async Task<int> UpdateAsync(ChapterModel chapter)
        {
            using var db = _dbFactory.CreateConnection();
            string query = "UPDATE chapters SET chapter_number = @ChapterNumber, title = @Title, content_plain = @ContentPlain, content_bulma = @ContentBulma, content_html = @ContentHtml WHERE chapter_id = @ChapterId";
            return await db.ExecuteAsync(query, chapter);
        }

        public async Task<int> DeleteAsync(int id)
        {
            using var db = _dbFactory.CreateConnection();
            return await db.ExecuteAsync("DELETE FROM chapters WHERE chapter_id = @Id", new { Id = id });
        }
    }
}
