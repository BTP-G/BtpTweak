﻿using R2API.Networking.Interfaces;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace BTP.RoR2Plugin.Messages {

    internal struct ItemMessage(Inventory inventory, ItemIndex itemIndex, int itemCount) : INetMessage {
        private Inventory _inventory = inventory;
        private ItemIndex _itemIndex = itemIndex;
        private int _itemCount = itemCount;

        public void Deserialize(NetworkReader reader) {
            _inventory = reader.ReadGameObject().GetComponent<Inventory>();
            _itemIndex = reader.ReadItemIndex();
            _itemCount = reader.ReadInt32();
        }

        public readonly void OnReceived() {
            _inventory.GiveItem(_itemIndex, _itemCount);
        }

        public readonly void Serialize(NetworkWriter writer) {
            writer.Write(_inventory.gameObject);
            writer.Write(_itemIndex);
            writer.Write(_itemCount);
        }

        [ModLoadMessageHandler]
        private static void Register() {
            if (R2API.Networking.NetworkingAPI.RegisterMessageType<ItemMessage>()) {
                (typeof(ItemMessage).FullName + " Register Successd!").LogMessage();
            } else {
                (typeof(ItemMessage).FullName + "ItemMessage Register Failed!").LogError();
            }
        }
    }
}