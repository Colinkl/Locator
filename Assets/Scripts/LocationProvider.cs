using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Android;
using UnityEngine.Events;

public class LocationProvider : MonoBehaviour
{
    [SerializeField]
    public Text Status;
    [SerializeField]
    public Text Accuracy;
    public UnityEvent<LocationInfo> OnNewLocationRecieved;

    private LocationService locationService;

    private LocationInfo lastLocationInfo;

    public IEnumerator LocationSubRoutine()
    {
        
        Status.text = "Init";
        Permission.RequestUserPermission(Permission.FineLocation);
        // Check if the user has location service enabled.
        if (!Input.location.isEnabledByUser)

            yield break;

        // Starts the location service.

        locationService = Input.location;


        locationService.Start();

        // Waits until the location service initializes
        int maxWait = 20;
        while (locationService.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            maxWait--;
            yield return new WaitForSeconds(1);
        }

        // If the service didn't initialize in 20 seconds this cancels location service use.
        if (maxWait < 1)
        {
            Status.text = "Timed out";
            yield break;
        }

        // If the connection failed this cancels location service use.
        if (locationService.status == LocationServiceStatus.Failed)
        {
            Status.text = "Unable to determine device location";
            yield break;
        }
        else
        {
            while (true)
            {
               
                if (lastLocationInfo.timestamp != locationService.lastData.timestamp)
                {
                    Status.text = $" Location: {Input.location.lastData.latitude} {Input.location.lastData.longitude} ";
                    Accuracy.text = $"Accuracy: {(int)Input.location.lastData.horizontalAccuracy} m";
                    lastLocationInfo = locationService.lastData;
                    OnNewLocationRecieved?.Invoke(lastLocationInfo);
                }
                yield return null;
            }

        }

        // Stops the location service if there is no need to query location updates continuously.
        // Input.location.Stop();
    }
    // Update is called once per frame

}
