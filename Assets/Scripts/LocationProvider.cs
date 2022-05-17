using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Android;


public class LocationProvider : MonoBehaviour
{
    [SerializeField]
    public Text Status;



    // Start is called before the first frame update
    IEnumerator Start()
    {
        Status.text = "Init";
        Permission.RequestUserPermission(Permission.FineLocation);
        // Check if the user has location service enabled.
        if (!Input.location.isEnabledByUser)

            yield break;

        // Starts the location service.
        Input.location.Start();

        // Waits until the location service initializes
        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        // If the service didn't initialize in 20 seconds this cancels location service use.
        if (maxWait < 1)
        {
            Status.text = "Timed out";
            yield break;
        }

        // If the connection failed this cancels location service use.
        if (Input.location.status == LocationServiceStatus.Failed)
        {
            Status.text =  "Unable to determine device location";
            yield break;
        }
        else
        {
            while (true)
            {
                Status.text = $" Location: {Input.location.lastData.latitude} {Input.location.lastData.longitude}  {Input.location.lastData.altitude}  {Input.location.lastData.horizontalAccuracy}  {Input.location.lastData.timestamp} ";
                yield return null;
            }
            
           
        }

        // Stops the location service if there is no need to query location updates continuously.
        // Input.location.Stop();
    }
    // Update is called once per frame
    void Update()
    {

    }
}
