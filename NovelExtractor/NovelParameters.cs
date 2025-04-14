using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NovelExtractor
{
    public  class NovelParameters
    {
        //Sample data
        public string NovelPath = "F:\\Arankparty";
        public  string NovelFileName = "novelSpaCand.txt";
        public string OutputDirectory = "F:\\Arankparty\\output";
        public  string ChapterSplitterText = "adventureworkstl.fyi";
        public string ChapterEndingTextPattern = @"^#{4,}";
        public  int StartChaptersAt = 1;
        public int StartVolumeAt = 1;
        public string VolumePattern = @"^VOLUME \d+";
        public string ChaptersNamePattern = "Chapter";
    }
}
