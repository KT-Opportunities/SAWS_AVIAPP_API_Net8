using DeviceDetectorNET;
using DeviceDetectorNET.Class;
using DeviceDetectorNET.Parser;
using DeviceDetectorNET.Parser.Device;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using NuGet.Protocol.Core.Types;
using SAWSCore8API.DbContexts;
using SAWSCore8API.Dto;
using SAWSCore8API.Dtos;
using SAWSCore8API.Interfaces;
using SAWSCore8API.Models;
using System.Data;
using System.Net.Mime;
using System.Xml.Linq;

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
        private readonly IActivityLoggerService _activityLogger;
        #endregion

        #region Constructors

        public AuthenticateController(
            SAWSDbContext context,
            ILogger<AuthenticateController> logger,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            IAuthenticateService authenticateService,
            IActivityLoggerService activityLogger
            )
        {
            _context = context;
            _logger = logger;
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _authenticateService = authenticateService;
            _activityLogger = activityLogger;
        }

        #endregion

        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginModel appUser)
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

            //get device details;
            string clientIp = string.Empty;
            string userAgent = string.Empty;
            DeviceDetector dd = new DeviceDetector();
            ActivityLog alog = new ActivityLog();
            try
            {
                var remoteIp = HttpContext.Connection.RemoteIpAddress;
                clientIp = remoteIp?.IsIPv4MappedToIPv6 == true
                                                    ? remoteIp.MapToIPv4().ToString()
                                                    : remoteIp?.ToString();

                userAgent = Request.Headers["User-Agent"].ToString();

                if (string.IsNullOrWhiteSpace(userAgent))
                    return BadRequest("User-Agent header is missing.");

                // Optional: Client Hints (modern browsers)
                var headers = Request.Headers.ToDictionary(
                    h => h.Key,
                    h => h.Value.ToString());

                var clientHints = ClientHints.Factory(headers);

                dd = new DeviceDetector(userAgent, clientHints);
                dd.Parse();
            }
            catch (Exception ex)
            {
                //
            }

            try
            {
                var loginResult = await _authenticateService.LoginUser(appUser);
                if (!loginResult.Success)
                {
                    var firstErrorMessage = loginResult?.ErrorMessages?.FirstOrDefault().Value.FirstOrDefault();

                    //get
                    alog.activityLogId = 0;
                    alog.activityType = "login";
                    alog.remoteipaddress = clientIp;
                    alog.activityAction = "";//??
                    alog.activityDescription = "Failed login attempt";
                    alog.createdby_aspnetusername = appUser.Username;
                    alog.createdby_aspnetuserId = ""; //appUser.Id.ToString();
                    //device details
                    alog.UserAgent = userAgent;
                    alog.IsMobile = dd.IsMobile();
                    alog.DeviceType = dd.GetDeviceName() ?? "unknown";
                    alog.Brand = dd.GetBrand() ?? "unknown";
                    alog.Model = dd.GetModel() ?? "unknown";
                    alog.OsName = dd.GetOs().Match?.Name ?? "unknown";
                    alog.OsVersion = dd.GetOs().Match?.Version ?? "unknown";
                    alog.BrowserName = dd.GetClient().Match?.Name ?? "unknown";
                    alog.BrowserVersion = dd.GetClient().Match?.Version ?? "unknown";
                    await _activityLogger.LogAsync(HttpContext, alog);
                    return Unauthorized(LoginResultDto.FailureResult(firstErrorMessage));
                }


                alog.activityLogId = 0;
                alog.activityType = "login";
                alog.remoteipaddress = clientIp;
                alog.activityAction = "";//??
                alog.activityDescription = "User logged in the application";
                alog.createdby_aspnetusername = appUser.Username;
                alog.createdby_aspnetuserId = loginResult.AspUserId;//appUser.Id.ToString();
                //device details
                alog.UserAgent = userAgent;
                alog.IsMobile = dd.IsMobile();
                alog.DeviceType = dd.GetDeviceName() ?? "unknown";
                alog.Brand = dd.GetBrand() ?? "unknown";
                alog.Model = dd.GetModel() ?? "unknown";
                alog.OsName = dd.GetOs().Match?.Name ?? "unknown";
                alog.OsVersion = dd.GetOs().Match?.Version ?? "unknown";
                alog.BrowserName = dd.GetClient().Match?.Name ?? "unknown";
                alog.BrowserVersion = dd.GetClient().Match?.Version ?? "unknown";
                await _activityLogger.LogAsync(HttpContext, alog);


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

                //log initialisation
                string clientIp = string.Empty;
                string userAgent = string.Empty;
                DeviceDetector dd = new DeviceDetector();
                ActivityLog alog = new ActivityLog();
                try
                {
                    var remoteIp = HttpContext.Connection.RemoteIpAddress;
                    clientIp = remoteIp?.IsIPv4MappedToIPv6 == true
                                                        ? remoteIp.MapToIPv4().ToString()
                                                        : remoteIp?.ToString();

                    userAgent = Request.Headers["User-Agent"].ToString();

                    if (string.IsNullOrWhiteSpace(userAgent))
                        return BadRequest("User-Agent header is missing.");

                    // Optional: Client Hints (modern browsers)
                    var headers = Request.Headers.ToDictionary(
                        h => h.Key,
                        h => h.Value.ToString());

                    var clientHints = ClientHints.Factory(headers);

                    dd = new DeviceDetector(userAgent, clientHints);
                    dd.Parse();
                }
                catch (Exception ex)
                {
                    //
                }



                alog.activityLogId = 0;
                alog.activityType = "RegisterAdmin";
                alog.remoteipaddress = clientIp;
                alog.activityAction = "";//??
                alog.activityDescription = "Admin user added successfully";
                alog.createdby_aspnetusername = appUser.Username;//TODO:modify method to require Authorize and pull user info
                alog.createdby_aspnetuserId = "";//loginResult.AspUserId;//TODO:modify method to require Authorize and pull user info
                                                 //device details
                alog.UserAgent = userAgent;
                alog.IsMobile = dd.IsMobile();
                alog.DeviceType = dd.GetDeviceName() ?? "unknown";
                alog.Brand = dd.GetBrand() ?? "unknown";
                alog.Model = dd.GetModel() ?? "unknown";
                alog.OsName = dd.GetOs().Match?.Name ?? "unknown";
                alog.OsVersion = dd.GetOs().Match?.Version ?? "unknown";
                alog.BrowserName = dd.GetClient().Match?.Name ?? "unknown";
                alog.BrowserVersion = dd.GetClient().Match?.Version ?? "unknown";
                try
                {
                    await _activityLogger.LogAsync(HttpContext, alog);
                }
                catch (Exception ex)
                {
                    //nlog the issue
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

            //log initialisation
            string clientIp = string.Empty;
            string userAgent = string.Empty;
            DeviceDetector dd = new DeviceDetector();
            ActivityLog alog = new ActivityLog();
            try
            {
                var remoteIp = HttpContext.Connection.RemoteIpAddress;
                clientIp = remoteIp?.IsIPv4MappedToIPv6 == true
                                                    ? remoteIp.MapToIPv4().ToString()
                                                    : remoteIp?.ToString();

                userAgent = Request.Headers["User-Agent"].ToString();

                if (string.IsNullOrWhiteSpace(userAgent))
                    return BadRequest("User-Agent header is missing.");

                // Optional: Client Hints (modern browsers)
                var headers = Request.Headers.ToDictionary(
                    h => h.Key,
                    h => h.Value.ToString());

                var clientHints = ClientHints.Factory(headers);

                dd = new DeviceDetector(userAgent, clientHints);
                dd.Parse();
            }
            catch (Exception ex)
            {
                //
            }


            if (UserExists(appUser.Email))
            {
                alog.activityLogId = 0;
                alog.activityType = "RegisterSubscriber";
                alog.remoteipaddress = clientIp;
                alog.activityAction = "";//??
                alog.activityDescription = "Subscriber user email already exist";
                alog.createdby_aspnetusername = appUser.Username;//TODO:modify method to require Authorize and pull user info
                alog.createdby_aspnetuserId = "";//loginResult.AspUserId;//TODO:modify method to require Authorize and pull user info
                                                 //device details
                alog.UserAgent = userAgent;
                alog.IsMobile = dd.IsMobile();
                alog.DeviceType = dd.GetDeviceName() ?? "unknown";
                alog.Brand = dd.GetBrand() ?? "unknown";
                alog.Model = dd.GetModel() ?? "unknown";
                alog.OsName = dd.GetOs().Match?.Name ?? "unknown";
                alog.OsVersion = dd.GetOs().Match?.Version ?? "unknown";
                alog.BrowserName = dd.GetClient().Match?.Name ?? "unknown";
                alog.BrowserVersion = dd.GetClient().Match?.Version ?? "unknown";
                try
                {
                    await _activityLogger.LogAsync(HttpContext, alog);
                }
                catch (Exception ex)
                {
                    //nlog the issue
                }

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

                


                alog.activityLogId = 0;
                alog.activityType = "RegisterSubscriber";
                alog.remoteipaddress = clientIp;
                alog.activityAction = "";//??
                alog.activityDescription = "Subscriber added successfully";
                alog.createdby_aspnetusername = appUser.Username;//TODO:modify method to require Authorize and pull user info
                alog.createdby_aspnetuserId = "";//loginResult.AspUserId;//TODO:modify method to require Authorize and pull user info
                                                 //device details
                alog.UserAgent = userAgent;
                alog.IsMobile = dd.IsMobile();
                alog.DeviceType = dd.GetDeviceName() ?? "unknown";
                alog.Brand = dd.GetBrand() ?? "unknown";
                alog.Model = dd.GetModel() ?? "unknown";
                alog.OsName = dd.GetOs().Match?.Name ?? "unknown";
                alog.OsVersion = dd.GetOs().Match?.Version ?? "unknown";
                alog.BrowserName = dd.GetClient().Match?.Name ?? "unknown";
                alog.BrowserVersion = dd.GetClient().Match?.Version ?? "unknown";
                try
                {
                    await _activityLogger.LogAsync(HttpContext, alog);
                }
                catch (Exception ex)
                {
                    //nlog the issue
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
        //[Authorize(Roles.Administrator)]
        // make sure that only admins can delete and update profile
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
/*                if (!UserExists(userProfile.email))
                {
                    return NotFound();
                }*/
                
                var updateUserProfileResult = await _authenticateService.UpdateUserProfile(userProfile);

                if (updateUserProfileResult.Success)
                {
                    await _authenticateService.UpdateIdentityEmail(userProfile);


                    //log initialisation
                    string clientIp = string.Empty;
                    string userAgent = string.Empty;
                    DeviceDetector dd = new DeviceDetector();
                    ActivityLog alog = new ActivityLog();
                    try
                    {
                        var remoteIp = HttpContext.Connection.RemoteIpAddress;
                        clientIp = remoteIp?.IsIPv4MappedToIPv6 == true
                                                            ? remoteIp.MapToIPv4().ToString()
                                                            : remoteIp?.ToString();

                        userAgent = Request.Headers["User-Agent"].ToString();

                        if (string.IsNullOrWhiteSpace(userAgent))
                            return BadRequest("User-Agent header is missing.");

                        // Optional: Client Hints (modern browsers)
                        var headers = Request.Headers.ToDictionary(
                            h => h.Key,
                            h => h.Value.ToString());

                        var clientHints = ClientHints.Factory(headers);

                        dd = new DeviceDetector(userAgent, clientHints);
                        dd.Parse();
                    }
                    catch (Exception ex)
                    {
                        //
                    }

                    alog.activityLogId = 0;
                    alog.activityType = "UpdateUserProfile";
                    alog.remoteipaddress = clientIp;
                    alog.activityAction = "";//??
                    alog.activityDescription = "Updating user profile successfull";
                    alog.createdby_aspnetusername = userProfile.email;//TODO:modify method to require Authorize and pull user info
                    alog.createdby_aspnetuserId = userProfile.aspuid;//loginResult.AspUserId;//TODO:modify method to require Authorize and pull user info
                                                     //device details
                    alog.UserAgent = userAgent;
                    alog.IsMobile = dd.IsMobile();
                    alog.DeviceType = dd.GetDeviceName() ?? "unknown";
                    alog.Brand = dd.GetBrand() ?? "unknown";
                    alog.Model = dd.GetModel() ?? "unknown";
                    alog.OsName = dd.GetOs().Match?.Name ?? "unknown";
                    alog.OsVersion = dd.GetOs().Match?.Version ?? "unknown";
                    alog.BrowserName = dd.GetClient().Match?.Name ?? "unknown";
                    alog.BrowserVersion = dd.GetClient().Match?.Version ?? "unknown";
                    try
                    {
                        await _activityLogger.LogAsync(HttpContext, alog);
                    }
                    catch (Exception ex)
                    {
                        //nlog the issue
                    }


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
        //[Authorize(Roles.Administrator)]
        // make sure that only admins can delete and update profile
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
                    //log initialisation
                    string clientIp = string.Empty;
                    string userAgent = string.Empty;
                    DeviceDetector dd = new DeviceDetector();
                    ActivityLog alog = new ActivityLog();
                    try
                    {
                        var remoteIp = HttpContext.Connection.RemoteIpAddress;
                        clientIp = remoteIp?.IsIPv4MappedToIPv6 == true
                                                            ? remoteIp.MapToIPv4().ToString()
                                                            : remoteIp?.ToString();

                        userAgent = Request.Headers["User-Agent"].ToString();

                        if (string.IsNullOrWhiteSpace(userAgent))
                            return BadRequest("User-Agent header is missing.");

                        // Optional: Client Hints (modern browsers)
                        var headers = Request.Headers.ToDictionary(
                            h => h.Key,
                            h => h.Value.ToString());

                        var clientHints = ClientHints.Factory(headers);

                        dd = new DeviceDetector(userAgent, clientHints);
                        dd.Parse();
                    }
                    catch (Exception ex)
                    {
                        //
                    }

                    alog.activityLogId = 0;
                    alog.activityType = "DeleteUserProfile";
                    alog.remoteipaddress = clientIp;
                    alog.activityAction = "";//??
                    alog.activityDescription = "Deleting user profile successfull";
                    alog.createdby_aspnetusername = "";//TODO:modify method to require Authorize and pull user info
                    alog.createdby_aspnetuserId = "";//TODO:modify method to require Authorize and pull user info
                    //device details
                    alog.UserAgent = userAgent;
                    alog.IsMobile = dd.IsMobile();
                    alog.DeviceType = dd.GetDeviceName() ?? "unknown";
                    alog.Brand = dd.GetBrand() ?? "unknown";
                    alog.Model = dd.GetModel() ?? "unknown";
                    alog.OsName = dd.GetOs().Match?.Name ?? "unknown";
                    alog.OsVersion = dd.GetOs().Match?.Version ?? "unknown";
                    alog.BrowserName = dd.GetClient().Match?.Name ?? "unknown";
                    alog.BrowserVersion = dd.GetClient().Match?.Version ?? "unknown";
                    try
                    {
                        await _activityLogger.LogAsync(HttpContext, alog);
                    }
                    catch (Exception ex)
                    {
                        //nlog the issue
                    }
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

        [HttpGet("LoginEmailExist")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> LoginEmailExist(string email)
        {
            try
            {
                var result = await _authenticateService.LoginEmailExist(email);

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
                _logger.LogError(ex, "Unhandled exception from AuthenticateController.LoginEmailExist");
                return Problem("Unable to get LoginEmailExist result");
            }
        }

        [HttpPost("RequestPasswordReset")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RequestPasswordReset(string email)
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
                var result = await _authenticateService.RequestPasswordReset(email);

                if (result.Success)
                {
                    //log initialisation
                    string clientIp = string.Empty;
                    string userAgent = string.Empty;
                    DeviceDetector dd = new DeviceDetector();
                    ActivityLog alog = new ActivityLog();
                    try
                    {
                        var remoteIp = HttpContext.Connection.RemoteIpAddress;
                        clientIp = remoteIp?.IsIPv4MappedToIPv6 == true
                                                            ? remoteIp.MapToIPv4().ToString()
                                                            : remoteIp?.ToString();

                        userAgent = Request.Headers["User-Agent"].ToString();

                        if (string.IsNullOrWhiteSpace(userAgent))
                            return BadRequest("User-Agent header is missing.");

                        // Optional: Client Hints (modern browsers)
                        var headers = Request.Headers.ToDictionary(
                            h => h.Key,
                            h => h.Value.ToString());

                        var clientHints = ClientHints.Factory(headers);

                        dd = new DeviceDetector(userAgent, clientHints);
                        dd.Parse();
                    }
                    catch (Exception ex)
                    {
                        //
                    }

                    alog.activityLogId = 0;
                    alog.activityType = "RequestPasswordReset";
                    alog.remoteipaddress = clientIp;
                    alog.activityAction = "";//??
                    alog.activityDescription = "password reset requested";
                    alog.createdby_aspnetusername = email;//TODO:modify method to require Authorize and pull user info
                    alog.createdby_aspnetuserId = "";//TODO:modify method to require Authorize and pull user info
                    //device details
                    alog.UserAgent = userAgent;
                    alog.IsMobile = dd.IsMobile();
                    alog.DeviceType = dd.GetDeviceName() ?? "unknown";
                    alog.Brand = dd.GetBrand() ?? "unknown";
                    alog.Model = dd.GetModel() ?? "unknown";
                    alog.OsName = dd.GetOs().Match?.Name ?? "unknown";
                    alog.OsVersion = dd.GetOs().Match?.Version ?? "unknown";
                    alog.BrowserName = dd.GetClient().Match?.Name ?? "unknown";
                    alog.BrowserVersion = dd.GetClient().Match?.Version ?? "unknown";
                    try
                    {
                        await _activityLogger.LogAsync(HttpContext, alog);
                    }
                    catch (Exception ex)
                    {
                        //nlog the issue
                    }

                    return Ok(result);
                }
                else
                {
                    return new UnauthorizedObjectResult(result);
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception from AuthenticateController.RequestPasswordReset");
                return Problem("Unable to get Request Password Reset result");

            }
        }

        [HttpPost("ResetPassword")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ResetPassword([FromBody] IDResetPassword reset)
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
                var result = await _authenticateService.ResetPassword(reset);

                //log initialisation
                string clientIp = string.Empty;
                string userAgent = string.Empty;
                DeviceDetector dd = new DeviceDetector();
                ActivityLog alog = new ActivityLog();
                try
                {
                    var remoteIp = HttpContext.Connection.RemoteIpAddress;
                    clientIp = remoteIp?.IsIPv4MappedToIPv6 == true
                                                        ? remoteIp.MapToIPv4().ToString()
                                                        : remoteIp?.ToString();

                    userAgent = Request.Headers["User-Agent"].ToString();

                    if (string.IsNullOrWhiteSpace(userAgent))
                        return BadRequest("User-Agent header is missing.");

                    // Optional: Client Hints (modern browsers)
                    var headers = Request.Headers.ToDictionary(
                        h => h.Key,
                        h => h.Value.ToString());

                    var clientHints = ClientHints.Factory(headers);

                    dd = new DeviceDetector(userAgent, clientHints);
                    dd.Parse();
                }
                catch (Exception ex)
                {
                    //
                }

                if (result.Success)
                {
                    

                    alog.activityLogId = 0;
                    alog.activityType = "ResetPassword";
                    alog.remoteipaddress = clientIp;
                    alog.activityAction = "";//??
                    alog.activityDescription = "password reset successfull";
                    alog.createdby_aspnetusername = reset.email;//TODO:modify method to require Authorize and pull user info
                    alog.createdby_aspnetuserId = "";//TODO:modify method to require Authorize and pull user info
                    //device details
                    alog.UserAgent = userAgent;
                    alog.IsMobile = dd.IsMobile();
                    alog.DeviceType = dd.GetDeviceName() ?? "unknown";
                    alog.Brand = dd.GetBrand() ?? "unknown";
                    alog.Model = dd.GetModel() ?? "unknown";
                    alog.OsName = dd.GetOs().Match?.Name ?? "unknown";
                    alog.OsVersion = dd.GetOs().Match?.Version ?? "unknown";
                    alog.BrowserName = dd.GetClient().Match?.Name ?? "unknown";
                    alog.BrowserVersion = dd.GetClient().Match?.Version ?? "unknown";
                    try
                    {
                        await _activityLogger.LogAsync(HttpContext, alog);
                    }
                    catch (Exception ex)
                    {
                        //nlog the issue
                    }
                    return Ok(result);
                }
                else
                {
                    alog.activityLogId = 0;
                    alog.activityType = "ResetPassword";
                    alog.remoteipaddress = clientIp;
                    alog.activityAction = "";//??
                    alog.activityDescription = "password reset failed";
                    alog.createdby_aspnetusername = reset.email;//TODO:modify method to require Authorize and pull user info
                    alog.createdby_aspnetuserId = "";//TODO:modify method to require Authorize and pull user info
                    //device details
                    alog.UserAgent = userAgent;
                    alog.IsMobile = dd.IsMobile();
                    alog.DeviceType = dd.GetDeviceName() ?? "unknown";
                    alog.Brand = dd.GetBrand() ?? "unknown";
                    alog.Model = dd.GetModel() ?? "unknown";
                    alog.OsName = dd.GetOs().Match?.Name ?? "unknown";
                    alog.OsVersion = dd.GetOs().Match?.Version ?? "unknown";
                    alog.BrowserName = dd.GetClient().Match?.Name ?? "unknown";
                    alog.BrowserVersion = dd.GetClient().Match?.Version ?? "unknown";
                    try
                    {
                        await _activityLogger.LogAsync(HttpContext, alog);
                    }
                    catch (Exception ex)
                    {
                        //nlog the issue
                    }

                    return new UnauthorizedObjectResult(result);
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception from AuthenticateController.ResetPassword");
                return Problem("Unable to reset password");

            }
        }

        [HttpPost("SendCredentials")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> SendCredentials([FromBody] IDCredentials credentials)
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
                var result = await _authenticateService.SendLogInCredentialsEmail(credentials);

                if (result.Success)
                {
                    return Ok(result);
                }
                else
                {
                    return new UnauthorizedObjectResult(result);
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception from AuthenticateController.SendCredentials");
                return Problem("Unable to send login credentials");

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

        //[HttpGet("deviceinfo")]
        //public IActionResult GetDeviceInfo()
        //{
        //    var userAgent = Request.Headers["User-Agent"].ToString();

        //    if (string.IsNullOrWhiteSpace(userAgent))
        //        return BadRequest("User-Agent header is missing.");

        //    // Optional: Client Hints (modern browsers)
        //    var headers = Request.Headers.ToDictionary(
        //        h => h.Key,
        //        h => h.Value.ToString());

        //    var clientHints = ClientHints.Factory(headers);

        //    var dd = new DeviceDetector(userAgent, clientHints);
        //    dd.Parse();

        //    var deviceInfo = new ActivityLog
        //    {
        //        UserAgent = userAgent,
        //        IsMobile = dd.IsMobile(),
        //        DeviceType = dd.GetDeviceName() ?? "unknown",
        //        Brand = dd.GetBrand() ?? "unknown",
        //        Model = dd.GetModel() ?? "unknown",
        //        OsName = dd.GetOs().Match?.Name ?? "unknown",
        //        OsVersion = dd.GetOs().Match?.Version ?? "unknown",
        //        BrowserName = dd.GetClient().Match?.Name ?? "unknown",
        //        BrowserVersion = dd.GetClient().Match?.Version ?? "unknown"
        //    };

        //    return Ok(deviceInfo);
        //}

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
