using System.Text.Json.Serialization;

namespace WeatherApi.TCP.Shared
{
    // --- GIỮ NGUYÊN CODE CŨ CỦA BẠN TỪ ĐÂY ---
    public class WeatherModel
    {
        [JsonPropertyName("location")]
        public Location? Location { get; set; }

        [JsonPropertyName("current")]
        public Current? Current { get; set; }

        [JsonPropertyName("forecast")]
        public Forecast? Forecast { get; set; }
    }

    public class Location
    {
        [JsonPropertyName("name")] public string? Name { get; set; }
        [JsonPropertyName("region")] public string? Region { get; set; }
        [JsonPropertyName("country")] public string? Country { get; set; }
        [JsonPropertyName("lat")] public double Lat { get; set; }
        [JsonPropertyName("lon")] public double Lon { get; set; }
        [JsonPropertyName("tz_id")] public string? TzId { get; set; }
        [JsonPropertyName("localtime")] public string? LocalTime { get; set; }
    }

    public class Current
    {
        [JsonPropertyName("last_updated")] public string? LastUpdated { get; set; }
        [JsonPropertyName("temp_c")] public double TempC { get; set; }
        [JsonPropertyName("temp_f")] public double TempF { get; set; } // Đã có cái này là OK
        [JsonPropertyName("is_day")] public int IsDay { get; set; }
        [JsonPropertyName("condition")] public Condition? Condition { get; set; }
        [JsonPropertyName("wind_kph")] public double WindKph { get; set; }
        [JsonPropertyName("wind_dir")] public string? WindDir { get; set; }
        [JsonPropertyName("pressure_mb")] public double PressureMb { get; set; }
        [JsonPropertyName("precip_mm")] public double PrecipMm { get; set; }
        [JsonPropertyName("humidity")] public int Humidity { get; set; }
        [JsonPropertyName("cloud")] public int Cloud { get; set; }
        [JsonPropertyName("feelslike_c")] public double FeelsLikeC { get; set; }
        [JsonPropertyName("vis_km")] public double VisKm { get; set; }
        [JsonPropertyName("uv")] public double Uv { get; set; }
        [JsonPropertyName("gust_kph")] public double GustKph { get; set; }
    }

    public class Condition
    {
        [JsonPropertyName("text")] public string? Text { get; set; }
    }

    public class Forecast
    {
        [JsonPropertyName("forecastday")]
        public List<ForecastDay>? ForecastDay { get; set; }
    }

    public class ForecastDay
    {
        [JsonPropertyName("date")] public string? Date { get; set; }
        [JsonPropertyName("day")] public Day? Day { get; set; }
    }

    public class Day
    {
        [JsonPropertyName("maxtemp_c")] public double MaxTempC { get; set; }
        [JsonPropertyName("mintemp_c")] public double MinTempC { get; set; }
        [JsonPropertyName("maxwind_kph")] public double MaxWindKph { get; set; }
        [JsonPropertyName("totalprecip_mm")] public double TotalPrecipMm { get; set; }
        [JsonPropertyName("avghumidity")] public double AvgHumidity { get; set; }
        [JsonPropertyName("daily_chance_of_rain")] public int DailyChanceOfRain { get; set; }
        [JsonPropertyName("condition")] public Condition? Condition { get; set; }
    }

    // --- ERROR ---
    public class ServerError
    {
        public bool Error { get; set; }
        public string? Message { get; set; }
    }
}

