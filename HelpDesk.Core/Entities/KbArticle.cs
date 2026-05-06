using HelpDesk.Core.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpDesk.Core.Entities
{
    public class KbArticle : BaseEntity
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        // Links to the same Category table used for Tickets
        [Required]
        public int CategoryId { get; set; }
        public Category? Category { get; set; }

        [Required]
        public string Content { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Tags { get; set; }

        // We will need an Enum for this: Draft or Published
        [Required]
        public KbArticleStatus Status { get; set; } = KbArticleStatus.Draft;

        [Required]
        public int VersionNumber { get; set; } = 1;

        public int ViewCount { get; set; } = 0;

        public int HelpfulCount { get; set; } = 0;

        public int NotHelpfulCount { get; set; } = 0;

        // --- NEW PRD SECURITY COLUMNS ---
        [Required]
        [MaxLength(450)]
        public string AuthorUserId { get; set; } = string.Empty;

        [ForeignKey("AuthorUserId")]
        public ApplicationUser? AuthorUser { get; set; }

        [Required]
        [MaxLength(450)]
        public string LastUpdatedByUserId { get; set; } = string.Empty;

        [ForeignKey("LastUpdatedByUserId")]
        public ApplicationUser? LastUpdatedByUser { get; set; }
    }
}
