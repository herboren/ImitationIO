using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ImitationIO
{
    class Dots : IDisposable
    {
        /// <summary>
        /// Implement supression of garbage collector.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
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
        
        public void DrawPanel(IntPtr _h, Size perimeter, bool drawn)
        {
            if (!drawn)
            {
                using (Graphics dotA = Graphics.FromHwnd(_h))
                {
                    dotA.FillEllipse(Brushes.Red, RandomDot(perimeter));
                    _hasDrawn = true;
                }
                
            }
            else
            {
                using (Graphics dotB = Graphics.FromHwnd(_h))
                {
                    dotB.FillEllipse(Brushes.Blue, RandomDot(perimeter));
                    _hasDrawn = false;
                }
            }

        }

        /// <summary>
        /// Generates a random dot for both A and B coordinates
        /// </summary>
        /// <returns></returns>
        private Rectangle RandomDot(Size _size)
        {
            Random rand = new Random();
            int _width = rand.Next(0, _size.Width - (int)((_size.Width * 0.05f)));
            int _height = rand.Next(0, _size.Height - (int)((_size.Height * 0.05f)));
            return new Rectangle(_width, _height, 10, 10);
        }

        public void DrawPanel_Clear(Region _r)
        {
            // Clear Panel
            _r.Dispose();
        }

        protected virtual void Dispose(bool disposed)
        {
            if (disposed)
            {
                disposed = true;
            }
        }
    }
}
