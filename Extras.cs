//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Reflection;
//using System.Threading.Tasks;
//using BepInEx.Bootstrap;
//using BepInEx.Logging;
//using UnityEngine;

//public class Extras
//{

//    static ManualLogSource Logger => MyMod.Logg;

//    public static string getPluginPath(string guid) {
//        var file = Chainloader.PluginInfos[guid].Location;
//        return Path.GetDirectoryName(file);
//    }

//    public static AssetBundle importAssetBundle(string assetFile, string modGuid) {
//        //Import Models
//        var dir = getPluginPath(modGuid);
//        var fileLoc = Path.Combine(dir, assetFile);
//        if(!File.Exists(fileLoc)) {
//            Logger.LogError("Cannot find Assetbundle called: " + fileLoc);
//            return null;
//        }

//        var MainAssetBundle = AssetBundle.LoadFromFile(fileLoc);
//        if(MainAssetBundle == null) {
//            Logger.LogError("Cannot find Assetbundle called: " + fileLoc);
//        }
//        return MainAssetBundle;

//    }

//    public static object callPrivateMethod(object objectToCallFrom, string methodName, object[] args) {
//        MethodInfo dynMethod = objectToCallFrom.GetType().GetMethod(methodName,
//        BindingFlags.NonPublic | BindingFlags.Instance);
//        return dynMethod.Invoke(objectToCallFrom, args);
//    }



//    public static void replaceBones(SkinnedMeshRenderer source, Dictionary<string, UnityEngine.Transform> transforms) {
//        var newBones = new List<Transform>();
//        Logger.LogInfo("Start replacements for: " + source.name);


//        for(int i = 0; i < source.bones.Length; i++) {
//            //Logger.LogInfo(i);
//            var bone = source.bones[i];
//            // Logger.LogInfo(bone?.name);
//            if(bone == null) continue;
//            if(!transforms.TryGetValue(bone.name, out var nBone)) {
//                //Logger.LogInfo("Create bone: " + bone.name);
//                Transform parentInLookup = null;
//                var boneToCopy = bone;
//                List<Transform> parentChain = new List<Transform>();
//                bool commonParent = true;
//                while(parentInLookup == null) {
//                    //Logger.LogInfo("look for parent of: " + boneToCopy.name);
//                    //Debug.Log(boneToCopy.name);
//                    if(!boneToCopy.parent) {
//                        //Logger.LogError("Cant find parent");
//                        //if this bone matters you messed up somewhere
//                        commonParent = false;
//                        transforms.Add(bone.name, transforms.First().Value);
//                        break;
//                    }
//                    transforms.TryGetValue(boneToCopy.parent.name, out parentInLookup);
//                    parentChain.Add(boneToCopy.parent);
//                    boneToCopy = boneToCopy.parent;
//                }
//                if(!commonParent) continue;
//                //go back down parent chain, instantiating all the parent bones
//                for(i = parentChain.Count - 1; i >= 0; i--) {
//                    var parent = transforms[parentChain[i].name];
//                    Transform boneToCreate = null;
//                    if(i > 0)
//                        boneToCreate = parentChain[i - 1];
//                    else
//                        boneToCreate = bone; //original bone
//                    createNewBone(boneToCreate, parent);
//                }
//                //newBones.Add(transforms[bone.name]);

//            } else {
//                //NOTE: having multiple custom bones not in skinnedmesh ruins the bone order, so bone list has to be calcualted at the end
//                //newBones.Add(nBone);
//            }
//        }
//        foreach(var bone in source.bones) {
//            newBones.Add(transforms[bone.name]);
//        }
//        //if(source.transform.parent)
//        //    foreach(var dyn in source.transform.parent.GetComponentsInChildren<DynamicBone>()) {
//        //        //move over all dynamic bones
//        //        transforms.TryGetValue(dyn?.m_Root?.name ?? "", out var newRoot);
//        //        Logger.LogMessage("Moved dynamic bone root"+ dyn?.m_Root?.name + " to target: " + newRoot?.name);
//        //        if(newRoot)
//        //            dyn.m_Root = transforms[dyn.m_Root.name];
//        //    }

