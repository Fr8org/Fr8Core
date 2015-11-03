using System;
using System.Collections.Generic;

namespace HubWeb.ViewModels
{
    public class DiagnosticInfoVM
    {
        public string ServiceName { get; set; }
        public string GroupName { get; set; }
        public int Attempts { get; set; }
        public int Success { get; set; }
        public int Percent { get; set; }
        public bool Operational { get; set; }
        public bool RunningTest { get; set; }
        public String Key { get; set; }
        public String LastUpdated { get; set; }
        public String LastSuccess { get; set; }
        public String LastFail { get; set; }
        public List<DiagnosticEventInfoVM> Events { get; set; }
        public List<DiagnosticActionVM> Tests { get; set; }
        public List<DiagnosticActionVM> Actions { get; set; }
        public Dictionary<String, Object> Flags { get; set; } 
    }

    public class DiagnosticEventInfoVM
    {
        public String Date { get; set; }
        public String EventName { get; set; }
    }

    public class DiagnosticActionVM
    {
        public String DisplayName { get; set; }
        public String ServerAction { get; set; }
    }
}