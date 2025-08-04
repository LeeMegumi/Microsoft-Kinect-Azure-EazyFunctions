using UnityEngine;

public class main : MonoBehaviour
{
    public static main Instance {  get; private set; }
    //tracker ids needed for when there are two trackers
    [Header("Kinect Azure Sensor")]
    private int TRACKER_ID = 0;
    // Handler for SkeletalTracking thread.
    [Header("BodyTracking Script Object")]
    public GameObject m_tracker;
    public SkeletalTrackingProvider m_skeletalTrackingProvider;
    public BackgroundData m_lastFrameData = new BackgroundData();
    private TrackerHandler_multi trackerHandler;

    private void Awake()
    {
        if (Instance == null) 
        {
            Instance = this;
        }
        m_skeletalTrackingProvider = new SkeletalTrackingProvider(TRACKER_ID);
    }
    void Start()
    {
        trackerHandler = m_tracker.GetComponent<TrackerHandler_multi>();
    }

    void Update()
    {
        if (m_skeletalTrackingProvider.IsRunning)
        {
            if (m_skeletalTrackingProvider.GetCurrentFrameData(ref m_lastFrameData))
            {
                if (m_lastFrameData.NumOfBodies >= 0)
                {
                   trackerHandler.updateTracker(m_lastFrameData);
                }
            }
        }

        if(Input.GetKeyUp(KeyCode.A))
        {
            trackerHandler.turnOnOffSkeletons();
        }
        //trackerHandler.(Microsoft.Azure.Kinect.BodyTracking.JointId.HandRight, 0);
    }

    void OnApplicationQuit()
    {
        if (m_skeletalTrackingProvider != null)
        {
            m_skeletalTrackingProvider.Dispose();
        }
    }
}
