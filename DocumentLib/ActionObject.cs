using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocumentLib
{
    public class ActionObject
    {
        public ActionObject(string imageFilePath)
        {
            this.imageFile = new System.IO.FileInfo(imageFilePath);
        }
        public System.IO.FileInfo imageFile
        {
            get;
            private set;
        }
        public string QRstr
        {
            get; private set;
        }
        public BarcodeType CodeType
        {
            get; private set;
        }
        void rotateImage(System.IO.FileInfo imageFile, QROrientation imageOrientation)
        {
            try
            {
                if (imageOrientation != QROrientation.normal)
                {
                    System.Drawing.Image image = System.Drawing.Image.FromFile(imageFile.FullName);
                    image.RotateFlip(QRProcessing.convertToRotateFlipType(imageOrientation));
                    image.Save(imageFile.FullName);
                    image.Dispose();
                }
            }
            catch (Exception e)
            {
                string eStr = e.ToString();
            }
        }
        public void processFile(Boolean isSaveResizedImage = false)
        {
            try
            {
                ImageProcessing imageProcessing = new ImageProcessing(this.imageFile.FullName);
                imageProcessing.Deskew();
                //imageProcessing.Resize(50, isSaveResizedImage);
                QRProcessing qrProcessing = null;
                #region withChangeDpi - commented
                /*
                int[] dpiArray = {150, 200, 250, 300};
                foreach (int dpi in dpiArray)
                {
                    System.Drawing.Bitmap origImage = new System.Drawing.Bitmap(this.imageFile.FullName);
                    System.Drawing.Bitmap newImage = ImageProcessing.changeDpi(dpi, origImage);
                    origImage.Dispose();
                    newImage.Save(String.Format("{0}{1}{2}.jpg", @"C:\Users\usmanov\Documents\Visual Studio 2013\Projects\DocumentOrganizer\DocumentTest\test\ChangeResolution\", this.imageFile.Name, dpi));
                    qrProcessing = new QRProcessing(newImage);

                    if (!System.String.IsNullOrEmpty(qrProcessing.qrStr))
                    {
                        this.QRstr = qrProcessing.qrStr;
                        newImage.Save(this.imageFile.FullName);
                        newImage.Dispose();
                        break;
                    }
                    newImage.Dispose();
                }
                */
                #endregion

                #region withResize - commented
                /*
                int[] sizePercentArray = { 25, 50, 75, 100 };
                forech (int sizePercent in sizePercentArray)
                {
                    if (sizePercent != 100)
                    {
                        imageProcessing.Resize(sizePercent, true); // TODO: isSaveResizedImage
                    }

                    qrProcessing = new QRProcessing(this.imageFile.FullName);
                    this.QRstr = qrProcessing.qrStr;
                    if (this.QRstr != "")
                    {
                        break;
                    }
                }
                */
                #endregion

                #region withRotate
                int[] rotateDegreeArray = { 0, 90, 180, 270 };
                foreach (int rotateDegree in rotateDegreeArray)
                {
                    System.Drawing.Bitmap inputImage = new System.Drawing.Bitmap(this.imageFile.FullName);
                    if (rotateDegree != 0)
                    {
                        inputImage.RotateFlip(QRProcessing.convertToRotateFlipType((QROrientation)rotateDegree));
                    }
                    qrProcessing = new QRProcessing(inputImage);
                    this.QRstr = qrProcessing.qrStr;
                    this.CodeType = qrProcessing.CodeType;
                    if (this.QRstr != "")
                    {
                        if (rotateDegree != 0)
                        {
                            System.Drawing.Bitmap rotatedImage = new System.Drawing.Bitmap(this.imageFile.FullName);
                            rotatedImage.RotateFlip(QRProcessing.convertToRotateFlipType((QROrientation)rotateDegree));
                            rotatedImage.Save(this.imageFile.FullName);
                        }
                        break;
                    }
                    inputImage.Dispose();
                }
                #endregion
                if (!System.String.IsNullOrEmpty(this.QRstr))
                {
                    this.rotateImage(this.imageFile, qrProcessing.imageOrientation);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"При обработке файла {imageFile.Name} возникла ошибка {ex.Message}");
                return;
            }            
        }
    }
}
