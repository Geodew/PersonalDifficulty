using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

using PersonalDifficulty.PacketMessages;

namespace PersonalDifficulty
{
	public class PDiffModPlayer : ModPlayer
	{
		private bool mLoadFinished = false;

		// v0.1.0
		public double mPlayerPowerScalar;
		public double mPlayerDamageDealtScalar;
		public double mPlayerDamageTakenScalar;
		public double mPlayerKnockbackDealtScalar;
		//public double mPlayerKnockbackTakenScalar;  // Apparently this can't be done
		//zzz Eventually add status effect duration increase/decrease on self

		public bool mDisableKnockbackOnSelf;
		public bool mDisableFallDamageOnSelf;
		public bool mShowDamageChangesInWeaponTooltip;
		public bool mShowKnockbackChangesInWeaponTooltip;

		// v0.2.0
		public double mEffectiveDamageDealtScalar;
		public double mEffectiveDamageTakenScalar;
		public float mGlowAmount;  // This is a float to save time on conversions: since we're inputting this directly into a function, there's no benefit from it being a double, only extra processing time. Note that it's still passed around as a double elsewhere for consistency in, for example, Save().
		public bool mDisableDrowningForSelf;
		public bool mDisableLavaDamageOnSelf;
		public bool mEnableGlow;

		// New Feature Comment Tag: Code change goes here


		public override TagCompound Save()
		{
			TagCompound data = new TagCompound();

			try  // Prevent data loss or failed character save if something goes wrong in here
			{
				byte[] version;
				List<double> scalars;
				List<bool> toggles;

				// New Feature Comment Tag: Code change goes here
				version = new byte[] { 0, 2, 0 };
				data.Add("Version", version);

				// v0.1.0
				scalars = new List<double> { mPlayerPowerScalar, mPlayerDamageDealtScalar, mPlayerDamageTakenScalar, mPlayerKnockbackDealtScalar };
				data.Add("Scalars", scalars);

				toggles = new List<bool> { mDisableKnockbackOnSelf, mDisableFallDamageOnSelf, mShowDamageChangesInWeaponTooltip, mShowKnockbackChangesInWeaponTooltip };
				data.Add("Toggles", toggles);

				// v0.2.0
				scalars = new List<double> { mEffectiveDamageDealtScalar, mEffectiveDamageTakenScalar, (double)mGlowAmount };
				data.Add("Scalars_v0_2_0", scalars);

				toggles = new List<bool> { mDisableDrowningForSelf, mDisableLavaDamageOnSelf, mEnableGlow };
				data.Add("Toggles_v0_2_0", toggles);

				// New Feature Comment Tag: Code change goes here
				//zzz Eventually add status effect duration increase/decrease on self
			}
			catch
			{
			}

			try  // Prevent data loss or failed character save if something goes wrong in here
			{
				PDiffConfigLocal.OnPlayerUnload();
			}
			catch
			{
			}

			return data;
		}

		public override void Load(TagCompound tag)
		{
			try  // Prevent data loss or failed character load if something goes wrong in here
			{
				UpdatePlayerFromConfigLoad();  // Set defaults in case loading throws an exception

				byte[] version = tag.GetByteArray("Version");

				if ((version != null) && (version.Length == 3) && (version[0] == 0) && (version[1] == 1) && (version[2] == 0))
				{
					Load_v0_1_0(tag);
				}
				else if ((version != null) && (version.Length == 3) && (version[0] == 0) && (version[1] == 2) && (version[2] == 0))
				{
					Load_v0_2_0(tag);
				}
				// New Feature Comment Tag: Code change goes here
				else
				{
					//zzz print warning
				}

				ErrorCheckLoad();  // If we didn't actually load anything, this should still be harmless
			}
			catch
			{
			}

			mLoadFinished = true;
		}

		public void Load_v0_2_0(TagCompound tag)
		{
			int i;

			Load_v0_1_0(tag);

			try
			{
				List<double> scalars = (List<double>)tag.GetList<double>("Scalars_v0_2_0");
				i = 0;
				mEffectiveDamageDealtScalar = scalars[i++];
				mEffectiveDamageTakenScalar = scalars[i++];
				mGlowAmount = (float)scalars[i++];
			}
			catch
			{
			}

			try
			{
				List<bool> toggles = (List<bool>)tag.GetList<bool>("Toggles_v0_2_0");
				i = 0;
				mDisableDrowningForSelf = toggles[i++];
				mDisableLavaDamageOnSelf = toggles[i++];
				mEnableGlow = toggles[i++];
			}
			catch
			{
			}
		}

