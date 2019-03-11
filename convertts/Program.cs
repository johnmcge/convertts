using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Threading;

namespace convertts
{
    class Program
    {
        public static ConcurrentQueue<string> FilesToProcessQueue = new ConcurrentQueue<string>();
        public static string Prefix2IndicateInProcess = "inproc-";
        public static bool Verbose = true;
        public static string LogFileName = "logfile.txt";

        static void Main(string[] args)
        {

            Console.WriteLine("Getting list of files to work on ..." + Environment.NewLine);
            foreach (string path in args)
            {
                if (Directory.Exists(path))
                    ProcessDirectory(path);
                else
                    Console.WriteLine("{0} is not a valid directory.", path);
            }

            Console.WriteLine(Environment.NewLine + "Number of files = " + FilesToProcessQueue.Count.ToString() + Environment.NewLine);

            while (FilesToProcessQueue.TryDequeue(out string FileToConvert))
            {
                TryConvert(FileToConvert);
            }
        }

        public static void ProcessDirectory(string targetDirectory)
        {
            // Process the list of files found in the directory.
            string[] fileEntries = Directory.GetFiles(targetDirectory);
            foreach (string fileName in fileEntries)
                ProcessFile(fileName);

            // Recurse into subdirectories of this directory.
            string[] subdirectoryEntries = Directory.GetDirectories(targetDirectory);
            foreach (string subdirectory in subdirectoryEntries)
                ProcessDirectory(subdirectory);
        }

        public static void ProcessFile(string path)
        {
            int idx = path.LastIndexOf('.');
            string ext = path.Substring((idx), (path.Length - idx));

            int idx1 = path.LastIndexOf("\\") + 1;
            string FullFileName = path.Substring(idx1, (path.Length - idx1));
            
            if (FullFileName.Length > Prefix2IndicateInProcess.Length)
                if (FullFileName.Substring(0, (Prefix2IndicateInProcess.Length)) == Prefix2IndicateInProcess)
                {
                    Console.WriteLine("*** Skipping file in process: " + FullFileName + Environment.NewLine);
                    return;
                }

            //if (Verbose)
            //{
            //    Console.WriteLine("Process file '{0}'", path);
            //    Console.WriteLine("Full FileName '{0}'", FullFileName);
            //    Console.WriteLine("  Extension =  '{0}'", ext);
            //    Console.WriteLine("");
            //}

            if (ext == ".ts")
                FilesToProcessQueue.Enqueue(path);

        }

        static void TryConvert(string FileToConvert)
        {
            if (!File.Exists(FileToConvert))
            {
                LogMessage(FileToConvert, "File skipped, not found", 0, 0);
                return;
            }

            int idx = FileToConvert.LastIndexOf("\\") + 1;
            string FilePath = FileToConvert.Substring(0, idx);
            string FullFileName = FileToConvert.Substring(idx, (FileToConvert.Length - idx));

            int idx1 = FullFileName.LastIndexOf('.');
            string FileName = FullFileName.Substring(0, idx1);
            string FileExtension = FullFileName.Substring((idx1), (FullFileName.Length - idx1));

            string inProcessFileName = Path.Combine(FilePath, (Prefix2IndicateInProcess + FullFileName));
            string outFileName = Path.Combine(FilePath, (FileName + ".mp4"));

            if (Verbose)
            {
                Console.WriteLine(" FileToConvert: " + FileToConvert);
                Console.WriteLine("Full File name: " + FullFileName);
                Console.WriteLine("     File name: " + FileName);
                Console.WriteLine("     File path: " + FilePath);
                Console.WriteLine("      File ext: " + FileExtension);
                Console.WriteLine("inProcFileName: " + inProcessFileName);
                Console.WriteLine("   OutFileName: " + outFileName);
                Console.WriteLine("");
            }

            long lengthOriginalFile = new FileInfo(FileToConvert).Length;

            try
            {
                File.Move(FileToConvert, inProcessFileName);
            }
            catch 
            {
                Console.WriteLine("Failed to move file to inProcess state: " + FileToConvert);
                return;
            }

            DateTime dtMarker = DateTime.Now;

            // ffmpeg -i input.ts -acodec copy -preset slow output.mp4

            StringBuilder sb = new StringBuilder();
            sb.Append(" -i " + "\"" + inProcessFileName + "\"");
            sb.Append(" -acodec copy -preset slow ");
            sb.Append("\"" + outFileName + "\"");


            Console.WriteLine("Start conversion: " + FileToConvert);
            using (Process process = new Process())
            {
                process.StartInfo.FileName = "ffmpeg.exe";
                process.StartInfo.Arguments = sb.ToString();
                process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                process.Start();
                process.WaitForExit();
            }

            Thread.Sleep(1000);
            try
            {
                File.Delete(FileToConvert);
            }
            catch
            {
                Console.WriteLine("Exception trying to delete source file: " + FileToConvert);
            }

            string dur = (DateTime.Now - dtMarker).ToString("hh':'mm':'ss");
            long lengthNewFile = new FileInfo(outFileName).Length;

            //LogMessage(outFileName, dur, lengthNewFile, lengthOriginalFile);

            Console.WriteLine("End conversion: " + FileToConvert);
        }

        static void LogMessage(string fname, string convertDuration, long newFile, long origFile)
        {
            string message = "";
            message += DateTime.Now.ToString("hh':'mm'");
            message += "\t" + fname;
            message += "\t" + convertDuration;
            message += "\t" + newFile.ToString();
            message += "\t" + origFile.ToString();

            File.AppendAllText(LogFileName, message);

        }

    }
}

/*
 * 
ffmpeg -ss 00:20:00 -t 59:59:59 -i "Young Sheldon (2017) - S02E01 - A High-Pitched Buzz and Training Wheels - TEST.ts" -acodec copy -vcodec copy "Young Sheldon (2017) - S02E01 - A High-Pitched Buzz and Training Wheels - TESTTRIMMED.ts"
 * */
