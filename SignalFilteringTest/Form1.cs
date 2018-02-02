using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace SignalFilteringTest {
    public partial class Form1 : Form {

        List<double> raw = new List<double>();
        List<double> output1;
        List<double> output2;

        public Form1() {
            InitializeComponent();
        }

        private void init() {
            this.chart1 = new System.Windows.Forms.DataVisualization.Charting.Chart();
        }

        // load the file, parse the data out, run any filtering operations, then run charting ops
        private void button1_Click(object sender, EventArgs e) {
            if (!File.Exists(textBox1.Text)) {
                MessageBox.Show("Did not find file " + textBox1.Text);
                return;
            }
            try {
                List<Int64> listA = new List<Int64>();
                List<int> listB = new List<int>();
                using (var reader = new StreamReader(textBox1.Text)) {

                    while (!reader.EndOfStream) {
                        var line = reader.ReadLine();

                        if (line == "") {
                            continue;
                        }

                        var values = line.Split(',');

                        listA.Add(Int64.Parse(values[0]));
                        raw.Add((double)int.Parse(values[1]));
                    }
                }
                // do some signal filtering on the values:
                MathNet.Filtering.OnlineFilter lowpass = MathNet.Filtering.OnlineFilter.CreateLowpass(MathNet.Filtering.ImpulseResponse.Finite, 20, 2,1);
                MathNet.Filtering.OnlineFilter denoise = MathNet.Filtering.OnlineFilter.CreateDenoise(3);
                output1 = denoise.ProcessSamples(raw.Select(a => (double)a).ToArray()).ToList();
                output2 = lowpass.ProcessSamples(output1.Select(a => (double)a).ToArray()).ToList();

                chart1.Series.Clear();

                // graph the data
                var series1 = new System.Windows.Forms.DataVisualization.Charting.Series {
                    Name = "Series1",
                    Color = System.Drawing.Color.Green,
                    IsVisibleInLegend = false,
                    IsXValueIndexed = false,
                    ChartType = SeriesChartType.Line
                };

                this.chart1.Series.Add(series1);

                int i = 0;
                foreach(var point in raw.ToArray()) {
                    series1.Points.AddXY(i, point);
                    i++;
                }

                var series2 = new System.Windows.Forms.DataVisualization.Charting.Series {
                    Name = "Series2",
                    Color = System.Drawing.Color.Red,
                    IsVisibleInLegend = false,
                    IsXValueIndexed = false,
                    ChartType = SeriesChartType.Line
                };

                this.chart1.Series.Add(series2);

                int i2 = 0;
                foreach (var point in output1.ToArray()) {
                    series2.Points.AddXY(i2, point);
                    i2++;
                    if (i2 > i-1) {
                        break;
                    }
                }


                var series3 = new System.Windows.Forms.DataVisualization.Charting.Series {
                    Name = "Series3",
                    Color = System.Drawing.Color.Blue,
                    IsVisibleInLegend = false,
                    IsXValueIndexed = false,
                    ChartType = SeriesChartType.Line
                };

                this.chart1.Series.Add(series3);

                int i3 = 0;
                foreach (var point in output2.ToArray()) {
                    series3.Points.AddXY(i3, point);
                    i3++;
                    if (i3 > i-1) {
                        break;
                    }
                }


                chart1.Invalidate();
                Application.DoEvents();
            }
            catch {
                // fall through
            }            
        }
        

        static private void WriteFile(double[] noisy, double[] clean) {
            using (TextWriter tw = new StreamWriter("data.csv")) {
                for (int i = 0; i < noisy.Length; i++)
                    tw.WriteLine(string.Format("{ 0:0.00}, { 1:0.00}", noisy[i], clean[i]));
                tw.Close();
            }
        }

        // not going to be used, but useful for testing perhaps
        static private double[] NoisySine() {
            // Create a noisy sine wave.
            double[] noisySine = new double[180];
            Random rnd = new Random();
            for (int i = 0; i < 180; i++)
                noisySine[i] = Math.Sin(Math.PI * i / 90) + rnd.NextDouble() - 0.5;
            return noisySine;
        }
    }
}
