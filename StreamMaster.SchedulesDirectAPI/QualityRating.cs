﻿using System.Text.Json.Serialization;

namespace StreamMaster.SchedulesDirectAPI;

public class QualityRating
{
    [JsonPropertyName("ratingsBody")]
    public string RatingsBody { get; set; }

    [JsonPropertyName("rating")]
    public string Rating { get; set; }

    [JsonPropertyName("minRating")]
    public string MinRating { get; set; }

    [JsonPropertyName("maxRating")]
    public string MaxRating { get; set; }

    [JsonPropertyName("increment")]
    public string Increment { get; set; }

    public QualityRating() { }
}
