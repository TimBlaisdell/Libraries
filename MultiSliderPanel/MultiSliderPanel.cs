using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace MSP {
    public partial class MultiSliderPanel : UserControl {
        public MultiSliderPanel() {
            InitializeComponent();
        }
        public int Accelleration { get; set; } = 2;
        public int AnimationSleepMS { get; set; }
        public override Image BackgroundImage {
            get => base.BackgroundImage;
            set {
                base.BackgroundImage = value;
                if (value == null) _bgImage = null;
                else {
                    if (_bgImage == null || _bgImage.Width != Width || _bgImage.Height != Height) _bgImage = new Bitmap(Width, Height);
                    using (var gfx = Graphics.FromImage(_bgImage)) {
                        gfx.FillRectangle(new SolidBrush(BackColor), 0, 0, Width, Height);
                        switch (BackgroundImageLayout) {
                            case ImageLayout.None:
                                gfx.DrawImage(BackgroundImage, 0, 0);
                                break;
                            case ImageLayout.Tile:
                                for (int y = 0; y < Height; y += BackgroundImage.Height) {
                                    for (int x = 0; x < Width; x += BackgroundImage.Width) {
                                        gfx.DrawImage(BackgroundImage, x, y);
                                    }
                                }
                                break;
                            case ImageLayout.Center:
                                gfx.DrawImage(BackgroundImage,
                                              new RectangleF((Width - BackgroundImage.Width) / 2F, (Height - BackgroundImage.Height) / 2F, BackgroundImage.Width, BackgroundImage.Height),
                                              new RectangleF(0, 0, BackgroundImage.Width, BackgroundImage.Height),
                                              GraphicsUnit.Pixel);
                                break;
                            case ImageLayout.Zoom:
                                var sz = new SizeF(Width, Width * (BackgroundImage.Height / (float) BackgroundImage.Width));
                                if (sz.Height > Height) sz = new SizeF(Height * (BackgroundImage.Width / (float) BackgroundImage.Height), Height);
                                gfx.DrawImage(BackgroundImage, new RectangleF(new PointF((Width - sz.Width) / 2F, (Height - sz.Height) / 2F), sz));
                                break;
                            case ImageLayout.Stretch:
                                gfx.DrawImage(BackgroundImage, new Rectangle(0, 0, Width, Height));
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                }
            }
        }
        public override ImageLayout BackgroundImageLayout {
            get => base.BackgroundImageLayout;
            set {
                if (value == base.BackgroundImageLayout) return;
                base.BackgroundImageLayout = value;
                BackgroundImage = base.BackgroundImage; // force it to regenerate _bgImage
            }
        }
        public int BorderWidth { get; set; } = 0;
        public Control Current {
            get {
                lock (_stack) return _stack.Any() ? _stack[0] : null;
            }
        }
        public bool DelaySlideMain { get; set; }
        public int InitialSpeed { get; set; } = 1;
        public Size InnerSize => new Size(Width - BorderWidth * 2, Height - BorderWidth * 2);
        public Control Main {
            get => _main;
            set {
                lock (_stack) {
                    //if (_controls.Any() || _stack.Any()) throw new Exception("You must set the main control before adding any sliding controls.");
                    _main = value;
                    if (_main == null) return;
                    if (DelaySlideMain) {
                        _main.Dock = DockStyle.None;
                        _main.Anchor = AnchorStyles.Top | AnchorStyles.Left;
                        _main.Visible = false;
                    }
                    else {
                        if (BorderWidth == 0) {
                            _main.Dock = DockStyle.Fill;
                        }
                        else {
                            _main.Dock = DockStyle.None;
                            _main.Location = new Point(BorderWidth, BorderWidth);
                            _main.Size = new Size(Width - BorderWidth * 2, Height - BorderWidth * 2);
                            _main.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom;
                        }
                        _main.Visible = true;
                        _stack.Add(_main);
                    }
                }
                Controls.Add(_main);
            }
        }
        public bool Sliding { get; private set; }
        public bool StopAnimation { get; set; }
        public bool ZOrderCorrection { get; set; }
        public event EventHandler<Control> AboutToSlide;
        public event EventHandler SlidingComplete;
        public void AddSlider(Control control, bool fromright, bool noBitmapDrawing = false) {
            //if (_main == null) throw new Exception("You must set the main control before adding any sliding controls.");
            control.Dock = DockStyle.None;
            control.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            control.Visible = false;
            Controls.Add(control);
            _controls.Add(new ControlInfo {Control = control, FromRight = fromright, NoBitmapDrawing = noBitmapDrawing});
        }
        public void SetEntry(Control control, bool fromRight) {
            if (ReferenceEquals(control, _main)) _mainEntersFromRight = fromRight;
            else {
                var info = _controls.First(i => ReferenceEquals(control, i.Control));
                info.FromRight = fromRight;
            }
        }
        public void SetNoBitmapDrawing(Control c, bool noBitmapDrawing) {
            var info = _controls.First(i => ReferenceEquals(c, i.Control));
            info.NoBitmapDrawing = noBitmapDrawing;
        }
        /// <summary>
        ///     Slides back to the previos panel on the stack.
        /// </summary>
        public void SlideBack(bool toMain = false) {
            if (ReferenceEquals(Current, _main) || Sliding) return;
            ControlInfo curInfo;
            lock (_stack) {
                if (_stack.Count < 2) throw new Exception("Cannot slide back because there's nothing to slide back to.");
                curInfo = _controls.Find(i => ReferenceEquals(i.Control, _stack[0]));
            }
            var info = new ControlInfo {Control = toMain ? _main : _stack[1], FromRight = !curInfo.FromRight};
            Sliding = true;
            new Thread(SlideThread).Start(info);
        }
        public void SlideMain(bool fromRight) {
            if (ReferenceEquals(Current, _main) || Sliding) return;
            lock (_stack) {
                if (_stack.Count != 0) throw new Exception("Cannot slide main control on while showing another control.");
            }
            var info = new ControlInfo {Control = _main, FromRight = fromRight};
            Sliding = true;
            new Thread(SlideThread).Start(info);
        }
        /// <summary>
        ///     Slides to one of the sub-panels.  Do not use with main panel.  Use SlideBack for that.
        /// </summary>
        public void SlideTo(Control control) {
            if (ReferenceEquals(Current, control) || Sliding) return;
            lock (_stack) {
                // if they said to slide to the control that's back one on the stack, just call SlideBack and leave.
                if (_stack.Count > 1 && ReferenceEquals(_stack[1], control)) {
                    SlideBack();
                    return;
                }
                if (_stack.Any(p => ReferenceEquals(p, control)))
                    throw new Exception("Cannot slide to a control that's already on the stack.");
            }
            var info = _controls.First(i => ReferenceEquals(control, i.Control));
            Sliding = true;
            new Thread(SlideThread).Start(info);
        }
        protected override void OnPaint(PaintEventArgs e) {
            if (RunningInDesigner()) {
                base.OnPaint(e);
                string s = string.IsNullOrWhiteSpace(Name) ? "MultiSliderPanel" : Name;
                e.Graphics.DrawRectangle(Pens.Gray, ClientRectangle);
                e.Graphics.DrawString(s, Font, Brushes.Black, ClientRectangle);
                return;
            }
            if (_drawBmps && Height > 0 && Width > 0) {
                if (_canvas == null || _canvas.Width != Width || _canvas.Height != Height) _canvas = new Bitmap(Width, Height);
                using (var gfx = Graphics.FromImage(_canvas)) {
                    if (_bgImage != null) gfx.DrawImage(_bgImage, 0, 0);
                    else gfx.FillRectangle(new SolidBrush(BackColor), 0, 0, Width, Height);
                    if (_toBmp != null) gfx.DrawImage(_toBmp, _curLeft, BorderWidth);
                    if (_fromBmp != null) gfx.DrawImage(_fromBmp, _fromRight ? _curLeft - Width : _curLeft + Width, BorderWidth);
                }
                e.Graphics.DrawImage(_canvas, 0, 0);
            }
        }
        protected override void OnPaintBackground(PaintEventArgs e) {
            if (!_drawBmps) base.OnPaintBackground(e);
        }
        protected override void OnSizeChanged(EventArgs e) {
            base.OnSizeChanged(e);
            if (Height == 0 || Width == 0) return;
            BackgroundImage = base.BackgroundImage; // force it to regenerate _bgImage
        }
        private void ReverseZOrder(Control control) {
            if (control == null) return;
            if (control.Controls.IsReadOnly) return; // can't do anything about this one (it's a built-in .NET control like SplitterPanel).
            foreach (Control c in control.Controls) {
                c.BringToFront();
                ReverseZOrder(c);
            }
        }
        private bool RunningInDesigner() {
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime) return true;
            using (Process process = Process.GetCurrentProcess()) {
                if (process.ProcessName == "devenv") return true;
            }
            return false;
        }
        private void SlideThread(object obj) {
            var toCtlInfo = (ControlInfo) obj;
            var toCtl = toCtlInfo.Control;
            Control fromCtl;
            lock (_stack) fromCtl = _stack.Any() ? _stack[0] : null;
            if (fromCtl == toCtl) throw new Exception("Cannot slide from and to the same control.");
            var fromCtlInfo = fromCtl == null ? null : _controls.FirstOrDefault(i => i.Control == fromCtl);
            bool fromDrawToBitmap = fromCtlInfo == null ? true : !fromCtlInfo.NoBitmapDrawing;
            bool toDrawToBitmap = !toCtlInfo.NoBitmapDrawing;
            Size innerSize = InnerSize;
            _fromBmp = fromCtl == null || !fromDrawToBitmap ? null : new Bitmap(innerSize.Width, innerSize.Height);
            _toBmp = toDrawToBitmap ? new Bitmap(innerSize.Width, innerSize.Height) : null;
            _curLeft = toCtlInfo.FromRight ? Width + BorderWidth : -1 * Width + BorderWidth;
            _fromRight = toCtlInfo.FromRight;
            InvokeIfRequired(this,
                             () => {
                                 // Draw the from panel to fromBmp -- it's already on-screen and visible.
                                 if (fromDrawToBitmap && fromCtl != null) {
                                     if (ZOrderCorrection) {
                                         SuspendDrawing(fromCtl);
                                         ReverseZOrder(fromCtl);
                                     }
                                     fromCtl.DrawToBitmap(_fromBmp ?? new Bitmap(0, 0), new Rectangle(0, 0, innerSize.Width, innerSize.Height));
                                     if (ZOrderCorrection) {
                                         ReverseZOrder(fromCtl);
                                         ResumeDrawing(fromCtl);
                                     }
                                 }
                                 // Move the to panel off screen, make it visible, and draw it to toBmp.  Then hide it again.
                                 toCtl.Location = new Point(toCtlInfo.FromRight ? Width + BorderWidth : -1 * (Width + BorderWidth), BorderWidth);
                                 toCtl.Size = innerSize;
                                 AboutToSlide?.Invoke(this, toCtl);
                                 if (toDrawToBitmap) {
                                     toCtl.Visible = true;
                                     if (ZOrderCorrection) {
                                         SuspendDrawing(toCtl);
                                         ReverseZOrder(toCtl);
                                     }
                                     toCtl.DrawToBitmap(_toBmp, new Rectangle(0, 0, innerSize.Width, innerSize.Height));
                                     if (ZOrderCorrection) {
                                         ReverseZOrder(toCtl);
                                         ResumeDrawing(toCtl);
                                     }
                                     //pboxTo.Image = toBmp;
                                     //pboxTo.Location = new Point(toCtlInfo.FromRight ? Width + BorderWidth : -1 * (Width + BorderWidth), BorderWidth);
                                     //pboxTo.Size = innerSize;
                                     //pboxTo.Visible = true;
                                 }
                                 _drawBmps = true;
                                 toCtl.Visible = !toDrawToBitmap;
                                 if (fromCtl != null) {
                                     fromCtl.Visible = !fromDrawToBitmap;
                                     //if (fromDrawToBitmap) {
                                     //    //pboxFrom.Image = fromBmp;
                                     //    //pboxFrom.Location = new Point(BorderWidth, BorderWidth);
                                     //    //pboxFrom.Size = innerSize;
                                     //    //pboxFrom.Visible = true;
                                     //    // Hide the from panel.
                                     //    fromCtl.Visible = false;
                                     //}
                                     fromCtl.Dock = DockStyle.None;
                                     fromCtl.Anchor = AnchorStyles.Left | AnchorStyles.Top;
                                 }
                             });
            int speed = InitialSpeed;
            int accel = Accelleration;
            int sign = toCtlInfo.FromRight ? -1 : 1;
            bool done = false;
            do {
                if (StopAnimation) {
                    Sliding = false;
                    return; // abort, program is closing down.
                }
                int move = speed;
                //int left = toDrawToBitmap ? pboxTo.Left : toCtl.Left;
                if (Math.Abs(_curLeft - BorderWidth) < move) move = Math.Abs(_curLeft - BorderWidth);
                move *= sign;
                _curLeft += move;
                if (fromCtl != null && !fromDrawToBitmap || !toDrawToBitmap) {
                    InvokeIfRequired(this,
                                     () => {
                                         //if (fromCtl != null) {
                                         //    if (fromDrawToBitmap) pboxFrom.Left += move;
                                         //    else fromCtl.Left += move;
                                         //}
                                         //if (toDrawToBitmap) pboxTo.Left += move;
                                         //else toCtl.Left += move;
                                         if (fromCtl != null && !fromDrawToBitmap) fromCtl.Left += move;
                                         if (!toDrawToBitmap) toCtl.Left += move;
                                     });
                }
                Invalidate();
                Thread.Sleep(AnimationSleepMS);
                //left = toDrawToBitmap ? pboxTo.Left : toCtl.Left;
                if (_curLeft == BorderWidth) done = true;
                speed += accel;
                accel += Accelleration;
            } while (!done);
            lock (_stack) {
                if (!_stack.Any(p => ReferenceEquals(p, toCtl))) _stack.Insert(0, toCtl);
                else {
                    do {
                        _stack.RemoveAt(0);
                    } while (_stack[0] != toCtl);
                }
            }
            InvokeIfRequired(this,
                             () => {
                                 if (BorderWidth == 0) {
                                     toCtl.Dock = DockStyle.Fill;
                                 }
                                 else {
                                     toCtl.Location = new Point(BorderWidth, BorderWidth);
                                     toCtl.Size = new Size(Width - BorderWidth * 2, Height - BorderWidth * 2);
                                     toCtl.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
                                 }
                                 toCtl.Visible = true;
                                 if (fromCtl != null) {
                                     //pboxFrom.Visible = false;
                                     //pboxFrom.Image = null;
                                     fromCtl.Visible = false;
                                 }
                                 _drawBmps = false;
                                 //pboxTo.Visible = false;
                                 //pboxTo.Image = null;
                                 SlidingComplete?.Invoke(this, EventArgs.Empty);
                             });
            Invalidate();
            Sliding = false;
        }
        /// <summary>
        /// Defined here so MultiSliderPanel can be included without any other libraries.
        /// The LDAQInterfaces version is preferred.  Use that one outside of here.
        /// </summary>
        private static void InvokeIfRequired(ISynchronizeInvoke obj, MethodInvoker action) {
            try {
                if (obj.InvokeRequired) {
                    var args = new object[0];
                    obj.Invoke(action, args);
                }
                else action();
            }
            catch {
                // do nothing.
            }
        }
        /// <summary>
        /// Defined here so MultiSliderPanel can be included without any other libraries.
        /// The LDAQInterfaces version is preferred.  Use that one outside of here.
        /// </summary>
        private static void ResumeDrawing(Control parent) {
            SendMessage(parent.Handle, WM_SETREDRAW, true, 0);
            if (parent.Visible) parent.Refresh();
        }
        /// <summary>
        /// Defined here so MultiSliderPanel can be included without any other libraries.
        /// The LDAQInterfaces version is preferred.  Use that one outside of here.
        /// </summary>
        [DllImport("user32.dll")] private static extern int SendMessage(IntPtr hWnd, Int32 wMsg, bool wParam, Int32 lParam);
        /// <summary>
        /// Defined here so MultiSliderPanel can be included without any other libraries.
        /// The LDAQInterfaces version is preferred.  Use that one outside of here.
        /// </summary>
        private static void SuspendDrawing(Control parent) {
            SendMessage(parent.Handle, WM_SETREDRAW, false, 0);
        }
        private Bitmap _bgImage;
        private Bitmap _canvas;
        private readonly List<ControlInfo> _controls = new List<ControlInfo>();
        private int _curLeft;
        private bool _drawBmps;
        private Bitmap _fromBmp;
        private bool _fromRight;
        private Control _main;
        private bool _mainEntersFromRight = true;
        private readonly List<Control> _stack = new List<Control>();
        private Bitmap _toBmp;
        private const int WM_SETREDRAW = 11;
        private class ControlInfo {
            public Control Control;
            public bool FromRight;
            public bool NoBitmapDrawing;
            //public bool RequiresZOrderCorrection;
        }
    }
}