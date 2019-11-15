using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CosmosDbPlugin
{
    public class DocumentBase
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        public string Body { get; set; } = string.Empty;
    }
}
