using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace PakRebuilder
{
    public class Program
    {
        private const int NUM_WORKERS = 5;

        private static List<PakRecord>[] _pakRecords = new List<PakRecord>[NUM_WORKERS];
        private static List<Tuple<string, byte[]>> _allChunks = new();
        private static List<string> _chunkTree = new();

        private static void Main(string[] args)
        {
            // Initialize our records for our workers.
            for (var i = 0; i < NUM_WORKERS; i++)
                _pakRecords[i] = new();

            LoadPakRecords();

            // Load all chunks.
            foreach (var file in Directory.GetFiles(@"D:\Fortnite OT11 PC\Fortnite OT11 PC\Part1"))
            {
                _allChunks.Add(new(Path.GetFileName(file), File.ReadAllBytes(file)));
            }

            LoadChunkTree();

            for (var i = 0; i < NUM_WORKERS; i++)
                new Thread((index) => new ChunkWorker((int)index, _pakRecords[(int)index], _allChunks, _chunkTree).Start()).Start(i);

            Console.ReadKey();
        }

        private static void LoadPakRecords()
        {
            var index = 0;
            var lines = File.ReadAllLines(@"C:\Users\Kaitlyn\source\ConsoleApp1\ConsoleApp1\bin\Debug\net6.0\FileList-OT11-Hash.txt");
            foreach (var line in lines)
            {
                if (index == NUM_WORKERS)
                    index = 0;

                _pakRecords[index].Add(new(line));

                index++;
            }
        }

        private static void LoadChunkTree()
        {
            var lines = File.ReadAllLines(@"D:\Fortnite OT11 PC\Fortnite OT11 PC\Part1.txt");
            foreach (var line in lines)
            {
                _chunkTree.Add(line.Split(',')[0].Split(':')[1].TrimStart());
            }
        }
    }
}