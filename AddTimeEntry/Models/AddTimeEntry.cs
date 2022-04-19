using System;
using Newtonsoft.Json;

namespace AddTimeEntry.Models;

public class AddTimeEntry
{
    [JsonProperty(Required = Required.Always)]
    public string StartOn { get; set; }
            
    [JsonProperty(Required = Required.Always)]
    public string EndOn { get; set; }
}