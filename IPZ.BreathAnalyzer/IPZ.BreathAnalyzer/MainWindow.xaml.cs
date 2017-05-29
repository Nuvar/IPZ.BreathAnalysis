using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using NAudio.Dsp;
using NAudio.Wave;
using OxyPlot;
using WaveletLogic;
using Point = System.Windows.Point;
using Size = System.Windows.Size;

namespace IPZ.BreathAnalyzer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        double fr = 1.0 / 44000;
        double ms = 0.001;
        private int step = 44;
        private int duration = 0;
        private short[] signal = new short[0];
        private Wavelet test = new WaveletLogic.Wavelet();
        private SurfacePlotModel myPlot = new SurfacePlotModel { ShowMiniCoordinates = true };
        public MainWindow()
        {
            InitializeComponent();
            wavelet.DataContext = myPlot;
        }

        private void OnFileLoadClick(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.InitialDirectory = System.AppDomain.CurrentDomain.BaseDirectory; // path to exe
            dialog.Filter = "WAV files (*.wav)|*.wav";
            dialog.Multiselect = false;
            dialog.CheckFileExists = true;
            dialog.CheckPathExists = true;
            if (dialog.ShowDialog() == true)
            {
                LoadSample(dialog.FileName);
                fileTitle.Text = System.IO.Path.GetFileName(dialog.FileName);
                waveletBox.IsEnabled = true;
            }
        }

        private void LoadSample(string path)
        {
            using (WaveFileReader reader = new WaveFileReader(path))
            {
                byte[] buffer = new byte[reader.Length];
                int read = reader.Read(buffer, 0, buffer.Length);
                signal = new short[read / 2];
                Buffer.BlockCopy(buffer, 0, signal, 0, read);
                duration = (int)reader.TotalTime.TotalMilliseconds;
                var points = new List<DataPoint>();

                for (int i = 0; i < reader.TotalTime.TotalMilliseconds; i++)
                {
                    points.Add(new DataPoint(Convert.ToDouble(i), Convert.ToDouble(signal[i * step])));
                }

                mainPlot.DataContext = points;

                var deltas = new List<double>();
                double significantChange = 1000.0;
                var times = new List<Tuple<double, int>>();
                var tempDeltas = new List<double>();
                int time = 0;
                for (int i = 1; i < points.Count; i++)
                {
                    double delta = Math.Abs(points[i].Y - points[i - 1].Y);
                    if (deltas.Count > 0)
                    {
                        if (delta - deltas[deltas.Count - 1] <= significantChange)
                        {
                            time++;
                            tempDeltas.Add(delta);
                        }
                        else if (time > 0)
                        {
                            times.Add(new Tuple<double, int>(tempDeltas.Average(), time));
                            time = 0;
                            tempDeltas.Clear();
                        }
                    }
                    deltas.Add(delta);
                }

                double avgDelta = deltas.Average();
                tbAmp.Text = avgDelta.ToString("F2");
                tbSmooth.Text = (1.0 - (double) times.Count/points.Count).ToString("F4");
                double max = deltas.Max();
                double avgMax = deltas.Where(d => d < max).Max();
                tbЬMaxAmp.Text = max.ToString("F2");
                tbAmpCount.Text = deltas.Count(d => d >= avgMax - 4000.0).ToString();
                double k = (avgDelta/max)/(1.0 - (double) times.Count/points.Count);
                if (k > 0.05)
                {
                    tbType.Text = "Smooth";
                }
                else if (k > 0.01 && deltas.Count(d => d >= avgMax - 4000.0) > 2)
                {
                    tbType.Text = "Beating or ticking";
                }
                else
                {
                    tbType.Text = "Explosive";
                }
                if (waveletBox.SelectedIndex > -1)
                {
                    switch (waveletBox.SelectedIndex)
                    {
                        case 0: RenderWavelet(test.WAVE_wavelet); break;
                        case 1: RenderWavelet(test.FHAT_wavelet); break;
                        case 2: RenderWavelet(test.Morlet_wavelet); break;
                        case 3: RenderWavelet(test.POISSON_wavelet); break;
                        case 4: RenderWavelet(test.HAAR_wavelet); break;
                        case 5: RenderWavelet(test.FrenchHat_wavelet); break;
                    }
                }
            }
        }

        private void RenderWavelet(Func<double, int, double> func)
        {
            var res = test.wavelet_transformation(duration, signal, step,
                func, 100);
            double[,] arr = new double[(int)res.Max(f => f.X) + 1, (int)res.Max(f => f.Y) + 1];
            foreach (Point3D point3D in res)
            {
                arr[(int)point3D.X, (int)point3D.Y] = point3D.Z;
            }

            myPlot.PlotFunction((x, y) => arr[(int)x, (int)y], res.Select(r => r.X).ToArray(), res.Select(r => r.Y).ToArray());
        }

        private void WAVE_selected(object sender, RoutedEventArgs e)
        {
            RenderWavelet(test.WAVE_wavelet);
        }

        private void MHAT_selected(object sender, RoutedEventArgs e)
        {
            RenderWavelet(test.FHAT_wavelet);
        }

        private void Morlet_selected(object sender, RoutedEventArgs e)
        {
            RenderWavelet(test.Morlet_wavelet);
        }

        private void Poisson_selected(object sender, RoutedEventArgs e)
        {
            RenderWavelet(test.POISSON_wavelet);
        }

        private void FHAT_selected(object sender, RoutedEventArgs e)
        {
            RenderWavelet(test.FHAT_wavelet);
        }

        private void HAAR_selected(object sender, RoutedEventArgs e)
        {
            RenderWavelet(test.HAAR_wavelet);
        }

        private void Export_OnClick(object sender, RoutedEventArgs e)
        {
            //TODO Export oxyPlot
            var dialog = new SaveFileDialog();
            dialog.FileName = $"export-{fileTitle.Text}.png";
            dialog.Filter = "Png Image (*.png)|*.png";
            dialog.OverwritePrompt = true;
            dialog.AddExtension = true;
            if (dialog.ShowDialog() == true)
            {
                Size size = new Size(wavelet.ActualWidth, wavelet.ActualHeight);

                RenderTargetBitmap result = new RenderTargetBitmap((int)size.Width, (int)size.Height, 96, 96, PixelFormats.Pbgra32);

                DrawingVisual drawingvisual = new DrawingVisual();
                using (DrawingContext context = drawingvisual.RenderOpen())
                {
                    context.DrawRectangle(new VisualBrush(wavelet), null, new Rect(new Point(), size));
                    context.Close();
                }
                
                result.Render(drawingvisual);
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(result));

                using (var file = dialog.OpenFile())
                {
                    encoder.Save(file);
                }
                Process.Start(dialog.FileName);
            }
        }

       private void ExportSample_OnClick(object sender, RoutedEventArgs e)
       {
           // mercy on me, im a copypastor
           var dialog = new SaveFileDialog();
           dialog.FileName = "export.png";
           dialog.Filter = "Png Image (*.png)|*.png";
           dialog.OverwritePrompt = true;
           dialog.AddExtension = true;
           if (dialog.ShowDialog() == true)
           {
             Size size = new Size(mainPlot.ActualWidth, mainPlot.ActualHeight);

             RenderTargetBitmap result = new RenderTargetBitmap((int)size.Width, (int)size.Height, 96, 96, PixelFormats.Pbgra32);

             DrawingVisual drawingvisual = new DrawingVisual();
             using (DrawingContext context = drawingvisual.RenderOpen())
             {
               context.DrawRectangle(new VisualBrush(mainPlot), null, new Rect(new Point(), size));
               context.Close();
             }

             result.Render(drawingvisual);
             PngBitmapEncoder encoder = new PngBitmapEncoder();
             encoder.Frames.Add(BitmapFrame.Create(result));

             using (var file = dialog.OpenFile())
             {
               encoder.Save(file);
             }
             Process.Start(dialog.FileName);
           }
       }

  }
}
