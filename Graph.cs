using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Media.Imaging;
using System.IO;

namespace Graph
{
    class Graph : Window
    {
        Canvas canvas;
        Node selectedNode;

        bool isDragging;
        FrameworkElement elDragging;
        Point ptMouseStart, ptElementStart;

        bool drawingLine;
        Line line;

        bool trackingLine;

        FrameworkElement elHitTest;

        [STAThread]
        public static void Main()
        {
            new Application().Run(new Graph());
        }

        public Graph()
        {
            Title = "Graph";
            Content = canvas = new Canvas();
            canvas.Background = Brushes.SkyBlue;
            Uri iconUri = new Uri("pack://application:,,,/icon.ico", UriKind.RelativeOrAbsolute);
            Icon = BitmapFrame.Create(iconUri);
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.ChangedButton == MouseButton.Middle)
            {
                Point pos = e.GetPosition(canvas);
                FrameworkElement el = GetHitedFrameworkElement(pos);

                if (el != null && el is Node)
                {
                    canvas.Children.Remove(el);
                    OnNodeDelete(el as Node);
                }
                else
                {
                    if (selectedNode != null)
                        selectedNode.BlinkBorderAnima(selectedNode.FocusBorderColor, selectedNode.DefaultBorderColor,
                            100, 1);

                    selectedNode = new Node();
                    selectedNode.BorderBrush = new SolidColorBrush(selectedNode.FocusBorderColor);
                    canvas.Children.Add(selectedNode);
                    Canvas.SetZIndex(selectedNode, 1);
                    Canvas.SetLeft(selectedNode, pos.X - selectedNode.Width / 2);
                    Canvas.SetTop(selectedNode, pos.Y - selectedNode.Height / 2);
                }
            }
        }

        FrameworkElement GetHitedFrameworkElement(Point pos)
        {
            VisualTreeHelper.HitTest(canvas,
              new HitTestFilterCallback(MyHitTestFilter),
              new HitTestResultCallback(MyHitTestResult),
              new PointHitTestParameters(pos));

            return elHitTest;
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            ptMouseStart = e.GetPosition(canvas);

            elDragging = GetHitedFrameworkElement(ptMouseStart);

            if (elDragging != null && elDragging is Node)
            {
                Node node = elDragging as Node;
                if (selectedNode != node)
                {
                    selectedNode.BlinkBorderAnima(selectedNode.FocusBorderColor, selectedNode.DefaultBorderColor, 300, 1);
                    selectedNode = node;
                    selectedNode.BlinkBorderAnima(selectedNode.DefaultBorderColor, selectedNode.FocusBorderColor, 300, 1);
                }

                ptElementStart = new Point(Canvas.GetLeft(elDragging), Canvas.GetTop(elDragging));

                CaptureMouse();
                isDragging = true;
            }
            else
            {
                line = new Line();
                line.Stroke = Brushes.Red;
                line.StrokeThickness = 3;
                line.X1 = line.X2 = ptMouseStart.X;
                line.Y1 = line.Y2 = ptMouseStart.Y;
                canvas.Children.Add(line);
                Canvas.SetZIndex(line, 0);
                trackingLine = true;
            }
        }

        protected override void OnMouseRightButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseRightButtonDown(e);

            Point pos = e.GetPosition(canvas);
            FrameworkElement el = GetHitedFrameworkElement(pos);

            if (el != null && el is Node)
            {
                line = new Line();
                line.Stroke = Brushes.Black;
                line.StrokeThickness = 3;
                line.X1 = line.X2 = Canvas.GetLeft(el) + el.Width / 2;
                line.Y1 = line.Y2 = Canvas.GetTop(el) + el.Height / 2; ;
                canvas.Children.Add(line);
                Canvas.SetZIndex(line, 0);

                CaptureMouse();
                drawingLine = true;
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            Point ptMouse = e.GetPosition(canvas);

            if (isDragging)
            {
                double distx = ptMouse.X - ptMouseStart.X;
                double disty = ptMouse.Y - ptMouseStart.Y;

                for (int i = canvas.Children.Count - 1; i >= 0; i--)
                {
                    if (canvas.Children[i] is Line)
                    {
                        line = canvas.Children[i] as Line;
                        if (line.X1 == (Canvas.GetLeft(elDragging) + elDragging.Width / 2) &&
                            line.Y1 == (Canvas.GetTop(elDragging) + elDragging.Height / 2))
                        {
                            line.X1 = ptElementStart.X + distx + elDragging.Width / 2;
                            line.Y1 = ptElementStart.Y + disty + elDragging.Height / 2;
                        }
                        else if (line.X2 == (Canvas.GetLeft(elDragging) + elDragging.Width / 2) &&
                            line.Y2 == (Canvas.GetTop(elDragging) + elDragging.Height / 2))
                        {
                            line.X2 = ptElementStart.X + distx + elDragging.Width / 2;
                            line.Y2 = ptElementStart.Y + disty + elDragging.Height / 2;
                        }
                    }
                }
                Canvas.SetLeft(elDragging, ptElementStart.X + distx);
                Canvas.SetTop(elDragging, ptElementStart.Y + disty);
            }
            else if (drawingLine)
            {
                line.X2 = ptMouse.X;
                line.Y2 = ptMouse.Y;
            }
            else if (trackingLine)
            {
                line.X2 = ptMouse.X;
                line.Y2 = ptMouse.Y;
            }
        }

