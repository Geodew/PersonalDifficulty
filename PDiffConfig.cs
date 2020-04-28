using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;
using Terraria.ModLoader.Config.UI;
using Terraria.UI;

namespace PersonalDifficulty
{
	// Note: At the time this was written, it was not possible for ModPlayer objects to write to ModConfig objects, so we have a bit of a roundabout way of figuring out if the current settings
	//       are loaded from a player or not.
	[Label("Personal Config Menu")]
	public class PDiffConfigLocal : ModConfig
	{
		public override ConfigScope Mode => ConfigScope.ClientSide;

		// "Lead Instance": tModLoader sets this automatically when the mod is loaded
		public static PDiffConfigLocal Instance;

		// This bool tells us if this is a fresh instance or was created as a copy of an instance that wasn't initialized from player data (at least yet)
		// I know the guide says not to use static data members, but I think it means public static members. I have a good reason for needing this (see comment above class declaration).
		private static bool mIsLeadInstanceInitializedFromPlayer = false;

		// v0.1.0
		// These are not always used; see accessors below
		// These don't need defaults, as they should either be initialized by `TryInitializeFromPlayer` or tModLoader calling `set`
		private float _PlayerPowerScalarPercentage;
		private float _PlayerDamageDealtScalarPercentage;
		private float _PlayerDamageTakenScalarPercentage;
		private float _PlayerKnockbackDealtScalarPercentage;
		//private float _PlayerKnockbackTakenScalarPercentage;  // Apparently this can't be done
		private bool _DisableKnockbackOnSelf;
		private bool _DisableFallDamageOnSelf;
		private bool _ShowDamageChangesInWeaponTooltip;
		private bool _ShowKnockbackChangesInWeaponTooltip;

		// v0.2.0
		private float _EffectiveDamageDealtScalarPercentage;
		private float _EffectiveDamageTakenScalarPercentage;
		private float _GlowAmountPercentage;
		private bool _DisableDrowningForSelf;
		private bool _DisableLavaDamageOnSelf;
		private bool _EnableGlow;


		// New Feature Comment Tag: Code change goes here (Variable (above))
		// New Feature Comment Tag: Code change goes here (Property Get/Set (below))


		[Header("Main Power Slider\n - Set to 0 to disable all damage/knockback changes.\n - You can change a setting beyond the slider limits by editing your mod config file manually.\n - All settings are saved by character. Changes from the main menu will be applied only if you then load a character as of yet untouched by this mod.")]
		[Range(-150.0f, 150.0f)]
		[Increment(5.0f)]
		[DefaultValue(0.0f)]
		[Label("Player \"Power\" Increase % (+/-)")]
		[Tooltip("Increase your personal player \"power\" by this percentage. (Use negative values to decrease your power.)\nKnockback is scaled to a portion of this by default (see below).\n\nExamples: Using default fine-tuning below, 100.0 will double your damage done and halve your damage taken.\nUsing default fine-tuning below, -100.0 will halve your damage done and double your damage taken.")]
		public float PlayerPowerScalarPercentage
		{
			get
			{
				TryInitializeFromPlayer();

				return _PlayerPowerScalarPercentage;
			}

			set
			{
				_PlayerPowerScalarPercentage = value;
			}
		}

		[Header("Game Mechanic Switches\n - (These settings are independent of the Power slider.)\n - Note that lowering your damage taken using other settings also affects damage taken from these mechanics (and anything else).\n - Careful not to make the game too easy! I recommend you only use these if you're totally fed up.\n(Part of the game is working around these problems, like crafting things to help you avoid them, so you'll miss out on pieces of the gameplay by using these options.)\n - Useful for less-experienced gamers (including, but not limited to, young children) who might die to these repeatedly, to the point where it's annoying, or for playing in a sort of creative or peaceful mode stress-free.")]
		[DefaultValue(false)]
		[Label("Disable Knockback on Self")]
		[Tooltip("This disables knockback on only you.\nTerraria/tModLoader doesn't support knockback resistance on players, so this is the next best thing.")]
		public bool DisableKnockbackOnSelf
		{
			get
			{
				TryInitializeFromPlayer();

				return _DisableKnockbackOnSelf;
			}

			set
			{
				_DisableKnockbackOnSelf = value;
			}
		}

