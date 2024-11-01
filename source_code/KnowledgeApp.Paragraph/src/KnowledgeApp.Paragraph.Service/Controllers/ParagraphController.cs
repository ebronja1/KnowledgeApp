using KnowledgeApp.Common;
using KnowledgeApp.Common.Redis;
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
        private readonly RedisCacheService _redisCacheService;
        private bool isCached;

        private const string CacheKeyPrefix = "Paragraph_";
        private const string IdempotencyKeyPrefix = "IdempotencyKey_";

        public ParagraphsController(IRepository<ParagraphModel> paragraphsRepository, IPublishEndpoint publishEndpoint, RedisCacheService redisCacheService)
        {
            _paragraphsRepository = paragraphsRepository;
            _publishEndpoint = publishEndpoint;
            _redisCacheService = redisCacheService;
            isCached = false;
        }

        // GET /paragraphs
        [HttpGet]
        public async Task<IActionResult> GetAsync()
        {
            try
            {

                // Try to get paragraphs from Redis cache
                var cachedParagraphs = _redisCacheService.GetCachedData<IEnumerable<ParagraphDto>>($"{CacheKeyPrefix}All");

                if (cachedParagraphs != null)
                {
                    isCached = true;
                    return Ok(cachedParagraphs); // Return cached data
                }

                // Fetch from MongoDB if not in cache
                var paragraphs = await _paragraphsRepository.GetAllAsync();

                if (paragraphs == null || !paragraphs.Any())
                {
                    isCached = false;
                    return NotFound(); // Return 404 if no paragraphs found
                }

                var paragraphDtos = paragraphs.Select(paragraph => paragraph.AsDto());

                // Cache the data in Redis for 10 minutes
                _redisCacheService.SetCachedData($"{CacheKeyPrefix}All", paragraphDtos, TimeSpan.FromMinutes(10));

                return Ok(new {paragraphDtos, isCached}); // Return 200 with the data
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
            // Try to get paragraph from Redis cache
            var cachedParagraph = _redisCacheService.GetCachedData<ParagraphDto>($"{CacheKeyPrefix}{id}");

            if (cachedParagraph != null)
            {
                return Ok(cachedParagraph); // Return cached data
            }

            var paragraph = await _paragraphsRepository.GetAsync(id);

            if (paragraph == null)
            {
                return NotFound();
            }

            var paragraphDto = paragraph.AsDto();

            // Cache the paragraph data in Redis for 10 minutes
            _redisCacheService.SetCachedData($"{CacheKeyPrefix}{id}", paragraphDto, TimeSpan.FromMinutes(10));

            return Ok(paragraphDto);
        }

        // POST /paragraphs
       
        [HttpPost]
        public ActionResult<ParagraphDto> PostAsync(ParagraphCreateDto paragraphCreateDto, [FromHeader(Name = "Idempotency-Key")] string idempotencyKey)
        {
            if (string.IsNullOrWhiteSpace(idempotencyKey))
            {
                return BadRequest("Idempotency-Key header is required.");
            }

            // Check if this idempotency key has been used
            var existingId = _redisCacheService.GetCachedData<Guid>($"{IdempotencyKeyPrefix}{idempotencyKey}");
            if (existingId != Guid.Empty)
            {
                var existingParagraph = _paragraphsRepository.GetAsync(existingId).Result; // Synchronously wait for the repository call
                if (existingParagraph != null)
                {
                    return CreatedAtAction(nameof(GetByIdAsync), new { id = existingParagraph.Id }, existingParagraph.AsDto());
                }
            }

            var paragraphModel = new ParagraphModel
            {
                Book = paragraphCreateDto.Book,
                Chapter = paragraphCreateDto.Chapter,
                ChapterNumber = paragraphCreateDto.ChapterNumber,
                ParagraphNumber = paragraphCreateDto.ParagraphNumber,
            };

            _paragraphsRepository.CreateAsync(paragraphModel).Wait(); // Synchronously wait for the repository call
            // Store the idempotency key with the paragraph ID in Redis
            _redisCacheService.SetCachedData($"{IdempotencyKeyPrefix}{idempotencyKey}", paragraphModel.Id, TimeSpan.FromMinutes(10));

            _publishEndpoint.Publish(new ParagraphCreated(paragraphModel.Id, paragraphCreateDto.ChapterNumber, paragraphCreateDto.ParagraphNumber)).Wait(); // Synchronously wait for publish

            // Invalidate cache for all paragraphs since a new paragraph was added
            _redisCacheService.RemoveCachedData($"{CacheKeyPrefix}All");

            return CreatedAtAction(nameof(GetByIdAsync), new { id = paragraphModel.Id }, paragraphModel);
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

            // Remove the cached data for this specific paragraph
            _redisCacheService.RemoveCachedData($"{CacheKeyPrefix}{id}");

            // Invalidate cache for all paragraphs
            _redisCacheService.RemoveCachedData($"{CacheKeyPrefix}All");

            return NoContent();
        }
    }
}
