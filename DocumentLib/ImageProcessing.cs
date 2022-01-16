using System;
using System.Runtime.InteropServices;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System.Drawing;
using System.Drawing.Imaging;

namespace DocumentLib
{
    internal class HoughLine
    {
        internal int Count { get; set; }
        internal int Index { get; set; }
        internal double Alpha { get; set; }
        internal double D { get; set; }
        internal HoughLine()
        {
            Count = 0; Index = 0; Alpha = 0.0; D = 0.0;
        }
    }
    public class ImageProcessing
    {
        const float angle_start = -45.0F;
        const float angle_step = 0.2F;

        const int top_lines_count = 20;

        const float bright_red_factor = 0.299F;
        const float bright_green_factor = 0.587F;
        const float bright_blue_factor = 0.114F;

        const int is_black_parameter = 140;

        const int step_count = 450;

        const int threshold = 140;

        int dmin = 0;

        OpenCvSharp.Mat inputImage;
        readonly OpenCvSharp.Mat originalImage;
        readonly string inputImageFileName;

        readonly int inputImageOrigWidth;
        readonly int inputImageOrigHeight;

        int cDcount;

        int matrixSize;

        double[] angleCos;
        double[] angleSin;

        int[] matrix;

        private float getAngle(int stepNum)
        {
            return angle_start + stepNum * angle_step;
        }

