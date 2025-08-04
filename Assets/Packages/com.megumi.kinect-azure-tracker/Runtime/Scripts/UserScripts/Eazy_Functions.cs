using System.Collections.Generic;
using UnityEngine;
using Microsoft.Azure.Kinect.BodyTracking;

public class Easy_Functions : MonoBehaviour
{
    [Header("References")]
    public TrackerHandler_multi trackerHandler;
    public DepthImageDisplay depthImageDisplay;

    [Header("Control Keys")]
    [Tooltip("切換骨骼顯示的按鍵")]
    public KeyCode toggleSkeletonKey = KeyCode.S;

    [Tooltip("切換深度影像顯示的按鍵")]
    public KeyCode toggleDepthImageKey = KeyCode.D;

    [Tooltip("列印偵測資訊的按鍵")]
    public KeyCode printInfoKey = KeyCode.I;

    [Tooltip("切換影像翻轉的按鍵")]
    public KeyCode toggleImageFlipKey = KeyCode.F;

    [Tooltip("調整影像縮放的按鍵（配合數字鍵1-3）")]
    public KeyCode scaleImageKey = KeyCode.LeftShift;

    [Header("Display Settings")]
    [Tooltip("是否顯示即時資訊在畫面上")]
    public bool showRealTimeInfo = true;

    [Tooltip("資訊顯示的位置")]
    public Vector2 infoDisplayPosition = new Vector2(10, 10);

    // 私有變數
    private bool lastSkeletonState = true;
    private bool lastDepthImageState = true;
    private float[] scaleValues = { 0.5f, 1.0f, 1.5f }; // 預設縮放值
    private int currentScaleIndex = 1; // 預設使用1.0倍縮放

    void Start()
    {
        InitializeReferences();
        PrintWelcomeMessage();
    }

    void Update()
    {
        HandleInputs();
    }

    /// <summary>
    /// 初始化組件參考
    /// </summary>
    private void InitializeReferences()
    {
        // 檢查必要組件
        if (trackerHandler == null)
        {
            Debug.LogError("Easy_Functions: TrackerHandler_multi 未找到！請在 Inspector 中指定。");
        }

        if (depthImageDisplay == null)
        {
            Debug.LogWarning("Easy_Functions: DepthImageDisplay 未找到！深度影像功能將無法使用。");
        }
    }

    /// <summary>
    /// 處理所有按鍵輸入
    /// </summary>
    private void HandleInputs()
    {
        // 切換骨骼顯示
        if (Input.GetKeyDown(toggleSkeletonKey))
        {
            ToggleSkeleton();
        }

        // 切換深度影像顯示
        if (Input.GetKeyDown(toggleDepthImageKey))
        {
            ToggleDepthImage();
        }

        // 列印當前資訊
        if (Input.GetKeyDown(printInfoKey))
        {
            PrintCurrentInfo();
        }

        // 切換影像翻轉
        if (Input.GetKeyDown(toggleImageFlipKey))
        {
            ToggleImageFlip();
        }

        // 調整影像縮放（Shift + 1/2/3）
        if (Input.GetKey(scaleImageKey))
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
                SetImageScale(0);
            else if (Input.GetKeyDown(KeyCode.Alpha2))
                SetImageScale(1);
            else if (Input.GetKeyDown(KeyCode.Alpha3))
                SetImageScale(2);
        }

