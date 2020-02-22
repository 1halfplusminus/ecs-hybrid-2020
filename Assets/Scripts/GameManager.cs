using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Entities;
using UnityEngine;
public class GameManager : MonoBehaviour {

    public static GameManager main;
    public GameObject ballPrefabs;

    public float xBound = 3f;
    public float yBound = 3f;
    public float ballSpeed = 3f;
    public float respawnDelay = 2f;

    private int[] playersScore;

    Entity ballEntityPrefab;
    EntityManager manager;

    WaitForSeconds oneSecond;
    WaitForSeconds delay;
    [SerializeField] private TextMeshProUGUI[] playersScoreText;
    [SerializeField] private TextMeshProUGUI mainText;

    // Start is called before the first frame update
    void Awake () {
        if (main != null && main != this) {
            Destroy (gameObject);
        }
        main = this;
        playersScore = new int[2];
        oneSecond = new WaitForSeconds (1f);
        delay = new WaitForSeconds (respawnDelay);

        StartCoroutine (CountdownAndSpawnBall ());
    }
    public void PlayerScored (int playerId) {
        playersScore[playerId]++;
        for (int i = 0; i < playersScore.Length && i < playersScoreText.Length; i++) {
            playersScoreText[i].text = playersScore[i].ToString ();
        }
        /*      StartCoroutine (CountdownAndSpawnBall ()); */
    }
    IEnumerator CountdownAndSpawnBall () {
        mainText.text = "Get Ready";
        yield return delay;

        mainText.text = "3";
        yield return oneSecond;

        mainText.text = "2";
        yield return oneSecond;

        mainText.text = "1";
        yield return oneSecond;

        mainText.text = "";

        SpawnBall ();
    }
    void SpawnBall () {

    }
}