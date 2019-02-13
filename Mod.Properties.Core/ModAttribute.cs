using System;
using System.Dynamic;
using System.Runtime.CompilerServices;

namespace Mod.Properties
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class ModAttribute : Attribute
    {
        public string Class;
        
        public ModAttribute([CallerMemberName] string Class = null)
        {
            this.Class = Class;
        }

        public string GetClass()
        {
            return Class;
        }
    }
}