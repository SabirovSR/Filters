using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;

namespace WindowsFormsApp1
{
    abstract class Filters
    {
        protected abstract Color calculateNewPixelColor(Bitmap sourceImage, int x, int y);

        public int Clamp(int value, int min, int max)
        {
            if (value < min)
            {
                return min;
            }

            if (value > max)
            {
                return max;
            }

            return value;
        }

        public virtual Bitmap processImage(Bitmap sourceImage, BackgroundWorker worker)
        {
            Bitmap resultImage = new Bitmap(sourceImage.Width, sourceImage.Height);

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
    }

    class SharpenBlurVignetteFilter2 : Filters
    {
        private double sharpnessStrength; // Сила резкости в центре
        private double blurStrength;      // Сила размытия на краях

        public SharpenBlurVignetteFilter2(double sharpnessStrength = 1.5, double blurStrength = 2.0)
        {
            this.sharpnessStrength = sharpnessStrength;
            this.blurStrength = blurStrength;
        }

        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            // Центр изображения
            int centerX = sourceImage.Width / 2;
            int centerY = sourceImage.Height / 2;

            // Максимальное расстояние от центра до угла
            double maxDistance = Math.Sqrt(centerX * centerX + centerY * centerY);

            // Расстояние от текущего пикселя до центра
            double distance = Math.Sqrt((x - centerX) * (x - centerX) + (y - centerY) * (y - centerY));

            // Коэффициент для смешивания резкости и размытия (0.0 - 1.0)
            double factor = 1.0 - (distance / maxDistance);

            // Применяем резкость или размытие в зависимости от расстояния
            Color resultColor;
            if (factor > 0.5)
            {
                // Ближе к центру — увеличиваем резкость
                resultColor = ApplySharpen(sourceImage, x, y, sharpnessStrength * factor);
            }
            else
            {
                // Дальше от центра — добавляем размытие
                resultColor = ApplyBlur(sourceImage, x, y, blurStrength * (1 - factor));
            }

            return resultColor;
        }

        // Метод для увеличения резкости
        private Color ApplySharpen(Bitmap sourceImage, int x, int y, double strength)
        {
            // Ядро резкости (лапласиан)
            double[,] kernel = {
            { -1, -1, -1 },
            { -1,  9, -1 },
            { -1, -1, -1 }
        };

            return ApplyKernel(sourceImage, x, y, kernel, strength);
        }

        // Метод для размытия
        private Color ApplyBlur(Bitmap sourceImage, int x, int y, double strength)
        {
            // Ядро размытия (гауссово размытие)
            double[,] kernel = {
            { 1, 2, 1 },
            { 2, 4, 2 },
            { 1, 2, 1 }
        };

            return ApplyKernel(sourceImage, x, y, kernel, strength);
        }

        // Общий метод для применения ядра (фильтра)
        private Color ApplyKernel(Bitmap sourceImage, int x, int y, double[,] kernel, double strength)
        {
            int kernelSize = kernel.GetLength(0);
            int radius = kernelSize / 2;

            double resultR = 0, resultG = 0, resultB = 0;

            // Применяем ядро к пикселю и его соседям
            for (int i = -radius; i <= radius; i++)
            {
                for (int j = -radius; j <= radius; j++)
                {
                    int neighborX = Clamp(x + j, 0, sourceImage.Width - 1);
                    int neighborY = Clamp(y + i, 0, sourceImage.Height - 1);

                    Color neighborColor = sourceImage.GetPixel(neighborX, neighborY);

                    double kernelValue = kernel[i + radius, j + radius];
                    resultR += neighborColor.R * kernelValue;
                    resultG += neighborColor.G * kernelValue;
                    resultB += neighborColor.B * kernelValue;
                }
            }

            // Нормализуем результат и применяем силу
            int newR = Clamp((int)(resultR / 16 * strength), 0, 255);
            int newG = Clamp((int)(resultG / 16 * strength), 0, 255);
            int newB = Clamp((int)(resultB / 16 * strength), 0, 255);

            return Color.FromArgb(newR, newG, newB);
        }

        // Метод для ограничения значения в диапазоне
        private int Clamp(int value, int min, int max)
        {
            return Math.Min(Math.Max(value, min), max);
        }
    }


    class SharpenBlurVignetteFilter : Filters
    {
        private SharpnessFilter sharpnessFilter;
        private BlurFilter blurFilter;

