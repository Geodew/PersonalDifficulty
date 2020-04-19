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

//using System.Diagnostics;
//using PersonalDifficulty.PacketMessages;

namespace PersonalDifficulty
{
	// Note: At the time this was written, it was not possible for ModPlayer objects to write to ModConfig objects, so we have a bit of a roundabout way of figuring out if the current settings
	//       are loaded from a player or not.
	[Label("Personal Config Menu")]
	public class PDiffConfigLocal : ModConfig
	{
		public override ConfigScope Mode => ConfigScope.ClientSide;

		// "Lead Instance": tModLoader sets this
		public static PDiffConfigLocal Instance;

		// Most of these types are just for debugging purposes, but we need to be able to identify the Lead Instance and its copies, at least
		//private enum ConfigCreationTypeEnum
		//{
		//	eInvalidCreationType,  // = 0
		//	eLeadInstance,
		//	eCopyOfLeadInstance,
		//	eCopyOfCopyOfLeadInstance,
		//	eOtherFreshInstance,
		//	eOtherCopiedInstance
		//}

		// This bool tells us if this is a fresh instance or was created as a copy of an instance not initialized from player data (at least yet)
		// I know the guide says not to use static data members, but I think it means public static members. I have a good reason for needing this (see comment above class declaration).
		private static bool mIsLeadInstanceInitializedFromPlayer = false;

		// I know the guide says not to use static data members, but I think it means public static members. I have a good reason for needing this (see comment above class declaration).
		//private static bool mInstancesCreated = false;
		//private static bool mDoesLeadingInstanceExist = false;

		//private ConfigCreationTypeEnum mCreationType
		//{
		//	get;

		//	set;
		//} = ConfigCreationTypeEnum.eInvalidCreationType;

		// The size of this array should be the number of public class members/properties.
		// All bool variables have a default of `false`, so leaving that as-is is intentional here.
		//private const uint mNumberOfProperties = 8;
		//private bool[] mHasBeenSet = new bool[mNumberOfProperties];

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

		// Fresh instance constructor
		//public PDiffConfigLocal()
		//	: base()
		//{
		//	if (!mDoesLeadingInstanceExist)
		//	{
		//		Debug.Assert(
		//			!mInstancesCreated,
		//			"Logic Error: New non-copy instance created after destroying Lead Instance. Expected mod library unload instead.");

		//		mDoesLeadingInstanceExist = true;
		//		mCreationType = ConfigCreationTypeEnum.eLeadInstance;
		//	}
		//	else
		//	{
		//		mCreationType = ConfigCreationTypeEnum.eOtherFreshInstance;
		//	}

		//	mInstancesCreated = true;
		//}

		// Copy constructor
		//public PDiffConfigLocal(PDiffConfigLocal other)
		//	: base()  // ModConfig doesn't have a copy constructor
		//{
		//	// This should copy everything but mCreationType

		//	//zzz Make sure to copy base class stuff so we don't crash

		//	// Use get/set functions, so 'other' will initialize from the player now, if it can and hasn't already

		//	PlayerPowerScalarPercentage = other.PlayerPowerScalarPercentage;
		//	PlayerDamageDealtScalarPercentage = other.PlayerDamageDealtScalarPercentage;
		//	PlayerDamageTakenScalarPercentage = other.PlayerDamageTakenScalarPercentage;
		//	PlayerKnockbackDealtScalarPercentage = other.PlayerKnockbackDealtScalarPercentage;
		//	//PlayerKnockbackTakenScalarPercentage = other.PlayerKnockbackTakenScalarPercentage;  // Apparently this can't be done

		//	DisableKnockbackOnSelf = other.DisableKnockbackOnSelf;
		//	DisableFallDamageOnSelf = other.DisableFallDamageOnSelf;
		//	ShowDamageChangesInWeaponTooltip = other.ShowDamageChangesInWeaponTooltip;
		//	ShowKnockbackChangesInWeaponTooltip = other.ShowKnockbackChangesInWeaponTooltip;

		//	// 'other' should now have checked if it should be initialized from the player or not
		//	mIsInitializedFromPlayer = other.mIsInitializedFromPlayer;

		//	if (other.mCreationType == ConfigCreationTypeEnum.eLeadInstance)
		//	{
		//		mCreationType = ConfigCreationTypeEnum.eCopyOfLeadInstance;
		//	}
		//	else if ((other.mCreationType == ConfigCreationTypeEnum.eCopyOfLeadInstance) || (other.mCreationType == ConfigCreationTypeEnum.eCopyOfCopyOfLeadInstance))
		//	{
		//		mCreationType = ConfigCreationTypeEnum.eCopyOfCopyOfLeadInstance;
		//	}
		//	else
		//	{
		//		mCreationType = ConfigCreationTypeEnum.eOtherCopiedInstance;
		//	}
		//}

