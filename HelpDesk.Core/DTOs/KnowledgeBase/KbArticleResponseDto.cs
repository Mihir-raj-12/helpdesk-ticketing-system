using HelpDesk.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpDesk.Core.DTOs.KnowledgeBase
{
    public class KbArticleResponseDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;

        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty; // Flattens the relationship for the UI

        public string Content { get; set; } = string.Empty;
        public string? Tags { get; set; }

        public KbArticleStatus Status { get; set; }
        public int VersionNumber { get; set; }

        public int ViewCount { get; set; }
        public int HelpfulCount { get; set; }
        public int NotHelpfulCount { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime LastUpdatedDate { get; set; }
    }
}
