using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientAgent : MonoBehaviour
{
    private float lerpTime;
    private Vector3 oldPos;
    private Vector3 newPos;

    void OnEnable()
    {
        oldPos = transform.position;
        newPos = transform.position;
    }

    void Update()
    {
        transform.position = Vector3.Lerp(oldPos, newPos, lerpTime);
        lerpTime += Time.deltaTime / NetworkConfig.Inst.FixedDeltaTimeRate;
        lerpTime = Mathf.Clamp(lerpTime, 0.0f, 1.0f);
    }

    public void UpdateNewPosition(Vector3 _newPos)
    {
        oldPos = transform.position;
        newPos = _newPos;
        lerpTime = 0.0f;
    }
}
