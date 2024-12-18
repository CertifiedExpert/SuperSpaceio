using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media.Imaging;

namespace ConsoleEngine
{
    public class UITextBox : UIComponent
    {
        public string Text { get; set; }
        public char? Outline { get; set; } // Null means no outline.
        public bool SpacedLetters { get; set; }
        public Vec2i TextPosition { get; set; }
        public UITextBox(Vec2i position, Vec2i size, char background, int priority, Vec2i textPosition, string text,
         Engine engine, char? outline = null, bool? spacedLetters = null) : base(engine, position, size, background, priority)
        {
            Text = text;
            TextPosition = textPosition;
            Outline = outline;
            SpacedLetters = spacedLetters ?? false;
        }
        
        public override void Update()
        {
            
        }

        public override void DrawComponentToBuffer(char [,] buffer, Vec2i offset)
        {
            var realPos = Position + offset;

            if (Outline != null)
            {
                DrawRectangleOutline(buffer, realPos, Outline.Value);
            }

            if (SpacedLetters)
            {
                var limit = (Size.X - TextPosition.X + 1) / 2;
                limit = Math.Min(limit, Text.Length);
                for (var i = 0; i < limit; i++)
                {
                    buffer[realPos.X + TextPosition.X + 2 * i, TextPosition.Y] = Text[i];
                }
            }
            else
            {
                var limit = Size.X - TextPosition.X;
                limit = Math.Min(limit, Text.Length);
                for (var i = 0; i < limit; i++)
                {
                    buffer[realPos.X + TextPosition.X + i, realPos.Y + TextPosition.Y] = Text[i];
                }
            }
        }
    }
}