		public void Load_v0_1_0(TagCompound tag)
		{
			int i;

			try
			{
				List<double> scalars = (List<double>)tag.GetList<double>("Scalars");
				i = 0;
				mPlayerPowerScalar = scalars[i++];
				mPlayerDamageDealtScalar = scalars[i++];
				mPlayerDamageTakenScalar = scalars[i++];
				mPlayerKnockbackDealtScalar = scalars[i++];
				//mPlayerKnockbackTakenScalar = scalars[i++];  // Apparently this can't be done
			}
			catch
			{
			}

			try
			{
				List<bool> toggles = (List<bool>)tag.GetList<bool>("Toggles");
				i = 0;
				mDisableKnockbackOnSelf = toggles[i++];
				mDisableFallDamageOnSelf = toggles[i++];
				mShowDamageChangesInWeaponTooltip = toggles[i++];
				mShowKnockbackChangesInWeaponTooltip = toggles[i++];
			}
			catch
			{
			}
		}

		// For compatibility with old versions of tModLoader? (note different input)
		public override void LoadLegacy(BinaryReader reader)
		{
			try  // Prevent data loss or failed character load if something goes wrong in here
			{
				UpdatePlayerFromConfigLoad();  // Set defaults in case loading throws an exception

				// v0.1.0
				if (reader.PeekChar() != -1)
				{
					mPlayerPowerScalar = reader.ReadDouble();
					mPlayerDamageDealtScalar = reader.ReadDouble();
					mPlayerDamageTakenScalar = reader.ReadDouble();
					mPlayerKnockbackDealtScalar = reader.ReadDouble();
					//mPlayerKnockbackTakenScalar = reader.ReadDouble();  // Apparently this can't be done
					//zzz Eventually add status effect duration increase/decrease on self

					mDisableKnockbackOnSelf = reader.ReadBoolean();
					mDisableFallDamageOnSelf = reader.ReadBoolean();
					mShowDamageChangesInWeaponTooltip = reader.ReadBoolean();
					mShowKnockbackChangesInWeaponTooltip = reader.ReadBoolean();
				}

				// v0.2.0
				if (reader.PeekChar() != -1)
				{
					mEffectiveDamageDealtScalar = reader.ReadDouble();
					mEffectiveDamageTakenScalar = reader.ReadDouble();
					mGlowAmount = (float)reader.ReadDouble();

					mDisableDrowningForSelf = reader.ReadBoolean();
					mDisableLavaDamageOnSelf = reader.ReadBoolean();
					mEnableGlow = reader.ReadBoolean();
				}

				// New Feature Comment Tag: Code change goes here
			}
			catch
			{
			}

			ErrorCheckLoad();

			mLoadFinished = true;
		}

		// Not yet a tModLoader feature.
		private void UpdateConfigFromPlayerLoad()
		{
			// Not yet a tModLoader feature.
			UpdateConfigFromPlayerLoad(ModContent.GetInstance<PDiffConfigLocal>());
		}

