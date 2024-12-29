using System.Net.Http;
using System.Text.Json;

namespace OneMansSky
{
    public static class CelestialBodiesAPI
    {
        //setting fileName to be appdata folder to get celestial bodies from
        private static readonly string fileName = Path.Combine(FileSystem.AppDataDirectory, "celestialBodies.json");
        private static List<CelestialBody>? savedBodies;

        public class CelestialBodyList
        {
            public List<CelestialBody>? bodies { get; set; }
        }

        public class CelestialBody
        {
            public string id { get; set; }                        
            public string englishName { get; set; }      
            public bool isPlanet { get; set; }           
            public string discoveryDate { get; set; }
            public string bodyType { get; set; }
            public double gravity { get; set; }
            public string discoveredBy { get; set; }

            public string shortDescription
            {
                get
                {
                    return $"{englishName} is a {bodyType}, discovered on {discoveryDate}, with a gravity of {gravity} m/s2.\nIt was discovered by {discoveredBy}.";
                }
            }
        }

        //func to return bodies from api or local file
        public static async Task<List<CelestialBody>> GetCelestialInfo()
        {
            
            if (!File.Exists(fileName))
            {
                //if bodies file doesnt exist calls api
                await SaveCelestialInfo();
            }
            else
            {
                //otherwise if file exists, get from that file
                string json = await File.ReadAllTextAsync(fileName);
                var bodyList = JsonSerializer.Deserialize<CelestialBodyList>(json);
                savedBodies = bodyList?.bodies ?? new List<CelestialBody>();
            }

            return savedBodies!;
        }

        //func to call api and save json info to a file
        private static async Task SaveCelestialInfo()
        {
            try
            {
                using HttpClient client = new HttpClient();
                string url = "https://api.le-systeme-solaire.net/rest/bodies/";
                string json = await client.GetStringAsync(url);
              
                //save to appdata
                Directory.CreateDirectory(Path.GetDirectoryName(fileName)!);
                await File.WriteAllTextAsync(fileName, json);

                //deserialize json info
                var bodyList = JsonSerializer.Deserialize<CelestialBodyList>(json);
                savedBodies = bodyList?.bodies ?? new List<CelestialBody>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error {ex.Message}");
                savedBodies = new List<CelestialBody>();
            }
        }
    }
}
