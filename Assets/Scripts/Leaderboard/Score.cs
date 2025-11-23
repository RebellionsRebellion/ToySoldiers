using System;
using Newtonsoft.Json;
using UnityEngine;

public class Score
{
    [JsonProperty("name")]
    public string Name;

    [JsonProperty("score")]
    public int Points;

    [JsonProperty("created_at")]
    public DateTime Date;
}
