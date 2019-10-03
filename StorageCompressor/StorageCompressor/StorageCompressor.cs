using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KSerialization;
using UnityEngine;

namespace StorageCompressor {
    class StorageCompressor : KMonoBehaviour, IUserControlledCapacity {
        private LoggerFS log;
        [Serialize]
        private float userMaxCapacity = float.PositiveInfinity;
        [Serialize]
        public string LockerName = string.Empty;
        protected FilteredStorage filteredStorage;
        private static readonly EventSystem.IntraObjectHandler<StorageCompressor> OnCopySettingsDelegate = (
            new EventSystem.IntraObjectHandler<StorageCompressor>(
                delegate (StorageCompressor component, object data) {
                    component.OnCopySettings(data);
                }
            )
        );

        public virtual float UserMaxCapacity {
            get { return Math.Min(userMaxCapacity, GetComponent<Storage>().capacityKg); }
            set { userMaxCapacity = value; filteredStorage.FilterChanged(); }
        }

        public float AmountStored => GetComponent<Storage>().MassStored();
        public float MinCapacity => 0f;
        public float MaxCapacity => GetComponent<Storage>().capacityKg;
        public bool WholeValues => false;
        public LocString CapacityUnits => GameUtil.GetCurrentMassUnit();

        protected override void OnPrefabInit() {
            Initialize(use_logic_meter: false);
        }

        protected void Initialize (bool use_logic_meter) {
            base.OnPrefabInit();
            log = new LoggerFS("StorageCompressor");
            filteredStorage = new FilteredStorage(this, null, null, this, use_logic_meter, Db.Get().ChoreTypes.StorageFetch);
            int eventHash = (int)GameHashes.CopySettings;
            Subscribe(eventHash, OnCopySettingsDelegate);
        }

        protected override void OnSpawn() {
            filteredStorage.FilterChanged();
            if (!LockerName.IsNullOrWhiteSpace()) {
                SetName(LockerName);
            }
        }

        protected override void OnCleanUp() {
            filteredStorage.CleanUp();
        }

        protected void OnCopySettings (object data) {
            GameObject gObj = data as GameObject;
            if (gObj != null) {
                StorageCompressor sComp = gObj.GetComponent<StorageCompressor>();
                if (sComp != null) {
                    UserMaxCapacity = sComp.UserMaxCapacity;
                }
            }
        }

        public void SetName(string name) {
            KSelectable component = GetComponent<KSelectable>();
            base.name = name;
            LockerName = name;
            if (component != null) {
                component.SetName(name);
            }
            gameObject.name = name;
            NameDisplayScreen.Instance.UpdateName(gameObject);
        }
    }
}
