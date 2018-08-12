using System;
using System.IO;

/*
 * 
 * Description: Tool will combine manifest files into one manifest file. 
 * Tool will append a referenced pdf to the end of an another
 * Date Created: 06/12/2018
 * 
 */

namespace FormExportFixerTool
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // Variables
            string inputFolderPath;
            string outputFolderPath;
            string xrefFilePath;

            // Start by getting input, output & manifest file locations from user
            do
            {
                Console.WriteLine("Enter parent folder path that contains the manifest files: ");
                inputFolderPath = Console.ReadLine();

                Console.WriteLine("\nEnter output path for the processed files to be copied to: ");
                outputFolderPath = Console.ReadLine();

                Console.WriteLine("\nEnter path to xref file: ");
                xrefFilePath = Console.ReadLine();

                // Validate we have legit paths\file. Else be a jerk and make them enter it all again
                if (Directory.Exists(inputFolderPath) && Directory.Exists(outputFolderPath) && File.Exists(xrefFilePath))
                {
                    break;
                }
                else
                {
                    Console.WriteLine("\nYou have entered an invalid folder andor file path. Please try again. \n");
                    inputFolderPath = null;
                    outputFolderPath = null;
                    xrefFilePath = null;
                }

            } while (true);

            Console.WriteLine("Processing Started....");

            // Combine manifest files into one
            Console.WriteLine("Action - Gather & combine manifest files....\n");
            ManifestFile manifestFile = new ManifestFile(inputFolderPath, outputFolderPath);

            string[] manifestFilePaths = manifestFile.GetManifestFiles(inputFolderPath);

            manifestFile.CombineManifestFiles(outputFolderPath, manifestFilePaths);

            Console.WriteLine("Complete - Combine manifest files\n");

            // Build the xref file paths & then append the cooresponding pages to the end
            Console.WriteLine("Action - Process Pdf files...\n");
            PdfFile pdf = new PdfFile(inputFolderPath, outputFolderPath, xrefFilePath);

            string[] xrefPaths = pdf.BuildXrefFilePath(xrefFilePath);
            string[] appendPaths = pdf.BuildAppendFilePath(xrefFilePath);

            pdf.ParsePdfsAndAppend(xrefPaths, appendPaths);

            Console.WriteLine("Complete - Updated Pdf files...");

            // Remove temp directory once processing is complete
            Directory.Delete(pdf.tempDirectory);

            //Message processing is done and close the console window
            Console.WriteLine("Processing Complete! \nCheck FormExportLog.csv for details or if any issues were encountered during processing...");

            Console.WriteLine("\nPress any key to close this console window...");
            Console.ReadKey();
        }
    }
}
