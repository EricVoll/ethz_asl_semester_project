using JetBrains.Annotations;
using Microsoft.MixedReality.Toolkit.UI;
using RosSharp.RosBridgeClient;
using SpotSharp;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIJoystick : MonoBehaviour, IServiceConsumer<IServiceMessage>
{
    [Tooltip("The RosConnector used to communicate with the rosBridge")]
    public RosConnector rosConnector;

    [Tooltip("The speed value used for commanding")]
    public float commandSpeed = 1f;

    [Tooltip("The duration after which a command is auto-stopped for safety in seconds")]
    public float commandFaleSafeDuration = 5f;

    // Start is called before the first frame update
    void Start()
    {
        Toolkit.singleton.RegisterServiceConsumer(this, "ConnectionStateService");
    }

    /// <summary>
    /// contains the values for the twist to command the velocity
    /// </summary>
    private float[] current_Velocities;

    // Update is called once per frame
    void Update()
    {
        if (current_Velocities != null && current_Velocities.Length == 3 && Time.realtimeSinceStartup - commandStartTime < commandFaleSafeDuration)
        {
            spot.CommandVelocity(new RosSharp.RosBridgeClient.MessageTypes.Geometry.Twist()
            {
                angular = new RosSharp.RosBridgeClient.MessageTypes.Geometry.Vector3()
                {
                    z = current_Velocities[2]
                },
                linear = new RosSharp.RosBridgeClient.MessageTypes.Geometry.Vector3()
                {
                    x = current_Velocities[0],
                    y = current_Velocities[1]
                }
            });
        }
    }

    public void OnClick(string direction)
    {
        if(spot == null)
        {
            NotifyNotConnected();
            return;
        }

        switch (direction)
        {
            case "left":
                CommandVelocityStart(0, commandSpeed, 0);
                break;
            case "up":
                CommandVelocityStart(commandSpeed, 0, 0);
                break;
            case "right":
                CommandVelocityStart(0, -commandSpeed, 0);
                break;
            case "down":
                CommandVelocityStart(-commandSpeed, 0, 0);
                break;
            case "rotate_left":
                CommandVelocityStart(0, 0, commandSpeed);
                break;
            case "rotate_right":
                CommandVelocityStart(0, 0, -commandSpeed);
                break;
            default:
                break;
        }
    }

    public void OnClickEnd(string direction)
    {
        CommandVelocitiyStop();
    }

    private void NotifyNotConnected()
    {
        Toolkit.singleton.TriggerEvent("message_box_service",
             new MessageBoxContent(5, $"Spot not connected", $"Spot is not yet connected. You'll be notified when a connection is created."));
    }

    Spot spot;

    public void ConsumeServiceItem(IServiceMessage item, string serviceName)
    {
        //Here we listen to all ros msg and update our robot state
        if (serviceName == "ConnectionStateService")
        {
            bool newRosConnectionState = ((ConnectionStateMessage)item).ConnectionState;
            if (newRosConnectionState)
            {
                spot = new Spot(rosConnector.RosSocket);
                Debug.Log("Did set spot");
            }
            else
            {
                spot = null;
            }
        }
    }

    void CommandVelocitiyStop()
    {
        current_Velocities = null;
    }

    float commandStartTime = 0;
    void CommandVelocityStart(float x, float y, float angle)
    {
        commandStartTime = Time.realtimeSinceStartup;
        current_Velocities = new[] { x, y, angle };
    }

    public void Stand()
    {
        if(spot == null)
        {
            NotifyNotConnected();
            return;
        }
        spot.Stand();
    }
    public void Sit()
    {
        if (spot == null)
        {
            NotifyNotConnected();
            return;
        }
        spot.Sit();
    }
    public void SelfRight()
    {
        if (spot == null)
        {
            NotifyNotConnected();
            return;
        }
        spot.SelfRight();
    }

    public void PowerOn()
    {
        if (spot == null)
        {
            NotifyNotConnected();
            return;
        }
        spot.PowerOn();
    }
    public void PowerOff()
    {
        if (spot == null)
        {
            NotifyNotConnected();
            return;
        }
        spot.Sit();
        spot.PowerOff();
    }
    
}
