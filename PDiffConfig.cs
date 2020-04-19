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

		// These are not always used; see accessors below
		// These don't need defaults, as they should either be initialized by `TryInitializeFromPlayer` or tModLoader calling `set`
		private float _PlayerPowerScalarPercentage;
		private float _PlayerDamageDealtScalarPercentage;
		private float _PlayerDamageTakenScalarPercentage;
		private float _PlayerKnockbackDealtScalarPercentage;
		//private float _PlayerKnockbackTakenScalarPercentage;  // Apparently this can't be done

		// These are not always used; see accessors below
		// These don't need defaults, as they should either be initialized by `TryInitializeFromPlayer` or tModLoader calling `set`
		private bool _DisableKnockbackOnSelf;
		private bool _DisableFallDamageOnSelf;
		private bool _ShowDamageChangesInWeaponTooltip;
		private bool _ShowKnockbackChangesInWeaponTooltip;

		[Header("Main Power Slider\n - Set to 0 to disable all.\n - You can change a setting beyond the slider limits by editing your mod config file manually.\n - All settings are saved by character. Changes from the main menu will be applied only if you then load a character as of yet untouched by this mod.")]
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

		[DefaultValue(false)]
		[Label("Disable Knockback on Self")]
		[Tooltip("This disables knockback on only you.\nTerraria/tModLoader doesn't support knockback resistance on players, so this is the next best thing.\nThis setting is independent of the Power slider.")]
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
		[Tooltip("This disables fall damage on only you.\nNote that lowering your damage taken using other settings also affects fall damage,\nbut this is a good last resort to address any remaining frustration.\nThis setting is independent of the Power slider.")]
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

		[Header("Fine-Tuning\n - \"Power\" multiplies all these to arrive at the final effect.\n - Set to 0 to disable that specific change.\n - You can change a setting beyond the slider limits by editing your mod config file manually.\n - Note: All settings are saved by character. Changes from the main menu will be applied only if you then load a character as of yet untouched by this mod.")]
		[Range(0.0f, 200.0f)]
		[Increment(5.0f)]
		[DefaultValue(75.0f)]
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
		[DefaultValue(85.0f)]
		[Label("Player Damage Taken Increase Multiplier %")]
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
