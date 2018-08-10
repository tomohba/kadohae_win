using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace 角から生えるやつ
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly DispatcherTimer _mainTimer = new DispatcherTimer();
        private readonly DispatcherTimer _finishTimer = new DispatcherTimer();
        private int _animationFrame = 0;
        private readonly int _animationFrameMax = 15;
        private List<BitmapImage> _animationImages;

        public MainWindow()
        {
            InitializeComponent();

            _animationImages = new List<BitmapImage>();
            for (var i=0; i<=_animationFrameMax; i++)
            {
                var imageNamed = String.Format("pack://application:,,,/images/{0}.png", i);
                var uri = new Uri(imageNamed);
                var bitmap = new BitmapImage(uri);
                _animationImages.Add(bitmap);
            }

            // アニメーション用Timer
            _mainTimer.Interval = TimeSpan.FromMilliseconds(25);
            _mainTimer.Tick += (_, __) =>
            {
                MainAnimation();
            };

            // アニメーション用Timer
            _finishTimer.Interval = TimeSpan.FromMilliseconds(1500);
            _finishTimer.Tick += (_, __) =>
            {
                FinishAnimation();
            };
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            AdjustWindowLocation();

            // マウスカーソルをトラッキング
            MouseHook.MouseMoveAsObservable()
                .Select(point=>point)
                .Where(point=> canStartMainAnimation(point.x, point.y))
                .Subscribe(point =>
            {
                // 条件に合致したらアニメーション開始
                StartMainAnimation();
            });

            MouseHook.StartMouseLowLevelHook();
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            MouseHook.FinishMouseLowLevelHook();
        }

        // DPI値を取得する
        private Point GetDpiScaleFactor()
        {
            var source = PresentationSource.FromVisual(this);
            if (source != null && source.CompositionTarget != null)
            {
                return new Point(
                    source.CompositionTarget.TransformToDevice.M11,
                    source.CompositionTarget.TransformToDevice.M22);
            }

            return new Point(1.0, 1.0);
        }


        // DPI値に応じてマウス座標を修正するロジック
        private void AdjustWindowLocation()
        {
            var factor = GetDpiScaleFactor();
            if (Screen.PrimaryScreen.Bounds.Width == Screen.PrimaryScreen.WorkingArea.Width)
            {
                // タスクバーは上か下
                Left = 0;
                if (Screen.PrimaryScreen.WorkingArea.Top == 0)
                {
                    // タスクバーは下
                    var h = Screen.PrimaryScreen.WorkingArea.Height / factor.Y;
                    Top = (double)h - this.Height;
                }
                else
                {
                    // タスクバーは上
                    var h = Screen.PrimaryScreen.Bounds.Height / factor.Y;
                    Top = (double)h - this.Height;
                }
            }
            else
            {
                // タスクバーは左か右
                var h = Screen.PrimaryScreen.Bounds.Height / factor.Y;
                Top = (double)h - this.Height;
                Left = 0;
                if (Screen.PrimaryScreen.Bounds.Left == 0)
                {
                    // タスクバーは左
                    Left = Screen.PrimaryScreen.WorkingArea.Left;
                }
                else
                {
                    // タスクバーは右
                    Left = 0;
                }
            }
        }

        // アニメーションを開始するかどうかを判定する
        private static bool canStartMainAnimation(int mouse_x, int mouse_y)
        {
            var y = Screen.PrimaryScreen.Bounds.Height - 2;
            var x = 1;

            return (mouse_x < x && mouse_y > y);
        }

        // アニメーション開始
        private void StartMainAnimation()
        {
            if (_mainTimer.IsEnabled == false && _finishTimer.IsEnabled == false)
            {
                AdjustWindowLocation();
                _animationFrame = 0;
                _mainTimer.Start();
            }
        }

        // メインアニメーション処理
        private void MainAnimation()
        {
            Console.WriteLine("MainAnimation _animationFrame:{0} {1}", _animationFrame, _mainTimer);

            KadoImage.Source = _animationImages[_animationFrame];
            KadoImage.Visibility = Visibility.Visible;

            _animationFrame++;
            if (_animationFrame < _animationFrameMax)
            {
                // NOP
                Console.WriteLine("_animationFrame {0}", _animationFrame);
            }
            else
            {
                _mainTimer.Stop();
                StartFinishAnimation();
            }
        }

        // アニメーション終了
        private void StartFinishAnimation()
        {
            _finishTimer.Start();
        }

        // 終了アニメーション処理
        private void FinishAnimation()
        {
            _finishTimer.Stop();
            KadoImage.Visibility = Visibility.Hidden;
            _animationFrame = 0;
            
        }
    }
}
