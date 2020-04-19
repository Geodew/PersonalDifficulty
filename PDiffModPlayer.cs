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

		public override TagCompound Save()
		{
			TagCompound data = new TagCompound();

			try  // Prevent data loss or failed character save if something goes wrong in here
			{
				byte[] version = new byte[] { 0, 1, 0 };
				data.Add("Version", version);

				//zzz Eventually add status effect duration increase/decrease on self
				//double[] scalars = new double[] { mPlayerPowerScalar, mPlayerDamageDealtScalar, mPlayerDamageTakenScalar, mPlayerKnockbackDealtScalar, mPlayerKnockbackTakenScalar };
				List<double> scalars = new List<double> { mPlayerPowerScalar, mPlayerDamageDealtScalar, mPlayerDamageTakenScalar, mPlayerKnockbackDealtScalar };
				data.Add("Scalars", scalars);

				List<bool> toggles = new List<bool> { mDisableKnockbackOnSelf, mDisableFallDamageOnSelf, mShowDamageChangesInWeaponTooltip, mShowKnockbackChangesInWeaponTooltip };
				data.Add("Toggles", toggles);
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

				if ((version.Length == 3) && (version[0] == 0) && (version[1] == 1) && (version[2] == 0))
				{
					Load_v0_1_0(tag);

					ErrorCheckLoad();
				}
				else
				{
					//zzz print warning
				}
			}
			catch
			{
			}

			mLoadFinished = true;
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
				//zzz Eventually add status effect duration increase/decrease on self
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

		// For compatibility with old versions of tModLoader (note different input)
		public override void LoadLegacy(BinaryReader reader)
		{
			try  // Prevent data loss or failed character load if something goes wrong in here
			{
				UpdatePlayerFromConfigLoad();  // Set defaults in case loading throws an exception

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

					ErrorCheckLoad();
				}
			}
			catch
			{
			}

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
			config.PlayerPowerScalarPercentage = GetPlayerPowerScalarPercentage();
			config.PlayerDamageDealtScalarPercentage = GetPlayerDamageDealtScalarPercentage();
			config.PlayerDamageTakenScalarPercentage = GetPlayerDamageTakenScalarPercentage();
			config.PlayerKnockbackDealtScalarPercentage = GetPlayerKnockbackDealtScalarPercentage();
			//config.PlayerKnockbackTakenScalarPercentage = GetPlayerKnockbackTakenScalarPercentage();  // Apparently this can't be done
			//zzz Eventually add status effect duration increase/decrease on self

			config.DisableKnockbackOnSelf = mDisableKnockbackOnSelf;
			config.DisableFallDamageOnSelf = mDisableFallDamageOnSelf;
			config.ShowDamageChangesInWeaponTooltip = mShowDamageChangesInWeaponTooltip;
			config.ShowKnockbackChangesInWeaponTooltip = mShowKnockbackChangesInWeaponTooltip;
		}

		private void UpdatePlayerFromConfigLoad()
		{
			UpdatePlayerFromConfigLoad(ModContent.GetInstance<PDiffConfigLocal>());
		}

		public void UpdatePlayerFromConfigLoad(PDiffConfigLocal config)
		{
			SetPlayerPowerScalarPercentage(config.PlayerPowerScalarPercentage);
			SetPlayerDamageDealtScalarPercentage(config.PlayerDamageDealtScalarPercentage);
			SetPlayerDamageTakenScalarPercentage(config.PlayerDamageTakenScalarPercentage);
			SetPlayerKnockbackDealtScalarPercentage(config.PlayerKnockbackDealtScalarPercentage);
			//SetPlayerKnockbackTakenScalarPercentage(config.PlayerKnockbackTakenScalarPercentage);  // Apparently this can't be done

			mDisableKnockbackOnSelf = config.DisableKnockbackOnSelf;
			mDisableFallDamageOnSelf = config.DisableFallDamageOnSelf;
			mShowDamageChangesInWeaponTooltip = config.ShowDamageChangesInWeaponTooltip;
			mShowKnockbackChangesInWeaponTooltip = config.ShowKnockbackChangesInWeaponTooltip;

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

		public override void ResetEffects()
		{
			// We're going to set these in a lot of functions because we want these settings to override other mods forcing them to false, if we can
			player.noKnockback |= mDisableKnockbackOnSelf;
			player.noFallDmg |= mDisableFallDamageOnSelf;

			base.ResetEffects();
		}

		public override void PreUpdate()
		{
			// We're going to set these in a lot of functions because we want these settings to override other mods forcing them to false, if we can
			player.noKnockback |= mDisableKnockbackOnSelf;
			player.noFallDmg |= mDisableFallDamageOnSelf;

			base.PreUpdate();
		}

		// We could edit "player.allDamageMult" etc. here, but other mods may cap (floor or ceiling) that value for difficulty reasons. We need to change it in a way compatible with (circumventing) other mods.
		public override void PreUpdateBuffs()
		{
			// We're going to set these in a lot of functions because we want these settings to override other mods forcing them to false, if we can
			player.noKnockback |= mDisableKnockbackOnSelf;
			player.noFallDmg |= mDisableFallDamageOnSelf;

			base.PreUpdateBuffs();
		}

		public override void PostUpdateEquips()
		{
			// We're going to set these in a lot of functions because we want these settings to override other mods forcing them to false, if we can
			player.noKnockback |= mDisableKnockbackOnSelf;
			player.noFallDmg |= mDisableFallDamageOnSelf;

			base.PostUpdateEquips();
		}

		public override void PostUpdateRunSpeeds()
		{
			// We're going to set these in a lot of functions because we want these settings to override other mods forcing them to false, if we can
			player.noKnockback |= mDisableKnockbackOnSelf;
			player.noFallDmg |= mDisableFallDamageOnSelf;

			base.PostUpdateRunSpeeds();
		}

		public override void PostUpdate()
		{
			// We're going to set these in a lot of functions because we want these settings to override other mods forcing them to false, if we can
			player.noKnockback |= mDisableKnockbackOnSelf;
			player.noFallDmg |= mDisableFallDamageOnSelf;

			base.PostUpdate();
		}

		public override bool PreHurt(bool pvp, bool quiet, ref int damage, ref int hitDirection, ref bool crit, ref bool customDamage, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource)
		{
			// We're going to set these in a lot of functions because we want these settings to override other mods forcing them to false, if we can
			player.noKnockback |= mDisableKnockbackOnSelf;
			player.noFallDmg |= mDisableFallDamageOnSelf;

			AdjustDamageTaken(ref damage);

			//AdjustKnockbackTaken(ref knockback);  // Apparently this can't be done

			return base.PreHurt(pvp, quiet, ref damage, ref hitDirection, ref crit, ref customDamage, ref playSound, ref genGore, ref damageSource);
		}

		public override void ModifyHitNPCWithProj(Projectile proj, NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
		{
			if (!mShowDamageChangesInWeaponTooltip)
			{
				AdjustDamageDealt(ref damage);
			}

			if (!mShowKnockbackChangesInWeaponTooltip)
			{
				AdjustKnockbackDealt(ref knockback);
			}

			base.ModifyHitNPCWithProj(proj, target, ref damage, ref knockback, ref crit, ref hitDirection);
		}

		public override void ModifyHitNPC(Item item, NPC target, ref int damage, ref float knockback, ref bool crit)
		{
			if (!mShowDamageChangesInWeaponTooltip)
			{
				AdjustDamageDealt(ref damage);
			}

			if (!mShowKnockbackChangesInWeaponTooltip)
			{
				AdjustKnockbackDealt(ref knockback);
			}

			base.ModifyHitNPC(item, target, ref damage, ref knockback, ref crit);
		}

		public override void ModifyHitPvp(Item item, Player target, ref int damage, ref bool crit)
		{
			if (!mShowDamageChangesInWeaponTooltip)
			{
				AdjustDamageDealt(ref damage);
			}

			if (!mShowKnockbackChangesInWeaponTooltip)
			{
				//zzz How to adjust knockback for PvP?
				//AdjustKnockbackDealt(ref knockback);
			}

			base.ModifyHitPvp(item, target, ref damage, ref crit);
		}

		public override void ModifyHitPvpWithProj(Projectile proj, Player target, ref int damage, ref bool crit)
		{
			if (!mShowDamageChangesInWeaponTooltip)
			{
				AdjustDamageDealt(ref damage);
			}

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
				AdjustDamageDealtMult(ref mult);
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

		private int Round(double x)
		{
			return (int)Math.Round(x);
		}

		private void AdjustDamageDealt(ref int damage)
		{
			if (mPlayerPowerScalar >= 0.0)
			{
				damage = Round((double)damage * (1.0 + (mPlayerPowerScalar * mPlayerDamageDealtScalar)));
			}
			else
			{
				damage = Round((double)damage / (1.0 - (mPlayerPowerScalar * mPlayerDamageDealtScalar)));
			}
		}

		private void AdjustDamageDealtMult(ref float mult)
		{
			if (mPlayerPowerScalar >= 0.0)
			{
				mult = (float)((double)mult * (1.0 + (mPlayerPowerScalar * mPlayerDamageDealtScalar)));
			}
			else
			{
				mult = (float)((double)mult / (1.0 - (mPlayerPowerScalar * mPlayerDamageDealtScalar)));
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
			if (mPlayerPowerScalar >= 0.0)
			{
				damage = Round((double)damage / (1.0 + (mPlayerPowerScalar * mPlayerDamageTakenScalar)));
			}
			else
			{
				damage = Round((double)damage * (1.0 - (mPlayerPowerScalar * mPlayerDamageTakenScalar)));
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

		public void SetPlayerPowerScalarPercentage(float PlayerPowerScalarPercentageIn)
		{
			mPlayerPowerScalar = ((double)PlayerPowerScalarPercentageIn / 100.0);
		}

		public void SetPlayerDamageDealtScalarPercentage(float PlayerDamageDealtScalarPercentageIn)
		{
			mPlayerDamageDealtScalar = ((double)PlayerDamageDealtScalarPercentageIn / 100.0);
		}

		public void SetPlayerDamageTakenScalarPercentage(float PlayerDamageTakenScalarPercentageIn)
		{
			mPlayerDamageTakenScalar = ((double)PlayerDamageTakenScalarPercentageIn / 100.0);
		}

		public void SetPlayerKnockbackDealtScalarPercentage(float PlayerKnockbackDealtScalarPercentageIn)
		{
			mPlayerKnockbackDealtScalar = ((double)PlayerKnockbackDealtScalarPercentageIn / 100.0);
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
	}
}
