using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleEngine
{
    public class UIImageBox : UIComponent
    {
        public char? Outline { get; set; }
        public Sprite Sprite { get; private set; } //Sprite attachment position is ignored by UIImageBox. Image position is used instead
        private Vec2i _imagePosition;
        public Vec2i ImagePosition 
        { 
            get => _imagePosition;
            set
            {
                if (value.X < 0 || value.Y < 0) _imagePosition = new Vec2i(0, 0);
                else _imagePosition = value;
            }
        }

        public UIImageBox(Vec2i position, Vec2i size) : base(position, size) { }

        public override void Update() { }
        
        public void SetImage(Sprite image, Vec2i imagePosition)
        {
            Sprite = image;
            ImagePosition = imagePosition;
        }
        
        public override void DrawComponentToBuffer(char[,] buffer, Vec2i origin)
        {
            var realPos = Position + origin;

            FillWithBackground(buffer, realPos);
            
            if (Outline != null) DrawRectangleOutline(buffer, realPos, Outline.Value);

            if (Sprite != null) Engine.Renderer.WriteImageBoxSpriteToBuffer(buffer, Position + ImagePosition,
                                    Position + Size, Sprite);
        }
    }
}
