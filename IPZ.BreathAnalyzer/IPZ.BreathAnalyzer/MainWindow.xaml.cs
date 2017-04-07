using System;
using System.Collections.Generic;
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
using NAudio.Dsp;
using NAudio.Wave;
using OxyPlot;
using WaveletLogic;

namespace IPZ.BreathAnalyzer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var test = new WaveletLogic.Wavelet();
            using (WaveFileReader reader = new WaveFileReader("sample.wav"))
            {
                byte[] buffer = new byte[reader.Length];
                int read = reader.Read(buffer, 0, buffer.Length);
                short[] sampleBuffer = new short[read / 2];
                Buffer.BlockCopy(buffer, 0, sampleBuffer, 0, read);
                var points = new List<DataPoint>();
                Func<double, int, double> blackman = (x, y) =>
                {
                    return 0.54 - 0.46 * Math.Cos(2.0 * Math.PI * x / 99);
                };
                double fr = 1.0/44000;
                double ms = 0.001;
                int step = (int) (ms/fr);
                for (int i = 0; i < reader.TotalTime.Milliseconds; i++)
                {
                    points.Add(new DataPoint(Convert.ToDouble(i), Convert.ToDouble(sampleBuffer[i*step])));
                }

                mainPlot.DataContext = points;

                var myPlot = new SurfacePlotModel();
                var myPlot1 = new SurfacePlotModel();
                wavelet.DataContext = myPlot;
                wavelet1.DataContext = myPlot1;
                var res = test.wavelet_transformation((int)reader.TotalTime.TotalMilliseconds, sampleBuffer, step, test.WAVE_wavelet, 100);
                var res1 = test.wavelet_transformation((int)reader.TotalTime.TotalMilliseconds, sampleBuffer, step, test.FHAT_wavelet, 100);
                double[,] arr = new double[(int)res.Max(f => f.X) + 1, (int)res.Max(f => f.Y) + 1];
                double[,] arr1 = new double[(int)res1.Max(f => f.X) + 1, (int)res1.Max(f => f.Y) + 1];
                foreach (Point3D point3D in res)
                {
                    arr[(int)point3D.X, (int)point3D.Y] = point3D.Z;
                }
                foreach (Point3D point3D in res1)
                {
                    arr1[(int)point3D.X, (int)point3D.Y] = point3D.Z;
                }
                myPlot.PlotFunction((x, y) => arr[(int)x, (int)y], res.Select(r => r.X).ToArray(), res.Select(r => r.Y).ToArray());
                myPlot1.PlotFunction((x, y) => arr1[(int)x, (int)y], res1.Select(r => r.X).ToArray(), res1.Select(r => r.Y).ToArray());
            }
        }
    }
}
