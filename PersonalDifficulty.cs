using Microsoft.Xna.Framework;
using System;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

using PersonalDifficulty.PacketMessages;

namespace PersonalDifficulty
{
	public class PersonalDifficulty : Mod
	{
		public PersonalDifficulty()
		{
			// By default, all Autoload properties are True. You only need to change this if you know what you are doing.
			//Properties = new ModProperties()
			//{
			//	Autoload = true,
			//	AutoloadGores = true,
			//	AutoloadSounds = true,
			//	AutoloadBackgrounds = true
			//};
		}

		public override void Load()
		{
			base.Load();
		}

		// tModLoader calls senderPlayerId `whoAmI` instead, but this name makes more sense to me.
		public override void HandlePacket(BinaryReader reader, int senderPlayerId)
		{
			PacketMessageTypeEnum messageType = (PacketMessageTypeEnum)reader.ReadInt32();
			switch (messageType)
			{
				case PacketMessageTypeEnum.PLAYER_CONFIG:
					PlayerConfigNetMsg playerConfigNetMsg = new PlayerConfigNetMsg();
					playerConfigNetMsg.HandlePacket(
						reader,
						senderPlayerId,
						this);
					break;

				default:
					//zzz log/warn unhandled message
					break;
			}

			base.HandlePacket(reader, senderPlayerId);
		}
	}
}
