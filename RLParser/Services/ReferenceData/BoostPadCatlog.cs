using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RLParser.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace RLParser.Services.ReferenceData
{
    public class BoostPadCatlog
    {
       // private static HashSet<string> _boostPadCatalog = new HashSet<string>();
        public static Dictionary<string, BoostPad> _boostPadCatalog = new Dictionary<string, BoostPad>();

        public static void InitializeCatalog()
        {
            string jsonString = File.ReadAllText("../../../BoostPickupsDump.json");

            // 1. Deserialize into a temporary dynamic dictionary
            var tempContainer = JsonConvert.DeserializeObject<Dictionary<string, List<BoostPad>>>(jsonString);

            // 2. Loop through the "unknown" keys and their arrays
            foreach (var entry in tempContainer)
            {
                foreach (var pad in entry.Value)
                {
                    // Add to your fast lookup dictionary
                    _boostPadCatalog.Add(pad.Name, pad);
                   //Console.WriteLine(JsonConvert.SerializeObject(pad));
                }
            }
        }
    }
}
