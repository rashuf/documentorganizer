using System;
using System.Configuration;
using System.Data;
using System.Linq;
using System.ServiceProcess;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;

namespace DocumentService
{
    public partial class DocumentService : ServiceBase
    {
        private System.Timers.Timer timer;
        private bool settingsIsRead;
        private bool isListFilesFinished;
        private ProcessStartInfo documentProcessingExe;

        public Settings ServiceSettings
        {
            get; private set;
        }
        
        public DocumentService()
        {
            InitializeComponent();
            settingsIsRead = false;
        }
        System.Timers.Timer intervalTimer
        {
            get
            {
                if (timer == null)
                {
                    timer = new System.Timers.Timer();
                    timer.Enabled = true;
                    timer.Interval = this.CalcTimerInterval(ServiceSettings.IntervalUnit, ServiceSettings.Interval);
                    timer.AutoReset = true;
                    timer.Elapsed += new System.Timers.ElapsedEventHandler(this.ListFiles);
                }

                return timer;
            }
        }

        private void ListFiles(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (documentProcessingExe == null)
            {
                documentProcessingExe = new ProcessStartInfo(
                        Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), ServiceSettings.DocumentProcessingFileExe), ServiceSettings.WorkingFolderWithFiles);
            }
                
            if (isListFilesFinished)
            {
                isListFilesFinished = false;
                Process process = Process.Start(documentProcessingExe);
                process.WaitForExit();
                isListFilesFinished = true;
            }
        }


        int CalcTimerInterval(IntervalUnit unit, int period)
        {
            return this.ConvertInvervalUnitToMs(unit) * period;
        }

        private int ConvertInvervalUnitToMs(IntervalUnit unit)
        {
            int multiplier;
            switch (unit)
            {
                case IntervalUnit.milliseconds :
                    multiplier = 1;
                    break;
                case IntervalUnit.seconds :
                    multiplier = 1000;
                    break;
                case IntervalUnit.minutes :
                    multiplier = 60 * 1000;
                    break;
                case IntervalUnit.hours :
                    multiplier = 60 * 60 * 1000;
                    break;
                default :
                    throw new ArgumentException("Неизвестная единица времени", "unit");
            }
            return multiplier;
        }

        private bool ReadSettings()
        {
            if (!settingsIsRead)
            {
                try
                {
                    ServiceSettings = new Settings();
                    ServiceSettings.WorkingFolderWithFiles = ConfigurationManager.AppSettings.Get("WorkingFolderWithFiles");
                    ServiceSettings.DocumentProcessingFileExe = ConfigurationManager.AppSettings.Get("DocumentProcessingFileExe");
                    ServiceSettings.Interval = Convert.ToInt32(ConfigurationManager.AppSettings.Get("Interval"));
                    ServiceSettings.IntervalUnit = (IntervalUnit)(System.Convert.ToSByte(ConfigurationManager.AppSettings.Get("IntervalUnit")));
                }
                catch
                {
                    settingsIsRead = false;
                }
            }
            isListFilesFinished = true;
            return settingsIsRead;
        }

        protected override void OnStart(string[] args)
        {
            #if DEBUG
                System.Threading.Thread.Sleep(35000);
            #endif
            if (!this.ReadSettings())
            {
                OnStop();
            }
            timer = intervalTimer;
            timer.Start();
        }

        protected override void OnStop()
        {
        }
    }   
}
