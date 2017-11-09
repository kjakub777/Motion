using Android.App;
using Android.Widget;
using Android.OS;
using Android.Hardware;
using Android.Runtime;
using Android.Content;
using System.Collections.Generic;
using System;
using System.Linq;
using Android.Graphics;
using Android.Content.PM;

namespace MotionAccel
{
    [Activity(Label = "MotionAccel", MainLauncher = true, Icon = "@drawable/icon", ScreenOrientation = ScreenOrientation.Portrait)]
    public class MainActivity : Activity, ISensorEventListener
    {
        int count = 1;
        static readonly object _syncLock = new object();
        SensorManager _sensorManager;
        TextView Deltas;
        TextView _sensorTextView;
        LinearLayout linearLayout1;
        private List<float> Xs = new List<float>();
        private List<float> Ys = new List<float>();
        private List<float> Zs = new List<float>(); Button button;
        private float LIMIT = 100;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);
            linearLayout1 = FindViewById<LinearLayout>(Resource.Id.linearLayout1);
            _sensorManager = (SensorManager)GetSystemService(Context.SensorService);
            _sensorTextView = FindViewById<TextView>(Resource.Id.accelerometer_text);
            Deltas = FindViewById<TextView>(Resource.Id.Deltas);
            ; button = FindViewById<Button>(Resource.Id.button);
            button.Click += delegate
            {
                linearLayout1.SetBackgroundDrawable(GetDrawable(Resource.Drawable.hap));// @android: drawable / dialog_holo_dark_frame); = Color.Pink;
            };

        }

        public void OnAccuracyChanged(Sensor sensor, [GeneratedEnum] SensorStatus accuracy)
        {

        }

        public void OnSensorChanged(SensorEvent e)
        {
            lock (_syncLock)
            {
                Xs.Add(e.Values[0]);
                Ys.Add(e.Values[1]);
                Zs.Add(e.Values[2]);
                if (Xs.Count > LIMIT || Ys.Count > LIMIT || Zs.Count > LIMIT)
                {

                    measure(e);
                }
                _sensorTextView.Text = string.Format("x={0:f}, y={1:f}, z={2:f}", e.Values[0], e.Values[1], e.Values[2]);
            }
        }

        private void measure(SensorEvent e)
        {
            List<float> sumsquares = new List<float>();
            // e.Sensor.MaximumRange;
            float averagex = Xs.Average();
            float sumOfSquaresOfDifferencesx = Xs.Select(val => (val - averagex) * (val - averagex)).Sum();
            float sdx = (float)Math.Sqrt(sumOfSquaresOfDifferencesx / Xs.Count);
            

            float averagey = Ys.Average();
            float sumOfSquaresOfDifferencesy = Ys.Select(val => (val - averagey) * (val - averagey)).Sum();
            float sdy = (float)Math.Sqrt(sumOfSquaresOfDifferencesy / Ys.Count);

            float averagez = Zs.Average();
            float sumOfSquaresOfDifferencesz = Zs.Select(val => (val - averagez) * (val - averagez)).Sum();
            float sdz = (float)Math.Sqrt(sumOfSquaresOfDifferencesz / Zs.Count);

            sumsquares.Add(sumOfSquaresOfDifferencesx); sumsquares.Add(sumOfSquaresOfDifferencesy); sumsquares.Add(sumOfSquaresOfDifferencesz);
            Deltas.Text = $"ssqx {sumOfSquaresOfDifferencesx} sdx {sdx}\nssqy {sumOfSquaresOfDifferencesy} sdy {sdy}\nssqz {sumOfSquaresOfDifferencesz} sdz {sdz}\n";
            try
            {
                
                Random rand = new Random((int)DateTime.Now.Ticks);
                Color myColor = Color.Argb(/*rand.Next(50,255)*/250,(Convert.ToInt32(((sumOfSquaresOfDifferencesz) / (((sumsquares.Max()+sumsquares.Min())/2f)+1)) * 255f)), (Convert.ToInt32(((sumOfSquaresOfDifferencesx) / (sumsquares.Max()+1)) * 255f)), (Convert.ToInt32(((sumOfSquaresOfDifferencesy) / (sumsquares.Max()+1)) * 255f))/*(int)Math.Floor( (1f / ((sdz + sdx + sdy) / .68f)) * 255)*/);
                string strcol = $"ARGB{myColor.A},{myColor.R},{myColor.G},{myColor.B}";
                Deltas.Text += strcol;
                Deltas.SetBackgroundColor(myColor);
                //Deltas.SetTextColor(Color.White);/// ParseColor(Color.Black.GetHashCode-myColor.GetHashCode)); 
                Deltas.SetTextColor(Color.Argb(250 , 255 - myColor.R, 255 - myColor.G, 255 - myColor.B));
                //Deltas.SetTextColor(Color.Argb(255 , 255 - myColor.R, 255 - myColor.G, 255 - myColor.B));

            }
            catch (Exception x)
            {

                Deltas.Text = x.ToString();
            }
            finally
            {
                Xs.Clear();
                Ys.Clear();
                Zs.Clear();
            }
        }

        protected override void OnResume()
        {
            base.OnResume();
            _sensorManager.RegisterListener(this, _sensorManager.GetDefaultSensor(SensorType.Accelerometer), SensorDelay.Ui);
        }
        protected override void OnPause()
        {
            base.OnPause();
            _sensorManager.UnregisterListener(this);
        }
    }
}

