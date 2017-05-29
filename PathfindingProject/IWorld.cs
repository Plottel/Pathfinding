using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;

namespace PathfindingProject
{
    public interface IWorld
    {
        void HandleInput();
        void Update();
        void Render(SpriteBatch spriteBatch);

        Grid Grid { get; set; }
        int Width { get; }
        int Height { get; }
    }
}
