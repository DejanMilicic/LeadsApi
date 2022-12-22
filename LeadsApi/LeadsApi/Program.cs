using LeadsApi;
using Raven.Client.Documents;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IDocumentStore>(ctx =>
{
    var store = new DocumentStore
    {
        Urls = new string[] { "http://127.0.0.1:8080/" },
        Database = "Leads"
    };

    store.Initialize();

    return store;
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapPost("/lead", async (Lead lead, IDocumentStore store) =>
{
    using var session = store.OpenSession();

    lead.Id = null;
    session.Store(lead);
    session.SaveChanges();

    return Results.Created($"/leads/{lead.Id}", lead);
})
.WithName("CreateLead")
.WithOpenApi();

app.MapGet("/lead", async (string id, IDocumentStore store) =>
    {
        using var session = store.OpenSession();

        return Results.Ok(session.Load<Lead>(id));
    })
    .WithName("GetLead")
    .WithOpenApi();

app.Run();
