using System;

namespace Utilities
{
    public static class Server
    {
        public static String ServerPhysicalPath { get; set; }
        public static String ServerHostName { get; set; }
        public static String ServerUrl { get; set; }

        public static bool IsDevMode { get; set; }

        public static bool IsProduction { get; set; }
    }
}
