namespace XliffParser
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Design;
    using System.IO;
    using System.Linq;
    using System.Resources;
    using System.Text;
    using System.Threading.Tasks;
    using System.Xml.Linq;
    using fmdev.ResX;

    public enum XlfDialect
    {
        Standard = 0,
        RCWinTrans11 = 1,
        MultilingualAppToolkit = 2
    }

    public class XlfDocument
    {
        private const string AttributeOriginal = "original";
        private const string ElementFile = "file";
        private const string AttributeVersion = "version";
        private XDocument doc;

        public XlfDocument(string fileName)
        {
            FileName = fileName;
            doc = XDocument.Load(FileName);
            Dialect = DetermineDialect();
        }

        [Flags]
        public enum ResXSaveOption
        {
            None = 0,
            SortEntries = 1,
            IncludeComments = 2
        }

        public string FileName
        { get; }

        public IEnumerable<XlfFile> Files
        {
            get
            {
                var ns = this.doc.Root.Name.Namespace;
                return this.doc.Descendants(ns + ElementFile).Select(f => new XlfFile(f, ns));
            }
        }

        public string Version
        {
            get { return this.doc.Root.Attribute(AttributeVersion).Value; }
            set { this.doc.Root.SetAttributeValue(AttributeVersion, value); }
        }

        public XlfDialect Dialect
        {
            get; set;
        }

        public XlfFile AddFile(string original, string dataType, string sourceLang)
        {
            var ns = this.doc.Root.Name.Namespace;
            var f = new XElement(ns + ElementFile);
            this.doc.Descendants(ns + ElementFile).Last().AddAfterSelf(f);

            return new XlfFile(f, ns, original, dataType, sourceLang);
        }

        public void RemoveFile(string original)
        {
            var ns = this.doc.Root.Name.Namespace;
            this.doc.Descendants(ns + ElementFile).Where(u =>
            {
                var a = u.Attribute(AttributeOriginal);
                return a != null && a.Value == original;
            }).Remove();
        }

        public void Save()
        {
            this.doc.Save(this.FileName);
        }

        public void SaveAsResX(string fileName)
        {
            SaveAsResX(fileName, ResXSaveOption.None);
        }

        public void SaveAsResX(string fileName, ResXSaveOption options)
        {
            var entries = new List<ResXEntry>();
            foreach (var f in Files)
            {
                foreach (var u in f.TransUnits)
                {
                    var entry = new ResXEntry() { Id = u.GetId(Dialect), Value = u.Target };

                    if (options.HasFlag(ResXSaveOption.IncludeComments) && u.Optional.Notes.Count() > 0)
                    {
                        entry.Comment = u.Optional.Notes.First().Value;
                    }

                    entries.Add(entry);
                }
            }

            if (options.HasFlag(ResXSaveOption.SortEntries))
            {
                entries.Sort();
            }

            ResXFile.Write(fileName, entries, options.HasFlag(ResXSaveOption.IncludeComments) ? ResXFile.Option.None : ResXFile.Option.SkipComments);
        }

        private XlfDialect DetermineDialect()
        {
            if (this.Files.First().Optional.ToolId == "MultilingualAppToolkit")
            {
                return XlfDialect.MultilingualAppToolkit;
            }

            if (doc.Root.GetNamespaceOfPrefix("rwt") == "http://www.schaudin.com/xmlns/rwt11")
            {
                return XlfDialect.RCWinTrans11;
            }

            return XlfDialect.Standard;
        }
    }
}