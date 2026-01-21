using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using WeatherApi.TCP.Shared;

Console.OutputEncoding = Encoding.UTF8;
Console.Title = "FULL WEATHER CLIENT (DEBUG MODE)";

const string SERVER_IP = "127.0.0.1";
const int PORT = 8888;

Console.WriteLine($"--- KẾT NỐI TỚI SERVER {PORT} ---");

while (true)
{
    Console.ForegroundColor = ConsoleColor.White;
    Console.Write("\nNhập tên thành phố (gõ 'exit' để thoát): ");
    string city = Console.ReadLine()?.Trim();

    if (string.IsNullOrEmpty(city)) continue;
    if (city.ToLower() == "exit") break;

    try
    {
        using TcpClient client = new TcpClient();
        await client.ConnectAsync(SERVER_IP, PORT);
        NetworkStream stream = client.GetStream();

        // 1. Gửi request
        byte[] dataToSend = Encoding.UTF8.GetBytes(city);
        await stream.WriteAsync(dataToSend);

        // 2. Nhận response (JSON)
        byte[] buffer = new byte[16384]; // Buffer lớn
        int bytesRead = await stream.ReadAsync(buffer);

        if (bytesRead == 0) { Console.WriteLine("Server ngắt kết nối."); continue; }

        string jsonResponse = Encoding.UTF8.GetString(buffer, 0, bytesRead);

        // ========================================================================
        // [YÊU CẦU CỦA BẠN] HIỂN THỊ RAW JSON
        // ========================================================================
        Console.ForegroundColor = ConsoleColor.DarkGray; // Màu xám tối để phân biệt
        Console.WriteLine("\n[RAW JSON FROM SERVER]:");
        Console.WriteLine(jsonResponse);
        Console.ResetColor();
        // ========================================================================

        // --- XỬ LÝ LỖI ---
        try
        {
            var errorObj = JsonSerializer.Deserialize<ServerError>(jsonResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (errorObj != null && errorObj.Error)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\n❌ SERVER BÁO LỖI: {errorObj.Message}");
                continue;
            }
        }
        catch { }

        // --- PARSE DỮ LIỆU & VẼ BẢNG ---
        var data = JsonSerializer.Deserialize<WeatherModel>(jsonResponse);

        if (data != null && data.Location != null)
        {
            RenderFullTable(data);
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("⚠️ Dữ liệu JSON không khớp với WeatherModel.");
        }
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Lỗi: {ex.Message}");
    }
}

// --- HÀM VẼ BẢNG CHI TIẾT (FULL OPTION) ---
static void RenderFullTable(WeatherModel r)
{
    var l = r.Location;
    var c = r.Current;

    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine("\n╔════════════════════════════════════════════════════════════════════╗");
    Console.WriteLine($"║  🌍 REPORT: {l?.Name?.ToUpper()}, {l?.Country?.ToUpper()}");
    Console.WriteLine("╠════════════════════════════════════════════════════════════════════╣");

    // INFO & LOCATION
    Console.WriteLine("║  [📍 INFO & LOCATION]");
    Console.WriteLine($"║   • Region:     {l?.Region,-20}  • Timezone: {l?.TzId}");
    Console.WriteLine($"║   • Lat/Lon:    {l?.Lat}/{l?.Lon}          • Local Time: {l?.LocalTime}");
    Console.WriteLine("║");

    // CURRENT STATUS
    Console.WriteLine("║  [🌡️ CURRENT STATUS]");
    Console.WriteLine($"║   • Condition:  {c?.Condition?.Text} ({(c?.IsDay == 1 ? "Day" : "Night")})");
    Console.WriteLine($"║   • Temp:       {c?.TempC}°C / {c?.TempF}°F     • Feels Like: {c?.FeelsLikeC}°C");
    Console.WriteLine($"║   • UV Index:   {c?.Uv,-20}  • Visibility: {c?.VisKm} km");
    Console.WriteLine("║");

    // WIND & ATMOSPHERE
    Console.WriteLine("║  [💨 WIND & ATMOSPHERE]");
    Console.WriteLine($"║   • Wind:       {c?.WindKph} km/h ({c?.WindDir})   • Gust: {c?.GustKph} km/h");
    Console.WriteLine($"║   • Humidity:   {c?.Humidity}%                 • Cloud: {c?.Cloud}%");
    Console.WriteLine($"║   • Pressure:   {c?.PressureMb} mb            • Precip: {c?.PrecipMm} mm");

    Console.WriteLine("╠════════════════════════════════════════════════════════════════════╣");
    Console.WriteLine("║  [📅 3-DAY FORECAST]                                               ║");
    Console.WriteLine("╠════════════════════════════════════════════════════════════════════╣");

    if (r.Forecast?.ForecastDay != null)
    {
        foreach (var f in r.Forecast.ForecastDay)
        {
            var d = f.Day;
            if (d == null) continue;

            Console.WriteLine($"║  DATE: {f.Date}  |  {d.Condition?.Text}");
            Console.WriteLine($"║  🌡️ Max/Min: {d.MaxTempC}°C / {d.MinTempC}°C   |  ☔ Rain Chance: {d.DailyChanceOfRain}%");
            Console.WriteLine($"║  💨 Wind: {d.MaxWindKph} km/h         |  💧 Avg Humid: {d.AvgHumidity}%");
            Console.WriteLine("╟────────────────────────────────────────────────────────────────────╢");
        }
    }

    Console.WriteLine($"║  (Last Updated: {c?.LastUpdated})");
    Console.WriteLine("╚════════════════════════════════════════════════════════════════════╝");
    Console.ResetColor();
}