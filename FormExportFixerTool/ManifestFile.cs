using System.IO;
using System.Linq;

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
        /// <param name="files"></param>
        internal void CombineManifestFiles(string writeOutputPath, string[] files)
        {
            string filePath = Path.Combine(writeOutputPath, manifestFileName);
            int fileNumber = 0;
            int lineCounter = 0;

            if (!File.Exists(filePath))
            {
                using (StreamWriter sw = File.CreateText(filePath))

                    foreach (var filename in files)
                    {
                        fileNumber++;
                        lineCounter = 0;

                        using (StreamReader sr = File.OpenText(filename))
                        {
                            // Skip header on first line
                            if (fileNumber > 1 && lineCounter == 0)
                            {
                                sr.ReadLine();
                            }

                            string line;
                            while ((line = sr.ReadLine()) != null)
                            {
                                sw.WriteLine(line);
                                lineCounter++;
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
        /// <returns>lineCount</returns>
        private int GetTotalLineCount(string filePath, bool skipHeaderLine)
        {
            int lineCount;

            if (skipHeaderLine == true)
            {
                return lineCount = File.ReadLines(filePath).Count() - 1;
            }
            else
            {
                return lineCount = File.ReadLines(filePath).Count();
            }
        }

    }
}
