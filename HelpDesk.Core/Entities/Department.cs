using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpDesk.Core.Entities
{
    public class Department : BaseEntity
    {
       

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        public string? DepartmentHeadId { get; set; }

        [ForeignKey("DepartmentHeadId")]
        public ApplicationUser? DepartmentHead { get; set; }

        public ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
    }
}
