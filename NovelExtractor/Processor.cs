using NovelExtractor.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NovelExtractor
{
    // Coordinates the overall processing.
    public class Processor
    {
        private readonly NovelParameters _parameters;
        private readonly SplitterStatus _status;
        private readonly NovelFileReader _reader;
        private readonly NovelFileWriter _writer;
        private readonly ChapterExtractor _extractor;
        private readonly ChapterMessageProducer _messageProducer;

        public Processor(NovelParameters parameters, SplitterStatus status)
        {
            _parameters = parameters;
            _status = status;
            _reader = new NovelFileReader(_parameters.NovelPath, _parameters.NovelFileName);
            _writer = new NovelFileWriter(_parameters.OutputDirectory);
            _extractor = new ChapterExtractor(_parameters, _status);
            _messageProducer = new ChapterMessageProducer("localhost", "novel_processor_queue");
        }

        public async Task ProcessText()
        {
            Console.WriteLine("Starting processing...");
            foreach (var line in _reader.ReadLines())
            {
                _extractor.ProcessLine(line);

                switch (_status.CurrentStatus)
                {
                    case SplitterStatus.Status.StartingChapter:
                        _status.CurrentFileName = $"VOL{_status.CurrentVolume}_{_parameters.ChaptersNamePattern}_{_status.CurrentChapter}.txt";
                        _status.CurrentStatus = SplitterStatus.Status.ReadingChapter;
                        break;

                    case SplitterStatus.Status.EndingChapter:
                        string chapterContent = _extractor.GetChapterContent();
                        Console.WriteLine($"Finishing chapter {_status.CurrentChapter}");
                        _writer.WriteFile(_status.CurrentFileName, chapterContent);
                        await _messageProducer.PublishChapter(_status.CurrentVolume, _status.CurrentChapter, chapterContent);
                        _status.CurrentStatus = SplitterStatus.Status.Idle;
                        _extractor.ClearChapterContent();
                        break;

                    // You can add additional logic for StartingVolume or others if needed.
                    default:
                        break;
                }
            }
        }
    }
}