//        source.rootBone = transforms[source.rootBone.name];
//        source.bones = newBones.ToArray();



//        void createNewBone(Transform boneToCopy, Transform parent) {
//            var nCreatedBone = new UnityEngine.GameObject(boneToCopy.name).transform;
//            nCreatedBone.parent = parent;
//            nCreatedBone.localPosition = boneToCopy.localPosition;
//            nCreatedBone.localScale = boneToCopy.localScale;
//            nCreatedBone.localRotation = boneToCopy.localRotation;

//            //if dynamic bone
//            var oldDyn = boneToCopy.GetComponent<DynamicBone>();
//            var newDyn = oldDyn?.CopyComponentTo(nCreatedBone.gameObject);
//            if(newDyn) {
//                transforms.TryGetValue(oldDyn.m_Root.name, out var newRoot);
//                if(newRoot)
//                    newDyn.m_Root = newRoot;
//            }

//            transforms.Add(nCreatedBone.name, nCreatedBone);
//        }
//    }

//    public static void replaceTransforms(GameObject source, Dictionary<string, Transform> transforms) {
//        var newBones = new List<Transform>();
//        var sourceBones = source.GetComponentsInChildren<Transform>();
//        for(int i = 0; i < sourceBones.Length; i++) {
//            //Logger.LogInfo(i);
//            var bone = sourceBones[i];
//            if(bone == null) continue;

//            if(!transforms.TryGetValue(bone.name, out var nBone)) {
//                if(bone.parent == null || bone.GetComponent<SkinnedMeshRenderer>() != null) continue;
//                //bone doesnt exist. Add it.
//                Logger.LogInfo("Adding Bone: " + bone.name);
//                var nCreatedBone = new GameObject(bone.name).transform;
//                var oldTfBoneName = bone.parent.name;
//                //if parent bone doesn't exist, we have a problem
//                var success = transforms.TryGetValue(oldTfBoneName, out var nParent);
//                if(!success) {
//                    Logger.LogError("Unable to find bone in old armature called " + bone.name);
//                    continue;
//                }
//                nCreatedBone.parent = nParent;
//                nCreatedBone.localPosition = bone.localPosition;
//                nCreatedBone.localScale = bone.localScale;
//                nCreatedBone.localRotation = bone.localRotation;
//                ComponentExtensions.CopyAllComponentsTo(bone.gameObject, nCreatedBone.gameObject);
//                transforms.Add(nCreatedBone.name, nCreatedBone);
//                newBones.Add(nCreatedBone);
//            } else {
//                //newBones.Add(nBone);
//                ComponentExtensions.CopyAllComponentsTo(bone.gameObject, nBone.gameObject);
//                //source.bones[i] = nBone;
//            }

//        }


//    }

//    public static void MountClothing(Transform original, Transform target, Dictionary<string, Transform> extraBones = null) {
//        Dictionary<string, Transform> transforms = extraBones ?? new Dictionary<string, Transform>();
//        Logger.LogInfo("Start mounting");
//        //index all transforms
//        foreach(var tf in target.GetComponentsInChildren<Transform>()) {
//            string name = tf.name;
//            if(transforms.ContainsKey(name)) {
//                Logger.LogWarning("Skinned Mesh has duplicate bone: " + name);
//            } else
//                transforms.Add(name, tf);
//        }
//        var meshes = original.GetComponentsInChildren<SkinnedMeshRenderer>();
//        foreach(SkinnedMeshRenderer skinnedMesh in original.GetComponentsInChildren<SkinnedMeshRenderer>()) {
//            replaceBones(skinnedMesh, transforms);
//        }
//    }

