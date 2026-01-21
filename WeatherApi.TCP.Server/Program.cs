using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using WeatherApi.TCP.Shared;

// --------------------------------------------------------------------------------
// --- CONFIGURATION ---
// --------------------------------------------------------------------------------
const int port = 8888;
// Đọc key từ biến môi trường hoặc User Secrets
var config = new Microsoft.Extensions.Configuration.ConfigurationBuilder()
    .AddUserSecrets<Program>() // Cần package: Microsoft.Extensions.Configuration.UserSecrets
    .Build();

// Ưu tiên lấy từ User Secrets, nếu không có thì lấy Environment (cho server)
var apiKey = config["MY_PROJECT_API_KEY"] ?? Environment.GetEnvironmentVariable("MY_PROJECT_API_KEY");

var ipEndpoint = new IPEndPoint(IPAddress.Any, port);

using HttpClient httpClient = new();

// --------------------------------------------------------------------------------
// --- START SERVER ---
// --------------------------------------------------------------------------------
TcpListener server = new(ipEndpoint);

try
{
    server.Start();
    Console.WriteLine($"[SERVER] JSON Weather Server Online on Port {port}...");
    Console.WriteLine($"[INFO] Mode: Serving JSON Data for WinForms Client");

    while (true)
    {
        TcpClient client = await server.AcceptTcpClientAsync();
        Console.WriteLine($"\n[SERVER] -> Client connected.");

        NetworkStream stream = client.GetStream();

        byte[] buffer = new byte[1024];
        int bytesRead = await stream.ReadAsync(buffer);

        if (bytesRead == 0) { client.Close(); continue; }

        string cityName = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
        Console.WriteLine($"[REQUEST]: '{cityName}'");

        // Gọi hàm lấy JSON thay vì hàm lấy bảng
        string jsonResponse = await GetWeatherJson(httpClient, cityName, apiKey);

        if (!string.IsNullOrEmpty(jsonResponse))
        {
            byte[] responseBytes = Encoding.UTF8.GetBytes(jsonResponse);
            await stream.WriteAsync(responseBytes);
        }

        Console.WriteLine("[SERVER]: Sent JSON data.");
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
static async Task<string> GetWeatherJson(HttpClient http, string city, string key)
{
    try
    {
        if (string.IsNullOrEmpty(key))
        {
            // Trả về lỗi dạng JSON để Frontend hứng được
            return JsonSerializer.Serialize(new { Error = true, Message = "Server API Key is missing" });
        }

        string url = $"http://api.weatherapi.com/v1/forecast.json?key={key}&q={city}&days=3&aqi=no&alerts=no";
        var response = await http.GetAsync(url);

        if (!response.IsSuccessStatusCode)
        {
            return JsonSerializer.Serialize(new { Error = true, Message = $"City '{city}' not found or API Error." });
        }

        string jsonString = await response.Content.ReadAsStringAsync();

        // Deserialize để kiểm tra dữ liệu có hợp lệ không trước khi gửi
        var data = JsonSerializer.Deserialize<WeatherModel>(jsonString);

        if (data?.Current == null || data?.Location == null)
        {
            return JsonSerializer.Serialize(new { Error = true, Message = "Data from API is empty." });
        }

        // --- QUAN TRỌNG: Serialize lại object thành JSON để gửi cho Client ---
        // Frontend sẽ nhận được chuỗi kiểu: {"location":{...}, "current":{...}, ...}
        return JsonSerializer.Serialize(data);
    }
    catch (Exception ex)
    {
        return JsonSerializer.Serialize(new { Error = true, Message = $"System Error: {ex.Message}" });
    }
}

// Class giả để hỗ trợ UserSecrets nếu dùng Top-level statements
public partial class Program { }