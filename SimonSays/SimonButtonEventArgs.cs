using System;

namespace SimonSays
{
    public class SimonButtonEventArgs : EventArgs
    {
        private readonly SimonButton _button;
        public SimonButton Button { get { return _button; } }

        public SimonButtonEventArgs(SimonButton button)
        {
            _button = button;
        }
    }
}