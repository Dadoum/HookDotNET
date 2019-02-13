using System;
using System.Runtime.CompilerServices;

namespace Mod.Properties
{
    public class HookMethodAttribute : Attribute
    {
        public string Method;
        
        public HookMethodAttribute([CallerMemberName] string Method = null)
        {
            this.Method = Method;
        }

        public string GetMethod()
        {
            return Method;
        }
    }
}