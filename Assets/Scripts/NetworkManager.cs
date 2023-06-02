using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{

    public float timeframe;
    public int populationSize;//размер популяции
    public GameObject prefab;//префаб бота

    public int[] layers = new int[3] { 5, 3, 2 };//инициализация нейронной сети

    [Range(0.0001f, 1f)] public float MutationChance = 0.01f;

    [Range(0f, 1f)] public float MutationStrength = 0.5f;

    [Range(0.1f, 10f)] public float Gamespeed = 1f;

    //public List<Bot> Bots;
    public List<NeuralNetwork> networks;
    private List<Bots> cars;

    void Start()
    {
        if (populationSize % 2 != 0)//проверка на корректность популяции
           populationSize = 50;
        
        InvokeRepeating("CreateBots", 0.1f, timeframe);//вызов нового поколения
        InitNetworks();
    }

    public void InitNetworks()
    {
        networks = new List<NeuralNetwork>();
        for (int i = 0; i < populationSize; i++)
        {
            Debug.Log("start");
            NeuralNetwork net = new NeuralNetwork(layers);
            // net.Load("Assets/Pre-trained.txt");//on start load the network save
            
            networks.Add(net);
        }
    }

    public void CreateBots()
    {
        Time.timeScale = Gamespeed;//sets gamespeed, which will increase to speed up training
        if (cars != null)
        {
            for (int i = 0; i < cars.Count; i++)
            {
                GameObject.Destroy(cars[i].gameObject);//if there are Prefabs in the scene this will get rid of them
            }

            SortNetworks();//this sorts networks and mutates them
        }

        cars = new List<Bots>();
        for (int i = 0; i < populationSize; i++)
        {
            Bots car = (Instantiate(prefab, new Vector3(0, 1.6f, -16), new Quaternion(0, 0, 1, 0))).GetComponent<Bots>();//create botes
            car.network = networks[i];//deploys network to each learner
            cars.Add(car);
        }
        
    }

    public void SortNetworks()
    {
        for (int i = 0; i < populationSize; i++)
        {
            cars[i].UpdateFitness();//gets bots to set their corrosponding networks fitness
        }
        networks.Sort();
        networks[populationSize - 1].Save("Assets/Save.txt");//saves networks weights and biases to file, to preserve network performance
        for (int i = 0; i < populationSize / 2; i++)
        {
            networks[i] = networks[i + populationSize / 2].copy(new NeuralNetwork(layers));
            networks[i].Mutate((int)(1/MutationChance), MutationStrength);
        }
    }
}