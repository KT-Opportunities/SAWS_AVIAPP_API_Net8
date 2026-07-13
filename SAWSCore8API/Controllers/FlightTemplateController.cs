using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SAWSCore8API.DbContexts;
using SAWSCore8API.DTOs;
using SAWSCore8API.DTOs.FlightTemplates;
using SAWSCore8API.Models;
using System.Security.Claims;

namespace SAWSCore8API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize]
    public class FlightTemplateController : ControllerBase
    {
        private readonly SAWSDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public FlightTemplateController(
            SAWSDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // POST: api/v1/FlightTemplate
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateFlightTemplateRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Username/email comes from the JWT
            var username = User.FindFirstValue(ClaimTypes.Name);

            if (string.IsNullOrWhiteSpace(username))
                return Unauthorized("Username not found in token.");

            // Find the Identity user
            var user = await _userManager.FindByNameAsync(username);

            if (user == null)
                return Unauthorized("User not found.");

            var template = new FlightTemplate
            {
                templateName = request.templateName,
                flightNumber = request.flightNumber,
                departureICAO = request.departureICAO,
                enRouteICAO = request.enRouteICAO,
                destinationICAO = request.destinationICAO,
                etd = request.etd,
                ete = request.ete,
                createdby_aspnetuserId = user.Id,
                createdby_aspnetusername = user.UserName,
                created_at = DateTime.Now,
                updated_at = DateTime.Now,
                isdeleted = false
            };

            _context.FlightTemplates.Add(template);
            await _context.SaveChangesAsync();

            return Ok(template);
        }

        // GET: api/v1/FlightTemplate
        [HttpGet]
        public async Task<IActionResult> GetTemplates()
        {
            var username = User.FindFirstValue(ClaimTypes.Name);

            if (string.IsNullOrWhiteSpace(username))
                return Unauthorized("Username not found in token.");

            var user = await _userManager.FindByNameAsync(username);

            if (user == null)
                return Unauthorized("User not found.");

            var templates = await _context.FlightTemplates
                .Where(x =>
                    x.createdby_aspnetuserId == user.Id &&
                    x.isdeleted == false)
                .OrderBy(x => x.templateName)
                .ToListAsync();

            return Ok(templates);
        }

        // DELETE: api/v1/FlightTemplate/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var username = User.FindFirstValue(ClaimTypes.Name);

            if (string.IsNullOrWhiteSpace(username))
                return Unauthorized("Username not found in token.");

            var user = await _userManager.FindByNameAsync(username);

            if (user == null)
                return Unauthorized("User not found.");

            var template = await _context.FlightTemplates.FirstOrDefaultAsync(x =>
                x.flightTemplateId == id &&
                x.createdby_aspnetuserId == user.Id &&
                x.isdeleted == false);

            if (template == null)
                return NotFound("Template not found.");

            template.isdeleted = true;
            template.deleted_at = DateTime.Now;
            template.updated_at = DateTime.Now;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Template deleted successfully."
            });
        }

        // GET: api/v1/FlightTemplate/claims
        [HttpGet("claims")]
        public IActionResult GetClaims()
        {
            return Ok(User.Claims.Select(c => new
            {
                c.Type,
                c.Value
            }));
        }
    }
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize]
    public class OperationalSettingsController : ControllerBase
    {
        private readonly SAWSDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public OperationalSettingsController(
            SAWSDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpPost]
        public async Task<IActionResult> Save(
            [FromBody] SaveOperationalSettingsRequest request)
        {
            var username = User.FindFirstValue(ClaimTypes.Name);

            var user = await _userManager.FindByNameAsync(username);

            if (user == null)
                return Unauthorized();

            var settings = await _context.OperationalSettings
                .FirstOrDefaultAsync(x =>
                    x.createdby_aspnetuserId == user.Id &&
                    !x.isdeleted);

            if (settings == null)
            {
                settings = new OperationalSettings
                {
                    PilotName = request.PilotName,
                    PilotLicense = request.PilotLicense,
                    DispatcherName = request.DispatcherName,
                    DispatcherLicense = request.DispatcherLicense,
                    createdby_aspnetuserId = user.Id,
                    createdby_aspnetusername = user.UserName,
                    created_at = DateTime.Now,
                    updated_at = DateTime.Now,
                    isdeleted = false
                };

                _context.OperationalSettings.Add(settings);
            }
            else
            {
                settings.PilotName = request.PilotName;
                settings.PilotLicense = request.PilotLicense;
                settings.DispatcherName = request.DispatcherName;
                settings.DispatcherLicense = request.DispatcherLicense;
                settings.updated_at = DateTime.Now;
            }

            await _context.SaveChangesAsync();

            return Ok(settings);
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var username = User.FindFirstValue(ClaimTypes.Name);

            var user = await _userManager.FindByNameAsync(username);

            if (user == null)
                return Unauthorized();

            var settings = await _context.OperationalSettings
                .FirstOrDefaultAsync(x =>
                    x.createdby_aspnetuserId == user.Id &&
                    !x.isdeleted);

            return Ok(settings);
        }
    }
}