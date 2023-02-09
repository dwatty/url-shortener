using HashidsNet;
using LiteDB;
using UrlShortener.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSingleton<ILiteDatabase, LiteDatabase>(_ => new LiteDatabase("url.db"));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(s => s.SwaggerDoc("v1", 
    new Microsoft.OpenApi.Models.OpenApiInfo{
        Description = "A URL shortener",
        Title = "URL Shortener",
        Version = "v1",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "Me",
            Url = new Uri("https://ddub.org")
        }
    })
);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

Hashids _hashIds = new("URL Shortern", 5);

/**
 * POST for adding a new URL to the hashes
 */
app.MapPost("/add", (UrlInfo urlInfo, ILiteDatabase context) =>
    {
        if(urlInfo is null || string.IsNullOrEmpty(urlInfo.Url))
        {
            return Results.BadRequest("Invalid object");
        }

        var collection = context.GetCollection<UrlInfo>(BsonAutoId.Int32);
        var entry = collection.Query().Where(x => x.Url.Equals(urlInfo.Url)).FirstOrDefault();

        if(entry is not null)
        {
            return Results.Ok(_hashIds.Encode(entry.Id));
        }

        BsonValue documentId = collection.Insert(urlInfo);
        var encodedId = _hashIds.Encode(documentId);

        return Results.Created(encodedId, encodedId);
    })
    .Produces<string>(StatusCodes.Status200OK)
    .Produces<string>(StatusCodes.Status201Created)
    .Produces(StatusCodes.Status400BadRequest);

/**
 * GET for returning a full URL for a given hash
 */
app.MapGet("/{shortUrl}", (string shortUrl, ILiteDatabase context) => 
    {
        int[] ids = _hashIds.Decode(shortUrl);

        var tempId = ids[0];
        var collection = context.GetCollection<UrlInfo>();
        var entry = collection.Query().Where(x => x.Id.Equals(tempId)).FirstOrDefault();

        if(entry is not null) 
        {
            return Results.Ok(entry.Url);
        }

        return Results.NotFound();
    })
    .Produces<string>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status404NotFound);

/**
 * GET for returning all currently stored hash/URL combos
 */
app.MapGet("/", (ILiteDatabase context) => 
    {
        var collection = context.GetCollection<UrlInfo>();
        var entries = collection.Query().ToList();
        return Results.Ok(entries);
    })
    .Produces<List<UrlInfo>>(StatusCodes.Status200OK);


// Run It!
app.Run();
