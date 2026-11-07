using UnityEngine;

namespace CampusGlitch {
/// <summary>Visual state follows the controller; never writes velocity, input or colliders.</summary>
public class CGGuardianVisual : MonoBehaviour {
    CGHero hero;
    SpriteRenderer view;
    int state = -1;
    float stateAt, attackSeen, hurtSeen, attackEnd, hurtEnd, landEnd;
    bool wasGrounded, celebrating;
    public void Configure(CGHero owner, SpriteRenderer renderer) { hero = owner; view = renderer; }
    public void Celebrate() { celebrating = true; }

    void LateUpdate() {
        if (hero == null || view == null || CGStore.Data.useOriginalWolf || !CGGuardianArt.Available) return;
        if (hero.attackUntil > attackSeen) { attackSeen = hero.attackUntil; attackEnd = Time.time + .44f; }
        if (hero.hurtUntil > hurtSeen) { hurtSeen = hero.hurtUntil; hurtEnd = Time.time + .34f; }
        if (!wasGrounded && hero.Grounded && state == 4) landEnd = Time.time + .22f;
        wasGrounded = hero.Grounded;
        int next = celebrating ? 7 : Time.time < hurtEnd ? 6 : Time.time < attackEnd ? 5 :
            !hero.Grounded ? (hero.Body.linearVelocity.y > .3f ? 3 : 4) :
            Time.time < landEnd ? 4 : Mathf.Abs(hero.moveInput) > .15f ? (hero.running ? 2 : 1) : 0;
        if (next != state) { state = next; stateAt = Time.time; }
        float elapsed = Time.time - stateAt;
        int frame;
        switch (state) {
            case 0: frame = (int)(elapsed * 8) % 8; break;
            case 1: frame = (int)(elapsed * 12) % 8; break;
            case 2: frame = (int)(elapsed * 15) % 8; break;
            case 3: frame = Mathf.Min(7, (int)(elapsed * 14)); break;
            case 4: frame = hero.Grounded ? 4 + Mathf.Clamp((int)((.22f - (landEnd - Time.time)) * 18), 0, 3) : (int)(elapsed * 10) % 4; break;
            case 5: frame = Mathf.Min(7, (int)(elapsed * 18)); break;
            case 6: frame = Mathf.Min(7, (int)(elapsed * 24)); break;
            default: frame = Mathf.Min(7, (int)(elapsed * 10)); break;
        }
        bool purple = CGStore.Data.purpleEquipped;
        view.sprite = CGGuardianArt.Get(purple, state, frame);
        view.flipX = !hero.facesRight;
        view.transform.localPosition = Vector3.zero;
        view.transform.localScale = Vector3.one * CGGuardianArt.Scale(purple);
    }
}
}
