using Microsoft.AspNetCore.Mvc;
using SAWSCore8API.Models;
using SAWSCore8API.DbContexts;

namespace SAWSCore8API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class RawSourceController : ControllerBase
    {
        #region Fields
        private readonly SAWSDbContext _context;

        #endregion

        #region Constructors

        public RawSourceController(SAWSDbContext context)
        {
            _context = context;
        }

        #endregion

        // GET: api/<RawSourceController>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<RawSourceController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<RawSourceController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<RawSourceController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<RawSourceController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
