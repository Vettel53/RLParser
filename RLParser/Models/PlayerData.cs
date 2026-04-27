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
        
        public int TotalBoostGrabs { get; set; }
        public int BigBoostGrabs { get; set; }
        public int SmallBoostGrabs { get; set; }
        public int BoostSteals { get; set; }

        public override string ToString()
        {
            return $"Player: Name: {Name} \nScore: {Score} \nGoals: {Goals} \nTeam: {Team} \nTotalBoostGrabs: {TotalBoostGrabs} \nBigBoostGrabs: {BigBoostGrabs} \nSmallBoostGrabs: {SmallBoostGrabs} \nBoostSteals: {BoostSteals}\n";
        }
    }
}
