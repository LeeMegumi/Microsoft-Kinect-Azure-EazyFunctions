using System.Collections.Generic;
using UnityEngine;
using Microsoft.Azure.Kinect.BodyTracking;

public class TrackerHandler_multi : MonoBehaviour
{
    [Header("Prefab Settings")]
    public GameObject pointBodyPrefab; // 在Inspector中指定pointBody預製體

    [Header("Tracking Settings")]
    public float maxTrackingDistance = 5.0f; // 最大追蹤距離
    public bool drawSkeletons = true;

    // 原有的映射字典
    public Dictionary<JointId, JointId> parentJointMap;
    Dictionary<JointId, Quaternion> basisJointMap;

    // 管理多個身體的字典 - 使用身體ID作為key
    private Dictionary<uint, GameObject> activeBodyObjects = new Dictionary<uint, GameObject>();
    private Dictionary<uint, Quaternion[]> bodyJointRotations = new Dictionary<uint, Quaternion[]>();

    // 新增：儲存關節位置資訊的字典
    private Dictionary<uint, Vector3[]> bodyJointPositions = new Dictionary<uint, Vector3[]>();

    Quaternion Y_180_FLIP = new Quaternion(0.0f, 1.0f, 0.0f, 0.0f);

    void Awake()
    {
        InitializeJointMappings();
    }

