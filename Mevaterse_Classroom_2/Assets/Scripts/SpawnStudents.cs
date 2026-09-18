using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

// Classe per gestire lo spawn degli studenti e assegnare tratti casuali
public class SpawnStudents : MonoBehaviourPunCallbacks
{
    private GameObject chairs; // GameObject che contiene tutte le sedie nella stanza
    private int chairNumber;
    private bool[] assignedSeats; // Array per tenere traccia delle sedie assegnate
    private int studentNumber;
    private List<Student> studentList;
    private float participationLevel = 0.5f;

    void Awake()
    {
        //DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        //DontDestroyOnLoad(gameObject);
        chairs = GameObject.Find("chairs");
        chairNumber = chairs.transform.childCount;
        assignedSeats = new bool[chairNumber];
    }

    public override void OnCreatedRoom()
    {
        base.OnCreatedRoom();

        if (photonView.IsMine)
        {
            InstantiateStudent(studentNumber);
        }
    }

    public void InstantiateStudent(int studentNumber)
    {
        if (chairs == null)
        {
            throw new System.Exception("Chairs GameObject is null!");
        }

        if (chairNumber == 0)
        {
            throw new System.Exception("Number of chairs is 0!");
        }

        if (studentNumber <= 0)
        {
            return;
        }
        else if (studentNumber > chairNumber)
        {
            Debug.LogError("Not enough chairs for all students! \nSetting maximum number of students to number of chairs.");
            studentNumber = chairNumber;
        }

        int randomIndex;
        studentList = new List<Student>();

        // Per ogni studente, assegna una sedia e tratti casuali
        for (int i = 0; i < studentNumber; i++)
        {
            // Trova una sedia non occupata
            do
            {   randomIndex = Random.Range(0, 42);
            } while (assignedSeats[randomIndex]);

            // Segna la sedia come occupata
            assignedSeats[randomIndex] = true;
            Transform randomChair = chairs.transform.GetChild(randomIndex);

            if (randomChair == null)
            {
                Debug.LogError("Chair transform is null!");
                return;
            }

            // Crea una posizione per lo spawn e assegna tratti casuali
            Vector3 spawnPosition = randomChair.position + new Vector3(0, .6f, .1f);

            // Genera tratti casuali
            Personality randomPersonality = (Personality)AdaptTraitFromSlider(participationLevel);
            Intelligence randomIntelligence = (Intelligence)AdaptTraitFromSlider(participationLevel);
            Interest randomInterest = (Interest)AdaptTraitFromSlider(participationLevel);
            Happiness randomHappiness = (Happiness)AdaptTraitFromSlider(participationLevel);

            Debug.Log($"Participation: {participationLevel} | Student traits: {randomPersonality}, {randomIntelligence}, {randomInterest}, {randomHappiness}");
            //Logger.Instance.LogInfo($"Spawned a {randomPersonality}, {randomIntelligence}, {randomInterest}, {randomHappiness} student!");

            // Instanzia lo studente nella posizione specificata e con tratti casuali
            GameObject student = PhotonNetwork.Instantiate("Student", spawnPosition, Quaternion.identity);
            studentList.Add(new Student(spawnPosition, randomPersonality, randomIntelligence, randomInterest, randomHappiness));

            if (student == null)
            {
                throw new System.Exception("Failed to instantiate SmartStudent!");
            }

            // Imposta l'animazione di idle dello studente
            Animator studentAnimator = student.GetComponent<Animator>();
            studentAnimator.Play("Idle");

            // Imposta i tratti di personalit� sul componente SmartStudentController
            SmartStudentController studentController = student.GetComponent<SmartStudentController>();
            if (studentController != null)
            {
                studentController.personality = randomPersonality;
                studentController.intelligence = randomIntelligence;
                studentController.interest = randomInterest;
                studentController.happiness = randomHappiness;
            }

            // Aggiorna la lista di studenti condivisa in rete
            string studentString = JsonUtility.ToJson(studentList);
            photonView.RPC("SetStudentData", RpcTarget.OthersBuffered, studentString);
        }
    }

    // Metodo pubblico chiamato in ConnectToServer.cs per impostare il numero di studenti
    public void SetStudentNumber(int number)
    {
        studentNumber = number;
    }

    public void SetParticipationLevel(float value)
    {
        participationLevel = Mathf.Clamp01(value);
    }

    private int AdaptTraitFromSlider(float sliderValue)
    {
        sliderValue = Mathf.Clamp01(sliderValue);

        // zona neutra
        if (sliderValue >= 0.4f && sliderValue <= 0.6f)
        {
            return UnityEngine.Random.Range(1, 6); // 1..5
        }

        // verso il minimo
        if (sliderValue < 0.4f)
        {
            if (sliderValue <= 0.15f)
                return 1;

            if (sliderValue <= 0.30f)
                return UnityEngine.Random.Range(1, 3); // 1..2

            return UnityEngine.Random.Range(1, 4); // 1..3
        }

        // verso il massimo
        if (sliderValue > 0.6f)
        {
            if (sliderValue >= 0.85f)
                return UnityEngine.Random.Range(4, 6); // 4..5

            if (sliderValue >= 0.70f)
                return UnityEngine.Random.Range(3, 6); // 3..5

            return UnityEngine.Random.Range(2, 5); // 2..4
        }

        return UnityEngine.Random.Range(1, 6);
    }

    [PunRPC]
    public void SetStudentData(string studentString)
    {
        studentList = JsonUtility.FromJson<List<Student>>(studentString);

        foreach (Student student in studentList)
        {
            GameObject studentObject = PhotonNetwork.Instantiate("Student", student.studentPosition, Quaternion.identity);

            // Imposta i tratti di personalit� su SmartStudentController
            SmartStudentController studentController = studentObject.GetComponent<SmartStudentController>();
            if (studentController != null)
            {
                studentController.personality = student.personality;
                studentController.intelligence = student.intelligence;
                studentController.interest = student.interest;
                studentController.happiness = student.happiness;
            }
        }
    }
}

// Classe per rappresentare uno studente con posizione e tratti di personalit�
[System.Serializable]
class Student
{
    public Vector3 studentPosition;
    public Personality personality;
    public Intelligence intelligence;
    public Interest interest;
    public Happiness happiness;

    public Student(Vector3 position, Personality personalityType, Intelligence intelligenceLevel, Interest interestLevel, Happiness happinessLevel)
    {
        studentPosition = position;
        personality = personalityType;
        intelligence = intelligenceLevel;
        interest = interestLevel;
        happiness = happinessLevel;
    }
}
