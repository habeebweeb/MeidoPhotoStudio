using MeidoPhotoStudio.Database.Background;
using MeidoPhotoStudio.Database.Props;
using UnityEngine;

namespace MeidoPhotoStudio.Plugin.Core.Props;

public class PropInstantiator
{
    private static GameObject myRoomDeploymentObject;

    public GameObject Instantiate(IPropModel propModel) =>
        propModel is null
            ? throw new System.ArgumentNullException(nameof(propModel))
            : propModel switch
            {
                BackgroundPropModel backgroundPropModel => Instantiate(backgroundPropModel),
                DeskPropModel deskPropModel => Instantiate(deskPropModel),
                MyRoomPropModel myRoomPropModel => Instantiate(myRoomPropModel),
                OtherPropModel otherPropModel => Instantiate(otherPropModel),
                PhotoBgPropModel photoBgPropModel => Instantiate(photoBgPropModel),
                _ => throw new System.NotImplementedException($"'{propModel.GetType()}' is not implemented"),
            };

    private static GameObject InstantiatePrefab(string prefabName)
    {
        var unityObject = Resources.Load<GameObject>($"Prefab/{prefabName}");

        return unityObject ? Object.Instantiate(unityObject) : null;
    }

    private static GameObject InstantiateAssetBundle(string assetName)
    {
        var gameObject = GameMain.Instance.BgMgr.CreateAssetBundle(assetName);

        return gameObject ? Object.Instantiate(gameObject) : null;
    }

    private GameObject Instantiate(BackgroundPropModel propModel)
    {
        var gameObject = propModel.Category is BackgroundCategory.MyRoomCustom
            ? InstantiateMyRoomBackground(propModel.AssetName)
            : InstantiateGameBackground(propModel.AssetName);

        return gameObject;

        static GameObject InstantiateMyRoomBackground(string assetName) =>
            MyRoomCustom.CreativeRoomManager.InstantiateRoom(assetName);

        static GameObject InstantiateGameBackground(string assetName)
        {
            var unityObject = GameMain.Instance.BgMgr.CreateAssetBundle(assetName);

            if (!unityObject)
                unityObject = Resources.Load<GameObject>($"BG/{assetName}");

            if (!unityObject)
                unityObject = Resources.Load<GameObject>($"BG/2_0/{assetName}");

            return unityObject ? Object.Instantiate(unityObject) : null;
        }
    }

    private GameObject Instantiate(DeskPropModel propModel)
    {
        GameObject gameObject = null;

        if (!string.IsNullOrEmpty(propModel.PrefabName))
            gameObject = InstantiatePrefab(propModel.PrefabName);
        else if (!string.IsNullOrEmpty(propModel.AssetName))
            gameObject = InstantiateAssetBundle(propModel.AssetName);

        return gameObject;
    }

    private GameObject Instantiate(MyRoomPropModel propModel)
    {
        var prefab = MyRoomCustom.PlacementData.GetData(propModel.ID).GetPrefab();

        if (!prefab)
            return null;

        var gameObject = Object.Instantiate(prefab);

        if (!gameObject)
            return null;

        var prop = WrapProp(gameObject);

        ParentToDeploymentObject(prop);

        return prop;

        static GameObject WrapProp(GameObject gameObject)
        {
            var container = new GameObject(gameObject.name);

            gameObject.transform.SetParent(container.transform, true);

            return container;
        }

        static void ParentToDeploymentObject(GameObject prop)
        {
            prop.transform.SetParent(GetDeploymentObject().transform, false);

            static GameObject GetDeploymentObject()
            {
                if (myRoomDeploymentObject)
                    return myRoomDeploymentObject;

                var foundParent = GameObject.Find("Deployment Object Parent");

                return myRoomDeploymentObject = foundParent ? foundParent : new GameObject("Deployment Object Parent");
            }
        }
    }

    private GameObject Instantiate(OtherPropModel propModel)
    {
        var gameObject = InstantiateAssetBundle(propModel.AssetName);

        if (!gameObject)
            gameObject = InstantiatePrefab(propModel.AssetName);

        return gameObject;
    }

    private GameObject Instantiate(PhotoBgPropModel propModel)
    {
        var assetName = string.IsNullOrEmpty(propModel.PrefabName)
            ? propModel.AssetName
            : propModel.PrefabName;

        if (string.IsNullOrEmpty(assetName))
            return null;

        // NOTE: Mod/MaidLoader allows for adding custom props so Prefab/AssetName cannot be trusted and the prop has to
        // be spawned by brute force.
        var gameObject = InstantiateAssetBundle(assetName);

        if (!gameObject)
            gameObject = InstantiatePrefab(assetName);

        return gameObject;
    }
}
