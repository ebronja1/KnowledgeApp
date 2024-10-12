namespace KnowledgeApp.Paragraph.Contracts
{
    public record ParagraphCreated(Guid ItemId, int ChapterNumber, int ParagraphNumber);
    public record ParagraphUpdated(Guid ItemId, int ChapterNumber, int ParagraphNumber);
    public record ParagraphDeleted(Guid ItemId);
}
