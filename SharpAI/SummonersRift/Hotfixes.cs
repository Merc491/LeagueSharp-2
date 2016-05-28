﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpAI.SummonersRift.Data;
using SharpAI.Utility;
using LeagueSharp;
using LeagueSharp.SDK;
using LeagueSharp.SDK.Enumerations;
using LeagueSharp.SDK.Utils;
using NLog;
using Random = SharpAI.Utility.Random;

namespace SharpAI.SummonersRift
{
    // this stuff needs better fixing to it but it would take me more lines and eventually time to complete so atm #hotfixed
    public static class Hotfixes
    {
        private static int _lastMovementCommand = 0;
        private static AutoLevel _autoLevel;
        public static bool AttackedByMinionsFlag = false;
        public static bool AttackedByTurretFlag = false;
        public static void Load()
        {
            Events.OnLoad += (obj, loadArgs) =>
            {
                Shop.Main.Init();
                _autoLevel = new AutoLevel(AutoLevel.GetSequenceFromDb());
                _autoLevel.Enable();
                Obj_AI_Base.OnIssueOrder += (sender, issueOrderArgs) =>
                {
                    if (SessionBasedData.Loaded && sender.IsMe)
                    {
                        if (issueOrderArgs.Order == GameObjectOrder.MoveTo)
                        {
                            if (ObjectManager.Player.IsRecalling() ||
                                (ObjectManager.Player.InFountain() && ObjectManager.Player.HealthPercent < 95))
                            {
                                issueOrderArgs.Process = false;
                                return;
                            }
                            if (issueOrderArgs.TargetPosition.IsDangerousPosition())
                            {
                                issueOrderArgs.Process = false;
                                return;
                            }
                            if (Environment.TickCount - _lastMovementCommand > Utility.Random.GetRandomInteger(300, 1100))
                            {
                                _lastMovementCommand = Environment.TickCount;
                                return;
                            }
                            issueOrderArgs.Process = false;
                        }
                        if (issueOrderArgs.Target != null)
                        {
                            if (issueOrderArgs.Target is Obj_AI_Hero)
                            {
                                if (ObjectManager.Player.IsUnderEnemyTurret() || (ObjectManager.Get<Obj_AI_Minion>().Count(m => m.IsEnemy && !m.IsDead && m.Distance(ObjectManager.Player) < 600) > 4))
                                {
                                    issueOrderArgs.Process = false;
                                    return;
                                }
                            }
                            if (issueOrderArgs.Target is Obj_AI_Minion && (issueOrderArgs.Target as Obj_AI_Minion).Team == GameObjectTeam.Neutral)
                            {
                                issueOrderArgs.Process = false;
                                return;
                            }
                        }
                    }
                };
                Spellbook.OnCastSpell += (sender, castSpellArgs) =>
                {
                    if (castSpellArgs.Slot == SpellSlot.Recall)
                    {
                        Variables.Orbwalker.ActiveMode = OrbwalkingMode.None;
                    }
                };
                Obj_AI_Base.OnProcessSpellCast += (sender, spellCastArgs) =>
                {
                    if (Variables.Orbwalker.ActiveMode != OrbwalkingMode.Combo && spellCastArgs.Target != null && spellCastArgs.Target.IsMe)
                    {
                        if (sender is Obj_AI_Minion)
                        {
                            AttackedByMinionsFlag = true;
                            DelayAction.Add(350, () => AttackedByMinionsFlag = false);
                        }
                        if (sender is Obj_AI_Turret)
                        {
                            AttackedByTurretFlag = true;
                            DelayAction.Add(500, () => AttackedByTurretFlag = false);
                        }
                    }
                };
                Game.OnUpdate += args =>
                {
                    if (ObjectManager.Player.IsRecalling() ||
                           (ObjectManager.Player.InFountain() && ObjectManager.Player.HealthPercent < 95))
                    {
                        Variables.Orbwalker.ActiveMode = OrbwalkingMode.None;
                        return;
                    }
                    if (ObjectManager.Player.Position.IsDangerousPosition())
                    {
                        AttackedByMinionsFlag = true;
                        DelayAction.Add(350, () => AttackedByMinionsFlag = false);
                    }
                    
                };
                Variables.Orbwalker.OnAction += (sender, args) =>
                {
                    if (ObjectManager.Player.IsRecalling())
                    {
                        args.Process = false;
                        Variables.Orbwalker.ActiveMode = OrbwalkingMode.None;
                    }
                };
            };
        }
    }
}