using System;

namespace SimonSays
{
    public class SimonSaysRoundCompleteEventArgs : EventArgs
    {
        public SimonSaysRoundCompleteEventArgs(bool success, int score)
        {
            _success = success;
            _score = score;
        }

        private readonly bool _success;
        public bool Success { get { return _success; } }

        private readonly int _score;
        public int Score { get { return _score; } }
    }
}