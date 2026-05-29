using OpenTK.Audio.OpenAL;

namespace BabyBearsEngine.Platform.OpenAL;

/// <summary>
/// Result of decoding an audio file into the raw PCM form OpenAL's buffer API expects.
/// </summary>
/// <param name="Pcm">Interleaved PCM samples ready to hand to <c>AL.BufferData</c>.</param>
/// <param name="Format">The OpenAL sample format (mono/stereo, 8/16-bit).</param>
/// <param name="SampleRate">Samples per second (e.g. 44100, 48000).</param>
/// <param name="Duration">Decoded length of the clip.</param>
internal sealed record DecodedAudio(byte[] Pcm, ALFormat Format, int SampleRate, TimeSpan Duration);