    void InitializeJointMappings()
    {
        parentJointMap = new Dictionary<JointId, JointId>();

        // 設定父子關係映射
        parentJointMap[JointId.Pelvis] = JointId.Count;
        parentJointMap[JointId.SpineNavel] = JointId.Pelvis;
        parentJointMap[JointId.SpineChest] = JointId.SpineNavel;
        parentJointMap[JointId.Neck] = JointId.SpineChest;
        parentJointMap[JointId.ClavicleLeft] = JointId.SpineChest;
        parentJointMap[JointId.ShoulderLeft] = JointId.ClavicleLeft;
        parentJointMap[JointId.ElbowLeft] = JointId.ShoulderLeft;
        parentJointMap[JointId.WristLeft] = JointId.ElbowLeft;
        parentJointMap[JointId.HandLeft] = JointId.WristLeft;
        parentJointMap[JointId.HandTipLeft] = JointId.HandLeft;
        parentJointMap[JointId.ThumbLeft] = JointId.HandLeft;
        parentJointMap[JointId.ClavicleRight] = JointId.SpineChest;
        parentJointMap[JointId.ShoulderRight] = JointId.ClavicleRight;
        parentJointMap[JointId.ElbowRight] = JointId.ShoulderRight;
        parentJointMap[JointId.WristRight] = JointId.ElbowRight;
        parentJointMap[JointId.HandRight] = JointId.WristRight;
        parentJointMap[JointId.HandTipRight] = JointId.HandRight;
        parentJointMap[JointId.ThumbRight] = JointId.HandRight;
        parentJointMap[JointId.HipLeft] = JointId.SpineNavel;
        parentJointMap[JointId.KneeLeft] = JointId.HipLeft;
        parentJointMap[JointId.AnkleLeft] = JointId.KneeLeft;
        parentJointMap[JointId.FootLeft] = JointId.AnkleLeft;
        parentJointMap[JointId.HipRight] = JointId.SpineNavel;
        parentJointMap[JointId.KneeRight] = JointId.HipRight;
        parentJointMap[JointId.AnkleRight] = JointId.KneeRight;
        parentJointMap[JointId.FootRight] = JointId.AnkleRight;
        parentJointMap[JointId.Head] = JointId.Pelvis;
        parentJointMap[JointId.Nose] = JointId.Head;
        parentJointMap[JointId.EyeLeft] = JointId.Head;
        parentJointMap[JointId.EarLeft] = JointId.Head;
        parentJointMap[JointId.EyeRight] = JointId.Head;
        parentJointMap[JointId.EarRight] = JointId.Head;

        // 設定基準旋轉
        Vector3 zpositive = Vector3.forward;
        Vector3 xpositive = Vector3.right;
        Vector3 ypositive = Vector3.up;

        Quaternion leftHipBasis = Quaternion.LookRotation(xpositive, -zpositive);
        Quaternion spineHipBasis = Quaternion.LookRotation(xpositive, -zpositive);
        Quaternion rightHipBasis = Quaternion.LookRotation(xpositive, zpositive);
        Quaternion leftArmBasis = Quaternion.LookRotation(ypositive, -zpositive);
        Quaternion rightArmBasis = Quaternion.LookRotation(-ypositive, zpositive);
        Quaternion leftHandBasis = Quaternion.LookRotation(-zpositive, -ypositive);
        Quaternion rightHandBasis = Quaternion.identity;
        Quaternion leftFootBasis = Quaternion.LookRotation(xpositive, ypositive);
        Quaternion rightFootBasis = Quaternion.LookRotation(xpositive, -ypositive);

        basisJointMap = new Dictionary<JointId, Quaternion>();

        basisJointMap[JointId.Pelvis] = spineHipBasis;
        basisJointMap[JointId.SpineNavel] = spineHipBasis;
        basisJointMap[JointId.SpineChest] = spineHipBasis;
        basisJointMap[JointId.Neck] = spineHipBasis;
        basisJointMap[JointId.ClavicleLeft] = leftArmBasis;
        basisJointMap[JointId.ShoulderLeft] = leftArmBasis;
        basisJointMap[JointId.ElbowLeft] = leftArmBasis;
        basisJointMap[JointId.WristLeft] = leftHandBasis;
        basisJointMap[JointId.HandLeft] = leftHandBasis;
        basisJointMap[JointId.HandTipLeft] = leftHandBasis;
        basisJointMap[JointId.ThumbLeft] = leftArmBasis;
        basisJointMap[JointId.ClavicleRight] = rightArmBasis;
        basisJointMap[JointId.ShoulderRight] = rightArmBasis;
        basisJointMap[JointId.ElbowRight] = rightArmBasis;
        basisJointMap[JointId.WristRight] = rightHandBasis;
        basisJointMap[JointId.HandRight] = rightHandBasis;
        basisJointMap[JointId.HandTipRight] = rightHandBasis;
        basisJointMap[JointId.ThumbRight] = rightArmBasis;
        basisJointMap[JointId.HipLeft] = leftHipBasis;
        basisJointMap[JointId.KneeLeft] = leftHipBasis;
        basisJointMap[JointId.AnkleLeft] = leftHipBasis;
        basisJointMap[JointId.FootLeft] = leftFootBasis;
        basisJointMap[JointId.HipRight] = rightHipBasis;
        basisJointMap[JointId.KneeRight] = rightHipBasis;
        basisJointMap[JointId.AnkleRight] = rightHipBasis;
        basisJointMap[JointId.FootRight] = rightFootBasis;
        basisJointMap[JointId.Head] = spineHipBasis;
        basisJointMap[JointId.Nose] = spineHipBasis;
        basisJointMap[JointId.EyeLeft] = spineHipBasis;
        basisJointMap[JointId.EarLeft] = spineHipBasis;
        basisJointMap[JointId.EyeRight] = spineHipBasis;
        basisJointMap[JointId.EarRight] = spineHipBasis;
    }

