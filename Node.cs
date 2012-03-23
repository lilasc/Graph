using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Media.Animation;

namespace Graph
{
    class Node : Border
    { 
        TextBlock txtblk;
        Color defaultBorderColor = Colors.Black;
        Color focusBorderColor = Colors.White;

        public Node()
        {
            Width = 128;
            Height = 32;

            BorderBrush = new SolidColorBrush(DefaultBorderColor);
            Background = Brushes.DodgerBlue;
            BorderThickness = new Thickness(3);
            CornerRadius = new CornerRadius(10);
            
            txtblk = new TextBlock();
            txtblk.FontSize = 18;
            txtblk.Foreground = SystemColors.ControlTextBrush;
            txtblk.HorizontalAlignment = HorizontalAlignment.Center;
            txtblk.VerticalAlignment = VerticalAlignment.Center;
            txtblk.Focusable = false;
            Child = txtblk;

            Cursor = Cursors.Hand;
            LightInAnima();
        }

        public void LightInAnima()
        {
            DoubleAnimation anima = new DoubleAnimation();
            anima.From = 0;
            anima.To = 1;
            anima.Duration = new Duration(TimeSpan.FromMilliseconds(300));
            anima.FillBehavior = FillBehavior.HoldEnd;

            this.BeginAnimation(OpacityProperty, anima);
        }

        public void BlinkBorderAnima(Color fromColor, Color toColor, int duration, int repeat)
        {
            ColorAnimation anima = new ColorAnimation();
            anima.From = fromColor;
            anima.To = toColor;
            anima.Duration = new Duration(TimeSpan.FromMilliseconds(duration));
            anima.AutoReverse = true;
            anima.RepeatBehavior = RepeatBehavior.Forever;
            Storyboard storyBoard = new Storyboard();
            storyBoard.Duration = new Duration(TimeSpan.FromMilliseconds(duration * repeat));
            storyBoard.Children.Add(anima);
            Storyboard.SetTarget(anima, this);
            Storyboard.SetTargetProperty(anima, new PropertyPath("BorderBrush.Color"));
            storyBoard.Begin();
        }

        public string Text
        {
            set { txtblk.Text = value; }
            get { return txtblk.Text; }
        }

        public Color DefaultBorderColor
        {
            set { defaultBorderColor = value; }
            get { return defaultBorderColor; }
        }

        public Color FocusBorderColor
        {
            set { focusBorderColor = value; }
            get { return focusBorderColor; }
        }


    }
}
