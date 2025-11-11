using System.Timers;
using System.Drawing;
using PT200_Rendering;
using PT200_Parser;

namespace PT200Emulator
{
    public class ConsoleRenderTarget : IRenderTarget
    {
        ConsoleCaretController caretController;

        public ConsoleRenderTarget()
        {
            caretController = new(this);
        }
        public void Clear() => Console.Clear();

        public void DrawRun(RenderRun run)
        {
            Console.SetCursorPosition(run.StartCol, run.Row);
            Console.ForegroundColor = TranslateColor(run.Fg);
            Console.BackgroundColor = TranslateColor(run.Bg);
            Console.Write(run.Chars);
        }

        public void SetCaret(int row, int col)
        {
            Console.SetCursorPosition(col, row);
            Console.CursorVisible = caretController._showCaret;
        }

        private static ConsoleColor TranslateColor(Color color) => color.ToArgb() switch
        {
            int argb when argb == Color.Black.ToArgb() => ConsoleColor.Black,
            int argb when argb == Color.DarkBlue.ToArgb() => ConsoleColor.DarkBlue,
            int argb when argb == Color.DarkGreen.ToArgb() => ConsoleColor.DarkGreen,
            int argb when argb == Color.DarkCyan.ToArgb() => ConsoleColor.DarkCyan,
            int argb when argb == Color.DarkRed.ToArgb() => ConsoleColor.DarkRed,
            int argb when argb == Color.DarkMagenta.ToArgb() => ConsoleColor.DarkMagenta,
            int argb when argb == Color.Gray.ToArgb() => ConsoleColor.Gray,
            int argb when argb == Color.Blue.ToArgb() => ConsoleColor.Blue,
            int argb when argb == Color.LimeGreen.ToArgb() => ConsoleColor.Green,
            int argb when argb == Color.Cyan.ToArgb() => ConsoleColor.Cyan,
            int argb when argb == Color.Red.ToArgb() => ConsoleColor.Red,
            int argb when argb == Color.Magenta.ToArgb() => ConsoleColor.Magenta,
            int argb when argb == Color.Yellow.ToArgb() => ConsoleColor.Yellow,
            int argb when argb == Color.White.ToArgb() => ConsoleColor.White,
            _ => ConsoleColor.Yellow
        };
    }

    public class ConsoleCaretController : ICaretController
    {
        private readonly ConsoleRenderTarget _renderTarget;
        private readonly System.Timers.Timer _blinkTimer;
        internal bool _showCaret = true;
        private int _row, _col;
        internal IRenderTarget.CursorStyle _style = IRenderTarget.CursorStyle.Block;

        public ConsoleCaretController(IRenderTarget renderTarget)
        {
            _renderTarget = (ConsoleRenderTarget)renderTarget;

            _blinkTimer = new System.Timers.Timer(500);
            _blinkTimer.Interval = 500;
            _blinkTimer.Elapsed += (s, e) =>
            {
                _showCaret = !_showCaret;
            };
            _blinkTimer.Start();
        }

        public void SetCaretPosition(int row, int col)
        {
            _row = row;
            _col = col;
        }

        public void SetCursorStyle(IRenderTarget.CursorStyle style)
        {
            _style = style;
        }
        public void MoveCaret(int dRow, int dCol) { /* flytta caret */ }
        public void Show()
        {
            _showCaret = true;
            _blinkTimer.Start();
        }

        public void Hide()
        {
            _showCaret = false;
            _blinkTimer.Stop();
        }
    }
}