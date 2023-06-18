using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Bot : MonoBehaviour
{
    private float[] input = new float[5];
    public NeuralNetwork network;
    public LayerMask raycastMask;

    public float speedFactor;
    public float rotationFactor;

    private int checkpointIndex;
    private bool isCrashed;

    [SerializeField] private List<GameObject> checkPoints;

    private void Start()
    {
        for (int i = 2; i < GameObject.FindGameObjectsWithTag("CheckPoint").Length + 2; i++)
        {
            checkPoints.Add(GameObject.Find("CheckPoint (" + i + ")"));
        }
    }

    void FixedUpdate()
    {
        if (!isCrashed)
        {
            for (int i = 0; i < 5; i++)
            {
                Vector3 newVector = Quaternion.AngleAxis(i * 45 - 90, new Vector3(0, 1, 0)) * transform.right;
                RaycastHit hit;
                Ray Ray = new Ray(transform.position, newVector);
                
                if (Physics.Raycast(Ray, out hit, 10, raycastMask))
                {
                    input[i] = (10 - hit.distance) / 10;
                }
                else
                {
                    input[i] = 0;
                }
            }

            float[] output = network.FeedForward(input);

            transform.Rotate(0, output[0] * rotationFactor, 0, Space.World);
            transform.position += this.transform.right * (output[1] * speedFactor);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        
        if (collision.collider.gameObject.layer == 7)
        {
            for (int i = 0; i < checkPoints.Count; i++)
            {
                if (collision.collider.gameObject == checkPoints[i].gameObject)
                {
                    if (i == (checkpointIndex + 1 + checkPoints.Count) % checkPoints.Count)
                    {
                        checkpointIndex++;
                        break;
                    }
                }
            }
        }
        else if (collision.collider.gameObject.layer != 6)
        {
            isCrashed = true;

        }
    }

    public void UpdateFitness()
    {
        network.fitness = checkpointIndex;
    }
}