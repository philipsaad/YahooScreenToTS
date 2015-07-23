using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Fizzler.Systems.HtmlAgilityPack;

namespace YahooScreenToTS
{
    class Yahoo
    {
        public static List<YahooSource> GetYahooSources(string videoID)
        {
            //TODO Check validity of videoID

            string videoHtmlUrl = "https://screen.yahoo.com/" + videoID + ".html";

            List<YahooSource> results = new List<YahooSource>();

            var wc = new WebClient();
            wc.Encoding = System.Text.UTF8Encoding.UTF8;

            string videoHtmlData = wc.DownloadString(videoHtmlUrl);

            var html = new HtmlAgilityPack.HtmlDocument();
            html.LoadHtml(videoHtmlData);

            var document = html.DocumentNode;
            var videoIDTag = document.QuerySelector(".pause-screen");

            string internalVideoID = videoIDTag.Attributes["data-uuid"].Value;

            string videoUrl = "https://video.media.yql.yahoo.com/v1/video/sapi/streams/" + internalVideoID;
            string videoData = wc.DownloadString(videoUrl);

            JObject videoInfo = JObject.Parse(videoData);
            JToken videoVersions = videoInfo["query"]["results"]["mediaObj"];

            foreach (JToken videoVersion in videoVersions)
            {
                string fileName = GetFileName(videoVersion);
                JToken streams = videoVersion["streams"];

                foreach (JToken stream in streams)
                {
                    string host = stream["host"].Value<string>();
                    string path = stream["path"].Value<string>();

                    int width = stream["width"].Value<int>();
                    int height = stream["height"].Value<int>();
                    int bitrate = stream["bitrate"].Value<int>();
                    int duration = stream["duration"].Value<int>();
                    string mimeType = stream["mime_type"].Value<string>();
                    bool live = stream["live"].Value<bool>();
                    string h264Profile = stream["h264_profile"].Value<string>();
                    bool isPrimary = stream["is_primary"].Value<bool>();
                    string cdn = stream["cdn"].Value<string>();
                    string format = stream["format"].Value<string>();
                    string url = host + path;

                    YahooSource vr = new YahooSource()
                    {
                        Host = host,
                        Path = path,
                        Width = width,
                        Height = height,
                        BitRate = bitrate,
                        Duration = duration,
                        MimeType = mimeType,
                        Live = live,
                        H264Profile = h264Profile,
                        CDN = cdn,
                        Format = format,
                        FileName = fileName + "." + format,
                        Url = url,
                    };

                    results.Add(vr);
                }
            }

            return results;
        }

        private static string GetFileName(JToken videoInfo)
        {
            string fileName = String.Empty;

            string title = videoInfo["meta"]["title"].ToString();

            IEnumerable<string> mainArtists = videoInfo["meta"]["credits"]["main_artists"].ToString().Split(',');
            IEnumerable<string> featuredArtists = videoInfo["meta"]["credits"]["featured_artists"].ToString().Split(',');

            int commaIndex = -1;

            fileName = string.Join(", ", mainArtists);

            commaIndex = fileName.LastIndexOf(",");
            if (commaIndex > -1)
            {
                fileName = fileName.Substring(0, commaIndex) + " &" + fileName.Substring(commaIndex + 1);
            }

            fileName += " - " + title;

            if (featuredArtists.Count() > 0)
            {
                fileName += " (feat. " + string.Join(", ", featuredArtists) + ")";

                commaIndex = fileName.LastIndexOf(",");
                if (commaIndex > -1 && featuredArtists.Count() > 1)
                {
                    fileName = fileName.Substring(0, commaIndex) + " &" + fileName.Substring(commaIndex + 1);
                }
            }

            fileName = string.Join("_", fileName.Split(Path.GetInvalidFileNameChars()));
            fileName = fileName.Replace(" (Official Video)", String.Empty);
            fileName = fileName.Replace(" (Official)", String.Empty);
            fileName = fileName.Replace(" (Explicit Video)", String.Empty);
            fileName = fileName.Replace(" (Explicit)", String.Empty);

            return fileName;
        }

        public static YahooSource GetBestYahooSource(string videoID)
        {
            List<YahooSource> yahooSources = GetYahooSources(videoID).ToList();

            YahooSource result = yahooSources.OrderByDescending(ys => ys.Height).OrderByDescending(ys => ys.BitRate).FirstOrDefault();

            return result;
        }

        public static List<string> GetYahooTransportFileList(string transportFileUrl)
        {
            List<string> result = new List<string>();

            var wc = new WebClient();
            wc.Encoding = System.Text.UTF8Encoding.UTF8;

            string m3uData = wc.DownloadString(transportFileUrl);

            List<string> m3uDataArray = m3uData.Split(new string[] { "\n" }, StringSplitOptions.None).ToList();

            string rootUrl = transportFileUrl.Substring(0, transportFileUrl.LastIndexOf("/") + 1);

            foreach (string m3uLine in m3uDataArray)
            {
                if (!String.IsNullOrWhiteSpace(m3uLine) && !m3uLine.StartsWith("#"))
                {
                    result.Add(rootUrl + m3uLine);
                }

            }

            return result;
        }
    }

    public class YahooSource
    {
        public string Host { get; set; }
        public string Path { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int BitRate { get; set; }
        public int Duration { get; set; }
        public string MimeType { get; set; }
        public bool Live { get; set; }
        public string H264Profile { get; set; }
        public bool IsPrimary { get; set; }
        public string CDN { get; set; }
        public string Format { get; set; }
        public string FileName { get; set; }
        public string Url { get; set; }
    }
}
