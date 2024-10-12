using KnowledgeApp.Paragraph.Service.Models;

namespace KnowledgeApp.Paragraph.Service
{
    public static class Extensions
    {
        public static ParagraphDto AsDto(this ParagraphModel paragraph)
        {
            return new ParagraphDto(paragraph.Id, paragraph.Book, paragraph.Chapter, paragraph.ChapterNumber, paragraph.ParagraphNumber);
        }
    }
}