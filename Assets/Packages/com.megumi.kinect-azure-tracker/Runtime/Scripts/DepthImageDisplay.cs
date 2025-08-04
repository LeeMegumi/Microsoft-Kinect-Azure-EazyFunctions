using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 深度影像顯示組件
/// 用於在Unity UI上顯示來自SkeletalTrackingProvider的深度影像資料
/// </summary>
public class DepthImageDisplay : MonoBehaviour
{
    [Header("UI 組件")]
    [Tooltip("用於顯示深度影像的RawImage組件")]
    public RawImage depthImageUI;

    [Header("顯示設定")]
    [Tooltip("是否啟用深度影像顯示")]
    public bool enableDisplay = true;

    [Tooltip("深度影像的更新間隔（幀數），用於控制更新頻率")]
    [Range(5, 10)]
    public int updateInterval = 5;

    [Header("影像處理")]
    [Tooltip("是否翻轉影像（水平翻轉）")]
    public bool flipHorizontal = false;

    [Tooltip("是否翻轉影像（垂直翻轉）")]
    public bool flipVertical = false;

    [Tooltip("影像縮放比例")]
    [Range(0.1f, 2.0f)]
    public float imageScale = 1.0f;

    // 內部變數
    private Texture2D depthTexture;
    private BackgroundData frameData = new BackgroundData();
    private SkeletalTrackingProvider trackingProvider;
    private int frameCounter = 0;

    // 影像尺寸快取
    private int lastImageWidth = 0;
    private int lastImageHeight = 0;

    void Start()
    {
        InitializeDepthDisplay();
    }

    /// <summary>
    /// 初始化深度影像顯示
    /// </summary>
    void InitializeDepthDisplay()
    {
        // 檢查UI組件是否已指定
        if (depthImageUI == null)
        {
            Debug.LogError("DepthImageDisplay: depthImageUI is not assigned!");
            return;
        }

        // 尋找SkeletalTrackingProvider（假設在同一個GameObject或父物件上）
        trackingProvider = main.Instance.m_skeletalTrackingProvider;

        if (trackingProvider == null)
        {
            Debug.LogError("DepthImageDisplay: Could not find SkeletalTrackingProvider!");
            return;
        }

        Debug.Log("DepthImageDisplay initialized successfully.");
    }

    void Update()
    {
        // 檢查是否啟用顯示
        if (!enableDisplay || trackingProvider == null || depthImageUI == null)
            return;

        // 控制更新頻率
        frameCounter++;
        if (frameCounter % updateInterval != 0)
            return;

        // 嘗試獲取最新的幀資料
        bool hasNewData = trackingProvider.GetCurrentFrameData(ref frameData);

        if (hasNewData && frameData.DepthImageSize > 0)
        {
            UpdateDepthTexture();
        }       
    }

