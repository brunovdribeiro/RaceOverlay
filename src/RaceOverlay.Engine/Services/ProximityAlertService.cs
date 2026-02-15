using System.IO;
using System.Media;

namespace RaceOverlay.Engine.Services;

public class ProximityAlertService
{
    private readonly SoundPlayer _leftPlayer;
    private readonly SoundPlayer _rightPlayer;
    private DateTime _lastLeftAlert = DateTime.MinValue;
    private DateTime _lastRightAlert = DateTime.MinValue;

    public int CooldownMs { get; set; } = 1500;

    public ProximityAlertService()
    {
        _leftPlayer = new SoundPlayer(GenerateWavStream(600, 150));
        _rightPlayer = new SoundPlayer(GenerateWavStream(800, 150));
        _leftPlayer.Load();
        _rightPlayer.Load();
    }

    public void PlayLeftAlert()
    {
        if ((DateTime.UtcNow - _lastLeftAlert).TotalMilliseconds < CooldownMs)
            return;
        _lastLeftAlert = DateTime.UtcNow;
        _leftPlayer.Play();
    }

    public void PlayRightAlert()
    {
        if ((DateTime.UtcNow - _lastRightAlert).TotalMilliseconds < CooldownMs)
            return;
        _lastRightAlert = DateTime.UtcNow;
        _rightPlayer.Play();
    }

    private static MemoryStream GenerateWavStream(int frequencyHz, int durationMs)
    {
        const int sampleRate = 44100;
        const int bitsPerSample = 16;
        const int channels = 1;

        int sampleCount = sampleRate * durationMs / 1000;
        int dataSize = sampleCount * channels * (bitsPerSample / 8);

        var ms = new MemoryStream();
        using var writer = new BinaryWriter(ms, System.Text.Encoding.UTF8, leaveOpen: true);

        // RIFF header
        writer.Write("RIFF"u8);
        writer.Write(36 + dataSize);
        writer.Write("WAVE"u8);

        // fmt sub-chunk
        writer.Write("fmt "u8);
        writer.Write(16); // sub-chunk size
        writer.Write((short)1); // PCM
        writer.Write((short)channels);
        writer.Write(sampleRate);
        writer.Write(sampleRate * channels * bitsPerSample / 8); // byte rate
        writer.Write((short)(channels * bitsPerSample / 8)); // block align
        writer.Write((short)bitsPerSample);

        // data sub-chunk
        writer.Write("data"u8);
        writer.Write(dataSize);

        // Generate sine wave with fade-in/out envelope
        int fadeInSamples = sampleRate * 10 / 1000; // 10ms
        int fadeOutSamples = sampleRate * 20 / 1000; // 20ms

        for (int i = 0; i < sampleCount; i++)
        {
            double t = (double)i / sampleRate;
            double amplitude = Math.Sin(2 * Math.PI * frequencyHz * t);

            // Envelope
            double envelope = 1.0;
            if (i < fadeInSamples)
                envelope = (double)i / fadeInSamples;
            else if (i > sampleCount - fadeOutSamples)
                envelope = (double)(sampleCount - i) / fadeOutSamples;

            short sample = (short)(amplitude * envelope * 16000);
            writer.Write(sample);
        }

        writer.Flush();
        ms.Position = 0;
        return ms;
    }
}
