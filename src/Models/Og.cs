namespace Stream.Models
{
    using System.Collections.Generic;

    public class OgImage
    {
        public string Image { get; set; }
        public string Url { get; set; }
        public string SecureUrl { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string Type { get; set; }
        public string Alt { get; set; }
    }

    public class OgVideo
    {
        public string Video { get; set; }
        public string Url { get; set; }
        public string SecureUrl { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string Type { get; set; }
    }

    public class OgAudio
    {
        public string Audio { get; set; }
        public string URL { get; set; }
        public string SecureUrl { get; set; }
        public string Type { get; set; }
    }

    public class Og : ResponseBase
    {
        public string Title { get; set; }
        public string Type { get; set; }
        public string Url { get; set; }
        public string Site { get; set; }
        public string SiteName { get; set; }
        public string Description { get; set; }
        public string Favicon { get; set; }
        public string Determiner { get; set; }
        public List<OgImage> Images { get; set; }
        public List<OgVideo> Videos { get; set; }
        public List<OgAudio> Audios { get; set; }
    }
}


