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
    string? city = Console.ReadLine()?.Trim(); // Thêm '?' vì ReadLine có thể null

    if (string.IsNullOrEmpty(city)) continue;
    if (city.ToLower() == "exit") break;

    try
    {
        // [TỐI ƯU] Dùng 'new()' thay vì 'new TcpClient()'
        using TcpClient client = new();
        await client.ConnectAsync(SERVER_IP, PORT);
        NetworkStream stream = client.GetStream();

        // 1. Gửi request
        byte[] dataToSend = Encoding.UTF8.GetBytes(city);
        await stream.WriteAsync(dataToSend);

        // 2. Nhận response (JSON)
        byte[] buffer = new byte[16384];
        int bytesRead = await stream.ReadAsync(buffer);

        if (bytesRead == 0) { Console.WriteLine("Server ngắt kết nối."); continue; }

        string jsonResponse = Encoding.UTF8.GetString(buffer, 0, bytesRead);

        // --- PARSE DỮ LIỆU & VẼ BẢNG ---
        try
        {
            var data = JsonSerializer.Deserialize<WeatherModel>(jsonResponse);

            if (data != null && data.Location != null)
            {
                // [QUAN TRỌNG] Phải có dòng này thì hàm RenderFullTable mới được dùng
                RenderFullTable(data);
            }
            else
            {
                // Thử check xem có phải lỗi từ server gửi về không
                var errorObj = JsonSerializer.Deserialize<ServerError>(jsonResponse);
                if (errorObj != null && errorObj.Error)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"\n❌ SERVER BÁO LỖI: {errorObj.Message}");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("⚠️ Dữ liệu không hợp lệ.");
                }
            }
        }
        catch (JsonException)
        {
            Console.WriteLine("Lỗi đọc JSON.");
        }
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Lỗi: {ex.Message}");
    }
}

// --- HÀM VẼ BẢNG CHI TIẾT ---
static void RenderFullTable(WeatherModel r)
{
    var l = r.Location;
    var c = r.Current;

    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine("\n╔════════════════════════════════════════════════════════════════════╗");
    Console.WriteLine($"║  🌍 REPORT: {l?.Name?.ToUpper()}, {l?.Country?.ToUpper()}");
    Console.WriteLine("╠════════════════════════════════════════════════════════════════════╣");

    // ... (Phần hiển thị Info/Current giữ nguyên như cũ) ...
    Console.WriteLine($"║   • Temp:       {c?.TempC}°C / {c?.TempF}°F     • Condition: {c?.Condition?.Text}");

    Console.WriteLine("╠════════════════════════════════════════════════════════════════════╣");
    Console.WriteLine("║  [📅 3-DAY FORECAST & HOURLY DETAIL]                               ║");
    Console.WriteLine("╠════════════════════════════════════════════════════════════════════╣");

    if (r.Forecast?.ForecastDay != null)
    {
        foreach (var f in r.Forecast.ForecastDay)
        {
            var d = f.Day;
            if (d == null) continue;

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"║  📅 DATE: {f.Date} ({d.Condition?.Text})");
            Console.WriteLine($"║  🌡️ Max: {d.MaxTempC}°C | Min: {d.MinTempC}°C | ☔ Rain: {d.DailyChanceOfRain}%");

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("║  ------------------------------------------------------------------");
            Console.WriteLine("║  ⏰ Time  | 🌡️ Temp | ☔ Rain% | 💨 Wind  | Condition");
            Console.WriteLine("║  ------------------------------------------------------------------");

            if (f.Hour != null)
            {
                // [TỐI ƯU] Dùng Collection Expression (C# 12) [] thay vì int[] {}
                int[] targetHours = [3, 6, 9, 15, 18, 21];

                foreach (var h in f.Hour)
                {
                    if (DateTime.TryParse(h.Time, out DateTime dt))
                    {
                        if (targetHours.Contains(dt.Hour))
                        {
                            string timeStr = dt.ToString("HH:mm");
                            string tempStr = $"{h.TempC}°C";
                            string rainStr = $"{h.ChanceOfRain}%";
                            string windStr = $"{h.WindKph}km";

                            // [SỬA LỖI] Substring simplified -> Dùng Range Operator [..]
                            string condStr = h.Condition?.Text ?? "";
                            if (condStr.Length > 15)
                            {
                                condStr = condStr[..12] + "..."; // Thay thế Substring(0, 12)
                            }

                            Console.WriteLine($"║  {timeStr,-6} | {tempStr,-7} | {rainStr,-7} | {windStr,-7} | {condStr}");
                        }
                    }
                }
            }
            Console.WriteLine("╟────────────────────────────────────────────────────────────────────╢");
        }
    }
    Console.WriteLine("╚════════════════════════════════════════════════════════════════════╝");
    Console.ResetColor();
}