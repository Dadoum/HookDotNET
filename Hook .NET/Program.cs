using System;
using System.Linq;

namespace Hook_.NET
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                var modding = true;
                
                var s = args.ToList();
                
                if (s.ToList().Contains("--unmod"))
                {
                    s.Remove("--unmod");
                    modding = false;
                }

                var exe = s[0];
                s.Remove(exe);

                if (!modding)
                {
                    Hook.Library.Hook.UnMod(exe, s.ToArray());
                }
                else
                {
                    Hook.Library.Hook.InjectMod(exe, s.ToArray());
                }

            }
            catch (Exception)
            {
                Console.WriteLine("Usage: \nmod <assembly to mod> <mod assembly 1> <mod assembly 2> ...");
            }
        }
    }
}