using KnowledgeApp.Common;
using MassTransit;
using KnowledgeApp.Paragraph.Contracts;
using Microsoft.AspNetCore.Mvc;
using KnowledgeApp.Paragraph.Service.Models;

namespace KnowledgeApp.Paragraph.Service.Controllers
{
    [ApiController]
    [Route("paragraphs")]
    public class ParagraphsController : ControllerBase
    {
        private readonly IRepository<ParagraphModel> _paragraphsRepository;
        private readonly IPublishEndpoint _publishEndpoint;

        public ParagraphsController(IRepository<ParagraphModel> paragraphsRepository, IPublishEndpoint publishEndpoint)
        {
            _paragraphsRepository = paragraphsRepository;
            _publishEndpoint = publishEndpoint;
        }

        [HttpGet]
        public async Task<IActionResult> GetAsync()
        {
            try
            {
                var paragraphs = await _paragraphsRepository.GetAllAsync();

                if (paragraphs == null || !paragraphs.Any())
                {
                    return NotFound(); // Return 404 if no paragraphs found
                }

                var paragraphDtos = paragraphs.Select(paragraph => paragraph.AsDto());
                return Ok(paragraphDtos); // Return 200 with the data
            }
            catch (Exception ex)
            {
                // Log the exception (consider using a logging framework)
                return StatusCode(500, "Internal server error: " + ex.Message); // Return 500 for unexpected errors
            }
        }
        // GET /paragraphs/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<ParagraphDto>> GetByIdAsync(Guid id)
        {
            var paragraph = await _paragraphsRepository.GetAsync(id);

            if (paragraph == null)
            {
                return NotFound();
            }

            return paragraph.AsDto();
        }

        // POST /paragraphs
        [HttpPost]
        public async Task<ActionResult<ParagraphDto>> PostAsync(ParagraphCreateDto paragraphCreateDto)
        {
            var paragraphModel = new ParagraphModel
            {
                Book = paragraphCreateDto.Book,
                Chapter = paragraphCreateDto.Chapter,
                ChapterNumber = paragraphCreateDto.ChapterNumber,
                ParagraphNumber = paragraphCreateDto.ParagraphNumber,
            };

            await _paragraphsRepository.CreateAsync(paragraphModel);

            await _publishEndpoint.Publish(new ParagraphCreated(paragraphModel.Id, paragraphCreateDto.ChapterNumber, paragraphCreateDto.ParagraphNumber));

            return CreatedAtAction(nameof(GetByIdAsync), new { id = paragraphModel.Id }, paragraphModel);
        }

        // PUT /paragraphs/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAsync(Guid id, ParagraphUpdateDto paragraphUpdateDto)
        {
            var existingParagraph = await _paragraphsRepository.GetAsync(id);

            if (existingParagraph == null)
            {
                return NotFound();
            }

            existingParagraph.Book = paragraphUpdateDto.Book;
            existingParagraph.Chapter = paragraphUpdateDto.Chapter;
            existingParagraph.ChapterNumber = paragraphUpdateDto.ChapterNumber;
            existingParagraph.ParagraphNumber = paragraphUpdateDto.ParagraphNumber;

            await _paragraphsRepository.UpdateAsync(existingParagraph);

            await _publishEndpoint.Publish(new ParagraphUpdated(existingParagraph.Id, existingParagraph.ChapterNumber, existingParagraph.ParagraphNumber));

            return NoContent();
        }

        // DELETE /paragraphs/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(Guid id)
        {
            var paragraphModel = await _paragraphsRepository.GetAsync(id);

            if (paragraphModel == null)
            {
                return NotFound();
            }

            await _paragraphsRepository.RemoveAsync(paragraphModel.Id);

            await _publishEndpoint.Publish(new ParagraphDeleted(id));

            return NoContent();
        }
    }
}
