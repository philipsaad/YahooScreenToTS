using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YahooScreenToTS
{
    class Program
    {
        static void Main(string[] args)
        {
            var options = new Options();
            if (!CommandLine.Parser.Default.ParseArguments(args, options))
            {
                return;
            }
            
            YahooSource yahooSource = Yahoo.GetBestYahooSource(options.VideoId);

            int downloadThreads = options.DownloadThreads ?? 1;
            if (downloadThreads == 0) { downloadThreads = -1; }

            if (yahooSource.Url.Contains(".m3u8"))
            {
                FileOperations.DeleteFiles(FileOperations.ConcatenateFiles(FileOperations.DownloadFiles(Yahoo.GetYahooTransportFileList(yahooSource.Url), downloadThreads), options.OutputFile ?? yahooSource.FileName));

                Console.Write("\r{0}", "".PadRight(60, ' '));
                Console.WriteLine("\rFile saved as: \"" + (options.OutputFile ?? yahooSource.FileName) + "\"");
            }
            else if (yahooSource.Url.Contains(".mp4"))
            {
                FileOperations.DeleteFiles(FileOperations.ConcatenateFiles(FileOperations.DownloadFiles(new List<string>() { yahooSource.Url }, downloadThreads), options.OutputFile ?? yahooSource.FileName));

                Console.Write("\r{0}", "".PadRight(60, ' '));
                Console.WriteLine("\rFile saved as: \"" + (options.OutputFile ?? yahooSource.FileName) + "\"");
            }
        }
    }
}
