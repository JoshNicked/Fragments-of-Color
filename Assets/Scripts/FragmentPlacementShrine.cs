using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

/// <summary>
/// Three socket placements for collected fragments. Player must equip a fragment and press the place key in range.
/// After all three are placed, runs a short cinematic (camera + beam) and lerps Global Volume Color Adjustments
/// to reveal the scene color.
/// </summary>
public class FragmentPlacementShrine : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Transform player;
    [SerializeField] PlayerMotor playerMotor;
    [SerializeField] HotbarSystem hotbar;
    [Tooltip("Usually the same transform ThirdPersonCamera moves (Main Camera).")]
    [SerializeField] Transform cameraTransform;
    [SerializeField] ThirdPersonCamera thirdPersonCamera;

    [Header("Sockets")]
    [SerializeField] Transform[] sockets = new Transform[3];

    [Header("Interaction")]
    [SerializeField] float interactionRadius = 4f;
    [SerializeField] KeyCode placeKey = KeyCode.G;
    [Tooltip("Player must carry at least three fragments before the first socket accepts a place.")]
    [SerializeField] bool requireThreeFragmentsBeforeFirstPlace = true;
    [SerializeField] bool requireRegisteredFragment = true;
    [SerializeField] float placedFragmentLocalScale = 0.35f;

    [Header("Cinematic — Camera")]
    [SerializeField] Transform cinematicViewPoint;
    [SerializeField] Transform cinematicLookAt;
    [SerializeField] float cameraFocusDuration = 2f;

    [Header("Cinematic — Beam")]
    [SerializeField] Transform beamRoot;
    [SerializeField] Vector3 beamStartScale = new Vector3(0.15f, 0.02f, 0.15f);
    [SerializeField] Vector3 beamEndScale = new Vector3(0.2f, 45f, 0.2f);
    [SerializeField] float beamRiseDuration = 1.25f;
    [SerializeField] float beamChargeAnticipation = 0.15f;
    [SerializeField] ParticleSystem beamChargeParticles;
    [SerializeField] ParticleSystem beamBurstParticles;
    [SerializeField] float explosionHold = 0.6f;

    [Header("Global Volume — Color Adjustments")]
    [SerializeField] Volume globalVolume;
    [Tooltip("Final color filter when the world is revealed (often white = no tint).")]
    [SerializeField] Color revealedColorFilter = Color.white;
    [Tooltip("Final saturation (-100…100). 0 is default vividness.")]
    [SerializeField] float revealedSaturation = 0f;
    [SerializeField] float colorRevealDuration = 2.5f;

    [Header("Persistence")]
    [Tooltip("PlayerPrefs id for this shrine (default: active scene name).")]
    [SerializeField] string persistenceId = "";

    const string PrefsPrefix = "FoC_VolReveal_";

    int placedCount;
    bool placementComplete;
    bool isSequencePlaying;

    string ResolvedPersistenceId =>
        string.IsNullOrEmpty(persistenceId) ? SceneManager.GetActiveScene().name : persistenceId;

    void Awake()
    {
        if (playerMotor == null)
            playerMotor = FindObjectOfType<PlayerMotor>();
        if (playerMotor != null && player == null)
            player = playerMotor.transform;
        if (hotbar == null)
            hotbar = FindObjectOfType<HotbarSystem>();
        if (thirdPersonCamera == null)
            thirdPersonCamera = FindObjectOfType<ThirdPersonCamera>();
        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;
        if (globalVolume == null)
            globalVolume = FindObjectOfType<Volume>();

        if (beamRoot != null)
            beamRoot.localScale = beamStartScale;
    }

    void Start()
    {
        TryApplySavedRevealedColor();
    }

    void Update()
    {
        if (isSequencePlaying || placementComplete)
            return;
        if (sockets == null || sockets.Length < 3)
            return;
        for (int i = 0; i < 3; i++)
        {
            if (sockets[i] == null)
                return;
        }

        if (player == null || hotbar == null)
            return;

        if (requireThreeFragmentsBeforeFirstPlace && placedCount == 0 && hotbar.GetFilledSlotCount() < 3)
            return;

        if (Vector3.Distance(player.position, transform.position) > interactionRadius)
            return;

        if (!Input.GetKeyDown(placeKey))
            return;

        if (hotbar.GetEquippedSlotIndex() < 0)
        {
            Debug.Log("[Shrine] Equip a fragment in the hotbar (1–3), stand near the shrine, then press " + placeKey + " to place it.");
            return;
        }

        HotbarItem pending = hotbar.GetItemAtSlot(hotbar.GetEquippedSlotIndex());
        if (pending == null || pending.prefab == null)
            return;

        if (requireRegisteredFragment)
        {
            FragmentRegistry reg = FragmentRegistry.GetInstance();
            if (reg != null && reg.GetFragment(pending.prefabName) == null)
            {
                Debug.LogWarning("[Shrine] Equipped item is not a registered fragment.");
                return;
            }
        }

        if (!hotbar.ConsumeEquippedItem(out HotbarItem placed))
            return;

        GameObject vis = Instantiate(placed.prefab, sockets[placedCount].position, sockets[placedCount].rotation, sockets[placedCount]);
        vis.transform.localScale = Vector3.one * placedFragmentLocalScale;
        foreach (Renderer r in vis.GetComponentsInChildren<Renderer>())
            r.enabled = true;
        foreach (SpriteRenderer sr in vis.GetComponentsInChildren<SpriteRenderer>())
            sr.enabled = true;

        placedCount++;

        if (placedCount >= 3)
        {
            placementComplete = true;
            StartCoroutine(CompletionSequence());
        }
    }

    IEnumerator CompletionSequence()
    {
        isSequencePlaying = true;

        if (playerMotor != null)
            playerMotor.inputFrozen = true;
        if (thirdPersonCamera != null)
            thirdPersonCamera.enabled = false;

        Transform cam = cameraTransform;
        if (cam == null && Camera.main != null)
            cam = Camera.main.transform;

        if (cam != null && cinematicViewPoint != null && cinematicLookAt != null)
        {
            Vector3 camStartPos = cam.position;
            Quaternion camStartRot = cam.rotation;
            Vector3 camEndPos = cinematicViewPoint.position;
            Vector3 lookDir = (cinematicLookAt.position - camEndPos).normalized;
            if (lookDir.sqrMagnitude < 0.001f)
                lookDir = Vector3.up;
            Quaternion camEndRot = Quaternion.LookRotation(lookDir, Vector3.up);

            for (float t = 0; t < cameraFocusDuration; t += Time.deltaTime)
            {
                float u = Mathf.SmoothStep(0f, 1f, t / cameraFocusDuration);
                cam.position = Vector3.Lerp(camStartPos, camEndPos, u);
                cam.rotation = Quaternion.Slerp(camStartRot, camEndRot, u);
                yield return null;
            }

            cam.position = camEndPos;
            cam.rotation = camEndRot;
        }
        else
            yield return null;

        if (beamRoot != null)
        {
            beamRoot.localScale = beamStartScale;
            for (float t = 0; t < beamRiseDuration; t += Time.deltaTime)
            {
                float u = t / beamRiseDuration;
                beamRoot.localScale = Vector3.Lerp(beamStartScale, beamEndScale, u);
                yield return null;
            }

            beamRoot.localScale = beamEndScale;
        }

        if (beamChargeParticles != null)
            beamChargeParticles.Play();

        yield return new WaitForSeconds(beamChargeAnticipation);

        if (beamBurstParticles != null)
        {
            beamBurstParticles.transform.position = cinematicLookAt != null
                ? cinematicLookAt.position
                : transform.position + Vector3.up * 20f;
            beamBurstParticles.Play();
        }

        yield return new WaitForSeconds(explosionHold);

        HideBeamAndParticles();

        yield return AnimateColorReveal();

        GameTimer gameTimer = FindObjectOfType<GameTimer>();
        if (gameTimer != null)
            gameTimer.PauseTimer();

        if (thirdPersonCamera != null)
        {
            thirdPersonCamera.SyncAnglesFromTransform();
            thirdPersonCamera.enabled = true;
        }

        if (playerMotor != null)
            playerMotor.inputFrozen = false;

        isSequencePlaying = false;
    }

    void HideBeamAndParticles()
    {
        if (beamRoot != null)
            beamRoot.gameObject.SetActive(false);

        if (beamChargeParticles != null)
        {
            beamChargeParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            beamChargeParticles.gameObject.SetActive(false);
        }

        if (beamBurstParticles != null)
        {
            beamBurstParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            beamBurstParticles.gameObject.SetActive(false);
        }
    }

    IEnumerator AnimateColorReveal()
    {
        if (globalVolume == null || globalVolume.profile == null)
            yield break;

        if (!globalVolume.profile.TryGet(out ColorAdjustments colorAdj))
        {
            Debug.LogWarning("[Shrine] Global Volume has no Color Adjustments override.");
            yield break;
        }

        Color c0 = colorAdj.colorFilter.value;
        float s0 = colorAdj.saturation.value;
        Color c1 = revealedColorFilter;
        float s1 = revealedSaturation;

        for (float t = 0; t < colorRevealDuration; t += Time.deltaTime)
        {
            float u = Mathf.SmoothStep(0f, 1f, t / colorRevealDuration);
            colorAdj.colorFilter.Override(Color.Lerp(c0, c1, u));
            colorAdj.saturation.Override(Mathf.Lerp(s0, s1, u));
            yield return null;
        }

        colorAdj.colorFilter.Override(c1);
        colorAdj.saturation.Override(s1);
        SaveRevealedColorState(c1, s1);
    }

    bool HasSavedRevealedColor()
    {
        string id = ResolvedPersistenceId;
        return PlayerPrefs.GetInt(PrefsPrefix + id + "_Complete", 0) == 1;
    }

    void SaveRevealedColorState(Color colorFilter, float saturation)
    {
        string id = ResolvedPersistenceId;
        PlayerPrefs.SetInt(PrefsPrefix + id + "_Complete", 1);
        PlayerPrefs.SetFloat(PrefsPrefix + id + "_R", colorFilter.r);
        PlayerPrefs.SetFloat(PrefsPrefix + id + "_G", colorFilter.g);
        PlayerPrefs.SetFloat(PrefsPrefix + id + "_B", colorFilter.b);
        PlayerPrefs.SetFloat(PrefsPrefix + id + "_A", colorFilter.a);
        PlayerPrefs.SetFloat(PrefsPrefix + id + "_Sat", saturation);
        PlayerPrefs.Save();
    }

    void TryApplySavedRevealedColor()
    {
        if (!HasSavedRevealedColor())
            return;

        if (globalVolume == null)
            globalVolume = FindObjectOfType<Volume>();
        if (globalVolume == null || globalVolume.profile == null)
            return;

        if (!globalVolume.profile.TryGet(out ColorAdjustments colorAdj))
            return;

        string id = ResolvedPersistenceId;
        Color c = new Color(
            PlayerPrefs.GetFloat(PrefsPrefix + id + "_R", 1f),
            PlayerPrefs.GetFloat(PrefsPrefix + id + "_G", 1f),
            PlayerPrefs.GetFloat(PrefsPrefix + id + "_B", 1f),
            PlayerPrefs.GetFloat(PrefsPrefix + id + "_A", 1f));
        float sat = PlayerPrefs.GetFloat(PrefsPrefix + id + "_Sat", 0f);

        colorAdj.colorFilter.Override(c);
        colorAdj.saturation.Override(sat);

        placementComplete = true;
        placedCount = 3;
        HideBeamAndParticles();
    }
}
