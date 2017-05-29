using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace PathfindingProject
{
    public enum DebugOp
    {
        ShowGrid = 1,
        CalcPath = 2,
        CalcNavMesh = 3,
        ShowUniqueGrid = 4,
        Count = 5 // For easy iteration
    }

    public static class Debug
    {
        private static Color _debugColor = Color.White;
        private static Dictionary<DebugOp, bool> _settings = new Dictionary<DebugOp, bool>();
        private static List<string> _hookedText = new List<string>();
        private static Point _hookedTextStart = new Point(1000, 5);

        public static void Init()
        {
            _settings.Add(DebugOp.ShowGrid, false);
            _settings.Add(DebugOp.CalcPath, false);
            _settings.Add(DebugOp.CalcNavMesh, false);
            _settings.Add(DebugOp.ShowUniqueGrid, false);
        }

        public static void HandleInput()
        {
            if (Input.KeyTyped(Keys.D1))
                _settings[DebugOp.ShowGrid] = !_settings[DebugOp.ShowGrid];

            if (Input.KeyTyped(Keys.D2))
                _settings[DebugOp.CalcPath] = !_settings[DebugOp.CalcPath];

            if (Input.KeyTyped(Keys.D3))
                _settings[DebugOp.CalcNavMesh] = !_settings[DebugOp.CalcNavMesh];

            if (Input.KeyTyped(Keys.D4))
                _settings[DebugOp.ShowUniqueGrid] = !_settings[DebugOp.ShowUniqueGrid];
        }

        public static bool IsOn(DebugOp setting)
        {
            return _settings[setting];
        }

        public static void RenderDebugOptionStates(SpriteBatch spriteBatch)
        {
            for (int i = 1; i < (int)DebugOp.Count; i++)
            {
                DebugOp opt = (DebugOp)i;
                spriteBatch.DrawString(Game1.Instance.smallFont, "(" + i + ") " + opt.ToString() + " --- " + _settings[opt], new Vector2(200, -5 + (i * 15)), _debugColor);
            }
        }

        public static void ClearHookedText()
        {
            _hookedText = new List<string>();
        }

        public static void HookText(string text)
        {
            _hookedText.Add(text);
        }

        public static void RenderHookedText(SpriteBatch spriteBatch)
        {
            for (int i = 0; i < _hookedText.Count; i++)
            {
                spriteBatch.DrawString(Game1.Instance.smallFont, _hookedText[i], new Vector2(_hookedTextStart.X, _hookedTextStart.Y + (i * 15)), _debugColor);
            }
        }
    }
}
