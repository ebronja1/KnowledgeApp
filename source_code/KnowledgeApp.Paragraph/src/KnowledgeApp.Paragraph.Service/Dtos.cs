using System.ComponentModel.DataAnnotations;

namespace KnowledgeApp.Paragraph.Service
{
    public record ParagraphDto(Guid Id, string Book, string Chapter, int ParagraphNumber, int ChapterNumber);
    public record ParagraphCreateDto([Required] string Book, string Chapter, [Required] int ParagraphNumber, int ChapterNumber);
    public record ParagraphUpdateDto(string Book, string Chapter, int ParagraphNumber, int ChapterNumber);
}