        public SharpenBlurVignetteFilter()
        {
            sharpnessFilter = new SharpnessFilter();
            blurFilter = new BlurFilter();
        }

        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            // Центр изображения
            int centerX = sourceImage.Width / 2;
            int centerY = sourceImage.Height / 2;

            // Максимальное расстояние от центра до угла
            double maxDistance = Math.Sqrt(centerX * centerX + centerY * centerY);

            // Расстояние от текущего пикселя до центра
            double distance = Math.Sqrt((x - centerX) * (x - centerX) + (y - centerY) * (y - centerY));

            // Коэффициент для смешивания резкости и размытия (0.0 - 1.0)
            double koeff = 1.0 - distance / maxDistance;

            // Применяем резкость или размытие в зависимости от расстояния
            Color resultColor;
            if (koeff > 0.5)
            {
                // Ближе к центру — увеличиваем резкость
                resultColor = sharpnessFilter.CalculateNewPixelColor(sourceImage, x, y);
            }
            else
            {
                // Дальше от центра — добавляем размытие
                resultColor = blurFilter.CalculateNewPixelColor(sourceImage, x, y);
            }

            return resultColor;
        }
    }

    // Сделать фильтр виньетка с плавной границей 
    class VignetteFilter : Filters
    {
        private bool darken; // Флаг для определения, затемнять или осветлять края

        public VignetteFilter(bool darken = true)
        {
            this.darken = darken;
        }

        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            Color sourceColor = sourceImage.GetPixel(x, y);

            // Центр изображения
            int centerX = sourceImage.Width / 2;
            int centerY = sourceImage.Height / 2;

            // Максимальное расстояние от центра до угла
            double maxDistance = Math.Sqrt(centerX * centerX + centerY * centerY);

            // Расстояние от текущего пикселя до центра
            double distance = Math.Sqrt((x - centerX) * (x - centerX) + (y - centerY) * (y - centerY));

            // Коэффициент затемнения/осветления (0.0 - 1.0)
            double factor = 1.0 - distance / maxDistance;

            int newR, newG, newB;
            // Затемнение
            if (darken)
            {
                newR = (int)(sourceColor.R * factor);
                newG = (int)(sourceColor.G * factor);
                newB = (int)(sourceColor.B * factor);
            }
            // Осветление
            else
            {
                newR = (int)(sourceColor.R + (255 - sourceColor.R) * (1 - factor));
                newG = (int)(sourceColor.G + (255 - sourceColor.G) * (1 - factor));
                newB = (int)(sourceColor.B + (255 - sourceColor.B) * (1 - factor));
            }

            newR = Clamp(newR, 0, 255);
            newG = Clamp(newG, 0, 255);
            newB = Clamp(newB, 0, 255);

            return Color.FromArgb(sourceColor.A, newR, newG, newB);
        }
    }

    class InvertFilter : Filters // инверсия
    {
        public InvertFilter() { }

        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            Color sourceColor = sourceImage.GetPixel(x, y);
            Color resultColor = Color.FromArgb(255 - sourceColor.R, 255 - sourceColor.G, 255 - sourceColor.B);
            return resultColor;
        }
    }

    class GrayScaleFilter : Filters // gray scale (серый)
    {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            Color sourceColor = sourceImage.GetPixel(x, y);

            int intensity = (int)((sourceColor.R * 0.36) + (sourceColor.G * 0.53) + (sourceColor.B * 0.11));

            Color resultColor = Color.FromArgb(intensity, intensity, intensity); // делает цвета по R, G, B соответственно

            return resultColor;
        }
    }

    class Sepiya : Filters // сепия
    {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            Color sourceColor = sourceImage.GetPixel(x, y);

            int intensity = (int)((sourceColor.R * 0.36) + (sourceColor.G * 0.53) + (sourceColor.B * 0.11));

            int resultR = (int)(intensity + 2 * 9);
            int resultB = (int)(intensity + 0.5 * 9);
            int resultG = (int)(intensity - 1 * 9);

            return Color.FromArgb(
                Clamp((int)resultR, 0, 255),
                Clamp((int)resultG, 0, 255),
                Clamp((int)resultB, 0, 255)
                );
        }
    }

    class LuminanceFilter : Filters // яркость
    {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            Color sourceColor = sourceImage.GetPixel(x, y);

            int resultR = sourceColor.R + 50;
            int resultG = sourceColor.G + 50;
            int resultB = sourceColor.B + 50;

            return Color.FromArgb(
                Clamp((int)resultR, 0, 255),
                Clamp((int)resultG, 0, 255),
                Clamp((int)resultB, 0, 255)
                );
        }
    }

    class MoveLeftFilter : Filters // сдвиг влево
    {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            if (x + 50 < sourceImage.Width)
            {
                return sourceImage.GetPixel(x + 50, y);
            }
            else { return Color.FromArgb(0, 0, 0); }
        }
    }

    class MoveRightFilter : Filters // сдвиг вправо
    {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            if (x - 50 >= 0)
            {
                return sourceImage.GetPixel(x - 50, y);
            }
            else { return Color.FromArgb(0, 0, 0); }
        }
    }

    class Rotate90DegreesFilter : Filters // поворот на 90 градусов
    {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            int x0 = sourceImage.Width / 2;
            int y0 = sourceImage.Height / 2; // x0, y0 - центр поворота

            double phi = Math.PI / 2; // угол поворота

            int new_x = (int)((x - x0) * Math.Cos(phi)) - (int)((y - y0) * Math.Sin(phi)) + x0;
            int new_y = (int)((x - x0) * Math.Sin(phi)) + (int)((y - y0) * Math.Cos(phi)) + y0;

            if (new_x >= 0 && new_x < sourceImage.Width && new_y >= 0 && new_y < sourceImage.Height)
            {
                return sourceImage.GetPixel(new_x, new_y);
            }
            else
            {
                return Color.FromArgb(0, 0, 0);

            }
        }
    }

    class Rotate180DegreesFilter : Filters // поворот на 180 градусов
    {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            int x0 = sourceImage.Width / 2;
            int y0 = sourceImage.Height / 2; // x0, y0 - центр поворота

            double phi = Math.PI; // угол поворота

            int new_x = (int)((x - x0) * Math.Cos(phi)) - (int)((y - y0) * Math.Sin(phi)) + x0;
            int new_y = (int)((x - x0) * Math.Sin(phi)) + (int)((y - y0) * Math.Cos(phi)) + y0;

            if (new_x >= 0 && new_x < sourceImage.Width && new_y >= 0 && new_y < sourceImage.Height)
            {
                return sourceImage.GetPixel(new_x, new_y);
            }
            else
            {
                return Color.FromArgb(0, 0, 0);
            }
        }
    }

    class WeakWaveFilter : Filters // волна по вертикале
    {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            int xX = x;
            int yY = y + (int)(20 * Math.Sin(2 * Math.PI * x / 60));

            xX = Clamp(xX, 0, sourceImage.Width - 1);
            yY = Clamp(yY, 0, sourceImage.Height - 1);

            return sourceImage.GetPixel(xX, yY);
        }
    }

    class StrongWaveFilter : Filters // волна по горизонтале
    {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            int xX = x + (int)(20 * Math.Sin(2 * Math.PI * y / 30));
            int yY = y;

            xX = Clamp(xX, 0, sourceImage.Width - 1);
            yY = Clamp(yY, 0, sourceImage.Height - 1);

            return sourceImage.GetPixel(xX, yY);
        }
    }

    class GlassEffectFilter : Filters // эффект стекла
    {
        protected Random rand = new Random();

        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            int newX = x + (int)((rand.NextDouble() - 0.5) * 10);
            int newY = y + (int)((rand.NextDouble() - 0.5) * 10);
            // .NextDouble - возвращает случайное число типа double, которое больше или равно 0.0 и меньше 1.0

            newX = Clamp(newX, 0, sourceImage.Width - 1);
            newY = Clamp(newY, 0, sourceImage.Height - 1);

            return sourceImage.GetPixel(newX, newY);
        }
    }


    abstract class MatrixFilter : Filters
    {
        protected float[,] kernel = null;
        protected MatrixFilter() { }
        public MatrixFilter(float[,] kernel)
        {
            this.kernel = kernel;
        }
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            int radiusX = kernel.GetLength(0) / 2;
            int radiusY = kernel.GetLength(1) / 2;

            float resultR = 0;
            float resultG = 0;
            float resultB = 0;

            for (int l = -radiusY; l <= radiusY; l++)
            {
                for (int k = -radiusX; k <= radiusX; k++)
                {
                    int idX = Clamp(x + k, 0, sourceImage.Width - 1);
                    int idY = Clamp(y + l, 0, sourceImage.Height - 1);

                    Color neighborColor = sourceImage.GetPixel(idX, idY);

                    resultR += neighborColor.R * kernel[k + radiusX, l + radiusY];
                    resultG += neighborColor.G * kernel[k + radiusX, l + radiusY];
                    resultB += neighborColor.B * kernel[k + radiusX, l + radiusY];
                }
            }

            return Color.FromArgb(
                Clamp((int)resultR, 0, 255),
                Clamp((int)resultG, 0, 255),
                Clamp((int)resultB, 0, 255)
                );
        }

    }


    class BlurFilter : MatrixFilter // размытие
    {
        public BlurFilter()
        {
            int sizeX = 3;
            int sizeY = 3;
            kernel = new float[sizeX, sizeY];
            for (int i = 0; i < sizeX; i++)
            {
                for (int j = 0; j < sizeY; j++)
                {
                    kernel[i, j] = 1.0f / (sizeX * sizeY);
                }
            }
        }

        public Color CalculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            return base.calculateNewPixelColor(sourceImage, x, y);
        }
    }

    class GaussianFilter : MatrixFilter // гаусс
    {
        public void createGaussianKarnel(int radius, float sigma)
        {
            int size = 2 * radius + 1; // определяем размер ядра

            kernel = new float[size, size]; // создаем ядро фильтра

            float norm = 0; // коэффицент нормировки ядра

            for (int i = -radius; i <= radius; i++) // рассчитываем ядро линейного фильтра
            {
                for (int j = -radius; j <= radius; j++)
                {
                    kernel[i + radius, j + radius] = (float)(Math.Exp(-(i * i + j * j) / (sigma * sigma)));
                    norm += kernel[i + radius, j + radius];
                }
            }

            for (int i = 0; i < size; i++)  // нормируем ядро
            {
                for (int j = 0; j < size; j++)
                {
                    kernel[i, j] /= norm;
                }
            }
        }
        public GaussianFilter()
        {
            createGaussianKarnel(3, 2);
        }
    }

    class MotionFilter : MatrixFilter // эффект движения
    {
        public MotionFilter()
        {
            int size = 5;
            kernel = new float[size, size];
            for (int i = 0; i < size; i++)
            {
                kernel[i, i] = 1.0f / (float)(size);
            }
        }
    }

    abstract class DoubleMatrixFilter : Filters
    {
        protected float[,] kernel1 = null;
        protected float[,] kernel2 = null;
        protected DoubleMatrixFilter() { }
        public DoubleMatrixFilter(float[,] kernel1, float[,] kernel2)
        {
            this.kernel1 = kernel1;
            this.kernel2 = kernel2;
        }
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            int radiusX = kernel1.GetLength(0) / 2;
            int radiusY = kernel1.GetLength(1) / 2;

            float resultR1 = 0;
            float resultG1 = 0;
            float resultB1 = 0;

            for (int i = -radiusY; i <= radiusY; i++)
            {
                for (int k = -radiusX; k <= radiusX; k++)
                {
                    int idX = Clamp(x + k, 0, sourceImage.Width - 1);
                    int idY = Clamp(y + i, 0, sourceImage.Height - 1);

                    Color neighbor = sourceImage.GetPixel(idX, idY);

                    resultR1 += neighbor.R * kernel1[k + radiusX, i + radiusY];
                    resultG1 += neighbor.G * kernel1[k + radiusX, i + radiusY];
                    resultB1 += neighbor.B * kernel1[k + radiusX, i + radiusY];
                }
            }

            int radiusX2 = kernel2.GetLength(0) / 2;
            int radiusY2 = kernel2.GetLength(1) / 2;

            float resultR2 = 0;
            float resultG2 = 0;
            float resultB2 = 0;

            for (int i = -radiusY2; i <= radiusY2; i++)
            {
                for (int k = -radiusX2; k <= radiusX2; k++)
                {
                    int idX = Clamp(x + k, 0, sourceImage.Width - 1);
                    int idY = Clamp(y + i, 0, sourceImage.Height - 1);

                    Color neighbor = sourceImage.GetPixel(idX, idY);

                    resultR2 += neighbor.R * kernel2[k + radiusX2, i + radiusY2];
                    resultG2 += neighbor.G * kernel2[k + radiusX2, i + radiusY2];
                    resultB2 += neighbor.B * kernel2[k + radiusX2, i + radiusY2];
                }
            }

            return Color.FromArgb(
            Clamp((int)Math.Sqrt(resultR1 * resultR1 + resultR2 * resultR2), 0, 255),
            Clamp((int)Math.Sqrt(resultG1 * resultG1 + resultG2 * resultG2), 0, 255),
            Clamp((int)Math.Sqrt(resultB1 * resultB1 + resultB2 * resultB2), 0, 255));
        }
    }

    class SobelFilter : DoubleMatrixFilter // собель
    {
        public SobelFilter()
        {
            kernel1 = new float[3, 3]
            {
                { -1, 0, 1 },
                { -2, 0, 2 },
                { -1, 0, 1 }
            };

            kernel2 = new float[3, 3]
            {
                { -1,-2,-1 },
                { 0, 0, 0 },
                { 1, 2, 1 }
            };
        }
    }

    class ScharraFilter : DoubleMatrixFilter // щарра
    {
        public ScharraFilter()
        {
            kernel1 = new float[3, 3]
            {
                {3, 0, -3 }, {10, 0, -10}, {3, 0, -3 }
            };

            kernel2 = new float[3, 3]
            {
                {3, 10, 3 }, {0, 0, 0}, {-3, -10, -3 }
            };
        }
    }

    class PruittaFilter : DoubleMatrixFilter
    { // прюитта
        public PruittaFilter()
        {
            kernel1 = new float[3, 3]
            {
                {-1, 0, 1 }, {-1, 0, 1}, {-1, 0, 1 }
            };

            kernel2 = new float[3, 3]
            {
                {-1, -1, -1 }, {0, 0, 0}, {1, 1, 1 }
            };
        }
    }

    class SharpnessFilter : MatrixFilter // резкость
    {
        public SharpnessFilter()
        {
            kernel = new float[3, 3]
                { {-1, -1, -1 }, {-1, 9, -1 }, {-1, -1, -1 } };
        }

        public Color CalculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            return base.calculateNewPixelColor(sourceImage, x, y);
        }
    }

    class EmbossingFilter : MatrixFilter // тиснение
    {

        public EmbossingFilter()
        {
            kernel = new float[3, 3]
            {
                {0, 1, 0 }, {1, 0, -1 }, {0, -1, 0}
            };
        }

        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            int radiusX = kernel.GetLength(0) / 2;
            int radiusY = kernel.GetLength(1) / 2;

            float resultR = 128; // делает фото серым
            float resultG = 128;
            float resultB = 128;

            for (int l = -radiusX; l <= radiusX; l++)
            {
                for (int k = -radiusY; k <= radiusY; k++)
                {
                    int idX = Clamp(x + k, 0, sourceImage.Width - 1);
                    int idY = Clamp(y + l, 0, sourceImage.Height - 1);

                    Color neighbourColor = sourceImage.GetPixel(idX, idY);

                    resultR += neighbourColor.R * kernel[k + radiusX, l + radiusY];
                    resultG += neighbourColor.G * kernel[k + radiusX, l + radiusY];
                    resultB += neighbourColor.B * kernel[k + radiusX, l + radiusY];
                }
            }
            return Color.FromArgb(
               Clamp((int)resultR, 0, 255),
               Clamp((int)resultG, 0, 255),
               Clamp((int)resultB, 0, 255)
               );
        }
    }

    class MedianFilter : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            List<int> AllR = new List<int>();
            List<int> AllG = new List<int>();
            List<int> AllB = new List<int>();

            int radiusX = 1;
            int radiusY = 1;

            for (int k = -radiusX; k <= radiusX; k++)
            {
                for (int l = -radiusY; l <= radiusY; l++)
                {
                    int xX = Clamp(x + k, 0, sourceImage.Width - 1);
                    int yY = Clamp(y + l, 0, sourceImage.Height - 1);

                    Color color = sourceImage.GetPixel(xX, yY);

                    AllR.Add(color.R);
                    AllG.Add(color.G);
                    AllB.Add(color.B);
                }
            }

            AllR.Sort();
            AllG.Sort();
            AllB.Sort();

            return Color.FromArgb(AllR[AllR.Count() / 2], AllG[AllG.Count() / 2], AllB[AllB.Count() / 2]);
        }
    }
}
