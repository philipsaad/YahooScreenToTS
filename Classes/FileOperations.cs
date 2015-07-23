using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace YahooScreenToTS
{
    public static class FileOperations
    {
        public static List<string> ConcatenateFiles(List<string> transportStreamFiles, string outputFile)
        {
            using (Stream outputStream = File.OpenWrite(outputFile))
            {
                foreach (string transportStreamFile in transportStreamFiles)
                {
                    using (Stream transportStream = File.OpenRead(transportStreamFile))
                    {
                        transportStream.CopyTo(outputStream);
                    }
                }
            }

            return transportStreamFiles;
        }

        public static List<string> DownloadFiles(List<string> transportStreamFiles, int downloadThreads)
        {
            List<string> downloadedTransportStreamFiles = transportStreamFiles;

            int count = 1;
            decimal total = transportStreamFiles.Count;

            Parallel.ForEach(transportStreamFiles,
                new ParallelOptions { MaxDegreeOfParallelism = downloadThreads },
                transportStreamFile =>
                {
                    using (var webClient = new WebClient())
                    {
                        string percent = (count / total).ToString("p");
                        Console.Write("\r{0}", "".PadRight(60, ' '));
                        Console.Write("\rDownloading file " + count + " of " + total + ". (" + percent + ")");

                        string tempFile = Path.GetTempFileName();
                        webClient.DownloadFile(transportStreamFile, tempFile);
                        downloadedTransportStreamFiles[transportStreamFiles.IndexOf(transportStreamFile)] = tempFile;

                        count++;
                    }
                });

            return downloadedTransportStreamFiles;
        }

        public static void DeleteFiles(List<string> transportStreamFiles)
        {
            //Use PLINQ to loop through each file using a parallel loop
            Parallel.ForEach(transportStreamFiles, fileToDelete =>
            {
                //Delete the file
                File.Delete(fileToDelete);
            });
        }
    }
}
