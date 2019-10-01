using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using TUNING;

namespace StorageCompressor
{
    public class StorageCompressorConfig : IBuildingConfig
    {
        public const string ID = "ClockPunkPanic.SolidStorageCompressor";
        public const string DisplayName = "Inflaton Compressor";
        public const string Description = "This is the description text!";
        public const string Effect = "This is the effect text!";

        public static void Setup ()
        {
            AddBuilding.AddStrings(ID, DisplayName, Description, Effect);
            AddBuilding.AddBuildingToPlanScreen("Base", ID, StorageLockerConfig.ID);
        }

        public override BuildingDef CreateBuildingDef()
        {
            int width = 3;
            int height = 3;
            string anim = "storagelocker_kanim";
            int hitpoints = 30;
            float construction_time = 10f;
            float[] tier = BUILDINGS.CONSTRUCTION_MASS_KG.TIER4;
            string[] raw_MINERALS = MATERIALS.RAW_MINERALS;
            float melting_point = 1600f;
            BuildLocationRule build_location_rule = BuildLocationRule.OnFloor;
            EffectorValues none = NOISE_POLLUTION.NONE;
            BuildingDef buildingDef = BuildingTemplates.CreateBuildingDef(ID, width, height, anim, hitpoints, construction_time, tier, raw_MINERALS, melting_point, build_location_rule, BUILDINGS.DECOR.PENALTY.TIER1, none, 0.2f);
            buildingDef.Floodable = false;
            buildingDef.AudioCategory = "Metal";
            buildingDef.Overheatable = false;
            buildingDef.RequiresPowerInput = true;
            buildingDef.EnergyConsumptionWhenActive = 960;
            buildingDef.SelfHeatKilowattsWhenActive = 1000;
            buildingDef.PowerInputOffset = new CellOffset(-1, 1);
            return buildingDef;
        }

        public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
        {
            SoundEventVolumeCache.instance.AddVolume("storagelocker_kanim", "StorageLocker_Hit_metallic_low", NOISE_POLLUTION.NOISY.TIER1);
            Prioritizable.AddRef(go);
            Storage storage = go.AddOrGet<Storage>();
            storage.capacityKg = 20000 * (8) * (5) * (3) * (2);
            // 10, 6, 3, 2
            // 10, 5, 4, 2
            storage.showInUI = true;
            storage.allowItemRemoval = true;
            storage.showDescriptor = true;
            
            List<Tag> tags = new List<Tag>();
            tags.AddRange(STORAGEFILTERS.BAGABLE_CREATURES);
            tags.AddRange(STORAGEFILTERS.NOT_EDIBLE_SOLIDS);
            storage.storageFilters = tags;
            storage.storageFullMargin = STORAGE.STORAGE_LOCKER_FILLED_MARGIN;
            storage.fetchCategory = Storage.FetchCategory.GeneralStorage;
            CopyBuildingSettings copyBuildingSettings = go.AddOrGet<CopyBuildingSettings>();
            copyBuildingSettings.copyGroupTag = GameTags.StorageLocker;
            go.AddOrGet<StorageLocker>();
        }

        public override void DoPostConfigureComplete(GameObject go)
        {
            go.AddOrGetDef<StorageController.Def>();
        }
    }

    internal static class AddBuilding
    {
        internal static void AddBuildingToPlanScreen(HashedString category, string buildingId, string parentId)
        {
            Debug.Log("[DylanPiep][StorageCompressor] ::: AddPlan @ " + System.DateTime.Now);

            int index = GetCategoryIndex(category, buildingId);

            if (index == -1)
                return;

            int? indexBuilding = null;
            if (!parentId.IsNullOrWhiteSpace())
            {
                indexBuilding = (BUILDINGS.PLANORDER[index].data as IList<string>)?.IndexOf(parentId);
                if (indexBuilding != null)
                {
                    ++indexBuilding;
                }
            }

            if (indexBuilding == null)
            {
                Console.WriteLine("ERROR: building \"" + parentId + "\" not found in category " + category + ". Placing " + buildingId + " at the end of the list");
            }

            AddBuildingToPlanScreen(category, buildingId, indexBuilding);
        }

