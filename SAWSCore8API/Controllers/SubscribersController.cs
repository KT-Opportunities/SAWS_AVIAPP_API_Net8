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
        private readonly ISawsService _sawsService;
        private readonly SAWSDbContext _context;
        private ILogger<SubscribersController> _logger;

        #endregion

        #region Constructors
        public SubscribersController(
            SAWSDbContext context,
            ISawsService sawsService,
            ILogger<SubscribersController> logger
        )
        {
            _context = context;
            _sawsService = sawsService;
            _logger = logger;
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
                var pagedSubscribers = await _sawsService.GetPagedAllUsers(filter, role);
                return new OkObjectResult(pagedSubscribers);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to get paged subscribers");
                return Problem("Unable to get paged subscribers");
            }
        }

        #endregion

        #region PayFast

        #endregion


        // GET: api/<SubscribersController>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<SubscribersController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<SubscribersController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<SubscribersController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<SubscribersController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
