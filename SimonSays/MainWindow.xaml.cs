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

            DisableButtons();

            MouseDown += MainWindow_MouseDown;
            Activated += MainWindow_Activated;
        }

        private void MainWindow_Activated(object sender, EventArgs e)
        {
            GameScoreLabel.Visibility = Visibility.Collapsed;
            GameButton.Text = "Start!";

            AnimateMessageBand(36);
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
        public void OnSimonButtonClicked(SimonButton button)
        {
            HighlightSimonButton(button);
            var handler = SimonButtonClicked;
            if (handler != null)
            {
                handler(this, new SimonButtonEventArgs(button));
            }
        }

        public void OnGameOver()
        {
            GameButton.Text = "Game Over!";
            GameScoreLabel.Visibility = Visibility.Visible;
            AnimateMessageBand(56);
            
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

        public void OnRoundSuccessful()
        {
            DisableButtons();
            GameButton.Text = string.Format("Round {0} completed! Ready?", ((SimonSaysRound)DataContext).Round);
            GameScoreLabel.Visibility = Visibility.Visible;
            AnimateMessageBand(56);
            GameButton.MouseDown += GameButtonNextRound;
        }

        private void AnimateMessageBand(double height)
        {
            var animation = new DoubleAnimation(height, new Duration(TimeSpan.FromMilliseconds(200)));
            RegisterName(MessageBar.Name, MessageBar);
            Storyboard.SetTargetName(animation, MessageBar.Name);
            Storyboard.SetTargetProperty(animation, new PropertyPath("Height"));
            var story = new Storyboard();
            story.Children.Add(animation);
            story.Begin(MessageBar);
            story.Remove();
            UnregisterName(MessageBar.Name);
        }

        private readonly IDictionary<SimonButton, Border> _buttons;

        public void HighlightSimonButton(SimonButton button)
        {
            var animation = new DoubleAnimation(0, 0.75, new Duration(TimeSpan.FromMilliseconds(100)));
            RegisterName(button.ToString(), _buttons[button]);
            Storyboard.SetTargetName(animation, button.ToString());
            Storyboard.SetTargetProperty(animation, new PropertyPath("Background.GradientStops[1].Offset"));
            var story = new Storyboard();
            story.Children.Add(animation);
            story.Begin(_buttons[button]);
            story.Remove();
            UnregisterName(button.ToString());
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

        private void GameButtonStartGame(object sender, MouseButtonEventArgs e)
        {
            AnimateMessageBand(0);
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
        private void GameButtonNextRound(object sender, MouseButtonEventArgs e)
        {
            AnimateMessageBand(0);
            e.Handled = true;

            var handler = PlayNextRound;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }

            GameButton.MouseDown -= GameButtonNextRound;
            EnableButtons();
        }

        private void GameButtonEndGame(object sender, MouseButtonEventArgs e)
        {
            Close();
            e.Handled = true;
        }
    }
}
