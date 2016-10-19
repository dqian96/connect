using LightBuzz.Vitruvius.FingerTracking;
using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WpfApplication1
{
    class ViewModel : INotifyPropertyChanged
    {
        string translatedText;

        // no active states before this steady state
        private bool initialState = true;
        // whether the last frame was active or steady
        private bool inActiveState = false;
        // list of past points, including last active state
        private List<Tuple<IList<CameraSpacePoint>, IList<CameraSpacePoint>>> cache = new List<Tuple<IList<CameraSpacePoint>, IList<CameraSpacePoint>>>();
        // counts the number of frames that I've been in steady frame
        private int steadyFrameCount = 0;
        // largest distance allowed between consecutive frames for steady state
        private const double DISTANCE_LIMIT = 0.1;
        // number of frames of "steady" to qualify for steady state
        private const int STEADY_STATE_FRAME_LIMIT = 90;
        // average of the previous contour
        private CameraSpacePoint prevPositionLeftHand;
        private CameraSpacePoint prevPositionRightHand;
        // number of hand-sets to send
        private int FRAMES_TO_SEND = 20;

        // opening all sensors
        public ViewModel()
        {
            this.translatedText = "Hello World";
            _sensor = KinectSensor.GetDefault();
            if (_sensor != null)
            {
                _depthReader = _sensor.DepthFrameSource.OpenReader();
                _depthReader.FrameArrived += DepthReader_FrameArrived;

                _infraredReader = _sensor.InfraredFrameSource.OpenReader();
                _infraredReader.FrameArrived += InfraredReader_FrameArrived;

                _bodyReader = _sensor.BodyFrameSource.OpenReader();
                _bodyReader.FrameArrived += BodyReader_FrameArrived;
                _bodies = new Body[_sensor.BodyFrameSource.BodyCount];

                // Initialize the HandsController and subscribe to the HandsDetected event.
                _handsController = new HandsController();
                _handsController.HandsDetected += HandsController_HandsDetected;

                _sensor.Open();
            }
        }

        // TODO
        public string TranslatedText
        {
            get { return this.translatedText; }
            set
            {
                this.translatedText = value;
                this.OnPropertyChanged("TranslatedText");
            }
        }

        void OnPropertyChanged(string property)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }

        public void Cleanup()
        {
            if (_bodyReader != null)
            {
                _bodyReader.Dispose();
                _bodyReader = null;
            }

            if (_depthReader != null)
            {
                _depthReader.Dispose();
                _depthReader = null;
            }

            if (_infraredReader != null)
            {
                _infraredReader.Dispose();
                _infraredReader = null;
            }

            if (_sensor != null)
            {
                _sensor.Close();
                _sensor = null;
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;

        private KinectSensor _sensor = null;
        private InfraredFrameReader _infraredReader = null;
        private DepthFrameReader _depthReader = null;
        private BodyFrameReader _bodyReader = null;
        private IList<Body> _bodies;
        private Body _body;
        private HandsController _handsController = new HandsController();

        // returns true if current hand positions correspond to an active state
        private bool isActiveState(CameraSpacePoint currPositionLeftHand, CameraSpacePoint currPositionRightHand)
        {
            var distanceLeft = Math.Sqrt(Math.Pow(currPositionLeftHand.X - prevPositionLeftHand.X, 2)
                                + Math.Pow(currPositionLeftHand.Y - prevPositionLeftHand.Y, 2)
                                + Math.Pow(currPositionLeftHand.Z - prevPositionLeftHand.Z, 2));

            var distanceRight = Math.Sqrt(Math.Pow(currPositionRightHand.X - prevPositionRightHand.X, 2)
                                + Math.Pow(currPositionRightHand.Y - prevPositionRightHand.Y, 2)
                                + Math.Pow(currPositionRightHand.Z - prevPositionRightHand.Z, 2));
            Console.WriteLine(distanceLeft);
            Console.WriteLine(distanceRight);

            if (distanceLeft >= DISTANCE_LIMIT || distanceRight >= DISTANCE_LIMIT)
            {
                steadyFrameCount = 0;
                TranslatedText = steadyFrameCount.ToString();
                return true;
            }

            // steady count begins
            steadyFrameCount++;
            TranslatedText = steadyFrameCount.ToString();
            Console.WriteLine(steadyFrameCount);

            if (steadyFrameCount >= STEADY_STATE_FRAME_LIMIT)
            {
                return false;
            }
            return true;

        }

        // called if active state; clears cache and starts cache anew with current position as active state
        private void doActiveState(IList<CameraSpacePoint> currContourLeftHand, IList<CameraSpacePoint> currContourRightHand)
        {
            // nothing should be added to the cache in steady state
            inActiveState = true;
            // TranslatedText = "Active Again";
            cache.Add(new Tuple<IList<CameraSpacePoint>, IList<CameraSpacePoint> >(currContourLeftHand, currContourRightHand));
        }

        private void doSteadyState(CameraSpacePoint currPositionLeft, CameraSpacePoint currPositionRight)
        {
            // just finished active state
            if (inActiveState)
            {
                inActiveState = false;
                // make WEB REQUEST
                Random rnd = new Random();
                using (System.IO.StreamWriter file = new System.IO.StreamWriter((new DateTime()).ToString("ss") + rnd.Next().ToString() + ".arff"))
                {
                    int j = 0;
                    // sampling frames
                    for (int i = 0; i < cache.Count; i += cache.Count / FRAMES_TO_SEND)
                    {
                        if (j == 20) break;
                        // parsing l and r contours
                        Console.Write(i);
                        int incrementLeft = cache[i].Item1.Count / 10;
                        int incrementRight = cache[i].Item2.Count / 10;
                        if (incrementLeft == 0 || incrementRight == 0)
                        {
                            for (int l = 0; l < 10; l ++)
                            {
                                file.Write(currPositionLeft.X.ToString() + ","
                                            + currPositionLeft.Y.ToString() + ","
                                            + currPositionLeft.Z.ToString() + ",");
                            }
                            for (int r = 0; r < 10; r++)
                            {
                                file.Write(currPositionRight.X.ToString() + ","
                                            + currPositionRight.Y.ToString() + ","
                                            + currPositionRight.Z.ToString() + ",");
                            }
                        } else
                        {
                            int a = 0;
                            for (int l = 0; l < cache[i].Item1.Count; l += incrementLeft)
                            {
                                if (a == 10) break;
                                file.Write(cache[i].Item1[l].X.ToString() + ","
                                            + cache[i].Item1[l].Y.ToString() + ","
                                            + cache[i].Item1[l].Z.ToString() + ",");
                                ++a;
                            }
                            a = 0;
                            for (int r = 0; r < cache[i].Item2.Count; r += incrementRight)
                            {
                                if (a == 10) break;
                                file.Write(cache[i].Item2[r].X.ToString() + ","
                                            + cache[i].Item2[r].Y.ToString() + ","
                                            + cache[i].Item2[r].Z.ToString() + ",");
                                ++a;
                            }
                        }
                        file.WriteLine("0");
                        j++;
                    }

                    TranslatedText = "Caching";
                    Console.Write("AT STEADY STATE");
                }

                cache.Clear();
            } else
            {
               
                //prevPositionLeftHand.X = (prevPositionLeftHand.X + currPositionLeft.X) / 2;
                //prevPositionLeftHand.Y = (prevPositionLeftHand.Y + currPositionLeft.Y) / 2;
                //prevPositionLeftHand.Z = (prevPositionLeftHand.Z + currPositionLeft.Z) / 2;

                //prevPositionRightHand.X = (prevPositionRightHand.X + currPositionRight.X) / 2;
                //prevPositionRightHand.Y = (prevPositionRightHand.Y + currPositionRight.Y) / 2;
                //prevPositionRightHand.Z = (prevPositionRightHand.Z + currPositionRight.Z) / 2;

            }
        }
        // calcs the average point
        private CameraSpacePoint getContourAverage(IList<CameraSpacePoint> contourPoints) 
        {
            CameraSpacePoint averagePoint = new CameraSpacePoint();
            averagePoint.X = 0;
            averagePoint.Y = 0;
            averagePoint.Z = 0;
            for (int i = 0; i < contourPoints.Count; i += 3)
            {
                averagePoint.X += contourPoints[i].X;
                averagePoint.Y += contourPoints[i].Y;
                averagePoint.Z += contourPoints[i].Z;
            }
            averagePoint.X /= contourPoints.Count;
            averagePoint.Y /= contourPoints.Count;
            averagePoint.Z /= contourPoints.Count;

            return averagePoint;
        }
        //private void invalidSign()
        //{
        //    if invalid sign....
        //            initialState = true;
        //    cache.Clear();
        //    inActiveState = false;
        //    steadyFrameCount = 0
        //}

        // every frame that hands are detected
        private void HandsController_HandsDetected(object sender, HandCollection e)
        {

            if (e.HandLeft != null)
            {
                // Draw contour.
                foreach (var point in e.HandLeft.ContourDepth)
                {
                    DrawEllipse(point, Brushes.Green, 2.0);
                }

                // Draw fingers.
                foreach (var finger in e.HandLeft.Fingers)
                {
                    DrawEllipse(finger.DepthPoint, Brushes.White, 4.0);
                }
            }

            if (e.HandRight != null)
            {
                // Draw contour.
                foreach (var point in e.HandRight.ContourDepth)
                {
                    DrawEllipse(point, Brushes.Blue, 2.0);
                }

                // Draw fingers.
                foreach (var finger in e.HandRight.Fingers)
                {
                    DrawEllipse(finger.DepthPoint, Brushes.White, 4.0);
                }
            }

            if (e.HandLeft != null && e.HandRight != null)
            {

                // both hands must exist
                CameraSpacePoint leftHandPosition = getContourAverage(e.HandLeft.ContourCamera);
                CameraSpacePoint rightHandPosition = getContourAverage(e.HandRight.ContourCamera);
                if (initialState)
                {
                    prevPositionLeftHand = leftHandPosition;
                    prevPositionRightHand = rightHandPosition;
                    initialState = false;
                }

                if (isActiveState(leftHandPosition, rightHandPosition))
                {
                    doActiveState(e.HandLeft.ContourCamera, e.HandRight.ContourCamera);
                } else
                {
                    doSteadyState(leftHandPosition, prevPositionRightHand);
                }
            }    
        }

        // every frame
        private void DepthReader_FrameArrived(object sender, DepthFrameArrivedEventArgs e)
        {
            canvas.Children.Clear();

            using (DepthFrame frame = e.FrameReference.AcquireFrame())
            {   
                if (frame != null)
                {
                    using (KinectBuffer buffer = frame.LockImageBuffer())
                    {
                        _handsController.Update(buffer.UnderlyingBuffer, _body);
                    }
                }
            }
        }


        static Image camera = null;
        static public Image Camera
        {
            get { return camera; }
            set
            {
                camera = value;
            }
        }
        private void InfraredReader_FrameArrived(object sender, InfraredFrameArrivedEventArgs e)
        {
            using (var frame = e.FrameReference.AcquireFrame())
            {
                if (frame != null)
                {
                    if (camera != null)
                    {
                        camera.Source = frame.ToBitmap();
                    }
                }
            }
        }

        private void BodyReader_FrameArrived(object sender, BodyFrameArrivedEventArgs e)
        {
            using (var bodyFrame = e.FrameReference.AcquireFrame())
            {
                if (bodyFrame != null)
                {
                    bodyFrame.GetAndRefreshBodyData(_bodies);

                    _body = _bodies.Where(b => b.IsTracked).FirstOrDefault();
                }
            }
        }
        static Canvas canvas = null;
        static public Canvas Canvas
        {
            get { return canvas; }
            set
            {
                canvas = value;
            }
        }

        private void DrawEllipse(DepthSpacePoint point, Brush brush, double radius)
        {
            Ellipse ellipse = new Ellipse
            {
                Width = radius,
                Height = radius,
                Fill = brush
            };

            canvas.Children.Add(ellipse);

            Canvas.SetLeft(ellipse, point.X - radius / 2.0);
            Canvas.SetTop(ellipse, point.Y - radius / 2.0);
        }
    }





    // IMAGE GENERATION ----------------------------------------
    internal class InfraredBitmapGenerator
    {
        public int Width { get; protected set; }

        public int Height { get; protected set; }

        public ushort[] InfraredData { get; protected set; }

        public byte[] Pixels { get; protected set; }

        public WriteableBitmap Bitmap { get; protected set; }

        public void Update(InfraredFrame frame)
        {
            if (Bitmap == null)
            {
                Width = frame.FrameDescription.Width;
                Height = frame.FrameDescription.Height;
                InfraredData = new ushort[Width * Height];
                Pixels = new byte[Width * Height * 4];
                Bitmap = new WriteableBitmap(Width, Height, 96.0, 96.0, PixelFormats.Bgr32, null);
            }

            frame.CopyFrameDataToArray(InfraredData);

            int colorIndex = 0;

            for (int infraredIndex = 0; infraredIndex < InfraredData.Length; infraredIndex++)
            {
                ushort ir = InfraredData[infraredIndex];

                byte intensity = (byte)(ir >> 6);

                Pixels[colorIndex++] = intensity; // Blue
                Pixels[colorIndex++] = intensity; // Green   
                Pixels[colorIndex++] = intensity; // Red

                colorIndex++;
            }

            Bitmap.Lock();

            Marshal.Copy(Pixels, 0, Bitmap.BackBuffer, Pixels.Length);
            Bitmap.AddDirtyRect(new Int32Rect(0, 0, Width, Height));

            Bitmap.Unlock();
        }
    }


    internal static class BitmapExtensions
    {
        private static InfraredBitmapGenerator _bitmapGenerator = new InfraredBitmapGenerator();

        public static WriteableBitmap ToBitmap(this InfraredFrame frame)
        {
            _bitmapGenerator.Update(frame);

            return _bitmapGenerator.Bitmap;
        }
    }
}
