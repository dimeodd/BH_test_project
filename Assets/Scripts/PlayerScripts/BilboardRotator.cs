using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BilboardRotator
{
    Transform _tf;
    Transform _camTf;

    public BilboardRotator(Transform target, Transform camera)
    {
        _tf = target;
        _camTf = camera;
    }

    // Update is called once per frame
    public void Update()
    {
        var quat = _camTf.rotation;
        _tf.transform.rotation = quat;
    }
}
