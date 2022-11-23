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

// These are equivalent... I had no idea
Hashids _hashIds = new("URL Shortern", 5);
var varHashes = new Hashids("URL Shortener", 5);

app.MapPost("/add", (UrlInfo urlInfo, ILiteDatabase context) =>
    {
        if(urlInfo is null || string.IsNullOrEmpty(urlInfo.Url))
        {
            return Results.BadRequest("Invalid object");
        }

        ILiteCollection<UrlInfo> collection = context.GetCollection<UrlInfo>(BsonAutoId.Int32);

        UrlInfo entry = collection.Query().Where(x => x.Url.Equals(urlInfo.Url)).FirstOrDefault();

        if(entry is not null)
        {
            return Results.Ok(_hashIds.Encode(entry.Id));
        }

        BsonValue documentId = collection.Insert(urlInfo);
        string encodedId = _hashIds.Encode(documentId);
        return Results.Created(encodedId, encodedId);
    })
    .Produces<string>(StatusCodes.Status200OK)
    .Produces<string>(StatusCodes.Status201Created)
    .Produces(StatusCodes.Status400BadRequest);

app.MapGet("/{shortUrl}", (string shortUrl, ILiteDatabase context) => 
    {
        int[] ids = _hashIds.Decode(shortUrl);
        int tempId = ids[0];

        ILiteCollection<UrlInfo> collection = context.GetCollection<UrlInfo>();

        UrlInfo entry = collection.Query().Where(x => x.Id.Equals(tempId)).FirstOrDefault();

        if(entry is not null) 
        {
            return Results.Ok(entry.Url);
        }

        return Results.NotFound();
    })
    .Produces<string>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status404NotFound);

app.Run();