		[DefaultValue(false)]
		[Label("Disable Fall Damage on Self")]
		[Tooltip("This disables fall damage on only you.")]
		public bool DisableFallDamageOnSelf
		{
			get
			{
				TryInitializeFromPlayer();

				return _DisableFallDamageOnSelf;
			}

			set
			{
				_DisableFallDamageOnSelf = value;
			}
		}

		[DefaultValue(false)]
		[Label("Disable Drowning for Self")]
		[Tooltip("This disables drowning on only you.\n(You gain the ability to breathe both water and air.)")]
		public bool DisableDrowningForSelf
		{
			get
			{
				TryInitializeFromPlayer();

				return _DisableDrowningForSelf;
			}

			set
			{
				_DisableDrowningForSelf = value;
			}
		}

		[DefaultValue(false)]
		[Label("Disable Lava Damage on Self")]
		[Tooltip("This disables lava damage on only you.\nThis also grants immunity to damage from standing on \"hot\" block types (\"Fire-Walking\") and immunity to being set on fire.")]
		// "Hot block types" to keep it mostly spoiler-free
		public bool DisableLavaDamageOnSelf
		{
			get
			{
				TryInitializeFromPlayer();

				return _DisableLavaDamageOnSelf;
			}

			set
			{
				_DisableLavaDamageOnSelf = value;
			}
		}

		[DefaultValue(false)]
		[Label("Enable Constant Glow on Self")]
		[Tooltip("Makes your character glow at all times. Adjust the brightness with the slider below.\nThis option is intended for players who keep dying because they can't see their enemies in the dark,\ndue to, for example, an accidental fall or being deep in water.")]
		public bool EnableGlow
		{
			get
			{
				TryInitializeFromPlayer();

				return _EnableGlow;
			}

			set
			{
				_EnableGlow = value;
			}
		}

		[Range(0.0f, 400.0f)]
		[Increment(10.0f)]
		[DefaultValue(100.0f)]
		[Label("Player Glow Strength")]
		[Tooltip("If Glow is enabled above, this controls how strongly you glow.\nOtherwise, it does nothing.\n0.0 = No glow at all\n100.0 = Glow as brightly as a torch\n200.0 = Glow twice as brightly as a torch\netc.\n\nNote: If you set this really high, the light may appear to lag behind you.\nMost light actually does this; it's just not noticeable unless the light is very bright. (It's not a bug.)")]
		public float GlowAmountPercentage
		{
			get
			{
				TryInitializeFromPlayer();

				return _GlowAmountPercentage;
			}

			set
			{
				_GlowAmountPercentage = value;
			}
		}

		[Header("Fine-Tuning\n - \"Power\" multiplies all these to arrive at the final effect.\n - Set to 0 to disable that specific change.\n - You can change a setting beyond the slider limits by editing your mod config file manually.\n - Note: All settings are saved by character. Changes from the main menu will be applied only if you then load a character as of yet untouched by this mod.")]
		[Range(0.0f, 200.0f)]
		[Increment(5.0f)]
		[DefaultValue(100.0f)]
		[Label("Player Damage Dealt Increase Multiplier %")]
		[Tooltip("For each 100% of Power Increase, your damage increases by this percent.\n(This also affects negative Power Increases similarly.)\n(Default 100.0)\n\nExamples: If Power Increase is 200.0, and this is set to 50.0, your damage will double.\nIf your Power Increase is -100.0 and this is set to 50.0, your damage will be divided by 1.5.")]
		public float PlayerDamageDealtScalarPercentage
		{
			get
			{
				TryInitializeFromPlayer();

				return _PlayerDamageDealtScalarPercentage;
			}

			set
			{
				_PlayerDamageDealtScalarPercentage = value;
			}
		}

