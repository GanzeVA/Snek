using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Snek
{
    public partial class GameWindow : Window
    {
        private const int tileSize = 40; //Size of one tile in gameWindow

        private int gameSpeed = 250; //Starting game speed, in miliseconds
        private const int gameSpeedMax = 150;

        private bool moveLock = false;

        private int score = 0;
        private int snakeLength = 3;

        private int maxY;

        private List<SnakeElement> snakeElements = new List<SnakeElement>();
        private SnakeDirection direction = SnakeDirection.Right;

        private Food curFood;

        private DispatcherTimer gameTimer = new DispatcherTimer();

        private DispatcherTimer timer = new DispatcherTimer();
        private Stopwatch stopwatch = new Stopwatch();

        private Random rand = new Random();

        private Brush snakeSkinBrush = Brushes.Green;
        private Brush foodBrush = Brushes.Red;
        
        public GameWindow()
        {
            InitializeComponent();

            highScoreTextBlock.Text = $"High Score:\n {HighScore.Default.Top}";
            curScoreTextBlock.Text = $"Score:\n {score}";

            highScoreMessage.Visibility = Visibility.Collapsed;
            gameOverMessage.Visibility = Visibility.Collapsed;
            pauseMessage.Visibility = Visibility.Collapsed;
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            StartGame();
        }

        private void StartGame()
        {
            snakeElements.Add(new SnakeElement(0, 0, false));
            snakeElements.Add(new SnakeElement(tileSize, 0, false));
            snakeElements.Add(new SnakeElement(tileSize * 2, 0, true));

            curFood = FindNewFoodPlace();

            CreateTiles();
            CreateSnake();
            CreateFood();

            timer.Tick += Timer_Tick;
            timer.Interval = new TimeSpan(0, 0, 0, 0, 1);
            stopwatch.Start();
            timer.Start();

            gameTimer.Interval = TimeSpan.FromMilliseconds(gameSpeed);
            gameTimer.Tick += GameTimer_Tick;
            gameTimer.Start();
        }

        private void PauseGame()
        {
            if (highScoreMessage.Visibility == Visibility.Visible || gameOverMessage.Visibility == Visibility.Visible)
                return;

            if (gameTimer.IsEnabled)
            {
                gameTimer.Stop();
                timer.Stop();
                stopwatch.Stop();
                pauseMessage.Visibility = Visibility.Visible;
            }
            else
            {
                gameTimer.Start();
                timer.Start();
                stopwatch.Start();
                pauseMessage.Visibility = Visibility.Collapsed;
            }
        }

        private void GameOver()
        {
            SoundsManager.PlayGameOverSound();

            gameTimer.Stop();
            timer.Stop();
            stopwatch.Stop();
            scoreHighTextBlock.Text = score.ToString();
            scoreOverTextBlock.Text = score.ToString();

            if (score > HighScore.Default.Top)
            {
                highScoreMessage.Visibility = Visibility.Visible;
                HighScore.Default.Top = score;
                HighScore.Default.Save();
            }
            else
            {
                gameOverMessage.Visibility = Visibility.Visible;
            }
        }

        private void CreateTiles()
        {
            int curX, curY; //Stores current x and y values for tiles
            curX = curY = 0;
            int row = 0;
            bool white = true; //Determines if a tile to be created should be white or black

            while (curY + tileSize < gameArea.ActualHeight)
            {
                Rectangle tRect = new Rectangle
                {
                    Width = tileSize,
                    Height = tileSize,
                    Fill = white ? Brushes.White : Brushes.LightGreen
                };
                gameArea.Children.Add(tRect);
                Canvas.SetLeft(tRect, curX);
                Canvas.SetTop(tRect, curY);

                white = !white;
                curX += tileSize;
                if (curX >= gameArea.ActualWidth)
                {
                    curX = 0;
                    curY += tileSize;
                    row++;
                    white = (row % 2 == 0);
                }
            }
            maxY = curY - tileSize;
        }

        private void CreateSnake()
        {
            foreach (SnakeElement snakeElement in snakeElements)
            {
                if (snakeElement.Element == null)
                {
                    snakeElement.Element = new Rectangle()
                    {
                        Width = tileSize,
                        Height = tileSize,
                    };
                    gameArea.Children.Add(snakeElement.Element);
                    Canvas.SetTop(snakeElement.Element, snakeElement.Position.Y);
                    Canvas.SetLeft(snakeElement.Element, snakeElement.Position.X);
                }
                snakeElement.Paint(snakeSkinBrush);
            }
        }

        private void CreateFood()
        {
            curFood.Element = new Ellipse()
            {
                Width = tileSize,
                Height = tileSize
            };
            curFood.Paint(foodBrush);
            gameArea.Children.Add(curFood.Element);
            Canvas.SetTop(curFood.Element, curFood.Position.Y);
            Canvas.SetLeft(curFood.Element, curFood.Position.X);
        }

        private void MoveSnake()
        {
            moveLock = false;

            int newX = (int)snakeElements[snakeElements.Count - 1].Position.X;
            int newY = (int)snakeElements[snakeElements.Count - 1].Position.Y;

            if (snakeElements.Count == snakeLength)
            {
                gameArea.Children.Remove(snakeElements[0].Element);
                snakeElements.RemoveAt(0); //Removing last snake element
            }

            foreach (SnakeElement snakeElement in snakeElements)
            {
                snakeElement.IsHead = false;
            }

            switch (direction)
            {
                case SnakeDirection.Left:
                    newX -= tileSize;
                    break;
                case SnakeDirection.Right:
                    newX += tileSize;
                    break;
                case SnakeDirection.Up:
                    newY -= tileSize;
                    break;
                case SnakeDirection.Down:
                    newY += tileSize;
                    break;
            }
            if (newX < 0)
                newX = (int)gameArea.ActualWidth - tileSize;
            if (newX > gameArea.ActualWidth - tileSize)
                newX = 0;
            if (newY < 0)
                newY = maxY;
            if (newY > gameArea.ActualHeight - tileSize)
                newY = 0;
            snakeElements.Add(new SnakeElement(newX, newY, true));

            CreateSnake();
            CheckForCollisions();
        }

        private void CheckForCollisions()
        {
            SnakeElement head = snakeElements[snakeElements.Count - 1];

            if (head.Position == curFood.Position)
            {
                EatFood();
                return;
            }

            foreach (SnakeElement snakeElement in snakeElements)
            {
                if (snakeElement == head)
                    continue;

                if (snakeElement.Position == head.Position)
                {
                    GameOver();
                    return;
                }
            }
        }

        private void EatFood()
        {
            SoundsManager.PlayPointSound();

            snakeLength++;
            score += snakeLength * 10;
            curScoreTextBlock.Text = $"Score:\n {score}";

            gameSpeed -= 5;
            gameTimer.Interval = TimeSpan.FromMilliseconds(gameSpeed < gameSpeedMax ? gameSpeedMax : gameSpeed);

            gameArea.Children.Remove(curFood.Element);
            curFood = FindNewFoodPlace();

            if (curFood == null) //Game won
                GameOver();

            CreateSnake();
            CreateFood();
        }
        private Food FindNewFoodPlace()
        {
            List<Point> possiblePoints = new List<Point>();

            for (int x = 0; x < gameArea.ActualWidth - tileSize; x += tileSize)
            {
                for (int y = 0; y < gameArea.ActualHeight - tileSize; y += tileSize)
                {
                    possiblePoints.Add(new Point(x, y));
                }
            }
            foreach (SnakeElement snakeElement in snakeElements)
            {
                if (possiblePoints.Contains(snakeElement.Position))
                    possiblePoints.Remove(snakeElement.Position);
            }

            if (possiblePoints.Count == 0) //Game ends
                return null;

            Point p = possiblePoints[rand.Next(0, possiblePoints.Count - 1)];
            return new Food((int)p.X, (int)p.Y);
        }

        /*
        private void GetNewRandomSkin()
        {
            Uri urlUri = new Uri(imageUrls[rand.Next(0, imageUrls.Count)]);
            var request = WebRequest.CreateDefault(urlUri);

            byte[] buffer = new byte[4096];

            BitmapImage bitmap = new BitmapImage();
            using (MemoryStream memoryStream = new MemoryStream())
            {
                try
                {
                    using (WebResponse response = request.GetResponse())
                    {
                        using (Stream stream = response.GetResponseStream())
                        {
                            int read;

                            while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                memoryStream.Write(buffer, 0, read);
                            }
                        }
                    }
                    memoryStream.Flush();
                    memoryStream.Position = 0;
                    bitmap.BeginInit();
                    bitmap.StreamSource = memoryStream;
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    bitmap.Freeze();
                }
                catch (WebException) //url 403 error
                {
                    return;
                }
            }
            snakeSkinBrush = new ImageBrush(bitmap);
        }
        */

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.W:
                case Key.Up:
                    if (!moveLock)
                        direction = (direction == SnakeDirection.Down) ? SnakeDirection.Down : SnakeDirection.Up;
                    moveLock = true;
                    break;

                case Key.D:
                case Key.Right:
                    if (!moveLock)
                        direction = (direction == SnakeDirection.Left) ? SnakeDirection.Left : SnakeDirection.Right;
                    moveLock = true;
                    break;

                case Key.S:
                case Key.Down:
                    if (!moveLock)
                        direction = (direction == SnakeDirection.Up) ? SnakeDirection.Up : SnakeDirection.Down;
                    moveLock = true;
                    break;

                case Key.A:
                case Key.Left:
                    if (!moveLock)
                        direction = (direction == SnakeDirection.Right) ? SnakeDirection.Right : SnakeDirection.Left;
                    moveLock = true;
                    break;

                case Key.P:
                    PauseGame();
                    break;
            }
        }

        private void endGameButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            curTimeTextBlock.Text = $"Time:\n {stopwatch.Elapsed.Minutes}:{stopwatch.Elapsed.Seconds}:{stopwatch.Elapsed.Milliseconds}";
        }

        private void GameTimer_Tick(object sender, EventArgs e)
        {
            MoveSnake();
        }
    }
}