    /// <summary>
    /// 更新深度紋理並顯示在UI上
    /// </summary>
    void UpdateDepthTexture()
    {
        try
        {
            int imageWidth = frameData.DepthImageWidth;
            int imageHeight = frameData.DepthImageHeight;

            // 檢查影像尺寸是否有效
            if (imageWidth <= 0 || imageHeight <= 0)
            {
                Debug.LogWarning("DepthImageDisplay: Invalid image dimensions.");
                return;
            }

            // 如果影像尺寸改變，重新創建紋理
            if (depthTexture == null || lastImageWidth != imageWidth || lastImageHeight != imageHeight)
            {
                CreateNewTexture(imageWidth, imageHeight);
            }

            // 更新紋理資料
            UpdateTextureData(imageWidth, imageHeight);

            // 應用變更到紋理
            depthTexture.Apply();

            // 設定到UI組件
            depthImageUI.texture = depthTexture;

            // 應用縮放
            ApplyImageTransform();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"DepthImageDisplay: Error updating depth texture: {e.Message}");
        }
    }

    /// <summary>
    /// 創建新的紋理
    /// </summary>
    void CreateNewTexture(int width, int height)
    {
        // 釋放舊的紋理
        if (depthTexture != null)
        {
            DestroyImmediate(depthTexture);
        }

        // 創建新紋理（RGB24格式）
        depthTexture = new Texture2D(width, height, TextureFormat.RGB24, false);
        depthTexture.filterMode = FilterMode.Point; // 使用點過濾以保持銳利
        depthTexture.wrapMode = TextureWrapMode.Clamp;

        // 更新尺寸快取
        lastImageWidth = width;
        lastImageHeight = height;

        Debug.Log($"DepthImageDisplay: Created new texture {width}x{height}");
    }

    /// <summary>
    /// 更新紋理像素資料
    /// </summary>
    void UpdateTextureData(int width, int height)
    {
        // 確保資料大小正確
        int expectedSize = width * height * 3; // RGB格式，每個像素3個位元組
        if (frameData.DepthImageSize != expectedSize)
        {
            Debug.LogWarning($"DepthImageDisplay: Data size mismatch. Expected: {expectedSize}, Got: {frameData.DepthImageSize}");
            return;
        }

        // 創建顏色陣列
        Color32[] pixels = new Color32[width * height];

        // 轉換深度資料為顏色資料
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // 計算在深度資料中的索引
                int depthIndex = GetDepthDataIndex(x, y, width, height);

                // 計算在像素陣列中的索引
                int pixelIndex = GetPixelIndex(x, y, width, height);

                // 確保索引在有效範圍內
                if (depthIndex >= 0 && depthIndex + 2 < frameData.DepthImage.Length &&
                    pixelIndex >= 0 && pixelIndex < pixels.Length)
                {
                    byte r = frameData.DepthImage[depthIndex];
                    byte g = frameData.DepthImage[depthIndex + 1];
                    byte b = frameData.DepthImage[depthIndex + 2];

                    pixels[pixelIndex] = new Color32(r, g, b, 255);
                }
            }
        }

        // 設定像素到紋理
        depthTexture.SetPixels32(pixels);
    }

    /// <summary>
    /// 獲取深度資料索引（考慮翻轉）
    /// </summary>
    int GetDepthDataIndex(int x, int y, int width, int height)
    {
        // 應用翻轉
        if (flipHorizontal)
            x = width - 1 - x;
        if (flipVertical)
            y = height - 1 - y;

        return ((y * width) + x) * 3; // RGB格式，每個像素3個位元組
    }

    /// <summary>
    /// 獲取像素陣列索引
    /// </summary>
    int GetPixelIndex(int x, int y, int width, int height)
    {
        // Unity的紋理座標系統：(0,0)在左下角
        return ((height - 1 - y) * width) + x;
    }

    /// <summary>
    /// 應用影像變換（縮放、翻轉等）
    /// </summary>
    void ApplyImageTransform()
    {
        if (depthImageUI != null)
        {
            // 應用縮放
            Vector3 scale = Vector3.one * imageScale;
            depthImageUI.transform.localScale = scale;
        }
    }

    /// <summary>
    /// 切換深度影像顯示的開關
    /// </summary>
    public void ToggleDepthDisplay()
    {
        enableDisplay = !enableDisplay;

        if (!enableDisplay && depthImageUI != null)
        {
            depthImageUI.texture = null;
            depthImageUI.color = new Color32(255,255,255,0);
        }
        else
        {
            depthImageUI.color = Color.white;
        }

            Debug.Log($"DepthImageDisplay: Display {(enableDisplay ? "enabled" : "disabled")}");
    }

    /// <summary>
    /// 設定影像翻轉
    /// </summary>
    public void SetImageFlip(bool horizontal, bool vertical)
    {
        flipHorizontal = horizontal;
        flipVertical = vertical;
    }

    /// <summary>
    /// 設定影像縮放
    /// </summary>
    public void SetImageScale(float scale)
    {
        imageScale = Mathf.Clamp(scale, 0.1f, 2.0f);
    }

    /// <summary>
    /// 獲取當前深度影像資訊
    /// </summary>
    public string GetDepthImageInfo()
    {
        if (frameData.DepthImageWidth > 0 && frameData.DepthImageHeight > 0)
        {
            return $"深度影像: {frameData.DepthImageWidth}x{frameData.DepthImageHeight}, " +
                   $"資料大小: {frameData.DepthImageSize} bytes, " +
                   $"時間戳: {frameData.TimestampInMs:F1}ms";
        }
        return "無深度影像資料";
    }

    void OnDestroy()
    {
        // 清理紋理資源
        if (depthTexture != null)
        {
            DestroyImmediate(depthTexture);
            depthTexture = null;
        }
    }

    void OnDisable()
    {
        // 當組件被禁用時清空顯示
        if (depthImageUI != null)
        {
            depthImageUI.texture = null;
        }
    }
}