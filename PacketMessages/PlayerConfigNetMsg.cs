using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace PersonalDifficulty.PacketMessages
{
	class PlayerConfigNetMsg
	{
		private const PacketMessageTypeEnum mPacketMessageType = PacketMessageTypeEnum.PLAYER_CONFIG;

		private int mPlayerId;
		//zzz add version information so we know what classes to expect from the other person and in what order

		private double mPlayerPowerScalar;
		private double mPlayerDamageDealtScalar;
		private double mPlayerDamageTakenScalar;
		private double mPlayerKnockbackDealtScalar;
		//private double mPlayerKnockbackTakenScalar;  // Apparently this can't be done
		private double mPlaceholder1;  //zzz Eventually add status effect duration increase/decrease on self
		private double mPlaceholder2;  //zzz Possibly add defense stuff
		private double mPlaceholder3;  //zzz Possibly add defense stuff

		private bool mDisableKnockbackOnSelf;
		private bool mDisableFallDamageOnSelf;
		private bool mShowDamageChangesInWeaponTooltip;
		private bool mShowKnockbackChangesInWeaponTooltip;


		private void Process(
				int senderPlayerId,
				Mod mod)
		{
			if (mPlayerId != Main.myPlayer)
			{
				Player player = Main.player[mPlayerId];
				PDiffModPlayer modPlayer = player.GetModPlayer<PDiffModPlayer>();

				modPlayer.mPlayerPowerScalar = mPlayerPowerScalar;
				modPlayer.mPlayerDamageDealtScalar = mPlayerDamageDealtScalar;
				modPlayer.mPlayerDamageTakenScalar = mPlayerDamageTakenScalar;
				modPlayer.mPlayerKnockbackDealtScalar = mPlayerKnockbackDealtScalar;
				//modPlayer.mPlayerKnockbackTakenScalar = mPlayerPowerScalar;  // Apparently this can't be done
				//zzz Eventually add status effect duration increase/decrease on self

				modPlayer.mDisableKnockbackOnSelf = mDisableKnockbackOnSelf;
				modPlayer.mDisableFallDamageOnSelf = mDisableFallDamageOnSelf;
				modPlayer.mShowDamageChangesInWeaponTooltip = mShowDamageChangesInWeaponTooltip;
				modPlayer.mShowKnockbackChangesInWeaponTooltip = mShowKnockbackChangesInWeaponTooltip;
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

				newPacket.Write(modPlayer.mPlayerPowerScalar);
				newPacket.Write(modPlayer.mPlayerDamageDealtScalar);
				newPacket.Write(modPlayer.mPlayerDamageTakenScalar);
				newPacket.Write(modPlayer.mPlayerKnockbackDealtScalar);
				//newPacket.Write(modPlayer.mPlayerKnockbackTakenScalar);  // Apparently this can't be done
				newPacket.Write(0.0);  //modPlayer.mPlaceholder1);  //zzz Eventually add status effect duration increase/decrease on self
				newPacket.Write(0.0);  //modPlayer.mPlaceholder2);  //zzz Possibly add defense stuff
				newPacket.Write(0.0);  //modPlayer.mPlaceholder3);  //zzz Possibly add defense stuff

				newPacket.Write(modPlayer.mDisableKnockbackOnSelf);
				newPacket.Write(modPlayer.mDisableFallDamageOnSelf);
				newPacket.Write(modPlayer.mShowDamageChangesInWeaponTooltip);
				newPacket.Write(modPlayer.mShowKnockbackChangesInWeaponTooltip);

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
			mPlaceholder1 = reader.ReadDouble();  //zzz Eventually add status effect duration increase/decrease on self
			mPlaceholder2 = reader.ReadDouble();  //zzz Possibly add defense stuff
			mPlaceholder3 = reader.ReadDouble();  //zzz Possibly add defense stuff

			mDisableKnockbackOnSelf = reader.ReadBoolean();
			mDisableFallDamageOnSelf = reader.ReadBoolean();
			mShowDamageChangesInWeaponTooltip = reader.ReadBoolean();
			mShowKnockbackChangesInWeaponTooltip = reader.ReadBoolean();
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
