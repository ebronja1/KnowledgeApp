using System.Threading.Tasks;
using MassTransit;
using KnowledgeApp.Paragraph.Contracts;
using KnowledgeApp.Common;
using KnowledgeApp.LearningState.Service.Models;
using Microsoft.Extensions.Logging;

namespace KnowledgeApp.LearningState.Service.Consumers
{
    public class ParagraphDeletedConsumer : IConsumer<ParagraphDeleted>
    {
        private readonly IRepository<ParagraphModel> _repository;
        private readonly ILogger<ParagraphDeletedConsumer> _logger;

        public ParagraphDeletedConsumer(IRepository<ParagraphModel> repository, ILogger<ParagraphDeletedConsumer> logger)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Consume(ConsumeContext<ParagraphDeleted> context)
        {
            var message = context.Message;

            _logger.LogInformation("Received ParagraphDeleted event for Paragraph ID: {ParagraphId}", message.Id);

            var paragraphModel = await _repository.GetAsync(message.Id);
            if (paragraphModel == null)
            {
                _logger.LogWarning("Paragraph with ID {ParagraphId} not found.", message.Id);
                return;
            }

            try
            {
                await _repository.RemoveAsync(message.Id);
                _logger.LogInformation("Successfully removed Paragraph with ID: {ParagraphId}", message.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while removing Paragraph with ID: {ParagraphId}", message.Id);
                throw; // Rethrow the exception to allow MassTransit to handle retry policies
            }
        }
    }
}
