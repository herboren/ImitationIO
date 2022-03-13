using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ImitationIO
{
    /// <summary>
    /// The idea of this application is to find the shortest distance between two points.
    /// The initial cursor position and the point at which motion is detected from the
    /// BlobMotionDetection frames. After the points have been identified, a random Bezier
    /// curve is generated with the smallest absolute value between polynomials to imitate
    /// the straightest path that a human could generate.
    /// 
    /// The screen is separated into Quadrants. Each quadrant determines the absolute
    /// coordinates of cursor position and determines the straightest path to draw.
    /// 
    /// The mouse moves at approx 15ms per ms tick it takes for the human to move the mouse
    /// from one position to another. The speed of human mouse movement is recorded
    /// (milliseconds) and stored as JSON for later use. Since coordinates cannot be recorded
    /// as the position will be arbitrary at all times, the travel of the mouse depends solely
    /// on the its current position and where motion is detected. Positions are never the same.
    /// </summary>
    /// 
    
    public partial class Form1 : Form
    {

        Timer _t;
        Stopwatch stopwatch;
        List<Tuple<Point, double>> ms_loc_el = new List<Tuple<Point, double>>();
        List<double> ms_sum = new List<double>();
        BackgroundWorker _t_bw;
        BackgroundWorker out_result;

        private double _milliseconds;
        public double Milliseconds
        {
            get { return _milliseconds; }
            set { _milliseconds = value; }
        }


        /// <summary>
        /// Initialize tick even for stopwatch and timer
        /// </summary>
        public Form1()
        {
            InitializeComponent();
            _t_bw = new BackgroundWorker();
            _t_bw.DoWork += _t_bw_DoWork;
            _t_bw.WorkerSupportsCancellation = true;
            //
            _t = new Timer();
            _t.Interval = 1;
            _t.Tick += _t_Tick;
        }

        /// <summary>
        /// Log timer events for mouse
        /// Record time taken to click two points
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _t_Tick(object sender, EventArgs e)
        {
            if (!_t_bw.IsBusy)
                _t_bw.RunWorkerAsync();
        }

        /// <summary>
        /// Start timer and stopwatch, stopwatch returns elapsed milliseconds between points
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnStart_Click(object sender, EventArgs e)
        {
            stopwatch = new Stopwatch();
            ms_loc_el.Clear();
            ms_sum.Clear();
            _t.Start();
            stopwatch.Start();
            txtJson.Clear();
            txtCursorLog.Clear();
        }

        /// <summary>
        /// Stop timer when click event ended
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnEnd_Click(object sender, EventArgs e)
        {
            out_result = new BackgroundWorker();
            out_result.DoWork += out_result_DoWork;

            stopwatch.Stop();
            _t.Stop();

            out_result.RunWorkerAsync();
        }

        /// <summary>
        /// Process serialize data and output for future use.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void out_result_DoWork(object sender, DoWorkEventArgs e)
        {
            Point temp = Point.Empty;
            string jsons = JsonConvert.SerializeObject(ms_loc_el, Formatting.Indented);

            foreach (Tuple<Point, double> item in ms_loc_el)
            {
                if (temp != item.Item1)
                {
                    txtCursorLog.Invoke((MethodInvoker)delegate
                    {
                        txtCursorLog.Text += $"Position: {item.Item1}{Environment.NewLine}";
                    });
                }
                temp = item.Item1;
            }

            txtCursorLog.Invoke((MethodInvoker)delegate
            {
                txtCursorLog.Text += $"{Environment.NewLine}Average: {GetAverageElapsed(ms_sum)}" +
                $"{Environment.NewLine}Total: {(double)ms_loc_el.ElementAt(ms_loc_el.Count - 1).Item2}";
            });

            txtJson.Invoke((MethodInvoker)delegate
            {
                txtJson.Text = jsons;
            });
        }

        /// <summary>
        /// Background worker check millisends accuracey and stores elapsed and location
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        private void _t_bw_DoWork(object sender, DoWorkEventArgs e)
        {
            ms_sum.Add(stopwatch.Elapsed.TotalMilliseconds);
            Milliseconds = stopwatch.Elapsed.TotalMilliseconds;
            ms_loc_el.Add(new Tuple<Point, double>(Cursor.Position, Milliseconds));
        }

        private void txtCursorLog_TextChanged(object sender, EventArgs e)
        {
            txtCursorLog.SelectionStart = txtCursorLog.Text.Length;
            txtCursorLog.ScrollToCaret();
        }

        /// <summary>
        /// Scroll bot bottom of the log during recording.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            txtCursorLog.SelectionStart = txtCursorLog.Text.Length;
            txtCursorLog.ScrollToCaret();
        }

        /// <summary>
        /// Gets average elapsed time, can be used with generating between random average and total time
        /// </summary>
        /// <param name="ms"></param>
        /// <returns></returns>
        public double GetAverageElapsed(List<double> ms)
        {
            double sum = ms.Sum();
            return (sum / ms.Count());
        }

        /// <summary>
        /// Scroll to end
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtJson_TextChanged(object sender, EventArgs e)
        {
            txtJson.SelectionStart = txtJson.Text.Length;
            txtJson.ScrollToCaret();
        }

        Stopwatch istopwatch;
        BackgroundWorker imouse;
        /// <summary>
        /// Run worker async when imitating mouse movement.
        /// Move on thread = faster prcoessing.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnImitate_Click(object sender, EventArgs e)
        {
            imouse = new BackgroundWorker();
            imouse.DoWork += imouse_DoWork;
            //
            imouse.RunWorkerAsync();
        }

        /// <summary>
        /// Thread worker, move mouse in the given time frame but at
        /// a rate of 15 ms per frame. When complete, stop watch log time taken.
        /// Compare average time between initial movement and imitation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void imouse_DoWork(object sender, DoWorkEventArgs e)
        {
            istopwatch = new Stopwatch();
            istopwatch.Start();
            do
            {
                try
                {
                    stopwatch.Start();
                    for (int m = 0; m <= (int)ms_loc_el.ElementAt(ms_loc_el.Count - 1).Item2; m++)
                    {
                        try
                        {
                            System.Threading.Thread.Sleep(15);
                            Cursor.Position = new Point(ms_loc_el.ElementAt(m).Item1.X, ms_loc_el.ElementAt(m).Item1.Y);
                        }
                        catch (Exception ex) { }
                    }

                    stopwatch.Stop();
                    istopwatch.Stop();
                }
                catch (Exception ex) { }

            } while (istopwatch.Elapsed.TotalMilliseconds < ms_loc_el.ElementAt(ms_loc_el.Count - 1).Item2);
        }

        /// <summary>
        /// Generate random points on the screen.
        /// This will help when generating random start and stop points for bezier
        /// curves. Then using the stopwatch the mouse will be controled between points.
        /// This is merely just testing the control and imitation of the mouse. The bezier
        /// curve will be generated betwee the starting point of the mouse and the blob
        /// motion detection.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// 

        public bool _hasDrawn = false;

        private void btnGeneratePoint_Click(object sender, EventArgs e)
        {
            using (Dots dots = new Dots())
            {
                if (!_hasDrawn)
                {
                    dots.DrawPanel(drawPanel.Handle, drawPanel.Size, _hasDrawn);
                    _hasDrawn = true;
                }
                else
                {
                    dots.DrawPanel(drawPanel.Handle, drawPanel.Size, _hasDrawn);
                    _hasDrawn = false;
                }
            }            
        }

        private void btnClearPoints_Click(object sender, EventArgs e)
        {
            // Clear Panel
            drawPanel.Invalidate();            
        }
    }
}