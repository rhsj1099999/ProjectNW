using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestMove : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(_keyCode_YUp) == true) 
        {
            transform.position += transform.up * _testMoveSpeed * Time.deltaTime;
        }
        if (Input.GetKey(_keyCode_YDown) == true)
        {
            transform.position -= transform.up * _testMoveSpeed * Time.deltaTime;
        }
        if (Input.GetKey(_keyCode_XUp) == true)
        {
            transform.position += transform.right * _testMoveSpeed * Time.deltaTime;
        }
        if (Input.GetKey(_keyCode_XDown) == true)
        {
            transform.position -= transform.right * _testMoveSpeed * Time.deltaTime;
        }
        if (Input.GetKey(_keyCode_ZUp) == true)
        {
            transform.position += transform.forward * _testMoveSpeed * Time.deltaTime;
        }
        if (Input.GetKey(_keyCode_ZDown) == true)
        {
            transform.position -= transform.forward * _testMoveSpeed * Time.deltaTime;
        }
    }

    [SerializeField] private float _testMoveSpeed = 5.0f;
    [SerializeField] private KeyCode _keyCode_YUp = KeyCode.W;
    [SerializeField] private KeyCode _keyCode_YDown = KeyCode.S;
    [SerializeField] private KeyCode _keyCode_XUp = KeyCode.D;
    [SerializeField] private KeyCode _keyCode_XDown = KeyCode.A;
    [SerializeField] private KeyCode _keyCode_ZUp = KeyCode.P;
    [SerializeField] private KeyCode _keyCode_ZDown = KeyCode.Semicolon;
}
