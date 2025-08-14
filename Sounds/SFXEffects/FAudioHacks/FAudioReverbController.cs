using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using System;
using System.Reflection;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using Terraria;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TerrariaAmbience.Sounds.SFXEffects.FAudioHacks; 


// magical shit that does not yet work
public class FAudioReverbController {
    static readonly Type CtxType =
        typeof(SoundEffect).GetNestedType("FAudioContext", BindingFlags.NonPublic);

    static readonly FieldInfo CtxField =
        CtxType.GetField("Context", BindingFlags.Static | BindingFlags.Public);

    static readonly MethodInfo AttachReverbMethod =
        CtxType.GetMethod("AttachReverb", BindingFlags.Instance | BindingFlags.Public);

    static readonly FieldInfo ReverbVoiceField =
        CtxType.GetField("ReverbVoice", BindingFlags.NonPublic | BindingFlags.Instance);


    [DllImport("FAudio", CallingConvention = CallingConvention.Cdecl)]
    private static extern void FAudioVoice_SetEffectParameters(
        IntPtr voice,
        uint effectIndex,
        IntPtr pParameters,
        uint parametersByteSize,
        uint operationSet
    );

    /*public static unsafe void SetParameters(in FAudio.FAudioFXReverbParameters parameters) {
        var ctx = CtxField.GetValue(null) ?? throw new InvalidOperationException("FAudioContext not initialized.");
        var reverbVoice = (IntPtr)ReverbVoiceField.GetValue(ctx);
        if (reverbVoice == IntPtr.Zero)
            throw new InvalidOperationException("ReverbVoice not created. Call EnsureAttached() first.");

        // copies to take the address of the struct
        FAudio.FAudioFXReverbParameters local = parameters;
        IntPtr ptr = (IntPtr)(&local);

        FAudioVoice_SetEffectParameters(
            reverbVoice,
            effectIndex: 0, // AttachReverb() creates a chain with a single effect at index 0... i think
            pParameters: ptr,
            parametersByteSize: (uint)Marshal.SizeOf<FAudio.FAudioFXReverbParameters>(),
            operationSet: 0
        );
    }*/

    public static FAudio.FAudioFXReverbParameters DefaultFNAReverb = new() {
        WetDryMix = 100f,
        ReflectionsDelay = 7,
        ReverbDelay = 11,
        RearDelay = FAudio.FAUDIOFX_REVERB_DEFAULT_REAR_DELAY,
        PositionLeft = FAudio.FAUDIOFX_REVERB_DEFAULT_POSITION,
        PositionRight = FAudio.FAUDIOFX_REVERB_DEFAULT_POSITION,
        PositionMatrixLeft = FAudio.FAUDIOFX_REVERB_DEFAULT_POSITION_MATRIX,
        PositionMatrixRight = FAudio.FAUDIOFX_REVERB_DEFAULT_POSITION_MATRIX,
        EarlyDiffusion = 15,
        LateDiffusion = 15,
        LowEQGain = 8,
        LowEQCutoff = 4,
        HighEQGain = 8,
        HighEQCutoff = 6,
        RoomFilterFreq = 5000f,
        RoomFilterMain = -10f,
        RoomFilterHF = -1f,
        ReflectionsGain = -26.0200005f,
        ReverbGain = 10.0f,
        DecayTime = 1.49000001f,
        Density = 100.0f,
        RoomSize = FAudio.FAUDIOFX_REVERB_DEFAULT_ROOM_SIZE
    };

    public unsafe static void ApplyCustomReverb(SoundEffectInstance inst, float rvGain) {
        var handle = inst.handle;

        if (handle == IntPtr.Zero) {
            return;
        }

        if (!inst.usingReverb) {
            // MyReverbAttach(handle, param);
            MyReverbAttachModified(handle);
            inst.usingReverb = true;
        }

        // Re-using this float array...
        float* outputMatrix = (float*)inst.dspSettings.pMatrixCoefficients;

        outputMatrix[0] = rvGain;
        if (inst.dspSettings.SrcChannelCount == 2)
            outputMatrix[1] = rvGain;

        FAudio.FAudioVoice_SetOutputMatrix(handle, SoundEffect.Device().ReverbVoice, inst.dspSettings.SrcChannelCount, 1, inst.dspSettings.pMatrixCoefficients, 0);
    }

