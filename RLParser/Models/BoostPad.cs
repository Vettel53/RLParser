using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Text;

namespace RLParser.Models
{
    public class BoostPad
    {
        public string Name { get; set; }
        public string BoostType { get; set; }
        public double BoostAmount { get; set; }
        public int RespawnDelay { get; set; }
        public Location Location { get; set; }
        public int CollisionHeight { get; set; }
        public int CollisionRadius { get; set; }
    }

    public class Location
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
    }


}

