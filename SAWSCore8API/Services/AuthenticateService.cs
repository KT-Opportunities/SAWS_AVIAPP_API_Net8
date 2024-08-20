using SAWSCore8API.Models;
using SAWSCore8API.DbContexts;
using SAWSCore8API.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using SAWSCore8API.Configurations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Configuration;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;

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

        public AuthenticateService(
                SAWSDbContext context,
                IUriService uriService,
                IHttpContextAccessor httpContextAccessor,
                IConfiguration configuration,
                UserManager<ApplicationUser> userManager,
                SignInManager<ApplicationUser> signInManager,
                RoleManager<IdentityRole> roleManager,
                ILogger<AuthenticateService> logger
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
        }

        public async Task<LoginResult> LoginUser(Login appUser)
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
            var user = new ApplicationUser
            {
                UserName = appUser.Username,
                Email = appUser.Email,
                IsActive = true,
                IsAdminUser = true
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
                userProfile.userrole = userRole;
                userProfile.created_at = DateTime.Now;
                userProfile.updated_at = DateTime.Now;
                userProfile.isdeleted = false;

                _context.userProfiles.Add(userProfile);
                Save();

                return CreateResult.SuccessResult(userProfile.userprofileid);
            }

            return CreateResult.FailureResult("Unable to add admin user profile");
        }

        public async Task<CreateResult> AddSubscriberUserProfile(RegisterSubscriber appUser)
        {
            var user = new ApplicationUser
            {
                UserName = appUser.Username,
                Email = appUser.Email,
                IsActive = true,
                IsAdminUser = false
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
                freeSubscription.subscription_status = "Active"; 
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

            if (user != null)
            {
                user.Email = userProfile.email;
                user.UserName = userProfile.email;

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

        /*      public IEnumerable<Advert> GetAllAdverts()
              {
                  return _context.Adverts
                  .Where(d => d.isdeleted == false && d.ispublished == true)
                  .Include(d => d.DocAdverts)
                  .ToList();
              }*/

        /*   public Advert GetAdvertByAdvertId(int id)
           {
               return _context.Adverts
                       .Where(d => d.advertId == id && d.isdeleted == false)
                       .Include(d => d.DocAdverts)
                       .FirstOrDefault();
           }*/

        /* public void DeleteAdvertById(int id)
         {
             var advert = _context.Adverts.First(a => a.advertId == id);

             advert.isdeleted = true;
             advert.deleted_at = DateTime.Now;

             Save();
         }*/


        public void Save()
        {
            _context.SaveChanges();
        }
    }
}
