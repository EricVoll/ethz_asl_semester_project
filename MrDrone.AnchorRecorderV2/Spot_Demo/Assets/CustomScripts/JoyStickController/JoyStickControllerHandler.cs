// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.MixedReality.Toolkit.Input;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;
using TMPro;
using Microsoft.Win32;
using RosSharp.RosBridgeClient;
using SpotSharp;
using System;
using Microsoft.MixedReality.Toolkit;
using System.Runtime.InteropServices.ComTypes;
using System.Collections;

/// <summary>
/// Example script to demonstrate joystick control in sample scene
/// </summary>
public class JoyStickControllerHandler : MonoBehaviour, IServiceConsumer<IServiceMessage>
{
    [SerializeField, FormerlySerializedAs("objectToManipulate")]
    [Tooltip("The large or small game object that receives manipulation by the joystick.")]
    private GameObject targetObject = null;
    public GameObject TargetObject
    {
        get => targetObject;
        set => targetObject = value;
    }

    [SerializeField]
    [Tooltip("A TextMeshPro object that displays joystick values.")]
    private TextMeshPro debugText = null;

    [SerializeField]
    [Tooltip("The joystick mesh that gets rotated when this control is interacted with.")]
    private GameObject joystickVisual = null;

    [SerializeField]
    [Tooltip("The mesh + collider object that gets dragged and controls the joystick visual rotation.")]
    private GameObject grabberVisual = null;

    [SerializeField]
    [Tooltip("Toggles on / off the GrabberVisual's mesh renderer because it can be dragged away from the joystick visual, it kind of breaks the illusion of pushing / pulling a lever.")]
    private bool showGrabberVisual = true;

    [Tooltip("The speed at which the JoystickVisual and GrabberVisual move / rotate back to a neutral position.")]
    [Range(1, 20)]
    public float ReboundSpeed = 5;

    [Tooltip("How sensitive the joystick reacts to dragging left / right. Customize this value to get the right feel for your scenario.")]
    [Range(0.01f, 10)]
    public float SensitivityLeftRight = 3;

    [Tooltip("How sensitive the joystick reacts to pushing / pulling. Customize this value to get the right feel for your scenario.")]
    [Range(0.01f, 10)]
    public float SensitivityForwardBack = 6;

    [Tooltip("The distance multiplier for joystick input. Customize this value to get the right feel for your scenario.")]
    [Range(0.0003f, 0.2f)]
    public float MoveSpeed = 0.01f;

    [Tooltip("The rotation multiplier for joystick input. Customize this value to get the right feel for your scenario.")]
    [Range(0.01f, 1f)]
    public float RotationSpeed = 0.05f;

    [Tooltip("The scale multiplier for joystick input. Customize this value to get the right feel for your scenario.")]
    [Range(0.00003f, 0.003f)]
    public float ScaleSpeed = 0.001f;

    [Tooltip("The RosConnector used to communicate with the rosBridge")]
    public RosConnector rosConnector;

    public Vector3 currentSpeed;

    private Vector3 startPosition;
    private Vector3 joystickGrabberPosition;
    private Vector3 joystickVisualRotation;
    private const int joystickVisualMaxRotation = 80;
    private bool isDragging = false;
    private void Start()
    {
        startPosition = grabberVisual.transform.localPosition;
        if (grabberVisual != null)
        {
            grabberVisual.GetComponent<MeshRenderer>().enabled = showGrabberVisual;
        }

        Toolkit.singleton.RegisterServiceConsumer(this, "ConnectionStateService");
    }

    private void Update()
    {
        if (!isDragging)
        {
            // when dragging stops, move joystick back to idle
            if (grabberVisual != null)
            {
                grabberVisual.transform.localPosition = Vector3.Lerp(grabberVisual.transform.localPosition, startPosition, Time.deltaTime * ReboundSpeed);
            }
        }
        CalculateJoystickRotation();
        ApplyJoystickValues();

        if(timeToRotate > Time.realtimeSinceStartup)
        {
            spot.CommandVelocity(new RosSharp.RosBridgeClient.MessageTypes.Geometry.Twist()
            {
                angular = new RosSharp.RosBridgeClient.MessageTypes.Geometry.Vector3()
                {
                    z = rotationAngle
                },
                linear = new RosSharp.RosBridgeClient.MessageTypes.Geometry.Vector3()
            });
        }
    }

    private void CalculateJoystickRotation()
    {
        joystickGrabberPosition = grabberVisual.transform.localPosition - startPosition;
        // Left Right = Horizontal
        joystickVisualRotation.z = Mathf.Clamp(-joystickGrabberPosition.x * SensitivityLeftRight, -joystickVisualMaxRotation, joystickVisualMaxRotation);
        // Forward Back = Vertical
        joystickVisualRotation.x = Mathf.Clamp(joystickGrabberPosition.z * SensitivityForwardBack, -joystickVisualMaxRotation, joystickVisualMaxRotation);
        // TODO: calculate joystickVisualRotation.y to always face the proper direction (for when the joystick container gets moved around the scene)
        if (joystickVisual != null)
        {
            joystickVisual.transform.localRotation = Quaternion.Euler(joystickVisualRotation);
        }
    }

    private void ApplyJoystickValues()
    {
        if (TargetObject != null)
        {
            currentSpeed = (joystickGrabberPosition * MoveSpeed);

            TargetObject.transform.position += currentSpeed;
            if (debugText != null)
            {
                debugText.text = currentSpeed.ToString();
            }

            CommandRobot(currentSpeed);
        }
    }

    /// <summary>
    /// The ObjectManipulator script uses this to determine when the joystick is grabbed.
    /// </summary>
    public void StartDrag()
    {
        isDragging = true;
    }
    /// <summary>
    /// The ObjectManipulator script uses this to determine when the joystick is released.
    /// </summary>
    public void StopDrag()
    {
        isDragging = false;
    }


    #region RosCommanding

    Spot spot;
    float timeToRotate = 0;
    float rotationAngle = 0;

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

    private void CommandRobot(Vector3 currentSpeed)
    {
        //We want to avoid exp(-t) effects sending weak signals
        if (currentSpeed.magnitude < .2f) return;

        if (spot == null)
        {
            Debug.Log("Spot is null. Translation ignored in Joystick controller.");
            return;
        }

        currentSpeed = RosSharp.TransformExtensions.Unity2Ros(currentSpeed);


        spot.CommandVelocity(new RosSharp.RosBridgeClient.MessageTypes.Geometry.Twist()
        {
            angular = new RosSharp.RosBridgeClient.MessageTypes.Geometry.Vector3(),
            linear = new RosSharp.RosBridgeClient.MessageTypes.Geometry.Vector3()
            {
                x = currentSpeed.x,
                y = currentSpeed.y,
                z = currentSpeed.z
            }
        });
    }

    public void RotateLeft()
    {
        if (spot == null)
        {
            Debug.Log("Spot is null. RotateLeft ignored");
            return;
        }

        timeToRotate = Time.realtimeSinceStartup + 1f;
        rotationAngle = .8f;
    }

    public void RotateRight()
    {
        if (spot == null)
        {
            Debug.Log("Spot is null. Rotateright ignored");
            return;
        }

        timeToRotate = Time.realtimeSinceStartup + 1f;
        rotationAngle = -.8f;
    }

    public void ToggleEnabled()
    {

    }

    #endregion

}

