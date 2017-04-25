using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Documents;
using System.Windows.Media.Media3D;

namespace WaveletLogic
{
    // all code will be here for now
    // todo restructure later
    public class Wavelet
    {
        const int k = 200;
        const double Period = 2 * Math.PI;
        double[] mas_initial_chart = new double[500];
        double[] mass_WAVE_wavelet = new double[500];
        double[] mass_FHAT_wavelet = new double[500];

        double[,] mass_drawing = new double[500, 500];
        double step = Period / (k - 1);
        double max = 0;
        double min = 0;
        double coefficient = 0;

        //Wave-вейвлет
        public double WAVE_wavelet(double t, int T)
        {
            return -1 / Math.Sqrt(T) * t * Math.Exp(-Math.Pow(t, 2) / 2);
        }

        //Вейвлет "Мексиканская шляпа"
        public double FHAT_wavelet(double t, int T)
        {
            return -1 / Math.Sqrt(T) * (Math.Pow(t, 2) - 1) * Math.Exp(-Math.Pow(t, 2) / 2);
        }

        // Вейвлет Пуассона
        // https://en.wikipedia.org/wiki/Poisson_wavelet
        public double POISSON_wavelet(double t, int T)
        {
            return 1 / Math.PI * ((1 - Math.Pow(t, 2) / Math.Pow(1 + Math.Pow(t, 2), 2)));
        }
		
		//Вейвлет Хаара
		//https://en.wikipedia.org/wiki/Haar_wavelet
		public double HAAR_wavelet(double t, int T)
        {
            double result = null;
			if (t >= 0 && t < 0.5)
			{
				result = 1;
			} else if (t >= 0.5 && t < 1)
			{
				result = -1;
			} else if (t < 0 || t >= 1)
			{
				result = 0;
			}
			return result;
        }

        /// <summary>
        /// amplitude or any signal representation as the returned result will work 
        /// </summary>
        /// <param name="t">time</param>
        /// <returns>in example it returned amplitude of a signal</returns>
        private double GetSignalQuantum(double t)
        {
            // TEMPORARY FUNCTION
            // TODO signal processing
            return Math.Cos(Math.Pow(Math.E, Math.Sin(3 * t * 0.33)));
        }

        //Вейвлет-преобразование
        public List<Point3D> wavelet_transformation(int T, short[] signal, int stp, Func<double, int, double> wavelet, int N)
        {
            int Tao = 0;
            int S = 0;
            double little_step = (1 - 0.01) / (k - 1);

            int st = T / N;
            if (st == 0)
            {
                st = 1;
            }
            List<Point3D> points = new List<Point3D>();
            int Tau = 0;
            int Ss = 0;
            int Tt = 0;
            try
            {


                for (int tau = 0; tau < T; tau += st)
                {
                    Tau = tau;
                    for (int s = 1; s <= 500; s += 49)
                    {
                        Ss = s;
                        var p = new Point3D(tau, s, 0.0);
                        for (int t = 0; t <= T; t += st)
                        {
                            Tt = t;
                            p.Z += Math.Abs(1.0 / Math.Sqrt(Math.Abs(s)) * signal[t * stp] * wavelet((double)(t - tau) / s, T));
                        }
                        points.Add(p);
                        S++;
                    }

                    Tao++;
                }
            }
            catch (Exception e)
            {

                throw;
            }
            

            // idk normalizing is unnesessary right now
            #region SAVE_FOR_LATER 

            min = points.Min(p => p.Z);
            max = points.Max(p => p.Z);

            coefficient = 255 / max;
            for (int i = 0; i < points.Count; i++)
            {
                Point3D p = points[i];
                p.Z -= min;
                p.Z *= coefficient;
                points[i] = p;
            }
            #endregion

            int breakpointPlease = 0;

            return points;
        }



    }
}