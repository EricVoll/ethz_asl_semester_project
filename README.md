# ethz_asl_semester_project
My Semester Project at the ETHZ ASL institute


# Setup
1. Install ROS melodic
2. Setup catkin workspace using:
 - `cd ~ && mkdir -p catkin_ws/src && cd catkin_ws`
 - `catkin config --init --merge-devel --cmake-args -DCMAKE_BUILD_TYPE=Release`
 - `catkin config --extend /opt/ros/melodic`
 - `sudo apt-get intsall -y libgoogle-glog-dev`
 - `sudo apt-get install ros-melodic-rosbridge-server`
 - ``
 - ``
 - ``

# Depenencies:
- https://github.com/ethz-asl/rotors_simulator
- https://github.com/EricVoll/ros-sharp

# Importing the drone-model into Unity:
Three things have to be done before you can import the robot model:
- Install and roslaunch the file_server package contained in [ros-sharp](https://github.com/EricVoll/ros-sharp)
- Roslaunch the rotors_description launch file you want to import to Unity. Make sure, that the launch file writes the robot's urdf into the "robot_description" paramter.
- Install and roslaunch the ros-bridge
- Pull the RosSharp folder into the Unity project, such that the "RosBridgeClient" menu item appears in the menu-bar. There you can enter the IP adress of the device running the ROS-master and click import.
