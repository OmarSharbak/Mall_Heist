using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RedLabsGames.Tools.QuickAnim.Examples
{
    public class TopDownPlayerController : MonoBehaviour
    {
        // Player Movement speed.
        public float speed = 5;

        // Time taken to blend between animations.
        public float animationBlendTime = 0.25f;


        
        // Variables to Store Values
        Vector2 input;
        Rigidbody2D rb;
        Vector3 moveVector;
        bool isDead = false;


        // Start is called before the first frame update
        void Start()
        {
            rb = GetComponent<Rigidbody2D>();
        }

        // Update is called once per frame
        void Update()
        {

            // Do Not Update when player is dead.
            if (isDead)
            {
                return;
            }

            // Gathering Inputs.
            input.x = Input.GetAxisRaw("Horizontal");
            input.y = Input.GetAxisRaw("Vertical");

            // Switch Animation Base On Input.
            if (input.normalized.magnitude > 0.1f)
            {
                // This Function helps to blend current playing animation to the given animation name.
                gameObject.CrossFadeQuickAnim("Move", animationBlendTime);
            }
            else
            {
                // This Function helps to blend current playing animation to the given animation name.
                gameObject.CrossFadeQuickAnim("Idle", animationBlendTime);
            }

        }


        private void FixedUpdate()
        {
            // Do Not Update when player is dead.
            if (isDead)
            {
                return;
            }


            // Apply Movement.
            moveVector.x = input.x * speed;
            moveVector.y = input.y * speed;
            rb.velocity = moveVector;
        }


        private void OnCollisionEnter2D(Collision2D collision)
        {
            // Checking for Collision with Traps.
            if (collision.collider.GetComponent<Trap>()!=null)
            {
                Die();    
            }
            // Checking for Collision with Coins.
            if (collision.collider.GetComponent<Coin>() != null) 
            {
                // Coins has an Quick Anim Unity Event at the end of the PickUp animation to make it disappear.
                collision.collider.gameObject.PlayQuickAnim("PickUp");
            }
        }

        void Die()
        {
            // Make player stop.
            rb.velocity = Vector2.zero;
            isDead = true;
            
            // This function instantly plays the dead animation.
            gameObject.PlayQuickAnim("Dead");
        }


        // This function is called from spawn animation event to enable movement after the spawn animation.
        public void Spawn()
        {
            // Switch to idle animation after spawning animation is played.
            gameObject.CrossFadeQuickAnim("Idle", animationBlendTime);

            // This enables this component.
            enabled = true;
        }


        // This function is called from death animation event to reload scene.
        public void ReloadScene()
        {
            // Reload Current Scene.
            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
        }
       
    }
}
