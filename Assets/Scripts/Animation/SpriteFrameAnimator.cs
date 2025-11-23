using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class SpriteFrameAnimator : MonoBehaviour
{
    public enum TargetMode { SpriteRenderer, UIImage }

    [Header("Target")]
    public TargetMode targetMode = TargetMode.SpriteRenderer;
    public SpriteRenderer targetSpriteRenderer; // untuk world sprite
    public Image targetUIImage;                 // untuk UI image

    [Header("Frames (drag sprites here)")]
    public List<Sprite> frames = new List<Sprite>(); // isi dengan frame-frame kamu (0..n-1)

    [Header("Playback")]
    [Tooltip("Default frames per second. Jika speedSlider diisi, nilai ini akan ditimpa oleh slider.")]
    public float fps = 6f;
    public bool loop = true;
    public bool playOnStart = true;
    public int startFrame = 0;

    [Header("Optional: UI Slider to control speed")]
    [Tooltip("Optional: kalau diisi, nilai slider akan mengontrol fps (slider.value = fps).")]
    public Slider speedSlider;

    [Header("Flip / Facing")]
    [Tooltip("Jika di-check, sprite akan dibalik horizontal saat SetFlip(true) dipanggil.")]
    public bool allowFlip = true;
    [Tooltip("Jika di-check untuk UIImage, flip dilakukan dengan membalik scale.x pada RectTransform.")]
    public bool useScaleFlipForUI = true;
    [Tooltip("Otomatis update flip saat FaceDirection dipanggil (dir > 0 => facing right).")]
    public bool flipUsingFaceDirection = true;

    // internal
    private int currentFrame = 0;
    private float timer = 0f;
    private bool isPlaying = false;

    // track current flip state (true = flipped horizontally)
    private bool isFlipped = false;

    void Reset()
    {
        // coba auto-assign SpriteRenderer atau Image jika ada di GameObject yang sama
        targetSpriteRenderer = GetComponent<SpriteRenderer>();
        targetUIImage = GetComponent<Image>();
    }

    void Start()
    {
        if (frames == null) frames = new List<Sprite>();
        currentFrame = Mathf.Clamp(startFrame, 0, Mathf.Max(0, frames.Count - 1));

        if (targetMode == TargetMode.SpriteRenderer && targetSpriteRenderer == null)
            targetSpriteRenderer = GetComponent<SpriteRenderer>();
        if (targetMode == TargetMode.UIImage && targetUIImage == null)
            targetUIImage = GetComponent<Image>();

        if (playOnStart) Play(); else Pause();

        ApplyCurrentFrame();
    }

    void Update()
    {
        if (frames == null || frames.Count == 0) return;
        if (!isPlaying) return;

        // jika ada slider, gunakan nilainya sebagai fps
        if (speedSlider != null)
        {
            // asumsikan developer sudah mengatur slider.minValue dan slider.maxValue di Inspector
            fps = Mathf.Max(0.0001f, speedSlider.value);
        }

        if (fps <= 0f) return; // fps 0 -> tidak maju

        timer += Time.deltaTime;
        float interval = 1f / fps;
        while (timer >= interval)
        {
            timer -= interval;
            AdvanceFrame();
        }
    }

    void AdvanceFrame()
    {
        currentFrame++;
        if (currentFrame >= frames.Count)
        {
            if (loop) currentFrame = 0;
            else
            {
                currentFrame = frames.Count - 1;
                Pause();
            }
        }
        ApplyCurrentFrame();
    }

    void ApplyCurrentFrame()
    {
        if (frames == null || frames.Count == 0) return;

        Sprite s = frames[Mathf.Clamp(currentFrame, 0, frames.Count - 1)];
        if (targetMode == TargetMode.SpriteRenderer)
        {
            if (targetSpriteRenderer != null)
                targetSpriteRenderer.sprite = s;
        }
        else // UIImage
        {
            if (targetUIImage != null)
                targetUIImage.sprite = s;
        }
    }

    // ----- Flip / Facing API -----

    /// <summary>
    /// SetFlip(true) -> flipped horizontally (mirrored).
    /// SetFlip(false) -> normal.
    /// </summary>
    public void SetFlip(bool flip)
    {
        if (!allowFlip) return;
        isFlipped = flip;
        ApplyFlip();
    }

    /// <summary>
    /// FaceDirection: if dir &gt; 0 => face right (no flip), dir &lt; 0 => face left (flip).
    /// If dir == 0 nothing changes.
    /// </summary>
    public void FaceDirection(float dir)
    {
        if (!flipUsingFaceDirection) return;
        if (dir > 0f) SetFlip(false);
        else if (dir < 0f) SetFlip(true);
    }

    private void ApplyFlip()
    {
        if (!allowFlip) return;

        if (targetMode == TargetMode.SpriteRenderer)
        {
            if (targetSpriteRenderer != null)
            {
                // gunakan SpriteRenderer.flipX karena lebih aman dari perubahan scale
                targetSpriteRenderer.flipX = isFlipped;
            }
        }
        else // UIImage
        {
            if (targetUIImage != null)
            {
                RectTransform rt = targetUIImage.rectTransform;
                if (useScaleFlipForUI)
                {
                    Vector3 scale = rt.localScale;
                    scale.x = (isFlipped ? -Mathf.Abs(scale.x) : Mathf.Abs(scale.x));
                    rt.localScale = scale;
                }
                else
                {
                    // alternatif: create a material flip or UV flip (lebih kompleks) - default pakai scale
                    Vector3 scale = rt.localScale;
                    scale.x = (isFlipped ? -Mathf.Abs(scale.x) : Mathf.Abs(scale.x));
                    rt.localScale = scale;
                }
            }
        }
    }

    // ----- Public control API -----
    public void Play()
    {
        if (frames == null || frames.Count == 0) { isPlaying = false; return; }
        isPlaying = true;
    }

    public void Pause()
    {
        isPlaying = false;
    }

    public void Stop()
    {
        isPlaying = false;
        currentFrame = 0;
        timer = 0f;
        ApplyCurrentFrame();
    }

    public void SetFrame(int index)
    {
        if (frames == null || frames.Count == 0) return;
        currentFrame = Mathf.Clamp(index, 0, frames.Count - 1);
        ApplyCurrentFrame();
    }

    public void SetFPS(float newFps)
    {
        fps = Mathf.Max(0f, newFps);
    }

    // method berguna untuk UI Slider OnValueChanged (opsional)
    public void OnSpeedSliderChanged(float sliderValue)
    {
        // jika slider langsung disambungkan ke field speedSlider, Update() sudah pakai slider.value.
        // Namun jika kamu ingin memanggil manual, gunakan ini:
        SetFPS(Mathf.Max(0.0001f, sliderValue));
    }

    // Untuk debug/inspector friendly
#if UNITY_EDITOR
    void OnValidate()
    {
        if (frames != null)
        {
            startFrame = Mathf.Clamp(startFrame, 0, Mathf.Max(0, frames.Count - 1));
            currentFrame = Mathf.Clamp(currentFrame, 0, Mathf.Max(0, frames.Count - 1));
        }
        // apply flip in editor if possible
        if (!Application.isPlaying)
        {
            ApplyFlip();
        }
    }
#endif
}
