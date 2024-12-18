using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleEngine
{
    public abstract class UIComponent // TODO: clean up access modifiers if needed
    {
        public Engine Engine { get; private set;}
        public Vec2i Position { get; private set; }
        public Vec2i Size { get; private set; }
        public char Background { get; set; }
        public int Priority { get; private set; }

        public UIComponent(Engine engine, Vec2i position, Vec2i size, char background, int priority)
        {
            Engine = engine;
            Position = position;
            Size = size;
            Background = background;
            Priority = priority;
        }
        public abstract void Update();

        public abstract void DrawComponentToBuffer(char[,] buffer, Vec2i offset)

        protected void FillWithBackground(char[,] buffer, Vec2i offset)
        {
            for (var x = offset.X; x < offset.X + Size.X; x++)
            {
                for (var y = offset.Y; y < offset.Y + Size.Y; y++)
                {
                    buffer[x, y] = Background;
                }
            }
        }

        public UIComponent(Vec2i position, Vec2i size)
        {
            Position = position;
            Size = size;
        }

        internal protected virtual void Validate(UIComponent parent)
        {
            if (IsOutsideOfCamera())
                throw new UIException("The UIComponent being added was partially or fully outside of the camera");

            if (parent != null)
            {
                if (IsOutsideOfParent(parent))
                    throw new UIException("The UIPanel being added was partially or fully outside of the parent UIPanel");
            }
        }
        
        private bool IsOutsideOfCamera()
        {
            if (Position.X < 0 ||
                Position.X + Size.X > Engine.Settings.CameraSizeX ||
                Position.Y < 0 ||
                Position.Y + Size.Y > Engine.Settings.CameraSizeY)
            {
                return true;
            }

            return false;
        }
        private bool IsOutsideOfParent(UIComponent parent)
        {
            if (Position.X < parent.Position.X ||
                Position.X + Size.X > parent.Position.X + parent.Size.X ||
                Position.Y < parent.Position.X ||
                Position.Y + Size.Y > parent.Position.Y + parent.Size.Y)
            {
                return true;
            }
            return false;
        }

        internal protected void DrawRectangleOutline(char[,] buffer, Vec2i origin, char c)
        {
            for (var x = origin.X; x < origin.X + Size.X; x++)
            {
                buffer[x, origin.Y] = c;
                buffer[x, origin.Y + Size.Y] = c;
            }
            for (var y = origin.Y; y < origin.Y + Size.Y; y++)
            {
                buffer[origin.X, y] = c;
                buffer[origin.X + Size.X, y] = c;
            }
        }

        internal protected void DrawFilledRectangle(char[,] buffer, Vec2i origin, Vec2i size, char c)
        {
            for (var x = origin.X; x < origin.X + size.X; x++)
            {
                for (var y = origin.Y; y < origin.Y + size.Y; y++)
                {
                    buffer[x, y] = c;
                }
            }
        }
    }
}
