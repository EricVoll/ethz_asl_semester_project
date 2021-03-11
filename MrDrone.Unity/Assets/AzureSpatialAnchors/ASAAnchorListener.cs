using RosSharp;
using RosSharp.RosBridgeClient;
using RosSharp.RosBridgeClient.MessageTypes.AsaRos;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ASAAnchorListener : MonoBehaviour
{

    private RosSocket Socket;
    public string AsaRosTopicNamespace = "asa_ros";
    public GameObject AnchorIndicatorPrefab;
    public RosConnector RosConnector;

    private UIThreadHandler uiThreadDispatcher = new UIThreadHandler();

    // Start is called before the first frame update
    void Start()
    {
        Socket = RosConnector.RosSocket;
        if (RosConnector.IsConnected.WaitOne(0))
        {
            InitializeListener();
        }
        else
            Socket.protocol.OnConnected += OnRosSocketConnected;
    }

    private void OnRosSocketConnected(object sender, System.EventArgs e)
    {
        InitializeListener();
    }

    // Update is called once per frame
    void Update()
    {
        uiThreadDispatcher.ReportUpdate();
    }

    /// <summary>
    /// Creates a GameObject indicating the anchor's position
    /// </summary>
    /// <param name="anchorId"></param>
    /// <param name="position"></param>
    /// <param name="rotation"></param>
    public void ReportAnchorFound(string anchorId, Vector3 position, Quaternion rotation)
    {
        GameObject instance;

        if (AnchorIndicatorPrefab == null)
        {
            instance = GameObject.CreatePrimitive(PrimitiveType.Cube);
        }
        else
            instance = GameObject.Instantiate(AnchorIndicatorPrefab, this.transform);


        instance.transform.position = position;
        instance.transform.rotation = rotation;
        instance.transform.localScale = new Vector3(.05f, .05f, .05f);

        instance.GetComponent<Renderer>().material.SetColor("_Color", Color.red);
    }

    private void InitializeListener()
    {
        SubscriptionHandler<FoundAnchor> subscriptionHandler = new SubscriptionHandler<FoundAnchor>((msg) =>
        uiThreadDispatcher.ExecuteOnMainThread(() =>
            ReportAnchorFound(msg.anchor_id, msg.anchor_in_world_frame.transform.translation.ToUnity(), msg.anchor_in_world_frame.transform.rotation.ToUnity())
            ));
        Socket.Subscribe<FoundAnchor>($"{AsaRosTopicNamespace}/found_anchor", subscriptionHandler, 800);
    }
}
