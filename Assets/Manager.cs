using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


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
    [SerializeField] private TMP_InputField InputPopulationSize;
    [SerializeField] private Slider InputGameSpeed;
    [SerializeField] private Slider InputMutationChance;
    [SerializeField] private Slider InputMutationStrength;
    [SerializeField] private TextMeshProUGUI InputGameSpeedText;
    [SerializeField] private TextMeshProUGUI InputMutationChanceText;
    [SerializeField] private TextMeshProUGUI InputMutationStrengthText;
    

    private List<NeuralNetwork> networks = new List<NeuralNetwork>();
    private List<Bot> cars;
    private int bestScore;
    private int generation;
    private string AppPath;
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
            net.Load(Path.Combine(Application.streamingAssetsPath, "Pre-trained.txt"));

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
            networks[populationSize - 1].Save(Path.Combine(Application.streamingAssetsPath, "/Save.txt"));

            generation += 1;
            scoreText.text = "Best gen score: " + networks[populationSize - 1].fitness;
            genText.text = "Current generation: " + generation;

            var newNetworks = new List<NeuralNetwork>();
            for (int i = 0; i < populationSize / 2; i++)
            {
                NeuralNetwork parent1 = networks[i];
                NeuralNetwork parent2 = networks[i + populationSize / 2];
                NeuralNetwork child1 = parent1.copy(new NeuralNetwork(layers));
                NeuralNetwork child2 = parent2.copy(new NeuralNetwork(layers));
                child1.Crossover(child2);
                child1.Mutate((int)(1 / mutationChance), mutationStrength);
                child2.Mutate((int)(1 / mutationChance), mutationStrength);
                newNetworks.Add(child1);
                newNetworks.Add(child2);
            }

            networks.Clear();
            networks.AddRange(newNetworks);
        }

        cars = new List<Bot>();
        for (int i = 0; i < populationSize; i++)
        {
            Bot car = (Instantiate(prefab, new Vector3(0, 1.6f, -16), new Quaternion(0, 0, 1, 0))).GetComponent<Bot>();
            car.network = networks[i];
            cars.Add(car);
        }
    }
    
    public void TextChange()
    {
        InputGameSpeedText.text = "x" + InputGameSpeed.value;
        InputMutationChanceText.text = InputMutationChance.value.ToString();
        InputMutationStrengthText.text = InputMutationStrength.value.ToString();
    }
    
    public void ChangeParameters()
    {
        bool isNumeric = int.TryParse(InputPopulationSize.text, out _);
        if (isNumeric)
            populationSize =  int.Parse(InputPopulationSize.text);
        
        mutationChance = InputMutationChance.value;
        mutationStrength = InputMutationStrength.value;
        gamespeed = InputGameSpeed.value;

    }

    
}