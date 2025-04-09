using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NovelExtractor
{
    public  class NovelParameters
    {
        public string NovelPath = "F:\\Arankparty";
        public  string NovelFileName = "novel.txt";
        public string OutputDirectory = "F:\\Arankparty\\output";
        public  string ChapterSplitterText = "valhallatls.blogspot.com";
        public string ChapterEndingTextPattern = @"^-{5,}";
        public  int StartChaptersAt = 1;
        public int StartVolumeAt = 1;
        public string VolumePattern = @"^VOLUME \d+";
        public string ChaptersNamePattern = "Chapter";
    }
}
