using Microsoft.AspNetCore.Mvc;
using KnowledgeApp.Common;
using KnowledgeApp.LearningState.Service;
using KnowledgeApp.LearningState.Service.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KnowledgeApp.LearningState.Service.Controllers
{
    [ApiController]
    [Route("learning-states")]
    public class LearningStateController : ControllerBase
    {
        private readonly IRepository<LearningStateModel> _learningStateRepository;
        private readonly IRepository<ParagraphModel> _paragraphRepository;

        public LearningStateController(
            IRepository<LearningStateModel> learningStateRepository,
            IRepository<ParagraphModel> paragraphRepository)
        {
            _learningStateRepository = learningStateRepository ?? throw new ArgumentNullException(nameof(learningStateRepository));
            _paragraphRepository = paragraphRepository ?? throw new ArgumentNullException(nameof(paragraphRepository));
        }

        [HttpGet("{userId:guid}")]
        public async Task<ActionResult<IEnumerable<LearningStateDto>>> GetAsync([FromRoute] Guid userId)
        {
            if (userId == Guid.Empty)
            {
                return BadRequest("User ID cannot be empty.");
            }

            // Retrieve learning states for the user
            var userLearningStates = await _learningStateRepository.GetAllAsync(state => state.UserId == userId);

            // Get the list of paragraph IDs as strings
            var paragraphIds = userLearningStates.Select(state => state.ParagraphId.ToString()).ToList();

            // Fetch paragraphs that match the retrieved paragraph IDs, converting Id to string for comparison
            var paragraphs = await _paragraphRepository.GetAllAsync(paragraph => paragraphIds.Contains(paragraph.Id.ToString()));

            // Project to LearningStateDto, handling cases where paragraphs may be null
            var learningStateDtos = userLearningStates
                .Select(learningState =>
                {
                    var paragraph = paragraphs.SingleOrDefault(p => p.Id.ToString() == learningState.ParagraphId.ToString());
                    if (paragraph == null)
                    {
                        return null;
                    }
                    return learningState.AsDto(paragraph.ChapterNumber, paragraph.ParagraphNumber, userId);
                })
                .Where(dto => dto != null);

            return Ok(learningStateDtos);
        }



        [HttpPost]
        public async Task<ActionResult> PostAsync([FromBody] AssignLearningStateDto assignLearningStateDto)
        {
            if (assignLearningStateDto == null)
            {
                return BadRequest("Invalid learning state data.");
            }

            var existingLearningState = await _learningStateRepository.GetAsync(
                state => state.UserId == assignLearningStateDto.UserId && state.ParagraphId == assignLearningStateDto.ParagraphId);

            if (existingLearningState == null)
            {
                var newLearningState = new LearningStateModel
                {
                    UserId = assignLearningStateDto.UserId,
                    ParagraphId = assignLearningStateDto.ParagraphId,
                    Type = assignLearningStateDto.Type,
                };

                await _learningStateRepository.CreateAsync(newLearningState);
                return CreatedAtAction(nameof(GetAsync), new { userId = newLearningState.UserId }, newLearningState);
            }
            else
            {
                existingLearningState.Type = assignLearningStateDto.Type;
                await _learningStateRepository.UpdateAsync(existingLearningState);
                return NoContent(); // Indicating that the update was successful
            }
        }
    }
}
