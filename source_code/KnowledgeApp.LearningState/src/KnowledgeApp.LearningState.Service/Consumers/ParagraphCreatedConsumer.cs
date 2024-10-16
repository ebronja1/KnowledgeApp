using System.Threading.Tasks;
using MassTransit;
using KnowledgeApp.Paragraph.Contracts;
using KnowledgeApp.Common;
using KnowledgeApp.LearningState.Service.Models;

namespace KnowledgeApp.LearningState.Service.Consumers
{
    public class ParagraphCreatedConsumer : IConsumer<ParagraphCreated>
    {
        private readonly IRepository<ParagraphModel> _repository;

        public ParagraphCreatedConsumer(IRepository<ParagraphModel> repository)
        {
            _repository = repository;
        }

        public async Task Consume(ConsumeContext<ParagraphCreated> context)
        {
            var message = context.Message;

            var paragraphModel = await _repository.GetAsync(message.Id);

            if (paragraphModel != null)
            {
                return;
            }

            paragraphModel = new ParagraphModel
            {
                Id = message.Id,
                ChapterNumber = message.ChapterNumber,
                ParagraphNumber = message.ParagraphNumber,
            };

            await _repository.CreateAsync(paragraphModel);
        }
    }
}