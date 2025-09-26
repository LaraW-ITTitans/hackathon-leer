using FastEndpoints;
using ITTitans.Hackathon2025.EntityModel;
using ITTitans.Hackathon2025.EntityModel.Auth;
using ITTitans.Hackathon2025.WebAPI.Auth;
using ITTitans.Hackathon2025.WebAPI.Model.Auth;
using Microsoft.AspNetCore.Identity;

namespace ITTitans.Hackathon2025.WebAPI.Endpoints.Auth;

public class LoginEndpoint : Endpoint<LoginBindingModel, LoginResponseBindingModel>
{
    private readonly UserManager<HackathonUserEntity> userManager;
    private readonly SignInManager<HackathonUserEntity> signInManager;
    private readonly IJwtWebApiService jwtWebApiService;

    public LoginEndpoint(UserManager<HackathonUserEntity> userManager, SignInManager<HackathonUserEntity> signInManager, IJwtWebApiService jwtWebApiService)
    {
        this.userManager = userManager;
        this.signInManager = signInManager;
        this.jwtWebApiService = jwtWebApiService;
    }

    public override void Configure()
    {
        this.Post("api/auth/login");
        this.AllowAnonymous();
        
        this.Description(builder => builder
            .WithName("Login")
            .WithTags("Auth"));
    }
    
    public override async Task HandleAsync(LoginBindingModel req, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(req);
        
        HackathonUserEntity? user = await this.userManager.FindByNameAsync(req.UserName);
        if (user is null || user.IsDeleted)
        {
            this.AddError("Invalid username or password");
            await this.Send.ErrorsAsync(cancellation: ct);
            return;
        }
        
        SignInResult signInResult =
            await this.signInManager.CheckPasswordSignInAsync(user, req.Password, lockoutOnFailure: false);
        if (!signInResult.Succeeded)
        {
            this.AddError("Invalid username or password");
            await this.Send.ErrorsAsync(cancellation: ct);
            return;
        }

        LoginResponseBindingModel result = await this.GenerateLoginResponseModelAsync(user, req.RememberMe, ct);
        await this.Send.OkAsync(result, ct);
    }
    
    private async Task<LoginResponseBindingModel> GenerateLoginResponseModelAsync(
        HackathonUserEntity user,
        bool longLived,
        CancellationToken cancellationToken = default)
    {
        string token = await this.jwtWebApiService.CreateToken(user, this.User, longLived, cancellationToken);

        return new LoginResponseBindingModel
        {
            Token = token,
        };
    }
}