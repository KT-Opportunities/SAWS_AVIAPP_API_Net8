using Microsoft.AspNetCore.Mvc;
using SAWSCore8API.Models;
using SAWSCore8API.DbContexts;
using SAWSCore8API.Interfaces;

namespace SAWSCore8API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class UserManagerController : Controller
    {
        #region Fields
        private readonly IPagedService _pagedService;
        private readonly SAWSDbContext _context;
        private ILogger<AdminsController> _logger;

        #endregion

        #region Constructors
        public UserManagerController(
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

        #region DeletedUsers

        [HttpGet("GetPagedDeletedUsers")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]

        public async Task<IActionResult> GetPagedDeletedUsers([FromQuery] PaginationFilter filter)
        {
            if (filter == null)
            {
                return new BadRequestResult();
            }

            try
            {
                var deletedUsers = await _pagedService.GetPagedAllDeletedUsers(filter);

                return new OkObjectResult(deletedUsers);
            }
            catch (Exception ex) 
            {
                _logger.LogError(ex, "unable to get paged deleted users");
                return Problem(ex.Message);
            }
        
        }

        #endregion
    }
}
