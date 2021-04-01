using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectionStateService : MonoBehaviour, IServiceOfferer<IServiceMessage>
{
    [Tooltip("Notifies the user of connection state changes using the msg box service.")]
    public bool NotifyUserWithMsgBox = true;

    public int Priority => 1;

    public bool SendMessageToNewSubscribers => true;

    public string GetServiceName() => nameof(ConnectionStateService);

    public bool HasMessage() => gotUpdate;

    /// <summary>
    /// Nothing to reset here.
    /// </summary>
    public void ReportMessageBroadcasted()
    {

    }

    /// <summary>
    /// Returns the current state of the connection (true -> connection is up)
    /// </summary>
    /// <returns></returns>
    public IServiceMessage RetrieveServiceItem()
    {
        gotUpdate = false;
        return new ConnectionStateMessage() { ConnectionState = currentState };
    }

    private bool currentState = false;
    private bool gotUpdate = false;

    /// <summary>
    /// Sets the current state of the RosBridgeConnection and will broadcast it via its service in the next update
    /// </summary>
    /// <param name="state"></param>
    public void SetCurrentState(bool state)
    {
        currentState = state;
        gotUpdate = true;

        if (NotifyUserWithMsgBox)
        {
            if (currentState)
            {
                Toolkit.singleton.TriggerEvent("message_box_service",
                    new MessageBoxContent(3, $"Spot Update", $"Spot is now connected."));
            }
            else
            {
                Toolkit.singleton.TriggerEvent("message_box_service",
                    new MessageBoxContent(3, $"Spot Update", $"Lost connection to spot."));
            }
        }
    }
}

/// <summary>
/// Simple messgae object for the ConnectionStateService
/// </summary>
public class ConnectionStateMessage : IServiceMessage
{
    public bool ConnectionState { get; set; }
}