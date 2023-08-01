using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace PakRebuilder
{
    public class ChunkWorker
    {
        private readonly Dictionary<string, string> _nextChunkMap = new()
        {
            //{ "54E4D90A4F582F33A910F38622D04723", "56B41FFE4E415EEC907F5AA83B4AF572" }
        };

        private int _workerId;
        private SHA1 _sha1;
        private List<PakRecord> _pakRecords;
        private List<Tuple<string, byte[]>> _allChunks;
        private List<string> _chunkTree;

        public ChunkWorker(int workerId, List<PakRecord> assignedPakRecords, List<Tuple<string, byte[]>> allChunks, List<string> chunkTree)
        {
            _workerId = workerId;
            _sha1 = SHA1.Create();
            _pakRecords = assignedPakRecords;
            _allChunks = allChunks;
            _chunkTree = chunkTree;
        }

        public void Start()
        {
            Console.WriteLine($"[W{_workerId}] Starting. Records = {_pakRecords.Count}");

            foreach (var record in _pakRecords)
            {
                ExtractDataInChunks(record);
            }

            Console.WriteLine($"[W{_workerId}] Finished!");
        }

        private string GetPakDirectoryPath(string path)
            => string.Join('\\', path.Split('/').SkipLast(1).ToArray());

        private string GetPakFileName(string path)
        {
            var split = path.Split('/');
            return split[split.Length - 1];
        }

        private void ExtractDataInChunks(PakRecord record)
        {
            //var dir = Path.Combine(@"D:\Fortnite OT11 PC\Fortnite OT11 PC\Dumped", GetPakDirectoryPath(record.FileName));
            //var filePath = Path.Combine(dir, GetPakFileName(record.FileName));

            //if (!File.Exists(filePath))
            {
                foreach (var file in _allChunks)
                {
                    byte[] buffer = file.Item2;
                    var positions = buffer.Locate(record.DataHash);

                    if (positions.Length > 0)
                    {
                        using (var stream = new MemoryStream(buffer))
                        using (var reader = new BinaryReader(stream))
                        {
                            Console.WriteLine($"[W{_workerId}] {positions.Length} positions.");

                            stream.Position = positions[0] + 25;

                            try
                            {
                                var data = reader.ReadBytes((int)record.Size);
                                var dataHash = _sha1.ComputeHash(data);
                                if (dataHash.SequenceEqual(record.DataHash))
                                {
                                    //Directory.CreateDirectory(dir);
                                    //File.WriteAllBytes(filePath, data);

                                    Console.WriteLine($"[W{_workerId}] Dumped {record.FileName} from chunk {Path.GetFileName(file.Item1)}.");
                                }
                                else
                                {
                                    //if (!TryMergeNextChunk(file.Item1, record, data))
                                    {
                                        Console.WriteLine($"[W{_workerId}] Couldn't extract {record.FileName} from chunk {Path.GetFileName(file.Item1)} due to hash not matching (chunk split?)");
                                        //File.WriteAllText("FailedFiles.txt", $"{record.FileName}|{Path.GetFileName(file.Item1)}\n");
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine($"[W{_workerId}] Couldn't extract {record.FileName} from chunk {Path.GetFileName(file.Item1)} due to an exception (likely couldn't read all bytes, chunk split?): {e}");
                                //File.WriteAllText("FailedExceptionFiles.txt", $"{record.FileName}|{Path.GetFileName(file.Item1)}\n");
                            }
                        }
                        break;
                    }
                }
            }
        }

        private bool TryMergeNextChunk(string chunkName, PakRecord record, byte[] data)
        {
            var dir = Path.Combine(@"D:\Fortnite OT11 PC\Fortnite OT11 PC\Dumped", GetPakDirectoryPath(record.FileName));
            var filePath = Path.Combine(dir, GetPakFileName(record.FileName));

            var nextChunkName = _nextChunkMap.ContainsKey(chunkName) ? _nextChunkMap[chunkName] : _chunkTree[_chunkTree.IndexOf(chunkName) + 1];
            var nextChunkData = _allChunks.Where(x => x.Item1 == nextChunkName).First().Item2;

            Console.WriteLine($"[W{_workerId}] Trying to merge {chunkName} with {nextChunkName}...");

            var mergedData = new byte[data.Length + nextChunkData.Length];
            Array.Copy(data, 0, mergedData, 0, data.Length);
            Array.Copy(nextChunkData, 0, mergedData, data.Length, nextChunkData.Length);

            var resizedData = new byte[record.Size];
            Array.Copy(mergedData, resizedData, resizedData.Length);

            var dataHash = _sha1.ComputeHash(resizedData);
            if (dataHash.SequenceEqual(record.DataHash))
            {
                Directory.CreateDirectory(dir);
                File.WriteAllBytes(filePath, resizedData);

                Console.WriteLine($"[W{_workerId}] Dumped {record.FileName} from chunk {Path.GetFileName(chunkName)} and {Path.GetFileName(nextChunkName)}.");
            }
            else
                return false;

            return true;
        }
    }
}
