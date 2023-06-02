using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Manager : MonoBehaviour
{

    [SerializeField] private float timeframe;
    [SerializeField] private int populationSize;
    [SerializeField] private GameObject prefab;

    [SerializeField] private int[] layers = new int[3] { 5, 3, 2 };

    [SerializeField, Range(0.0001f, 1f)] private float mutationChance = 0.01f;
    [SerializeField, Range(0f, 1f)] private float mutationStrength = 0.5f;
    [SerializeField, Range(0.1f, 10f)] private float gamespeed = 1f;

    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI genText;

    private List<NeuralNetwork> networks = new List<NeuralNetwork>();
    private List<Bot> cars;
    private int bestScore;
    private int generation;

    private void Start()
    {
        if (populationSize % 2 != 0)
        {
            populationSize = 50;
        }

        InvokeRepeating("CreateBots", 0.1f, timeframe);
        InitNetworks();
    }

    private void InitNetworks()
    {
        networks.Clear();
        for (int i = 0; i < populationSize; i++)
        {
            NeuralNetwork net = new NeuralNetwork(layers);
            net.Load("Assets/Pre-trained.txt");

            networks.Add(net);
        }
    }

    private void CreateBots()
    {
        Time.timeScale = gamespeed;
        if (cars != null)
        {
            for (int i = 0; i < cars.Count; i++)
            {
                cars[i].UpdateFitness();
                Destroy(cars[i].gameObject);
            }

            networks.Sort();
            networks[populationSize - 1].Save("Assets/Save.txt");//saves networks weights and biases to file, to preserve network performance

            generation += 1;
            scoreText.text = "Best score: " + networks[populationSize - 1].fitness;
            genText.text = "Current generation: " + generation;

            var new_networks = new List<NeuralNetwork>();
            for (int i = 0; i < populationSize / 2; i++)
            {
                var parent1 = networks[i];
                var parent2 = networks[i + populationSize / 2];
                var child1 = parent1.copy(new NeuralNetwork(layers));
                var child2 = parent2.copy(new NeuralNetwork(layers));
                child1.Crossover(child2);
                child1.Mutate((int)(1 / mutationChance), mutationStrength);
                child2.Mutate((int)(1 / mutationChance), mutationStrength);
                new_networks.Add(child1);
                new_networks.Add(child2);
            }

            networks.Clear();
            networks.AddRange(new_networks);
        }

        cars = new List<Bot>();
        for (int i = 0; i < populationSize; i++)
        {
            Bot car = (Instantiate(prefab, new Vector3(0, 1.6f, -16), new Quaternion(0, 0, 1, 0))).GetComponent<Bot>();
            car.network = networks[i];
            cars.Add(car);
        }
    }
}