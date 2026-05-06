using HelpDesk.Core.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpDesk.Core.DTOs.KnowledgeBase
{
    public class CreateKbArticleDto
    {
        [Required(ErrorMessage = "Title is required.")]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Category is required.")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Content is required.")]
        public string Content { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Tags { get; set; }

        // NEW: Admins can pass this in. (Agents will be forced to Draft by the service anyway!)
        public KbArticleStatus Status { get; set; } = KbArticleStatus.Draft;
    }
}
