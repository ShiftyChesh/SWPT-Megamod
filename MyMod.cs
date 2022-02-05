//using BepInEx;
//using BepInEx.Bootstrap;
//using BepInEx.Logging;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using HarmonyLib;
//using System.Reflection;
//using System.Linq;

//[BepInPlugin(MOD_GUID, MOD_NAME, VERSION)]
//public class MyMod : BaseUnityPlugin
//{
//    public const string MOD_GUID = "shifty.swpt.mymod";
//    public const string MOD_NAME = "mymod";
//    public const string VERSION = "1.0";

//    public const string ASSET_BUNDLE = "furniture";
//    public const string CLOTH_PATH = "Assets/Prefabs/clothing/";

//    public const string CUSTOM_TAG = "customLoad";

//    public static ManualLogSource Logg { get; private set; }

//    static List<Transform> Lingeries = new List<Transform>();

//    static AssetBundle Bundle;
//    static HashSet<string> assetNames = new HashSet<string>();
//    //static Dictionary<string, Transform> activeCustomItems = new Dictionary<string, Transform>();

//    // Start is called before the first frame update
//    void Awake() {
//        Logg = Logger;
//        Logger.LogInfo("Start logging for Meeee");
//        //loadFurniture();

//        Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
//    }



//    static void addFurniture(UnityEngine.Transform model) {
//        RM.code.allBuildings.AddItem(model);
//    }

//    public static void loadAssets() {
//        Bundle = Extras.importAssetBundle(ASSET_BUNDLE, MOD_GUID);

//        var assets = Bundle?.LoadAllAssets<GameObject>();

//        foreach(var asset in assets) {
//            Logg.LogInfo("Loading: " + asset.name);
//            assetNames.Add(asset.name);
//            if(tryLoadFurniture(asset)) continue;

//            if(tryLoadClothing(asset)) continue;
//        }

//    }
//    public static bool tryLoadFurniture(GameObject asset) {
//        var fur = asset.GetComponentInChildren<Furniture>();
//        if(fur != null) {
//            Logg.LogMessage("Adding furniture item:" + fur.name);
//            //var instance = Instantiate(fur);
//            addFurniture(fur.transform);
//            return true;
//        }
//        return false;
//    }
//    public static bool tryLoadClothing(GameObject asset) {
//        var item = asset.GetComponent<Item>();
//        if(item == null) return false;
//        var instance = Instantiate(asset.gameObject);
//        Destroy(instance.GetComponent<Appeal>());
//        item = instance.GetComponent<Item>();
//        instance.name = asset.name;
//        item.model = null;
//        //remove all other information besides item data
//        foreach(Transform child in instance.transform) {
//            Destroy(child.gameObject);
//        }
//        DontDestroyOnLoad(instance);

//        Logg.LogMessage("Adding equipable item: " + item.name + " - ID: " + item.transform.GetInstanceID());
//        if(item.itemType == ItemType.lingerie) {
//            RM.code.allLingeries.AddItem(item.transform);
//            Lingeries.Add(item.transform);
//        } else {
//            RM.code.allArmors.AddItem(item.transform);
//        }
//        RM.code.allItems.AddItem(item.transform);

//        Logg.LogWarning($"{asset.name} appeal status: {asset.GetComponent<Appeal>()}");
//        DontDestroyOnLoad(asset);
//        return true;
//    }
//    private static bool isSlave(CharacterCustomization character) {
//        return character.name == "Slave";
//    }

//    public static void attachCharacter(CharacterCustomization character, Furniture furniture) {
//        Logg.LogMessage("Attaching character: " + character?.name);
//        var hasUser = furniture.user;
//        if(hasUser) {
//            furniture.QuitInteractWithOnlyPoses(hasUser);
//            if(isSlave(hasUser))
//                Destroy(hasUser.gameObject);
//            return;
//        }
//        furniture.user = character;
//        character.interactingObject = furniture.transform;
//        if(furniture.posesGroup) {
//            furniture.posesGroup.gameObject.SetActive(true);
//        }
//        //furniture.DoInteract(character);

//        //call private method, like a boss
//        Extras.callPrivateMethod(furniture, "DoInteract", new object[] { character });

