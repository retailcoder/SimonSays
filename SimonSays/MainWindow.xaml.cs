using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace SimonSays
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly IDictionary<SimonButton, Border> _buttons;

        public MainWindow()
        {
            InitializeComponent();
            _buttons = new Dictionary<SimonButton, Border>
                {
                    { SimonButton.Green, Green },
                    { SimonButton.Red, Red },
                    { SimonButton.Yellow, Yellow },
                    { SimonButton.Blue, Blue }
                };

            RegisterName(MessageBar.Name, MessageBar);
            foreach (var button in _buttons)
            {
                RegisterName(button.Value.Name, button.Value);
            }

            DisableButtons();

            MouseDown += MainWindow_MouseDown;
            Activated += MainWindow_Activated;
        }

        private async void MainWindow_Activated(object sender, EventArgs e)
        {
            GameScoreLabel.Visibility = Visibility.Collapsed;
            GameButton.Text = "Start!";

            await AnimateMessageBand(36);
            Activated -= MainWindow_Activated;

            GameButton.MouseDown += GameButtonStartGame;
        }

        private void MainWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
        }

        public event EventHandler<SimonButtonEventArgs> SimonButtonClicked;
        public async Task OnSimonButtonClicked(SimonButton button)
        {
            var handler = SimonButtonClicked;
            if (handler != null)
            {
                handler.Invoke(this, new SimonButtonEventArgs(button));
            }
        }

        public async Task OnGameOver()
        {
            GameButton.Text = string.Format("Oops! {0} rounds completed.", ((SimonSaysRound)DataContext).Round - 1);
            GameScoreLabel.Visibility = Visibility.Visible;
            await AnimateMessageBand(56);
            
            DisableButtons();

            GameButton.MouseDown += GameButtonEndGame;
        }

        private void DisableButtons()
        {
            Green.MouseDown -= Green_MouseDown;
            Red.MouseDown -= Red_MouseDown;
            Yellow.MouseDown -= Yellow_MouseDown;
            Blue.MouseDown -= Blue_MouseDown;
        }

        public void EnableButtons()
        {
            Green.MouseDown += Green_MouseDown;
            Red.MouseDown += Red_MouseDown;
            Yellow.MouseDown += Yellow_MouseDown;
            Blue.MouseDown += Blue_MouseDown;
        }

        public async Task OnRoundSuccessful()
        {
            DisableButtons();
            GameButton.Text = string.Format("Round {0} completed! Ready?", ((SimonSaysRound)DataContext).Round);
            GameScoreLabel.Visibility = Visibility.Visible;
            await AnimateMessageBand(56);
            GameButton.MouseDown += GameButtonNextRound;
        }

        public async Task AnimateMessageBand(double height)
        {
            var animation = new DoubleAnimation(height, new Duration(TimeSpan.FromMilliseconds(200)));
            
            Storyboard.SetTargetName(animation, MessageBar.Name);
            Storyboard.SetTargetProperty(animation, new PropertyPath("Height"));

            var story = new Storyboard();
            story.Children.Add(animation);
            await story.BeginAsync(MessageBar);

            story.Remove();
        }

        public async Task HighlightSimonButton(SimonButton button)
        {
            var duration = new Duration(TimeSpan.FromMilliseconds(100));

            var border = _buttons[button];
            var animation = new DoubleAnimation(0, 0.75, duration);

            Storyboard.SetTargetName(animation, button.ToString());
            Storyboard.SetTargetProperty(animation, new PropertyPath("Background.GradientStops[1].Offset"));

            var story = new Storyboard();
            story.Children.Add(animation);

            await story.BeginAsync(border);

            story.Remove();
        }

        private void Blue_MouseDown(object sender, MouseButtonEventArgs e)
        {
            OnSimonButtonClicked(SimonButton.Blue);
            e.Handled = true;
        }

        private void Yellow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            OnSimonButtonClicked(SimonButton.Yellow);
            e.Handled = true;
        }

        private void Green_MouseDown(object sender, MouseButtonEventArgs e)
        {
            OnSimonButtonClicked(SimonButton.Green);
            e.Handled = true;
        }

        private void Red_MouseDown(object sender, MouseButtonEventArgs e)
        {
            OnSimonButtonClicked(SimonButton.Red);
            e.Handled = true;
        }

        private async void GameButtonStartGame(object sender, MouseButtonEventArgs e)
        {
            await AnimateMessageBand(0);
            e.Handled = true;
            
            GameButton.MouseDown -= GameButtonStartGame;

            var handler = PlayNextRound;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
            EnableButtons();
        }

        public event EventHandler PlayNextRound;
        private async void GameButtonNextRound(object sender, MouseButtonEventArgs e)
        {
            await AnimateMessageBand(0);
            e.Handled = true;

            var handler = PlayNextRound;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }

            GameButton.MouseDown -= GameButtonNextRound;
            EnableButtons();
        }

        private async void GameButtonEndGame(object sender, MouseButtonEventArgs e)
        {
            await AnimateMessageBand(0);
            Close();
            e.Handled = true;
        }
    }

    public static class StoryboardExtensions
    {
        public static Task BeginAsync(this Storyboard storyboard, FrameworkElement containingObject)
        {
            var source = new TaskCompletionSource<bool>();
            if (storyboard == null)
                source.SetException(new ArgumentNullException());
            else
            {
                EventHandler onComplete = null;
                onComplete = (sender, args) =>
                {
                    storyboard.Completed -= onComplete;
                    source.SetResult(true);
                };
                storyboard.Completed += onComplete;
                containingObject.Dispatcher.Invoke(() => storyboard.Begin(containingObject));
            }
            return source.Task;
        }
    }
}
