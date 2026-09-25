using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExcelDBImporter.Models;
using Microsoft.EntityFrameworkCore;

namespace ExcelDBImporter.Models.ToInput
{
    internal class DMInputScanCode
    {
        public string StrClassName { get;} = nameof(DMInputScanCode);
        [Comment("")]
        [Description(@"%SpecCode17")]
        public QrOPcode OPcode { get; set; }
    }
}