    public void updateTracker(BackgroundData trackerFrameData)
    {
        // 獲取當前檢測到的所有身體ID
        HashSet<uint> currentBodyIds = new HashSet<uint>();
        HashSet<uint> validBodyIds = new HashSet<uint>(); // 有效範圍內的身體ID

        // 處理所有檢測到的身體
        for (int i = 0; i < (int)trackerFrameData.NumOfBodies; i++)
        {
            Body skeleton = trackerFrameData.Bodies[i];
            uint bodyId = skeleton.Id;

            // 將所有檢測到的身體ID加入currentBodyIds
            currentBodyIds.Add(bodyId);

            // 檢查身體是否在追蹤範圍內
            if (IsBodyInTrackingRange(skeleton))
            {
                validBodyIds.Add(bodyId);

                // 如果是新的身體，創建對應的pointBody
                if (!activeBodyObjects.ContainsKey(bodyId))
                {
                    CreateBodyObject(bodyId);
                }

                // 更新身體骨架
                UpdateBodySkeleton(skeleton, bodyId);
            }
        }
        Debug.Log(GetActiveBodyCount());
        // 移除超出範圍或不再被檢測到的身體
        RemoveInactiveBodies(validBodyIds);
    }

    private bool IsBodyInTrackingRange(Body skeleton)
    {
        // 檢查pelvis joint是否有效
        var pelvisPosition = skeleton.JointPositions3D[(int)JointId.Pelvis];

        // 檢查joint position是否有效（有些情況下可能會回傳無效值）
        if (float.IsNaN(pelvisPosition.X) || float.IsNaN(pelvisPosition.Y) || float.IsNaN(pelvisPosition.Z) ||
            float.IsInfinity(pelvisPosition.X) || float.IsInfinity(pelvisPosition.Y) || float.IsInfinity(pelvisPosition.Z))
        {
            return false;
        }

        Vector3 pelvisPos = new Vector3((float)pelvisPosition.X, (float)pelvisPosition.Y, (float)pelvisPosition.Z);

        // 檢查是否為零向量（可能表示無效位置）
        if (pelvisPos.magnitude < 0.01f)
        {
            return false;
        }

        return pelvisPos.magnitude <= maxTrackingDistance;
    }

    private void CreateBodyObject(uint bodyId)
    {
        if (pointBodyPrefab == null)
        {
            Debug.LogError("pointBodyPrefab is not assigned!");
            return;
        }

        // 實例化pointBody預製體
        GameObject newBodyObject = Instantiate(pointBodyPrefab, transform);
        newBodyObject.name = $"Body_{bodyId}";

        // 加入管理字典
        activeBodyObjects[bodyId] = newBodyObject;
        bodyJointRotations[bodyId] = new Quaternion[(int)JointId.Count];
        bodyJointPositions[bodyId] = new Vector3[(int)JointId.Count]; // 新增：初始化位置陣列

        Debug.Log($"Created body object for ID: {bodyId}");
    }

    private void UpdateBodySkeleton(Body skeleton, uint bodyId)
    {
        if (!activeBodyObjects.ContainsKey(bodyId))
            return;

        GameObject bodyObject = activeBodyObjects[bodyId];
        Quaternion[] jointRotations = bodyJointRotations[bodyId];
        Vector3[] jointPositions = bodyJointPositions[bodyId]; // 新增：獲取位置陣列

        // 更新每個關節
        for (int jointNum = 0; jointNum < (int)JointId.Count; jointNum++)
        {
            // 計算關節位置和旋轉
            Vector3 jointPos = new Vector3(
                skeleton.JointPositions3D[jointNum].X,
                -skeleton.JointPositions3D[jointNum].Y,
                skeleton.JointPositions3D[jointNum].Z
            );

            Quaternion jointRot = Y_180_FLIP * new Quaternion(
                skeleton.JointRotations[jointNum].X,
                skeleton.JointRotations[jointNum].Y,
                skeleton.JointRotations[jointNum].Z,
                skeleton.JointRotations[jointNum].W
            ) * Quaternion.Inverse(basisJointMap[(JointId)jointNum]);

            // 儲存旋轉和位置資訊
            jointRotations[jointNum] = jointRot;
            jointPositions[jointNum] = jointPos; // 新增：儲存位置資訊

            // 更新關節Transform
            Transform jointTransform = bodyObject.transform.GetChild(jointNum);
            jointTransform.localPosition = jointPos;
            jointTransform.localRotation = jointRot;

            // 更新骨骼連線
            UpdateBoneConnection(skeleton, bodyObject, jointNum);
        }
    }

