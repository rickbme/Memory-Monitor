using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Memory_Monitor
{
    /// <summary>
    /// Handles Windows Touch input for gesture recognition.
    /// Supports swipe, tap, long-press, and two-finger tap gestures.
    /// Gracefully handles systems without touch support.
    /// </summary>
    public class TouchGestureHandler : IDisposable
    {
        #region Windows Touch API

        private const int WM_TOUCH = 0x0240;
        private const int WM_GESTURE = 0x0119;
        private const int WM_GESTURENOTIFY = 0x011A;

        // Touch event flags
        private const int TOUCHEVENTF_MOVE = 0x0001;
        private const int TOUCHEVENTF_DOWN = 0x0002;
        private const int TOUCHEVENTF_UP = 0x0004;
        private const int TOUCHEVENTF_PRIMARY = 0x0010;

        // Gesture IDs
        private const int GID_BEGIN = 1;
        private const int GID_END = 2;
        private const int GID_ZOOM = 3;
        private const int GID_PAN = 4;
        private const int GID_ROTATE = 5;
        private const int GID_TWOFINGERTAP = 6;
        private const int GID_PRESSANDTAP = 7;

        // Gesture flags
        private const int GF_BEGIN = 0x00000001;
        private const int GF_INERTIA = 0x00000002;
        private const int GF_END = 0x00000004;

        // Gesture configuration flags
        private const int GC_ALLGESTURES = 0x00000001;

        [StructLayout(LayoutKind.Sequential)]
        private struct TOUCHINPUT
        {
            public int x;
            public int y;
            public IntPtr hSource;
            public int dwID;
            public int dwFlags;
            public int dwMask;
            public int dwTime;
            public IntPtr dwExtraInfo;
            public int cxContact;
            public int cyContact;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct GESTUREINFO
        {
            public int cbSize;
            public int dwFlags;
            public int dwID;
            public IntPtr hwndTarget;
            public POINTS ptsLocation;
            public int dwInstanceID;
            public int dwSequenceID;
            public ulong ullArguments;
            public int cbExtraArgs;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct POINTS
        {
            public short x;
            public short y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct GESTURECONFIG
        {
            public int dwID;
            public int dwWant;
            public int dwBlock;
        }

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool RegisterTouchWindow(IntPtr hWnd, uint ulFlags);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnregisterTouchWindow(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetTouchInputInfo(IntPtr hTouchInput, int cInputs,
            [In, Out] TOUCHINPUT[] pInputs, int cbSize);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseTouchInputHandle(IntPtr hTouchInput);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetGestureInfo(IntPtr hGestureInfo, ref GESTUREINFO pGestureInfo);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseGestureInfoHandle(IntPtr hGestureInfo);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetGestureConfig(IntPtr hWnd, int dwReserved, int cIDs,
            [In] GESTURECONFIG[] pGestureConfig, int cbSize);

        #endregion

        private readonly Form _form;
        private readonly int _touchInputSize;
        private bool _isTouchRegistered;
        private bool _disposed;
        private bool _registrationAttempted;

        // Gesture tracking state
        private Point _touchStartPoint;
        private DateTime _touchStartTime;
        private bool _isTouching;
        private int _activeTouchId;

        // Long press timer
        private readonly System.Windows.Forms.Timer _longPressTimer;
        private const int LONG_PRESS_DURATION_MS = 600;

        // Swipe detection thresholds
        private const int SWIPE_THRESHOLD_PIXELS = 80;
        private const int TAP_THRESHOLD_PIXELS = 20;
        private const int TAP_MAX_DURATION_MS = 300;

        /// <summary>
        /// Gets whether touch input is available and registered.
        /// </summary>
        public bool IsTouchAvailable => _isTouchRegistered;

        #region Events

        /// <summary>Raised when a horizontal swipe is detected. Positive = right, Negative = left.</summary>
        public event EventHandler<SwipeEventArgs>? SwipeDetected;

        /// <summary>Raised when a tap is detected on a control.</summary>
        public event EventHandler<TapEventArgs>? TapDetected;

        /// <summary>Raised when a long press is detected.</summary>
        public event EventHandler<LongPressEventArgs>? LongPressDetected;

        /// <summary>Raised when a two-finger tap is detected.</summary>
        public event EventHandler<PointEventArgs>? TwoFingerTapDetected;

        /// <summary>Raised when a pinch/zoom gesture is detected.</summary>
        public event EventHandler<ZoomEventArgs>? ZoomDetected;

        #endregion

        public TouchGestureHandler(Form form)
        {
            _form = form ?? throw new ArgumentNullException(nameof(form));
            _touchInputSize = Marshal.SizeOf(typeof(TOUCHINPUT));

            _longPressTimer = new System.Windows.Forms.Timer
            {
                Interval = LONG_PRESS_DURATION_MS
            };
            _longPressTimer.Tick += LongPressTimer_Tick;

            // Don't register for touch here - wait until the handle is created
            // This prevents forcing early handle creation which can cause ShowInTaskbar issues
            if (_form.IsHandleCreated)
            {
                RegisterForTouch();
                if (_isTouchRegistered)
                {
                    ConfigureGestures();
                }
            }
            else
            {
                // Register when handle is created
                _form.HandleCreated += Form_HandleCreated;
            }
        }

        private void Form_HandleCreated(object? sender, EventArgs e)
        {
            _form.HandleCreated -= Form_HandleCreated;
            RegisterForTouch();
            if (_isTouchRegistered)
            {
                ConfigureGestures();
            }
        }

        private void RegisterForTouch()
        {
            if (_registrationAttempted)
                return;
            
            _registrationAttempted = true;

            try
            {
                if (RegisterTouchWindow(_form.Handle, 0))
                {
                    _isTouchRegistered = true;
                    Debug.WriteLine("Touch input registered successfully");
                }
                else
                {
                    int error = Marshal.GetLastWin32Error();
                    Debug.WriteLine($"Touch registration failed with error code: {error}");
                    _isTouchRegistered = false;
                }
            }
            catch (Exception ex)
            {
                // Touch API may not be available on this system
                Debug.WriteLine($"Touch registration not available: {ex.Message}");
                _isTouchRegistered = false;
            }
        }

        private void ConfigureGestures()
        {
            try
            {
                var configs = new GESTURECONFIG[]
                {
                    new GESTURECONFIG
                    {
                        dwID = 0, // All gestures
                        dwWant = GC_ALLGESTURES,
                        dwBlock = 0
                    }
                };

                if (SetGestureConfig(_form.Handle, 0, configs.Length, configs, Marshal.SizeOf(typeof(GESTURECONFIG))))
                {
                    Debug.WriteLine("Gesture configuration applied successfully");
                }
                else
                {
                    Debug.WriteLine("Gesture configuration failed");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Gesture configuration error: {ex.Message}");
            }
        }

        /// <summary>
        /// Process Windows messages for touch and gesture input.
        /// Call this from the form's WndProc override.
        /// </summary>
        /// <returns>True if the message was handled.</returns>
        public bool ProcessMessage(ref Message m)
        {
            // If touch is not available, don't process touch messages
            if (!_isTouchRegistered)
                return false;

            switch (m.Msg)
            {
                case WM_TOUCH:
                    return HandleTouchMessage(ref m);

                case WM_GESTURE:
                    return HandleGestureMessage(ref m);

                case WM_GESTURENOTIFY:
                    // Allow default gesture handling
                    return false;

                default:
                    return false;
            }
        }

        private bool HandleTouchMessage(ref Message m)
        {
            int inputCount = m.WParam.ToInt32() & 0xFFFF;
            if (inputCount <= 0)
                return false;

            var inputs = new TOUCHINPUT[inputCount];

            if (!GetTouchInputInfo(m.LParam, inputCount, inputs, _touchInputSize))
            {
                return false;
            }

            try
            {
                foreach (var input in inputs)
                {
                    // Convert touch coordinates (in hundredths of pixels) to screen pixels
                    Point screenPoint = new Point(input.x / 100, input.y / 100);
                    Point clientPoint = _form.PointToClient(screenPoint);

                    if ((input.dwFlags & TOUCHEVENTF_DOWN) != 0)
                    {
                        HandleTouchDown(input.dwID, clientPoint);
                    }
                    else if ((input.dwFlags & TOUCHEVENTF_MOVE) != 0)
                    {
                        HandleTouchMove(input.dwID, clientPoint);
                    }
                    else if ((input.dwFlags & TOUCHEVENTF_UP) != 0)
                    {
                        HandleTouchUp(input.dwID, clientPoint);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error processing touch input: {ex.Message}");
            }
            finally
            {
                CloseTouchInputHandle(m.LParam);
            }

            return true;
        }

        private void HandleTouchDown(int touchId, Point location)
        {
            _isTouching = true;
            _activeTouchId = touchId;
            _touchStartPoint = location;
            _touchStartTime = DateTime.Now;

            // Start long press detection
            _longPressTimer.Start();

            Debug.WriteLine($"Touch down at {location}");
        }

        private void HandleTouchMove(int touchId, Point location)
        {
            if (!_isTouching || touchId != _activeTouchId)
                return;

            // Cancel long press if moved too far
            int deltaX = Math.Abs(location.X - _touchStartPoint.X);
            int deltaY = Math.Abs(location.Y - _touchStartPoint.Y);

            if (deltaX > TAP_THRESHOLD_PIXELS || deltaY > TAP_THRESHOLD_PIXELS)
            {
                _longPressTimer.Stop();
            }
        }

        private void HandleTouchUp(int touchId, Point location)
        {
            if (!_isTouching || touchId != _activeTouchId)
                return;

            _longPressTimer.Stop();
            _isTouching = false;

            var duration = DateTime.Now - _touchStartTime;
            int deltaX = location.X - _touchStartPoint.X;
            int deltaY = location.Y - _touchStartPoint.Y;

            // Detect swipe (horizontal movement is more significant than vertical)
            if (Math.Abs(deltaX) > SWIPE_THRESHOLD_PIXELS && Math.Abs(deltaX) > Math.Abs(deltaY) * 2)
            {
                var direction = deltaX > 0 ? SwipeDirection.Right : SwipeDirection.Left;
                OnSwipeDetected(direction, _touchStartPoint, location);
            }
            // Detect vertical swipe
            else if (Math.Abs(deltaY) > SWIPE_THRESHOLD_PIXELS && Math.Abs(deltaY) > Math.Abs(deltaX) * 2)
            {
                var direction = deltaY > 0 ? SwipeDirection.Down : SwipeDirection.Up;
                OnSwipeDetected(direction, _touchStartPoint, location);
            }
            // Detect tap (short duration, small movement)
            else if (duration.TotalMilliseconds < TAP_MAX_DURATION_MS &&
                     Math.Abs(deltaX) < TAP_THRESHOLD_PIXELS &&
                     Math.Abs(deltaY) < TAP_THRESHOLD_PIXELS)
            {
                Control? tappedControl = FindControlAt(_form, location);
                OnTapDetected(location, tappedControl);
            }

            Debug.WriteLine($"Touch up at {location}, delta: ({deltaX}, {deltaY}), duration: {duration.TotalMilliseconds}ms");
        }

        private void LongPressTimer_Tick(object? sender, EventArgs e)
        {
            _longPressTimer.Stop();

            if (_isTouching)
            {
                Control? control = FindControlAt(_form, _touchStartPoint);
                OnLongPressDetected(_touchStartPoint, control);
            }
        }

        private bool HandleGestureMessage(ref Message m)
        {
            var gi = new GESTUREINFO();
            gi.cbSize = Marshal.SizeOf(typeof(GESTUREINFO));

            if (!GetGestureInfo(m.LParam, ref gi))
            {
                return false;
            }

            try
            {
                Point location = new Point(gi.ptsLocation.x, gi.ptsLocation.y);
                Point clientLocation = _form.PointToClient(location);

                switch (gi.dwID)
                {
                    case GID_ZOOM:
                        HandleZoomGesture(gi, clientLocation);
                        return true;

                    case GID_PAN:
                        // Let the touch handler deal with panning
                        return false;

                    case GID_TWOFINGERTAP:
                        OnTwoFingerTapDetected(clientLocation);
                        return true;

                    case GID_ROTATE:
                        // Could implement rotation if needed
                        return true;

                    default:
                        return false;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error processing gesture: {ex.Message}");
                return false;
            }
            finally
            {
                CloseGestureInfoHandle(m.LParam);
            }
        }

        private void HandleZoomGesture(GESTUREINFO gi, Point location)
        {
            if ((gi.dwFlags & GF_BEGIN) != 0)
            {
                Debug.WriteLine("Zoom gesture started");
            }
            else if ((gi.dwFlags & GF_END) != 0)
            {
                Debug.WriteLine("Zoom gesture ended");
            }
            else
            {
                // Calculate zoom factor from ullArguments (distance between fingers)
                double distance = gi.ullArguments;
                OnZoomDetected(location, distance);
            }
        }

        private Control? FindControlAt(Control parent, Point location)
        {
            foreach (Control child in parent.Controls)
            {
                if (child.Visible && child.Bounds.Contains(location))
                {
                    Point relativePoint = new Point(location.X - child.Left, location.Y - child.Top);
                    Control? nested = FindControlAt(child, relativePoint);
                    return nested ?? child;
                }
            }

            return parent == _form ? null : parent;
        }

        #region Event Raisers

        protected virtual void OnSwipeDetected(SwipeDirection direction, Point start, Point end)
        {
            SwipeDetected?.Invoke(this, new SwipeEventArgs(direction, start, end));
        }

        protected virtual void OnTapDetected(Point location, Control? control)
        {
            TapDetected?.Invoke(this, new TapEventArgs(location, control));
        }

        protected virtual void OnLongPressDetected(Point location, Control? control)
        {
            LongPressDetected?.Invoke(this, new LongPressEventArgs(location, control));
        }

        protected virtual void OnTwoFingerTapDetected(Point location)
        {
            TwoFingerTapDetected?.Invoke(this, new PointEventArgs(location));
        }

        protected virtual void OnZoomDetected(Point center, double distance)
        {
            ZoomDetected?.Invoke(this, new ZoomEventArgs(center, distance));
        }

        #endregion

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            _longPressTimer.Stop();
            _longPressTimer.Dispose();

            if (_isTouchRegistered)
            {
                try
                {
                    UnregisterTouchWindow(_form.Handle);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error unregistering touch window: {ex.Message}");
                }
            }
        }
    }

    #region Event Args Classes

    /// <summary>
    /// Swipe direction enumeration.
    /// </summary>
    public enum SwipeDirection
    {
        Left,
        Right,
        Up,
        Down
    }

    /// <summary>
    /// Event arguments for swipe gestures.
    /// </summary>
    public class SwipeEventArgs : EventArgs
    {
        public SwipeDirection Direction { get; }
        public Point StartPoint { get; }
        public Point EndPoint { get; }
        public int Distance => Direction switch
        {
            SwipeDirection.Left or SwipeDirection.Right => Math.Abs(EndPoint.X - StartPoint.X),
            _ => Math.Abs(EndPoint.Y - StartPoint.Y)
        };

        public SwipeEventArgs(SwipeDirection direction, Point start, Point end)
        {
            Direction = direction;
            StartPoint = start;
            EndPoint = end;
        }
    }

    /// <summary>
    /// Event arguments for tap gestures.
    /// </summary>
    public class TapEventArgs : EventArgs
    {
        public Point Location { get; }
        public Control? TappedControl { get; }

        public TapEventArgs(Point location, Control? control)
        {
            Location = location;
            TappedControl = control;
        }
    }

    /// <summary>
    /// Event arguments for long press gestures.
    /// </summary>
    public class LongPressEventArgs : EventArgs
    {
        public Point Location { get; }
        public Control? Control { get; }

        public LongPressEventArgs(Point location, Control? control)
        {
            Location = location;
            Control = control;
        }
    }

    /// <summary>
    /// Event arguments for point-based gestures.
    /// </summary>
    public class PointEventArgs : EventArgs
    {
        public Point Location { get; }

        public PointEventArgs(Point location)
        {
            Location = location;
        }
    }

    /// <summary>
    /// Event arguments for zoom/pinch gestures.
    /// </summary>
    public class ZoomEventArgs : EventArgs
    {
        public Point Center { get; }
        public double Distance { get; }

        public ZoomEventArgs(Point center, double distance)
        {
            Center = center;
            Distance = distance;
        }
    }

    #endregion
}
