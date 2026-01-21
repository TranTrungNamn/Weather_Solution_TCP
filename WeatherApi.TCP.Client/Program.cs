using System.Net.Sockets;
using System.Text;

// Cấu hình kết nối
const string serverIp = "127.0.0.1";
const int port = 8888;

// Bắt buộc dùng UTF8 để hiển thị khung bảng đẹp
Console.OutputEncoding = Encoding.UTF8;
Console.Title = "SUPER WEATHER CLIENT";

Console.WriteLine("--- KẾT NỐI ĐẾN MÁY CHỦ THỜI TIẾT ---");

while (true)
{
    Console.ForegroundColor = ConsoleColor.White;
    Console.Write("\nNhập tên thành phố (gõ 'exit' để thoát): ");
    string city = Console.ReadLine();

    if (string.Equals(city, "exit", StringComparison.OrdinalIgnoreCase)) break;

    try
    {
        using TcpClient client = new TcpClient();
        await client.ConnectAsync(serverIp, port);

        NetworkStream stream = client.GetStream();

        // 1. Gửi tên thành phố
        byte[] dataToSend = Encoding.UTF8.GetBytes(city);
        await stream.WriteAsync(dataToSend);

        // 2. Nhận báo cáo (QUAN TRỌNG: Buffer lớn để chứa hết dữ liệu)
        byte[] buffer = new byte[8192];
        int bytesRead = await stream.ReadAsync(buffer);

        string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);

        // In ra màn hình với màu xanh cyan
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(response);
    }
    catch (SocketException)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("Lỗi: Không kết nối được Server.");
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Lỗi: {ex.Message}");
    }
}