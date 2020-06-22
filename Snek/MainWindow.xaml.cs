using System.Windows;

namespace Snek
{
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();

			highScoreTextBlock.Text = $"High Score: {HighScore.Default.Top}";
		}

		private void startGameButton_Click(object sender, RoutedEventArgs e)
		{
			GameWindow gameWindow = new GameWindow();
			gameWindow.Show();
			this.Close();
		}
	}
}
