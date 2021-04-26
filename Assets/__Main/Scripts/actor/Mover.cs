using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class Mover : MonoBehaviour
{
    [SerializeField]
    private float moveSpeed;

    const float distX = 5.0f;
    private float sign = 1.0f;
    private float origX;

    void OnEnable()
    {
#if UNITY_EDITOR
        if (NetworkConfig.Inst.IsEditorClientOnlyNetwork)
        {
            Destroy(this);
            return;
        }
#else
        if (SystemInfo.graphicsDeviceType != GraphicsDeviceType.Null)
        {
            Destroy(this);
            return;
        }
#endif

        origX = transform.position.x;
    }

    void Update()
    {
        if (transform.position.x > origX + distX)
        {
            sign = -1.0f;
        }
        else if (transform.position.x < origX - distX)
        {
            sign = 1.0f;
        }

        var newPos = transform.position;
        newPos.x += moveSpeed * sign * Time.deltaTime;

        transform.position = newPos;
    }
}
