﻿using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using static AramBuddy.MainCore.Utility.Misc;

namespace AramBuddy.Plugins.Champions.Annie
{
    internal class Annie : Base
    {
        static Annie()
        {
            MenuIni = MainMenu.AddMenu(MenuName, MenuName);
            AutoMenu = MenuIni.AddSubMenu("Auto");
            ComboMenu = MenuIni.AddSubMenu("Combo");
            HarassMenu = MenuIni.AddSubMenu("Harass");
            LaneClearMenu = MenuIni.AddSubMenu("LaneClear");
            KillStealMenu = MenuIni.AddSubMenu("KillSteal");

            foreach (var spell in SpellList)
            {
                ComboMenu.CreateCheckBox(spell.Slot, "Use " + spell.Slot);
                if (spell != R && spell != E)
                {
                    HarassMenu.CreateCheckBox(spell.Slot, "Use " + spell.Slot);
                    HarassMenu.CreateSlider(spell.Slot + "mana", spell.Slot + " Mana Manager", 60);
                    LaneClearMenu.CreateCheckBox(spell.Slot, "Use " + spell.Slot);
                    LaneClearMenu.CreateSlider(spell.Slot + "mana", spell.Slot + " Mana Manager", 60);
                }
                if (spell != E)
                {
                    KillStealMenu.CreateCheckBox(spell.Slot, "Use " + spell.Slot);
                }
            }
            AutoMenu.CreateSlider("RAOE", "R AOE hit count {0}", 3, 1, 6);
            AutoMenu.CreateCheckBox("GapStun", "Anti-GapCloser Stun");
            AutoMenu.CreateCheckBox("IntStun", "Interrupter Stun");
            AutoMenu.CreateCheckBox("SaveStun", "Save Stun");

            ComboMenu.CreateSlider("RAOE", "R AOE hit count {0}", 3, 1, 6);

            Gapcloser.OnGapcloser += Gapcloser_OnGapcloser;
            Interrupter.OnInterruptableSpell += Interrupter_OnInterruptableSpell;
            Dash.OnDash += Dash_OnDash;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
        }

        private static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender == null || !sender.IsMe || sender.IsDead)
                return;