        internal static void AddBuildingToPlanScreen(HashedString category, string buildingId, int? index = null)
        {
            int CategoryIndex = GetCategoryIndex(category, buildingId);

            if (CategoryIndex == -1)
                return;
            if (index != null)
            {
                if (index >= 0 && index < (BUILDINGS.PLANORDER[CategoryIndex].data as IList<string>)?.Count)
                {
                    (BUILDINGS.PLANORDER[CategoryIndex].data as IList<string>)?.Insert(index.Value, buildingId);
                    return;
                }
            }

            (BUILDINGS.PLANORDER[CategoryIndex].data as IList<string>)?.Add(buildingId);
        }

        internal static void ReplaceBuildingInPlanScreen(HashedString category, string buildingId, string parentId)
        {
            int index = GetCategoryIndex(category, buildingId);

            if (index == -1)
                return;

            int? indexBuilding = null;
            indexBuilding = (BUILDINGS.PLANORDER[index].data as IList<string>)?.IndexOf(parentId);
            if (indexBuilding != null)
            {
                (BUILDINGS.PLANORDER[index].data as IList<string>)?.Remove(parentId);
                (BUILDINGS.PLANORDER[index].data as IList<string>)?.Insert(indexBuilding.Value, buildingId);
                return;
            }


            if (indexBuilding == null)
            {
                Console.WriteLine("ERROR: building \"" + parentId + "\" not found in category " + category + ". Placing " + buildingId + " at the end of the list");
            }

            AddBuildingToPlanScreen(category, buildingId, indexBuilding);
        }

        private static int GetCategoryIndex(HashedString category, string buildingId)
        {
            int index = BUILDINGS.PLANORDER.FindIndex(x => x.category == category);

            if (index == -1)
            {
                Console.WriteLine("ERROR: can't add building " + buildingId + " to non-existing category " + category);
            }

            return index;
        }


        // --------------------------------------

        internal static void IntoTechTree(string Tech, string BuildingID)
        {

            var TechGroup = new List<string>(Database.Techs.TECH_GROUPING[Tech]) { };
            TechGroup.Insert(1, BuildingID);
            Database.Techs.TECH_GROUPING[Tech] = TechGroup.ToArray();

            // TODO figure out how to control the order within a group

        }

        internal static void ReplaceInTechTree(string Tech, string BuildingID, string old)
        {

            var TechGroup = new List<string>(Database.Techs.TECH_GROUPING[Tech]) { };
            int index = TechGroup.FindIndex(x => x == old);
            if (index != -1)
            {
                TechGroup[index] = BuildingID;
                Database.Techs.TECH_GROUPING[Tech] = TechGroup.ToArray();
            }
            else
            {
                IntoTechTree(Tech, BuildingID);
            }
        }


        private static int GetTechCategoryIndex(HashedString category, string buildingId)
        {
            int index = BUILDINGS.PLANORDER.FindIndex(x => x.category == category);

            if (index == -1)
            {
                Console.WriteLine("ERROR: can't add building " + buildingId + " to non-existing category " + category);
            }

            return index;
        }

        internal static void AddStrings(string ID, string Name, string Description, string Effect)
        {
            // UI.FormatAsLink(Name, ID); would be the clean implementation of a link, but it has a nameclash with TURING
            Debug.Log("[DylanPiep][StorageCompressor] ::: AddStrings @ " + System.DateTime.Now);
            Strings.Add($"STRINGS.BUILDINGS.PREFABS.{ID.ToUpperInvariant()}.NAME", "<link=\"" + ID + "\">" + Name + "</link>");
            Strings.Add($"STRINGS.BUILDINGS.PREFABS.{ID.ToUpperInvariant()}.DESC", Description);
            Strings.Add($"STRINGS.BUILDINGS.PREFABS.{ID.ToUpperInvariant()}.EFFECT", Effect);
        }
    }
}
