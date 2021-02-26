using Newtonsoft.Json;

namespace Stream
{
    public class OgImage
    {
        [JsonProperty("image")]
        public string Image { get; set; }

        [JsonProperty("url")]
        public string URL { get; set; }

        [JsonProperty("secure_url")]
        public string SecureURL { get; set; }

        [JsonProperty("width")]
        public int Width { get; set; }

        [JsonProperty("height")]
        public int Height { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("alt")]
        public string Alt { get; set; }

        [JsonConstructor]
        internal OgImage()
        {
        }
    }

    public class OgVideo
    {

        [JsonProperty("video")]
        public string Video { get; set; }

        [JsonProperty("url")]
        public string URL { get; set; }

        [JsonProperty("secure_url")]
        public string SecureURL { get; set; }

        [JsonProperty("width")]
        public int Width { get; set; }

        [JsonProperty("height")]
        public int Height { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonConstructor]
        internal OgVideo()
        {
        }
    }

    public class OgAudio
    {

        [JsonProperty("audio")]
        public string Audio { get; set; }

        [JsonProperty("url")]
        public string URL { get; set; }

        [JsonProperty("secure_url")]
        public string SecureURL { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonConstructor]
        internal OgAudio()
        {
        }
    }

    public class Og
    {

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("url")]
        public string URL { get; set; }

        [JsonProperty("site")]
        public string Site { get; set; }

        [JsonProperty("site_name")]
        public string SiteName { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("favicon")]
        public string Favicon { get; set; }

        [JsonProperty("determiner")]
        public string Determiner { get; set; }

        [JsonProperty("images")]
        public OgImage[] Images { get; set; }

        [JsonProperty("videos")]
        public OgVideo[] Videos { get; set; }

        [JsonProperty("audios")]
        public OgAudio[] Audios { get; set; }

        [JsonProperty("duration")]
        public string Duration { get; set; }

        [JsonConstructor]
        internal Og()
        {
        }
    }
}


