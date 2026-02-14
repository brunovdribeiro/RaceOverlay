using System.Text.Json.Serialization;

namespace RaceOverlay.Engine.Models;

public class TrackOutlineData
{
    [JsonPropertyName("trackId")]
    public int TrackId { get; set; }

    [JsonPropertyName("trackName")]
    public string TrackName { get; set; } = string.Empty;

    [JsonPropertyName("points")]
    public double[][] Points { get; set; } = [];

    [JsonIgnore]
    public (double X, double Y)[] TrackOutline =>
        Points.Select(p => (p[0], p[1])).ToArray();

    public static TrackOutlineData FromOutline(int trackId, string trackName, (double X, double Y)[] outline)
    {
        return new TrackOutlineData
        {
            TrackId = trackId,
            TrackName = trackName,
            Points = outline.Select(p => new[] { p.X, p.Y }).ToArray()
        };
    }
}
