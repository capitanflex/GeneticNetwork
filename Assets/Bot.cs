using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Bot : MonoBehaviour
{
    private float[] input = new float[5];//input to the neural network
    public NeuralNetwork network;
    public LayerMask raycastMask;//Mask for the sensors

    public float speedFactor;
    public float rotationFactor;

    private int checkpointIndex;
    private bool isCrashed;

    void FixedUpdate()
    {
        if (!isCrashed)//if the car has not collided with the wall, it uses the neural network to get an output
        {
            for (int i = 0; i < 5; i++)//draws five debug rays as inputs
            {
                Vector3 newVector = Quaternion.AngleAxis(i * 45 - 90, new Vector3(0, 1, 0)) * transform.right;//calculating angle of raycast
                RaycastHit hit;
                Ray Ray = new Ray(transform.position, newVector);
                // Debug.DrawRay(Ray.origin, Ray.direction*10);
                if (Physics.Raycast(Ray, out hit, 10, raycastMask))
                {
                    input[i] = (10 - hit.distance) / 10;//return distance, 1 being close
                }
                else
                {
                    input[i] = 0;//if nothing is detected, will return 0 to network
                }
            }

            float[] output = network.FeedForward(input);//Call to network to feedforward

            transform.Rotate(0, output[0] * rotationFactor, 0, Space.World);//controls the cars movement
            transform.position += this.transform.right * (output[1] * speedFactor);//controls the cars turning
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.gameObject.layer == LayerMask.NameToLayer("CheckPoint"))//check if the car passes a gate
        {
            GameObject[] checkPoints = GameObject.FindGameObjectsWithTag("CheckPoint");
            for (int i = 0; i < checkPoints.Length; i++)
            {
                if (collision.collider.gameObject == checkPoints[i] && i == (checkpointIndex + 1 + checkPoints.Length) % checkPoints.Length)
                {
                    checkpointIndex++;//if the gate is one ahead of it, it increments the position, which is used for the fitness/performance of the network
                    break;
                }
            }
        }
        else if (collision.collider.gameObject.layer != LayerMask.NameToLayer("Learner"))
        {
            isCrashed = true;//stop operation if car has collided

        }
    }

    public void UpdateFitness()
    {
        network.fitness = checkpointIndex;//updates fitness of network for sorting
    }
}