		public void UpdateConfigFromPlayerLoad(PDiffConfigLocal config)
		{
			// v0.1.0
			config.PlayerPowerScalarPercentage = GetPlayerPowerScalarPercentage();
			config.PlayerDamageDealtScalarPercentage = GetPlayerDamageDealtScalarPercentage();
			config.PlayerDamageTakenScalarPercentage = GetPlayerDamageTakenScalarPercentage();
			config.PlayerKnockbackDealtScalarPercentage = GetPlayerKnockbackDealtScalarPercentage();
			//config.PlayerKnockbackTakenScalarPercentage = GetPlayerKnockbackTakenScalarPercentage();  // Apparently this can't be done

			config.DisableKnockbackOnSelf = mDisableKnockbackOnSelf;
			config.DisableFallDamageOnSelf = mDisableFallDamageOnSelf;
			config.ShowDamageChangesInWeaponTooltip = mShowDamageChangesInWeaponTooltip;
			config.ShowKnockbackChangesInWeaponTooltip = mShowKnockbackChangesInWeaponTooltip;

			// v0.2.0
			config.EffectiveDamageDealtScalarPercentage = GetEffectiveDamageDealtScalarPercentage();
			config.EffectiveDamageTakenScalarPercentage = GetEffectiveDamageTakenScalarPercentage();
			config.GlowAmountPercentage = GetGlowAmountPercentage();
			config.DisableDrowningForSelf = mDisableDrowningForSelf;
			config.DisableLavaDamageOnSelf = mDisableLavaDamageOnSelf;
			config.EnableGlow = mEnableGlow;

			// New Feature Comment Tag: Code change goes here
			//zzz Eventually add status effect duration increase/decrease on self
		}

		private void UpdatePlayerFromConfigLoad()
		{
			UpdatePlayerFromConfigLoad(ModContent.GetInstance<PDiffConfigLocal>());
		}

		public void UpdatePlayerFromConfigLoad(PDiffConfigLocal config)
		{
			// v0.1.0
			SetPlayerPowerScalarPercentage(config.PlayerPowerScalarPercentage);
			SetPlayerDamageDealtScalarPercentage(config.PlayerDamageDealtScalarPercentage);
			SetPlayerDamageTakenScalarPercentage(config.PlayerDamageTakenScalarPercentage);
			SetPlayerKnockbackDealtScalarPercentage(config.PlayerKnockbackDealtScalarPercentage);
			//SetPlayerKnockbackTakenScalarPercentage(config.PlayerKnockbackTakenScalarPercentage);  // Apparently this can't be done

			mDisableKnockbackOnSelf = config.DisableKnockbackOnSelf;
			mDisableFallDamageOnSelf = config.DisableFallDamageOnSelf;
			mShowDamageChangesInWeaponTooltip = config.ShowDamageChangesInWeaponTooltip;
			mShowKnockbackChangesInWeaponTooltip = config.ShowKnockbackChangesInWeaponTooltip;

			// v0.2.0
			SetEffectiveDamageDealtScalarPercentage(config.EffectiveDamageDealtScalarPercentage);
			SetEffectiveDamageTakenScalarPercentage(config.EffectiveDamageTakenScalarPercentage);
			SetGlowAmountPercentage(config.GlowAmountPercentage);
			mDisableDrowningForSelf = config.DisableDrowningForSelf;
			mDisableLavaDamageOnSelf = config.DisableLavaDamageOnSelf;
			mEnableGlow = config.EnableGlow;

			// New Feature Comment Tag: Code change goes here

			if (mLoadFinished)
			{
				try
				{
					PlayerConfigNetMsg.SerializeAndSend(
						mod,
						this);
				}
				catch
				{
				}
			}
		}

		private void ErrorCheckLoad()
		{
			try
			{
				//zzz check for NaN and +/- Inf
				//zzz Ensuring non-zero isn't all that important, actually, let's just check the multipliers below during runtime to make sure they don't make damage negative
				//mPlayerDamageDealtScalar = Math.Max(0.0, mPlayerDamageDealtScalar);
				//mPlayerDamageTakenScalar = Math.Max(0.0, mPlayerDamageTakenScalar);
				//mPlayerKnockbackDealtScalar = Math.Max(0.0, mPlayerKnockbackDealtScalar);
				//mPlayerKnockbackTakenScalar = Math.Max(0.0, mPlayerKnockbackTakenScalar);  // Apparently this can't be done
				//zzz Eventually add status effect duration increase/decrease on self

				// v0.2.0
				mGlowAmount = Math.Max(0.0f, mGlowAmount);

				// New Feature Comment Tag: Code change goes here
			}
			catch
			{
				// Floating point exception in load, probably.
				UpdatePlayerFromConfigLoad();
			}
		}

		public override void OnEnterWorld(Player playerIn)
		{
			UpdateConfigFromPlayerLoad();  // Not yet a tModLoader feature.

			WelcomeMessage(playerIn);

			base.OnEnterWorld(playerIn);
		}

		private void WelcomeMessage(Player playerIn)
		{
			//zzz chat
		}

