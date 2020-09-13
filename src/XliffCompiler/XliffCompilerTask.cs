using Microsoft.Build.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using XliffParser;

namespace XliffCompiler
{
    public class XliffCompilerTask : Task
    {
        public override bool Execute()
        {
            // This will be the parent project's directory
            var projectDir = Directory.GetCurrentDirectory();

            List<string> filesOutputted = new List<string>();

            try
            {
                foreach (var file in Directory.GetFiles(projectDir, "*.xlf", SearchOption.AllDirectories))
                {
                    var doc = new XlfDocument(file);
                    var xlfFile = doc.Files.Single();

                    // The two-char language code
                    string targetLanguageCode = Path.GetFileName(file).Split('.').Reverse().ElementAt(1);

                    // Original seems to include project directory in path, so we go up one level first, and then we need to get just the directory
                    string originalFile = Path.Combine(Directory.GetParent(projectDir).FullName, xlfFile.Original);
                    if (!File.Exists(originalFile))
                    {
                        Log.LogError("Original file not found at path: " + originalFile);
                        return false;
                    }

                    string outDir = Path.GetDirectoryName(originalFile);
                    string originalFileName = Path.GetFileName(originalFile);

                    string outFileName = Path.Combine(outDir, $"{Path.GetFileNameWithoutExtension(originalFileName)}.{targetLanguageCode}.resx");

                    doc.SaveAsResX(outFileName);
                    Log.LogMessage("Exported " + outFileName);
                    filesOutputted.Add(outFileName);
                }
            }
            catch (Exception ex)
            {
                Log.LogErrorFromException(ex);
                return false;
            }

            finally
            {
                //if (filesOutputted.Count > 0)
                //{
                //    File.WriteAllText("XliffCompilerWrittenFiles.txt", string.Join("\n", filesOutputted));
                //}
            }

            return true;
        }
    }
}