//        //MethodInfo dynMethod = furniture.GetType().GetMethod("DoInteract",
//        //BindingFlags.NonPublic | BindingFlags.Instance);
//        //dynMethod.Invoke(furniture, new object[] { character });
//    }

//    public static void customAddItem(CharacterCustomization custom, Transform item) {
//        if(!custom.misc6) {
//            //create holding object for customs
//            var gObj = new GameObject("Custom Items");
//            custom.misc6 = gObj.transform;
//            gObj.transform.SetParent(custom.characterBase);
//            gObj.AddComponent<Item>();
//        }
//        var holder = custom.misc6;
//        var alreadyEquipped = holder.transform.Find(item.name);
//        if(!alreadyEquipped) {
//            item.SetParent(holder.transform);
//            item.GetComponent<Item>().InstantiateModel(custom);
//            //activeCustomItems.Add(item.name, item);
//        }
//    }
//    public static Transform getCustomItemEquipped(CharacterCustomization custom, string name) {
//        if(custom.misc6) {
//            return custom.misc6.Find(name);
//        }
//        return null;
//    }
//    public static List<Transform> getAllCustomEquipped(CharacterCustomization custom) {
//        var children = new List<Transform>();
//        if(!custom.misc6) return children;
//        foreach(Transform tf in custom.misc6.transform) {
//            children.Add(tf);
//        }
//        return children;
//    }

//    public static void placeCharacterInFurniture() {
//        Logg.LogInfo("Pressed custom button interactioner");
//        var furniture = Player.code.focusedInteraction.GetComponent<Furniture>();
//        var characters = Object.FindObjectsOfType<CharacterCustomization>().Where(cc =>
//            cc.isActiveAndEnabled && cc._Companion != null && cc.name != "Player"
//        );
//        var user = furniture.user;
//        if(user) {
//            furniture.QuitInteract();
//            if(isSlave(user)) {
//                Destroy(user.gameObject);
//            } else {
//                Scene.code.SpawnCompanion(user.transform);
//            }
//            return;
//        }

//        generateNewCharacter((character) => {
//            attachCharacter(character, furniture);
//        });

//    }
//    public static void generateNewCharacter(System.Action<CharacterCustomization> callback) {
//        var companion = Global.code.companions.items[0];//RM.code.allCompanions.GetItemWithName("Kira")
//        if(!companion) {
//            Logg.LogError("Unable to find companion to copy for placing in furniture");
//            return;
//        }
//        Transform compCopy = Utility.Instantiate(companion);
//        var custom = companion.GetComponent<CharacterCustomization>();
//        custom.StartCoroutine(setCharacterData(compCopy, companion, callback));

//    }
//    private static IEnumerator setCharacterData(Transform character, Transform originalCharacter, System.Action<CharacterCustomization> callback) {
//        yield return new WaitForSeconds(0.1f);
//        var compCustom = originalCharacter.GetComponent<CharacterCustomization>();

//        character.name = "Slave";
//        //companion.tag = "Slave";
//        var custom = character.GetComponent<CharacterCustomization>();
//        randomize(custom);

//        for(int i = 0; i < compCustom.body.sharedMesh.blendShapeCount; i++) {
//            var value = compCustom.body.GetBlendShapeWeight(i);
//            custom.body.SetBlendShapeWeight(i, value);
//        }

//        custom.RefreshClothesVisibility();
//        callback(custom);
//        yield return null;
//    }

