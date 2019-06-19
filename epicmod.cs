using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Graphics;
using ModConfiguration;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace epicmod {
    public class epicmod : Mod {
        public static readonly ModConfig Config = new ModConfig("epicmod");

        public override void Load() {
            Properties = new ModProperties() {
                Autoload = true,
                AutoloadBackgrounds = true,
                AutoloadSounds = true
            };

            Config.Load();
        }

        public override void PostDrawInterface(SpriteBatch spriteBatch) {
            epicmodPlayer wsp = Main.player[Main.myPlayer].GetModPlayer<epicmodPlayer>(this);
            wsp.Draw(spriteBatch);
        }

        public override void HandlePacket(BinaryReader reader, int whoAmI) {
            PacketMessageType message = (PacketMessageType)reader.ReadByte();
            byte player = reader.ReadByte();
            epicmodPlayer modPlayer = Main.player[player].GetModPlayer<epicmodPlayer>();

            switch(message) {
                case PacketMessageType.All:

                    for(int i = 1; i <= epicmodPlayer.accessories; i++) {
                        modPlayer.slots[i - 1].Item = ItemIO.Receive(reader);
                    }

                    if(Main.netMode == 2) {
                        ModPacket packet = GetPacket();
                        packet.Write((byte)PacketMessageType.All);
                        packet.Write(player);
                        for(int i = 1; i <= epicmodPlayer.accessories; i++) {
                            ItemIO.Send(modPlayer.slots[i - 1].Item, packet);
                        }
                        packet.Send(-1, whoAmI);
                    }
                    break;
                case PacketMessageType.EquipSlot:
                    for(int i = 1; i <= epicmodPlayer.accessories; i++) {
                        modPlayer.slots[i - 1].Item = ItemIO.Receive(reader);
                    }
                    if(Main.netMode == 2) {

                        for(int i = 1; i <= epicmodPlayer.accessories; i++) {
                            modPlayer.SendSingleItemPacket(PacketMessageType.EquipSlot, modPlayer.slots[i - 1].Item, -1, whoAmI);
                        }
                    }
                    break;
                default:
                    ErrorLogger.Log("epic Slot: Unknown message type: " + message);
                    break;
            }
        }

    }
}
