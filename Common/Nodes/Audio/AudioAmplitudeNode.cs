using DG.Tweening;
using UnityEngine;
using Warudo.Core.Attributes;
using Warudo.Core.Graphs;

namespace Warudo.Plugins.Core.Nodes;

[NodeType(Id = "mira-audio-amplitude-node", Title = "Audio Amplitude", Category = "Audio")]
public class AudioAmplitudeNode : Node
{
    private float masterAmplitude = 0f;
    private float bassAmplitude = 0f;
    private float midAmplitude = 0f;
    private float trebleAmplitude = 0f;
    private float customChannelAmplitude = 0f;

    public override void OnFixedUpdate()
    {
        base.OnFixedUpdate();
        this.updateAmplitude();
    }

    [DataInput]
    [Label("Custom Channel Index")]
    public int CustomChannelIndex = 0;

    [DataOutput]
    [Label("Master")]
    public float MasterAmplitude()
    {
        return this.masterAmplitude;
    }

    [DataOutput]
    [Label("Bass")]
    public float BassAmplitude()
    {
        return this.bassAmplitude;
    }

    [DataOutput]
    [Label("Mid")]
    public float MidAmplitude()
    {
        return this.midAmplitude;
    }

    [DataOutput]
    [Label("Treble")]
    public float TrebleAmplitude()
    {
        return this.trebleAmplitude;
    }

    [DataOutput]
    [Label("Custom Channel")]
    public float CustomChannelAmplitude()
    {
        return this.customChannelAmplitude;
    }

    private void updateAmplitude()
    {
        float[] spectrumData = new float[1024];
        // Get the spectrum data from the AudioListener
        AudioListener.GetSpectrumData(spectrumData, 0, FFTWindow.BlackmanHarris);

        // Calculate the average amplitude from the spectrum data
        float masterSum = 0f;
        float bassSum = 0f;
        float midSum = 0f;
        float trebleSum = 0f;
        for (int i = 0; i < spectrumData.Length; i++)
        {
            masterSum += spectrumData[i];
            if (i < spectrumData.Length / 3)
            {
                bassSum += spectrumData[i];
            }
            else if (i < 2 * spectrumData.Length / 3)
            {
                midSum += spectrumData[i];
            }
            else
            {
                trebleSum += spectrumData[i];
            }
        }

        if (CustomChannelIndex >= 0 && CustomChannelIndex < spectrumData.Length)
        {
            this.customChannelAmplitude = spectrumData[CustomChannelIndex];
        }
        else
        {
            this.customChannelAmplitude = 0f;
        }

        this.masterAmplitude = masterSum / spectrumData.Length;
        this.bassAmplitude = bassSum / (spectrumData.Length / 3);
        this.midAmplitude = midSum / (spectrumData.Length / 3);
        this.trebleAmplitude = trebleSum / (spectrumData.Length / 3);
    }
}
