using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace WpfApp1
{
    public partial class MainWindow : Window
    {
        private bool[,]? field;               //true черная клетка
        private int width, height;
        private int antX, antY, direction;    //0вверх, 1вправо, 2вниз, 3влево

        private readonly List<string> whiteRules = new();
        private readonly List<string> blackRules = new();

        private DispatcherTimer? timer;
        private bool isRunning;

        private double cellSize = 23;
        private readonly List<Rectangle> cells = new();
        private Polygon? antArrow;

        //для анимации
        private DispatcherTimer? animTimer;
        private double animX, animY, targetX, targetY;
        private double animProgress;
        private bool isAnimating;

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

            //заполняем выпадающие списки
            foreach (var a in Actions)
            {
                CmbWhiteAction.Items.Add(a);
                CmbBlackAction.Items.Add(a);
            }
            CmbWhiteAction.SelectedIndex = 0;
            CmbBlackAction.SelectedIndex = 0;

            //правила по умолчанию
            whiteRules.AddRange(new[] { "Повернуть направо", "Перекрасить клетку" });
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

        //генерация поля
        private void BtnGenerate_Click(object sender, RoutedEventArgs e)
        {
            Stop();

            if (!int.TryParse(TxtWidth.Text, out width) || width < 5 || width > 50 ||
                !int.TryParse(TxtHeight.Text, out height) || height < 5 || height > 50)
            {
                MessageBox.Show("Размер поля должен быть от 5 до 50");
                return;
            }

            var rnd = new Random();
            field = new bool[width, height];

            //случайные клетки
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    field[x, y] = rnd.NextDouble() < 0.3;

            //позиция муравья
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
            //автоматически подгоняем размер клетки под размеры правой панели
            double panelWidth = ((FrameworkElement)FieldCanvas.Parent).ActualWidth - 30;
            double panelHeight = ((FrameworkElement)FieldCanvas.Parent).ActualHeight - 30;

            if (panelWidth < 100) panelWidth = 600; //на случай если панель еще не прорисовалась
            if (panelHeight < 100) panelHeight = 600;

            //вычисляем размер клетки под минимум
            cellSize = Math.Max(5, Math.Min(panelWidth / width, panelHeight / height));

            direction = rnd.Next(4);
            DrawField();
        }

        private void DrawField()
        {
            FieldCanvas.Children.Clear();
            cells.Clear();
            antArrow = null;

            if (field == null) return;

            FieldCanvas.Width = width * cellSize;
            FieldCanvas.Height = height * cellSize;

            //клетки
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var rect = new Rectangle
                    {
                        Width = cellSize - 1,
                        Height = cellSize - 1,
                        Fill = field[x, y] ? Brushes.Black : Brushes.White,
                        Stroke = Brushes.LightGray,
                        StrokeThickness = 0.5
                    };
                    Canvas.SetLeft(rect, x * cellSize);
                    Canvas.SetTop(rect, y * cellSize);
                    FieldCanvas.Children.Add(rect);
                    cells.Add(rect);
                }
            }

            DrawAnt(antX * cellSize, antY * cellSize);
        }

        //рисует красную стрелку
        private void DrawAnt(double left, double top)
        {
            if (antArrow != null)
                FieldCanvas.Children.Remove(antArrow);

            double cx = left + cellSize / 2;
            double cy = top + cellSize / 2;
            double s = cellSize * 0.35;

            Point[] pts = direction switch
            {
                0 => new[] { new Point(cx, cy - s), new Point(cx - s * 0.7, cy + s * 0.6), new Point(cx + s * 0.7, cy + s * 0.6) }, // вверх
                1 => new[] { new Point(cx + s, cy), new Point(cx - s * 0.6, cy - s * 0.7), new Point(cx - s * 0.6, cy + s * 0.7) }, // вправо
                2 => new[] { new Point(cx, cy + s), new Point(cx - s * 0.7, cy - s * 0.6), new Point(cx + s * 0.7, cy - s * 0.6) }, // вниз
                _ => new[] { new Point(cx - s, cy), new Point(cx + s * 0.6, cy - s * 0.7), new Point(cx + s * 0.6, cy + s * 0.7) }  // влево
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

        private void UpdateCell(int x, int y)
        {
            if (field == null) return;
            int index = y * width + x;
            if (index >= 0 && index < cells.Count)
                cells[index].Fill = field[x, y] ? Brushes.Black : Brushes.White;
        }

        private void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            if (field == null)
            {
                MessageBox.Show("Сначала нажмите «Сгенерировать»");
                return;
            }

            isRunning = true;
            BtnStart.IsEnabled = false;
            BtnPause.IsEnabled = true;
            BtnGenerate.IsEnabled = false;

            timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(250) };
            timer.Tick += OnStep;
            timer.Start();
        }

        private void BtnPause_Click(object sender, RoutedEventArgs e) => Stop();

        private void Stop()
        {
            isRunning = false;
            timer?.Stop();
            timer = null;
            animTimer?.Stop();
            animTimer = null;
            isAnimating = false;

            BtnStart.IsEnabled = true;
            BtnPause.IsEnabled = false;
            BtnGenerate.IsEnabled = true;
        }

        //один шаг
        private void OnStep(object? sender, EventArgs e)
        {
            if (isAnimating || field == null) return;

            //правила в зависимости от цвета
            var rules = field[antX, antY] ? blackRules : whiteRules;

            bool needMove = false;
            int turn = 0;

            foreach (var rule in rules)
            {
                if (rule == "Повернуть направо") turn = (turn + 1) % 4;
                else if (rule == "Повернуть налево") turn = (turn + 3) % 4;
                else if (rule == "Перекрасить клетку")
                {
                    field[antX, antY] = !field[antX, antY];
                    UpdateCell(antX, antY);
                }
                else if (rule == "Идти прямо") needMove = true;
            }

            //поворот
            direction = (direction + turn) % 4;
            DrawAnt(antX * cellSize, antY * cellSize);

            if (!needMove) return;

            //новая позиция
            int nx = antX, ny = antY;
            if (direction == 0) ny--;
            else if (direction == 1) nx++;
            else if (direction == 2) ny++;
            else nx--;

            //через край с другой стороны
            if (nx < 0) nx = width - 1;
            if (nx >= width) nx = 0;
            if (ny < 0) ny = height - 1;
            if (ny >= height) ny = 0;

            //если перешли через край сразу телепорт иначе анимация
            if (Math.Abs(nx - antX) > 1 || Math.Abs(ny - antY) > 1)
            {
                antX = nx;
                antY = ny;
                DrawAnt(antX * cellSize, antY * cellSize);
            }
            else
            {
                StartAnimation(antX, antY, nx, ny);
            }
        }

        //анимация
        private void StartAnimation(int fromX, int fromY, int toX, int toY)
        {
            isAnimating = true;
            animX = fromX * cellSize;
            animY = fromY * cellSize;
            targetX = toX * cellSize;
            targetY = toY * cellSize;
            animProgress = 0;

            antX = toX;
            antY = toY;

            animTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
            animTimer.Tick += OnAnim;
            animTimer.Start();
        }

        private void OnAnim(object? sender, EventArgs e)
        {
            animProgress += 0.15;
            if (animProgress >= 1)
            {
                animTimer?.Stop();
                isAnimating = false;
                DrawAnt(antX * cellSize, antY * cellSize);
                return;
            }

            double x = animX + (targetX - animX) * animProgress;
            double y = animY + (targetY - animY) * animProgress;
            DrawAnt(x, y);
        }
    }
}
