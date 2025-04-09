using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NovelExtractor
{
    // Processes each line of the text and detects volumes and chapters.
    public class ChapterExtractor
    {
        private readonly NovelParameters _parameters;
        private readonly SplitterStatus _status;
        private readonly StringBuilder _chapterContent = new StringBuilder();

        public ChapterExtractor(NovelParameters parameters, SplitterStatus status)
        {
            _parameters = parameters;
            _status = status;
        }

        // Process a single line and update status/content accordingly.
        public void ProcessLine(string line)
        {
            // 1. Check for chapter ending.
            if (Regex.IsMatch(line, _parameters.ChapterEndingTextPattern))
            {
                _status.CurrentStatus = SplitterStatus.Status.EndingChapter;
                _status.CurrentChapter++;
                _chapterContent.AppendLine(line);
                return;
            }

            // 2. Check for volume start.
            if (Regex.IsMatch(line, _parameters.VolumePattern))
            {
                _status.CurrentVolume++;
                _status.CurrentStatus = SplitterStatus.Status.StartingVolume;
                _status.CurrentChapter = _parameters.StartChaptersAt;
                _chapterContent.Clear();
                _chapterContent.AppendLine(line);
                Console.WriteLine($"Processing volume {_status.CurrentVolume}");
                return;
            }

            // 3. Check for chapter start.
            if (line.Contains(_parameters.ChapterSplitterText))
            {
                _status.CurrentStatus = SplitterStatus.Status.StartingChapter;
                _chapterContent.Clear();
                _chapterContent.AppendLine($"Chapter {_status.CurrentChapter}");
                Console.WriteLine($"Starting chapter {_status.CurrentChapter}");
                return;
            }

            // 4. Append line if within a chapter.
            if (_status.CurrentStatus == SplitterStatus.Status.ReadingChapter && !string.IsNullOrWhiteSpace(line))
            {
                _chapterContent.AppendLine(line);
            }
        }

        public string GetChapterContent()
        {
            return _chapterContent.ToString();
        }

        public void ClearChapterContent()
        {
            _chapterContent.Clear();
        }
    }
}
