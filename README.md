# ethz_asl_semester_project
My Semester Project at the ETHZ ASL institute


# Setup
1. Install ROS melodic
2. Setup catkin workspace using:
 ```
 cd ~ && mkdir -p catkin_ws/src && cd catkin_ws`
 catkin config --init --merge-devel --cmake-args -DCMAKE_BUILD_TYPE=Release`
 catkin config --extend /opt/ros/melodic`
 sudo apt-get install ros-melodic-rosbridge-server`
 cd ~/catkin_ws/src
 git clone https://github.com/EricVoll/azure_spatial_anchors_ros
 sudo apt-get install ros-$ROS_DISTRO-catkin ros-$ROS_DISTRO-tf2-eigen python-catkin-tools libgflags-dev libgoogle-glog-dev python-wstool
 git clone https://github.com/EricVoll/SemesterProjectRos mr-drone
 git clone https://github.com/ethz-asl/rovio 
 cd rovio && git submodule update --init --recursive && cd ..
 git clone -b feature/poseupdate https://github.com/ethz-asl/rovio
 git clone https://github.com/catkin/catkin_simple.git
 git clone https://github.com/ethz-asl/kindr
 git clone https://github.com/ethz-asl/rotors_simulator
 git clone https://github.com/ethz-asl/mav_comm
 sudo apt-get update
 sudo apt-get install ros-melodic-mavros ros-melodic-mavros-extras
 sudo apt-get install ros-melodic-octomap ros-melodic-octomap-ros python-wstool python-catkin-tools protobuf-compiler libgoogle-glog-dev
 
 catkin build
 ```
 If you run into weird errors while building, try a clean build by deleting the `catkin_ws/build` folder.
 
 
# Depenencies:
- https://github.com/ethz-asl/rotors_simulator
- https://github.com/EricVoll/ros-sharp
- https://github.com/ethz-asl/rovio (read this [issue](https://github.com/ethz-asl/rovio/issues/183) for installing)

# Importing the drone-model into Unity:
Three things have to be done before you can import the robot model:
- Install and roslaunch the file_server package contained in [ros-sharp](https://github.com/EricVoll/ros-sharp)
- Roslaunch the rotors_description launch file you want to import to Unity. Make sure, that the launch file writes the robot's urdf into the "robot_description" paramter.
- Install and roslaunch the ros-bridge
- Pull the RosSharp folder into the Unity project, such that the "RosBridgeClient" menu item appears in the menu-bar. There you can enter the IP adress of the device running the ROS-master and click import.
