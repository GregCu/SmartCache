using Azure.Data.Tables;
using System.ComponentModel.DataAnnotations;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<TableServiceClient>(_ =>
    new TableServiceClient("UseDevelopmentStorage=true"));

var app = builder.Build();
var tableClient = app.Services.GetRequiredService<TableServiceClient>().GetTableClient("BreachedEmails");

app.MapGet("traditionalApproach/emails/{email}", async ([EmailAddress] string email) =>
{
    var partitionKey = email.Split('@')[1];
    var rowKey = email.Split('@')[0];

    try
    {
        var entity = await tableClient.GetEntityIfExistsAsync<TableEntity>(partitionKey, rowKey);
        return entity.HasValue ? Results.Ok() : Results.NotFound();
    }
    catch
    {
        return Results.NotFound();
    }
});


app.MapPost("traditionalApproach/emails/{email}", async ([EmailAddress] string email) =>
{
    var partitionKey = email.Split('@')[1];
    var rowKey = email.Split('@')[0];

    var existingEntity = await tableClient.GetEntityIfExistsAsync<TableEntity>(partitionKey, rowKey);
    if (existingEntity.HasValue)
    {
        return Results.Conflict();
    }

    var entity = new TableEntity(partitionKey, rowKey);
    await tableClient.AddEntityAsync(entity);
    return Results.Created();
});

app.Run();