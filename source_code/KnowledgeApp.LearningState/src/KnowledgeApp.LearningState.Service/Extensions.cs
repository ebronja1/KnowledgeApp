using KnowledgeApp.LearningState.Service;
using KnowledgeApp.LearningState.Service.Models;

namespace KnowledgeApp.LearningState.Service
{
    public static class Extensions
    {
        public static AssignLearningStateDto AsDto(this LearningStateModel learningStateModel, int chapterNumber, int paragraphNumber, Guid userId)
        {
            return new AssignLearningStateDto(learningStateModel.ParagraphId, learningStateModel.Type, chapterNumber, paragraphNumber, userId);
        }
    }
}