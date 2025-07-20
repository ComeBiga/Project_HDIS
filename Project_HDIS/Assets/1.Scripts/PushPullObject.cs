using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PushPullObject : MonoBehaviour
{
    public float PushPullSpeed => _pushPullSpeed;

    [SerializeField] private float _pushPullSpeed = 1f;

    private Rigidbody mRigidbody;

    public void PushPull(Vector3 velocity)
    {
        mRigidbody.velocity = velocity;
    }

    // Start is called before the first frame update
    void Start()
    {
        mRigidbody = GetComponent<Rigidbody>();
    }
}
