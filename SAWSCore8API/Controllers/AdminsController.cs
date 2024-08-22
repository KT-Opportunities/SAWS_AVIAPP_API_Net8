using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SAWSCore8API.Models;
using SAWSCore8API.DbContexts;
using SAWSCore8API.Interfaces;
using Humanizer;

namespace SAWSCore8API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class AdminsController : ControllerBase
    {
        #region Fields
        private readonly ISawsService _sawsService;
        private readonly SAWSDbContext _context;
        private ILogger<AdminsController> _logger;

        #endregion

        #region Constructors
        public AdminsController(
            SAWSDbContext context,
            ISawsService sawsService,
            ILogger<AdminsController> logger
            )
        {
            _context = context;
            _sawsService = sawsService;
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
                var pagedAdmins = await _sawsService.GetPagedAllUsers(filter, role);
                
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
