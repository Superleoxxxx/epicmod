using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.UI;
using TerraUI.Objects;

namespace epicmod {
    internal class epicmodPlayer : ModPlayer {
        private const string prefix = "epicaccessory";
        public const int accessories = 42;
        public UIItemSlot[] slots;

        public override void clientClone(ModPlayer clientClone) {
            epicmodPlayer clone = clientClone as epicmodPlayer;

            if(clone == null) {
                return;
            }
            for(int i = 1; i <= accessories; i++) {
               
                clone.slots[i - 1].Item = slots[i - 1].Item.Clone();
            }
        }

        public override void SendClientChanges(ModPlayer clientPlayer) {
            epicmodPlayer oldClone = clientPlayer as epicmodPlayer;

            if(oldClone == null) {
                return;
            }

            for(int i = 1; i <= accessories; i++) 
            {
                if(oldClone.slots[i - 1].Item.IsNotTheSameAs(slots[i - 1].Item)) {
                    SendSingleItemPacket(PacketMessageType.EquipSlot, slots[i - 1].Item, -1, player.whoAmI);
                }
            }
        }

        internal void SendSingleItemPacket(PacketMessageType message, Item item, int toWho, int fromWho) {
            ModPacket packet = mod.GetPacket();
            packet.Write((byte)message);
            packet.Write((byte)player.whoAmI);
            ItemIO.Send(item, packet);
            packet.Send(toWho, fromWho);
        }

        public override void SyncPlayer(int toWho, int fromWho, bool newPlayer) {
            ModPacket packet = mod.GetPacket();
            packet.Write((byte)PacketMessageType.All);
            packet.Write((byte)player.whoAmI);

            for(int i = 1; i <= accessories; i++)
                ItemIO.Send(slots[i - 1].Item, packet);

            packet.Send(toWho, fromWho);
        }


        public override void Initialize() {

            slots = new UIItemSlot[accessories];
          for(int i = 1; i<=accessories; i++) {

                var slot = new UIItemSlot(Vector2.Zero, context: ItemSlot.Context.EquipAccessory, conditions: Slot_Conditions,
              drawBackground: Slot_DrawBackground, scaleToInventory: true, hoverText: String.Format("Epic Accessory [{0}]", i));
                slots[i-1] = slot;
             
            }
            InitEpAccessory();
        }


        public override void UpdateEquips(ref bool wallSpeedBuff, ref bool tileSpeedBuff, ref bool tileRangeBuff) {
            for(int i = 1; i <= accessories; i++) {
                var accessory = slots[i-1].Item;
                if(accessory.stack > 0) {
                    player.VanillaUpdateAccessory(player.whoAmI, accessory, false, ref wallSpeedBuff, ref tileSpeedBuff,
                        ref tileRangeBuff);
                    player.VanillaUpdateEquip(accessory);
                }
            } 
        }

        public override TagCompound Save() {
            
            TagCompound compound = new TagCompound();

            for(int i = 1; i <= accessories; i++) {
                string name =prefix + i.ToString(); // String.Format("{0}{1}", prefix, i);
                var slot = slots[i - 1];
                compound.Add(name, ItemIO.Save(slot.Item));
            }
            return compound;

        }

        public override void Load(TagCompound tag) {

            for(int i = 1; i <= accessories; i++) {
                string name =prefix + i.ToString(); // String.Format("{0}{1}", prefix, i);
                SetAccessory(ItemIO.Load(tag.GetCompound(name)), i);
            }

        }


        private void Slot_DrawBackground(UIObject sender, SpriteBatch spriteBatch) {
            UIItemSlot slot = (UIItemSlot)sender;

            if(ShouldDrawSlots()) {
                slot.OnDrawBackground(spriteBatch);

                if(slot.Item.stack == 0) {
                    Texture2D tex = mod.GetTexture(epicmod.WingSlotBackTex);
                    Vector2 origin = tex.Size() / 2f * Main.inventoryScale;
                    Vector2 position = slot.Rectangle.TopLeft();

                    spriteBatch.Draw(
                        tex,
                        position + (slot.Rectangle.Size() / 2f) - (origin / 2f),
                        null,
                        Color.White * 0.35f,
                        0f,
                        origin,
                        Main.inventoryScale,
                        SpriteEffects.None,
                        0f); // layer depth 0 = front
                }
            }
        }

        private static bool Slot_Conditions(Item item) {
            if(!item.accessory) {
                return false;
            }

            return true;
        }

        public void Draw(SpriteBatch spriteBatch) {

            if(!ShouldDrawSlots() ){
                return;
            }

            int mapH = 0;
            int rX;
            int rY;
            int magic = 47;
            float origScale = Main.inventoryScale;

            Main.inventoryScale = 0.85f;

            if(Main.mapEnabled) {
                if(!Main.mapFullscreen && Main.mapStyle == 1) {
                    mapH = 256;
                }
            }

            if(Main.mapEnabled) {
                int adjustY = 600;

                
                if(Main.player[Main.myPlayer].ExtraAccessorySlotsShouldShow) {
                    adjustY = 610 + PlayerInput.UsingGamepad.ToInt() * 30;
                }

                if((mapH + adjustY) > Main.screenHeight) {
                    mapH = Main.screenHeight - adjustY;
                }
            }

            int counter = 0;
            int total = 0;
            for(int y = 1; y <= 6; y++) {
                rX = Main.screenWidth - 92 - 14 - (magic * 3) - (int)(Main.extraTexture[58].Width * Main.inventoryScale);
                rY = (int)(mapH + 174 + 4 + counter * 56 * Main.inventoryScale);
                for(int x = 1; x <= 7; x++) {
                    slots[total].Position = new Vector2(rX, rY);
                    rX -= magic;
                    total++;
                }
                counter++;
            }
            
            for(int i = 1; i <= accessories; i++) {
                slots[i-1].Draw(spriteBatch);
            }

            Main.inventoryScale = origScale;


            for(int i = 1; i <= accessories; i++)
                slots[i - 1].Update();

        }

        private static bool ShouldDrawSlots() {
            if(Main.playerInventory) {
                if(Main.EquipPage == 0) {
                    return true;
                }
            }
            return false;
        }


        private void InitEpAccessory() {

            for(int i = 1; i <= accessories; i++) {
                slots[i - 1].Item = new Item();
                slots[i - 1].Item.SetDefaults(0, true);
            }
            for(int i = 1; i <= accessories; i++) {
                slots[i - 1].Item = new Item();
                slots[i - 1].Item.SetDefaults(0, true);
            }
        }

        private void SetAccessory(Item item, int slot)
            {
            slots[slot-1].Item = item.Clone();
        }

    }
}
