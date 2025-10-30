using System;
using PT200_Rendering;

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

        public void SetCaret(int row, int col)
        {
            Console.SetCursorPosition(col, row);
            Console.CursorVisible = true;
        }
    }
}