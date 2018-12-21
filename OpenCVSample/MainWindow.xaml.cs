using OpenCvSharp;
using OpenCvSharp.Extensions; // これ追加しておく
using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using System.Windows;

namespace OpenCVSample {
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : System.Windows.Window {
        public bool IsExitCapture { get; set; }

        public MainWindow() {
            this.InitializeComponent();
        }

        /// <summary>
        /// カメラ画像を取得して次々に表示を切り替える
        /// </summary>
        public virtual void Capture(object state) {
            var haarCascade = new CascadeClassifier("data/haarcascades/haarcascade_frontalface_default.xml");

            var camera = new VideoCapture(0/*0番目のデバイスを指定*/) {
                // キャプチャする画像のサイズフレームレートの指定
                FrameWidth = 480,
                FrameHeight = 270,
                // Fps = 60
            };
            using (var img = new Mat()) // 撮影した画像を受ける変数
            using (camera) {
                while (true) {
                    if (this.IsExitCapture) {
                        this.Dispatcher.Invoke(() => this._Image.Source = null);
                        break;
                    }


                    camera.Read(img); // Webカメラの読み取り（バッファに入までブロックされる

                    if (img.Empty()) {
                        break;
                    }
                    var result = img.Clone();
                    using (var gray = new Mat()) {
                        Cv2.CvtColor(img, gray, ColorConversionCodes.BGR2GRAY);
                        var faces = haarCascade.DetectMultiScale(
                                    gray,
                                    1.08,
                                    2,
                                    HaarDetectionType.FindBiggestObject,
                                    new OpenCvSharp.Size(50, 50)
                                );
                        foreach (var face in faces) {
                            var center = new OpenCvSharp.Point {
                                X = (int)(face.X + face.Width * 0.5),
                                Y = (int)(face.Y + face.Height * 0.5)
                            };
                            var axes = new OpenCvSharp.Size {
                                Width = (int)(face.Width * 0.5),
                                Height = (int)(face.Height * 0.5)
                            };
                            Cv2.Ellipse(result, center, axes, 0, 0, 360, new Scalar(255, 0, 255), 4);
                        }
                    }

                    this.Dispatcher.Invoke(() => {
                        this._Image.Source = result.ToWriteableBitmap(); // WPFに画像を表示
                    });
                }
            }

        }

        // ---- EventHandlers ----

        /// <summary>
        /// Windowがロードされた時
        /// </summary>
        private void Window_Loaded(object sender, RoutedEventArgs e) {
            ThreadPool.QueueUserWorkItem(this.Capture);
        }

        /// <summary>
        /// Exit Captureボタンが押され時
        /// </summary>
        protected virtual void Button_Click(object sender, RoutedEventArgs e) {
            this.IsExitCapture = true;
        }
    }
}
