using UnityEngine;
using Valve.VR;

public class SteamVRTracker : MonoBehaviour {
  bool startedSteamVR = false;
    public int deviceID = 1;

	void Start () {
    if (OpenVR.System == null) {
      var error = EVRInitError.None;
      OpenVR.Init(ref error, EVRApplicationType.VRApplication_Utility);
      startedSteamVR = true;
    }
  }

	void LateUpdate () {
    TrackedDevicePose_t[] devicePoses = new TrackedDevicePose_t[16];
    if (OpenVR.System != null) {
      OpenVR.System.GetDeviceToAbsoluteTrackingPose(ETrackingUniverseOrigin.TrackingUniverseRawAndUncalibrated, 0.03f, devicePoses);
    } else
        {
            Debug.Log("no OpenVR system found");
        }

      int i = deviceID;
      if (devicePoses[i].bDeviceIsConnected || devicePoses[i].bPoseIsValid) {
        SteamVR_Utils.RigidTransform pose = new SteamVR_Utils.RigidTransform(devicePoses[i].mDeviceToAbsoluteTracking);
        transform.localPosition = pose.pos;
        transform.localRotation = pose.rot;
      }
  }

  void OnDestroy() {
    if (startedSteamVR) {
      OpenVR.Shutdown();
    }
  }
}
