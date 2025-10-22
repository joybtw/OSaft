using Cosmos.HAL;
using System;

namespace OSaft.applications
{
    public static class Clock
    {
        private static bool IsActive = true;

        // Cosmos zieht die Uhrzeit durch RTC vom Hostsystem, Sommer/-Winterzeit muss dabei nicht beachtet werden.
        // wenn nicht virtualisert wird, dann wird die Hardwareuhr genutzt. HW Uhr 1:00Uhr UTC,
        // Hostsystem 2:00Uhr MEZ 3:00 MESZ, Cosmos Strg+c, strg+v
        public static void ToggleClock() {
            IsActive = !IsActive;
        }

        public static string ShowTime()
        {
            if (!IsActive) { return ""; }
            int h = RTC.Hour;
            int m = RTC.Minute;
            int s = RTC.Second;

            return h.ToString("D2") + ":" + m.ToString("D2") + s.ToString("D2");
            
        }
        public static string GetShortTime()
        {
            if (!IsActive) return "";
            int h = RTC.Hour;
            int m = RTC.Minute;
            return h.ToString("D2") + ":" + m.ToString("D2");
        }
    }
}