		public override void SyncPlayer(int toWho, int fromWho, bool newPlayer)
		{
			try
			{
				PlayerConfigNetMsg.SerializeAndSend(
					mod,
					this,
					toWho,
					fromWho);
			}
			catch
			{
			}

			base.SyncPlayer(toWho, fromWho, newPlayer);
		}

		// New Feature Comment Tag: Code change goes here (Add actual functionality in appropriate function)

		public override void ResetEffects()
		{
			// We're going to set these in a lot of functions because we want these settings to override other mods forcing them to false, if we can
			// v0.1.0
			player.noKnockback |= mDisableKnockbackOnSelf;
			player.noFallDmg |= mDisableFallDamageOnSelf;

			// v0.2.0
			player.gills |= mDisableDrowningForSelf;
			player.lavaImmune |= mDisableLavaDamageOnSelf;                 // Not hurt by lava
			player.fireWalk |= mDisableLavaDamageOnSelf;                   // Prevents damage from Hellstone and Meteorite blocks
			player.buffImmune[BuffID.OnFire] |= mDisableLavaDamageOnSelf;  // lavaImmune doesn't cover the ticking damage debuff

			base.ResetEffects();
		}

		public override void PreUpdate()
		{
			// We're going to set these in a lot of functions because we want these settings to override other mods forcing them to false, if we can
			// v0.1.0
			player.noKnockback |= mDisableKnockbackOnSelf;
			player.noFallDmg |= mDisableFallDamageOnSelf;

			// v0.2.0
			player.gills |= mDisableDrowningForSelf;
			player.lavaImmune |= mDisableLavaDamageOnSelf;                 // Not hurt by lava
			player.fireWalk |= mDisableLavaDamageOnSelf;                   // Prevents damage from Hellstone and Meteorite blocks
			player.buffImmune[BuffID.OnFire] |= mDisableLavaDamageOnSelf;  // lavaImmune doesn't cover the ticking damage debuff

			base.PreUpdate();
		}

		// We could edit "player.allDamageMult" etc. here, but other mods may cap (floor or ceiling) that value for difficulty reasons. We need to change it in a way compatible with (circumventing) other mods.
		public override void PreUpdateBuffs()
		{
			// We're going to set these in a lot of functions because we want these settings to override other mods forcing them to false, if we can
			// v0.1.0
			player.noKnockback |= mDisableKnockbackOnSelf;
			player.noFallDmg |= mDisableFallDamageOnSelf;

			// v0.2.0
			player.gills |= mDisableDrowningForSelf;
			player.lavaImmune |= mDisableLavaDamageOnSelf;                 // Not hurt by lava
			player.fireWalk |= mDisableLavaDamageOnSelf;                   // Prevents damage from Hellstone and Meteorite blocks
			player.buffImmune[BuffID.OnFire] |= mDisableLavaDamageOnSelf;  // lavaImmune doesn't cover the ticking damage debuff

			base.PreUpdateBuffs();
		}

		public override void PostUpdateEquips()
		{
			// We're going to set these in a lot of functions because we want these settings to override other mods forcing them to false, if we can
			// v0.1.0
			player.noKnockback |= mDisableKnockbackOnSelf;
			player.noFallDmg |= mDisableFallDamageOnSelf;

			// v0.2.0
			player.gills |= mDisableDrowningForSelf;
			player.lavaImmune |= mDisableLavaDamageOnSelf;                 // Not hurt by lava
			player.fireWalk |= mDisableLavaDamageOnSelf;                   // Prevents damage from Hellstone and Meteorite blocks
			player.buffImmune[BuffID.OnFire] |= mDisableLavaDamageOnSelf;  // lavaImmune doesn't cover the ticking damage debuff
			if (mEnableGlow)
			{
				// Note: We only want the Glow in one spot so we only spawn one "light source."
				// That's fine, though; it's unlikely that another mod will override us spawning light, since it's not a Player class property/variable.
				Lighting.AddLight(player.position, mGlowAmount, mGlowAmount, mGlowAmount);
			}

			base.PostUpdateEquips();
		}

