using System;
using Mod.Properties;

namespace ExampleMod
{
    [Mod(Class = "Program")]
    public class ModMain
    {
        private static bool mod;
        
        public static void Main()
        {
            // if (mod) return;
            Console.Write("Modded ");
            mod = true;
        }
    }
}