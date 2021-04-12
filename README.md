# ethz_asl_semester_project
My Semester Project at the ETHZ ASL institute. [Video](https://youtu.be/SxmkRreG5j8).

The drone performs Visual Inertial Odometry while using Azure Spatial Anchors to compensate for its drift using anchors created by the HoloLens 2. Simultaneously, the HoloLens 2 receives the drone's pose relative to an anchor and can project holograms on it.

This handheld sensor pod is a mockup with the same hardware as the real omnidirectional drone has - just no actuators. The drone is supposed to follow the trajectory and clean/measure/touch/exert forces onto the real-world structure's surface while having a much more intuitive interface using MR.

This repo contains the code for the Unity Parts of the system, including everything you can see in the video linked above.
The GitHub repos for the ROS parts can be found here: github.com/EricVoll/vio_drift_comp_using_ASA
The ROS repo also contains a details report of the project.

# Importing the drone-model into Unity:
Three things have to be done before you can import the robot model:
- Install and roslaunch the file_server package contained in [ros-sharp](https://github.com/EricVoll/ros-sharp)
- Roslaunch the rotors_description launch file you want to import to Unity. Make sure, that the launch file writes the robot's urdf into the "robot_description" paramter.
- Install and roslaunch the ros-bridge
- Pull the RosSharp folder into the Unity project, such that the "RosBridgeClient" menu item appears in the menu-bar. There you can enter the IP adress of the device running the ROS-master and click import.
