using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NovelExtractor
{
    public class NovelFileWriter
    {
        private readonly string _outputDirectory;

        // Responsible for writing text to a file.
        public NovelFileWriter(string outputDirectory)
        {
            _outputDirectory = outputDirectory;
        }

        public void WriteFile(string fileName, string text)
        {
            string filePath = Path.Combine(_outputDirectory, fileName);
            if (!Directory.Exists(_outputDirectory))
            {
                Console.WriteLine("Creating output directory.");
                Directory.CreateDirectory(_outputDirectory);
            }
            try
            {
                Console.WriteLine($"Writing to {filePath}");
                File.WriteAllText(filePath, text);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
            }
        }
    }
}
