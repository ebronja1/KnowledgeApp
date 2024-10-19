using System.ComponentModel.DataAnnotations;
using KnowledgeApp.LearningState.Service.Models;

namespace KnowledgeApp.LearningState.Service
{
    public record ParagraphDto(Guid Id, int ParagraphNumber, int ChapterNumber);
    public record LearningStateDto(Guid Id, LearningStateType Type);
    public record AssignLearningStateDto(Guid ParagraphId, LearningStateType Type, int chaperNumber, int paragraphNumber);
}