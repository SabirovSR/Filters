﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApp1
{
    abstract class OtherFilters : Filters
    {
        protected float r1; // среднее по каналу R
        protected float g1; // среднее по каналу G
        protected float b1; // среднее по каналу B

        protected float maxR, minR;
        protected float maxG, minG;
        protected float maxB, minB;

        protected long brightness;

        public void GetAverageColor(Bitmap sourceImage)
        {
            Color color = sourceImage.GetPixel(0, 0);

            float resultR = 0;
            float resultG = 0;
            float resultB = 0;

            r1 = b1 = g1 = 0;

            for (int i = 0; i < sourceImage.Width; i++)
            {
                for (int j = 0; j < sourceImage.Height; j++)
                {
                    color = sourceImage.GetPixel(i, j);

                    resultR += color.R;
                    resultG += color.G;
                    resultB += color.B;
                }
            }

            r1 = resultR / sourceImage.Width * sourceImage.Height;
            g1 = resultG / sourceImage.Width * sourceImage.Height;
            b1 = resultB / sourceImage.Width * sourceImage.Height;
        }

        public void GetMaxColor(Bitmap sourceImage)
        {
            maxR = maxG = maxB = 0;

            for (int i = 0; i < sourceImage.Width; i++)
            {
                for (int j = 0; j < sourceImage.Height; j++)
                {
                    Color color = sourceImage.GetPixel(i, j);

                    maxR = Math.Max(maxR, color.R);
                    maxB = Math.Max(maxB, color.B);
                    maxG = Math.Max(maxG, color.G);
                }
            }
        }

        public void GetMinColor(Bitmap sourceImage)
        {
            minR = minG = minB = 255;

            for (int i = 0;i < sourceImage.Width; i++)
            {
                for(int j = 0; j < sourceImage.Height; j++)
                {
                    Color color = sourceImage.GetPixel(i, j);

                    minR = Math.Min(minR, color.R);
                    minG = Math.Min(minG, color.G);
                    minB = Math.Min(minB, color.B);
                }
            }
        }

        public void GetBrightness(Bitmap sourseImage)
        {
            brightness = 0;

            for (int i = 0; i < sourseImage.Width; i++)
            {
                for (int j = 0; j < sourseImage.Height; j++)
                {
                    long pix = 0;

                    Color color = sourseImage.GetPixel(i, j);

                    pix += color.R;
                    pix += color.G;
                    pix += color.B;
                    pix /= 3;

                    brightness += pix;
                }
            }

            brightness /= sourseImage.Width * sourseImage.Height;
        }
    }

    class GrayWorldFilter : OtherFilters // серый мир
    {
        protected float avg;

        public override Bitmap processImage(Bitmap sourceImage, BackgroundWorker worker)
        {
            Bitmap resultImage = new Bitmap(sourceImage.Width, sourceImage.Height);

            GetAverageColor(sourceImage);

            avg = (r1 + g1 + b1) / 3;

            for (int i = 0; i < sourceImage.Width; i++)
            {
                worker.ReportProgress((int)((float)i / resultImage.Width * 100));

                if (worker.CancellationPending)
                {
                    return null;
                }

                for (int j = 0; j < sourceImage.Height; j++)
                {
                    resultImage.SetPixel(i, j, calculateNewPixelColor(sourceImage, i, j));
                }
            }

            return resultImage;
        }

        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            Color color = sourceImage.GetPixel(x, y);

            float R = color.R * avg / r1;
            float G = color.G * avg / g1;
            float B = color.B * avg / b1;

            return Color.FromArgb(
                Clamp((int)R, 0, 255),
                Clamp((int)G, 0, 255),
                Clamp((int)B, 0, 255)
                );
        }
    }

    class LinearStretchingFilter : OtherFilters // линейное растяжение
    {
        public override Bitmap processImage(Bitmap sourceImage, BackgroundWorker worker)
        {
            Bitmap resultImage = new Bitmap(sourceImage.Width, sourceImage.Height);

            GetMaxColor(sourceImage); // поиск максимального и минимального значений интенсивности
            GetMinColor(sourceImage);

            for (int i = 0; i < sourceImage.Width; i++)
            {
                worker.ReportProgress((int)((float)i / resultImage.Width * (100 - 33 - 33)) + (33 + 33)); // масштабирования прогресса обработки изображения в рамках общего прогресса задачи.
                for (int j = 0; j < sourceImage.Height; j++)
                {
                    resultImage.SetPixel(i, j, calculateNewPixelColor(sourceImage, i, j));
                }
            }

            return resultImage;
        }

        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            Color pixel = sourceImage.GetPixel(x, y);

            float newR = (pixel.R - minR) * 255 / (maxR - minR);
            float newG = (pixel.G - minG) * 255 / (maxG - minG);
            float newB = (pixel.B - minB) * 255 / (maxB - minB);

            return Color.FromArgb(Clamp((int)newR, 0, 255),
                                  Clamp((int)newG, 0, 255),
                                  Clamp((int)newB, 0, 255));
        }
    } 

}
