using System.Text.Json;

namespace ParkingAppApi
{
    public class JsonFileManager<T>
    {
        private string filepath;
        public JsonFileManager(string filepath)
        {
            this.filepath = filepath;
        }
        public void WriteToFile(List<T> data) 
        {
            try 
            {
                var json = JsonSerializer.Serialize(data);
                File.WriteAllText(filepath, json);
            }
            catch (Exception ex) {Console.WriteLine(ex.Message); }
        }
        public List<T> ReadFromFile()
        {
            if (File.Exists(filepath))
            {
                var json = File.ReadAllText(filepath);
                return JsonSerializer.Deserialize<List<T>>(json) ?? new List<T>();
            }
            return new List<T>();
            
        }
    }
}
