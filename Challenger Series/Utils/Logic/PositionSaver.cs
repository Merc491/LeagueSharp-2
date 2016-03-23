﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using LeagueSharp;
using LeagueSharp.SDK;
using LeagueSharp.SDK.Core.UI.IMenu.Values;
using LeagueSharp.SDK.Core.Wrappers.Damages;
using SharpDX;
using Challenger_Series.Utils;
using LeagueSharp.SDK.Core.Utils;
using Geometry = Challenger_Series.Utils.Geometry;
using Menu = LeagueSharp.SDK.Core.UI.IMenu.Menu;
using Color = System.Drawing.Color;

namespace Challenger_Series.Utils.Logic
{
    public class PositionSaver
    {
        private PositionSaverCore _core;
        private MenuKeyBind _saveKey;
        private MenuBool _isEnabled;
        private Spell _spellToUse;
        public PositionSaver(Menu menu, Spell spellToUse)
        {
            _core = new PositionSaverCore();
            _isEnabled = menu.Add(new MenuBool("positionsaverenabled", "Auto use in custom positions", true));
            _saveKey = menu.Add(new MenuKeyBind("positionsaversavekey", "Save cursor position as custom pos!", Keys.O, KeyBindType.Press));
            _spellToUse = spellToUse;
            Drawing.OnDraw += OnDraw;
        }

        private void OnDraw(EventArgs args)
        {
            if (_isEnabled)
            {
                if (_saveKey.Active)
                {
                    if (!_core.Positions.Any(pos => pos.Distance(Game.CursorPos) < 100))
                    {
                        _core.SavePosition(Game.CursorPos);
                        _core.Positions.Add(Game.CursorPos);
                    }
                }
                if (_core.Positions.Any())
                {
                    foreach (var savedLocation in _core.Positions.Where(pos => pos.Distance(ObjectManager.Player.Position) < 4000))
                    {
                        Drawing.DrawCircle(savedLocation, 100, savedLocation.Distance(ObjectManager.Player.Position) < _spellToUse.Range ? Color.Gold : Color.White);
                    }
                }
                var positionCandidate = _core.Positions.FirstOrDefault(pos => pos.Distance(ObjectManager.Player.Position) < _spellToUse.Range);
                if (positionCandidate != null && positionCandidate.IsValid() && !GameObjects.AllyMinions.Any(m => m.Position.Distance(positionCandidate) < 100))
                {
                    _spellToUse.Cast(positionCandidate);
                }
            }
        }
    }
}
