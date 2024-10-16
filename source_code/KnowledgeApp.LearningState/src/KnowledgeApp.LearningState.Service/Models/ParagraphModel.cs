using System;
using KnowledgeApp.Common;

namespace KnowledgeApp.LearningState.Service.Models
{
    public class ParagraphModel : IModel
    {
        public Guid Id { get; set; }

        public int ChapterNumber { get; set; }

        public int ParagraphNumber { get; set; }

    }
}