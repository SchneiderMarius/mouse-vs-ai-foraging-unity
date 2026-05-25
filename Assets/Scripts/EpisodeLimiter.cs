using System;
using UnityEngine;
using Unity.MLAgents;

public class EpisodeLimiter : MonoBehaviour
{
    public int defaultEpisodes = 50;
    int    maxEpisodes;
    int    episodeCount = 0;

    void Awake()
    {
        // Parse --episodes=NNN
        maxEpisodes = defaultEpisodes;
        foreach (var arg in Environment.GetCommandLineArgs())
            if (arg.StartsWith("--episodes=") &&
                int.TryParse(arg.Substring(11), out var v))
                maxEpisodes = v;

        Debug.Log($"[EpisodeLimiter] Will run {maxEpisodes} episodes");

        // Subscribe to the Academy reset event
        Academy.Instance.OnEnvironmentReset += OnEpisodeBegin;
    }

    void OnDisable()
    {
        Academy.Instance.OnEnvironmentReset -= OnEpisodeBegin;
    }

    void OnEpisodeBegin()
    {
        episodeCount++;
        if (maxEpisodes > 0 && episodeCount > maxEpisodes)
        {
            Debug.Log($"[EpisodeLimiter] Reached {episodeCount}/{maxEpisodes} → Quitting");
            Application.Quit(0);
        }
    }
}
