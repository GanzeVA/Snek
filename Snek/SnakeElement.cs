using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;


namespace Snek
{
	public class SnakeElement
	{
		public UIElement Element { get; set; }
		public Point Position { get; set; }
		public bool IsHead { get; set; }

		public SnakeElement(int x, int y, bool isHead = false)
		{
			Position = new Point(x, y);
			IsHead = isHead;
		}

		public void Paint(Brush snakeSkinBrush)
		{
			if (IsHead)
				(Element as Rectangle).Fill = Brushes.Green;
			else
				(Element as Rectangle).Fill = snakeSkinBrush;
		}
	}
}
