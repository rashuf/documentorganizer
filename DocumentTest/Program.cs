using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocumentTest
{
    class Program
    {
        static void Main(string[] args)
        {
            System.Collections.Generic.List<string> list = new System.Collections.Generic.List<string>();
            foreach (string picture in System.IO.Directory.GetFiles(@"C:\Users\usmanov\Documents\Visual Studio 2013\Projects\DocumentOrganizer\DocumentTest\test", "*.*", System.IO.SearchOption.TopDirectoryOnly).Where(s => "*.jpg,*.jpe,*.jpeg,*.png,*.bmp".Contains(System.IO.Path.GetExtension(s).ToLower())))
            {
                try
                {
                    DocumentLib.ActionObject actionObject = new DocumentLib.ActionObject(picture);
                    actionObject.processFile();
                    string newFileName;
                    if (!System.String.IsNullOrEmpty(actionObject.QRstr))
                    {
                        newFileName = System.IO.Path.Combine(@"C:\Users\usmanov\Documents\Visual Studio 2013\Projects\DocumentOrganizer\DocumentTest\test\Recognized", actionObject.imageFile.Name);
                        Console.WriteLine(String.Format("{0} - {1}", picture, actionObject.QRstr));
                    }
                    else
                    {
                        newFileName = System.IO.Path.Combine(@"C:\Users\usmanov\Documents\Visual Studio 2013\Projects\DocumentOrganizer\DocumentTest\test\Unrecognized", actionObject.imageFile.Name);
                    }
                    //newFileName = newFileName.Replace(@"\\", @"\");
                    //System.IO.File.Move(actionObject.imageFile.FullName, newFileName);
                    System.IO.File.Move(actionObject.imageFile.FullName, newFileName);
                    ;
                }
                catch (Exception ex)
                {
                    string eStrt = ex.ToString();
                    continue;
                }                
            }
            Console.ReadKey();
        }
    }
}
