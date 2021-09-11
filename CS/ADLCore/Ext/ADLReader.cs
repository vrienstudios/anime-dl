using System;
using System.Collections.Generic;
using System.Text;

namespace ADLCore.Ext
{
    public class ADLReader
    {
        private ArchiveManager writer;
        private ArchiveManager reader;

        public void OpenADL(string path)
        {
            writer = new ArchiveManager();
            reader = new ArchiveManager();
            
            writer.InitWriteOnlyStream(path);
            reader.InitReadOnlyStream(path);
        }
    }
}
