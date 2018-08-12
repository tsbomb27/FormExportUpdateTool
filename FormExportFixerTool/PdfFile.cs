using System;
using System.IO;
using System.Collections.Generic;
using iTextSharp.text.pdf;
using Microsoft.VisualBasic.FileIO;
using ShellProgressBar;

namespace FormExportFixerTool
{
    internal class PdfFile
    {
        // Class variables
        private readonly string _inputFolderPath;
        private readonly string _outputFolderPath;
        private readonly string _xrefFilePath;
        private int numberOfTicks;
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
        /// Method - Build the file paths for affected forms that need a pdf appended
        /// </summary>
        /// <returns></returns>
        internal string[] BuildXrefFilePath(string xrefFilePath)
        {
            string userId;
            string lastName;
            string formName;
            string line;
            List<string> xrefFilePaths = new List<string>();

            TextFieldParser xrefParser = new TextFieldParser(xrefFilePath);
            xrefParser.TextFieldType = FieldType.Delimited;
            xrefParser.SetDelimiters(",");

            bool firstLine = true;

            while (!xrefParser.EndOfData)
            {
                string[] fields = xrefParser.ReadFields();

                // Skip the first line header
                if (firstLine)
                {
                    firstLine = false;
                    continue;
                }
                userId = fields[1];
                lastName = fields[3];
                formName = fields[9].Replace(" ", "");

                // Build xref file path for files needing Pdf appended
                line = String.Format("{0}\\{1}_{2}\\{1}_{2}_{3}.pdf", _outputFolderPath, userId, lastName, formName);
                xrefFilePaths.Add(line);
            }
            return xrefFilePaths.ToArray();
        }

        /// <summary>
        /// Method - Store off paths for corresponding pdfs to be appended
        /// </summary>
        /// <param name="xrefFilePath"></param>
        /// <returns></returns>
        internal string[] BuildAppendFilePath(string xrefFilePath)
        {
            List<string> appendPdfFilePaths = new List<string>();

            TextFieldParser xrefParser = new TextFieldParser(xrefFilePath);
            xrefParser.TextFieldType = FieldType.Delimited;
            xrefParser.SetDelimiters(",");

            string line;
            bool firstLine = true;

            while (!xrefParser.EndOfData)
            {
                string[] fields = xrefParser.ReadFields();

                // Skip first line header
                if (firstLine)
                {
                    firstLine = false;
                    continue;
                }
                line = fields[11];
                appendPdfFilePaths.Add(line);
            }
            return appendPdfFilePaths.ToArray();
        }

        /// <summary>
        /// Method - Call methods to parse out xref paths & append pdf paths, then call to append pdf method
        /// </summary>
        /// <param name="xrefFilePaths"></param>
        /// <param name="appendPdfFilePaths"></param>
        internal void ParsePdfsAndAppend(string[] xrefFilePaths, string[] appendPdfFilePaths)
        {
            numberOfTicks = CountFilesToUpdate(appendPdfFilePaths);
            using (var progressBar = new ProgressBar(numberOfTicks, "Action 2 of 2 - Update and Append Pdf Files...", new ProgressBarOptions { BackgroundColor = System.ConsoleColor.DarkGray }))

                for (int i = 0; i < appendPdfFilePaths.Length; i++)
                {
                    string xrefFilePath = xrefFilePaths[i];
                    string appendPdfPath = appendPdfFilePaths[i];
                    AppendToDocument(xrefFilePath, appendPdfPath);
                    progressBar.Tick();
                }
        }

        /// <summary>
        /// Method - Appends corresponding pdf to the end of the affected pdf docs.
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
                            WriteToLogFile(updatedFileName, outputPdfPath, "Complete");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                WriteToLogFile(updatedFileName, outputPdfPath, e.ToString());
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
