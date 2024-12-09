using CsvHelper.Configuration;
using System.Reflection;
namespace ExcelDBImporter
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            string strMutex = GetMutexNambyAssemblyName();
            using (Mutex mutex =  new Mutex(true, strMutex,out bool creaedNew))
            {
                if (!creaedNew)
                {
                    MessageBox.Show("既に起動しています。多重起動は誤動作の可能性があるので禁止しています。");
                    return;
                }
                try
                {
                    Application.Run(new FrmExcelImpoerter());
                }
                catch (Exception)
                {
                    return;
                }

            }
        }
        /// <summary>
        /// アセンブリ名を元にMutex識別子を返す
        /// </summary>
        /// <returns></returns>
        static string GetMutexNambyAssemblyName()
        {
            AssemblyName assembly = Assembly.GetExecutingAssembly().GetName();
            string? strAppName = assembly.Name ?? null;
            return $"Global\\{strAppName?.ToString()}_Mutex" ?? string.Empty;
        }
    }
}