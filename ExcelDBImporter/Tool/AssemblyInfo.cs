using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Threading.Tasks;

namespace ExcelDBImporter.Tool
{
    internal class AssemblyInfo
    {
        public static string AssemblyVersion()
        {
            try
            {
                Assembly asm = Assembly.GetExecutingAssembly();
                if (asm == null) { return string.Empty; }
                AssemblyName assemblyName = asm.GetName();
                if (assemblyName == null) { return string.Empty; }
                Version version = assemblyName.Version ?? new Version("");
                return version.ToString();
            }
            catch (Exception)
            {
                return string.Empty;
                throw;
            }
            
        }
    }
}