//    public static void randomize(CharacterCustomization custom) {
//        custom.ResetAppearence();
//        Logg.LogMessage("Randomize: " + custom.name);
//        custom.body.SetBlendShapeWeight(custom.body.sharedMesh.GetBlendShapeIndex("Genesis8Female__FHM-SASEBlake"), 0f);
//        custom.skin = RM.code.allSkins.items[Random.Range(0, RM.code.allSkins.items.Count)];
//        custom.hair = RM.code.allHairs.items[Random.Range(0, RM.code.allHairs.items.Count)];
//        custom.hairColor = RM.code.allHairColors.items[Random.Range(0, RM.code.allHairColors.items.Count)].GetComponent<CustomizationItem>().color;
//        custom.horn = null;//RM.code.allHorns.items[Random.Range(0, RM.code.allHorns.items.Count)];
//        custom.wing = null;//RM.code.allWings.items[Random.Range(0, RM.code.allWings.items.Count)];
//        custom.tail = null;//RM.code.allTails.items[Random.Range(0, RM.code.allTails.items.Count)];
//        custom.eyeTexture = RM.code.allEyes.items[Random.Range(0, RM.code.allEyes.items.Count)];
//        custom.lipstickColor = RM.code.allLipsticks.items[Random.Range(0, RM.code.allLipsticks.items.Count - 3)].GetComponent<CustomizationItem>().color;
//        custom.lipstickStrength = Random.Range(0f, 0.7f);
//        custom.eyebrows = RM.code.allEyebrows.items[Random.Range(0, RM.code.allEyebrows.items.Count)];
//        custom.eyeBrowsStrength = 1.2f;
//        custom.eyeShadowStrength = Random.Range(0f, 1.35f);
//        custom.eyeLinerStrength = Random.Range(0f, 1.35f);
//        //custom.skinColor = this.defaultSkinColor;
//        custom.nailColor = RM.code.allNails.items[Random.Range(0, RM.code.allNails.items.Count)];
//        custom.toenailColor = RM.code.allToeNails.items[Random.Range(0, RM.code.allToeNails.items.Count)];
//        custom.RefreshAppearence();

//    }

//    public static void customSave(Mainframe mf, CharacterCustomization customization, Transform item, string slotName) {
//        ES2.Save<string>(item.name, mf.GetFolderName() + customization.name + ".txt?tag=" + ("S" + slotName));
//    }
//    public static void customSaves(Mainframe mf, CharacterCustomization customization, List<Transform> items, string tag) {
//        if(items == null || items.Count == 0) {
//            Logg.LogMessage("No custom items to save");
//            return;
//        }
//        var saveString = "";
//        items.ForEach(item => {
//            if(!saveString.IsNullOrWhiteSpace())
//                saveString += "|";
//            saveString += item.name;
//        });

//        ES2.Save<string>(saveString, mf.GetFolderName() + customization.name + ".txt?tag=" + tag);
//    }
//    public static void customLoads(Mainframe mf, CharacterCustomization customization, string tag) {
//        if(ES2.Exists(mf.GetFolderName() + customization.name + ".txt?tag=" + tag)) {
//            var names = ES2.Load<string>(mf.GetFolderName() + customization.name + ".txt?tag=" + tag);
//            foreach(var name in names.Split('|')) {
//                Logg.LogMessage("Load: " + name);
//                var asset = RM.code.allItems.GetItemWithName(name);
//                if(asset) {
//                    Transform instance = Utility.Instantiate(asset);
//                    customAddItem(customization, instance);

//                }
//            }
//        }
//    }


//    public static Transform customLoad(Mainframe mf, CharacterCustomization customization, string slotName) {
//        if(ES2.Exists(mf.GetFolderName() + customization.name + ".txt?tag=" + ("S" + slotName))) {
//            string equipName = ES2.Load<string>(mf.GetFolderName() + customization.name + ".txt?tag=" + ("S" + slotName));

//            var itemBase = RM.code.allItems.GetItemWithName(equipName);
//            if(itemBase) {
//                Transform transform = Utility.Instantiate(itemBase);
//                customization.AddItem(transform, slotName);
//                transform.GetComponent<Item>().InstantiateModel(customization);
//                return transform;
//            } else {
//                Logg.LogError("Unable to find item with name: " + equipName);
//                return null;
//            }
//        }
//        return null;
//    }
//    [HarmonyPatch(typeof(CharacterCustomization), "FixedUpdate")]
//    private static class CharacterCustomization_FixedUpdate_Patch
//    {

//        private static void Postfix(CharacterCustomization __instance) {

//            var customItems = getAllCustomEquipped(__instance);
//            foreach(Transform item in customItems) {
//                var appeal = item?.GetComponent<Item>()?.GetAppeal();
//                if(!appeal) return;
//                var count = appeal?.allRenderers?.FirstOrDefault()?.sharedMesh?.blendShapeCount;
//                if(count != null && count >= 53)
//                    appeal.SyncBreathing();
//            }
         

