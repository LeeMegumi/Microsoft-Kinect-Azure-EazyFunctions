using System;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// 抽象的背景資料提供者基類
/// 用於在背景執行緒中處理資料，並提供執行緒安全的資料交換機制
/// 實作IDisposable介面以確保資源正確釋放
/// </summary>
public abstract class BackgroundDataProvider : IDisposable
{
    // 儲存背景執行緒產生的資料
    private BackgroundData m_frameBackgroundData = new BackgroundData();
    
    // 標記是否有最新的資料可用
    private bool m_latest = false;
    
    // 用於執行緒同步的鎖定物件，確保資料交換的執行緒安全
    object m_lockObj = new object();
    
    /// <summary>
    /// 指示背景資料提供者是否正在運行
    /// </summary>
    public bool IsRunning { get; set; } = false;
    
    // 用於取消背景任務的取消令牌來源
    private CancellationTokenSource _cancellationTokenSource;
    
    // 取消令牌，用於通知背景執行緒停止運行
    private CancellationToken _token;

    /// <summary>
    /// 建構函式 - 初始化背景資料提供者
    /// </summary>
    /// <param name="id">提供者的識別ID</param>
    public BackgroundDataProvider(int id)
    {
#if UNITY_EDITOR
        // 在Unity編輯器中，當編輯器關閉時自動清理資源
        UnityEditor.EditorApplication.quitting += OnEditorClose;
#endif
        // 建立取消令牌來源，用於控制背景執行緒的生命週期
        _cancellationTokenSource = new CancellationTokenSource();
        _token = _cancellationTokenSource.Token;
        
        // 啟動背景執行緒任務
        Task.Run(() => RunBackgroundThreadAsync(id, _token));
    }

    /// <summary>
    /// Unity編輯器關閉時的事件處理器
    /// 確保在編輯器關閉時正確清理資源
    /// </summary>
    private void OnEditorClose()
    {
        Dispose();
    }

    /// <summary>
    /// 抽象方法 - 子類必須實作此方法來定義背景執行緒的具體工作
    /// 此方法將在背景執行緒中持續運行，直到收到取消信號
    /// </summary>
    /// <param name="id">提供者的識別ID</param>
    /// <param name="token">用於檢查是否需要取消執行的令牌</param>
    protected abstract void RunBackgroundThreadAsync(int id, CancellationToken token);

    /// <summary>
    /// 設定當前幀的資料 (由背景執行緒調用)
    /// 使用雙重緩衝機制，避免資料複製的開銷
    /// </summary>
    /// <param name="currentFrameData">要設定的當前幀資料</param>
    public void SetCurrentFrameData(ref BackgroundData currentFrameData)
    {
        lock (m_lockObj) // 確保執行緒安全
        {
            // 使用指標交換的方式避免資料複製
            // 將新資料與內部緩衝區交換
            var temp = currentFrameData;
            currentFrameData = m_frameBackgroundData;
            m_frameBackgroundData = temp;
            
            // 標記有新資料可用
            m_latest = true;
        }
    }

    /// <summary>
    /// 獲取當前幀的資料 (由主執行緒調用)
    /// 使用雙重緩衝機制，避免資料複製的開銷
    /// </summary>
    /// <param name="dataBuffer">用於接收資料的緩衝區</param>
    /// <returns>如果有新資料則返回true，否則返回false</returns>
    public bool GetCurrentFrameData(ref BackgroundData dataBuffer)
    {
        lock (m_lockObj) // 確保執行緒安全
        {
            // 使用指標交換的方式避免資料複製
            // 將內部資料與外部緩衝區交換
            var temp = dataBuffer;
            dataBuffer = m_frameBackgroundData;
            m_frameBackgroundData = temp;
            
            // 記錄是否有最新資料
            bool result = m_latest;
            
            // 重置最新資料標記
            m_latest = false;
            
            return result;
        }
    }

    /// <summary>
    /// 釋放資源並停止背景執行緒
    /// 實作IDisposable介面
    /// </summary>
    public void Dispose()
    {
#if UNITY_EDITOR
        // 移除Unity編輯器事件監聽
        UnityEditor.EditorApplication.quitting -= OnEditorClose;
#endif
        
        // 發送取消信號給背景執行緒
        _cancellationTokenSource?.Cancel();
        
        // 釋放取消令牌來源的資源
        _cancellationTokenSource?.Dispose();
        
        // 清空引用
        _cancellationTokenSource = null;
    }
}