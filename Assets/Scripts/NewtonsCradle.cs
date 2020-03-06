using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PBD;

[RequireComponent(typeof(World))]
public class PBDNewtonsCradle : MonoBehaviour
{
    [SerializeField] private GameObject ballPrefab;

    private World world;



    private void Start()
    {
        world = GetComponent<World>();
    }

    private void FixedUpdate()
    {
    }
}