    private void UpdateBoneConnection(Body skeleton, GameObject bodyObject, int jointNum)
    {
        const int boneChildNum = 0;
        Transform jointTransform = bodyObject.transform.GetChild(jointNum);

        if (parentJointMap[(JointId)jointNum] != JointId.Head && parentJointMap[(JointId)jointNum] != JointId.Count)
        {
            Vector3 jointPos = new Vector3(
                skeleton.JointPositions3D[jointNum].X,
                -skeleton.JointPositions3D[jointNum].Y,
                skeleton.JointPositions3D[jointNum].Z
            );

            Vector3 parentTrackerSpacePosition = new Vector3(
                skeleton.JointPositions3D[(int)parentJointMap[(JointId)jointNum]].X,
                -skeleton.JointPositions3D[(int)parentJointMap[(JointId)jointNum]].Y,
                skeleton.JointPositions3D[(int)parentJointMap[(JointId)jointNum]].Z
            );

            Vector3 boneDirectionTrackerSpace = jointPos - parentTrackerSpacePosition;
            Vector3 boneDirectionWorldSpace = transform.rotation * boneDirectionTrackerSpace;
            Vector3 boneDirectionLocalSpace = Quaternion.Inverse(jointTransform.rotation) * Vector3.Normalize(boneDirectionWorldSpace);

            Transform boneTransform = jointTransform.GetChild(boneChildNum);
            boneTransform.localScale = new Vector3(1, 20.0f * 0.5f * boneDirectionWorldSpace.magnitude, 1);
            boneTransform.localRotation = Quaternion.FromToRotation(Vector3.up, boneDirectionLocalSpace);
            boneTransform.position = jointTransform.position - 0.5f * boneDirectionWorldSpace;

            if (!boneTransform.gameObject.activeInHierarchy)
                boneTransform.gameObject.SetActive(true);
        }
        else
        {
            jointTransform.GetChild(boneChildNum).gameObject.SetActive(false);
        }
    }

    private void RemoveInactiveBodies(HashSet<uint> validBodyIds)
    {
        List<uint> bodiesToRemove = new List<uint>();

        // 找出需要移除的身體（不在有效範圍內的身體）
        foreach (uint bodyId in activeBodyObjects.Keys)
        {
            if (!validBodyIds.Contains(bodyId))
            {
                bodiesToRemove.Add(bodyId);
            }
        }

        // 移除無效的身體
        foreach (uint bodyId in bodiesToRemove)
        {
            if (activeBodyObjects.ContainsKey(bodyId))
            {
                GameObject bodyObjectToDestroy = activeBodyObjects[bodyId];
                activeBodyObjects.Remove(bodyId);
                bodyJointRotations.Remove(bodyId);
                bodyJointPositions.Remove(bodyId); // 新增：同時移除位置資訊

                // 使用Destroy而不是DestroyImmediate，避免在Update中出現問題
                if (bodyObjectToDestroy != null)
                {
                    Destroy(bodyObjectToDestroy);
                }

                Debug.Log($"Removed body object for ID: {bodyId}");
            }
        }
    }

    public void turnOnOffSkeletons()
    {
        drawSkeletons = !drawSkeletons;

        // 對所有活躍的身體對象切換可見性
        foreach (GameObject bodyObject in activeBodyObjects.Values)
        {
            for (int jointNum = 0; jointNum < (int)JointId.Count; jointNum++)
            {
                Transform jointTransform = bodyObject.transform.GetChild(jointNum);
                MeshRenderer jointRenderer = jointTransform.gameObject.GetComponent<MeshRenderer>();
                MeshRenderer boneRenderer = jointTransform.GetChild(0).GetComponent<MeshRenderer>();

                if (jointRenderer != null)
                    jointRenderer.enabled = drawSkeletons;
                if (boneRenderer != null)
                    boneRenderer.enabled = drawSkeletons;
            }
        }
    }