        public ImageProcessing(string pictureFileName)
        {
            originalImage = OpenCvSharp.Cv2.ImRead(pictureFileName);
            inputImage = OpenCvSharp.Cv2.ImRead(pictureFileName);
            inputImageFileName = pictureFileName;

            angleCos = new double[step_count];
            angleSin = new double[step_count];

            for (int stepNum = 0; stepNum < step_count; stepNum++)
            {
                double angle = this.getAngle(stepNum) * OpenCvSharp.Cv2.PI / 180.0;
                angleSin[stepNum] = System.Math.Sin(angle);
                angleCos[stepNum] = System.Math.Cos(angle);
            }
            inputImageOrigHeight = inputImage.Rows;
            inputImageOrigWidth = inputImage.Cols;

            cDcount = 2 * (inputImageOrigHeight + inputImageOrigWidth);

            matrixSize = cDcount * step_count;

            matrixSize = System.Math.Abs(matrixSize);

            matrix = new int[matrixSize];

            for (int i = 0; i < matrixSize; i++)
            {
                matrix[i] = 0;
            }
        }
        private HoughLine[] GetTop(int count)
        {
            HoughLine[] hl = new HoughLine[count];

            for (int i = 0; i < count; i++)
            {
                hl[i] = new HoughLine();
            }

            for (int i = 0; i < matrixSize; i++)
            {
                if (matrix[i] > hl[count - 1].Count)
                {
                    hl[count - 1].Count = matrix[i];
                    hl[count - 1].Index = i;

                    int j = count - 1;

                    while (j > 0 && hl[j].Count > hl[j - 1].Count)
                    {
                        HoughLine tmp = hl[j];
                        hl[j] = hl[j - 1];
                        hl[j - 1] = tmp;
                        j--;
                    }
                }
            }

            for (int i = 0; i < count; i++)
            {
                int dIndex = hl[i].Index / step_count;
                int alphaIndex = hl[i].Index - dIndex * step_count;

                hl[i].Alpha = angle_start + alphaIndex * angle_step;
                hl[i].D = dIndex + dmin;
            }

            return hl;
        }
        private bool IsBlackPix(int y, int x)
        {
            OpenCvSharp.Vec3b colour = inputImage.At<OpenCvSharp.Vec3b>(x, y);
            float bright = colour.Item0 * bright_blue_factor
                + colour.Item1 * bright_green_factor
                + colour.Item2 * bright_red_factor;

            return bright < is_black_parameter;
        }
        public float GetSkewAngle()
        {
            int heightPix = inputImage.Height;
            int widthPix = inputImage.Width;
            for (int y = heightPix / 4; y < heightPix * 3 / 4; y++)
            {
                for (int x = 1; x < widthPix - 1; x++)
                {
                    if (IsBlackPix(x, y) && !IsBlackPix(x, y + 1))
                    {
                        for (int alpha = 0; alpha < step_count; alpha++)
                        {
                            double d = y * angleCos[alpha] - x * angleSin[alpha];
                            int dIndex = (int)System.Math.Round(d - dmin);
                            int index = dIndex * step_count + alpha;
                            if (index > 0)
                            {
                                matrix[index]++;
                            }
                        }
                    }
                }
            }

            HoughLine[] hl = GetTop(top_lines_count);

            double sum = 0;

            for (int i = 0; i < top_lines_count; i++)
            {
                sum += hl[i].Alpha;
            }

            return (float)(sum / top_lines_count);
        }
        void Rotate(float angle)
        {
            // get rotation matrix for rotating the image around its center
            OpenCvSharp.Point2f center = new OpenCvSharp.Point2f(inputImage.Cols / 2.0F, inputImage.Rows / 2.0F);
            OpenCvSharp.Mat rot = OpenCvSharp.Cv2.GetRotationMatrix2D(center, angle, 1.0);
            // determine bounding rectangle
            OpenCvSharp.Rect bbox = new OpenCvSharp.RotatedRect(center, new OpenCvSharp.Size2f(inputImage.Width, inputImage.Height), angle).BoundingRect();
            // adjust transformation matrix
            rot.Set<double>(0, 2, rot.At<double>(0, 2) + bbox.Width / 2.0 - center.X);
            rot.Set<double>(1, 2, rot.At<double>(1, 2) + bbox.Height / 2.0 - center.Y);

            //OpenCvSharp.Mat dst = inputImage.Clone();

            OpenCvSharp.Cv2.WarpAffine(inputImage, inputImage, rot, bbox.Size, OpenCvSharp.InterpolationFlags.Linear, OpenCvSharp.BorderTypes.Replicate/*, 10*/); // cv::INTER_BITS2 = INTER_BITS * 2 = 5 * 2

            inputImage.SaveImage(inputImageFileName);
        }
        internal void Deskew()
        {
            float angle = GetSkewAngle();

            //if (angle > 0.7 || angle < -0.7)
            if (angle != 0)
            {
                Rotate(angle);
                CropToOriginalSize();
            }
        }
        public void Resize(double fx, double fy, Boolean saveResizedImage = false)
        {
            inputImage = originalImage.Resize(new OpenCvSharp.Size(), fx, fy);

            if (saveResizedImage)
            {
                inputImage.SaveImage(inputImageFileName);
            }
            inputImage.Dispose();
        }
        public void Resize(float percent, Boolean saveResizedImage = false)
        {
            int newWidth = (int)System.Math.Round(inputImage.Width * percent / 100);
            int newHeight = (int)System.Math.Round(inputImage.Height * percent / 100);
            OpenCvSharp.Size newSize = new OpenCvSharp.Size(newWidth, newHeight);            
            inputImage = inputImage.Resize(newSize);
            if (saveResizedImage)
            {
                inputImage.SaveImage(inputImageFileName);
            }
        }
        static internal Bitmap ChangeDpi(int dpi, Bitmap inputImage)
        {
            //изменение разрешения в той же картинке не работает, подтверждается ссылкой https://stackoverflow.com/a/4427207/8450286
            Bitmap newBitmap = (Bitmap)inputImage.Clone();
            newBitmap.SetResolution(dpi, dpi);
            return newBitmap;
            //newBitmap.Dispose();
        }
        public void CropToOriginalSize()
        {
            int width = inputImage.Width;
            int height = inputImage.Height;

            OpenCvSharp.Point2f center = new OpenCvSharp.Point2f((width - inputImageOrigWidth) / 2, (height - inputImageOrigHeight) / 2);

            Rect roi = new Rect(center, new OpenCvSharp.Size(inputImageOrigWidth, inputImageOrigHeight));
            inputImage = new Mat(inputImage, roi);
            inputImage.SaveImage(inputImageFileName);
        }
        static internal byte[] GetRGB_noWork(Bitmap image, int startX = 0, int startY = 0, int w = 0, int h = 0/*, int offset = 0, int scansize*/)
        //https://stackoverflow.com/questions/4747428/getting-rgb-array-from-image-in-c-sharp
        {
            const int PixelWidth = 3;
            const PixelFormat PixelFormat = PixelFormat.Format24bppRgb;

            // En garde!
            if (image == null)
            {
                throw new ArgumentNullException("image");
            }
            /*if (rgbArray == null)
            {
                throw new ArgumentNullException("rgbArray");
            }*/
            if (startX < 0 || startX + w > image.Width)
            {
                throw new ArgumentOutOfRangeException("startX");
            }
            if (startY < 0 || startY + h > image.Height)
            {
                throw new ArgumentOutOfRangeException("startY");
            }
            if (w != 0 && (w < 0 || w > image.Width))
            {
                throw new ArgumentOutOfRangeException("w");
            }
            else if (w == 0)
            {
                w = image.Width;
            }
            if (h != 0 && (h < 0 || h > image.Height))
            {
                throw new ArgumentOutOfRangeException("h");
            }
            else if (h == 0)
            {
                h = image.Height;
            }

            byte[] rgbArray = new byte[(w - startX) * (h - startY)];
             BitmapData data = image.LockBits(new Rectangle(startX, startY, w, h), System.Drawing.Imaging.ImageLockMode.ReadOnly, PixelFormat);
            try
            {
                byte[] pixelData = new Byte[data.Stride];
                for (int scanline = 0; scanline < data.Height; scanline++)
                {
                    Marshal.Copy(data.Scan0 + (scanline * data.Stride), pixelData, 0, data.Stride);
                    for (int pixeloffset = 0; pixeloffset < data.Width; pixeloffset++)
                    {
                        // PixelFormat.Format32bppRgb means the data is stored
                        // in memory as BGR. We want RGB, so we must do some 
                        // bit-shuffling.
                        rgbArray[scanline + pixeloffset] =
                            (byte)((pixelData[pixeloffset * PixelWidth + 2] << 16) +   // R 
                            (pixelData[pixeloffset * PixelWidth + 1] << 8) +    // G
                            pixelData[pixeloffset * PixelWidth]);                // B
                    }
                }
            }
            finally
            {
                image.UnlockBits(data);
            }
            return rgbArray;
        }
        static internal Bitmap BinarizeNet(Bitmap image, int threshold = threshold) // TODO: нужна проверка на 0 <= threshold <= 255
        {
            Bitmap result = new Bitmap(image.Width, image.Height);
            Color color = new Color();
            try
            {
                for (int j = 0; j < image.Height; j++)
                {
                    for (int i = 0; i < image.Width; i++)
                    {
                        color = image.GetPixel(i, j);
                        int K = ((color.R + color.G + color.B) / 3);
                        result.SetPixel(i, j, (K <= threshold ? Color.Black : Color.White));
                    }
                }
            }
            catch (Exception ex)
            {
                string exStr = ex.ToString();
            }
            return result;
        }
        static internal Bitmap BinarizeOpenCv(Bitmap image, int threshold = threshold) // TODO: нужна проверка на 0 <= threshold <= 255
        {
            Mat img = BitmapConverter.ToMat(image);
            Cv2.CvtColor(img, img, ColorConversionCodes.BGR2GRAY);

            Cv2.Threshold(img, img, 0, threshold, ThresholdTypes.Binary | ThresholdTypes.Otsu);

            Bitmap binarizedBitmap = BitmapConverter.ToBitmap(img);

            return binarizedBitmap;
        }
    }
}
