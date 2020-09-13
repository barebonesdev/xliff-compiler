using fmdev.ResX;
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
            return Execute(Directory.GetCurrentDirectory());
        }

        public bool Execute(string projectDir)
        {
            List<string> filesOutputted = new List<string>();
            Dictionary<string, List<ResXEntry>> parsedSourceFiles = new Dictionary<string, List<ResXEntry>>();

            try
            {
                foreach (var file in Directory.GetFiles(projectDir, "*.xlf", SearchOption.AllDirectories))
                {
                    var doc = new XlfDocument(file);
                    var xlfFile = doc.Files.Single();

                    // The two-char language code
                    string targetLanguageCode = Path.GetFileName(file).Split('.').Reverse().ElementAt(1);

                    // Original seems to include project directory in path, so we go up one level first, and then we need to get just the directory
                    string originalFile = Path.Combine(Directory.GetParent(projectDir).FullName, xlfFile.Original.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar));
                    if (!File.Exists(originalFile))
                    {
                        Log.LogError("Original file not found at path: " + originalFile);
                        return false;
                    }

                    string outDir = Path.GetDirectoryName(originalFile);
                    string originalFileName = Path.GetFileName(originalFile);

                    string outFileName = Path.Combine(outDir, $"{Path.GetFileNameWithoutExtension(originalFileName)}.{targetLanguageCode}.resx");

                    // First update the XLF file
                    List<ResXEntry> parsedSourceFile;
                    if (!parsedSourceFiles.TryGetValue(originalFile, out parsedSourceFile))
                    {
                        try
                        {
                            parsedSourceFile = ResXFile.Read(originalFile);
                            parsedSourceFiles[originalFile] = parsedSourceFile;
                        }
                        catch (Exception ex)
                        {
                            Log.LogError("Invalid source file: " + originalFile + ". " + ex.ToString());
                            return false;
                        }
                    }

                    UpdateXlfFile(doc, parsedSourceFile);

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

        private void UpdateXlfFile(XlfDocument doc, List<ResXEntry> sourceFile)
        {
            var xlfFile = doc.Files.Single();
            Dictionary<string, XlfTransUnit> existingUnits = xlfFile.TransUnits.ToDictionary(i => i.Id, i => i);
            bool modified = false;

            foreach (var source in sourceFile)
            {
                if (existingUnits.TryGetValue(source.Id, out XlfTransUnit existing))
                {
                    // Exists, see if we need to update it
                    if (source.Value != existing.Source)
                    {
                        existing.Source = source.Value;
                        existing.TargetState = XlfTransUnit.TargetState_New;
                        modified = true;
                    }

                    // Remove it since we processed it
                    existingUnits.Remove(source.Id);
                }
                else
                {
                    // Doesn't exist, need to create it
                    xlfFile.AddTransUnit(source.Id, source.Value, source.Value, XlfFile.AddMode.DontCheckExisting, XlfDialect.MultilingualAppToolkit);
                    existingUnits.Remove(source.Id);
                    modified = true;
                }
            }

            if (existingUnits.Count > 0)
            {
                // Need to remove no-longer-used translations
                foreach (var existing in existingUnits.Values)
                {
                    existing.Remove();
                }

                modified = true;
            }

            if (modified)
            {
                doc.Save();
            }
        }
    }
}
