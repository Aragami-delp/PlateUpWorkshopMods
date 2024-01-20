using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace KitchenBetterDiscordRichPresence.Utility
{
    public static class Utility
    {
        public static System.Reflection.Assembly GetLoadedAssembly(string Name)
        {
            try
            {
                foreach (Assembly TempAssembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    string assemblyName = TempAssembly.GetName().Name;
                    if (assemblyName.Equals(Name, StringComparison.InvariantCultureIgnoreCase)
                        || assemblyName.Equals(Name + "-Workshop", StringComparison.InvariantCultureIgnoreCase)
                    )
                    {
                        return TempAssembly;
                    }
                }
                return null;
            }
            catch { return null; }
        }
    }
}
