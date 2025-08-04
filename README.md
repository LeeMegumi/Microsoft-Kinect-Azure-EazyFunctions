# \# Kinect Azure Eazy Functions （多人體骨架追蹤）



> 基於 Microsoft 官方 \*\*Azure Kinect Unity Body-Tracking Sample\*\* 改裝而成，  

> 讓希望「快速取得骨架資料」的開發者能夠開箱即用。

##### ---

#### \## 特色簡介

\* \*\*一鍵啟動\*\*：內建 `Kinect Launcher`，開場景即開始擷取深度與骨架。  

\* \*\*Easy\_Functions API\*\*：提供多項常用函式，兩行程式即可讀取人數、關節座標、切換 UI 顯示等。  

\* \*\*完整範例\*\*：`Samples/BasicExample` 場景示範即時偵測、深度影像顯示與快捷鍵操作。

##### ---

#### \## 安裝步驟

1\. 下載並解壓 \*\*Microsoft-Kinect-Azure-EazyFunctions\*\* 專案。  

&nbsp;  建議以 \*\*Unity 2020.3.53f1\*\* 或相容版本開啟。  

2\. 安裝 \*\*Microsoft Azure Kinect SDK\*\* 與 \*\*Body Tracking SDK\*\*  

&nbsp;  （需包含 \*Kinect Azure Viewer\* 與 \*Body Tracking\* 元件）。  

&nbsp;  \*\*⚠ 必裝！\*\* 若未正確安裝，深度相機將無法運作。  

3\. 於 Unity 專案中開啟範例場景：  

&nbsp;  `Assets/Packages/com.megumi.kinect-azure-tracker/Samples/BasicExample/BasicExample.unity`

##### ---

#### \## 範例場景說明

###### | 物件 | 作用 |

###### |------|------|

###### | \*\*Environment\*\* | 燈光與地面，純佈景，可忽略。 |

###### | \*\*Kinect Launcher\*\* | 初始化深度相機與骨架追蹤流程。 |

###### | \*\*Main Camera\*\* | 場景視角，相機本身無特殊功能。 |

###### | \*\*Canvas\*\* | UI 根物件，內含即時深度影像。 |

###### | \*\*How to use\*\* | 掛載 `Easy\_Functions` 腳本，可直接呼叫其 Public 方法。 |

##### ---

#### \## Easy\_Functions 公開方法一覽

###### | 方法 | 說明 |

###### |------|------|

###### | `GetDetectedBodyCount()` | 取得目前偵測到的人數。 |

###### | `GetHeadPosition(bodyId=0)` | 取得指定人物頭部的本地座標。 |

###### | `GetHeadWorldPosition(bodyId=0)` | 取得頭部的世界座標。 |

###### | `GetAllHeadPositions()` | 取得所有人物頭部座標清單。 |

###### | `GetJointPosition(jointId, bodyId=0)` | 取得任意關節本地座標。 |

###### | `GetAllBodyIds()` | 取得所有偵測到的 body ID。 |

###### | `GetClosestBodyId()` | 取得距離最近的人物 ID。 |

###### | `ToggleSkeleton()` / `SetSkeletonVisibility(bool)` | 切換或設定骨架顯示。 |

###### | `ToggleDepthImage()` / `SetDepthImageVisibility(bool)` | 切換或設定深度影像顯示。 |

###### | `ToggleImageFlip()` | 循環切換影像翻轉模式。 |

###### | `SetImageScale(int index)` | 設定深度影像縮放（內建 0.5×/1×/1.5×）。 |

###### | `ResetAllSettings()` | 重置骨架、影像顯示與影像設定。 |

###### | `PrintCurrentInfo()` | 在 Console 印出偵測詳情。 |

###### | `GetSystemStatusString()` | 取得系統狀態字串。 |

###### | `EstimateBodyHeight(bodyId=0)` | 粗估身高。 |

###### | `IsRaisingBothHands(bodyId=0)` | 是否舉雙手。 |

###### | `IsWaving(bodyId=0)` | 是否招手。 |

##### ---

###### \## 鍵盤快捷鍵（範例場景）

###### | 鍵 | 功能 |

###### |----|------|

###### | \*\*S\*\* | 開 / 關骨架顯示 |

###### | \*\*D\*\* | 開 / 關深度影像 |

###### | \*\*I\*\* | Console 列印偵測資訊 |

