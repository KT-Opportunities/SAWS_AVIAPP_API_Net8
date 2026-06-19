using Microsoft.AspNetCore.Mvc;
using SAWSCore8API.Models;
using SAWSCore8API.DbContexts;
using SAWSCore8API.Interfaces;

namespace SAWSCore8API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class SubscribersController : ControllerBase
    {
        #region Fields
        private readonly IPagedService _pagedService;
        private readonly SAWSDbContext _context;
        private ILogger<SubscribersController> _logger;
        private readonly IActivityLoggerService _activityLogger;

        #endregion

        #region Constructors
        public SubscribersController(
            SAWSDbContext context,
            IPagedService pagedService,
            ILogger<SubscribersController> logger,
            IActivityLoggerService activityLogger
        )
        {
            _context = context;
            _pagedService = pagedService;
            _logger = logger;
            _activityLogger = activityLogger;
        }

        #endregion

        #region Subscriber

        [HttpGet("GetPagedAllSubscribers")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetPagedAllSubscribers([FromQuery] PaginationFilter filter)
        {
            if (filter == null)
            {
                return new BadRequestResult();
            }

            var role = "Subscriber";

            try
            {
                var pagedSubscribers = await _pagedService.GetPagedAllUsers(filter, role);
                return new OkObjectResult(pagedSubscribers);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to get paged subscribers");
                return Problem("Unable to get paged subscribers");
            }
        }

        #endregion
    }
}