//        }
//    }


//    [HarmonyPatch(typeof(RM), "LoadResources")]
//    private static class RM_LoadResources_Patch
//    {
//        private static void Postfix(RM __instance) {

//            Logg.LogInfo("tryin");

//            bool enabled = true;//!MyMod.modEnabled.Value;
//            if(enabled && !Bundle) {
//                loadAssets();
//            }
//        }
//    }

//    //LoadCharacterCustomization
//    [HarmonyPatch(typeof(Mainframe), "LoadCharacterCustomization")]
//    private static class Mainframe_LoadCharacterCustomization_Patch
//    {
//        private static void Postfix(Mainframe __instance, CharacterCustomization gen) {
//            Logg.LogInfo("Load custom items");

//            customLoads(__instance, gen, CUSTOM_TAG);

//            //var miscSlots = new List<Transform>() {
//            //    null,
//            //    gen.misc1,
//            //    gen.misc2,
//            //    gen.misc3,
//            //    gen.misc4,
//            //    gen.misc5,
//            //    gen.misc6,
//            //    gen.misc7,
//            //    gen.misc8,
//            //};
//            //for(int i = 3; i <= 8; i++) {
//            //    var slotName = $"misc{i}";
//            //    Logg.LogInfo("try load slot: " + slotName);
//            //    var loaded = customLoad(__instance, gen, slotName);
//            //    Logg.LogInfo("loaded: " + loaded?.name);

//            //}


//            gen.RefreshClothesVisibility();

//        }

//    }
//    [HarmonyPatch(typeof(Mainframe), "SaveCharacterCustomization")]
//    private static class Mainframe_SaveCharacterCustomization_Patch
//    {
//        private static void Postfix(Mainframe __instance, CharacterCustomization customization) {
//            Logg.LogInfo("save custom misc slots for: " + customization?.name);

//            var items = getAllCustomEquipped(customization);

//            customSaves(__instance, customization, items, CUSTOM_TAG);

//            //var miscSlots = new List<Transform>() {
//            //    null,
//            //    customization.misc1,
//            //    customization.misc2,
//            //    customization.misc3,
//            //    customization.misc4,
//            //    customization.misc5,
//            //    customization.misc6,
//            //    customization.misc7,
//            //    customization.misc8,
//            //};

//            //for(int i = 3; i <= 8; i++) {
//            //    var slotName = $"misc{i}";
//            //    var miscItem = miscSlots[i];
//            //    if(miscItem == null) continue;
//            //    int id = miscItem.GetInstanceID();

//            //    Logg.LogInfo("try save slot: " + slotName + " item id: " + id);
//            //    customSave(__instance, customization, miscItem, slotName);
//            //}

//            customization.RefreshClothesVisibility();

//        }
//    }


//    [HarmonyPatch(typeof(Mainframe), "LoadLingeries")]
//    private static class Mainframe_LoadLingeries_Patch
//    {
//        private static void Postfix(Mainframe __instance) {
//            Logg.LogInfo("Load custom lingeries");
//            foreach(var asset in Lingeries) {

//                Global.code.playerLingerieStorage.AddItemToCollection(asset, false, false);
//            }

//        }
//    }

//    [HarmonyPatch(typeof(Item), "InstantiateModel")]
//    private static class Item_InstantiateModel_Patch
//    {
//        private static bool Prefix(Item __instance, CharacterCustomization _character, ref Transform __result) {

//            //is a custom asset from this mod :)
//            if(!__instance.model && assetNames.Contains(__instance.name)) {
//                Logg.LogInfo("Trying to instantiate Custom Model: " + __instance.name);

//                var asset = Bundle.LoadAsset<GameObject>(__instance.name);
//                Logg.LogInfo("Asset: " + asset);

//                if(!asset)
//                    return true;

//                __instance.model = Utility.Instantiate(asset.transform);
//                Logg.LogInfo("Instantiated: " + asset?.name);

