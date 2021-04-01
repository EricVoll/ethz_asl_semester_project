using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AsaReporter : MonoBehaviour, IServiceConsumer<IServiceMessage>
{
    AsaUtilities asaUtils;

    public static AsaReporter instance;

    //Maps anchor_id to an action (callback) as soon as the AsaUtilities reports that the anchor
    //has been found by the asa_ros wrapper
    Dictionary<string, Action> anchorFoundCallBacks = new Dictionary<string, Action>();

    /// <summary>
    /// A flag that is true if a request is currently being processed. A reuqest is finished as soon as a callback is executed
    /// which reports that the anchor was found by the asa ros client.
    /// </summary>
    private bool isBusyQueryingAnchor = false;

    /// <summary>
    /// this que is used to buffer requests which come in while another request is running. The asa ros client can only find one anchor at a time.
    /// If multiple request are sent at the same time, it overwrites old requests. This is why we have to do that in a sequence here.
    /// </summary>
    List<Action> FindAnchorQue { get; set; } = new List<Action>();

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        Toolkit.singleton.RegisterServiceConsumer(this, "ConnectionStateService");
    }



    /// <summary>
    /// A buffer list of actions used when the connection to the rosbridge is not yet established
    /// </summary>
    List<Action> buffer = new List<Action>();

    /// <summary>
    /// Reports to the ROS side that the UnityApp created an anchor and that the ROS side should look
    /// for the anchor as well
    /// </summary>
    /// <param name="anchorId"></param>
    /// <param name="position"></param>
    /// <param name="orientation"></param>
    /// <param name="anchorFoundByAsaRosCallBack"></param>
    public void ReportAsaAnchorCreated(string anchorId, Vector3 position, Quaternion orientation, Action anchorFoundByAsaRosCallBack)
    {
#if !UNITY_EDITOR
        if (asaUtils == null)
        {
            Debug.Log("AsaUtils in AsaReporter is null. Buffering request.");
            //this might result in an endless loop or at least in double executions, but that shouldn't be an issue,
            //since the ros side can handle the same request multiple times. No problem.
            buffer.Add(() => ReportAsaAnchorCreated(anchorId, position, orientation, anchorFoundByAsaRosCallBack));
            return;
        }
#endif

        if (!isBusyQueryingAnchor)
        {
            isBusyQueryingAnchor = true;
            anchorFoundCallBacks[anchorId] = anchorFoundByAsaRosCallBack;
#if UNITY_EDITOR
            position = RosSharp.TransformExtensions.Unity2Ros(position);
            Debug.Log("Reporting Anchor find anchor bc created (mocked)!");
            asaUtils?.MockCreateAnchorAt(position.x, position.y, position.z, anchorId);
            StartCoroutine(MockCallBackAfterSeconds(anchorId, 5));
#else
            //Tell asa ros wrapper to find the anchor
            Debug.Log("Reporting Anchor find anchor bc created (real)!");
            asaUtils.AsaRosWrapperFindAnchorServiceCall(anchorId);
#endif
        }
        else
        {
            Debug.Log("AsaReporter is busy. Buffering request.");
            FindAnchorQue.Add(() => ReportAsaAnchorCreated(anchorId, position, orientation, anchorFoundByAsaRosCallBack));
        }

    }

    /// <summary>
    /// A method which reports to the ROS Side that an anchor was found from the UnityApp and that
    /// the ros side should look for the anchor as well
    /// </summary>
    /// <param name="anchorId"></param>
    /// <param name="anchorFoundByAsaRosCallBack"></param>
    public void ReportAsaAnchorFound(string anchorId, Action anchorFoundByAsaRosCallBack)
    {
#if !UNITY_EDITOR
        if (asaUtils == null)
        {
            Debug.Log("AsaUtils in AsaReporter is null. Buffering request.");
            buffer.Add(() => ReportAsaAnchorFound(anchorId, anchorFoundByAsaRosCallBack));
            return;
        }
#endif

        anchorFoundCallBacks[anchorId] = anchorFoundByAsaRosCallBack;

        if (!isBusyQueryingAnchor)
        {
            isBusyQueryingAnchor = true;
#if UNITY_EDITOR
            Debug.Log("Reporting to find anchor (mocked)!");
            asaUtils?.MockFindAnchorAt(0, 0, 0, anchorId);
            StartCoroutine(MockCallBackAfterSeconds(anchorId, 5));
#else
            //Tell asa ros wrapper to find the anchor
            Debug.Log("Reporting to find anchor (real)!");
            asaUtils.AsaRosWrapperFindAnchorServiceCall(anchorId);
#endif
        }
        else
        {
            Debug.Log("AsaReporter is busy. Buffering request.");
            FindAnchorQue.Add(() => ReportAsaAnchorFound(anchorId, anchorFoundByAsaRosCallBack));
        }
    }

    public void ConsumeServiceItem(IServiceMessage item, string serviceName)
    {
        //this service item is called as soon as the rosConnector connects or disconnects to the RosBridge
        bool newRosConnectionState = ((ConnectionStateMessage)item).ConnectionState;
        if (newRosConnectionState)
        {
            var socket = this.GetComponent<RosSharp.RosBridgeClient.RosConnector>().RosSocket;
            asaUtils = new AsaUtilities(socket);
            asaUtils.foundAnchorEvent += FoundAnchorEventHandler;

            //Process all buffered items which were created before the rosConnector connected
            foreach (var bufferedItem in buffer)
            {
                bufferedItem();
                Debug.Log("Executed item from buffer");
            }
            buffer.Clear();
        }
        else
        {
            asaUtils = null;
        }
    }

