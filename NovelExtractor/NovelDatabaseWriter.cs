using DataAccess.Data;
using DataAccess.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NovelExtractor
{
    public class NovelDatabaseWriter
    {
        public readonly IChapterRepository _repository;

        public NovelDatabaseWriter(IChapterRepository repository)
        {
            _repository = repository;
        }

        public bool PlainTextWriter(string text, string title, int chapterId, int volumeId)
        {

            return true;
        }
    }
       
}
