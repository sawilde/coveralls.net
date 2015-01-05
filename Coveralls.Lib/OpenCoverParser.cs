﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Coveralls.Lib
{
    public interface ICoverageParser
    {
        XDocument Report { get; set; }
        List<CoverageFile> Generate();
    }

    public class OpenCoverParser : ICoverageParser
    {
        private IFileSystem _fileSystem;
        public OpenCoverParser(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public XDocument Report { get; set; }

        public List<CoverageFile> Generate()
        {
            var files = new List<CoverageFile>();

            if (Report == null || Report.Root == null) return files;

            foreach (var module in Report.Root.XPathSelectElements("//Modules/Module"))
            {
                var skippedAttr = module.Attribute("skippedDueTo");
                if (skippedAttr != null && skippedAttr.Value.IsNotBlank()) continue;

                foreach (var file in module.XPathSelectElements("./Files/File"))
                {
                    var fileid = file.Attribute("uid").Value;
                    var fullPath = file.Attribute("fullPath").Value;

                    var coverageFile = new CoverageFile
                    {
                        Path = fullPath.ToUnixPath(),
                        Source = _fileSystem.ReadFileText(fullPath)
                    };

                    foreach (var @class in file.XPathSelectElements("./Classes/Class"))
                    {
                        foreach (var method in @class.XPathSelectElements("./Methods/Method"))
                        {
                            foreach(var sequencePoint in method.XPathSelectElements("./SequencePoints/SequencePoint"))
                            {
                                var sequenceFileId = sequencePoint.Attribute("fileid").Value;
                                if (fileid == sequenceFileId)
                                {
                                    var line = int.Parse(sequencePoint.Attribute("sl").Value);
                                    var visits = int.Parse(sequencePoint.Attribute("vc").Value);

                                    coverageFile.Record(line, visits);
                                }
                            }
                        }
                    }

                    files.Add(coverageFile);
                }
            }

            return files;
        }
    }
}
