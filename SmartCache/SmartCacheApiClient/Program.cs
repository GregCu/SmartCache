using Orleans.Configuration;
using System.ComponentModel.DataAnnotations;
using SmartCacheGrains.Abstractions;
using SmartCacheApiClient.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<IHashService, HashService>();

builder.Services.AddOrleansClient(clientBuilder =>
{
    clientBuilder
        .UseLocalhostClustering()
        .Configure<ClusterOptions>(options =>
        {
            options.ClusterId = "dev-cluster";
            options.ServiceId = "EmailDomainService";
        });
});

var app = builder.Build();

var client = app.Services.GetRequiredService<IClusterClient>();

app.MapGet("emails/{email}",  async (IHashService hashService, [EmailAddress(ErrorMessage = "Invalid Email Address")] string email) =>
{
    //var domain = email.Split('@')[1];
    //var mailboxName = hashService.CalculateHash(email.Split('@')[0]); //hashing mailboxName part of the email address (maybe overkill)
    //var grain = client.GetGrain<IEmailDomainGrain>(domain);
    //bool exists = await grain.EmailExists(mailboxName);

    var domain = email.Split('@')[1];
    var mailboxName = email.Split('@')[0];
    var grain = client.GetGrain<IEmailDomainGrain>(domain);

    bool exists = await grain.EmailExists(mailboxName);

    return exists ? Results.Ok() : Results.NotFound();
});

app.MapPost("emails/{email}", async (IHashService hashService, [EmailAddress(ErrorMessage = "Invalid Email Address")] string email) =>
{
    //var domain = email.Split('@')[1]; 
    //var mailboxName = hashService.CalculateHash(email.Split('@')[0]);
    //var grain = client.GetGrain<IEmailDomainGrain>(domain);
    //bool added = await grain.AddEmail(mailboxName);

    var domain = email.Split('@')[1];
    var mailboxName = email.Split('@')[0];
    var grain = client.GetGrain<IEmailDomainGrain>(domain);

    bool added = await grain.AddEmail(mailboxName);

    return added ? Results.Created() : Results.Conflict();
});


app.Run();