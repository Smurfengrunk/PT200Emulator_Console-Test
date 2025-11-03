using System.Timers;
using PT200_Rendering;
using PT200_Parser;

namespace PT200Emulator
{
    public class ConsoleRenderTarget : IRenderTarget
    {
        public void Clear() => Console.Clear();

        public void DrawRun(RenderRun run)
        {
            Console.SetCursorPosition(run.StartCol, run.Row);
            Console.ForegroundColor = run.Fg;
            Console.BackgroundColor = run.Bg;
            Console.Write(run.Chars);
        }

        public void SetCaret(int row, int col, bool visible, IRenderTarget.CursorStyle style)
        {
            Console.SetCursorPosition(col, row);
            Console.CursorVisible = visible;
        }
    }

    public class ConsoleCaretController : ICaretController
    {
        private readonly ConsoleRenderTarget _renderTarget;
        private readonly System.Timers.Timer _blinkTimer;
        private bool _showCaret = true;
        private int _row, _col;
        private IRenderTarget.CursorStyle _style = IRenderTarget.CursorStyle.Block;

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