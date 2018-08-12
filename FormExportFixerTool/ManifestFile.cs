using System.Collections.Generic;
using System.IO;
using System.Linq;
using ShellProgressBar;

namespace FormExportFixerTool
{
    internal class ManifestFile
    {
        // Class variables
        private readonly string manifestFileName = "manifest.txt";
        private readonly string _InputFolderPath;
        private readonly string _OutputFolderPath;

        // Constructors
        public ManifestFile()
        {
        }

        public ManifestFile(string InputFolderPath, string OutputFolderPath)
        {
            _InputFolderPath = InputFolderPath;
            _OutputFolderPath = OutputFolderPath;
        }

        /// <summary>
        /// Gather all manifest file paths in a directory
        /// </summary>
        /// <param name="inputPath"></param>
        /// <returns></returns>
        internal string[] GetManifestFiles(string inputPath)
        {
            string[] manifestFiles;
            return manifestFiles = Directory.GetFiles(_InputFolderPath, manifestFileName, SearchOption.AllDirectories);
        }

        /// <summary>
        /// Combine manifest files into single file.
        /// </summary>
        /// <param name="writeOutputPath"></param>
        /// <param name="manifestFiles"></param>
        internal void CombineManifestFiles(string writeOutputPath, string[] manifestFiles)
        {
            string filePath = Path.Combine(writeOutputPath, manifestFileName);
            int fileNumber = 0;
            int lineCounter = 0;

            using (var progressBar = new ProgressBar(GetTotalLineCount(manifestFiles), "Action 1 of 2 - Combine Manifest Files...", new ProgressBarOptions { BackgroundColor = System.ConsoleColor.DarkGray }))

                if (!File.Exists(filePath))
                {
                    using (StreamWriter sw = File.CreateText(filePath))
                        foreach (string filename in manifestFiles)
                        {
                            fileNumber++;
                            lineCounter = 0;
                            using (StreamReader sr = File.OpenText(filename))
                            {
                                // Skip first line header after first file
                                if (fileNumber > 1 && lineCounter == 0)
                                {
                                    sr.ReadLine();
                                }
                                string line;
                                while ((line = sr.ReadLine()) != null)
                                {
                                    sw.WriteLine(line);
                                    lineCounter++;
                                    progressBar.Tick();                                    
                                }
                            }
                        }
                }
        }

        /// <summary>
        /// Returns total count of a given file search pattern
        /// </summary>
        /// <param name="searchPattern"></param>
        /// Example: int totalManifestFiles = GetNumberOfFiles(manifestFileName);
        /// <returns></returns>
        private int GetNumberOfFiles(string searchPattern)
        {
            return Directory.GetFiles(_InputFolderPath, searchPattern, SearchOption.AllDirectories).Length;
        }

        /// <summary>
        /// Gets total line count from a file
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="skipHeaderLine"></param>
        /// <returns>lineCount without header lines. Except counting header line of the first file
        private int GetTotalLineCount(string[] filePaths)
        {
            int lineCount = 0;
            string filePath;
            List<int> totalLines = new List<int>();
            for (int i = 0; i < filePaths.Length; i++)
            {
                filePath = filePaths[i];
                lineCount = File.ReadLines(filePath).Count() - 1;
                totalLines.Add(lineCount);
            }
            return totalLines.Sum() + 1;
        }

    }
}
