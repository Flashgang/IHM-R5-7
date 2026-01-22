using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Text.Json;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LogsController : ControllerBase
    {
        private readonly IMongoCollection<LogEntry> _logsCollection;

        public LogsController(IMongoClient mongoClient)
        {
            var database = mongoClient.GetDatabase("ProjetLogsDB");
            _logsCollection = database.GetCollection<LogEntry>("logs");

            // Index d'unicité sur le Hash
            var indexKeys = Builders<LogEntry>.IndexKeys;
            var indexOptions = new CreateIndexOptions { Unique = true };
            var indexModel = new CreateIndexModel<LogEntry>(indexKeys.Ascending(x => x.Hash), indexOptions);
            _logsCollection.Indexes.CreateOne(indexModel);
        }

        // --- NOUVEAU : INSERTION EN MASSE (BULK) ---
        [HttpPost("bulk")]
        public async Task<IActionResult> PostBulk([FromBody] List<LogPayload> payloads)
        {
            if (payloads == null || !payloads.Any()) return Ok(new { count = 0 });

            // 1. Récupérer tous les hashs entrants
            var incomingHashes = payloads.Select(p => p.Hash).ToList();

            // 2. Trouver ceux qui existent DÉJÀ en base (en une seule requête)
            var existingHashes = await _logsCollection
                .Find(x => incomingHashes.Contains(x.Hash))
                .Project(x => x.Hash)
                .ToListAsync();
            
            var existingHashSet = new HashSet<string>(existingHashes);
            var newEntries = new List<LogEntry>();

            // 3. Préparer uniquement les nouveaux
            foreach (var payload in payloads)
            {
                if (!existingHashSet.Contains(payload.Hash))
                {
                    var jsonString = JsonSerializer.Serialize(payload.Data);
                    newEntries.Add(new LogEntry
                    {
                        Hash = payload.Hash,
                        Content = BsonDocument.Parse(jsonString),
                        FullText = jsonString, // Pour la recherche
                        InsertedAt = DateTime.UtcNow
                    });
                }
            }

            // 4. Tout insérer d'un coup
            if (newEntries.Any())
            {
                // Ordered = false permet de continuer même si une erreur survient sur un élément
                await _logsCollection.InsertManyAsync(newEntries, new InsertManyOptions { IsOrdered = false });
            }

            return Ok(new { inserted = newEntries.Count, skipped = payloads.Count - newEntries.Count });
        }
        // ---------------------------------------------

        [HttpGet("check/{hash}")]
        public async Task<bool> CheckHashExists(string hash)
        {
            return await _logsCollection.Find(x => x.Hash == hash).AnyAsync();
        }

        [HttpGet("search")]
        public IActionResult Search(string keyword)
        {
            var logsFromDb = _logsCollection.AsQueryable()
                .Where(l => l.FullText.Contains(keyword))
                .ToList();

            var resultsSafe = logsFromDb.Select(l => new 
            {
                l.Id,
                l.Hash,
                l.InsertedAt,
                Data = JsonSerializer.Deserialize<object>(l.FullText) 
            });

            return Ok(resultsSafe);
        }
    }

    public class LogPayload
    {
        public string Hash { get; set; } = string.Empty;
        public object Data { get; set; } = new object();
    }

    public class LogEntry
    {
        [MongoDB.Bson.Serialization.Attributes.BsonId]
        public ObjectId Id { get; set; }
        public string Hash { get; set; } = string.Empty;
        public BsonDocument Content { get; set; }
        public string FullText { get; set; } = string.Empty;
        public DateTime InsertedAt { get; set; }
    }
}