using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PT200_Rendering
{
    public record RenderRun(int Row, int StartCol, ConsoleColor Fg, ConsoleColor Bg, char[] Chars);

    public interface IRenderTarget
    {
        public enum CursorStyle
        {
            Block,
            HorizontalBar,
            VerticalBar
        }

        void Clear();
        void DrawRun(RenderRun run);
        void SetCaret(int row, int col, bool visible, CursorStyle style);
    }
}