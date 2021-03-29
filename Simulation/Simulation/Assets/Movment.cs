using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movment : MonoBehaviour
{

    [SerializeField]public Rigidbody rigidbody;
    Vector3 pos = new Vector3(0, 0.5f ,0);

    private Vector3 rot = new Vector3(0, 0, 0);
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        rigidbody.MovePosition(pos);
        rigidbody.MoveRotation(Quaternion.Euler(rot));
    }

    public void Move(Vector2 move)
    {
        pos += rigidbody.rotation * new Vector3(move.x , 0 , move.y);
    }

    public void Rotate(float rotate)
    {
        rot += new Vector3(0 , rotate , 0);
    }
}
