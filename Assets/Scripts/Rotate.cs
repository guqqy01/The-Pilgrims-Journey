using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotate : MonoBehaviour
{
    [SerializeField]
    private float rotationSpeed = 1f;

    [SerializeField]
    private bool useUpdateGap = false;

    private float currentZRotation = 0f;

    private float currentTimeGap;
    [SerializeField]
    private float updateStagger = 0.3f;


    // Update is called once per frame
    void Update()
    {
        if (!useUpdateGap)
        {
            transform.rotation = Quaternion.Euler(0, 0, rotationSpeed * Time.time);
            return;
        }

        currentZRotation = rotationSpeed * Time.time;

        currentTimeGap += Time.deltaTime;

        if(currentTimeGap >= updateStagger)
        {
            transform.rotation = Quaternion.Euler(0, 0, currentZRotation);

            currentTimeGap = currentTimeGap - updateStagger;
        }
    }
}
