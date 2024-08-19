using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using SAWSCore8API.Models;
using SAWSCore8API.DbContexts;
using SAWSCore8API.Interfaces;

namespace SAWSCore8API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class FeedbacksController : ControllerBase
    {

        #region Fields
        private readonly ISawsService _sawsService;
        private readonly SAWSDbContext _context;
        private ILogger<FeedbacksController> _logger;

        #endregion

        #region Constructors

        public FeedbacksController(
            SAWSDbContext context,
            ISawsService sawsService,
            ILogger<FeedbacksController> logger
        )
        {
            _context = context;
            _sawsService = sawsService;
            _logger = logger;
        }

        #endregion

        #region Feedbacks

        [HttpGet("GetPagedAllFeedbacks")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetPagedAllFeedbacks([FromQuery] PaginationFilter filter)
        {
            if (filter == null)
            {
                return new BadRequestResult();
            }

            try
            {
                var pagedFeedbacks = await _sawsService.GetPagedAllFeedbacks(filter);
                return new OkObjectResult(pagedFeedbacks);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to get paged feedbacks");
                return Problem("Unable to get paged feedbacks");
            }
        }

        [HttpGet("GetPagedAllFeedbacksByUniqueEmail")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetPagedAllFeedbacksByUniqueEmail([FromQuery] PaginationFilter filter)
        {
            if (filter == null)
            {
                return new BadRequestResult();
            }

            try
            {
                var pagedFeedbacks = await _sawsService.GetPagedAllFeedbacksByUniqueEmail(filter);
                return new OkObjectResult(pagedFeedbacks);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to get paged feedbacks by email");
                return Problem("Unable to get paged feedbacks by email");
            }
        }

        [HttpGet("GetPagedAllBroadcasts")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetPagedAllBroadcasts([FromQuery] PaginationFilter filter)
        {
            if (filter == null)
            {
                return new BadRequestResult();
            }

            try
            {
                var pagedBroadcasts = await _sawsService.GetPagedAllBroadcasts(filter);
                return new OkObjectResult(pagedBroadcasts);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to get paged broadcasts");
                return Problem("Unable to get paged broadcasts");
            }
        }

        #endregion

        // GET: api/Feedbacks
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Feedback>>> GetFeedbacks()
        {
            return await _context.Feedbacks.ToListAsync();
        }

        // GET: api/Feedbacks/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Feedback>> GetFeedback(int id)
        {
            var feedback = await _context.Feedbacks.FindAsync(id);

            if (feedback == null)
            {
                return NotFound();
            }

            return feedback;
        }

        // PUT: api/Feedbacks/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutFeedback(int id, Feedback feedback)
        {
            if (id != feedback.feedbackId)
            {
                return BadRequest();
            }

            _context.Entry(feedback).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!FeedbackExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Feedbacks
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Feedback>> PostFeedback(Feedback feedback)
        {
            _context.Feedbacks.Add(feedback);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetFeedback", new { id = feedback.feedbackId }, feedback);
        }

        // DELETE: api/Feedbacks/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFeedback(int id)
        {
            var feedback = await _context.Feedbacks.FindAsync(id);
            if (feedback == null)
            {
                return NotFound();
            }

            _context.Feedbacks.Remove(feedback);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool FeedbackExists(int id)
        {
            return _context.Feedbacks.Any(e => e.feedbackId == id);
        }
    }
}
