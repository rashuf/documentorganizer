using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZXing;
using System.Drawing;

namespace DocumentLib
{
    enum QROrientation
    {
        normal = 0,
        rotatedLeft = 90,
        flippedOver = 180,
        rotatedRight = 270
    }

    public enum BarcodeType
    {
        Undefined = 0,
        QR,
        Code39
    }
    
    class QRProcessing
    {
        ZXing.Result result;
        ZXing.ResultPoint[] qrReferPoints;
        public QRProcessing(System.Drawing.Bitmap bitmap)
        {
            this.sourceImage = (System.Drawing.Bitmap)bitmap.Clone();
            //this.sourceImage = this.Binarize(this.sourceImage);
            //this.sourceImage.Save(@".\1.jpg");
            bitmap.Dispose();
            this.parseQR();
            sourceImage.Dispose();
        }       
        
        public QRProcessing(string imageFilePath) : this((System.Drawing.Bitmap)System.Drawing.Bitmap.FromFile(imageFilePath))
        {            
        }
        public System.Drawing.Bitmap sourceImage
        {
            get; set;
        }
        public bool qrParseResult
        {
            get;
            private set;
        }
        public string qrStr
        {
            get
            {                
                if (result != null)
                {
                    return result.Text;
                }
                else
                {
                    return "";
                }
            }
        }
        public BarcodeType CodeType
        {
            get; private set;
        }
        float getQRAngle()
        {
            float dx = 0.0F;
            float dy = 0.0F;
            if (CodeType == BarcodeType.QR)
            {
                dx = qrReferPoints[2].X - qrReferPoints[1].X;
                dy = qrReferPoints[2].Y - qrReferPoints[1].Y;
            }
            else if (CodeType == BarcodeType.Code39)
            {
                dx = qrReferPoints[1].X - qrReferPoints[0].X;
                dy = qrReferPoints[1].Y - qrReferPoints[0].Y;
            }

            if (dx == 0 || (dx > 0 && dx < 3) || (dx < 0 && dx > -3))
            {
                if (dy > 3)
                {
                    return 90;
                }
                else if (dy < -3)
                {
                    return -90;
                }
            }

            if ((dx > 3 && dy >= 0))
            {
                return (float)(System.Math.Atan(dy / dx) * 180 / System.Math.PI);
            }
            else if ((dx < 3 && dy <= 0))
            {
                return (float)(System.Math.Atan(dy / dx) * 180 / System.Math.PI - 180);
            }
            return 0;
        }
        QROrientation determineQROrientation()
        {
            QROrientation qrOrientation;
            float qrAngle = this.getQRAngle();

            if (qrAngle > 85 && qrAngle <= 95)
            {
                qrOrientation = QROrientation.rotatedRight;
            }
            else if (qrAngle < -85 && qrAngle >= -95)
            {
                qrOrientation = QROrientation.rotatedLeft;
            }
            else if ((qrAngle > 175 && qrAngle <= 185) || (qrAngle < -175 && qrAngle >= -185))
            {
                qrOrientation = QROrientation.flippedOver;
            }
            else
            {
                qrOrientation = QROrientation.normal;
            }

            return qrOrientation;
        }
        public static System.Drawing.RotateFlipType convertToRotateFlipType(QROrientation qrOrientation)
        {
            switch (qrOrientation)
            {
                case QROrientation.normal :
                    return RotateFlipType.RotateNoneFlipNone;
                case QROrientation.rotatedLeft :
                    return RotateFlipType.Rotate90FlipNone;
                case QROrientation.flippedOver :
                    return RotateFlipType.Rotate180FlipNone;
                case QROrientation.rotatedRight :
                    return RotateFlipType.Rotate270FlipNone;
                default :
                    throw new System.ArgumentException("Неизвестный тип аргумента");
            }
        }
        public QROrientation imageOrientation
        {
            get
            {
                if (result != null)
                {
                    return this.determineQROrientation();
                }
                else
                {
                    return QROrientation.normal;
                }
            }
        }
        private bool parseQR()
        {
            BitmapLuminanceSource source;
            
            /*source = new BitmapLuminanceSource(ImageProcessing.BinarizeOpenCv(this.sourceImage)); // используются черно-белые ("бинаризованные") изображения
            ZXing.Binarizer binarizer = new ZXing.Common.GlobalHistogramBinarizer(source);*/

            source = new BitmapLuminanceSource(this.sourceImage);
            ZXing.Common.HybridBinarizer binarizer = new ZXing.Common.HybridBinarizer(source);
            BinaryBitmap bitmap = new BinaryBitmap(binarizer);

            System.Collections.Generic.Dictionary<DecodeHintType, Object> hints = new System.Collections.Generic.Dictionary<DecodeHintType, Object>();
            hints.Add(DecodeHintType.TRY_HARDER, true);

            System.Collections.Generic.List<ZXing.BarcodeFormat> possibleFormats = new System.Collections.Generic.List<ZXing.BarcodeFormat>();
            possibleFormats.Add(ZXing.BarcodeFormat.QR_CODE);
            possibleFormats.Add(ZXing.BarcodeFormat.CODE_39);

            hints.Add(DecodeHintType.POSSIBLE_FORMATS, possibleFormats);

            result = new ZXing.MultiFormatReader().decode(bitmap, hints);
            /*ZXing.MultiFormatReader reader = new ZXing.MultiFormatReader();
            reader.Hints = hints;
            reader.decodeWithState(bitmap);*/

            if (result != null)
            {
                switch (result.BarcodeFormat)
                {
                    case BarcodeFormat.QR_CODE:
                        CodeType = BarcodeType.QR;
                        break;
                    case BarcodeFormat.CODE_39:
                        CodeType = BarcodeType.Code39;
                        break;
                    default:
                        CodeType = BarcodeType.Undefined;
                        break;
                }
                qrReferPoints = result.ResultPoints;
                this.qrParseResult = true;                
            }
            return this.qrParseResult;
        }
    }
}
