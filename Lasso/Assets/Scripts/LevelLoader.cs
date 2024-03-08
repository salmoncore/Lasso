using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
    [SerializeField] private string sceneName;
    public Animator transition;
    public float transitionTime = 1f;

    // opted for collision instead of a button down (can be changed)
    // also need more info on if the player can skip levels or they need certain
    // conditions to be able to continue on
    private void OnTriggerEnter2D(Collider2D collision) {
        
        PlayerMovement player = collision.gameObject.GetComponent<PlayerMovement>();

        if (player)
            loadNextLevel();
    }

    // wrapper function for the coroutine to start
    public void loadNextLevel() {

        // send in the scene name we want to go to
        StartCoroutine(loadLevel(sceneName));
    }

    // coroutine for transition
    IEnumerator loadLevel(string scene) {
        // plays the animation
        transition.SetTrigger("Start");

        // wait for animation
        yield return new WaitForSeconds(transitionTime);

        // load new scene
        SceneManager.LoadScene(scene);
    }
}
