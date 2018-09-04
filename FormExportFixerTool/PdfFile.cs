using iTextSharp.text.pdf;
using Microsoft.VisualBasic.FileIO;
using ShellProgressBar;
using System;
using System.Collections.Generic;
using System.IO;

namespace FormExportFixerTool
{
    internal class PdfFile
    {
        // Class variables
        private readonly string _inputFolderPath;
        private readonly string _outputFolderPath;
        private readonly string _xrefFilePath;
        internal string tempDirectory;


        // Constructors
        public PdfFile()
        {
        }

        public PdfFile(string inputPath, string outputPath, string xrefFile)
        {
            _inputFolderPath = inputPath;
            _outputFolderPath = outputPath;
            _xrefFilePath = xrefFile;
        }

        /// <summary>
        /// Parses comma delimited file and stores source and append file paths
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns>Dictionary of strings</returns>
        internal Dictionary<string, string> ParseFilePaths(string filePath)
        {
            string userId;
            string lastName;
            string formName;
            string sourceLinesPath;
            string appendLinesPath;
            bool firstLine = true;
            string[] fields;

            Dictionary<string, string> appendFilePaths = new Dictionary<string, string>();

            TextFieldParser xrefParser = new TextFieldParser(filePath);
            xrefParser.TextFieldType = FieldType.Delimited;
            xrefParser.SetDelimiters(",");

            while (!xrefParser.EndOfData)
            {
                try
                {
                    fields = xrefParser.ReadFields();

                    // Skip first line header info
                    if (firstLine)
                    {
                        firstLine = false;
                        continue;
                    }
                    userId = fields[1];
                    lastName = fields[3];
                    formName = fields[9].Replace(" ", "");
                    appendLinesPath = fields[11];
                    sourceLinesPath = String.Format("{0}\\{1}_{2}\\{1}_{2}_{3}.pdf", _outputFolderPath, userId, lastName, formName);

                    appendFilePaths.Add(appendLinesPath, sourceLinesPath);
                }
                catch (Exception parser)
                {
                    Console.WriteLine(String.Format("Error parsing file at line {0}.\n Error message:\n {1}", xrefParser.ErrorLineNumber, parser.ToString()));
                }
            }
            return appendFilePaths;
        }

        /// <summary>
        /// Retrieves file paths of source and append files and calls method to process the files
        /// </summary>
        /// <param name="xrefFilePaths"></param>
        /// <param name="appendPdfFilePaths"></param>
        internal void ParsePdfsAndAppend(Dictionary<string, string> sourceAppendFilePaths)
        {
            string xrefFilePath;
            string appendFilePath;

            using (var progressBar = new ProgressBar(sourceAppendFilePaths.Count, "Action 2 of 2 - Update and Append Files...", new ProgressBarOptions { BackgroundColor = System.ConsoleColor.DarkGray }))

                foreach (KeyValuePair<string, string> filePath in sourceAppendFilePaths)
                {
                    appendFilePath = filePath.Key;
                    xrefFilePath = filePath.Value;
                    AppendToDocument(xrefFilePath, appendFilePath);
                    progressBar.Tick();
                }
        }

        /// <summary>
        /// Appends corresponding pdf to the end of the affected pdf docs.
        /// Note - Path must include the file name as well
        /// </summary>
        /// <param name="sourcePdfPath"></param>
        /// <param name="appendPdfPath"></param>
        internal void AppendToDocument(string sourcePdfPath, string appendPdfPath)
        {
            tempDirectory = String.Format("{0}_{1}", Path.GetPathRoot(_outputFolderPath), "_Temp");

            // Temp storage for files while create\append
            if (!Directory.Exists(tempDirectory))
            {
                Directory.CreateDirectory(tempDirectory);
            }
            string updatedFileName = Path.GetFileName(sourcePdfPath);
            string outputPdfPath = Path.Combine(tempDirectory, updatedFileName);

            try
            {
                using (var sourceDocumentStream1 = new FileStream(sourcePdfPath, FileMode.Open))
                {
                    using (var sourceDocumentStream2 = new FileStream(appendPdfPath, FileMode.Open))
                    {
                        using (var destinationDocumentStream = new FileStream(outputPdfPath, FileMode.Create))
                        {
                            var pdfConcat = new PdfConcatenate(destinationDocumentStream);
                            var pdfReader = new PdfReader(sourceDocumentStream1);

                            var pages = new List<int>();
                            for (int i = 0; i <= pdfReader.NumberOfPages; i++)
                            {
                                pages.Add(i);
                            }

                            pdfReader.SelectPages(pages);
                            pdfConcat.AddPages(pdfReader);

                            pdfReader = new PdfReader(sourceDocumentStream2);

                            pages = new List<int>();
                            for (int i = 0; i <= pdfReader.NumberOfPages; i++)
                            {
                                pages.Add(i);
                            }

                            pdfReader.SelectPages(pages);
                            pdfConcat.AddPages(pdfReader);

                            pdfReader.Close();
                            pdfConcat.Close();

                            File.Delete(sourcePdfPath);
                            File.Move(outputPdfPath, sourcePdfPath);
                            WriteToLogFile(updatedFileName, sourcePdfPath, "Complete");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                WriteToLogFile(updatedFileName, sourcePdfPath, e.ToString());
            }
        }

        /// <summary>
        /// Write processed file information to log
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="filePath"></param>
        /// <param name="message"></param>
        internal void WriteToLogFile(string fileName, string filePath, string message)
        {
            string logFileName = "FormExportLog.csv";
            string logFilePath = Path.Combine(_outputFolderPath, logFileName);
            string line;

            if (!File.Exists(logFilePath))
            {
                string logFileHeader = "FileName,FilePath,Message";
                File.WriteAllText(logFilePath, logFileHeader);
            }
            line = String.Format("\n{0},{1},{2}", fileName, filePath, message);
            File.AppendAllText(logFilePath, line);
        }

        /// <summary>
        /// Counts number of pdf files to be processed
        /// </summary>
        /// <param name="filesToBeAppended"></param>
        /// <returns>int - number of files</returns>
        private int CountFilesToUpdate(string[] filesToBeAppended)
        {
            return filesToBeAppended.Length;
        }
    }
}
