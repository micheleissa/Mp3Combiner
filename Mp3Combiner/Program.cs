using NAudio.Wave;
using System;
using System.IO;
using System.Linq;

namespace Mp3Combiner
{
    class Program
    {
        static void Main(string[] args)
        {
            Spinner spinner = null;
            var lineIndex = 2;
            try
            {
                while (true)
                {
                    spinner = new Spinner(0, lineIndex);
                    Console.Write("Enter the folder path, enter 'e' to exit, 'c' to clear the screen : ");
                    var line = args?.Length > 0 ? args[0] : Console.ReadLine();
                    if (string.Equals(line, "e", StringComparison.CurrentCultureIgnoreCase))
                        break;
                    if (string.Equals(line, "c", StringComparison.CurrentCultureIgnoreCase))
                    {
                        Console.Clear();
                        lineIndex = 2;
                        continue;
                    }
                    if (!Directory.Exists(line))
                    {
                        Console.WriteLine("Path does not exist");
                        args = null;
                        continue;
                    }
                    var files = Directory.GetFiles(line, "*.mp3");
                    if (!files.Any())
                    {
                        Console.WriteLine("Path does not contain any .mp3 files.");
                        args = null;
                        continue;
                    }

                    var resultFile = $@"{line}/Output/combined.mp3";
                    if (!Directory.Exists($@"{line}/Output"))
                        Directory.CreateDirectory($@"{line}/Output");

                    spinner.Start();
                    using (var fileStream = new FileStream(resultFile, FileMode.Create))
                    {
                        Combine(files, fileStream);
                    }
                    spinner.Stop();
                    Console.WriteLine("Done.");
                    lineIndex += 3;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception has happened: {ex.Message}");
                if (spinner != null)
                    spinner.Stop();
            }
        }

        public static void Combine(string[] inputFiles, Stream output)
        {
            foreach (var file in inputFiles)
            {
                using (var reader = new Mp3FileReader(file))
                {
                    if ((output.Position == 0) && (reader.Id3v2Tag != null))
                    {
                        output.Write(reader.Id3v2Tag.RawData, 0, reader.Id3v2Tag.RawData.Length);
                    }
                    Mp3Frame frame;
                    while ((frame = reader.ReadNextFrame()) != null)
                    {
                        output.Write(frame.RawData, 0, frame.RawData.Length);
                    }
                }

            }
        }
    }
}