//    // Token: 0x0600061D RID: 1565 RVA: 0x0003BD78 File Offset: 0x00039F78
//    //public static void MountClothing(Transform original, Transform target) {
//    //    Transform[] componentsInChildren = target.GetComponentsInChildren<Transform>();
//    //    foreach(SkinnedMeshRenderer skinnedMeshRenderer in original.GetComponentsInChildren<SkinnedMeshRenderer>()) {
//    //        skinnedMeshRenderer.gameObject.SetActive(true);
//    //        Transform[] array = new Transform[skinnedMeshRenderer.bones.Length];
//    //        for(int j = 0; j < skinnedMeshRenderer.bones.Length; j++) {
//    //            Transform[] array2 = componentsInChildren;
//    //            int k = 0;
//    //            while(k < array2.Length) {
//    //                Transform transform = array2[k];
//    //                if(skinnedMeshRenderer.bones[j] && skinnedMeshRenderer.bones[j].name == transform.name) {
//    //                    array[j] = transform;
//    //                    if(transform.name == skinnedMeshRenderer.rootBone.name) {
//    //                        skinnedMeshRenderer.rootBone = transform;
//    //                        break;
//    //                    }
//    //                    break;
//    //                } else {
//    //                    k++;
//    //                }
//    //            }
//    //        }
//    //        skinnedMeshRenderer.bones = array;
//    //    }
//    //}

//    public static void replaceMeshes(GameObject source, GameObject target) {
//        SkinnedMeshRenderer[] meshes = source.GetComponentsInChildren<SkinnedMeshRenderer>();
//        SkinnedMeshRenderer[] targetMeshes = target.GetComponentsInChildren<SkinnedMeshRenderer>();
//        Transform rootBone = targetMeshes[0].rootBone;
//        Dictionary<string, Transform> transforms = new Dictionary<string, Transform>();
//        foreach(var tf in target.GetComponentsInChildren<Transform>()) {
//            string name = tf.name;
//            if(transforms.ContainsKey(name)) {
//                Logger.LogInfo("Already contains: " + name);
//            } else
//                transforms.Add(name, tf);
//        }
//        replaceTransforms(source, transforms);
//        //var transforms = .ToDictionary(t => tfName(t));
//        //move new renderers to character
//        foreach(var sourceMesh in meshes) {
//            var look = sourceMesh.name;
//            replaceBones(sourceMesh, transforms);
//            sourceMesh.transform.parent = target.transform;
//            sourceMesh.rootBone = rootBone;
//        }
//        //disable all old renderers
//        foreach(var targetMesh in targetMeshes) {
//            targetMesh.enabled = false;
//        }
//        //var dynamics = source.GetComponentsInChildren<DynamicBone>();
//        //foreach(var d in dynamics){
//        //    var nBone = transforms[tfName(d.transform)];
//        //    var root = transforms[tfName(d.m_Root)];
//        //    var db = nBone.gameObject.AddComponent<DynamicBone>();
//        //    db.m_Root = root;
//        //    db.m_Elasticity = d.m_Elasticity;
//        //    db.m_Stiffness = d.m_Stiffness;
//        //    db.m_Damping = d.m_Damping;
//        //    db.m_Force = d.m_Force;
//        //    db.m_Gravity = d.m_Gravity;
//        //    Logger.LogInfo("Dynamic bone: "+d.name + " Copied to: "+nBone.name);
//        //}

//    }



//}
//public static class ComponentExtensions
//{
//    public static void CopyAllComponentsTo(this GameObject original, GameObject destination) {
//        var parent = destination.transform.parent;
//        var name = destination.name;
//        foreach(var comp in original.GetComponents<Component>()) {
//            comp.CopyComponentTo(destination);
//        }
//        destination.transform.parent = parent;
//        destination.name = name;
//    }
//    public static T CopyComponentTo<T>(this T original, GameObject destination) where T : UnityEngine.Component {
//        System.Type type = original.GetType();
//        var dst = destination.GetComponent(type) as T;
//        if(!dst) dst = destination.AddComponent(type) as T;
//        var fields = type.GetFields();
//        foreach(var field in fields) {
//            if(field.IsStatic) continue;
//            field.SetValue(dst, field.GetValue(original));
//        }
//        var props = type.GetProperties();
//        foreach(var prop in props) {
//            if(!prop.CanWrite || !prop.CanWrite || prop.Name == "name") continue;
//            prop.SetValue(dst, prop.GetValue(original, null), null);
//        }
//        return dst as T;
//    }
//}


