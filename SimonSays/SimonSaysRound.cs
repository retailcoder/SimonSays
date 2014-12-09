using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using SimonSays.Annotations;

namespace SimonSays
{
    public class SimonSaysRound : INotifyPropertyChanged
    {
        private const int PointsForGoodMatch = 5;

        private readonly SimonButton[] _sequence;
        private int _matches;
        private int _score;

        public SimonSaysRound(IEnumerable<SimonButton> sequence, int score)
        {
            _sequence = sequence.ToArray();
            _score = score;
            _matches = 0;
        }

        public event EventHandler<SimonSaysRoundCompleteEventArgs> RoundCompleted;
        public void OnRoundCompleted()
        {
            var handler = RoundCompleted;
            if (handler != null)
            {
                var result = _matches == _sequence.Length;
                RoundCompleted(this, new SimonSaysRoundCompleteEventArgs(result, Score));
            }
        }

        public void Play(SimonButton button)
        {
            var success = _sequence[_matches] == button;
            if (success)
            {
                Score += PointsForGoodMatch;
                _matches++;
            }

            if (!success || _matches == _sequence.Length)
            {
                OnRoundCompleted();
            }
        }

        public int Round
        {
            get { return _sequence.Length; }
        }

        public int Score
        {
            get { return _score; }
            private set
            {
                if (value == _score) return;
                _score = value;
                OnPropertyChanged();
            }
        }

        public int Length { get { return _sequence.Length; } }
        public IEnumerable<SimonButton> Sequence { get { return _sequence; } }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}