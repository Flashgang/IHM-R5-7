using MongoDB.Driver;
using WebApi.Controllers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Configuration MongoDB
builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var connectionString = Environment.GetEnvironmentVariable("ConnectionString") ?? "mongodb://localhost:27017";
    
    // --- LIGNE DE DEBUG ---
    Console.WriteLine($"[DEBUG] Connexion MongoDB utilisée : {connectionString}");
    // ----------------------

    return new MongoClient(connectionString);
});

var app = builder.Build();
app.MapControllers();
app.Run();