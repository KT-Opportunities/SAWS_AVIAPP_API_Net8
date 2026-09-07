using SAWSCore8API.Models;
using SAWSCore8API.DbContexts;
using SAWSCore8API.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using SAWSCore8API.Dtos;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity.UI.Services;
namespace SAWSCore8API.Services
{
    public class AuthenticateService : IAuthenticateService
    {
        private readonly SAWSDbContext _context;
        private readonly IUriService _uriService;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private ILogger<AuthenticateService> _logger;
        private readonly IEmailSender _emailSender;
        public AuthenticateService(
                SAWSDbContext context,
                IUriService uriService,
                IHttpContextAccessor httpContextAccessor,
                IConfiguration configuration,
                UserManager<ApplicationUser> userManager,
                SignInManager<ApplicationUser> signInManager,
                RoleManager<IdentityRole> roleManager,
               // IEmailService emailService,
                ILogger<AuthenticateService> logger,
                IEmailSender emailSender
            )
        {
            _context = context;
            _uriService = uriService;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _logger = logger;
             _emailSender = emailSender;
        }

        public async Task<LoginResult> LoginUser(LoginModel appUser)
        {
            var user = _context.User
                    .Where(u => u.UserName == appUser.Username || u.Email == appUser.Username)
                    .SingleOrDefault();

            if (user == null)
            {
                return LoginResult.FailureResult("Please check your password and username");
            }

            if (!user.IsActive)
            {
                return LoginResult.FailureResult("Account is deactivated. Please contact administration");
            }


            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));

            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            var token = new JwtSecurityToken(
               issuer: _configuration["JWT:ValidIssuer"],
               audience: _configuration["JWT:ValidAudience"],
               expires: DateTime.Now.AddHours(3),
               claims: authClaims,
               signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            );

            var userRoles = await _userManager.GetRolesAsync(user);
            var roles = (List<string>)userRoles;

            //model.RememberMe
            var signInResult = await _signInManager.PasswordSignInAsync(user, appUser.Password, false, false);