		[Range(0.0f, 200.0f)]
		[Increment(5.0f)]
		[DefaultValue(100.0f)]
		[Label("Player Damage Taken Decrease Multiplier %")]
		[Tooltip("For each 100% of Power Increase, the damage you can effectively withstand increases by this percent.\nIncoming damage is lowered to do this; your actual max HP is unchanged.\n(This also affects negative Power Increases similarly.)\n(Default 100.0)\n\nExamples: If Power Increase is 200.0, and this is set to 50.0, damage you take is halved.\nIf your Power Increase is -100.0 and this is set to 50.0, you will take 50% more damage.")]
		public float PlayerDamageTakenScalarPercentage
		{
			get
			{
				TryInitializeFromPlayer();

				return _PlayerDamageTakenScalarPercentage;
			}

			set
			{
				_PlayerDamageTakenScalarPercentage = value;
			}
		}

		[Range(0.0f, 50.0f)]
		[Increment(1.0f)]
		[DefaultValue(10.0f)]
		[Label("Player Knockback Dealt Increase Multiplier %")]
		[Tooltip("For each 100% of Power Increase, your knockback increases by this percent.\n(This also affects negative Power Increases similarly.)\n(Default 10.0)\n\nExamples: If Power Increase is 50.0, and this is set to 10.0, your knockback dealt increases by 5%.\nIf your Power Increase is -100.0 and this is set to 20.0, your knockback will be divided by 1.2.\n\nNote: The default not sound like much of a change, but it is the same damage-to-knockback increase ratio that Expert Mode applies.")]
		public float PlayerKnockbackDealtScalarPercentage
		{
			get
			{
				TryInitializeFromPlayer();

				return _PlayerKnockbackDealtScalarPercentage;
			}

			set
			{
				_PlayerKnockbackDealtScalarPercentage = value;
			}
		}

		[Range(0.0f, 100.0f)]
		[Increment(5.0f)]
		[DefaultValue(100.0f)]
		[Label("Effective Damage Dealt Multiplier %")]
		[Tooltip("This controls whether your damage dealt changes are to your effective damage or your base damage.\n100.0 = Change effective damage.\n0.0 = Change base damage.\nOther = Scale between the two values.\n(Default 100.0)\n\nExamples: Suppose your original base damage is 30, and you hit an enemy with 10 Defense on Normal Mode. That enemy would take 25 damage.\nSuppose PersonalDifficulty is set to double your damage, and this is set to 100. Then your base damage would increase to 55 so that your new end result is 50 damage, double of 25.\nSuppose this is set to 0, then your *base* damage would be doubled to 60, so you'd deal 55 damage.\nIf this were set to 40, you'd do 40% in between: 52 damage.")]
		public float EffectiveDamageDealtScalarPercentage
		{
			get
			{
				TryInitializeFromPlayer();

				return _EffectiveDamageDealtScalarPercentage;
			}

			set
			{
				_EffectiveDamageDealtScalarPercentage = value;
			}
		}

		[Range(0.0f, 100.0f)]
		[Increment(5.0f)]
		[DefaultValue(100.0f)]
		[Label("Effective Damage Taken Multiplier %")]
		[Tooltip("This controls whether your damage taken changes are to the effective damage or the base damage.\n100.0 = Change effective damage.\n0.0 = Change base damage.\nOther = Scale between the two values.\n(Default 100.0)\n\nExamples: Suppose an enemy hits you with an original base damage of 30 on Normal Mode, and you have 10 Defense. You would take 25 damage.\nSuppose PersonalDifficulty is set to double your damage taken, and this is set to 100.\nThen the effective (end result) damage would increase to 50, double of 25.\nSuppose this is set to 0, then the *base* damage would be doubled to 60, so you'd take 55 damage.\nIf this were set to 40, you'd take 40% in between: 52 damage.")]
		public float EffectiveDamageTakenScalarPercentage
		{
			get
			{
				TryInitializeFromPlayer();

				return _EffectiveDamageTakenScalarPercentage;
			}

			set
			{
				_EffectiveDamageTakenScalarPercentage = value;
			}
		}

