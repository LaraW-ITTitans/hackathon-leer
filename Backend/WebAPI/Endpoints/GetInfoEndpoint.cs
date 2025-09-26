using FastEndpoints;
using ITTitans.Hackathon2025.WebAPI.Model;
using System.Diagnostics;
using System.Reflection;

namespace ITTitans.Hackathon2025.WebAPI.Endpoints;

public class GetInfoEndpoint : EndpointWithoutRequest<ApiInfoBindingModel>
{
    private readonly IHostEnvironment hostEnvironment;

    public GetInfoEndpoint(IHostEnvironment hostEnvironment)
    {
        this.hostEnvironment = hostEnvironment;
    }

    public override void Configure()
    {
        this.Get("api/info");
        this.AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        string version = this.GetCurrentFileVersion();
        string systemEnvironment = this.hostEnvironment.EnvironmentName;

        var apiInfoBindingModel = new ApiInfoBindingModel { Version = version, SystemEnvironment = systemEnvironment };
        await this.Send.OkAsync(apiInfoBindingModel, ct);
    }
    
    private string GetCurrentFileVersion()
    {
        var assembly = Assembly.GetExecutingAssembly();
        FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
        return fileVersionInfo.FileVersion ?? throw new InvalidOperationException("file version is not set");
    }
}