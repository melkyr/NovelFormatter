using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NovelExtractor
{
    public class SplitterStatus
    {
        public int CurrentChapter { get; set; }
        public int CurrentVolume { get; set; }
        public Status CurrentStatus { get; set; }
        public string CurrentFileName { get; set; }

        public enum Status {StartingChapter,ReadingChapter,EndingChapter,StartingVolume,Idle }
        
    }
    
}
