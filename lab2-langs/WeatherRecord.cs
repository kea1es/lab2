using System;

namespace WeatherJournalApp
{
    public class WeatherRec
    {
        public int Id { get; set; }
        public DateTime DateOfRecord { get; set; }
        public float Temperature { get; set; }
        public float Pressure { get; set; }
        public string Type { get; set; }

        public WeatherRec() { }

        public override string ToString()
        {
            return $"[{DateOfRecord:yyyy-MM-dd HH:mm}]\nТемпература: {Temperature:F1} градусов Цельсия;\nPressure: {Pressure:F1} мм рт.ст.;\nТип осадков: {Type}\n";
        }
    }
}