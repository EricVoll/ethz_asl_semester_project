using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using WaypointControl.Anchor;

public class SpatialAnchorController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    async void Awake()
    {

    }


    bool startAnchorCreationOnNextUpdate = false;
    bool shouldStartLookingForAnchor = false;

    [SerializeField]
    GameObject AnchorFoundIndicator;

    // Update is called once per frame
    async void Update()
    {
        if (startAnchorCreationOnNextUpdate && Anchor != null && Anchor.Id == null && ASAController.asaController.IsInitialized && ASAController.asaController.IsFreeToUse)
        {
            Debug.Log("Started to create anchor");
            startAnchorCreationOnNextUpdate = false;
            Anchor.Id = await ASAController.asaController.CreateAnchor(this.gameObject);
            AsaReporter.instance.ReportAsaAnchorCreated(Anchor.Id, this.transform.position, this.transform.rotation, AnchorFoundByAsaRosWrapper);
        }
        else if (shouldStartLookingForAnchor && ASAController.asaController.IsInitialized && ASAController.asaController.IsFreeToUse)
        {
            Debug.Log("Looking for anchor " + Anchor.Id);
            shouldStartLookingForAnchor = false;
            ASAController.asaController.FindAnchor(this.gameObject, Anchor.Id);
            AsaReporter.instance.ReportAsaAnchorFound(Anchor.Id, AnchorFoundByAsaRosWrapper);
        }
    }

    SpatialAnchor Anchor { get; set; }

    [SerializeField]
    GameObject WaypointContainer;

    public void InitAsync(SpatialAnchor anchor)
    {
        Anchor = anchor;

        if (anchor.Id == null)
        {
            //create anchor and set id
            startAnchorCreationOnNextUpdate = true;
        }
        else
        {
            //find anchor from id
            shouldStartLookingForAnchor = true;
        }
    }

    public void SetPose(SpatialAnchorController parentController)
    {
        this.transform.localPosition = parentController.transform.localPosition + Anchor.RelativePoseToParent.ToPositionVector();
        this.transform.localRotation = parentController.transform.localRotation * Anchor.RelativePoseToParent.ToRotationQuaternion();
    }

    public void SetAnchorAsParent(GameObject go, bool worldPosStays = false)
    {
        go.transform.SetParent(WaypointContainer.transform, worldPosStays);
    }

    public List<WaypointController> GetAllWaypointControllers()
    {
        return WaypointContainer.GetComponentsInChildren<WaypointController>().Where(x => !x.isDestroyRequested).ToList();
    }

    public SpatialAnchor GetAnchor()
    {
        return Anchor;
    }

    private void AnchorFoundByAsaRosWrapper()
    {
        if (AnchorFoundIndicator != null)
        {
            AnchorFoundIndicator.SetActive(true);
        }
        else
        {
            Debug.Log("AnchorFoundIndicator was null...");
        }
    }
}
