using Microsoft.AspNetCore.Mvc;
using SAWSCore8API.Models;
using SAWSCore8API.DbContexts;
using SAWSCore8API.Interfaces;
using SAWSCore8API.Dtos;
using System.Net.Mime;

namespace SAWSCore8API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class AdvertsController : ControllerBase
    {

        #region Fields
        private readonly SAWSDbContext _context;
        private readonly IPagedService _pagedService;
        private readonly IAdvertService _advertService;
        private ILogger<AdvertsController> _logger;
        public IConfiguration _configuration { get; }

        #endregion

        #region Constructors

        public AdvertsController(
            SAWSDbContext context,
            IPagedService pagedService,
            IAdvertService advertService,
            ILogger<AdvertsController> logger,
            IConfiguration configuration
            )
        {
            _context = context;
            _pagedService = pagedService;
            _advertService = advertService;
            _logger = logger;
            _configuration = configuration;
        }

        #endregion

        #region Adverts

        [HttpGet("GetPagedAllAdverts")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetPagedAllAdverts([FromQuery] PaginationFilter filter)
        {
            if (filter == null)
            {
                return new BadRequestResult();
            }

            try
            {
                var pagedAdverts = await _pagedService.GetPagedAllAdverts(filter);
                return new OkObjectResult(pagedAdverts);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to get paged adverts");
                return Problem("Unable to get paged adverts");
            }
        }

        [HttpPost("PostInsertNewAdvert")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status202Accepted, Type = typeof(Advert))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(UpdateResult))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PostInsertNewAdvert(Advert advert)
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

            try
            {
                if (advert.advertId == 0)
                {
                    // Creating new advert
                    var newAdvertResult = await _advertService.CreateAdvert(advert);

                    if (newAdvertResult.Success)
                    {
                        // return Ok(new Response
                        // {
                        //     Status = "Success",
                        //     Message = "Successfully added new advert",
                        //     DetailDescription = advert
                        // });
                        
                        return Ok(newAdvertResult);
                    }

                    return BadRequest(new CreateResult
                    {
                        Success = false,
                        ErrorMessages = new Dictionary<string, IEnumerable<string>>
                            {
                                { "General", new[] { "Failed to insert advert. Invalid condition." } }
                            }
                    });
                }
                else
                {
                    // Updating existing advert
                    if (!AdvertExists(advert.advertId))
                    {
                        return NotFound();
                    }

                    var updateAdvertResult = await _advertService.UpdateAdvert(advert);

                    if (updateAdvertResult.Success)
                    {
                        // return Ok(new Response
                        // {
                        //     Status = "Success",
                        //     Message = "Successfully updated advert",
                        //     DetailDescription = advert
                        // });
                        return Ok(updateAdvertResult);
                    }

                    return BadRequest(new CreateResult
                    {
                        Success = false,
                        ErrorMessages = new Dictionary<string, IEnumerable<string>>
                            {
                                { "General", new[] { "Failed to update advert. Invalid condition." } }
                            }
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception from AdvertsController.PostInsertNewAdvert");
                return Problem("Unable to process the advert.");
            }
        }

        [HttpPost("PostInsertAdvertClick")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status202Accepted, Type = typeof(AdvertClick))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(CreateResult))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PostInsertAdvertClick(AdvertClick click)
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

            try
            {
                if (click.advertClickId == 0)
                {
                    // Creating new click
                    var newAdvertClickResult = await _advertService.AddAdvertClick(click);

                    if (newAdvertClickResult.Success)
                    {
                        return Ok(new ResponseDto
                        {
                            Status = "Success",
                            Message = "Successfully added new advert click",
                        });
                    }

                    return BadRequest(new CreateResult
                    {
                        Success = false,
                        ErrorMessages = new Dictionary<string, IEnumerable<string>>
                            {
                                { "General", new[] { "Failed to insert advert click. Invalid condition." } }
                            }
                    });
                }
                return new BadRequestObjectResult("Failed to add click");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception from AdvertsController.PostInsertAdvertClick");
                return Problem("Unable to process the advert click.");
            }
        }


        [HttpGet("GetAllAdverts")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult GetAllAdverts()
        {
            try
            {
                var adverts = _advertService.GetAllAdverts();

                var app_url = _configuration["AppURL"];
                var host_location = _configuration["HostLocation"];

                var toReturn = adverts.Select(ad => new AdvertDto
                {
                    advertId = ad.advertId,
                    advert_caption = ad.advert_caption,
                    advert_url = ad.advert_url,
                    file_url = app_url + host_location + "/" + ad.DocAdverts.FirstOrDefault()?.DocTypeName + "/" + ad.advertId + "/" + ad.DocAdverts.FirstOrDefault()?.file_origname
                }).ToList();

                return new OkObjectResult(toReturn);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception from AdvertsController.GetAllAdverts");
                return Problem("Unable to get the adverts");
            }
        }

        [HttpGet("GetAdvertByAdvertId")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Advert))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetAdvertByAdvertId(int id)
        {
            var app_url = _configuration["AppURL"];
            var host_location = _configuration["HostLocation"];

            try
            {
                var advert = _advertService.GetAdvertByAdvertId(id);
                if (advert == null)
                {
                    return NotFound();
                }

                string fileUrl = app_url + host_location + "/"  + advert.DocAdverts.FirstOrDefault()?.DocTypeName + "/" + advert.advertId + "/" + advert.DocAdverts.FirstOrDefault()?.file_origname;

                return Ok(new Response
                {
                    Status = "Success",
                    Message = "Successfully returned advert",
                    DetailDescription = new
                    {
                        Advert = advert,
                        FileUrl = fileUrl
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception from AdvertsController.GetAdvertByAdvertId");
                return Problem("Unable to Get the advert");
            }
        }

        [HttpDelete("DeleteAdvertById")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(DeleteResult))]
        // [Authorize(Roles.Administrator)]
        public async Task<IActionResult> DeleteAdvertById(int id)
        {
            try
            {
                if (!AdvertExists(id))
                {
                    return NotFound();
                }

                var result = await _advertService.DeleteAdvertById(id);

                if (result.Success)
                {
                    return Ok(result);
                }
                else
                {
                    return new BadRequestResult();
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception from AdvertsController.DeleteAdvertById");
                return Problem("Unable to Delete the advert");
            }

        }

        #endregion

        #region Helper Methods

        private bool AdvertExists(int id)
        {
            return _context.Adverts.Any(e => e.advertId == id);
        }

        #endregion
    }
}
