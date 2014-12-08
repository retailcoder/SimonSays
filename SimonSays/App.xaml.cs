using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Media;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace SimonSays
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly MainWindow _mainWindow = new MainWindow();
        private SimonSaysRound _currentRound;

        private Random _random;
        private readonly int _seed;

        public App()
        {
            _seed = new Random().Next();
        }

        protected override void OnStartup(StartupEventArgs e)
        {            
            _mainWindow.SimonButtonClicked += OnSimonButtonClick;
            _mainWindow.PlayNextRound += _mainWindow_PlayNextRound;
            _mainWindow.ShowDialog();
        }

        private void _mainWindow_PlayNextRound(object sender, EventArgs e)
        {
            PlayNextRound();
        }

        private void PlayNextRound()
        {
            var sequenceLength = 1;
            var score = 0;
            if (_currentRound != null)
            {
                sequenceLength = _currentRound.Length + 1;
                score = _currentRound.Score;
            }

            _currentRound = new SimonSaysRound(GenerateSequence(sequenceLength), score);
            _currentRound.RoundCompleted += _currentRound_RoundCompleted;
            _mainWindow.DataContext = _currentRound;
            _mainWindow.DataContext = _currentRound;
            PlaySequence();
        }

        private IEnumerable<SimonButton> GenerateSequence(int length)
        {
            _random = new Random(_seed);
            for (var i = 0; i < length; i++)
            {
                yield return (SimonButton)_random.Next(Enum.GetValues(typeof(SimonButton)).GetLength(0));
            }
        }

        private void _currentRound_RoundCompleted(object sender, SimonSaysScoreEventArgs e)
        {
            if (e.Success)
            {
                _mainWindow.OnRoundSuccessful();
            }
            else
            {
                _mainWindow.OnGameOver();
            }
        }

        private void PlaySequence()
        {
            var tasks = _currentRound.Sequence.Select(
                button => new Task(() => OnSimonButtonClick(null, new SimonButtonEventArgs(button))));

            foreach (var task in tasks)
            {
                task.RunSynchronously();
            }
        }

        private void OnSimonButtonClick(object sender, SimonButtonEventArgs e)
        {
            var folder = Path.GetDirectoryName(GetType().Assembly.Location);
            var soundFile = Path.Combine(folder ?? string.Empty, "Resources", e.Button + ".wav");
            using (var player = new SoundPlayer(soundFile))
            {
                player.Play();
            }

            if (sender != null)
            {
                _currentRound.Play(e.Button);
            }
            else
            {
                _mainWindow.HighlightSimonButton(e.Button);
            }
        }
    }
}