		//  ===== Apparently this can't be done =====
		//[Range(0.0f, 50.0f)]
		//[Increment(1.0f)]
		//[DefaultValue(10.0f)]
		//[Label("Player Knockback Taken Increase Multiplier %")]
		//[Tooltip("For each 100% of Power Increase, your knockback resistance increases, as if your mass increased by this percent.\n(This also affects negative Power Increases similarly.)\n(Default 10.0)\n\nExamples: If Power Increase is 50.0, and this is set to 10.0, knockback on you is divided by 1.05.\nIf your Power Increase is -100.0 and this is set to 20.0, knockback on you will increase by 20%.\n\nNote: The default not sound like much of a change, but it is the same damage-to-knockback increase ratio that Expert Mode applies.")]
		//public float PlayerKnockbackTakenScalarPercentage;
		//  ===== Apparently this can't be done =====

		//zzz Eventually add status effect duration increase/decrease on self

		[Header("Display Options (basically purely visual)\n - All settings are saved by character. Changes from the main menu will be applied only if you then load a character as of yet untouched by this mod.")]
		[DefaultValue(false)]
		[Label("Show Damage Changes In Weapon Tooltip")]
		[Tooltip("True = Damage values in weapon tooltips update to your new damage.\nFalse = Show what the damage would normally be.\n(\"False\" is useful for comparing gear quality to other players.)\n(You get damage bonuses/penalties regardless of this setting.)")]
		public bool ShowDamageChangesInWeaponTooltip
		{
			get
			{
				TryInitializeFromPlayer();

				return _ShowDamageChangesInWeaponTooltip;
			}

			set
			{
				_ShowDamageChangesInWeaponTooltip = value;
			}
		}

		[DefaultValue(false)]
		[Label("Show Knockback Changes In Weapon Tooltip")]
		[Tooltip("True = Knockback values in weapon tooltips update to your new knockback.\nFalse = Show what the knockback would normally be.\n(\"False\" is useful for comparing gear quality to other players.)\n(You get knockback bonuses/penalties regardless of this setting.)")]
		public bool ShowKnockbackChangesInWeaponTooltip
		{
			get
			{
				TryInitializeFromPlayer();

				return _ShowKnockbackChangesInWeaponTooltip;
			}

			set
			{
				_ShowKnockbackChangesInWeaponTooltip = value;
			}
		}

		public override void OnChanged()
		{
			// Changes might be set at the main menu, with no active player
			if (IsPlayerActive())
			{
				PDiffModPlayer modPlayer = GetMyModPlayer();

				if (modPlayer.GetLoadFinished())
				{
					modPlayer.UpdatePlayerFromConfigLoad(this);  // This also automatically syncs with other players, if appropriate
				}
			}
		}

		private void TryInitializeFromPlayer()
		{
			// Changes might be set at the main menu, with no active player
			if (ReferenceEquals(this, Instance) && !mIsLeadInstanceInitializedFromPlayer && IsPlayerActive())
			{
				PDiffModPlayer modPlayer = GetMyModPlayer();

				if (modPlayer.GetLoadFinished())
				{
					mIsLeadInstanceInitializedFromPlayer = true;  // This goes first to prevent recursion when modPlayer calls `set`
					modPlayer.UpdateConfigFromPlayerLoad(this);
				}
			}
		}

		// Changes might be set at the main menu, with no active player
		private bool IsPlayerActive()
		{
			return (!Main.gameMenu) && (Main.playerLoaded) && (Main.myPlayer >= 0);
		}

		private PDiffModPlayer GetMyModPlayer()
		{
			Player player = Main.player[Main.myPlayer];
			return player.GetModPlayer<PDiffModPlayer>();
		}

		public static void OnPlayerUnload()
		{
			mIsLeadInstanceInitializedFromPlayer = false;
		}
	}
}
