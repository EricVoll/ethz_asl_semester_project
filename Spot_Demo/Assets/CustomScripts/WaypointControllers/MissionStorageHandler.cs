using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WaypointControl.Anchor;
using WaypointControl.Mission;
using WaypointControl.Waypoint;
using AzureCosmosDBClient;
using System;
using UnityEngine.Networking;

public class MissionStorageHandler : MonoBehaviour
{
    JsonSerializerSettings jsonSerializerSettings;

    [Header("Cosmos DB Information")]
    [Tooltip("the endpoint url including the port for your cosmos DB")]
    public string cosmosDBResourceEndPoint = @"https://mr-spot-control-mission-db-v2.documents.azure.com:443";
    [Tooltip("The master key (primary) for your cosmos DB")]
    public string cosmosDBMasterKey = "ha1e0ZI4thLnjgCY8HI1iJbQTxG7z7UKh7TXvxFe1HOVwkdhRQ1BfMQOOTjKbs4JgROOL2lLmbkSyeOa2XmzwA==";
    [Tooltip("The container name to use for the online mission storage")]
    public string cosmosContainerName = "missions";
    [Tooltip("The collection name for the online mission storage")]
    public string comsosCollectionName = "stored";


    MissionController controller;
    AzureCosmosClient dbClient;
    System.Random rnd;

    private bool isEnabled = true;
    // Start is called before the first frame update
    void Start()
    {
        jsonSerializerSettings = new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.Auto,
            ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
            PreserveReferencesHandling = PreserveReferencesHandling.Objects
        };

        controller = GetComponent<MissionController>();
        if (controller == null)
        {
            Debug.LogError("MissionController component must be on the same GameObject as MissionStorageHandler");
            isEnabled = false;
        }

        dbClient = new AzureCosmosClient(cosmosDBResourceEndPoint, cosmosDBMasterKey);
        dbClient.SetURLEscapeDelegate(UnityWebRequest.EscapeURL);
        rnd = new System.Random();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SaveMission()
    {
        if (!isEnabled)
        {
            Debug.Log("MissionStorageHandler is disabled.");
            return;
        }
        SaveMissionInternal(controller.mission);
    }

    public void LoadMission()
    {
        if (!isEnabled)
        {
            Debug.Log("MissionStorageHandler is disabled.");
            return;
        }
        StartCoroutine(controller.SetMission(LoadMissionInternal()));
    }



    /// <summary>
    /// Stores the mission as a json string in the player prefs
    /// </summary>
    private void SaveMissionInternal(Mission mission)
    {
        //mission.SpatialAnchors.Clear();
        //foreach (var wp in mission.Waypoints)
        //{
        //    wp.SetPose(wp.Pose, new SpatialAnchor("", new RobotUtilities.Pose(), ""));
        //}
        mission.Waypoints.ForEach(
        x =>
        {
            x.TargetSetTasks.RemoveAll(y => y is ColorChangeTask);
            x.EntryTasks.RemoveAll(y => y is ColorChangeTask);
            x.MainTasks.RemoveAll(y => y is ColorChangeTask);
            x.ExitTasks.RemoveAll(y => y is ColorChangeTask);
        });
        string json = JsonConvert.SerializeObject(mission, jsonSerializerSettings);
        PlayerPrefs.SetString("mission_save", json);
        PlayerPrefs.Save();

        SaveMissionToCosmosDB(json);
    }

    /// <summary>
    /// Loads the stored mission from the playerprefs
    /// </summary>
    /// <returns></returns>
    private Mission LoadMissionInternal()
    {
        string json = PlayerPrefs.GetString("mission_save");
        Mission mission = JsonConvert.DeserializeObject<Mission>(json, jsonSerializerSettings);
        mission.Waypoints.ForEach(
            x =>
            {
                x.TargetSetTasks.RemoveAll(y => y is ColorChangeTask);
                x.EntryTasks.RemoveAll(y => y is ColorChangeTask);
                x.MainTasks.RemoveAll(y => y is ColorChangeTask);
                x.ExitTasks.RemoveAll(y => y is ColorChangeTask);
            }
        );

        string newJson = JsonConvert.SerializeObject(mission, jsonSerializerSettings);
        return mission;
    }



    private async void SaveMissionToCosmosDB(string mission)
    {
        
        string id = DateTime.Today.ToString("dd.MM.yyy") + "_" + rnd.Next(0, 100).ToString().PadLeft(3, '0');
        var item = new
        {
            id = id,
            json = mission
        };

        var result = await dbClient.SendPostRequestAsync($"dbs/{cosmosContainerName}/colls/{comsosCollectionName}", "docs", JsonConvert.SerializeObject(item), true);

        if (result.StatusCode == System.Net.HttpStatusCode.Created)
        {
            Toolkit.singleton.TriggerEvent("message_box_service",
                   new MessageBoxContent(5, $"Mission upload:", $"Mission was uploaded to CosmosDB with id {id}"));
        }
        else
        {
            Toolkit.singleton.TriggerEvent("message_box_service",
                   new MessageBoxContent(5, $"Mission upload:", $"Mission failed to upload. Status code: {result.StatusCode}, description: {result.StatusDescription}"));
        }

        Debug.Log(result);

    }
}
