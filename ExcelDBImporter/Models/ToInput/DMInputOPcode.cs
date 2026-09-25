using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExcelDBImporter.Models;

namespace ExcelDBImporter.Models.ToInput
{
    internal class DMInputOPcode
    {
        public string StrClassName { get;} = nameof(DMInputOPcode)!;
        public QrOPcode QROPcode { get; set; }

    }
}
