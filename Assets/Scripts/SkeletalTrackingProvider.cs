using Microsoft.Azure.Kinect.BodyTracking;
using Microsoft.Azure.Kinect.Sensor;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine;

/// <summary>
/// Kinect Azure 骨骼追蹤資料提供者
/// 繼承自 BackgroundDataProvider，專門負責在背景執行緒中從 Kinect 設備獲取身體追蹤資料
/// </summary>
public class SkeletalTrackingProvider : BackgroundDataProvider
{
    // 標記是否已讀取第一幀資料，用於初始化時間戳
    bool readFirstFrame = false;

    // 記錄初始時間戳，用於計算相對時間
    TimeSpan initialTimestamp;

    /// <summary>
    /// 建構函式 - 初始化骨骼追蹤提供者
    /// </summary>
    /// <param name="id">Kinect 設備的 ID</param>
    public SkeletalTrackingProvider(int id) : base(id)
    {
        Debug.Log("in the skeleton provider constructor");
    }

    /// <summary>
    /// 二進位序列化格式器，用於資料記錄功能
    /// </summary>
    System.Runtime.Serialization.Formatters.Binary.BinaryFormatter binaryFormatter { get; set; } = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();

    /// <summary>
    /// 原始資料記錄檔案串流，如果不為 null 則會記錄追蹤資料
    /// </summary>
    public Stream RawDataLoggingFile = null;

    /// <summary>
    /// 背景執行緒的主要執行方法
    /// 負責初始化 Kinect 設備、啟動攝像頭、創建追蹤器並持續處理身體追蹤資料
    /// </summary>
    /// <param name="id">Kinect 設備 ID</param>
    /// <param name="token">用於取消操作的令牌</param>
    protected override void RunBackgroundThreadAsync(int id, CancellationToken token)
    {
        try
        {
            UnityEngine.Debug.Log("Starting body tracker background thread.");

            // 分配緩衝區用於存儲當前幀資料
            BackgroundData currentFrameData = new BackgroundData();

            // 開啟 Kinect 設備
            using (Device device = Device.Open(id))
            {
                // 啟動攝像頭並設定配置參數
                device.StartCameras(new DeviceConfiguration()
                {
                    CameraFPS = FPS.FPS15,                    // 設定幀率為 30 FPS
                    ColorResolution = ColorResolution.Off,   // 關閉彩色攝像頭以提升效能
                    DepthMode = DepthMode.WFOV_Unbinned,     // 使用窄視角深度模式，提供更高精度
                    WiredSyncMode = WiredSyncMode.Standalone, // 獨立模式，不與其他設備同步
                });

                UnityEngine.Debug.Log("Open K4A device successful. id " + id + "sn:" + device.SerialNum);

                // 獲取設備校準資料，用於座標轉換和追蹤精度
                var deviceCalibration = device.GetCalibration();

                // 創建身體追蹤器
                using (Tracker tracker = Tracker.Create(deviceCalibration, new TrackerConfiguration()
                {
                    ProcessingMode = TrackerProcessingMode.Gpu,  // 使用 GPU 加速處理
                    SensorOrientation = SensorOrientation.Default // 使用預設感測器方向
                }))
                {
                    UnityEngine.Debug.Log("Body tracker created.");

                    // 主要處理迴圈，持續運行直到收到取消信號
                    while (!token.IsCancellationRequested)
                    {
                        // 從感測器獲取最新的擷取資料
                        using (Capture sensorCapture = device.GetCapture())
                        {
                            // 將最新幀加入追蹤器的處理佇列
                            tracker.EnqueueCapture(sensorCapture);
                        }

                        // 嘗試從追蹤器獲取最新的處理結果
                        using (Frame frame = tracker.PopResult(TimeSpan.Zero, throwOnTimeout: false))
                        {
                            if (frame == null)
                            {
                                // 如果沒有可用的結果，繼續下一次迴圈
                                UnityEngine.Debug.Log("Pop result from tracker timeout!");
                            }
                            else
                            {
                                // 標記追蹤器正在運行
                                IsRunning = true;

                                // 獲取當前幀中的身體數量
                                currentFrameData.NumOfBodies = frame.NumberOfBodies;

                                // 複製所有檢測到的身體資料
                                for (uint i = 0; i < currentFrameData.NumOfBodies; i++)
                                {
                                    // 從 Kinect SDK 複製身體資料到我們的資料結構
                                    currentFrameData.Bodies[i].CopyFromBodyTrackingSdk(frame.GetBody(i), deviceCalibration);
                                }

                                // 處理深度影像資料
                                Capture bodyFrameCapture = frame.Capture;
                                Image depthImage = bodyFrameCapture.Depth;

                                // 如果這是第一幀，記錄初始時間戳
                                if (!readFirstFrame)
                                {
                                    readFirstFrame = true;
                                    initialTimestamp = depthImage.DeviceTimestamp;
                                }

                                // 計算相對時間戳（毫秒）
                                currentFrameData.TimestampInMs = (float)(depthImage.DeviceTimestamp - initialTimestamp).TotalMilliseconds;

                                // 記錄深度影像的尺寸資訊
                                currentFrameData.DepthImageWidth = depthImage.WidthPixels;
                                currentFrameData.DepthImageHeight = depthImage.HeightPixels;

                                // 從 SDK 讀取影像資料並轉換為 ushort 陣列
                                var depthFrame = MemoryMarshal.Cast<byte, ushort>(depthImage.Memory.Span);

                                // 重新封裝深度資料並儲存影像資料
                                int byteCounter = 0;
                                // 計算 RGB 格式的影像大小（每個像素 3 個位元組）
                                currentFrameData.DepthImageSize = currentFrameData.DepthImageWidth * currentFrameData.DepthImageHeight * 3;

                                // 將深度資料轉換為 RGB 格式的視覺化資料
                                for (int it = currentFrameData.DepthImageWidth * currentFrameData.DepthImageHeight - 1; it > 0; it--)
                                {
                                    // 將深度值正規化到 0-255 範圍，用於視覺化顯示
                                    byte b = (byte)(depthFrame[it] / (ConfigLoader.Instance.Configs.SkeletalTracking.MaximumDisplayedDepthInMillimeters) * 255);

                                    // 將灰階值填入 R、G、B 三個通道
                                    currentFrameData.DepthImage[byteCounter++] = b;
                                    currentFrameData.DepthImage[byteCounter++] = b;
                                    currentFrameData.DepthImage[byteCounter++] = b;
                                }

                                // 如果啟用了資料記錄功能，將資料序列化並寫入檔案
                                if (RawDataLoggingFile != null && RawDataLoggingFile.CanWrite)
                                {
                                    binaryFormatter.Serialize(RawDataLoggingFile, currentFrameData);
                                }

                                // 更新共享的資料變數，供 UI 執行緒讀取
                                // 這裡使用父類別的執行緒安全方法
                                SetCurrentFrameData(ref currentFrameData);
                            }
                        }
                    }

                    // 收到取消信號後，清理追蹤器資源
                    Debug.Log("dispose of tracker now!!!!!");
                    tracker.Dispose();
                }

                // 清理設備資源
                device.Dispose();
            }

            // 關閉資料記錄檔案
            if (RawDataLoggingFile != null)
            {
                RawDataLoggingFile.Close();
            }
        }
        catch (Exception e)
        {
            // 捕獲並記錄背景執行緒中的例外
            Debug.Log($"catching exception for background thread {e.Message}");

            // 如果收到取消請求，重新拋出 OperationCanceledException
            token.ThrowIfCancellationRequested();
        }
    }
}