    // memory leaks or something happen here, so we don't use this
    unsafe static void MyReverbAttach(IntPtr handle, in FAudio.FAudioFXReverbParameters param) {
        var device = SoundEffect.Device();

        // Only create a reverb voice if they ask for it!
        if (device.ReverbVoice == IntPtr.Zero) {
            IntPtr reverb;
            FAudio.FAudioCreateReverb(out reverb, 0);

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
                /* Reverb will be omnidirectional */ 
                device.DeviceDetails.OutputFormat.Format.nSamplesPerSec, 0, 0, IntPtr.Zero, chainPtr);

            FAudio.FAPOBase_Release(reverb);

            FNAPlatform.Free(reverbChain->pEffectDescriptors);
            FNAPlatform.Free(chainPtr);

            // Defaults based on FAUDIOFX_I3DL2_PRESET_GENERIC
            IntPtr rvbParamsPtr = FNAPlatform.Malloc(MarshalHelper.SizeOf<FAudio.FAudioFXReverbParameters>());
            FAudio.FAudioFXReverbParameters* rvbParams = (FAudio.FAudioFXReverbParameters*)rvbParamsPtr;
            rvbParams->WetDryMix = param.WetDryMix;
            rvbParams->ReflectionsDelay = param.ReflectionsDelay;
            rvbParams->ReverbDelay = param.ReverbDelay;
            rvbParams->RearDelay = param.RearDelay;
            rvbParams->PositionLeft = param.PositionLeft;
            rvbParams->PositionRight = param.PositionRight;
            rvbParams->PositionMatrixLeft = param.PositionMatrixLeft;
            rvbParams->PositionMatrixRight = param.PositionMatrixRight;
            rvbParams->EarlyDiffusion = param.EarlyDiffusion;
            rvbParams->LateDiffusion = param.LateDiffusion;
            rvbParams->LowEQGain = param.LowEQGain;
            rvbParams->LowEQCutoff = param.LowEQCutoff;
            rvbParams->HighEQGain = param.HighEQGain;
            rvbParams->HighEQCutoff = param.HighEQCutoff;
            rvbParams->RoomFilterFreq = param.RoomFilterFreq;
            rvbParams->RoomFilterMain = param.RoomFilterMain;
            rvbParams->RoomFilterHF = param.RoomFilterHF;
            rvbParams->ReflectionsGain = param.ReflectionsGain;
            rvbParams->ReverbGain = param.ReverbGain;
            rvbParams->DecayTime = param.DecayTime;
            rvbParams->Density = param.Density;
            rvbParams->RoomSize = param.RoomSize;

            FAudio.FAudioVoice_SetEffectParameters(device.ReverbVoice, 0, rvbParamsPtr, (uint)MarshalHelper.SizeOf<FAudio.FAudioFXReverbParameters>(), 0);
            FNAPlatform.Free(rvbParamsPtr);

            device.reverbSends = new FAudio.FAudioVoiceSends();
            device.reverbSends.SendCount = 2;
            device.reverbSends.pSends = FNAPlatform.Malloc(2 * MarshalHelper.SizeOf<FAudio.FAudioSendDescriptor>());
            FAudio.FAudioSendDescriptor* sendDesc = (FAudio.FAudioSendDescriptor*)device.reverbSends.pSends;
            sendDesc[0].Flags = 0;
            sendDesc[0].pOutputVoice = device.MasterVoice;
            sendDesc[1].Flags = 0;
            sendDesc[1].pOutputVoice = device.ReverbVoice;
        }

        // Oh hey here's where we actually attach it
        FAudio.FAudioVoice_SetOutputVoices(handle, ref device.reverbSends);
    }
    public static FAudio.FAudioFXReverbParameters TAReverbParams = new() {
        WetDryMix = 100f,
        ReflectionsDelay = 7,
        ReverbDelay = 11,
        RearDelay = FAudio.FAUDIOFX_REVERB_DEFAULT_REAR_DELAY,
        PositionLeft = FAudio.FAUDIOFX_REVERB_DEFAULT_POSITION,
        PositionRight = FAudio.FAUDIOFX_REVERB_DEFAULT_POSITION,
        PositionMatrixLeft = FAudio.FAUDIOFX_REVERB_DEFAULT_POSITION_MATRIX,
        PositionMatrixRight = FAudio.FAUDIOFX_REVERB_DEFAULT_POSITION_MATRIX,
        EarlyDiffusion = 15,
        LateDiffusion = 15,
        LowEQGain = 8,
        LowEQCutoff = 4,
        HighEQGain = 8,
        HighEQCutoff = 6,
        RoomFilterFreq = 5000f,
        RoomFilterMain = -10f,
        RoomFilterHF = -1f,
        ReflectionsGain = -26.0200005f,
        ReverbGain = 10.0f,
        DecayTime = 1.49000001f,
        Density = 100.0f,
        RoomSize = FAudio.FAUDIOFX_REVERB_DEFAULT_ROOM_SIZE
    };

    static bool _init;
    public unsafe static void RvInit() {
        if (_init)
            return;

        _init = true;

        var device = SoundEffect.Device();

        IntPtr reverb;
        FAudio.FAudioCreateReverb(out reverb, 0);

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
            /* Reverb will be omnidirectional */
            device.DeviceDetails.OutputFormat.Format.nSamplesPerSec, 0, 0, IntPtr.Zero, chainPtr);

        FAudio.FAPOBase_Release(reverb);

        FNAPlatform.Free(reverbChain->pEffectDescriptors);
        FNAPlatform.Free(chainPtr);
    }
    unsafe static void MyReverbAttachModified(IntPtr handle) {
        var device = SoundEffect.Device();

        var rvbParams = TAReverbParams;
        FAudio.FAudioFXReverbParameters* rvbParamsPtr = &rvbParams;

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