    /// <summary>
    /// 獲取指定身體和關節的相對旋轉
    /// </summary>
    /// <param name="jointId">關節ID</param>
    /// <param name="bodyId">身體ID，預設為0（自動選擇第一個身體）</param>
    /// <returns>關節的相對旋轉</returns>
    public Quaternion GetRelativeJointRotation(JointId jointId, uint bodyId = 0)
    {
        // 如果沒有指定bodyId，使用第一個找到的身體
        if (bodyId == 0 && activeBodyObjects.Count > 0)
        {
            bodyId = new List<uint>(activeBodyObjects.Keys)[0];
        }

        if (!bodyJointRotations.ContainsKey(bodyId))
            return Quaternion.identity;

        Quaternion[] jointRotations = bodyJointRotations[bodyId];
        JointId parent = parentJointMap[jointId];
        Quaternion parentJointRotationBodySpace = Quaternion.identity;

        if (parent == JointId.Count)
        {
            parentJointRotationBodySpace = Y_180_FLIP;
        }
        else
        {
            parentJointRotationBodySpace = jointRotations[(int)parent];
        }

        Quaternion jointRotationBodySpace = jointRotations[(int)jointId];
        Quaternion relativeRotation = Quaternion.Inverse(parentJointRotationBodySpace) * jointRotationBodySpace;

        return relativeRotation;
    }

    /// <summary>
    /// 獲取指定身體和關節的本地位置（相對於追蹤器）
    /// </summary>
    /// <param name="jointId">關節ID</param>
    /// <param name="bodyId">身體ID，預設為0（自動選擇第一個身體）</param>
    /// <returns>關節的本地位置，如果找不到則返回Vector3.zero</returns>
    public Vector3 GetJointPosition(JointId jointId, uint bodyId = 0)
    {
        // 如果沒有指定bodyId，使用第一個找到的身體
        if (bodyId == 0 && activeBodyObjects.Count > 0)
        {
            bodyId = new List<uint>(activeBodyObjects.Keys)[0];
        }

        // 檢查身體是否存在
        if (!bodyJointPositions.ContainsKey(bodyId))
        {
            Debug.LogWarning($"Body ID {bodyId} not found in joint positions!");
            return Vector3.zero;
        }

        // 檢查關節ID是否有效
        int jointIndex = (int)jointId;
        if (jointIndex < 0 || jointIndex >= bodyJointPositions[bodyId].Length)
        {
            Debug.LogWarning($"Invalid joint ID: {jointId}");
            return Vector3.zero;
        }

        return bodyJointPositions[bodyId][jointIndex];
    }

    /// <summary>
    /// 獲取指定身體和關節的世界位置
    /// </summary>
    /// <param name="jointId">關節ID</param>
    /// <param name="bodyId">身體ID，預設為0（自動選擇第一個身體）</param>
    /// <returns>關節的世界位置，如果找不到則返回Vector3.zero</returns>
    public Vector3 GetJointWorldPosition(JointId jointId, uint bodyId = 0)
    {
        Vector3 localPosition = GetJointPosition(jointId, bodyId);

        if (localPosition == Vector3.zero)
            return Vector3.zero;

        // 轉換為世界座標
        Vector3 worldPosition = transform.TransformPoint(localPosition);
        return worldPosition;
    }

    /// <summary>
    /// 獲取指定身體的所有關節位置
    /// </summary>
    /// <param name="bodyId">身體ID，預設為0（自動選擇第一個身體）</param>
    /// <returns>包含所有關節位置的陣列，如果找不到則返回null</returns>
    public Vector3[] GetAllJointPositions(uint bodyId = 0)
    {
        // 如果沒有指定bodyId，使用第一個找到的身體
        if (bodyId == 0 && activeBodyObjects.Count > 0)
        {
            bodyId = new List<uint>(activeBodyObjects.Keys)[0];
        }

        if (!bodyJointPositions.ContainsKey(bodyId))
        {
            Debug.LogWarning($"Body ID {bodyId} not found!");
            return null;
        }

        // 回傳陣列的副本，避免外部修改
        Vector3[] allPositions = new Vector3[bodyJointPositions[bodyId].Length];
        System.Array.Copy(bodyJointPositions[bodyId], allPositions, bodyJointPositions[bodyId].Length);

        return allPositions;
    }

