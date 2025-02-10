using Go2GrooveApi.Common;
using Go2GrooveApi.Domain.Dtos;
using Go2GrooveApi.Domain.Models;
using Go2GrooveApi.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Go2GrooveApi.Endpoints
{
    public static class AccountsEndpoint
    {
        public static RouteGroupBuilder MapAccountsEndpoints(this IEndpointRouteBuilder app)
        {
            var endpointsGroup = app.MapGroup("/api/accounts").WithTags("accounts");

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

            return endpointsGroup;
        }
    }
}
