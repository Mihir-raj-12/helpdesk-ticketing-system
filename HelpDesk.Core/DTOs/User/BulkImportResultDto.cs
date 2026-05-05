using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpDesk.Core.DTOs.User
{
    public class BulkImportResultDto
    {
        public int TotalProcessed { get; set; }
        public int SuccessfullyCreated { get; set; }
        public int SkippedDuplicates { get; set; }
        public List<string> Errors { get; set; } = new();
    }
}