        // ESC 鍵重置所有設定
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ResetAllSettings();
        }
    }

    /// <summary>
    /// 顯示歡迎訊息和操作說明
    /// </summary>
    private void PrintWelcomeMessage()
    {
        Debug.Log("=== Easy_Functions 已啟動 ===");
        Debug.Log("操作說明：");
        Debug.Log($"[{toggleSkeletonKey}] - 切換骨骼顯示");
        Debug.Log($"[{toggleDepthImageKey}] - 切換深度影像");
        Debug.Log($"[{printInfoKey}] - 列印偵測資訊");
        Debug.Log($"[{toggleImageFlipKey}] - 切換影像翻轉");
        Debug.Log($"[{scaleImageKey} + 1/2/3] - 調整影像縮放 (0.5x/1.0x/1.5x)");
        Debug.Log("[ESC] - 重置所有設定");
        Debug.Log("========================");
    }

    /// <summary>
    /// 在畫面上顯示即時資訊
    /// </summary>
    void OnGUI()
    {
        if (!showRealTimeInfo) return;

        GUIStyle style = new GUIStyle();
        style.fontSize = 14;
        style.normal.textColor = Color.white;
        style.fontStyle = FontStyle.Bold;

        // 建立資訊文字
        string infoText = BuildRealTimeInfoText();

        // 計算文字區域大小
        Vector2 textSize = style.CalcSize(new GUIContent(infoText));
        Rect backgroundRect = new Rect(infoDisplayPosition.x - 5, infoDisplayPosition.y - 5,
                                     textSize.x + 10, textSize.y + 10);

        // 繪製半透明背景
        GUI.color = new Color(0, 0, 0, 0.7f);
        GUI.DrawTexture(backgroundRect, Texture2D.whiteTexture);
        GUI.color = Color.white;

        // 顯示文字
        GUI.Label(new Rect(infoDisplayPosition.x, infoDisplayPosition.y, textSize.x, textSize.y),
                 infoText, style);
    }

    /// <summary>
    /// 建立即時資訊文字
    /// </summary>
    private string BuildRealTimeInfoText()
    {
        string info = "=== Kinect Azure 狀態 ===\n";

        // 偵測人數
        int bodyCount = GetDetectedBodyCount();
        info += $"偵測人數: {bodyCount}\n";

        // 骨骼顯示狀態
        bool skeletonVisible = trackerHandler?.drawSkeletons ?? false;
        info += $"骨骼顯示: {(skeletonVisible ? "開啟" : "關閉")}\n";

        // 深度影像狀態
        bool depthVisible = depthImageDisplay?.enableDisplay ?? false;
        info += $"深度影像: {(depthVisible ? "顯示" : "隱藏")}\n";

        // 深度影像資訊
        if (depthImageDisplay != null && depthVisible)
        {
            string depthInfo = depthImageDisplay.GetDepthImageInfo();
            info += $"{depthInfo}\n";
        }

        // 顯示頭部位置（如果有人）
        if (bodyCount > 0)
        {
            Vector3 headPos = GetHeadPosition();
            if (headPos != Vector3.zero)
            {
                info += $"頭部位置: ({headPos.x:F2}, {headPos.y:F2}, {headPos.z:F2})\n";
                info += $"距離: {headPos.magnitude:F2}m\n";
            }
        }

        info += "\n=== 操作說明 ===\n";
        info += $"[{toggleSkeletonKey}] 骨骼  [{toggleDepthImageKey}] 深度  [{printInfoKey}] 資訊\n";
        info += $"[{toggleImageFlipKey}] 翻轉  [Shift+1/2/3] 縮放";

        return info;
    }

    #region 公開介面函數 - 偵測功能

    /// <summary>
    /// 獲取當前偵測到的人數
    /// </summary>
    public int GetDetectedBodyCount()
    {
        if (trackerHandler == null) return 0;
        return trackerHandler.GetActiveBodyCount();
    }

    /// <summary>
    /// 獲取指定人物的頭部座標位置（本地座標）
    /// </summary>
    public Vector3 GetHeadPosition(uint bodyId = 0)
    {
        if (trackerHandler == null) return Vector3.zero;
        return trackerHandler.GetJointPosition(JointId.Head, bodyId);
    }

    /// <summary>
    /// 獲取指定人物的頭部世界座標位置
    /// </summary>
    public Vector3 GetHeadWorldPosition(uint bodyId = 0)
    {
        if (trackerHandler == null) return Vector3.zero;
        return trackerHandler.GetJointWorldPosition(JointId.Head, bodyId);
    }

    /// <summary>
    /// 獲取所有偵測到的人物頭部位置
    /// </summary>
    public List<Vector3> GetAllHeadPositions()
    {
        List<Vector3> headPositions = new List<Vector3>();

        if (trackerHandler == null) return headPositions;

        List<uint> bodyIds = trackerHandler.GetActiveBodyIds();

        foreach (uint bodyId in bodyIds)
        {
            Vector3 headPos = GetHeadPosition(bodyId);
            if (headPos != Vector3.zero)
            {
                headPositions.Add(headPos);
            }
        }

        return headPositions;
    }

    /// <summary>
    /// 獲取指定身體的關節位置
    /// </summary>
    public Vector3 GetJointPosition(JointId jointId, uint bodyId = 0)
    {
        if (trackerHandler == null) return Vector3.zero;
        return trackerHandler.GetJointPosition(jointId, bodyId);
    }

    /// <summary>
    /// 獲取所有偵測到的身體ID
    /// </summary>
    public List<uint> GetAllBodyIds()
    {
        if (trackerHandler == null) return new List<uint>();
        return trackerHandler.GetActiveBodyIds();
    }

    /// <summary>
    /// 獲取最接近攝影機的人物ID
    /// </summary>
    public uint GetClosestBodyId()
    {
        if (trackerHandler == null) return 0;

        List<uint> bodyIds = GetAllBodyIds();
        if (bodyIds.Count == 0) return 0;

        uint closestId = bodyIds[0];
        float closestDistance = float.MaxValue;

        foreach (uint bodyId in bodyIds)
        {
            Vector3 headPos = GetHeadPosition(bodyId);
            float distance = headPos.magnitude;

            if (distance > 0 && distance < closestDistance)
            {
                closestDistance = distance;
                closestId = bodyId;
            }
        }

        return closestId;
    }

    #endregion

    #region 公開介面函數 - 控制功能

    /// <summary>
    /// 切換骨骼顯示
    /// </summary>
    public void ToggleSkeleton()
    {
        if (trackerHandler != null)
        {
            trackerHandler.turnOnOffSkeletons();
            bool isVisible = trackerHandler.drawSkeletons;
            Debug.Log($"骨骼顯示已{(isVisible ? "開啟" : "關閉")}");
        }
        else
        {
            Debug.LogWarning("TrackerHandler_multi 未找到，無法切換骨骼顯示");
        }
    }

    /// <summary>
    /// 設定骨骼顯示狀態
    /// </summary>
    public void SetSkeletonVisibility(bool visible)
    {
        if (trackerHandler != null && trackerHandler.drawSkeletons != visible)
        {
            trackerHandler.turnOnOffSkeletons();
        }
    }

    /// <summary>
    /// 切換深度影像顯示
    /// </summary>
    public void ToggleDepthImage()
    {
        if (depthImageDisplay != null)
        {
            depthImageDisplay.ToggleDepthDisplay();
            bool isVisible = depthImageDisplay.enableDisplay;
            Debug.Log($"深度影像已{(isVisible ? "顯示" : "隱藏")}");
        }
        else
        {
            Debug.LogWarning("DepthImageDisplay 未找到，無法切換深度影像顯示");
        }
    }

    /// <summary>
    /// 切換影像翻轉
    /// </summary>
    public void ToggleImageFlip()
    {
        if (depthImageDisplay != null)
        {
            // 在水平和垂直翻轉之間循環
            bool currentH = depthImageDisplay.flipHorizontal;
            bool currentV = depthImageDisplay.flipVertical;

            if (!currentH && !currentV)
            {
                depthImageDisplay.SetImageFlip(true, false); // 水平翻轉
                Debug.Log("影像已設定為水平翻轉");
            }
            else if (currentH && !currentV)
            {
                depthImageDisplay.SetImageFlip(false, true); // 垂直翻轉
                Debug.Log("影像已設定為垂直翻轉");
            }
            else if (!currentH && currentV)
            {
                depthImageDisplay.SetImageFlip(true, true); // 兩個都翻轉
                Debug.Log("影像已設定為水平+垂直翻轉");
            }
            else
            {
                depthImageDisplay.SetImageFlip(false, false); // 取消翻轉
                Debug.Log("影像翻轉已取消");
            }
        }
        else
        {
            Debug.LogWarning("DepthImageDisplay 未找到，無法切換影像翻轉");
        }
    }

    /// <summary>
    /// 設定影像縮放
    /// </summary>
    public void SetImageScale(int scaleIndex)
    {
        if (depthImageDisplay != null && scaleIndex >= 0 && scaleIndex < scaleValues.Length)
        {
            currentScaleIndex = scaleIndex;
            float scale = scaleValues[scaleIndex];
            depthImageDisplay.SetImageScale(scale);
            Debug.Log($"影像縮放已設定為 {scale}x");
        }
    }

    /// <summary>
    /// 重置所有設定
    /// </summary>
    public void ResetAllSettings()
    {
        Debug.Log("重置所有設定...");

        // 重置骨骼顯示為開啟
        if (trackerHandler != null && !trackerHandler.drawSkeletons)
        {
            trackerHandler.turnOnOffSkeletons();
        }

        // 重置深度影像為顯示
        if (depthImageDisplay != null && !depthImageDisplay.enableDisplay)
        {
            depthImageDisplay.ToggleDepthDisplay();
        }

        // 重置影像設定
        if (depthImageDisplay != null)
        {
            depthImageDisplay.SetImageFlip(false, false);
            depthImageDisplay.SetImageScale(1.0f);
            currentScaleIndex = 1;
        }

        Debug.Log("所有設定已重置為預設值");
    }

    #endregion

    #region 公開介面函數 - 資訊功能

    /// <summary>
    /// 列印當前偵測資訊
    /// </summary>
    public void PrintCurrentInfo()
    {
        Debug.Log("=== 當前偵測資訊 ===");

        if (trackerHandler == null)
        {
            Debug.Log("TrackerHandler_multi 未找到！");
            return;
        }

        int bodyCount = GetDetectedBodyCount();
        Debug.Log($"偵測人數: {bodyCount}");

        if (bodyCount > 0)
        {
            List<uint> bodyIds = GetAllBodyIds();

            for (int i = 0; i < bodyIds.Count; i++)
            {
                uint bodyId = bodyIds[i];
                PrintBodyInfo(bodyId, i + 1);
            }
        }
        else
        {
            Debug.Log("目前沒有偵測到任何人物");
        }

        // 顯示系統狀態
        PrintSystemStatus();

        Debug.Log("==================");
    }

    /// <summary>
    /// 列印指定身體的詳細資訊
    /// </summary>
    private void PrintBodyInfo(uint bodyId, int index)
    {
        Debug.Log($"--- 人物 {index} (ID: {bodyId}) ---");

        // 頭部位置
        Vector3 headPos = GetHeadPosition(bodyId);
        if (headPos != Vector3.zero)
        {
            Debug.Log($"  頭部位置: ({headPos.x:F2}, {headPos.y:F2}, {headPos.z:F2})");
            Debug.Log($"  距離攝影機: {headPos.magnitude:F2}m");
        }

        // 左右手位置
        Vector3 leftHand = GetJointPosition(JointId.HandLeft, bodyId);
        Vector3 rightHand = GetJointPosition(JointId.HandRight, bodyId);

        if (leftHand != Vector3.zero)
            Debug.Log($"  左手位置: ({leftHand.x:F2}, {leftHand.y:F2}, {leftHand.z:F2})");

        if (rightHand != Vector3.zero)
            Debug.Log($"  右手位置: ({rightHand.x:F2}, {rightHand.y:F2}, {rightHand.z:F2})");

        // 估算身高
        float height = EstimateBodyHeight(bodyId);
        if (height > 0)
            Debug.Log($"  估算身高: {height:F2}m");

        // 檢查是否舉手
        bool isRaisingHands = IsRaisingBothHands(bodyId);
        Debug.Log($"  是否舉起雙手: {(isRaisingHands ? "是" : "否")}");
    }

    /// <summary>
    /// 列印系統狀態
    /// </summary>
    private void PrintSystemStatus()
    {
        Debug.Log("--- 系統狀態 ---");

        bool skeletonVisible = trackerHandler?.drawSkeletons ?? false;
        Debug.Log($"骨骼顯示: {(skeletonVisible ? "開啟" : "關閉")}");

        if (depthImageDisplay != null)
        {
            bool depthVisible = depthImageDisplay.enableDisplay;
            Debug.Log($"深度影像: {(depthVisible ? "顯示" : "隱藏")}");

            if (depthVisible)
            {
                string depthInfo = depthImageDisplay.GetDepthImageInfo();
                Debug.Log($"深度影像資訊: {depthInfo}");
                Debug.Log($"影像縮放: {scaleValues[currentScaleIndex]}x");
                Debug.Log($"水平翻轉: {depthImageDisplay.flipHorizontal}");
                Debug.Log($"垂直翻轉: {depthImageDisplay.flipVertical}");
            }
        }
        else
        {
            Debug.Log("深度影像組件: 未找到");
        }
    }

    /// <summary>
    /// 獲取完整的系統狀態字串
    /// </summary>
    public string GetSystemStatusString()
    {
        string status = "=== Kinect Azure 系統狀態 ===\n";
        status += $"偵測人數: {GetDetectedBodyCount()}\n";
        status += $"骨骼顯示: {(trackerHandler?.drawSkeletons ?? false ? "開啟" : "關閉")}\n";

        if (depthImageDisplay != null)
        {
            status += $"深度影像: {(depthImageDisplay.enableDisplay ? "顯示" : "隱藏")}\n";
            if (depthImageDisplay.enableDisplay)
            {
                status += $"深度影像資訊: {depthImageDisplay.GetDepthImageInfo()}\n";
            }
        }

        status += "========================";
        return status;
    }

    #endregion

    #region 進階分析功能

    /// <summary>
    /// 計算指定身體的身高（估算）
    /// </summary>
    public float EstimateBodyHeight(uint bodyId = 0)
    {
        if (trackerHandler == null) return 0f;

        Vector3 headPos = GetJointPosition(JointId.Head, bodyId);
        Vector3 footLeftPos = GetJointPosition(JointId.FootLeft, bodyId);
        Vector3 footRightPos = GetJointPosition(JointId.FootRight, bodyId);

        if (headPos == Vector3.zero) return 0f;

        // 使用較低的腳作為基準
        float footY = Mathf.Min(footLeftPos.y, footRightPos.y);
        return Mathf.Abs(headPos.y - footY);
    }

    /// <summary>
    /// 檢查指定身體是否舉起雙手
    /// </summary>
    public bool IsRaisingBothHands(uint bodyId = 0)
    {
        Vector3 leftHand = GetJointPosition(JointId.HandLeft, bodyId);
        Vector3 rightHand = GetJointPosition(JointId.HandRight, bodyId);
        Vector3 head = GetJointPosition(JointId.Head, bodyId);

        if (leftHand == Vector3.zero || rightHand == Vector3.zero || head == Vector3.zero)
            return false;

        return leftHand.y > head.y && rightHand.y > head.y;
    }

    /// <summary>
    /// 檢查指定身體是否在做招手動作
    /// </summary>
    public bool IsWaving(uint bodyId = 0)
    {
        Vector3 rightHand = GetJointPosition(JointId.HandRight, bodyId);
        Vector3 rightShoulder = GetJointPosition(JointId.ShoulderRight, bodyId);

        if (rightHand == Vector3.zero || rightShoulder == Vector3.zero)
            return false;

        // 檢查右手是否在肩膀高度以上
        return rightHand.y > rightShoulder.y;
    }

    #endregion
}
