using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using WeatherApi.TCP.Shared;

// --------------------------------------------------------------------------------
// --- CONFIGURATION ---
// --------------------------------------------------------------------------------
const int port = 8888;
var apiKey = Environment.GetEnvironmentVariable("MY_PROJECT_API_KEY");
var ipEndpoint = new IPEndPoint(IPAddress.Any, port);

// Fix: 'new' expression simplified
using HttpClient httpClient = new();

// --------------------------------------------------------------------------------
// --- START SERVER ---
// --------------------------------------------------------------------------------
// Fix: 'new' expression simplified
TcpListener server = new(ipEndpoint);

try
{
    server.Start();
    Console.WriteLine($"[SERVER] Weather Server Online on Port {port}...");

    while (true)
    {
        TcpClient client = await server.AcceptTcpClientAsync();
        Console.WriteLine($"\n[SERVER] -> Client connected.");

        NetworkStream stream = client.GetStream();

        byte[] buffer = new byte[1024];
        int bytesRead = await stream.ReadAsync(buffer);

        // Fix: Xử lý null warning
        string cityName = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
        Console.WriteLine($"[REQUEST]: '{cityName}'");

        string responseMessage = await GetWeatherTable(httpClient, cityName, apiKey);

        if (!string.IsNullOrEmpty(responseMessage))
        {
            byte[] responseBytes = Encoding.UTF8.GetBytes(responseMessage);
            await stream.WriteAsync(responseBytes);
        }

        Console.WriteLine("[SERVER]: Sent detailed table.");
        client.Close();
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Critical Error: {ex.Message}");
}
finally
{
    server.Stop();
}

// --------------------------------------------------------------------------------
// --- HELPER ---
// --------------------------------------------------------------------------------
static async Task<string> GetWeatherTable(HttpClient http, string city, string key)
{
    try
    {
        string url = $"http://api.weatherapi.com/v1/forecast.json?key={key}&q={city}&days=3&aqi=no&alerts=no";

        var response = await http.GetAsync(url);

        if (!response.IsSuccessStatusCode)
            return $"ERROR: City '{city}' not found or API Key issue.";

        string jsonString = await response.Content.ReadAsStringAsync();
        var data = JsonSerializer.Deserialize<WeatherModel>(jsonString);

        // Fix: Dereference of a possibly null reference
        if (data?.Current == null || data?.Location == null)
            return "ERROR: Data is empty.";

        var r = data;
        var c = r.Current;
        var l = r.Location;

        StringBuilder sb = new(); // Fix: Simplified new

        sb.AppendLine("\n╔════════════════════════════════════════════════════════════════════╗");
        sb.AppendLine($"║  🌍 WEATHER REPORT FOR: {l.Name?.ToUpper()}, {l.Country?.ToUpper()}");
        sb.AppendLine("╠════════════════════════════════════════════════════════════════════╣");

        sb.AppendLine("║  [📍 INFO & LOCATION]");
        sb.AppendLine($"║   • Region:     {l.Region,-20}  • Timezone: {l.TzId}");
        sb.AppendLine($"║   • Lat/Lon:    {l.Lat}/{l.Lon}          • Local Time: {l.LocalTime}");
        sb.AppendLine("║");

        sb.AppendLine("║  [🌡️ CURRENT STATUS]");
        sb.AppendLine($"║   • Condition:  {c.Condition?.Text} ({(c.IsDay == 1 ? "Day" : "Night")})");

        // --- CHỖ NÀY GIỜ SẼ HẾT LỖI VÌ ĐÃ CÓ TempF Ở MODEL ---
        sb.AppendLine($"║   • Temp:       {c.TempC}°C / {c.TempF}°F     • Feels Like: {c.FeelsLikeC}°C");

        sb.AppendLine($"║   • UV Index:   {c.Uv,-20}  • Visibility: {c.VisKm} km");
        sb.AppendLine("║");

        sb.AppendLine("║  [💨 WIND & ATMOSPHERE]");
        sb.AppendLine($"║   • Wind:       {c.WindKph} km/h ({c.WindDir})   • Gust: {c.GustKph} km/h");
        sb.AppendLine($"║   • Humidity:   {c.Humidity}%                 • Cloud: {c.Cloud}%");
        sb.AppendLine($"║   • Pressure:   {c.PressureMb} mb            • Precip: {c.PrecipMm} mm");

        sb.AppendLine("╠════════════════════════════════════════════════════════════════════╣");
        sb.AppendLine("║  [📅 3-DAY FORECAST]                                               ║");
        sb.AppendLine("╠════════════════════════════════════════════════════════════════════╣");

        if (r.Forecast?.ForecastDay != null)
        {
            foreach (var f in r.Forecast.ForecastDay)
            {
                var d = f.Day;
                // Fix: Thêm check null cho d
                if (d == null) continue;

                sb.AppendLine($"║  DATE: {f.Date}  |  {d.Condition?.Text}");
                sb.AppendLine($"║  🌡️ Max/Min: {d.MaxTempC}°C / {d.MinTempC}°C   |  ☔ Rain Chance: {d.DailyChanceOfRain}%");
                sb.AppendLine($"║  💨 Wind: {d.MaxWindKph} km/h         |  💧 Avg Humid: {d.AvgHumidity}%");
                sb.AppendLine("╟────────────────────────────────────────────────────────────────────╢");
            }
        }

        sb.AppendLine($"║  (Last Updated: {c.LastUpdated})");
        sb.AppendLine("╚════════════════════════════════════════════════════════════════════╝");

        return sb.ToString();
    }
    catch (Exception ex)
    {
        return $"SYSTEM ERROR: {ex.Message}";
    }
}