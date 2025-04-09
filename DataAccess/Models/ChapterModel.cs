using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Models
{
    public class ChapterModel
    {
        public int ChapterId { get; set; }
        public int NovelId { get; set; }
        public int? VolumeId { get; set; }
        public int ChapterNumber { get; set; }
        public string Title { get; set; }
        public string ContentPlain { get; set; }
        public string ContentBulma { get; set; }
        public string ContentHtml { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