//                if(_character) {
//                    if(__instance.model.GetComponent<Appeal>()) {
//                        MountClothingPlus(__instance, _character);
//                        //__instance.model.GetComponent<Appeal>().MountClothing(_character);
//                    }
//                    if(__instance.model.GetComponent<Collider>()) {
//                        __instance.model.GetComponent<Collider>().enabled = false;
//                    }
//                    if(__instance.model.GetComponent<Rigidbody>()) {
//                        __instance.model.GetComponent<Rigidbody>().isKinematic = true;
//                    }
//                    __instance.model.SetParent(__instance.transform);
//                    __instance.model.localEulerAngles = Vector3.zero;
//                    __instance.model.localPosition = Vector3.zero;
//                    Logg.LogDebug("setup character");
//                }
//                __result = __instance.model;
//                return false;
//            }
//            return true;
//        }
//        private static void MountClothingPlus(Item item, CharacterCustomization custom) {
//            var appeal = item.model.GetComponent<Appeal>();
//            Logg.LogInfo("Mount: " + appeal);
//            appeal._CharacterCustomization = custom;
//            appeal.gameObject.SetActive(true);
//            var hip = custom.body.rootBone;
//            var extraBones = new Dictionary<string, Transform>() { [custom.transform.name] = custom.transform, [hip.parent.name] = hip.parent };
//            Extras.MountClothing(item.model.transform, hip, extraBones);
//            custom.RefreshClothesVisibility();
//            appeal.allRenderers = item.model.GetComponentsInChildren<SkinnedMeshRenderer>().ToList();
//            appeal.SyncBlendshape();
//        }
//    }
//    [HarmonyPatch(typeof(CharacterCustomization), "SyncBlendshape")]
//    private static class CharacterCustomization_SyncBlendshape_Patch
//    {
//        private static void Postfix(CharacterCustomization __instance) {
//            var customItems = getAllCustomEquipped(__instance);
//            foreach(Transform item in customItems) {
//                item?.GetComponent<Item>()?.GetAppeal()?.SyncBlendshape();
//            }
//        }
//    }

//        [HarmonyPatch(typeof(InventoryClosetIcon), "ButtonTryOn")]
//    private static class InventoryClosetIcon_ButtonTryOn_Patch
//    {
//        private static bool Prefix(InventoryClosetIcon __instance) {
//            //Logg.LogInfo("Start prefix");
//            CharacterCustomization curCustomization = Global.code.uiInventory.curCustomization;
//            var item = __instance.item;
//            Logg.LogInfo("get item: " + item.name);

//            if(assetNames.Contains(item.name)) {
//                var instance = getCustomItemEquipped(curCustomization, item.name);
//                if(instance) {
//                    Object.Destroy(instance.gameObject);
//                    //activeCustomItems.Remove(instance.name);

//                } else {
//                    var newInstance = Utility.Instantiate(item);
//                    customAddItem(curCustomization, newInstance);
//                }
//                RM.code.PlayOneShot(RM.code.sndPutonLingerie);
//                Global.code.uiInventory.ButtonUnderwearGroup();
//                Global.code.uiInventory.RefreshEquipment();
//                return false;
//            }


//            return true;
//        }
//    }

//    [HarmonyPatch(typeof(UICombat), "Start")]
//    public static class UICombat_Start_Patch
//    {


//        // Token: 0x06000009 RID: 9 RVA: 0x0000247C File Offset: 0x0000067C
//        public static void Postfix(UICombat __instance) {
//            GameObject panel = __instance.interactionOptionsPanel;
//            Transform childButton = panel.transform.GetChild(0);

//            var newButton = Instantiate(childButton, panel.transform);
//            newButton.SetSiblingIndex(1);
//            Logg.LogMessage("Add button to Interaction Menu");
//            var text = newButton.GetComponentInChildren<UnityEngine.UI.Text>();
//            var localization = newButton.GetComponentInChildren<LocalizationText>();
//            Destroy(localization);

//            text.text = "Place Character";
//            var button = newButton.GetComponent<UnityEngine.UI.Button>();
//            var buttonAction = button.onClick;

//            buttonAction.SetPersistentListenerState(0, UnityEngine.Events.UnityEventCallState.Off);

//            buttonAction.AddListener(() => {
//                placeCharacterInFurniture();

//            });



//        }
//    }




//}
