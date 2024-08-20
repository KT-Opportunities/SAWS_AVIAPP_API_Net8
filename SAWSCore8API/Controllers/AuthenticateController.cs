using Microsoft.AspNetCore.Mvc;
using SAWSCore8API.Models;
using SAWSCore8API.DbContexts;
using Microsoft.AspNetCore.Identity;
using SAWSCore8API.Interfaces;
using SAWSCore8API.Dtos;
using System.Net.Mime;
using SAWSCore8API.Dto;

namespace SAWSCore8API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class AuthenticateController : ControllerBase
    {

        #region Fields

        private readonly SAWSDbContext _context;
        private ILogger<AuthenticateController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IAuthenticateService _authenticateService;

        #endregion

        #region Constructors

        public AuthenticateController(
            SAWSDbContext context,
            ILogger<AuthenticateController> logger,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            IAuthenticateService authenticateService
            )
        {
            _context = context;
            _logger = logger;
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _authenticateService = authenticateService;
        }

        #endregion

        [HttpPost("Login")]
        public async Task<IActionResult> Login(Login appUser)
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
                var loginResult = await _authenticateService.LoginUser(appUser);
                if (!loginResult.Success)
                {
                    var firstErrorMessage = loginResult?.ErrorMessages?.FirstOrDefault().Value.FirstOrDefault();

                    return Unauthorized(LoginResultDto.FailureResult(firstErrorMessage));
                }

                return Ok(loginResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception from AuthenticateController.Login");
                return Problem("Unable to process the user login.");
            }
        }

        [HttpPost("RegisterAdmin")]
        public async Task<IActionResult> RegisterAdmin(RegisterAdmin appUser)
        {
            if (!ModelState.IsValid)
            {
                return new BadRequestResult();
            }

            if (UserExists(appUser.Email))
            {
                return new BadRequestObjectResult(new ResponseDto
                {
                    Status = "Failed",
                    Message = "User already exist",
                });
            }

            try
            {
                var newAdminResult = await _authenticateService.AddAdminUserProfile(appUser);

                if (!newAdminResult.Success)
                {
                    return new BadRequestObjectResult("Admin user not added");
                }

                return Ok(newAdminResult);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception from AuthenticateController.RegisterAdmin");
                return Problem("Unable to process the admin user registration.");
            }
        }

        [HttpPost("RegisterSubscriber")]
        public async Task<IActionResult> RegisterSubscriber(RegisterSubscriber appUser)
        {
            if (!ModelState.IsValid)
            {
                return new BadRequestResult();
            }

            if (UserExists(appUser.Email))
            {
                return new BadRequestObjectResult(new ResponseDto
                {
                    Status = "Failed",
                    Message = "User already exist",
                });
            }

            try
            {
                var newSubscriberResult = await _authenticateService.AddSubscriberUserProfile(appUser);

                if (!newSubscriberResult.Success)
                {
                    return new BadRequestObjectResult("Subscriber user not added");
                }

                return Ok(newSubscriberResult);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception from AuthenticateController.RegisterSubscriber");
                return Problem("Unable to process the subscriber user registration.");
            }
        }

        [HttpPost("UpdateUserProfile")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status202Accepted, Type = typeof(UserProfile))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(UpdateResult))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateUserProfile(UserProfile userProfile)
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
                if (!UserExists(userProfile.email))
                {
                    return NotFound();
                }

                var updateUserProfileResult = await _authenticateService.UpdateUserProfile(userProfile);

                if (updateUserProfileResult.Success)
                {
                    await _authenticateService.UpdateIdentityEmail(userProfile);

                    return Ok(new Response
                    {
                        Status = "Success",
                        Message = "Successfully updated admin user",
                        DetailDescription = userProfile
                    });
                }

                return BadRequest(new CreateResult
                {
                    Success = false,
                    ErrorMessages = new Dictionary<string, IEnumerable<string>>
                            {
                                { "General", new[] { "Failed to update admin user. Invalid condition." } }
                            }
                });
                // }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception from AdvertsController.PostInsertNewAdvert");
                return Problem("Unable to process the advert.");
            }
        }

        // [HttpPost("InsertUpdateUserProfile")]
        // [Consumes(MediaTypeNames.Application.Json)]
        // [ProducesResponseType(StatusCodes.Status202Accepted, Type = typeof(UserProfile))]
        // [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(UpdateResult))]
        // [ProducesResponseType(StatusCodes.Status404NotFound)]
        // public async Task<IActionResult> InsertUpdateUserProfile(UserProfile userProfile)
        // {
        //     if (!ModelState.IsValid)
        //     {
        //         var errorMessages = ModelState.ToDictionary(
        //             kvp => kvp.Key,
        //             kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).AsEnumerable()
        //         );

        //         return BadRequest(new CreateResult
        //         {
        //             Success = false,
        //             ErrorMessages = errorMessages
        //         });
        //     }

        //     try
        //     {
        //         if (userProfile.userprofileid == 0)
        //         {
        //             // Creating new userprofile
        //             var newUserProfileResult = await _authenticateService.AddUserProfile(userProfile);

        //             if (newUserProfileResult.Success)
        //             {
        //                 await _authenticateService.UpdateIdentityEmail(userProfile);

        //                 return Ok(new Response
        //                 {
        //                     Status = "Success",
        //                     Message = "Successfully added new admin user",
        //                     DetailDescription = userProfile
        //                 });
        //             }

        //             return BadRequest(new CreateResult
        //             {
        //                 Success = false,
        //                 ErrorMessages = new Dictionary<string, IEnumerable<string>>
        //                     {
        //                         { "General", new[] { "Failed to insert admin user. Invalid condition." } }
        //                     }
        //             });
        //         }
        //         else
        //         {
        //             // Updating existing advert
        //             if (!UserExists(userProfile.email))
        //             {
        //                 return NotFound();
        //             }

        //             var updateUserProfileResult = await _authenticateService.UpdateUserProfile(userProfile);

        //             if (updateUserProfileResult.Success)
        //             {
        //                 await _authenticateService.UpdateIdentityEmail(userProfile);

        //                 return Ok(new Response
        //                 {
        //                     Status = "Success",
        //                     Message = "Successfully updated admin user",
        //                     DetailDescription = userProfile
        //                 });
        //             }

        //             return BadRequest(new CreateResult
        //             {
        //                 Success = false,
        //                 ErrorMessages = new Dictionary<string, IEnumerable<string>>
        //                     {
        //                         { "General", new[] { "Failed to update admin user. Invalid condition." } }
        //                     }
        //             });
        //         }
        //     }
        //     catch (Exception ex)
        //     {
        //         _logger.LogError(ex, "Unhandled exception from AdvertsController.PostInsertNewAdvert");
        //         return Problem("Unable to process the advert.");
        //     }
        // }


        #region Helper Methods
        private bool UserExists(string email)
        {
            return _context.User.Any(e => e.UserName == email || e.Email == email);
        }

        #endregion


        // GET: api/<AuthenticateController>
        /*     [HttpGet]

             public IEnumerable<string> Get()
             {
                 return new string[] { "value1", "value2" };
             }*/

        // GET api/<AuthenticateController>/5
        /*     [HttpGet("{id}")]
             public string Get(int id)
             {
                 return "value";
             }*/

        // POST api/<AuthenticateController>
        /*    [HttpPost]
            public void Post([FromBody] string value)
            {
            }*/

        // PUT api/<AuthenticateController>/5
        /*        [HttpPut("{id}")]

                public void Put(int id, [FromBody] string value)
                {
                }*/

        // DELETE api/<AuthenticateController>/5
        /*        [HttpDelete("{id}")]
                public void Delete(int id)
                {
                }*/
    }
}
