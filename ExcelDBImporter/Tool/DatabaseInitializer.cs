using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExcelDBImporter.Context;
using Microsoft.EntityFrameworkCore;

namespace ExcelDBImporter.Tool
{
    public class DatabaseInitializer
    {
        public static void DatabaseExlistCheker() 
        {
            try
            {
                using ExcelDbContext dbContext = new();
                dbContext.Database.Migrate();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                throw;
            }
            finally
            {
            }
        }
    }
}
