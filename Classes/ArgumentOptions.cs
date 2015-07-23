using CommandLine;
using CommandLine.Text;

namespace YahooScreenToTS
{
    class Options
    {
        [Option('v', "videoId", Required = true, HelpText = "Yahoo! Screen Video Id.")]
        public string VideoId { get; set; }

        [Option('o', "outputFile", HelpText = "Output filename.")]
        public string OutputFile { get; set; }

        [Option('t', "threads", HelpText = "Download threads. Defaults to 1. 0 = unlimited")]
        public int? DownloadThreads { get; set; }

        [ParserState]
        public IParserState LastParserState { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            return HelpText.AutoBuild(this, (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));
        }
    }
}