		public override void PostUpdateRunSpeeds()
		{
			// We're going to set these in a lot of functions because we want these settings to override other mods forcing them to false, if we can
			// v0.1.0
			player.noKnockback |= mDisableKnockbackOnSelf;
			player.noFallDmg |= mDisableFallDamageOnSelf;

			// v0.2.0
			player.gills |= mDisableDrowningForSelf;
			player.lavaImmune |= mDisableLavaDamageOnSelf;                 // Not hurt by lava
			player.fireWalk |= mDisableLavaDamageOnSelf;                   // Prevents damage from Hellstone and Meteorite blocks
			player.buffImmune[BuffID.OnFire] |= mDisableLavaDamageOnSelf;  // lavaImmune doesn't cover the ticking damage debuff

			base.PostUpdateRunSpeeds();
		}

		public override void PostUpdate()
		{
			// We're going to set these in a lot of functions because we want these settings to override other mods forcing them to false, if we can
			// v0.1.0
			player.noKnockback |= mDisableKnockbackOnSelf;
			player.noFallDmg |= mDisableFallDamageOnSelf;

			// v0.2.0
			player.gills |= mDisableDrowningForSelf;
			player.lavaImmune |= mDisableLavaDamageOnSelf;                 // Not hurt by lava
			player.fireWalk |= mDisableLavaDamageOnSelf;                   // Prevents damage from Hellstone and Meteorite blocks
			player.buffImmune[BuffID.OnFire] |= mDisableLavaDamageOnSelf;  // lavaImmune doesn't cover the ticking damage debuff

			base.PostUpdate();
		}

		public override bool PreHurt(bool pvp, bool quiet, ref int damage, ref int hitDirection, ref bool crit, ref bool customDamage, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource)
		{
			// We're going to set these in a lot of functions because we want these settings to override other mods forcing them to false, if we can
			// v0.1.0
			player.noKnockback |= mDisableKnockbackOnSelf;
			player.noFallDmg |= mDisableFallDamageOnSelf;

			// v0.2.0
			player.gills |= mDisableDrowningForSelf;
			player.lavaImmune |= mDisableLavaDamageOnSelf;                 // Not hurt by lava
			player.fireWalk |= mDisableLavaDamageOnSelf;                   // Prevents damage from Hellstone and Meteorite blocks
			player.buffImmune[BuffID.OnFire] |= mDisableLavaDamageOnSelf;  // lavaImmune doesn't cover the ticking damage debuff

			AdjustDamageTaken(ref damage);

			//AdjustKnockbackTaken(ref knockback);  // Apparently this can't be done

			return base.PreHurt(pvp, quiet, ref damage, ref hitDirection, ref crit, ref customDamage, ref playSound, ref genGore, ref damageSource);
		}

