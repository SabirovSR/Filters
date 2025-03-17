using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        Bitmap image;
        public Form1()
        {
            InitializeComponent();
        }

        Bitmap[] historyImages = new Bitmap[sizeHistoryImage];

        static int sizeHistoryImage = 5;
        static int currentIndexHistoryImage = -1;

        protected void newHistoryImage(Bitmap newImage)
        {
            if (currentIndexHistoryImage + 1 >= 5)
            {
                for (int i = 0; i < historyImages.Length - 1; i++)
                {
                    historyImages[i] = historyImages[i + 1];
                }

                historyImages[historyImages.Length - 1] = newImage;
                return;
            }

            currentIndexHistoryImage++;
            historyImages[currentIndexHistoryImage] = newImage;
        }

        private void открытьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Image files|*.png;*.jpg;*.bmp|All files)*.*|*.*";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                image = new Bitmap(dialog.FileName);
            }
            pictureBox1.Image = image;
            pictureBox1.Refresh();

            newHistoryImage(image);
        }

        private void инверсияToolStripMenuItem_Click(object sender, EventArgs e)
        {
            InvertFilter filter = new InvertFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            Bitmap newImage = ((Filters)e.Argument).processImage(image, backgroundWorker1);
            if (backgroundWorker1.CancellationPending != true)
            {
                image = newImage;
            }
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar1.Value = e.ProgressPercentage;
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                pictureBox1.Image = image;
                pictureBox1.Refresh();

                newHistoryImage(image);
            }
            progressBar1.Value = 0;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            backgroundWorker1.CancelAsync();
        }

        private void размытиеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new BlurFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void гауссToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new GaussianFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void grayScaleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new GrayScaleFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void сепияToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new Sepiya();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void яркостьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new LuminanceFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void собельToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new SobelFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void резкостьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new SharpnessFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void тиснениеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new EmbossingFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void сдвигВправоToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filters = new MoveRightFilter();
            backgroundWorker1.RunWorkerAsync(filters);
        }

        private void сдвигВлевоToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filters = new MoveLeftFilter();
            backgroundWorker1.RunWorkerAsync(filters);
        }

        private void поворот90ГрадусовToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new Rotate90DegreesFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void поворот180ГрадусовToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new Rotate180DegreesFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void вертикальнаяВолнаToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new WeakWaveFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void горизонтальнаяВолнаToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new StrongWaveFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void стеклоToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new GlassEffectFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void движениеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new MotionFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void щарраToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new ScharraFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void прюиттаToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new PruittaFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void медианныйToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new MedianFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void серыйМирToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OtherFilters filter = new GrayWorldFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void линейноеРастяжениеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OtherFilters filter = new LinearStretchingFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }


        //=MathMorphols=
        private void расширениеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Choice choice = new Choice();
            choice.ShowDialog();
            bool answer = choice.Answer;

            if (answer == false)
            {
                Filters filter = new DilationFilter();
                backgroundWorker1.RunWorkerAsync(filter);
            }
            else
            {
                NewStruct newStruct = new NewStruct();
                newStruct.ShowDialog();

                Filters filter = new DilationFilter();
                backgroundWorker1.RunWorkerAsync(filter);
            }
        }

        private void сужениеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Choice choice = new Choice();
            choice.ShowDialog();
            bool answer = choice.Answer;

            if (answer == false)
            {
                Filters filter = new ErosionFilter();
                backgroundWorker1.RunWorkerAsync(filter);
            }
            else
            {
                NewStruct newStruct = new NewStruct();
                newStruct.ShowDialog();

                Filters filter = new ErosionFilter();
                backgroundWorker1.RunWorkerAsync(filter);
            }
        }

        private void topHatToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Choice choice = new Choice();
            choice.ShowDialog();
            bool answer = choice.Answer;

            if (answer == false)
            {
                Filters filter = new TopHatFilter();
                backgroundWorker1.RunWorkerAsync(filter);
            }
            else
            {
                NewStruct newStruct = new NewStruct();
                newStruct.ShowDialog();

                Filters filter = new TopHatFilter();
                backgroundWorker1.RunWorkerAsync(filter);
            }
        }

        private void blackHatToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Choice choice = new Choice();
            choice.ShowDialog();
            bool answer = choice.Answer;

            if (answer == false)
            {
                Filters filter = new BlackHatFilter();
                backgroundWorker1.RunWorkerAsync(filter);
            }
            else
            {
                NewStruct newStruct = new NewStruct();
                newStruct.ShowDialog();

                Filters filter = new BlackHatFilter();
                backgroundWorker1.RunWorkerAsync(filter);
            }
        }

        private void gradToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Choice choice = new Choice();
            choice.ShowDialog();
            bool answer = choice.Answer;

            if (answer == false)
            {
                Filters filter = new GradFilter();
                backgroundWorker1.RunWorkerAsync(filter);
            }
            else
            {
                NewStruct newStruct = new NewStruct();
                newStruct.ShowDialog();

                Filters filter = new GradFilter();
                backgroundWorker1.RunWorkerAsync(filter);
            }
        }

        private void открытиеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Choice choice = new Choice();
            choice.ShowDialog();
            bool answer = choice.Answer;

            if (answer == false)
            {
                Filters filter = new OpeningFilter();
                backgroundWorker1.RunWorkerAsync(filter);
            }
            else
            {
                NewStruct newStruct = new NewStruct();
                newStruct.ShowDialog();

                Filters filter = new OpeningFilter();
                backgroundWorker1.RunWorkerAsync(filter);
            }
        }

        private void закрытиеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Choice choice = new Choice();
            choice.ShowDialog();
            bool answer = choice.Answer;

            if (answer == false)
            {
                Filters filter = new ClosingFilter();
                backgroundWorker1.RunWorkerAsync(filter);
            }
            else
            {
                NewStruct newStruct = new NewStruct();
                newStruct.ShowDialog();

                Filters filter = new ClosingFilter();
                backgroundWorker1.RunWorkerAsync(filter);
            }
        }

        private void темнаяToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new VignetteFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void светлаяToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new VignetteFilter(false);
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void глазОкаToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new SharpenBlurVignetteFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void глазОка2ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new SharpenBlurVignetteFilter2();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void сохранитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog SaveDialog = new SaveFileDialog();

            SaveDialog.Filter = "Image files | *.png; *.jpg; *.bmp; | All Files (*.*) | *.*";

            if (SaveDialog.ShowDialog() == DialogResult.OK)
            {
                pictureBox1.Image.Save(SaveDialog.FileName, System.Drawing.Imaging.ImageFormat.Jpeg);
            }
        }

        private void назадToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (currentIndexHistoryImage - 1 < 0) throw new Exception("Вы в начале истории изображений");

                pictureBox1.Image = historyImages[currentIndexHistoryImage - 1];
                currentIndexHistoryImage--;

                image = historyImages[currentIndexHistoryImage];

                pictureBox1.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void впередToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (currentIndexHistoryImage >= historyImages.Length - 1) throw new Exception("Вы на последнем изображении");
                pictureBox1.Image = historyImages[currentIndexHistoryImage + 1];
                currentIndexHistoryImage++;

                image = historyImages[currentIndexHistoryImage];

                pictureBox1.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
