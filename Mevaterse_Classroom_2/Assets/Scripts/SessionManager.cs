using UnityEngine;

public class SessionManager : MonoBehaviour
{
    public static SessionManager Instance { get; private set; }

    public string Subject { get; private set; } = "Default"; // Default subject if none provided
    public float ParticipationLevel { get; private set; } = 0.5f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetSubject(string subject)
    {
        Subject = subject;
    }

    public void SetParticipationLevel(float value)
    {
        ParticipationLevel = Mathf.Clamp01(value);
    }


}
