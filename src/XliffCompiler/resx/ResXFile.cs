// <copyright file="ResXFile.cs" company="Florian Mücke">
// Copyright (c) Florian Mücke. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace fmdev.ResX
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Xml;

    public static class ResXFile
    {
        [Flags]
        public enum Option
        {
            None = 0,
            SkipComments = 1
        }

        public static void Write(string filename, IEnumerable<ResXEntry> entries, Option options = Option.None)
        {
            string str;

            using (var memoryStream = new MemoryStream())
            {
                using (var streamWriter = new StreamWriter(memoryStream, Encoding.UTF8))
                {
                    using (var writer = XmlWriter.Create(streamWriter))
                    {
                        writer.WriteStartDocument();

                        writer.WriteComment("Auto-generated document. Do not modify. Modify the xlf files instead.");

                        writer.WriteStartElement("root");
                        {
                            writer.WriteStartElement("resheader");
                            {
                                writer.WriteAttributeString("name", "resmimetype");
                                writer.WriteElementString("value", "text/microsoft-resx");
                            }
                            writer.WriteEndElement();

                            writer.WriteStartElement("resheader");
                            {
                                writer.WriteAttributeString("name", "version");
                                writer.WriteElementString("value", "2.0");
                            }
                            writer.WriteEndElement();

                            writer.WriteStartElement("resheader");
                            {
                                writer.WriteAttributeString("name", "reader");
                                writer.WriteElementString("value", "System.Resources.ResXResourceReader, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
                            }
                            writer.WriteEndElement();

                            writer.WriteStartElement("resheader");
                            {
                                writer.WriteAttributeString("name", "writer");
                                writer.WriteElementString("value", "System.Resources.ResXResourceWriter, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
                            }
                            writer.WriteEndElement();

                            foreach (var entry in entries)
                            {
                                writer.WriteStartElement("data");
                                {
                                    writer.WriteAttributeString("name", entry.Id);
                                    writer.WriteAttributeString("xml", "space", null, "preserve");
                                    writer.WriteElementString("value", entry.Value);
                                }
                                writer.WriteEndElement();
                            }
                        }
                        writer.WriteEndElement();
                        writer.WriteEndDocument();

                        writer.Flush();


                        memoryStream.Position = 0;

                        using (var reader = new StreamReader(memoryStream))
                        {
                            str = reader.ReadToEnd();
                        }
                    }
                }
            }

            // Only write if actually updated
            if (!File.Exists(filename) || File.ReadAllText(filename) != str)
            {
                File.WriteAllText(filename, str);
            }
        }

        /// <summary>
        /// Generates a public C# designer class.
        /// </summary>
        /// <param name="resXFile">The source resx file.</param>
        /// <param name="className">The base class name.</param>
        /// <param name="namespaceName">The namespace for the generated code.</param>
        /// <returns>false if generation of at least one property couldn't be generated.</returns>
        //public static bool GenerateDesignerFile(string resXFile, string className, string namespaceName)
        //{
        //    return GenerateDesignerFile(resXFile, className, namespaceName, false);
        //}

        /// <summary>
        /// Generates a C# designer class.
        /// </summary>
        /// <param name="resXFile">The source resx file.</param>
        /// <param name="className">The base class name.</param>
        /// <param name="namespaceName">The namespace for the generated code.</param>
        /// <param name="internalClass">Specifies if the class has internal or public access level.</param>
        /// <returns>false if generation of at least one property failed.</returns>
        //public static bool GenerateDesignerFile(string resXFile, string className, string namespaceName, bool internalClass)
        //{
        //    if (!File.Exists(resXFile))
        //    {
        //        throw new FileNotFoundException($"The file '{resXFile}' could not be found");
        //    }

        //    if (string.IsNullOrEmpty(className))
        //    {
        //        throw new ArgumentException($"The class name must not be empty or null");
        //    }

        //    if (string.IsNullOrEmpty(namespaceName))
        //    {
        //        throw new ArgumentException($"The namespace name must not be empty or null");
        //    }

        //    string[] unmatchedElements;
        //    var codeProvider = new Microsoft.CSharp.CSharpCodeProvider();
        //    System.CodeDom.CodeCompileUnit code =
        //        System.Resources.Tools.StronglyTypedResourceBuilder.Create(
        //            resXFile,
        //            className,
        //            namespaceName,
        //            codeProvider,
        //            internalClass,
        //            out unmatchedElements);

        //    var designerFileName = Path.Combine(Path.GetDirectoryName(resXFile), $"{className}.Designer.cs");
        //    using (StreamWriter writer = new StreamWriter(designerFileName, false, System.Text.Encoding.UTF8))
        //    {
        //        codeProvider.GenerateCodeFromCompileUnit(code, writer, new System.CodeDom.Compiler.CodeGeneratorOptions());
        //    }

        //    return unmatchedElements.Length == 0;
        //}
    }
}