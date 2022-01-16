using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ServiceProcess;
using System.Configuration.Install;
using System.Linq;
using System.Threading.Tasks;

namespace DocumentService
{
    [RunInstaller(true)]
    public partial class DocumentServiceInstaller : System.Configuration.Install.Installer
    {
        ServiceInstaller serviceInstaller;
        ServiceProcessInstaller processInstaller;
        public DocumentServiceInstaller()
        {
            InitializeComponent();
            serviceInstaller = new ServiceInstaller();
            processInstaller = new ServiceProcessInstaller();

            processInstaller.Account = ServiceAccount.NetworkService;
            serviceInstaller.StartType = ServiceStartMode.Manual;
            serviceInstaller.ServiceName = "DocumentService";
            serviceInstaller.DisplayName = "Обработка документов";
            serviceInstaller.Description = "Служба предназначена для предобработки файлов отсканированных изображений с помощью ZXing и OpenCV для последующего создания PDF-файла и его прикрепления в MS Dynamics Ax 2009.";
            Installers.Add(processInstaller);
            Installers.Add(serviceInstaller);
        }
    }
}
