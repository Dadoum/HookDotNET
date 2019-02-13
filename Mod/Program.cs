using System.Linq;

namespace Mod
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            if (args.ToList().Contains("--unmod"))
            {
                var s = args.ToList();
                s.Remove("--unmod");
                Hook.Library.Core.Hook.UnMod(s.ToArray());
            }
            else
            {
                Hook.Library.Core.Hook.InjectMod(args);
            }
        }
    }
}