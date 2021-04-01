using Microsoft.MixedReality.Toolkit.SpatialAwareness;
using Microsoft.MixedReality.Toolkit.Utilities.GameObjectManagement;
using Newtonsoft.Json;
using RobotUtilities;
using RosSharp.RosBridgeClient.UrdfTransfer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using WaypointControl.Anchor;
using WaypointControl.Edge.Interfaces;
using WaypointControl.Mission;
using WaypointControl.Waypoint;
using WaypointControl.Waypoint.Interfaces;

public class MissionController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        mission = new Mission();

        mission.AddActiveAnchorChangedEventListener(ActiveAnchorChanged);

        InitMission();
    }


    #region Initialization

    /// <summary>
    /// Initializes the mission with an EntryElement. If none exists already, it creates one
    /// </summary>
    private void InitMission()
    {
        //Entry Element Configuration
        var EntryElement = this.GetComponentInChildren<WaypointController>();
        if (EntryElement == null)
        {
            //Create and add the root anchor
            mission.SpatialAnchors.Clear();

            SpatialAnchor RootAnchor = CreateAnchor(new RobotUtilities.Pose(), null).GetAnchor();

            EntryElement = CreateWpController(new RobotUtilities.Pose(), RootAnchor);
        }

        mission.SetEntryPoint(EntryElement.waypoint);

        //Add some debugging stuff
        var newWp1 = AddChild(EntryElement);
        var newWp2 = AddChild(newWp1);
    }


    #endregion

    internal Mission mission;


    #region Inspector fields


    [Header("Prefabs")]
    [SerializeField]
    private GameObject WaypointPrefab;
    [SerializeField]
    private GameObject LinePrefab;

    [Header("Scene References")]
    [SerializeField]
    private GameObject LineRendererContainer;
    [SerializeField]
    private GameObject SpatialAnchorPrefab;
    [SerializeField]
    private RobotHandler RobotHandler;
    [SerializeField]
    private GameObject SpatialAnchorsContainer;

    #endregion

    #region Unity Lifecycle


    // Update is called once per frame
    void Update()
    {
        if (Que.Count > 0)
            lock (Que)
            {
                foreach (var action in Que)
                {
                    action();
                }
                Que.Clear();
            }
    }

    #endregion

    #region Spatial Anchor Management

    private void ActiveAnchorChanged(object sender, SpatialAnchorChangedEventArgs e)
    {
        RobotHandler.SetCurrentAnchor(SpatialAnchorToControllerMap[e.NewAnchor]);
    }

    Dictionary<SpatialAnchor, SpatialAnchorController> SpatialAnchorToControllerMap =
        new Dictionary<SpatialAnchor, SpatialAnchorController>();

    /// <summary>
    /// Generates and creates an anchor and its GameObjects
    /// </summary>
    /// <param name="relativePose"></param>
    /// <param name="parentAnchor"></param>
    /// <returns></returns>
    public SpatialAnchorController CreateAnchor(RobotUtilities.Pose relativePose, SpatialAnchor parentAnchor)
    {
        SpatialAnchor newAnchor = new WaypointControl.Anchor.SpatialAnchor(null, relativePose, parentAnchor != null ? parentAnchor.Id : "root");
        mission.AddAnchor(newAnchor);
        var controller = CreateAnchorGameObject(newAnchor, parentAnchor);
        return controller;
    }

    /// <summary>
    /// Create an Anchors GameObjects
    /// </summary>
    /// <param name="newAnchor"></param>
    /// <param name="parentAnchor"></param>
    /// <returns></returns>
    private SpatialAnchorController CreateAnchorGameObject(SpatialAnchor newAnchor, SpatialAnchor parentAnchor = null)
    {
        GameObject anchorObject = Instantiate(SpatialAnchorPrefab, SpatialAnchorsContainer.transform);
        SpatialAnchorController controller = anchorObject.GetComponent<SpatialAnchorController>();

        mission.AddAnchor(newAnchor);
        controller.InitAsync(newAnchor);

        SpatialAnchorToControllerMap[newAnchor] = controller;

        if (parentAnchor == null)
            parentAnchor = mission.SpatialAnchors.FirstOrDefault(x => x.Id == newAnchor.ParentAnchorId);

        if (parentAnchor != null)
        {
            controller.SetPose(SpatialAnchorToControllerMap[parentAnchor]);
        }

        return controller;
    }

    /// <summary>
    /// Returns the nearest spatial anchor controller if it is within the definex max distance
    /// Otherwise returns null
    /// </summary>
    /// <param name="transform"></param>
    /// <param name="maxDistance"></param>
    /// <returns></returns>
    public (SpatialAnchorController, bool) FindAnchorNearMe(Transform transform, float maxDistance)
    {
        var closest = SpatialAnchorToControllerMap.Values.Aggregate((curMin, x) => (curMin == null || (x.transform.position - transform.position).magnitude < (curMin.transform.position - transform.position).magnitude ? x : curMin));
        return (closest, closest == null ? false : (closest.transform.position - transform.position).Magnitude2D() < maxDistance);
    }

    /// <summary>
    /// Returns the spatial anchor controller which managed the specified anchor id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public SpatialAnchorController GetAnchorControllerById(string id)
    {
        var key = SpatialAnchorToControllerMap.Keys.FirstOrDefault(x => x.Id == id);
        if (key != null) return SpatialAnchorToControllerMap[key];

#if UNITY_EDITOR
        else return SpatialAnchorToControllerMap.Values.First();
#else
        else return null;
#endif
        }

