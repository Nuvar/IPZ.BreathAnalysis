﻿using System;
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
using Microsoft.Win32;
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
        double fr = 1.0 / 44000;
        double ms = 0.001;
        private int step = 44;
        private int duration = 0;
        private short[] signal = new short[0];
        private Wavelet test = new WaveletLogic.Wavelet();
        private SurfacePlotModel myPlot = new SurfacePlotModel();
        public MainWindow()
        {
            InitializeComponent();
            wavelet.DataContext = myPlot;
        }

        private void OnFileLoadClick(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
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
                signal = new short[read/2];
                Buffer.BlockCopy(buffer, 0, signal, 0, read);
                duration = (int) reader.TotalTime.TotalMilliseconds;
                var points = new List<DataPoint>();

                for (int i = 0; i < reader.TotalTime.Milliseconds; i++)
                {
                    points.Add(new DataPoint(Convert.ToDouble(i), Convert.ToDouble(signal[i*step])));
                }

                mainPlot.DataContext = points;
            }
        }

        private void RenderWavelet(Func<double,int,double> func)
        {
            var res = test.wavelet_transformation(duration, signal, step,
                func, 100);
            double[,] arr = new double[(int) res.Max(f => f.X) + 1, (int) res.Max(f => f.Y) + 1];
            foreach (Point3D point3D in res)
            {
                arr[(int) point3D.X, (int) point3D.Y] = point3D.Z;
            }
            myPlot.PlotFunction((x, y) => arr[(int) x, (int) y], res.Select(r => r.X).ToArray(), res.Select(r => r.Y).ToArray());
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
    }
}