#if UNITY_EDITOR
    /// <summary>
    /// Mocks the anchor callback after the given amount of time to test the App integration while in the Editormode
    /// </summary>
    /// <param name="anchor_id"></param>
    /// <param name="seconds"></param>
    /// <returns></returns>
    private IEnumerator MockCallBackAfterSeconds(string anchor_id, int seconds)
    {
        yield return new WaitForSeconds(seconds);

        FoundAnchorEventHandler(null, new AnchorFoundEventArgs(new RosSharp.RosBridgeClient.MessageTypes.AsaRos.FoundAnchor() { anchor_id = anchor_id }));
    }
#endif

    private void FoundAnchorEventHandler(object sender, AnchorFoundEventArgs e)
    {
        Debug.Log($"Found anchor with id {e.FoundAnchor.anchor_id}");

        //find the found anchor and invoke its callback, if it is set.
        //This check might be use-less, since this instance always instructs the asa_ros wrapper
        //to find the anchors, and therefore has the id registered already, but we never know what happens
        //in the future, or if the asa_ros wrapper finds an anchor commanded by another instance.
        if (anchorFoundCallBacks.ContainsKey(e.FoundAnchor.anchor_id))
        {
            ExecuteOnMainThread(anchorFoundCallBacks[e.FoundAnchor.anchor_id]);
        }
        else
            Debug.Log($"Found an anchor with id {e.FoundAnchor.anchor_id} which was not created or initially found by me.");

        //Process the next item in the anchor finding que
        if (FindAnchorQue.Count > 0)
        {
            ExecuteOnMainThread(() => {
                Debug.Log($"Executing the next query from the buffer list.");
                //get the first item of the list and remove it from the list to avoid infinite loops if the communication is super fast
                var queItemFindRequest = FindAnchorQue.First();
                FindAnchorQue.RemoveAt(0);
                //Release the flag to enable a new query
                isBusyQueryingAnchor = false;
                //find the anchor
                queItemFindRequest();
            });
        }
        else
        {
            //Release the flag such that the next query is executed instantly
            isBusyQueryingAnchor = false;
        }
    }


#region mainThread
    private void ExecuteOnMainThread(Action action)
    {
        actionsToExecuteOnMainThread.Insert(0, action);
    }

    List<Action> actionsToExecuteOnMainThread = new List<Action>();
    public void Update()
    {
        lock (actionsToExecuteOnMainThread)
        {
            if (actionsToExecuteOnMainThread.Count > 0)
            {
                for (int i = actionsToExecuteOnMainThread.Count - 1; i >= 0; i--)
                {
                    actionsToExecuteOnMainThread[i]();
                }
            }
            actionsToExecuteOnMainThread.Clear();
        }
    }

#endregion


}
