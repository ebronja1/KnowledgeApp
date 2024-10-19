using System.Threading.Tasks;
using MassTransit;
using KnowledgeApp.Paragraph.Contracts;
using KnowledgeApp.Common;
using KnowledgeApp.LearningState.Service.Models;

namespace KnowledgeApp.LearningState.Service.Consumers
{
    public class ParagraphUpdatedConsumer : IConsumer<ParagraphUpdated>
    {
        private readonly IRepository<ParagraphModel> _repository;

        public ParagraphUpdatedConsumer(IRepository<ParagraphModel> repository)
        {
            _repository = repository;
        }

        public async Task Consume(ConsumeContext<ParagraphUpdated> context)
        {
            var message = context.Message;

            var paragraphModel = await _repository.GetAsync(message.Id);

            if (paragraphModel == null)
            {
                paragraphModel = new ParagraphModel
                {
                    Id = message.Id,
                    ChapterNumber = message.ChapterNumber,
                    ParagraphNumber = message.ParagraphNumber
                };

                await _repository.CreateAsync(paragraphModel);
            }
            else
            {
                paragraphModel.ChapterNumber = message.ChapterNumber;
                paragraphModel.ParagraphNumber = message.ParagraphNumber;

                await _repository.UpdateAsync(paragraphModel);
            }
        }
    }
}