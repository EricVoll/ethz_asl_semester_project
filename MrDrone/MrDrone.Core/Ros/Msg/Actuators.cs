﻿/* 
 * This message is auto generated by ROS#. Please DO NOT modify.
 * Note:
 * - Comments from the original code will be written in their own line 
 * - Variable sized arrays will be initialized to array of size 0 
 * Please report any issues at 
 * <https://github.com/siemens/ros-sharp> 
 */



using RosSharp.RosBridgeClient.MessageTypes.Std;

namespace RosSharp.RosBridgeClient.MessageTypes.Mav
{
    public class Actuators : Message
    {
        public override string RosMessageName => "mav_msgs/Actuators";

        public Header header { get; set; }
        //  This message defines lowest level commands to be sent to the actuator(s). 
        public double[] angles { get; set; }
        //  Angle of the actuator in [rad]. 
        //  E.g. servo angle of a control surface(not angle of the surface!), orientation-angle of a thruster.      
        public double[] angular_velocities { get; set; }
        //  Angular velocities of the actuator in [rad/s].
        //  E.g. "rpm" of rotors, propellers, thrusters 
        public double[] normalized { get; set; }
        //  Everything that does not fit the above, normalized between [-1 ... 1].

        public Actuators()
        {
            this.header = new Header();
            this.angles = new double[0];
            this.angular_velocities = new double[0];
            this.normalized = new double[0];
        }

        public Actuators(Header header, double[] angles, double[] angular_velocities, double[] normalized)
        {
            this.header = header;
            this.angles = angles;
            this.angular_velocities = angular_velocities;
            this.normalized = normalized;
        }
    }
}
