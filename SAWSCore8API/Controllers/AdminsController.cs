using Microsoft.AspNetCore.Mvc;
using SAWSCore8API.Models;
using SAWSCore8API.DbContexts;
using SAWSCore8API.Interfaces;

namespace SAWSCore8API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class AdminsController : ControllerBase
    {
        #region Fields
        private readonly IPagedService _pagedService;
        private readonly SAWSDbContext _context;
        private ILogger<AdminsController> _logger;
        private readonly IActivityLoggerService _activityLogger;

        #endregion

        #region Constructors
        public AdminsController(
            SAWSDbContext context,
            IPagedService pagedService,
            ILogger<AdminsController> logger
            )
        {
            _context = context;
            _pagedService = pagedService;
            _logger = logger;
        }

        #endregion

        #region Admins

        [HttpGet("GetPagedAllAdmins")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetPagedAllAdmins([FromQuery] PaginationFilter filter)
        {
            if (filter == null)
            {
                return new BadRequestResult();
            }

            var role = "Admin";

            try
            {
                var pagedAdmins = await _pagedService.GetPagedAllUsers(filter, role);
                
                return new OkObjectResult(pagedAdmins);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to get paged admins");
                return Problem("Unable to get paged admins");
            }
        }


        [HttpGet("GetPagedAllActivityLogs")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetPagedAllActivityLogs([FromQuery] PaginationFilter filter)
        {
            if (filter == null)
            {
                return new BadRequestResult();
            }

            var role = "Admin";

            try
            {
                var pagedAdmins = await _pagedService.GetPagedAllActivityLogs(filter);

                return new OkObjectResult(pagedAdmins);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to get paged admins");
                return Problem("Unable to get paged admins");
            }
        }

        #endregion
    }
}
