using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NovelExtractor
{
    // Responsible for reading the novel file.
    public class NovelFileReader
    {
        private readonly string _filePath;

        public NovelFileReader(string novelPath, string fileName)
        {
            _filePath = Path.Combine(novelPath, fileName);
        }

        public IEnumerable<string> ReadLines()
        {
            using (StreamReader reader = new StreamReader(_filePath))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    yield return line;
                }
            }
        }
    }
}
