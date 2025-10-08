using System;
using System.Text;

namespace OSaft_Apps
{
    public class OSaft_Security
    {
        public static string FNV1a(string input)
        {
            const uint Prime = 16777619;
            const uint offsetBasis = 2166136261;
            uint hash = offsetBasis;

            byte[] data = Encoding.UTF8.GetBytes(input);
            for (int i = 0; i < data.Length; i++)
            {
                hash ^= data[i];
                hash *= Prime;
            }
            return hash.ToString("X8");
        }




    }




}