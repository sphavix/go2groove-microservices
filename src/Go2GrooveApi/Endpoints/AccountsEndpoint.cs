using Go2GrooveApi.Common;
using Go2GrooveApi.Domain.Dtos;
using Go2GrooveApi.Domain.Models;
using Go2GrooveApi.Services;
using Go2GrooveApi.Services.Accounts;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Go2GrooveApi.Endpoints
{
    public static class AccountsEndpoint
    {
        public static RouteGroupBuilder MapAccountsEndpoints(this IEndpointRouteBuilder app)
        {
            var endpointsGroup = app.MapGroup("/api/accounts").WithTags("accounts");

            // register endpoint
            endpointsGroup.MapPost("/register", async (HttpContext context, UserManager<ApplicationUser> _userManager,
                [AsParameters] RegisterDto model, IFormFile? profilePicture) =>
            {
                var user = await _userManager.FindByEmailAsync(model.Email);

                if(user is not null)
                {
                    return Results.BadRequest(Response<string>.Failure("This email has already been taken!"));
                }

                if(profilePicture is null)
                {
                    return Results.BadRequest(Response<string>.Failure("Profile picture is required"));
                }

                var picture = await FileUpload.UploadFile(profilePicture);

                picture = $"{context.Request.Scheme}://{context.Request.Host}/uploads{picture}";

                var newUser = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FullName = model.FullName,
                    ProfilePicture = picture
                };

                var result = await _userManager.CreateAsync(newUser, model.Password);

                if(!result.Succeeded)
                {
                    return Results.BadRequest(Response<string>.Failure(result.Errors.Select(x => x.Description).FirstOrDefault()));
                }

                return Results.Ok(Response<string>.Success("", "User registered successfully"));
            }).DisableAntiforgery();

            // login endpoint
            endpointsGroup.MapPost("/login", async (UserManager<ApplicationUser> _userManager, TokenService _tokenService, LoginDto model) =>
            {
                if (model is null)
                {
                    return Results.BadRequest(Response<string>.Failure("Please enter login details"));
                }

                var user = await _userManager.FindByEmailAsync(model.Email);

                if (user is null)
                {
                    return Results.BadRequest(Response<string>.Failure("User cannot be found"));
                }

                var results = await _userManager.CheckPasswordAsync(user!, model.Password);

                if (!results)
                {
                    return Results.BadRequest(Response<string>.Failure("Incorrect Username or Password"));
                }

                var token = _tokenService.GenerateToken(user.Id, user.UserName);

                return Results.Ok(Response<string>.Success(token, "Successfully logged in!"));
            });

            // get current logged on user
            endpointsGroup.MapGet("/me", async (IUserContext _httpContext, UserManager<ApplicationUser> _userManager) =>
            {
                try
                {
                    var currentUser = _httpContext.GetCurrentUser();

                    if (currentUser == null)
                    {
                        return Results.Unauthorized();
                    }

                    var currentLoggedInUserId = currentUser.Id;

                    var currentLoggedInUser = await _userManager.Users.SingleOrDefaultAsync(x => x.Id == currentLoggedInUserId.ToString());

                    if (currentLoggedInUser == null)
                    {
                        return Results.NotFound(Response<ApplicationUser>.Failure("User was not found!"));
                    }

                    return Results.Ok(Response<ApplicationUser>.Success(currentLoggedInUser, "Currently logged in!"));
                }
                catch (UnauthorizedAccessException ex)
                {
                    return Results.Unauthorized(); // Return 401 if the user is not authenticated
                }
                catch (InvalidOperationException ex)
                {
                    return Results.BadRequest(ex.Message); // Return 400 for missing claims or other issues
                }
            });


            return endpointsGroup;
        }
    }
}
