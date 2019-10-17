### CMU North Star Institute
Last Updated: Oct 16, 2019
# North Star Setup

1. Download and extract this repository 
2. Open `Unity Project - North Star & Vive Tracker` in Unity


## Importing the display calibration file for your North Star hardware
1. Your North Star hardware number is on the rear of the left display with a Roman numeral
2. Under the `North Star + Leap Motion` game object, select the `ARCameraRig` game object
3. Under `Optical Calibration Manger` component, change *Input Calibration File* to ’NS-X.json’ where X is the Arabic number of your headset
	 - All North Star calibration files are located within `Assets / CalibrationFiles`

## Setting up 6-DOF tracking with Vive Tracker 
1. Turn off all Vive Controllers, only the Tracker should be connected
2. Select `North Star + Leap Motion` game object
	 - Under `Steam VR Tracker` component, set *Device ID* to 1


# Other

## Customizing the Leap Motion’s relative transform
1. Navigate to the game object `North Star + Leap Motion` → `AR CameraRig` → `Head` → `Leap Provider`
2. Within the `Leap XR Service Provider` component, under advanced, you can set *Device Offset Mode* to Default, Manual Head Offset, or Transform


# Helpful Links
 - https://github.com/leapmotion/ProjectNorthStar/
 - https://leapmotion.github.io/ProjectNorthStar/
