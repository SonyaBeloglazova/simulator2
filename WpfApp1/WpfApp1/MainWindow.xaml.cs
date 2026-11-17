using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace WpfApp1
{
    public partial class MainWindow : Window
    {
        private bool[,]? field;
        private int width, height;
        private int antX, antY, direction;

        private readonly List<string> whiteRules = new();
        private readonly List<string> blackRules = new();

        private const double CellSize = 20;
        private readonly List<Rectangle> cells = new();
        private Polygon? antArrow;

        private static readonly string[] Actions =
        {
            "Повернуть направо",
            "Повернуть налево",
            "Идти прямо",
            "Перекрасить клетку"
        };

        public MainWindow()
        {
            InitializeComponent();

            foreach (var a in Actions)
            {
                CmbWhiteAction.Items.Add(a);
                CmbBlackAction.Items.Add(a);
            }
            CmbWhiteAction.SelectedIndex = 0;
            CmbBlackAction.SelectedIndex = 0;

            whiteRules.AddRange(new[] { "Повернуть направо", "Перекрасить клетку", "Идти прямо" });
            blackRules.AddRange(new[] { "Повернуть налево", "Перекрасить клетку", "Идти прямо" });
            UpdateRuleLists();
        }

        private void UpdateRuleLists()
        {
            ListWhiteActions.Items.Clear();
            foreach (var r in whiteRules) ListWhiteActions.Items.Add(r);

            ListBlackActions.Items.Clear();
            foreach (var r in blackRules) ListBlackActions.Items.Add(r);
        }

        private void BtnAddWhite_Click(object sender, RoutedEventArgs e)
        {
            if (CmbWhiteAction.SelectedItem is string action)
            {
                whiteRules.Add(action);
                UpdateRuleLists();
            }
        }

        private void BtnRemoveWhite_Click(object sender, RoutedEventArgs e)
        {
            int i = ListWhiteActions.SelectedIndex;
            if (i >= 0) { whiteRules.RemoveAt(i); UpdateRuleLists(); }
        }

        private void BtnAddBlack_Click(object sender, RoutedEventArgs e)
        {
            if (CmbBlackAction.SelectedItem is string action)
            {
                blackRules.Add(action);
                UpdateRuleLists();
            }
        }

        private void BtnRemoveBlack_Click(object sender, RoutedEventArgs e)
        {
            int i = ListBlackActions.SelectedIndex;
            if (i >= 0) { blackRules.RemoveAt(i); UpdateRuleLists(); }
        }

        private void BtnGenerate_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(TxtWidth.Text, out width) || width < 5 || width > 50 ||
                !int.TryParse(TxtHeight.Text, out height) || height < 5 || height > 50)
            {
                MessageBox.Show("Размер поля должен быть от 5 до 50");
                return;
            }

            var rnd = new Random();
            field = new bool[width, height];

            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    field[x, y] = rnd.NextDouble() < 0.3;

            if (int.TryParse(TxtStartX.Text, out int sx) && int.TryParse(TxtStartY.Text, out int sy) &&
                sx >= 0 && sx < width && sy >= 0 && sy < height)
            {
                antX = sx;
                antY = sy;
            }
            else
            {
                antX = rnd.Next(width);
                antY = rnd.Next(height);
                TxtStartX.Text = antX.ToString();
                TxtStartY.Text = antY.ToString();
            }

            direction = rnd.Next(4);
            DrawField();
        }

        private void DrawField()
        {
            FieldCanvas.Children.Clear();
            cells.Clear();
            antArrow = null;

            if (field == null) return;

            FieldCanvas.Width = width * CellSize;
            FieldCanvas.Height = height * CellSize;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var rect = new Rectangle
                    {
                        Width = CellSize - 1,
                        Height = CellSize - 1,
                        Fill = field[x, y] ? Brushes.Black : Brushes.White,
                        Stroke = Brushes.LightGray,
                        StrokeThickness = 0.5
                    };
                    Canvas.SetLeft(rect, x * CellSize);
                    Canvas.SetTop(rect, y * CellSize);
                    FieldCanvas.Children.Add(rect);
                    cells.Add(rect);
                }
            }

            DrawAnt(antX * CellSize, antY * CellSize);
        }

        private void DrawAnt(double left, double top)
        {
            if (antArrow != null)
                FieldCanvas.Children.Remove(antArrow);

            double cx = left + CellSize / 2;
            double cy = top + CellSize / 2;
            double s = CellSize * 0.35;

            Point[] pts = direction switch
            {
                0 => new[] { new Point(cx, cy - s), new Point(cx - s * 0.7, cy + s * 0.6), new Point(cx + s * 0.7, cy + s * 0.6) },
                1 => new[] { new Point(cx + s, cy), new Point(cx - s * 0.6, cy - s * 0.7), new Point(cx - s * 0.6, cy + s * 0.7) },
                2 => new[] { new Point(cx, cy + s), new Point(cx - s * 0.7, cy - s * 0.6), new Point(cx + s * 0.7, cy - s * 0.6) },
                _ => new[] { new Point(cx - s, cy), new Point(cx + s * 0.6, cy - s * 0.7), new Point(cx + s * 0.6, cy + s * 0.7) }
            };

            antArrow = new Polygon
            {
                Points = new PointCollection(pts),
                Fill = Brushes.Red,
                Stroke = Brushes.DarkRed,
                StrokeThickness = 1
            };
            FieldCanvas.Children.Add(antArrow);
        }

        private void BtnStart_Click(object sender, RoutedEventArgs e) { }
        private void BtnPause_Click(object sender, RoutedEventArgs e) { }
    }
}