            if (AutoMenu.CheckBoxValue("SaveStun") && user.HasBuff("pyromania_particle") && args.Target.Type == GameObjectType.obj_AI_Minion)
                args.Process = false;
        }

        private static void Dash_OnDash(Obj_AI_Base sender, Dash.DashEventArgs e)
        {
            if (sender == null || !sender.IsEnemy || !sender.IsKillable())
                return;

            if (!user.HasBuff("pyromania_particle")) return;

            if (Q.IsReady() && AutoMenu.CheckBoxValue("GapStun") && 
                (sender.IsKillable(Q.Range) || e.EndPos.IsInRange(user, Q.Range)))
            {
                Q.Cast(sender);
                return;
            }

            if (W.IsReady() && AutoMenu.CheckBoxValue("GapStun") &&
                (sender.IsKillable(W.Range) || e.EndPos.IsInRange(user, W.Range)))
            {
                W.Cast(sender);
            }
        }

        private static void Interrupter_OnInterruptableSpell(Obj_AI_Base sender, Interrupter.InterruptableSpellEventArgs e)
        {
            if (sender == null || !sender.IsEnemy)
                return;

            if (!Player.HasBuff("pyromania_particle")) return;

            if (Q.IsReady() && AutoMenu.CheckBoxValue("IntStun") && sender.IsKillable(Q.Range))
            {
                Q.Cast(sender);
                return;
            }

            if (W.IsReady() && AutoMenu.CheckBoxValue("IntStun") && sender.IsKillable(W.Range))
            {
                W.Cast(sender);
                return;
            }

            if (R.IsReady() && AutoMenu.CheckBoxValue("IntStun") && sender.IsKillable(R.Range) && e.DangerLevel > DangerLevel.Low)
            {
                R.Cast(sender);
            }
        }

        private static void Gapcloser_OnGapcloser(AIHeroClient sender, Gapcloser.GapcloserEventArgs e)
        {
            if (sender == null || !sender.IsEnemy || !sender.IsKillable())
                return;

            if (!Player.HasBuff("pyromania_particle")) return;

            if (Q.IsReady() && AutoMenu.CheckBoxValue("GapStun") &&
                (sender.IsKillable(Q.Range) || e.End.IsInRange(user, Q.Range)))
            {
                Q.Cast(sender);
                return;
            }

            if (W.IsReady() && AutoMenu.CheckBoxValue("GapStun") &&
                (sender.IsKillable(W.Range) || e.End.IsInRange(user, W.Range)))
            {
                W.Cast(sender);
            }
        }

        public override void Active()
        {
            R.SetSkillshot().CastAOE(ComboMenu.SliderValue("RAOE"));
        }

        public override void Combo()
        {
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Magical);
            if (target == null || !target.IsKillable(Q.Range))

            if (ComboMenu.CheckBoxValue(SpellSlot.Q) && Q.IsReady())
            {
                Q.Cast(target);
            }

            if (ComboMenu.CheckBoxValue(SpellSlot.W) && W.IsReady() && target.IsKillable(W.Range))
            {
                W.Cast(target);
            }

            if (ComboMenu.CheckBoxValue(SpellSlot.E) && E.IsReady() && !Player.HasBuff("pyromania_particle"))
            {
                E.Cast();
            }

            if (!ComboMenu.CheckBoxValue(SpellSlot.R) || !R.IsReady() || !target.IsKillable(R.Range)) return;

            if (target.CountEnemiesInRange(R.SetSkillshot().Width) >= ComboMenu.SliderValue("RAOE"))
            {
                R.Cast(target);
            }
        }

        public override void Harass()
        {
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Magical);
            if (target == null || !target.IsKillable(Q.Range))

            if (HarassMenu.CheckBoxValue(SpellSlot.Q) && Q.IsReady() && HarassMenu.CompareSlider(Q.Slot + "mana", user.ManaPercent))
            {
                Q.Cast(target);
            }
            if (!HarassMenu.CheckBoxValue(SpellSlot.W) || !W.IsReady() ||
                !HarassMenu.CompareSlider(W.Slot + "mana", user.ManaPercent)) return;

            if (target.IsKillable(W.Range))
            {
                W.Cast(target);
            }
        }

        public override void LaneClear()
        {
            foreach (var target in EntityManager.MinionsAndMonsters.EnemyMinions.Where(m => m != null && m.IsValidTarget()))
            {
                if (Q.IsReady() && target.IsKillable(Q.Range) && LaneClearMenu.CheckBoxValue(SpellSlot.Q) && LaneClearMenu.CompareSlider(Q.Slot + "mana", user.ManaPercent) 
                    && Q.WillKill(target))
                {
                    Q.Cast(target);
                }
                if (W.IsReady() && target.IsKillable(W.Range) && LaneClearMenu.CheckBoxValue(SpellSlot.W) && LaneClearMenu.CompareSlider(W.Slot + "mana", user.ManaPercent) 
                    && target.CountEnemyMinionsInRange(W.Range - user.Distance(target)) >= 3)
                {
                    W.Cast(target);
                }
            }
        }

        public override void Flee()
        {
        }

        public override void KillSteal()
        {
            foreach (var target in EntityManager.Heroes.Enemies.Where(e => e != null && e.IsValidTarget()))
            {
                if (Q.IsReady() && target.IsKillable(Q.Range) && Q.WillKill(target) && KillStealMenu.CheckBoxValue(SpellSlot.Q))
                {
                    Q.Cast(target);
                }
                if (W.IsReady() && target.IsKillable(W.Range) && W.WillKill(target) && KillStealMenu.CheckBoxValue(SpellSlot.W))
                {
                    W.Cast(target);
                }
                if (R.IsReady() && target.IsKillable(R.Range) && R.WillKill(target) && KillStealMenu.CheckBoxValue(SpellSlot.R))
                {
                    R.Cast(target);
                }
            }
        }
    }
}
