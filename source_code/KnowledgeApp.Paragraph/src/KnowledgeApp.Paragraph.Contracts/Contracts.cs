namespace KnowledgeApp.Paragraph.Contracts
{
    public record ParagraphCreated(Guid Id, int ChapterNumber, int ParagraphNumber);
    public record ParagraphUpdated(Guid Id, int ChapterNumber, int ParagraphNumber);
    public record ParagraphDeleted(Guid Id);
}
