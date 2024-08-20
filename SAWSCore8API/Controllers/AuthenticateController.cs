using Microsoft.AspNetCore.Mvc;
using SAWSCore8API.Models;
using SAWSCore8API.DbContexts;
using Microsoft.AspNetCore.Identity;
using SAWSCore8API.Interfaces;
using SAWSCore8API.Dtos;
using System.Net.Mime;
using SAWSCore8API.Dto;
using SAWSCore8API.Services;

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
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status202Accepted, Type = typeof(RegisterSubscriber))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResponseDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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
                        Message = "Successfully updated user",
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
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception from AuthenticationController.UpdateUserProfile");
                return Problem("Unable to process the user update.");
            }
        }

        [HttpDelete("DeleteUserProfileById")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(DeleteResult))]
        // [Authorize(Roles.Administrator)]
        public async Task<IActionResult> DeleteUserProfileById(int id)
        {
            try
            {
                if (!UserIdExists(id))
                {
                    return NotFound();
                }

                var result = await _authenticateService.DeleteUserProfileById(id);

                if (result.Success)
                {
                    return Ok(result);
                } else
                {
                    return new BadRequestResult();
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception from AuthenticateController.DeleteUserProfileById");
                return Problem("Unable to Delete the user");
            }

        }

        [HttpGet("GetLoggedInUser")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(LoggedInResult))]
        public async Task<IActionResult> GetLoggedInUser(string id)
        {
            try
            {
                var result = await _authenticateService.GetLoggedInUser(id);

                if (result.Success)
                {
                    return Ok(result);
                }
                else
                {
                    return new NotFoundResult();
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception from AuthenticateController.GetLoggedInUser");
                return Problem("Unable to get logged in user");
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

        private bool UserIdExists(int id)
        {
            return _context.userProfiles.Any(e => e.userprofileid == id);
        }

        #endregion
    }
}
