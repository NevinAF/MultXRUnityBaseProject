This is a temporary readme.

This is a base project file for XR Mulity-Platform development in unity. This is intended to be used for SteamVR and Andriod (Quest) builds, but also has some basic functionallity for viewing and running applications without a VR setup.

Usage information is a wip. If trying to use the files now, here is some insight:

- To add the multi-platform rig to a scene, add the following prefab: "Assets\NevsBaseDevelopmentKit\HMD Manager.prefab"
- Input: The system currently works with Device-Based XR Interaction and keyboard buttons. Other input management systems (like SteamVR or Action-Based XR) will only work on some platforms (if as all, but just as of now).
- Use InputDeviceWrapper class to get inputs from the user (more on this later).