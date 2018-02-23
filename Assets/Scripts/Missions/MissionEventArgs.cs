﻿using System;

namespace Missions
{
    public class MissionEventArgs : EventArgs
    {
        public int MissionNumber { get; set; }
        public float MissionLength { get; set; }
        public string MissionBrief { get; set; }
    }
}