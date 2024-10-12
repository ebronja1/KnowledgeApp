using System.ComponentModel.DataAnnotations;
using KnowledgeApp.Common;

namespace KnowledgeApp.Paragraph.Service.Models
{
    public class ParagraphModel : IModel
    {       
        public Guid Id { get; set; }

        [Required]
        public string Book { get; set; } = string.Empty;

        public string Chapter { get; set; } = string.Empty;

        [Required]
        public int ParagraphNumber { get; set; } = 1;
        
        public int ChapterNumber { get; set; } = 1;
    }
}