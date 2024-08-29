using Microsoft.AspNetCore.Mvc;

using SAWSCore8API.Models;
using SAWSCore8API.DbContexts;
using SAWSCore8API.Interfaces;
using System.Net.Mime;

namespace SAWSCore8API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class FeedbacksController : ControllerBase
    {

        #region Fields
        private readonly IPagedService _pagedService;
        private readonly IFeedbackService _feedbackService;
        private readonly SAWSDbContext _context;
        private ILogger<FeedbacksController> _logger;
        public IConfiguration _configuration { get; }

        #endregion

        #region Constructors

        public FeedbacksController(
            SAWSDbContext context,
            IPagedService pagedService,
            IFeedbackService feedbackService,
            ILogger<FeedbacksController> logger,
             IConfiguration configuration
        )
        {
            _context = context;
            _pagedService = pagedService;
            _feedbackService = feedbackService;
            _logger = logger;
            _configuration = configuration;
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
                var pagedFeedbacks = await _pagedService.GetPagedAllFeedbacks(filter);
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
                var pagedFeedbacks = await _pagedService.GetPagedAllFeedbacksByUniqueEmail(filter);
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
                var pagedBroadcasts = await _pagedService.GetPagedAllBroadcasts(filter);
                return new OkObjectResult(pagedBroadcasts);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to get paged broadcasts");
                return Problem("Unable to get paged broadcasts");
            }
        }

        [HttpPost("PostInsertNewFeedback")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status202Accepted, Type = typeof(Feedback))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(UpdateResult))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PostInsertNewFeedback(Feedback feedback)
        {
            if (!ModelState.IsValid)
            {
                var errorMessages = ModelState.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).AsEnumerable()
                );

                return BadRequest(new CreateResult
                {
                    Success = false,
                    ErrorMessages = errorMessages
                });
            }

            ProcessFeedbackMessage(feedback);

            try
            {
                if (feedback.feedbackId == 0)
                {
                    // Creating new feedback
                    var newFeedbackResult = await _feedbackService.AddFeedback(feedback);

                    if (newFeedbackResult.Success)
                    {
                     /*   return Ok(new Response
                        {
                            Status = "Success",
                            Message = "Successfully added new feedback",
                            DetailDescription = feedback
                        });*/

                        return Ok(newFeedbackResult);
                    }

                    return BadRequest(new CreateResult
                    {
                        Success = false,
                        ErrorMessages = new Dictionary<string, IEnumerable<string>>
                            {
                                { "General", new[] { "Failed to insert feedback. Invalid condition." } }
                            }
                    });
                }
                else
                {
                    // Updating existing feedback
                    if (!FeedbackExists(feedback.feedbackId))
                    {
                        return new NotFoundResult();
                    }

                    var updateFeedbackResult = await _feedbackService.UpdateFeedback(feedback);

                    if (updateFeedbackResult.Success)
                    {
                        return Ok(new Response
                        {
                            Status = "Success",
                            Message = "Successfully updated feedback",
                            DetailDescription = feedback
                        });

                        /*return Ok(updateFeedbackResult);*/
                    }

                    return BadRequest(new CreateResult
                    {
                        Success = false,
                        ErrorMessages = new Dictionary<string, IEnumerable<string>>
                            {
                                { "General", new[] { "Failed to update feedback. Invalid condition." } }
                            }
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception from FeedbackController.PostInsertNewFeedback");
                return Problem("Unable to process the feedback.");
            }
        }

        [HttpPost("PostInsertBroadcastMessages")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status202Accepted, Type = typeof(Feedback))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(UpdateResult))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PostInsertBroadcastMessages(List<Feedback> feedbackList)
        {
            if (feedbackList == null || !feedbackList.Any())
            {
                return new BadRequestObjectResult("The feedback list is empty.");
            }

            foreach (Feedback feedback in feedbackList)
            {
                if (!ModelState.IsValid)
                {
                    var errorMessages = ModelState.ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).AsEnumerable()
                    );

                    return BadRequest(new CreateResult
                    {
                        Success = false,
                        ErrorMessages = errorMessages
                    });
                }

                var batchId = Guid.NewGuid().ToString();
                var broadcastId = Guid.NewGuid().ToString();

                ProcessBroadcastMessage(feedback, broadcastId);

                try
                {
                    if (feedback.feedbackId == 0)
                    {
                        // Creating new feedback
                        var newFeedbackResult = await _feedbackService.AddBroadcast(feedback, batchId, broadcastId);

                        if (newFeedbackResult.Success)
                        {
                             /*   return Ok(new Response
                                {
                                Status = "Success",
                                Message = "Successfully added new feedback",
                                DetailDescription = feedback
                            });
                             */
                            
                            return Ok(newFeedbackResult);
                        }

                        return BadRequest(new CreateResult
                        {
                            Success = false,
                            ErrorMessages = new Dictionary<string, IEnumerable<string>>
                            {
                                { "General", new[] { "Failed to insert feedback. Invalid condition." } }
                            }
                        });
                    }
                    else
                    {
                        // Updating existing feedback
                        if (!FeedbackExists(feedback.feedbackId))
                        {
                            return new NotFoundResult();
                        }

                        var updateFeedbackResult = await _feedbackService.UpdateBroadcast(feedback, batchId, broadcastId);

                        if (updateFeedbackResult.Success)
                        {
                           /* return Ok(new Response
                            {
                                Status = "Success",
                                Message = "Successfully updated feedback",
                                DetailDescription = feedback
                            });*/

                            return Ok(updateFeedbackResult);
                        }

                        return BadRequest(new CreateResult
                        {
                            Success = false,
                            ErrorMessages = new Dictionary<string, IEnumerable<string>>
                            {
                                { "General", new[] { "Failed to update broadcast. Invalid condition." } }
                            }
                        });
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unhandled exception from FeedbackController.PostInsertBroadcastMessages");
                    return Problem("Unable to process the broadcast.");
                }

            }

            return BadRequest();
        }

        [HttpDelete("DeleteFeedbackById")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(DeleteResult))]
        public async Task<ActionResult> DeleteFeedbackById(int id)
        {
            try
            {
                if (!FeedbackExists(id))
                {
                    return new NotFoundResult();
                }

                var result = await _feedbackService.DeleteFeedbackById(id);

                if (result.Success)
                {
                    return new OkObjectResult(result);
                }
                else
                {
                    return new BadRequestResult();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception from FeedbackController.DeleteFeedbackById");
                return Problem("Unable to Delete the advert");
            }
        }

        [HttpDelete("DeleteBroadcastByBatchId")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(DeleteResult))]
        public async Task<ActionResult> DeleteBroadcastByBatchId(string id)
        {
            try
            {
                /* if (!FeedbackExists(id))
                 {
                     return new NotFoundResult();
                 }*/

                var result = await _feedbackService.DeleteBroadcastByBatchId(id);

                if (result.Success)
                {
                    return new OkObjectResult(result);
                }
                else
                {
                    return new BadRequestResult();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception from FeedbackController.DeleteBroadcastByBatchId");
                return Problem("Unable to Delete the advert");
            }
        }

        [HttpGet("GetFeedbackById")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Advert))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetFeedbackById(int id)
        {
            var app_url = _configuration["AppURL"];

            try
            {
                var feedback = _feedbackService.GetFeedbackById(id);

                if (feedback == null)
                {
                    return new NotFoundResult();
                }

                foreach (var feedbackMessage in feedback.FeedbackMessages)
                {
                    foreach (var docFeedback in feedbackMessage.DocFeedbacks)
                    {
                        docFeedback.file_url = app_url + $"/aviapp_api/Uploads/{docFeedback.DocTypeName}/{docFeedback.feedbackMessageId}/{docFeedback.file_origname}";
                    }
                }

                return Ok(new Response
                {
                    Status = "Success",
                    Message = "Successfully returned feedback",
                    DetailDescription = feedback
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception from FeedbackController.GetFeedbackById");
                return Problem("Unable to get the feedback");
            }

        }

        [HttpGet("GetBroadcastMessages")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult GetBroadcastMessages()
        {
            try
            {
                var broadcasts = _feedbackService.GetBroadcastMessages();

                return new OkObjectResult(broadcasts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception from FeedbackController.GetBroadcastMessages");
                return Problem("Unable to get Broadcast Messages");
            }
        }

        [HttpGet("GetFeedbackMessagesBySenderId")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult GetFeedbackMessagesBySenderId(string id)
        {
            try
            {
                var feedbacks = _feedbackService.GetFeedbackMessagesBySenderId(id);

                return new OkObjectResult(feedbacks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception from FeedbackController.GetBroadcastMessages");
                return Problem("Unable to get Broadcast Messages");
            }
        }

        #endregion

        #region Helper Methods

        private void ProcessFeedbackMessage(Feedback feedback)
        {
            foreach (var feedbackMessage in feedback.FeedbackMessages)
            {
                feedbackMessage.responderId = feedback.responderId;
                feedbackMessage.responderEmail = feedback.responderEmail;
                feedbackMessage.created_at = DateTime.Now;
                feedbackMessage.updated_at = DateTime.Now;
                feedbackMessage.isdeleted = false;
            }
        }

        private void ProcessBroadcastMessage(Feedback feedback, string broadcastId)
        {
            foreach (var feedbackMessage in feedback.FeedbackMessages)
            {
                feedbackMessage.responderId = feedback.responderId;
                feedbackMessage.responderEmail = feedback.responderEmail;
                feedbackMessage.created_at = DateTime.Now;
                feedbackMessage.updated_at = DateTime.Now;
                feedbackMessage.isdeleted = false;
                feedbackMessage.broadcastId = broadcastId;
            }
        }

        private bool FeedbackExists(int id)
        {
            return _context.Feedbacks.Any(e => e.feedbackId == id);
        }

        #endregion
    }
}
