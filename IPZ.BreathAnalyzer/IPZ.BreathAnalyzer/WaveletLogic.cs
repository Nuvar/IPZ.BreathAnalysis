using System;

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
    double[,] mass = new double[500, 500];
    double[,] mass_drawing = new double[500, 500];
    double step = Period / (k - 1);
    double max = 0;
    double min = 0;
    double coefficient = 0;

    //Wave-вейвлет
    private double WAVE_wavelet(double t)
    {
      return -1 / Math.Sqrt(Period) * t * Math.Exp(-Math.Pow(t, 2) / 2);
    }

    //Вейвлет "Мексиканская шляпа"
    private double FHAT_wavelet(double t)
    {
      return -1 / Math.Sqrt(Period) * (Math.Pow(t, 2) - 1) * Math.Exp(-Math.Pow(t, 2) / 2);
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
    public void wavelet_transformation()
    {
      int Tao = 0;
      int S = 0;
      double little_step = (1 - 0.01) / (k - 1);

      for (double tau = -Period; tau <= Period; tau += step)
      {
        S = 0;

        for (double s = 0.01; s <= 1; s += little_step)
        {
          for (double t = -Period; t <= Period; t += step)
            mass[Tao, S] += 1.0 / Math.Sqrt(Math.Abs(s)) * GetSignalQuantum(t) * WAVE_wavelet((t - tau) / s);

          S++;
        }

        Tao++;
      }

      // idk normalizing is unnesessary right now
      #region SAVE_FOR_LATER 
      /*
      for (int i = 0; i < k; i++)
      {
        for (int j = 0; j < k; j++)
        {
          if (min > mass[i, j])
          {
            min = mass[i, j];
          }

        }
      }

      for (int i = 0; i < k; i++)
      {
        for (int j = 0; j < k; j++)
        {
          mass[i, j] = mass[i, j] - min;
        }
      }

      for (int i = 0; i < k; i++)
      {
        for (int j = 0; j < k; j++)
        {
          if (max < mass[i, j])
          {
            max = mass[i, j];
          }

        }
      }

      coefficient = 255 / max;
      for (int i = 0; i < k; i++)
      {
        for (int j = 0; j < k; j++)
        {
          mass[i, j] = mass[i, j] * coefficient;

        }
      }
    */
      #endregion

      int breakpointPlease = 0;
    }

  }
}