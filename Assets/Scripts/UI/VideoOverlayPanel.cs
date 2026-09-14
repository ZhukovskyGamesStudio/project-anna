using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using System;

public class VideoOverlayPanel : ObjectPanel
{
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private RawImage videoImage;
    [SerializeField] private RenderTexture targetTexture;

    private Action onFinished;
    private bool playAudio;

    public void PlayClip(VideoClip clip, Action onComplete, bool withAudio)
    {
        onFinished = onComplete;
        playAudio = withAudio;

        if (clip == null || videoPlayer == null)
        {
            FinishNow();
            return;
        }

        videoPlayer.clip = clip;
        UIManager.Instance.OpenPanel(this);
    }

    public override void Show()
    {
        ClearRenderTexture();

        base.Show();

        if (videoImage != null)
        {
            videoImage.enabled = false;
        }

        videoPlayer.isLooping = false;

        if (playAudio)
        {
            videoPlayer.audioOutputMode = VideoAudioOutputMode.Direct;
        }
        else
        {
            videoPlayer.audioOutputMode = VideoAudioOutputMode.None;
        }

        videoPlayer.loopPointReached += OnVideoFinished;
        videoPlayer.prepareCompleted += OnPrepared;
        videoPlayer.Prepare();
    }

    private void OnPrepared(VideoPlayer vp)
    {
        videoPlayer.prepareCompleted -= OnPrepared;

        if (videoImage != null)
        {
            videoImage.enabled = true;
        }

        videoPlayer.Play();
    }

    public override void Hide()
    {
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= OnVideoFinished;
            videoPlayer.prepareCompleted -= OnPrepared;
            videoPlayer.Stop();
        }

        base.Hide();
    }

    private void OnVideoFinished(VideoPlayer vp)
    {
        UIManager.Instance.CloseCurrentPanel();
        FinishNow();
    }

    private void ClearRenderTexture()
    {
        if (targetTexture == null)
        {
            return;
        }

        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = targetTexture;
        GL.Clear(true, true, Color.black);
        RenderTexture.active = previous;
    }

    private void FinishNow()
    {
        Action callback = onFinished;
        onFinished = null;

        if (callback != null)
        {
            callback();
        }
    }
}