using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Snek
{
	public class Food
	{
		public UIElement Element { get; set; }
		public Point Position { get; set; }

		public Food(int x, int y)
		{
			Position = new Point(x, y);
		}

		public void Paint(Brush foodBrush)
		{
			(Element as Ellipse).Fill = foodBrush;
		}
	}
}
