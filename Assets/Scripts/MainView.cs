using Assets.Scripts.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class MainView : MonoBehaviour
{
    [SerializeField]
    public Text ScaleLabel;
    private LocationProvider locationProvider;
    private RadarScript radarScript;
    private Button PlusButton;
    private Button MinusButton;

    private List<POI> pois = new List<POI>
        {
            new POI() {Id = 1, Name = "Doner House", Desctiption = "?Дачная, 28/1? Николаевка, Красноярск, 660074 " , Coordinates =new Coordinates ( 55.9966905f, 92.8002853f), Rating = 3.5M},
            new POI() {Id = 2, Name = "Шаверма", Desctiption =  "?Академика Киренского, 32/2? Николаевка, Красноярск, 660074", Coordinates = new Coordinates(55.99793f,92.79766f), Rating = 4.5M },
            new POI() {Id = 3, Name = "Fire food", Desctiption = "?Академика Киренского, 17а ст1?, Красноярск, 660074", Coordinates = new Coordinates(55.998068f, 92.795778f), Rating = 2M},
            new POI() {Id = 4, Name = "Fire food", Desctiption = "?улица Борисова, 14с7 Красноярск, 660074", Coordinates = new Coordinates(55.993610f, 92.790633f), Rating = 2M },
            new POI() {Id = 5, Name = "Doner House", Desctiption = "Свободный проспект, 81Г/1", Coordinates = new Coordinates(56.003064f, 92.776260f), Rating = 4.1M}
        };

    // Start is called before the first frame update
    void Start()
    {
        locationProvider = gameObject.GetComponent<LocationProvider>();
        radarScript = gameObject.GetComponent<RadarScript>();

        PlusButton = GameObject.Find("PlusButton").GetComponent<Button>();
        MinusButton = GameObject.Find("MinusButton").GetComponent<Button>();

        PlusButton.onClick.AddListener(() => onPlusScale());
        MinusButton.onClick.AddListener(() => onMinusScale());

        locationProvider.OnNewLocationRecieved.AddListener(radarScript.OnCoordsUpdate);

        radarScript.Points = pois;
        StartCoroutine(locationProvider.LocationSubRoutine());
        StartCoroutine(radarScript.StartCompass());

        //radarScript.CurrentCoordinate = new Geolocation.Coordinate(55.994431, 92.797389);
        radarScript.CalcDistance();
        radarScript.ShowDots();

    }

    private void onMinusScale()
    {
        UpdateScale(1);
    }

    private void onPlusScale()
    {
        UpdateScale(-1);
    }

    float[] Scales = new float[] { 100f, 200f, 500f, 1000f, 3000f, 5000f };
    int currentScaleStep = 0;
    private void UpdateScale(int dir)
    {
        if (dir > 0)
        {
            if (currentScaleStep + dir > 5)
            {
                currentScaleStep = 5;
                return;
            }
        }
        else if (dir < 0)
        {
            if ((currentScaleStep + dir) < 0)
            {
                currentScaleStep = 0;
                return;
            }

        }
        currentScaleStep += dir;
        radarScript.UpdateScale(Scales[currentScaleStep]);
        ScaleLabel.text = $"Scale: {(int)Scales[currentScaleStep]} m ";
    }

    // Update is called once per frame
    void Update()
    {

    }
}
