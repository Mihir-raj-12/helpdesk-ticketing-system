using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpDesk.Core.Entities
{
    public class KbArticleVersion : BaseEntity
    {
        [Required]
        public int KbArticleId { get; set; }

        [ForeignKey("KbArticleId")]
        public KbArticle? KbArticle { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        [Required]
        public int VersionNumber { get; set; }

        [Required]
        [MaxLength(450)]
        public string UpdatedByUserId { get; set; } = string.Empty;

        [ForeignKey("UpdatedByUserId")]
        public ApplicationUser? UpdatedByUser { get; set; }

        public DateTime UpdatedDate { get; set; }
    }
}
