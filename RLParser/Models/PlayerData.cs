using System;
using System.Collections.Generic;
using System.Text;

namespace RLParser.Models
{
    public class PlayerData
    {
        public string Name { get; set; } = String.Empty;
        public int Score { get; set; }
        public int Goals { get; set; }
        public int Team { get; set; }
    }
}
