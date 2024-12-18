using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace ConsoleEngine
{
    public class UIPanel : UIComponent
    {
        private List<UIComponent> children = new List<UIComponent>();
        public ReadOnlyCollection<UIComponent> Children { get; }
        
        public UIPanel(Vec2i position, Vec2i size, int priority, Engine engine) : base (engine, position, size, ' ', priority)
        {
            Children = children.AsReadOnly();
        }

        public override void Update()
        {
            foreach (var uiComponent in Children) uiComponent.Update();
        }

        public void AddUIComponent(UIComponent uiComponent)
        {
            uiComponent.Validate(this);
            children.Add(uiComponent);
        }
        public void RemoveUIComponent(UIComponent uiComponent)
        {
            children.Remove(uiComponent);
        }

        public override void DrawComponentToBuffer(char[,] buffer, Vec2i offset)
        {
            var realPos = offset + Position;

            FillWithBackground(buffer, realPos);

            children.OrderBy(c => c.Priority);
            foreach (var child in children) child.DrawComponentToBuffer(buffer, realPos);
        }
    }
}
