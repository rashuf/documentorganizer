using System;
using System.Collections.Generic;
using System.Linq;
using System.Configuration;
using System.IO;

namespace DocumentProcessing
{
    public class Handler
    {
        bool isListFilesFinished;
        private Logger logger;

        public Settings Settings
        {
            get; private set;
        }

        protected Handler() { }

        public Handler(string folderWithFiles)
        {
            WorkingFolderWithFiles = folderWithFiles;
            Init();
        }

        public bool SettingsIsRead { get; set; }

        public string WorkingFolderWithFiles { get; private set; }

        private bool Init()
        {
            if (!SettingsIsRead)
            {
                try
                {
                    Settings settings = new Settings();               
                    settings.OrigSubFolder = ConfigurationManager.AppSettings.Get("OrigSubFolder");
                    settings.RecognizedSubFolder = ConfigurationManager.AppSettings.Get("RecognizedSubFolder");
                    settings.UnrecognizedSubFolder = ConfigurationManager.AppSettings.Get("UnrecognizedSubFolder");
                    settings.TrashSubFolder = ConfigurationManager.AppSettings.Get("TrashSubFolder");
                    settings.StructsFileName = ConfigurationManager.AppSettings.Get("StructsFileName");
                    settings.SupportedFiles = ConfigurationManager.AppSettings.Get("SupportedFiles");
                    //settings.IsSaveResizedImage = System.Convert.ToBoolean(System.Convert.ToInt32(ConfigurationManager.AppSettings.Get("IsSaveResizedImage")));

                    string settingFileName = System.String.Format(@"{0}\{1}", System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), settings.StructsFileName);

                    if (System.String.IsNullOrEmpty(settings.StructsFileName)
                        || !File.Exists(settingFileName))
                    {
                        throw new MissingFieldException("Не найден файл настроек классификатора документов (параметр StructsFileName)");
                    }
                    settings.StructsFileName = settingFileName;
                    settings.InitDocumentStructs();
                    logger = new Logger(settings.DocumentStructs);
                    Settings = settings;
                    
                    if (CheckSettings())
                    {
                        CreateDirs();

                        SettingsIsRead = true;
                    }                    
                }
                catch
                {
                    SettingsIsRead = false;
                }
            }
            isListFilesFinished = true;
            return SettingsIsRead;
        }

        private bool CheckSettings()
        {
            if (string.IsNullOrEmpty(Settings.RecognizedSubFolder) || string.IsNullOrEmpty(Settings.UnrecognizedSubFolder))
            {
                throw new MissingFieldException("Проверьте настройки каталогов для распознанных и нераспознанных файлов (RecognizedSubFolder и UnrecognizedSubFolder)");
            }

            return true;
        }

        private void CreateDirs()
        {
            if (!string.IsNullOrEmpty(Settings.OrigSubFolder) && !Directory.Exists(Settings.OrigSubFolder))
            {
                Directory.CreateDirectory(Path.Combine(WorkingFolderWithFiles, Settings.OrigSubFolder));
            }

            if (!string.IsNullOrEmpty(Settings.TrashSubFolder) && !Directory.Exists(Settings.TrashSubFolder))
            {
                Directory.CreateDirectory(Path.Combine(WorkingFolderWithFiles, Settings.TrashSubFolder));
            }

            if (!Directory.Exists(Settings.RecognizedSubFolder))
            {
                Directory.CreateDirectory(Path.Combine(WorkingFolderWithFiles, Settings.RecognizedSubFolder));
            }

            if (!Directory.Exists(Settings.UnrecognizedSubFolder))
            {
                Directory.CreateDirectory(Path.Combine(WorkingFolderWithFiles, Settings.UnrecognizedSubFolder));
            }
        }

        public void ProcessFiles()
        {
            if (isListFilesFinished)
            {
                isListFilesFinished = false;
                IEnumerable<string> files;
                try
                {
                    files = Directory.GetFiles(WorkingFolderWithFiles, "*.*", SearchOption.TopDirectoryOnly).Where(s => Settings.SupportedFiles.Contains(Path.GetExtension(s).ToLower()));
                }
                catch (Exception ex)
                {
                    throw ex.InnerException;
                }

                foreach (string picture in files)
                {
                    try
                    {
                        DocumentLib.ActionObject actionObject = new DocumentLib.ActionObject(picture);
                        string fileInOrigFolder = Path.Combine(Settings.OrigSubFolder, actionObject.imageFile.Name);
                        if (Directory.Exists(Settings.OrigSubFolder) && !File.Exists(fileInOrigFolder))
                        {
                            File.Copy(picture, fileInOrigFolder);
                        }
                        actionObject.processFile();
                        string newFileName;
                        FileInfo xmlFile = logger.createXml(actionObject);
                        string xmlNewFileName;
                        if (!System.String.IsNullOrEmpty(actionObject.QRstr))
                        {
                            newFileName = Path.Combine(WorkingFolderWithFiles, Settings.RecognizedSubFolder, actionObject.imageFile.Name);
                            xmlNewFileName = Path.Combine(WorkingFolderWithFiles, Settings.RecognizedSubFolder, xmlFile.Name);
                        }
                        else
                        {
                            newFileName = Path.Combine(WorkingFolderWithFiles, Settings.UnrecognizedSubFolder, actionObject.imageFile.Name);
                            xmlNewFileName = Path.Combine(WorkingFolderWithFiles, Settings.UnrecognizedSubFolder, xmlFile.Name);
                        }
                        File.Copy(xmlFile.FullName, xmlNewFileName, true);
                        File.Delete(xmlFile.FullName);
                        File.Move(actionObject.imageFile.FullName, newFileName);
                    }
                    catch (Exception ex)
                    {
                        string eStrt = ex.ToString();
                        continue;
                    }
                }
                if (Directory.Exists(Path.Combine(WorkingFolderWithFiles, Settings.TrashSubFolder)))
                {
                    MoveUnsupportedFiles();
                }                

                isListFilesFinished = true;
            }
        }

        private void MoveUnsupportedFiles()
        {
            foreach (string unsupportedFile in Directory.GetFiles(WorkingFolderWithFiles, "*.*", SearchOption.TopDirectoryOnly).Where(s => !Settings.SupportedFiles.Contains(Path.GetExtension(s).ToLower())))
            {
                FileAttributes fileAttr = File.GetAttributes(unsupportedFile);
                if ((fileAttr & FileAttributes.System) == FileAttributes.System)
                {
                    continue;
                }
                try
                {
                    string newFileName = Path.Combine(WorkingFolderWithFiles, Settings.TrashSubFolder, Path.GetFileName(unsupportedFile));
                    File.Move(unsupportedFile, newFileName);
                }
                catch (Exception ex)
                {
                    string eStrt = ex.ToString();
                    continue;
                }
            }
        }
    }
}