        bool LineSegmentIntersection(double Ax, double Ay, double Bx, double By, double Cx, double Cy, double Dx, double Dy)
        {
            double distAB, theCos, theSin, newX, posAB;

            if (Ax == Bx && Ay == By || Cx == Dx && Cy == Dy)
                return false;

            Bx -= Ax; By -= Ay;
            Cx -= Ax; Cy -= Ay;
            Dx -= Ax; Dy -= Ay;

            distAB = Math.Sqrt(Bx * Bx + Bx * Bx);
            theCos = Bx / distAB;
            theSin = By / distAB;
            newX = Cx * theCos + Cy * theSin;
            Cy = Cy * theCos - Cx * theSin; Cx = newX;
            newX = Dx * theCos + Dy * theSin;
            Dy = Dy * theCos - Dx * theSin; Dx = newX;

            if (Cy < 0 && Dy < 0 || Cy >= 0 && Dy >= 0)
                return false;

            posAB = Dx + (Cx - Dx) * Dy / (Dy - Cy);

            if (posAB < 0 || posAB > distAB)
                return false;

            return true;
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);

            Point ptMouse = e.GetPosition(canvas);

            if (isDragging)
            {
                isDragging = false;
                ReleaseMouseCapture();
            }
            else if (trackingLine)
            {
                trackingLine = false;
                canvas.Children.Remove(line);
                if (ptMouse == ptMouseStart)
                    return;

                for (int i = canvas.Children.Count - 1; i >= 0; i--)
                {
                    if (canvas.Children[i] is Line)
                    {
                        line = canvas.Children[i] as Line;
                        if (LineSegmentIntersection(ptMouseStart.X, ptMouseStart.Y, ptMouse.X, ptMouse.Y, line.X1, line.Y1, line.X2, line.Y2))
                            canvas.Children.RemoveAt(i);
                    }
                }
            }
        }

        protected override void OnMouseRightButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseRightButtonUp(e);

            Point pos = e.GetPosition(canvas);
            FrameworkElement el = GetHitedFrameworkElement(pos);

            if (el != null && el is Node)
            {
                line.X2 = Canvas.GetLeft(el) + el.Width / 2;
                line.Y2 = Canvas.GetTop(el) + el.Height / 2; ;
            }
            else
            {
                canvas.Children.Remove(line);
            }
            drawingLine = false;
            ReleaseMouseCapture();
        }

        public HitTestResultBehavior MyHitTestResult(HitTestResult result)
        {
            elHitTest = result.VisualHit as FrameworkElement;

            return HitTestResultBehavior.Stop;
        }

        public HitTestFilterBehavior MyHitTestFilter(DependencyObject o)
        {
            if (o.GetType() == typeof(TextBlock))
                return HitTestFilterBehavior.ContinueSkipSelf;
            else
                return HitTestFilterBehavior.Continue;
        }

        protected override void OnTextInput(TextCompositionEventArgs args)
        {
            base.OnTextInput(args);

            if (args.Text == "\x1B")
            {
                if (isDragging)
                {
                    Canvas.SetLeft(elDragging, ptElementStart.X);
                    Canvas.SetTop(elDragging, ptElementStart.Y);

                    isDragging = false;
                    ReleaseMouseCapture();
                }
            }
            else if (args.Text == "\x8" && selectedNode.Text.Length > 0)
                selectedNode.Text = selectedNode.Text.Substring(0, selectedNode.Text.Length - 1);
            else
            {
                selectedNode.Text += args.Text;
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (e.Key == Key.Delete)
            {
                canvas.Children.Remove(selectedNode);
                OnNodeDelete(selectedNode);
            }
            //if (e.Key == Key.S)
            //    SaveCanvasToFile(canvas, "c:/a.bmp");
        }

        void OnNodeDelete(Node node)
        {
            for (int i = canvas.Children.Count - 1; i >= 0; i--)
            {
                if (canvas.Children[i] is Node)
                {
                    selectedNode = canvas.Children[i] as Node;
                    selectedNode.BlinkBorderAnima(selectedNode.DefaultBorderColor, selectedNode.FocusBorderColor, 300, 1);
                    break;
                }
                else if (canvas.Children[i] is Line)
                {
                    line = canvas.Children[i] as Line;
                    if (line.X1 == (Canvas.GetLeft(node) + node.Width / 2) &&
                        line.Y1 == (Canvas.GetTop(node) + node.Height / 2) ||
                        line.X2 == (Canvas.GetLeft(node) + node.Width / 2) &&
                        line.Y2 == (Canvas.GetTop(node) + node.Height / 2))
                        canvas.Children.Remove(line);
                }
            }
        }

        public static void SaveCanvasToFile(Canvas surface, string filename)
        {
            Size size = new Size(surface.ActualWidth, surface.ActualHeight);

            surface.Measure(size);
            surface.Arrange(new Rect(size));

            RenderTargetBitmap renderBitmap =
              new RenderTargetBitmap(
                (int)size.Width,
                (int)size.Height,
                96d,
                96d,
                PixelFormats.Pbgra32);
            renderBitmap.Render(surface);

            // Create a file stream for saving image
            using (FileStream outStream = new FileStream(filename, FileMode.Create))
            {
                BmpBitmapEncoder encoder = new BmpBitmapEncoder();
                // push the rendered bitmap to it
                encoder.Frames.Add(BitmapFrame.Create(renderBitmap));
                // save the data to the stream
                encoder.Save(outStream);
            }
        }


    }
}
