using Avalonia.Platform.Storage;
using Newtonsoft.Json.Linq;
using RocketRP;
using RocketRP.Serializers;
using System;
using System.IO;

namespace RLParser.Services
{
    public class ReplayFileReader
    {
        public JObject ParseFileToJson(IStorageFile file)
        {
            string jsonOutput = null;

            try
            {
                var localPath = file.TryGetLocalPath();
                if (string.IsNullOrWhiteSpace(localPath))
                {
                    Console.WriteLine("Could not resolve local file path.");
                    return null;
                }

                var replay = Replay.Deserialize(localPath, parseNetstream: true, true);
                var serializer = new ReplayJsonSerializer();
                jsonOutput = serializer.Serialize(replay, prettyPrint: true);

                const string outputPath = "replay_dump.json";
                File.WriteAllText(outputPath, jsonOutput);

                Console.WriteLine("Replay parsed successfully!");
                Console.WriteLine($"Engine Version: {replay.EngineVersion}");
                Console.WriteLine($"Licensee Version: {replay.LicenseeVersion}");
                Console.WriteLine($"Done! Analysis saved to {outputPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }

            return jsonOutput is null ? null : JObject.Parse(jsonOutput);
        }
    }
}