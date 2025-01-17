﻿using System;
using PartyCSharpSDK.Interop;

namespace PartyCSharpSDK
{
    //typedef enum PARTY_AUDIO_OUTPUT_STATE
    //{
    //    PARTY_AUDIO_OUTPUT_STATE_NO_OUTPUT,
    //    PARTY_AUDIO_OUTPUT_STATE_INITIALIZED,
    //    PARTY_AUDIO_OUTPUT_STATE_NOT_FOUND,
    //    PARTY_AUDIO_OUTPUT_STATE_UNSUPPORTED_FORMAT,
    //    PARTY_AUDIO_OUTPUT_STATE_ALREADY_IN_USE,
    //    PARTY_AUDIO_OUTPUT_STATE_UNKNOWN_ERROR,
    //}
    //PARTY_AUDIO_OUTPUT_STATE;
    public enum PARTY_AUDIO_OUTPUT_STATE : UInt32
    {
        PARTY_AUDIO_OUTPUT_STATE_NO_OUTPUT,
        PARTY_AUDIO_OUTPUT_STATE_INITIALIZED,
        PARTY_AUDIO_OUTPUT_STATE_NOT_FOUND,
        PARTY_AUDIO_OUTPUT_STATE_UNSUPPORTED_FORMAT,
        PARTY_AUDIO_OUTPUT_STATE_ALREADY_IN_USE,
        PARTY_AUDIO_OUTPUT_STATE_UNKNOWN_ERROR,
    }
}