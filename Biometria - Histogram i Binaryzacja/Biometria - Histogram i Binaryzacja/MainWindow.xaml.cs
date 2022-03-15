using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Biometria___Histogram_i_Binaryzacja
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private Bitmap bmp;

        //
        private void BtnLoadFromFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                Uri fileUri = new Uri(openFileDialog.FileName);
                imgDynamic.Source = new BitmapImage(fileUri);
                bmp = new Bitmap(openFileDialog.FileName);
            }
        }

        private void BtnLoadFromResource_Click(object sender, RoutedEventArgs e)
        {
            Uri resourceUri = new Uri("Red_Apple.jpg", UriKind.Relative);
            imgDynamic.Source = new BitmapImage(resourceUri);
        }

        private void BtnSaveToFile_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "All supported graphics|*.jpg;*.jpeg;*.png|" +
                "JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg|" +
                "Portable Network Graphic (*.png)|*.png";
            if (saveFileDialog.ShowDialog() == true)
            {
                FileStream saveStream = new FileStream(saveFileDialog.FileName, FileMode.OpenOrCreate);
                BmpBitmapEncoder encoder = new BmpBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create((BitmapImage)imgDynamic.Source));
                //encoder.Frames.Add(BitmapFrame.Create(Image.Image));
                encoder.Save(saveStream);
                saveStream.Close();
            }
        }


        //HISTOGRAM
        public void Histogram_Click(object sender, RoutedEventArgs e)
        {
            if (bmp == null)
                return;

            var data = bmp.LockBits
            (
                new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height),
                System.Drawing.Imaging.ImageLockMode.ReadWrite,
                System.Drawing.Imaging.PixelFormat.Format24bppRgb
            );

            var bmpData = new byte[data.Stride * data.Height];

            //Przerzucanie z Bitmapy do Tablicy
            Marshal.Copy(data.Scan0, bmpData, 0, bmpData.Length);


            int[] histogram = new int[256];
            foreach (byte i in bmpData)
                ++histogram[i];

            double max = histogram.Max();
            for (int i = 0; i < histogram.Length; i++)
                histogram[i] = (int)(histogram[i] / max * 512.0);

            bmpData = new byte[bmpData.Length];
            for (int i = 0; i < bmpData.Length; i++)
                bmpData[i] = 255;

            for (int i = 0; i < histogram.Length; i++)
            {
                for (int j = 0; j < histogram[i]; j++)
                {
                    int index = i * 3 + (511 - j) * data.Stride;

                    bmpData[index + 0] =
                    bmpData[index + 1] =
                    bmpData[index + 2] = 0;
                }
            }

            //Przerzucanie z Tablicy do Bitmapy
            Marshal.Copy(bmpData, 0, data.Scan0, bmpData.Length);
            
            bmp.UnlockBits(data);
            var handle = bmp.GetHbitmap();
            imgDynamic.Source = Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
        }

        //BINARYZACJA
        public void BinaryThreshold_Click(object sender, RoutedEventArgs e)
        {
            if (bmp == null)
                return;

            byte threshold = 100;
            threshold = (byte)threshSlider.Value;
                //(int)Combo.SelectedItem;

            string n = Combo.SelectedItem?.ToString();
            

            var data = bmp.LockBits
            (
                new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height),
                System.Drawing.Imaging.ImageLockMode.ReadWrite,
                System.Drawing.Imaging.PixelFormat.Format24bppRgb
            );

            var bmpData = new byte[data.Stride * data.Height];

            //Przerzucenie z bitmpay do tablicy
            Marshal.Copy(data.Scan0, bmpData, 0, bmpData.Length);

            for (int i = 0; i < bmpData.Length; i += 3)
            {
                byte r = bmpData[i + 0];
                byte g = bmpData[i + 1];
                byte b = bmpData[i + 2];

                byte mean = (byte)((r + g + b) / 3);

                bmpData[i + 0] =
                bmpData[i + 1] =
                bmpData[i + 2] = mean > threshold
                    ? byte.MaxValue
                    : byte.MinValue;
            }

            //Przerzuci z tablicy do Bitmapy
            Marshal.Copy(bmpData, 0, data.Scan0, bmpData.Length);

            bmp.UnlockBits(data);
            var handle = bmp.GetHbitmap();
            imgDynamic.Source = Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
        }

    }
}
