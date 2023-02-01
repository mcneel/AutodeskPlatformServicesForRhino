using System;

using Eto.Forms;

namespace APSRHPlugin.Controls
{
    public class APSSpinner : Panel
    {
        readonly ProgressBar _pbar = new ProgressBar()
        {
            Value = 0,
            Height = 6,
        };

        uint _counter = 0;

        public event EventHandler SpinStopped;

        public APSSpinner()
        {
            Height = 6;
            Spinning = false;
            Content = _pbar;
        }

        public bool Spinning
        {
            get => _pbar.Indeterminate;
            set
            {
                _pbar.Indeterminate = value;
                _pbar.Visible = _pbar.Indeterminate;
            }
        }

        public void Spin()
        {
            _counter++;
            Spinning = true;
        }

        public void UnSpin()
        {
            _counter--;
            Spinning = _counter > 0;

            if (!Spinning)
                SpinStopped?.Invoke(this, new EventArgs());
        }
    }
}