###### | \*\*F\*\* | 影像翻轉模式切換 |

###### | \*\*Shift + 1/2/3\*\* | 深度影像縮放 0.5× / 1× / 1.5× |

###### | \*\*Esc\*\* | 重置所有設定 |

##### ---

#### \## 授權

#### 本專案僅供學術研究與教學示範使用。請遵守 Microsoft Azure Kinect SDK 及 Body Tracking SDK 官方授權條款。

# 

#### ---------------------------

# 

# \# Kinect Azure Eazy Functions (Multi-Body Tracker)

> A plug-and-play wrapper around the official \*\*Azure Kinect Unity Body-Tracking Sample\*\*.  

> Grab real-time skeleton data in just a few lines of code.



#### \## Features

\* \*\*One-click launch\*\* – `Kinect Launcher` starts depth capture and body tracking automatically.  

\* \*\*Easy\_Functions API\*\* – call ready-made methods to get body count, joint positions or toggle UI.  

\* \*\*Complete sample\*\* – see `Samples/BasicExample` for live tracking, depth view and hot-keys.



#### \## Installation

1\. Download and unzip \*\*Microsoft-Kinect-Azure-EazyFunctions\*\*.  

&nbsp;  Open with \*\*Unity 2020.3.53f1\*\* (or a compatible LTS).  

2\. Install \*\*Azure Kinect SDK\*\* and \*\*Body Tracking SDK\*\* including \*Kinect Azure Viewer\* and \*Body Tracking\* modules.  

&nbsp;  \*\*Required –\*\* the depth camera will not work without them.  

3\. Open the sample scene:  

&nbsp;  `Assets/Packages/com.megumi.kinect-azure-tracker/Samples/BasicExample/BasicExample.unity`



#### \## Sample Scene Objects

###### | Object | Purpose |

###### |--------|---------|

###### | \*\*Environment\*\* | Lights and floor only. |

###### | \*\*Kinect Launcher\*\* | Initializes depth camera and body tracking. |

###### | \*\*Main Camera\*\* | Scene view. |

###### | \*\*Canvas\*\* | UI root, contains the live depth texture. |

###### | \*\*How to use\*\* | Holds the `Easy\_Functions` script – call its methods directly. |

###### 

#### \## Easy\_Functions Public API

###### | Method | Description |

###### |--------|-------------|

###### | `GetDetectedBodyCount()` | Current number of tracked bodies. |

###### | `GetHeadPosition(bodyId=0)` | Local coordinates of the head joint. |

###### | `GetHeadWorldPosition(bodyId=0)` | World coordinates of the head joint. |

###### | `GetAllHeadPositions()` | List of all head positions. |

###### | `GetJointPosition(jointId, bodyId=0)` | Local coords of any joint. |

###### | `GetAllBodyIds()` | List of tracked body IDs. |

###### | `GetClosestBodyId()` | Returns the ID of the closest body. |

###### | `ToggleSkeleton()` / `SetSkeletonVisibility(bool)` | Toggle or set skeleton lines. |

###### | `ToggleDepthImage()` / `SetDepthImageVisibility(bool)` | Toggle or set depth view. |

###### | `ToggleImageFlip()` | Cycle through flip modes. |

###### | `SetImageScale(int index)` | Set depth view scale (0.5× / 1× / 1.5×). |

###### | `ResetAllSettings()` | Restore default display settings. |

###### | `PrintCurrentInfo()` | Log detailed tracking info to Console. |

###### | `GetSystemStatusString()` | Returns a status string. |

###### | `EstimateBodyHeight(bodyId=0)` | Rough height estimation. |

###### | `IsRaisingBothHands(bodyId=0)` | Checks if both hands are raised. |

###### | `IsWaving(bodyId=0)` | Checks if waving with right hand. |

# 

#### \## Hot-keys (Sample Scene)

###### | Key | Action |

###### |-----|--------|

###### | \*\*S\*\* | Toggle skeleton |

###### | \*\*D\*\* | Toggle depth view |

###### | \*\*I\*\* | Print tracking info |

###### | \*\*F\*\* | Flip image |

###### | \*\*Shift + 1/2/3\*\* | Scale image 0.5× / 1× / 1.5× |

###### | \*\*Esc\*\* | Reset all settings |



#### \## License

For academic and educational use only. Please comply with Microsoft Azure Kinect SDK and Body Tracking SDK license terms.



