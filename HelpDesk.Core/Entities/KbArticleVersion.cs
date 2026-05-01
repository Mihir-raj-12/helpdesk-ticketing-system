using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpDesk.Core.Entities
{
    public class KbArticleVersion : BaseEntity
    {
        [Required]
        public int KbArticleId { get; set; }
        public KbArticle? KbArticle { get; set; }

        [Required]
        public string TitleSnapshot { get; set; } = string.Empty;

        [Required]
        public string ContentSnapshot { get; set; } = string.Empty;

        [Required]
        public int VersionNumber { get; set; }

        [Required]
        public string SavedByUserId { get; set; } = string.Empty;
        public ApplicationUser? SavedByUser { get; set; }
    }
}
