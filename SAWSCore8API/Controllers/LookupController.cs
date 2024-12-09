using Microsoft.AspNetCore.Mvc;
using SAWSCore8API.Models;
using SAWSCore8API.DbContexts;

namespace SAWSCore8API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class LookupController : ControllerBase
    {

        #region Fields
        private readonly SAWSDbContext _context;

        #endregion

        #region Constructors

        public LookupController(SAWSDbContext context)
        {
            _context = context;
        }

        #endregion

        // GET: api/<LookupController>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<LookupController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<LookupController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<LookupController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<LookupController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
