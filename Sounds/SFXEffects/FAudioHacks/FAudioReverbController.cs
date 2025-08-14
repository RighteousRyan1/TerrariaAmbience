using Microsoft.Xna.Framework.Audio;
using System;
using System.Reflection;
using System.Runtime.InteropServices;

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
}
