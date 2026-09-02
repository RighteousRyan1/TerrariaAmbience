using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using System;

namespace TerrariaAmbience.Sounds.SoundFilters.FAudioHacks; 

public class FAudioReverbController {

    public static readonly FAudio.FAudioFXReverbParameters DefaultFAudioReverb = new() {
        WetDryMix = 100f,
        ReflectionsDelay = 7,
        ReverbDelay = 11,
        RearDelay = FAudio.FAUDIOFX_REVERB_DEFAULT_REAR_DELAY,
        PositionLeft = FAudio.FAUDIOFX_REVERB_DEFAULT_POSITION,
        PositionRight = FAudio.FAUDIOFX_REVERB_DEFAULT_POSITION,
        PositionMatrixLeft = FAudio.FAUDIOFX_REVERB_DEFAULT_POSITION_MATRIX,
        PositionMatrixRight = FAudio.FAUDIOFX_REVERB_DEFAULT_POSITION_MATRIX,
        EarlyDiffusion = FAudio.FAUDIOFX_REVERB_DEFAULT_EARLY_DIFFUSION, // 15?
        LateDiffusion = FAudio.FAUDIOFX_REVERB_DEFAULT_LATE_DIFFUSION, // also 15?
        LowEQGain = FAudio.FAUDIOFX_REVERB_DEFAULT_LOW_EQ_GAIN,
        LowEQCutoff = FAudio.FAUDIOFX_REVERB_DEFAULT_LOW_EQ_CUTOFF,
        HighEQGain = FAudio.FAUDIOFX_REVERB_DEFAULT_HIGH_EQ_GAIN,
        HighEQCutoff = FAudio.FAUDIOFX_REVERB_DEFAULT_HIGH_EQ_CUTOFF, // 6?
        RoomFilterFreq = FAudio.FAUDIOFX_REVERB_DEFAULT_ROOM_FILTER_FREQ,
        RoomFilterMain = FAudio.FAUDIOFX_REVERB_DEFAULT_ROOM_FILTER_MAIN, // -10?
        RoomFilterHF = FAudio.FAUDIOFX_REVERB_DEFAULT_ROOM_FILTER_HF, // -1?
        ReflectionsGain = FAudio.FAUDIOFX_REVERB_DEFAULT_REFLECTIONS_GAIN, // -26.02?
        ReverbGain = 10.0f, // FAudio.FAUDIOFX_REVERB_DEFAULT_REVERB_GAIN,
        DecayTime = 1.49000001f,
        Density = FAudio.FAUDIOFX_REVERB_DEFAULT_DENSITY,
        RoomSize = FAudio.FAUDIOFX_REVERB_DEFAULT_ROOM_SIZE
    };

    public unsafe static void ApplyCustomReverb(SoundEffectInstance inst, float rvGain, FilterParams param) {
        var handle = inst.handle;

        if (handle == IntPtr.Zero) {
            return;
        }

        if (!inst.usingReverb) {
            ReverbAttach(handle, param.Reverb);
            inst.usingReverb = true;
        }

        // Re-using this float array...
        float* outputMatrix = (float*)inst.dspSettings.pMatrixCoefficients;

        outputMatrix[0] = rvGain;
        if (inst.dspSettings.SrcChannelCount == 2)
            outputMatrix[1] = rvGain;

        FAudio.FAudioVoice_SetOutputMatrix(handle, SoundEffect.Device().ReverbVoice, inst.dspSettings.SrcChannelCount, 1, inst.dspSettings.pMatrixCoefficients, 0);
    }

    static bool _init;

    // yadda yadda... create more instances somehow. too lazy, too bad.
    public unsafe static void RvInit() {
        if (_init)
            return;

        _init = true;

        var device = SoundEffect.Device();

        FAudio.FAudioCreateReverb(out nint reverb, 0);

        IntPtr chainPtr;
        chainPtr = FNAPlatform.Malloc(MarshalHelper.SizeOf<FAudio.FAudioEffectChain>());

        FAudio.FAudioEffectChain* reverbChain = (FAudio.FAudioEffectChain*)chainPtr;

        reverbChain->EffectCount = 1;
        reverbChain->pEffectDescriptors = FNAPlatform.Malloc(MarshalHelper.SizeOf<FAudio.FAudioEffectDescriptor>());

        FAudio.FAudioEffectDescriptor* reverbDesc = (FAudio.FAudioEffectDescriptor*)reverbChain->pEffectDescriptors;

        reverbDesc->InitialState = 1;
        reverbDesc->OutputChannels = (uint)(
            (device.DeviceDetails.OutputFormat.Format.nChannels == 6) ? 6 : 1
        );
        reverbDesc->pEffect = reverb;

        FAudio.FAudio_CreateSubmixVoice(device.Handle, out device.ReverbVoice, 1,
            // always omnidirectional reverb
            device.DeviceDetails.OutputFormat.Format.nSamplesPerSec, 0, 0, IntPtr.Zero, chainPtr);

        FAudio.FAPOBase_Release(reverb);

        FNAPlatform.Free(reverbChain->pEffectDescriptors);
        FNAPlatform.Free(chainPtr);
    }
    unsafe static void ReverbAttach(IntPtr handle, FAudio.FAudioFXReverbParameters param) {
        var device = SoundEffect.Device();

        var rvbParams = param;
        FAudio.FAudioFXReverbParameters* rvbParamsPtr = &rvbParams;

        // effectIndex could be useful for tracking multiple reverbs
        FAudio.FAudioVoice_SetEffectParameters(device.ReverbVoice, 0, (IntPtr)rvbParamsPtr, (uint)MarshalHelper.SizeOf<FAudio.FAudioFXReverbParameters>(), 0);

        device.reverbSends = new FAudio.FAudioVoiceSends();
        device.reverbSends.SendCount = 2;
        device.reverbSends.pSends = FNAPlatform.Malloc(2 * MarshalHelper.SizeOf<FAudio.FAudioSendDescriptor>());
        FAudio.FAudioSendDescriptor* sendDesc = (FAudio.FAudioSendDescriptor*)device.reverbSends.pSends;
        sendDesc[0].Flags = 0;
        sendDesc[0].pOutputVoice = device.MasterVoice;
        sendDesc[1].Flags = 0;
        sendDesc[1].pOutputVoice = device.ReverbVoice;

        // Oh hey here's where we actually attach it
        FAudio.FAudioVoice_SetOutputVoices(handle, ref device.reverbSends);
    }
}
