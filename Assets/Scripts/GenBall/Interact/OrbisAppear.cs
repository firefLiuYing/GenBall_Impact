using System;
using System.Collections;
using System.Collections.Generic;
using GenBall;
using GenBall.BattleSystem.Character;
using GenBall.Enemy;
using UnityEngine;
using UnityEngine.Playables;
using GenBall.Utils.EntityCreator;

public class OrbisAppear : MonoBehaviour
{
    public PlayableDirector playableDirector;
    private bool hasTriggered = false;
    public Transform appearPoint;
    

    private void OnTriggerEnter(Collider other)
    {
        if (!hasTriggered)
            {
            hasTriggered = true;
            playableDirector.Play();
            for (int i = 1; i <= 100; i++)
            {
                GameEntry.CharacterCreator.CreateEntity<CharacterState>(nameof(EnemyId.TestOrbis),appearPoint);
            }

            }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