            if (signInResult.Succeeded)
            {
                var userProfile = _context.userProfiles
                    .Include(up => up.Subscription)
                    .FirstOrDefault(up => up.aspuid == user.Id);

                return LoginResult.SuccessResult(token, user, userProfile, roles);
            }
            else
            {
                return LoginResult.FailureResult("Invalid login attempt");
            }
        }

        public async Task<CreateResult> AddAdminUserProfile(RegisterAdmin appUser)
        {
            string[] nameParts = appUser.Fullname.Split(' ');

            var firstName = nameParts.Length > 0 ? nameParts[0] : string.Empty;
            var middleName = nameParts.Length > 2 ? string.Join(" ", nameParts[1..^1]) : string.Empty;
            var lastName = nameParts.Length > 1 ? nameParts[1] : string.Empty;

            var user = new ApplicationUser
            {
                UserName = appUser.Username,
                Email = appUser.Email,
                IsActive = true,
                IsAdminUser = true,
                FirstName = firstName + (string.IsNullOrEmpty(middleName) ? "" : " " + middleName),
                LastName = lastName
            };

            var userRole = "Admin";

            var result = await _userManager.CreateAsync(user, appUser.Password);

            if (!await _roleManager.RoleExistsAsync(userRole))
                await _roleManager.CreateAsync(new IdentityRole(userRole));

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, userRole);

                UserProfile userProfile = new UserProfile();

                userProfile.email = appUser.Email;
                userProfile.aspuid = user.Id;
                userProfile.fullname = appUser.Fullname;
                userProfile.username = appUser.Username;
                userProfile.isactive = true;
                userProfile.userrole = userRole;
                userProfile.created_at = DateTime.Now;
                userProfile.updated_at = DateTime.Now;
                userProfile.isdeleted = false;

                _context.userProfiles.Add(userProfile);
                Save();

                Subscription adminSubscription = new Subscription();
                adminSubscription.userprofileid = userProfile.userprofileid;
                adminSubscription.package_name = "Admin";
                adminSubscription.package_id = 7;
                adminSubscription.package_price = 0;
                adminSubscription.start_date = DateTime.Now;
                adminSubscription.end_date = DateTime.Now.AddYears(1);
                adminSubscription.subscription_duration = 365;
                adminSubscription.subscription_token = "";
                adminSubscription.isactive = true;
                adminSubscription.created_at = DateTime.Now;
                adminSubscription.updated_at = DateTime.Now;
                adminSubscription.isdeleted = false;

                _context.Subscriptions.Add(adminSubscription);
                Save();

                return CreateResult.SuccessResult(userProfile.userprofileid);
            }

            return CreateResult.FailureResult("Unable to add admin user profile");
        }

        public async Task<CreateResult> AddSubscriberUserProfile(RegisterSubscriber appUser)
        {
            string[] nameParts = appUser.Fullname.Split(' ');

            var firstName = nameParts.Length > 0 ? nameParts[0] : string.Empty;
            var lastName = nameParts.Length > 1 ? nameParts[1] : string.Empty;
            var middleName = nameParts.Length > 2 ? string.Join(" ", nameParts[1..^1]) : string.Empty;

            var user = new ApplicationUser
            {
                UserName = appUser.Username,
                Email = appUser.Email,
                IsActive = true,
                IsAdminUser = false,
                FirstName = firstName + (string.IsNullOrEmpty(middleName) ? "" : " " + middleName),
                LastName = lastName
            };

            var userRole = "Subscriber";

            var result = await _userManager.CreateAsync(user, appUser.Password);


            if (!await _roleManager.RoleExistsAsync(userRole))
                await _roleManager.CreateAsync(new IdentityRole(userRole));

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, userRole);

                UserProfile userProfile = new UserProfile();

                userProfile.email = appUser.Email;
                userProfile.aspuid = user.Id;
                userProfile.userrole = userRole;
                userProfile.fullname = appUser.Fullname;
                userProfile.username = appUser.Username;
                userProfile.isactive = true;
                userProfile.created_at = DateTime.Now;
                userProfile.updated_at = DateTime.Now;
                userProfile.isdeleted = false;

                _context.userProfiles.Add(userProfile);
                Save();

                Subscription freeSubscription = new Subscription();
                freeSubscription.userprofileid = userProfile.userprofileid;
                freeSubscription.package_name = "monthly Free";
                freeSubscription.package_id = 1;
                freeSubscription.package_price = 0;
                freeSubscription.start_date = DateTime.Now;
                freeSubscription.end_date = DateTime.Now.AddYears(1);
                freeSubscription.subscription_duration = 365;
                freeSubscription.subscription_token = "";
                freeSubscription.isactive = true;
                freeSubscription.created_at = DateTime.Now;
                freeSubscription.updated_at = DateTime.Now;
                freeSubscription.isdeleted = false;

                _context.Subscriptions.Add(freeSubscription);
                Save();

                /*  using var transaction = await _context.Database.BeginTransactionAsync();
                  try
                  {
                      _context.userProfiles.Add(userProfile);
                      _context.Subscriptions.Add(freeSubscription);
                      Save();
                      await transaction.CommitAsync();
                  }
                  catch (Exception ex)
                  {
                      await transaction.RollbackAsync();
                      _logger.LogError(ex, "Unhandled exception from AuthenticateService.AddSubscriberUserProfile");
                      return CreateResult.FailureResult("Unable to process the commit to database.");
                  }*/


                return CreateResult.SuccessResult(userProfile.userprofileid);
            }

            return CreateResult.FailureResult("Unable to add subscriber user profile");
        }

        public Task<UpdateResult> UpdateUserProfile(UserProfile user)
        {
            user.updated_at = DateTime.Now;
            user.isdeleted = false;

            _context.userProfiles.Update(user);
            Save();

            return Task.FromResult(UpdateResult.SuccessResult());
        }

        public async Task<UpdateResult> UpdateIdentityEmail(UserProfile userProfile)
        {
            var user = await _userManager.FindByIdAsync(userProfile.aspuid);
            // var user = await _userManager.FindByNameAsync(userProfile.username);

            string[] nameParts = userProfile.fullname.Split(' ');

            var firstName = nameParts.Length > 0 ? nameParts[0] : string.Empty;
            var lastName = nameParts.Length > 1 ? nameParts[1] : string.Empty;
            var middleName = nameParts.Length > 2 ? string.Join(" ", nameParts[1..^1]) : string.Empty;

            if (user != null)
            {
                user.Email = userProfile.email;
                user.NormalizedEmail = userProfile.email;
                user.UserName = userProfile.username;
                user.NormalizedUserName = userProfile.username;
                user.FirstName = firstName + (string.IsNullOrEmpty(middleName) ? "" : " " + middleName);
                user.LastName = lastName;
                user.UserName = userProfile.username;
                user.IsActive = (bool)userProfile.isactive;

                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    return UpdateResult.SuccessResult();
                }
                else
                {
                    return UpdateResult.FailureResult("Unable to update identity user email");
                }
            }

            return UpdateResult.FailureResult("Identity user does not exist");
        }

        public async Task<LoggedInResult> GetLoggedInUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user != null)
            {
                var userRoles = await _userManager.GetRolesAsync(user);
                List<string> roles = (List<string>)userRoles;
                string rolesList = string.Join(",", roles.ToArray());

                return LoggedInResult.SuccessResult(user, rolesList);
            }
            else
            {
                return LoggedInResult.FailureResult("User Not Found");
            }
        }

        public async Task<LoginExistResult> LoginEmailExist(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user != null)
            {
                return LoginExistResult.SuccessResult(true);
            }
            else
            {
                return LoginExistResult.SuccessResult(false);
            }
        }

        public async Task<CreatePasswordResult> RequestPasswordReset(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user != null)
            {
                if (!user.IsActive)
                {
                    return CreatePasswordResult.FailureResult("Email account is deactivated, please contact administration");
                }
                else
                {
                    var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);

                    //Generate email with the new token
                    byte[] resetTokenGeneratedBytes = Encoding.UTF8.GetBytes(resetToken);
                    var validResetToken = Uri.EscapeDataString(WebEncoders.Base64UrlEncode(resetTokenGeneratedBytes));

                    var app_url = _configuration["AppURL"];

                    string resetUrl = app_url + @"#/reset-password?email=" + email + "&token=" + validResetToken;

                    string resetEmailBody = $"<h1>South African Weather Service</h1>" + $"<p>to reset your password <a href='{resetUrl}'>Click here</a></p>";

                    try
                    {
                        
                       // _emailService.SendPasswordResetEmail(user.Email, resetEmailBody);
                      await _emailSender.SendEmailAsync(user?.Email, "South African Weather Service forgot/reset password request", resetEmailBody);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error with sending reset password email {user.Email}");
                        throw;
                    }

                    return CreatePasswordResult.SuccessResult($"Reset details sent to {user.Email}");
                }
            }
            else
            {
                return CreatePasswordResult.FailureResult("Email does not exist");
            }
        }

        public async Task<CreatePasswordResult> SendLogInCredentialsEmail(IDCredentials credentials)
        {
            //Generate email          
            var app_url = _configuration["AppURL"];
            string loginUrl = app_url + @"#/login";
            string resetEmailBody = $"<h1>South African Weather Service</h1>"
                + $"<p>Your login credentials are as follows:</p>"
                + $"<p><strong>Username:</strong> {credentials.username}</p>"
                + $"<p><strong>Password:</strong> {credentials.password}</p>"
                + $"<p>You can log in by clicking <a href='{loginUrl}'>here</a>.</p>"
                + "<p>If you did not request these credentials, please contact support.</p>";

            try
            {
                //EmailService emailService = new EmailService(_configuration);
               // _emailService.SendLogInCredentialsEmail(credentials.username, resetEmailBody);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error with sending login credentials to {credentials.username}");
                throw;
            }

            return CreatePasswordResult.SuccessResult($"Login credentials sent to {credentials.username}");
        }

        public async Task<CreatePasswordResult> ResetPassword(IDResetPassword reset)
        {
            var user = await _userManager.FindByEmailAsync(reset.email);

            if (user != null)
            {
                var decodedResetToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(reset.token));

                var result = await _userManager.ResetPasswordAsync(user, decodedResetToken, reset.newPassword);

                if (result.Succeeded)
                {
                    return CreatePasswordResult.SuccessResult($"Password reset successful for {user.Email}");
                }
                else
                {
                    return CreatePasswordResult.FailureResult("Password reset not successful");
                }

            }
            else
            {
                return CreatePasswordResult.FailureResult("Email does not exist");
            }
        }

        public async Task<DeleteResult> DeleteUserProfileById(int id)
        {
            var user = _context.userProfiles.First(a => a.userprofileid == id);

            user.isdeleted = true;
            user.isactive = false;
            user.deleted_at = DateTime.Now;

            Save();

            var IdentityUser = await _userManager.FindByIdAsync(user.aspuid);

            if (IdentityUser == null)
            {
                return DeleteResult.SuccessResult("Identity user does not exist");
            }

            IdentityUser.IsActive = false;

            var result = await _userManager.UpdateAsync(IdentityUser);

            if (result.Succeeded)
            {
                return DeleteResult.SuccessResult("User successfully deleted");
            }
            else
            {
                return DeleteResult.FailureResult("Failed to delete user");
            }

        }
        public async Task<CreatePasswordResult> RequestPasswordResetOTP(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return CreatePasswordResult.FailureResult("Email does not exist");
            if (!user.IsActive) return CreatePasswordResult.FailureResult("Email account is deactivated, please contact administration");

            var otp = RandomNumberGenerator.GetInt32(100000, 999999).ToString();
            var expiresAt = DateTime.UtcNow.AddMinutes(10);

            _context.PasswordResetOtps.RemoveRange(_context.PasswordResetOtps.Where(x => x.Email == email));
            _context.PasswordResetOtps.Add(new PasswordResetOtp { Email = email, OTP = otp, ExpiresAt = expiresAt });
            await _context.SaveChangesAsync();

            string resetEmailBody = $"<h1>South African Weather Service</h1>"
                + $"<p>Your password reset OTP is:</p>"
                + $"<h2 style='letter-spacing:5px; text-align:center; background:#f0f0f0; padding:15px;'>{otp}</h2>"
                + $"<p>This OTP expires in 10 minutes.</p>";

            try
            {
                await _emailSender.SendEmailAsync(user.Email, "South African Weather Service forgot/reset password request", resetEmailBody);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error with sending OTP email {user.Email}");
                // don't throw, still continue
            }

            // RETURN OTP IN OBJECT HERE
            return CreatePasswordResult.SuccessWithData(
                $"OTP sent to {user.Email}",
                new { email = user.Email, otp = otp, expiresAt = expiresAt }
            );
        }

        public async Task<CreatePasswordResult> VerifyOTPAndResetPassword(VerifyOTPDto dto)
        {
            var otpRecord = await _context.PasswordResetOtps.FirstOrDefaultAsync(x => x.Email == dto.Email && x.OTP == dto.OTP);
            if (otpRecord == null) return CreatePasswordResult.FailureResult("Invalid OTP");
            if (otpRecord.ExpiresAt < DateTime.UtcNow) return CreatePasswordResult.FailureResult("OTP has expired");

            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null) return CreatePasswordResult.FailureResult("User not found");

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, dto.NewPassword);

            if (result.Succeeded)
            {
                _context.PasswordResetOtps.Remove(otpRecord);
                await _context.SaveChangesAsync();
                return CreatePasswordResult.SuccessResult("Password reset successfully");
            }
            return CreatePasswordResult.FailureResult(string.Join(", ", result.Errors.Select(e => e.Description)));
        }
        public void Save()
        {
            _context.SaveChanges();
        }


    }
}
