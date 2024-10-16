using System;
using KnowledgeApp.Common;

namespace KnowledgeApp.LearningState.Service.Models
{
    public class LearningStateModel : IModel
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string ParagraphId { get; set; }
         public LearningStateType Type { get; set; }

    }
}