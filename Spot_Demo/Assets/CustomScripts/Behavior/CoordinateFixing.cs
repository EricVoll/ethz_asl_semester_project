using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoordinateFixing : MonoBehaviour, IServiceConsumer<IServiceMessage>
{
    /// <summary>
    /// If true, then the attached gameObject will be fixed to y = 0;
    /// </summary>
    [SerializeField]
    private bool yFixingEnabled = true;

    /// <summary>
    /// The y level used to fix this target
    /// </summary>
    [SerializeField]
    public float yFixingLevel { get; private set; }

    // Start is called before the first frame update
    void Start()
    {
        Toolkit.singleton.RegisterServiceConsumer(this, "asa_y_level_publisher");
    }

    // Update is called once per frame
    void Update()
    {
        if (yFixingEnabled && !inSmoothingProcess && this.transform.position.y != 0)
        {
            Vector3 v = transform.localPosition;
            transform.localPosition = new Vector3(v.x, yFixingLevel, v.z);
        }
        else if (inSmoothingProcess)
        {
            Vector3 v = transform.localPosition;
            Vector3 target = new Vector3(v.x, yFixingLevel, v.z);
            this.transform.localPosition = v + (target - v) * (Time.realtimeSinceStartup - smoothingTimeStampStart) / 3;
            if ((target - v).magnitude < .001)
            {
                inSmoothingProcess = false;
            }
        }
    }

    /// <summary>
    /// Flag indicating whether or not this component is currently smoothing into its fixed y position 
    /// after an animation.
    /// </summary>
    private bool inSmoothingProcess = false;
    /// <summary>
    /// a timestamp stating when the smoothing process started. Used for smoothing-linear-interpolation.
    /// </summary>
    private float smoothingTimeStampStart;

    /// <summary>
    /// Disables the y fixing.
    /// </summary>
    public void DisableYFixing()
    {
        yFixingEnabled = false;
    }

    /// <summary>
    /// Enables the y-Fixing and starts the smoothing animation back to its root-position.
    /// </summary>
    public void EnableYFixing()
    {
        yFixingEnabled = true;
        inSmoothingProcess = true;
        smoothingTimeStampStart = Time.realtimeSinceStartup;
    }

    public void ConsumeServiceItem(IServiceMessage item, string serviceName)
    {
        if (serviceName == "asa_y_level_publisher")
        {
            yFixingLevel = ((Vector3ServiceMessage)item).Vector.y;
            EnableYFixing();
        }
    }
}
