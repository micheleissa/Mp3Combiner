using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Mp3Combiner
{
    class Program
    {
        static void Main(string[] args)
        {
            Spinner spinner = null;
            var lineIndex = 3;
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
                        lineIndex = 3;
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
                    WriteMediaProperties(resultFile);
                    spinner.Stop();
                    Console.WriteLine("Done.");
                    lineIndex += 3;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception has happened: {ex.Message}");
                spinner?.Stop();
            }
        }

        public static void Combine(string[] inputFiles, Stream output)
        {
            foreach (var file in OrderFilesByMediaProperties(inputFiles))
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

        public static void WriteMediaProperties(string output)
        {
            var combined = TagLib.File.Create(output);
            combined.Tag.Track = 0;
            combined.Tag.Title = "Combined";
            combined.Save();
        }

        public static List<string> OrderFilesByMediaProperties(string[] unorderedFiles)
        {
            var orderedList = new List<string>();
            if (unorderedFiles == null || unorderedFiles.Length == 0)
                return orderedList;
            var tagFiles = unorderedFiles.Select(TagLib.File.Create).ToList();
            if (tagFiles.All(tf => tf.Tag.Track != 0))
            {
                tagFiles = tagFiles.OrderBy(tf => tf.Tag.Track).ToList();
                orderedList.AddRange(tagFiles.Select(tf => tf.Name));
            }
            else
            {
                tagFiles = tagFiles.OrderBy(tf => tf.Tag.Title).ToList();
                orderedList.AddRange(tagFiles.Select(tf => tf.Name));
            }
            return orderedList;
        }
    }
}
