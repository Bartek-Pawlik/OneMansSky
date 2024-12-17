using System.Net.Http;
using System.Text.Json;

namespace OneMansSky
{
    public static class CelestialDataService
    {
        //setting fileName to be appdata folder to get celestial bodies from
        private static readonly string fileName = Path.Combine(FileSystem.AppDataDirectory, "celestialBodies.json");
        private static List<CelestialBody>? savedBodies;

        public class CelestialBodyList
        {
            public List<CelestialBody>? bodies { get; set; }
        }

        //celestial body field, very basic for now, more will be added later
        public class CelestialBody
        {
            public string name { get; set; }
        }

        //func to return bodies from api or local file
        public static async Task<List<CelestialBody>> GetCelestialInfo()
        {
            if (savedBodies is not null)
            {
                return savedBodies;
            }

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
