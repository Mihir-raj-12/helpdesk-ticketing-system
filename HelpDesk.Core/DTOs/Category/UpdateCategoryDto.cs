using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpDesk.Core.DTOs.Category
{
    public class UpdateCategoryDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;
    }
}