#endregion

    #region Mission Editing

    public List<WaypointController> GetAllWaypiontControllers()
    {
        return SpatialAnchorToControllerMap.Values.SelectMany(x => x.GetAllWaypointControllers()).ToList();
    }

    private WaypointController CreateWpController(IPose parentPose, SpatialAnchor spatialAnchor)
    {
        GameObject newWayPoint = GameObject.Instantiate(WaypointPrefab);
        WaypointController newWp = newWayPoint.GetComponent<WaypointController>();
        newWp.Init(new Waypoint(parentPose.Translate<RobotUtilities.Poses.PositionOnly2DPose>(new Vector3(0, 0, .4f)), spatialAnchor), this);
        newWp.waypoint.SetBranchDecisionTask(new BranchingDecisionTask(GetAllWaypiontControllers));
        SpatialAnchorToControllerMap[spatialAnchor].SetAnchorAsParent(newWayPoint);
        return newWp;
    }

    private IEdge GetEdge(IWaypoint source, IWaypoint target)
    {
        return mission.Edges.First(x => x.Source == source && x.Target == target);
    }

    /// <summary>
    /// Adds a child to the specified controller
    /// </summary>
    /// <param name="wp"></param>
    /// <returns></returns>
    public WaypointController AddChild(WaypointController wp)
    {
        WaypointController newWp = CreateWpController(wp.waypoint.Pose, wp.waypoint.Anchor);
        mission.AddWaypointAsChild(wp.waypoint, newWp.waypoint);
        AddLineRendererBetween(wp.gameObject, newWp.gameObject, GetEdge(wp.waypoint, newWp.waypoint));
        return newWp;
    }

    /// <summary>
    /// Adds a line renderer between the two gameobjects, which updates its lines
    /// </summary>
    /// <param name="wp1"></param>
    /// <param name="wp2"></param>
    private void AddLineRendererBetween(GameObject wp1, GameObject wp2, IEdge edge)
    {
        GameObject line = GameObject.Instantiate(LinePrefab, LineRendererContainer.transform, false);
        var comp = line.GetComponent<WaypointLine>();
        comp.SetTargets(new Transform[] { wp1.transform, wp2.transform });
        comp.Init(this, edge);
    }

    /// <summary>
    /// Inserst a Child in the sequence
    /// </summary>
    /// <param name="parentWpController"></param>
    /// <returns></returns>
    public WaypointController InsertChild(WaypointController parentWpController)
    {
        WaypointController newWp = CreateWpController(parentWpController.waypoint.Pose, parentWpController.waypoint.Anchor);
        mission.InsertWaypointAsChild(parentWpController.waypoint, newWp.waypoint);
        StartCoroutine(RecreateAllLines(mission));
        return newWp;
    }

    /// <summary>
    /// Inserts the waypoint as a new entry point
    /// </summary>
    /// <param name="currentEntryWpController"></param>
    /// <returns></returns>
    public WaypointController InsertNewEntryPoint(WaypointController currentEntryWpController)
    {
        WaypointController newWp = CreateWpController(currentEntryWpController.waypoint.Pose, currentEntryWpController.waypoint.Anchor);
        mission.InsertNewEntryWaypoint(newWp.waypoint);
        AddLineRendererBetween(currentEntryWpController.gameObject, newWp.gameObject, GetEdge(newWp.waypoint, currentEntryWpController.waypoint));
        return newWp;
    }

    /// <summary>
    /// Removes the waypoint
    /// </summary>
    /// <param name="wp"></param>
    /// <returns></returns>
    public IEnumerator RemoveWaypoint(WaypointController wp)
    {
        DeleteChildren(LineRendererContainer.transform);
        yield return new WaitForSeconds(0.001f);
        mission.RemoveWaypoint(wp.waypoint);
        DeleteChildren(wp.transform);
        wp.isDestroyRequested = true;
        yield return RecreateAllLines(mission);
        GameObject.Destroy(wp);
    }

    /// <summary>
    /// Deletes all line renderers and re-initializes them
    /// </summary>
    /// <returns></returns>
    internal IEnumerator RecreateAllLines(Mission m)
    {
        DeleteChildren(LineRendererContainer.transform);
        yield return new WaitForSeconds(.1f);
        //Recreate all lines
        var newWaypoints = SpatialAnchorToControllerMap.Values.SelectMany(x => x.GetAllWaypointControllers());
        var edges = m.Edges;

        foreach (var edge in edges)
        {
            GameObject source = newWaypoints.First(x => x.waypoint.LifetimeId == edge.Source.LifetimeId).gameObject;
            GameObject target = newWaypoints.First(x => x.waypoint.LifetimeId == edge.Target.LifetimeId).gameObject;
            AddLineRendererBetween(source, target, edge);
        }
    }


    /// <summary>
    /// Adds the edge between the specified waypoints
    /// </summary>
    /// <param name="source"></param>
    /// <param name="target"></param>
    internal IEnumerator AddEdge(IWaypoint source, IWaypoint target)
    {
        mission.AddEdge(source, target);
        yield return RecreateAllLines(mission);
    }

    /// <summary>
    /// Removes the Edge
    /// </summary>
    /// <param name="edge"></param>
    public void DeleteEdge(IEdge edge)
    {
        mission.RemoveEdge(edge);
        StartCoroutine(RecreateAllLines(mission));
    }

    /// <summary>
    /// Destroy all existing waypoints and creates new one corresponding to the mission passed
    /// </summary>
    /// <param name="m"></param>
    /// <returns></returns>
    private IEnumerator RecreateAllWaypoints(Mission m)
    {
        //Wait for a short duration until the object are actually deleted
        yield return new WaitForSeconds(0.1f);

        //Recreate all waypoints
        List<IWaypoint> waypoints = m.FindAllWaypoints();
        foreach (var waypoint in waypoints)
        {
            GameObject newWayPoint = GameObject.Instantiate(WaypointPrefab);
            SpatialAnchorToControllerMap[waypoint.Anchor].SetAnchorAsParent(newWayPoint);
            WaypointController newWp = newWayPoint.GetComponent<WaypointController>();
            newWp.Init(waypoint, this, addUXTasks: true);
            mission.AddWaypoint(newWp.waypoint);
        }
    }

    /// <summary>
    /// Recreates all spatial anchor gameObjects
    /// </summary>
    /// <param name="mission"></param>
    /// <returns></returns>
    private IEnumerator RecreateAllSpatialAnchors(Mission mission)
    {
        //clean house
        SpatialAnchorToControllerMap = new Dictionary<SpatialAnchor, SpatialAnchorController>();
        DeleteChildren(SpatialAnchorsContainer.transform);
        DeleteChildren(LineRendererContainer.transform);

        yield return new WaitForSeconds(.1f);

        foreach (var anchor in mission.SpatialAnchors)
        {
            CreateAnchorGameObject(anchor);
        }
    }

    /// <summary>
    /// Overwrites all current objects with the given mission
    /// </summary>
    /// <param name="m"></param>
    internal IEnumerator SetMission(Mission m)
    {
        //give reference to the missionController
        mission = m;

        yield return RecreateAllSpatialAnchors(m);
        yield return RecreateAllWaypoints(m);
        yield return RecreateAllLines(m);

        mission.AddActiveAnchorChangedEventListener(ActiveAnchorChanged);
    }

    /// <summary>
    /// Deletes all child transforms of this transform
    /// </summary>
    /// <param name="t"></param>
    private void DeleteChildren(Transform t)
    {
        var transforms = new HashSet<Transform>(t.GetComponentsInChildren<Transform>());
        transforms.Remove(t);
        var children = transforms.ToArray();

        for (int i = children.Length - 1; i >= 0; i--)
        {
            GameObject.Destroy(children[i].gameObject);
        }
    }

#endregion

    #region UpdateThreadExecution
    List<Action> Que = new List<Action>();
    public void ExecuteOnUpdate(Action action)
    {
        lock (Que)
        {
            Que.Add(action);
        }
    }
#endregion

    #region Mission Execution

    public void StartMission()
    {
        mission.Start(RobotHandler.Robot);
    }

#endregion
}
