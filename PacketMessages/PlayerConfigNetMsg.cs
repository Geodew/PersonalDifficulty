using System.Diagnostics;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace PersonalDifficulty.PacketMessages
{
	class PlayerConfigNetMsg
	{
		private const PacketMessageTypeEnum mPacketMessageType = PacketMessageTypeEnum.PLAYER_CONFIG;

		// Version numbers should be in ascending order so >= operator works as intended
		enum MessageVersionEnum
		{
			eVersionInvalid,
			eVersion_0_1_1,
			eVersion_0_2_0

			// New Feature Comment Tag: Code change goes here
		}
		private const MessageVersionEnum eLatestVersion = MessageVersionEnum.eVersion_0_2_0;  // New Feature Comment Tag: Code change goes here

		private int mPlayerId;

		private double mPlayerPowerScalar;
		private double mPlayerDamageDealtScalar;
		private double mPlayerDamageTakenScalar;
		private double mPlayerKnockbackDealtScalar;
		//private double mPlayerKnockbackTakenScalar;  // Apparently this can't be done

		private bool mDisableKnockbackOnSelf;
		private bool mDisableFallDamageOnSelf;
		private bool mShowDamageChangesInWeaponTooltip;
		private bool mShowKnockbackChangesInWeaponTooltip;

		// Stuff after this was added in eVersion_0_2_0 or later
		MessageVersionEnum mMessageVersion = MessageVersionEnum.eVersionInvalid;

		// eVersion_0_2_0
		private double mEffectiveDamageDealtScalar;
		private double mEffectiveDamageTakenScalar;
		private double mGlowAmount;
		private bool mDisableDrowningForSelf;
		private bool mDisableLavaDamageOnSelf;
		private bool mEnableGlow;

		// New Feature Comment Tag: Code change goes here
		//zzz Eventually add status effect duration increase/decrease on self


		private void Process(
				int senderPlayerId,
				Mod mod)
		{
			if (mPlayerId != Main.myPlayer)
			{
				Player player = Main.player[mPlayerId];
				PDiffModPlayer modPlayer = player.GetModPlayer<PDiffModPlayer>();

				// MessageVersionEnum.eVersion_0_1_1 or earlier
				Debug.Assert(mMessageVersion >= MessageVersionEnum.eVersion_0_1_1);

				modPlayer.mPlayerPowerScalar = mPlayerPowerScalar;
				modPlayer.mPlayerDamageDealtScalar = mPlayerDamageDealtScalar;
				modPlayer.mPlayerDamageTakenScalar = mPlayerDamageTakenScalar;
				modPlayer.mPlayerKnockbackDealtScalar = mPlayerKnockbackDealtScalar;
				//modPlayer.mPlayerKnockbackTakenScalar = mPlayerPowerScalar;  // Apparently this can't be done

				modPlayer.mDisableKnockbackOnSelf = mDisableKnockbackOnSelf;
				modPlayer.mDisableFallDamageOnSelf = mDisableFallDamageOnSelf;
				modPlayer.mShowDamageChangesInWeaponTooltip = mShowDamageChangesInWeaponTooltip;
				modPlayer.mShowKnockbackChangesInWeaponTooltip = mShowKnockbackChangesInWeaponTooltip;

				if (mMessageVersion >= MessageVersionEnum.eVersion_0_2_0)
				{
					modPlayer.mEffectiveDamageDealtScalar = mEffectiveDamageDealtScalar;
					modPlayer.mEffectiveDamageTakenScalar = mEffectiveDamageTakenScalar;
					modPlayer.mGlowAmount = (float)mGlowAmount;
					modPlayer.mDisableDrowningForSelf = mDisableDrowningForSelf;
					modPlayer.mDisableLavaDamageOnSelf = mDisableLavaDamageOnSelf;
					modPlayer.mEnableGlow = mEnableGlow;
				}


				// New Feature Comment Tag: Code change goes here
				//zzz Eventually add status effect duration increase/decrease on self
			}
		}

		public void HandlePacket(
				BinaryReader reader,
				int senderPlayerId,
				Mod mod)
		{
			Deserialize(
				reader,
				senderPlayerId);
			ServerBroadcast(
				senderPlayerId,
				mod);
			Process(
				senderPlayerId,
				mod);
		}

		public static void SerializeAndSend(
				Mod mod,
				PDiffModPlayer modPlayer,
				int toWho = -1,    // These defaults must match the defaults of newPacket.Send
				int fromWho = -1)  // These defaults must match the defaults of newPacket.Send
		{
			if (Main.netMode != NetmodeID.SinglePlayer)
			{
				ModPacket newPacket = mod.GetPacket();

				newPacket.Write((int)mPacketMessageType);

				newPacket.Write(modPlayer.player.whoAmI);

				// MessageVersionEnum.eVersion_0_1_1 or earlier
				newPacket.Write(modPlayer.mPlayerPowerScalar);
				newPacket.Write(modPlayer.mPlayerDamageDealtScalar);
				newPacket.Write(modPlayer.mPlayerDamageTakenScalar);
				newPacket.Write(modPlayer.mPlayerKnockbackDealtScalar);
				//newPacket.Write(modPlayer.mPlayerKnockbackTakenScalar);  // Apparently this can't be done
				newPacket.Write((double)modPlayer.mGlowAmount);          // Note: Glow Amount added in place of placeholder here
				newPacket.Write(modPlayer.mEffectiveDamageDealtScalar);  // Note: Defense stuff added in place of placeholders here
				newPacket.Write(modPlayer.mEffectiveDamageTakenScalar);  // Note: Defense stuff added in place of placeholders here

				newPacket.Write(modPlayer.mDisableKnockbackOnSelf);
				newPacket.Write(modPlayer.mDisableFallDamageOnSelf);
				newPacket.Write(modPlayer.mShowDamageChangesInWeaponTooltip);
				newPacket.Write(modPlayer.mShowKnockbackChangesInWeaponTooltip);

				// Stuff after this was added in eVersion_0_2_0 or later
				newPacket.Write((int)eLatestVersion);

				// MessageVersionEnum.eVersion_0_2_0
				// Note: Defense stuff and Glow Amount added in place of placeholders above
				//mEffectiveDamageDealtScalar
				//mEffectiveDamageTakenScalar
				//mGlowAmount
				newPacket.Write(modPlayer.mDisableDrowningForSelf);
				newPacket.Write(modPlayer.mDisableLavaDamageOnSelf);
				newPacket.Write(modPlayer.mEnableGlow);


				// New Feature Comment Tag: Code change goes here
				//zzz Eventually add status effect duration increase/decrease on self

				newPacket.Send(toWho, fromWho);
			}
		}

		private void Deserialize(
				BinaryReader reader,
				int senderPlayerId)
		{
			mPlayerId = reader.ReadInt32();

			mPlayerPowerScalar = reader.ReadDouble();
			mPlayerDamageDealtScalar = reader.ReadDouble();
			mPlayerDamageTakenScalar = reader.ReadDouble();
			mPlayerKnockbackDealtScalar = reader.ReadDouble();
			//mPlayerKnockbackTakenScalar = reader.ReadDouble();  // Apparently this can't be done
			mGlowAmount = reader.ReadDouble();                  // Note: Glow Amount added in place of placeholder here
			mEffectiveDamageDealtScalar = reader.ReadDouble();  // Note: Defense stuff added in place of placeholders here
			mEffectiveDamageTakenScalar = reader.ReadDouble();  // Note: Defense stuff added in place of placeholders here

			mDisableKnockbackOnSelf = reader.ReadBoolean();
			mDisableFallDamageOnSelf = reader.ReadBoolean();
			mShowDamageChangesInWeaponTooltip = reader.ReadBoolean();
			mShowKnockbackChangesInWeaponTooltip = reader.ReadBoolean();

			if (reader.PeekChar() == 0)
			{
				mMessageVersion = MessageVersionEnum.eVersion_0_1_1;
			}
			else
			{
				int messageVersionTemp = reader.ReadInt32();

				if ((messageVersionTemp <= (int)MessageVersionEnum.eVersionInvalid) || (messageVersionTemp > (int)eLatestVersion))
				{
					//zzz Print & Log error

					// The other person is likely using a newer version, but let's load everything that we can. Some desync will likely happen, though.
					mMessageVersion = eLatestVersion;
				}
				else
				{
					mMessageVersion = (MessageVersionEnum)messageVersionTemp;
				}
			}

			if (mMessageVersion >= MessageVersionEnum.eVersion_0_2_0)
			{
				// Note: Defense stuff and Glow Amount added in place of placeholders above
				//mEffectiveDamageDealtScalar
				//mEffectiveDamageTakenScalar
				//mGlowAmount
				mDisableDrowningForSelf = reader.ReadBoolean();
				mDisableLavaDamageOnSelf = reader.ReadBoolean();
				mEnableGlow = reader.ReadBoolean();
			}


			// New Feature Comment Tag: Code change goes here
		}

		private void ServerBroadcast(
				int senderPlayerId,
				Mod mod)
		{
			if (Main.netMode == NetmodeID.Server)
			{
				Player player = Main.player[mPlayerId];
				PDiffModPlayer modPlayer = player.GetModPlayer<PDiffModPlayer>();

				SerializeAndSend(
					mod,
					modPlayer,
					-1,
					mPlayerId);
			}
		}
	}
}