    /// <summary>
    /// 計算兩個關節之間的距離
    /// </summary>
    /// <param name="jointId1">第一個關節ID</param>
    /// <param name="jointId2">第二個關節ID</param>
    /// <param name="bodyId">身體ID，預設為0（自動選擇第一個身體）</param>
    /// <returns>兩關節之間的距離，如果無法計算則返回-1</returns>
    public float GetJointDistance(JointId jointId1, JointId jointId2, uint bodyId = 0)
    {
        Vector3 pos1 = GetJointPosition(jointId1, bodyId);
        Vector3 pos2 = GetJointPosition(jointId2, bodyId);

        if (pos1 == Vector3.zero || pos2 == Vector3.zero)
        {
            Debug.LogWarning($"Cannot calculate distance between {jointId1} and {jointId2} for body {bodyId}");
            return -1f;
        }

        return Vector3.Distance(pos1, pos2);
    }

    /// <summary>
    /// 檢查指定關節的位置是否有效
    /// </summary>
    /// <param name="jointId">關節ID</param>
    /// <param name="bodyId">身體ID，預設為0（自動選擇第一個身體）</param>
    /// <returns>位置是否有效</returns>
    public bool IsJointPositionValid(JointId jointId, uint bodyId = 0)
    {
        Vector3 position = GetJointPosition(jointId, bodyId);

        // 檢查是否為零向量或包含無效值
        return position != Vector3.zero &&
               !float.IsNaN(position.x) && !float.IsNaN(position.y) && !float.IsNaN(position.z) &&
               !float.IsInfinity(position.x) && !float.IsInfinity(position.y) && !float.IsInfinity(position.z);
    }

    // 獲取當前追蹤的身體數量
    public int GetActiveBodyCount()
    {
        return activeBodyObjects.Count;
    }

    // 獲取所有活躍身體的ID列表
    public List<uint> GetActiveBodyIds()
    {
        return new List<uint>(activeBodyObjects.Keys);
    }

    // 根據ID獲取身體對象
    public GameObject GetBodyObject(uint bodyId)
    {
        return activeBodyObjects.ContainsKey(bodyId) ? activeBodyObjects[bodyId] : null;
    }

    // 清理所有身體對象（在需要重置時使用）
    public void ClearAllBodies()
    {
        List<uint> allBodyIds = new List<uint>(activeBodyObjects.Keys);

        foreach (uint bodyId in allBodyIds)
        {
            GameObject bodyObjectToDestroy = activeBodyObjects[bodyId];
            activeBodyObjects.Remove(bodyId);
            bodyJointRotations.Remove(bodyId);
            bodyJointPositions.Remove(bodyId); // 新增：清理位置資訊

            if (bodyObjectToDestroy != null)
            {
                Destroy(bodyObjectToDestroy);
            }
        }

        Debug.Log("Cleared all body objects");
    }

    // 手動檢查並清理無效的身體對象（可以在需要時調用）
    public void ForceCleanupInvalidBodies()
    {
        List<uint> bodiesToRemove = new List<uint>();

        foreach (var kvp in activeBodyObjects)
        {
            if (kvp.Value == null)
            {
                bodiesToRemove.Add(kvp.Key);
            }
        }

        foreach (uint bodyId in bodiesToRemove)
        {
            activeBodyObjects.Remove(bodyId);
            bodyJointRotations.Remove(bodyId);
            bodyJointPositions.Remove(bodyId); // 新增：清理位置資訊
            Debug.Log($"Force removed invalid body object for ID: {bodyId}");
        }
    }

    void OnDestroy()
    {
        ClearAllBodies();
    }
}