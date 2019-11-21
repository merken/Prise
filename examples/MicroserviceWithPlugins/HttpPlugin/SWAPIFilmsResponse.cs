using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace HttpPlugin
{
    public class SWAPIFilm
    {
        [JsonProperty(PropertyName = "episode_id")]
        public int EpisodeId { get; set; }
        [JsonProperty(PropertyName = "title")]
        public string Title { get; set; }
        [JsonProperty(PropertyName = "opening_crawl")]
        public string Description { get; set; }
        [JsonProperty(PropertyName = "director")]
        public string Director { get; set; }
    }

    public class SWAPIFilmsResponse
    {
        [JsonProperty(PropertyName = "results")]
        public SWAPIFilm[] Films { get; set; }
    }
}