		//~PDiffConfigLocal()
		//{
		//	if (mCreationType == ConfigCreationTypeEnum.eLeadInstance)
		//	{
		//		mDoesLeadingInstanceExist = false;
		//	}
		//	else
		//	{
		//		Debug.Assert(
		//			mCreationType != ConfigCreationTypeEnum.eInvalidCreationType,
		//			"Logic Error: Instance not properly initialized.");
		//	}
		//}

		//public override ModConfig Clone()
		//{
		//	PDiffConfigLocal returnValue = new PDiffConfigLocal();

		//	// This should copy everything but mCreationType

		//	//zzz Make sure to copy base class stuff so we don't crash

		//	// Use get/set functions, so this instance will initialize from the player now, if it can and hasn't already

		//	returnValue.PlayerPowerScalarPercentage = PlayerPowerScalarPercentage;
		//	returnValue.PlayerDamageDealtScalarPercentage = PlayerDamageDealtScalarPercentage;
		//	returnValue.PlayerDamageTakenScalarPercentage = PlayerDamageTakenScalarPercentage;
		//	returnValue.PlayerKnockbackDealtScalarPercentage = PlayerKnockbackDealtScalarPercentage;
		//	//returnValue.PlayerKnockbackTakenScalarPercentage = PlayerKnockbackTakenScalarPercentage;  // Apparently this can't be done

		//	returnValue.DisableKnockbackOnSelf = DisableKnockbackOnSelf;
		//	returnValue.DisableFallDamageOnSelf = DisableFallDamageOnSelf;
		//	returnValue.ShowDamageChangesInWeaponTooltip = ShowDamageChangesInWeaponTooltip;
		//	returnValue.ShowKnockbackChangesInWeaponTooltip = ShowKnockbackChangesInWeaponTooltip;

		//	// 'other' should now have checked if it should be initialized from the player or not
		//	returnValue.mIsInitializedFromPlayer = mIsInitializedFromPlayer;

		//	if (mCreationType == ConfigCreationTypeEnum.eLeadInstance)
		//	{
		//		returnValue.mCreationType = ConfigCreationTypeEnum.eCopyOfLeadInstance;
		//	}
		//	else if ((mCreationType == ConfigCreationTypeEnum.eCopyOfLeadInstance) || (mCreationType == ConfigCreationTypeEnum.eCopyOfCopyOfLeadInstance))
		//	{
		//		returnValue.mCreationType = ConfigCreationTypeEnum.eCopyOfCopyOfLeadInstance;
		//	}
		//	else
		//	{
		//		returnValue.mCreationType = ConfigCreationTypeEnum.eOtherCopiedInstance;
		//	}

		//	return returnValue;

		//	return base.Clone();
		//}

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
				//mHasBeenSet[0] = true;

				//TryInitializeFromPlayer();
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
				//mHasBeenSet[1] = true;

				//TryInitializeFromPlayer();
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
				//mHasBeenSet[2] = true;

				//TryInitializeFromPlayer();
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
				//mHasBeenSet[3] = true;

				//TryInitializeFromPlayer();
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
				//mHasBeenSet[4] = true;

				//TryInitializeFromPlayer();
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
				//mHasBeenSet[5] = true;

				//TryInitializeFromPlayer();
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
				//mHasBeenSet[6] = true;

				//TryInitializeFromPlayer();
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
				//mHasBeenSet[7] = true;

				//TryInitializeFromPlayer();
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
			// Wait until tModLoader has initialized all fields with what it thinks goes there
			//for (uint i = 0;i < mNumberOfProperties; i++)
			//{
			//	if (!mHasBeenSet[i])
			//	{
			//		return;
			//	}
			//}

			// tModLoader is finished, now override with ModPlayer load data, if available
			//if ((mCreationType == ConfigCreationTypeEnum.eLeadInstance) || (mCreationType == ConfigCreationTypeEnum.eLeadInstance) || (mCreationType == ConfigCreationTypeEnum.eLeadInstance))
			//{

			// Changes might be set at the main menu, with no active player
			if (ReferenceEquals(this, Instance) && !mIsLeadInstanceInitializedFromPlayer && IsPlayerActive())
			{
				//Debug.Assert(
				//	(mCreationType != ConfigCreationTypeEnum.eLeadInstance) && (mCreationType != ConfigCreationTypeEnum.eLeadInstance),
				//	"Logic Error: Copy of Lead Instance persisted from menu into world load. Expected only Lead Instance persisting.");

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