		public override void ModifyHitNPCWithProj(Projectile proj, NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
		{
			AdjustDamageDealt(ref damage, target);

			if (!mShowKnockbackChangesInWeaponTooltip)
			{
				AdjustKnockbackDealt(ref knockback);
			}

			base.ModifyHitNPCWithProj(proj, target, ref damage, ref knockback, ref crit, ref hitDirection);
		}

		public override void ModifyHitNPC(Item item, NPC target, ref int damage, ref float knockback, ref bool crit)
		{
			AdjustDamageDealt(ref damage, target);

			if (!mShowKnockbackChangesInWeaponTooltip)
			{
				AdjustKnockbackDealt(ref knockback);
			}

			base.ModifyHitNPC(item, target, ref damage, ref knockback, ref crit);
		}

		public override void ModifyHitPvp(Item item, Player target, ref int damage, ref bool crit)
		{
			AdjustDamageDealt(ref damage, target);

			if (!mShowKnockbackChangesInWeaponTooltip)
			{
				//zzz How to adjust knockback for PvP?
				//AdjustKnockbackDealt(ref knockback);
			}

			base.ModifyHitPvp(item, target, ref damage, ref crit);
		}

		public override void ModifyHitPvpWithProj(Projectile proj, Player target, ref int damage, ref bool crit)
		{
			AdjustDamageDealt(ref damage, target);

			if (!mShowKnockbackChangesInWeaponTooltip)
			{
				//zzz How to adjust knockback for PvP?
				//AdjustKnockbackDealt(ref knockback);
			}

			base.ModifyHitPvpWithProj(proj, target, ref damage, ref crit);
		}

		public override void ModifyWeaponDamage(Item item, ref float add, ref float mult, ref float flat)
		{
			if (mShowDamageChangesInWeaponTooltip)
			{
				// Target's Defense and effective damage calculations is accounted for later
				AdjustBaseDamageDealtMult(ref mult);
			}

			base.ModifyWeaponDamage(item, ref add, ref mult, ref flat);
		}

		public override void GetWeaponKnockback(Item item, ref float knockback)
		{
			if (mShowKnockbackChangesInWeaponTooltip)
			{
				AdjustKnockbackDealt(ref knockback);
			}

			base.GetWeaponKnockback(item, ref knockback);
		}

		private void AdjustDamageDealt(ref int damage, NPC target)
		{
			double mult = 1.0;
			double newBaseDamage;

			AdjustBaseDamageDealtMult(ref mult);

			if (mShowDamageChangesInWeaponTooltip)
			{
				newBaseDamage = damage;
			}
			else
			{
				newBaseDamage = ((double)damage * mult);
			}

			damage = Round(newBaseDamage - GetDefenseDamageReducedEnemy(damage, target) * (mEffectiveDamageDealtScalar * (mult - 1.0)));

			if (damage < 0)
			{
				damage = 0;
			}
		}

		private void AdjustDamageDealt(ref int damage, Player target)
		{
			double mult = 1.0;
			double newBaseDamage;

			AdjustBaseDamageDealtMult(ref mult);

			if (mShowDamageChangesInWeaponTooltip)
			{
				newBaseDamage = damage;
			}
			else
			{
				newBaseDamage = ((double)damage * mult);
			}

			damage = Round(newBaseDamage - GetDefenseDamageReducedEnemy(damage, target) * (mEffectiveDamageDealtScalar * (mult - 1.0)));

			if (damage < 0)
			{
				damage = 0;
			}
		}

		private void AdjustBaseDamageDealtMult(ref float mult)
		{
			if (mPlayerPowerScalar >= 0.0)
			{
				mult = (float)((double)mult * (1.0 + (mPlayerPowerScalar * mPlayerDamageDealtScalar)));
			}
			else
			{
				mult = (float)((double)mult / (1.0 - (mPlayerPowerScalar * mPlayerDamageDealtScalar)));
			}

			if (mult < 0.0f)
			{
				mult = 0.0f;
			}
		}

		private void AdjustBaseDamageDealtMult(ref double mult)
		{
			if (mPlayerPowerScalar >= 0.0)
			{
				mult = (mult * (1.0 + (mPlayerPowerScalar * mPlayerDamageDealtScalar)));
			}
			else
			{
				mult = (mult / (1.0 - (mPlayerPowerScalar * mPlayerDamageDealtScalar)));
			}

			if (mult < 0.0)
			{
				mult = 0.0;
			}
		}

		private void AdjustKnockbackDealt(ref float knockback)
		{
			if (mPlayerPowerScalar >= 0.0)
			{
				knockback = (float)((double)knockback * (1.0 + (mPlayerPowerScalar * mPlayerKnockbackDealtScalar)));
			}
			else
			{
				knockback = (float)((double)knockback / (1.0 - (mPlayerPowerScalar * mPlayerKnockbackDealtScalar)));
			}
		}

		private void AdjustDamageTaken(ref int damage)
		{
			double mult;

			if (mPlayerPowerScalar >= 0.0)
			{
				mult = 1.0 / (1.0 + (mPlayerPowerScalar * mPlayerDamageTakenScalar));
			}
			else
			{
				mult = (1.0 - (mPlayerPowerScalar * mPlayerDamageTakenScalar));
			}

			damage = Round(((double)damage * mult) - GetDefenseDamageReducedSelf(damage) * (mEffectiveDamageTakenScalar * (mult - 1.0)));

			if (damage < 0)
			{
				damage = 0;
			}
		}

		//  ===== Apparently this can't be done =====
		//private void AdjustKnockbackTaken(ref float knockback)
		//{
		//	if (mPlayerPowerScalar >= 0.0)
		//	{
		//		knockback = Round((double)knockback / (1.0 + (mPlayerPowerScalar * mPlayerKnockbackTakenScalar)));
		//	}
		//	else
		//	{
		//		knockback = Round((double)knockback * (1.0 - (mPlayerPowerScalar * mPlayerKnockbackTakenScalar)));
		//	}
		//}
		//  ===== Apparently this can't be done =====

		// Equal to: Original Base Damage - Original Effective Damage
		private double GetDefenseDamageReducedEnemy(int damage, NPC target)
		{
			//double defenseMultiplier = (Main.expertMode ? 0.75 : 0.5);
			double defenseMultiplier = 0.5;  // NPCs' damage reduction from Defense doesn't change in Expert Mode (according to Wiki)
			int baseDamage;  // Original damage value

			if (mShowDamageChangesInWeaponTooltip)
			{
				double damageMultiplier = 1.0;

				AdjustBaseDamageDealtMult(ref damageMultiplier);

				// Not quite a perfect inversion, but we'll manage
				baseDamage = Round((double)damage / damageMultiplier);
			}
			else
			{
				baseDamage = damage;
			}

			// Wiki says damage reduction from Defense is rounded up:                   https://terraria.gamepedia.com/Defense#Mechanics
			// Wiki says damage reduction/increase multiplier is applied after Defense: https://terraria.gamepedia.com/Defense#Damage_reduction_boosts
			// This might be negative if the target has a large damage taken multiplier (damage taken increase). That's expected.
			//return Math.Round(Math.Ceiling(target.defense * defenseMultiplier) * (double)target.takenDamageMultiplier + (double)baseDamage * (1.0 - (double)target.takenDamageMultiplier));
			return Math.Round((double)baseDamage - (((double)baseDamage - Math.Ceiling(target.defense * defenseMultiplier)) * (double)target.takenDamageMultiplier));
		}

		// Equal to: Base Damage - Effective Damage
		private double GetDefenseDamageReducedEnemy(int damage, Player target)
		{
			double defenseMultiplier = (Main.expertMode ? 0.75 : 0.5);
			int baseDamage;  // Original damage value
			double endurance = (target.endurance > -1.0f) ? (double)target.endurance : -0.99;  // If another mod breaks the player's endurance such that they should take infinite damage, then with this, we won't crash. It'll probably crash somewhere else, but at least people might not report the bug to me...?

			if (mShowDamageChangesInWeaponTooltip)
			{
				double damageMultiplier = 1.0;

				AdjustBaseDamageDealtMult(ref damageMultiplier);

				// Not quite a perfect inversion, but we'll manage
				baseDamage = Round((double)damage / damageMultiplier);
			}
			else
			{
				baseDamage = damage;
			}

			// Wiki says damage reduction from Defense is rounded up:                   https://terraria.gamepedia.com/Defense#Mechanics
			// Wiki says damage reduction/increase multiplier is applied after Defense: https://terraria.gamepedia.com/Defense#Damage_reduction_boosts
			// This might be negative if the target has a large damage taken multiplier (damage taken increase). That's expected.
			//return Math.Round(Math.Ceiling(target.defense * defenseMultiplier) * (double)target.takenDamageMultiplier + (double)damage * (1.0 - (double)target.takenDamageMultiplier));
			return Math.Round((double)baseDamage - (((double)baseDamage - Math.Ceiling(target.statDefense * defenseMultiplier)) / (1.0 + endurance)));
		}

		// Equal to: Base Damage - Effective Damage
		private double GetDefenseDamageReducedSelf(int damage)
		{
			double defenseMultiplier = (Main.expertMode ? 0.75 : 0.5);
			int baseDamage = damage;  // Original damage value: This is provided by the other NPC/Player object, so this player's tooltip options don't matter
			double endurance = (player.endurance > -1.0f) ? (double)player.endurance : -0.99;  // If another mod breaks the player's endurance such that they should take infinite damage, then with this, we won't crash. It'll probably crash somewhere else, but at least people might not report the bug to me...?

			// Wiki says damage reduction from Defense is rounded up:                   https://terraria.gamepedia.com/Defense#Mechanics
			// Wiki says damage reduction/increase multiplier is applied after Defense: https://terraria.gamepedia.com/Defense#Damage_reduction_boosts
			// This might be negative if the target has a large damage taken multiplier (damage taken increase). That's expected.
			//return Math.Round(Math.Ceiling(target.defense * defenseMultiplier) * (double)target.takenDamageMultiplier + (double)damage * (1.0 - (double)target.takenDamageMultiplier));
			return Math.Round((double)baseDamage - (((double)baseDamage - Math.Ceiling(player.statDefense * defenseMultiplier)) / (1.0 + endurance)));
		}

		private int Round(double x)
		{
			return (int)Math.Round(x);
		}

		public bool GetLoadFinished()
		{
			return mLoadFinished;
		}

		public float GetPlayerPowerScalarPercentage()
		{
			return (float)(mPlayerPowerScalar * 100.0);
		}

		public float GetPlayerDamageDealtScalarPercentage()
		{
			return (float)(mPlayerDamageDealtScalar * 100.0);
		}

		public float GetPlayerDamageTakenScalarPercentage()
		{
			return (float)(mPlayerDamageTakenScalar * 100.0);
		}

		public float GetPlayerKnockbackDealtScalarPercentage()
		{
			return (float)(mPlayerKnockbackDealtScalar * 100.0);
		}

		//  ===== Apparently this can't be done =====
		//public float GetPlayerKnockbackTakenScalarPercentage()
		//{
		//	return (float)(mPlayerKnockbackTakenScalar * 100.0);
		//}
		//  ===== Apparently this can't be done =====

		//zzz Eventually add status effect duration increase/decrease on self
		//public float Get...()
		//{
		//	return (float)(... * 100.0);
		//}

		public float GetEffectiveDamageDealtScalarPercentage()
		{
			return (float)(mEffectiveDamageDealtScalar * 100.0);
		}

		public float GetEffectiveDamageTakenScalarPercentage()
		{
			return (float)(mEffectiveDamageTakenScalar * 100.0);
		}

		public float GetGlowAmountPercentage()
		{
			return (mGlowAmount * 100.0f);
		}

		// New Feature Comment Tag: Code change goes here (floats only)

		public void SetPlayerPowerScalarPercentage(float playerPowerScalarPercentage)
		{
			mPlayerPowerScalar = ((double)playerPowerScalarPercentage / 100.0);
		}

		public void SetPlayerDamageDealtScalarPercentage(float playerDamageDealtScalarPercentage)
		{
			mPlayerDamageDealtScalar = ((double)playerDamageDealtScalarPercentage / 100.0);
		}

		public void SetPlayerDamageTakenScalarPercentage(float playerDamageTakenScalarPercentage)
		{
			mPlayerDamageTakenScalar = ((double)playerDamageTakenScalarPercentage / 100.0);
		}

		public void SetPlayerKnockbackDealtScalarPercentage(float playerKnockbackDealtScalarPercentage)
		{
			mPlayerKnockbackDealtScalar = ((double)playerKnockbackDealtScalarPercentage / 100.0);
		}

		//  ===== Apparently this can't be done =====
		//public void SetPlayerKnockbackTakenScalarPercentage(float PlayerKnockbackTakenScalarPercentageIn)
		//{
		//	mPlayerKnockbackTakenScalar =  ((double)PlayerKnockbackTakenScalarPercentageIn / 100.0);
		//}
		//  ===== Apparently this can't be done =====

		//zzz Eventually add status effect duration increase/decrease on self
		//public void Set...(float ...)
		//{
		//	... = ((double)... / 100.0);
		//}

		public void SetEffectiveDamageDealtScalarPercentage(float effectiveDamageDealtScalarPercentage)
		{
			mEffectiveDamageDealtScalar = ((double)effectiveDamageDealtScalarPercentage / 100.0);
		}

		public void SetEffectiveDamageTakenScalarPercentage(float effectiveDamageTakenScalarPercentage)
		{
			mEffectiveDamageTakenScalar = ((double)effectiveDamageTakenScalarPercentage / 100.0);
		}

		public void SetGlowAmountPercentage(float glowAmountPercentage)
		{
			mGlowAmount = (glowAmountPercentage / 100.0f);
		}

		// New Feature Comment Tag: Code change goes here (floats only)
	}
}
