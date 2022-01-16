using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DocumentService
{
    public enum IntervalUnit
    {
        milliseconds = 0,
        seconds = 1,
        minutes = 2,
        hours = 3
    }
    public class Settings
    {
        public string WorkingFolderWithFiles
        {
            get; internal set;
        }

        public string DocumentProcessingFileExe
        {
            get; internal set;
        }
        
        public IntervalUnit IntervalUnit
        {
            get; internal set;
        }

        public int Interval
        {
            get; internal set;
        }
    }
}
