﻿using System;
using PartyCSharpSDK.Interop;

namespace PartyCSharpSDK
{
    //typedef enum PARTY_VOICE_CHAT_TRANSCRIPTION_PHRASE_TYPE
    //{
        // PARTY_VOICE_CHAT_TRANSCRIPTION_PHRASE_TYPE_HYPOTHESIS = 0,
        // PARTY_VOICE_CHAT_TRANSCRIPTION_PHRASE_TYPE_FINAL = 1,
    //}
    //PARTY_VOICE_CHAT_TRANSCRIPTION_PHRASE_TYPE;
    public enum PARTY_VOICE_CHAT_TRANSCRIPTION_PHRASE_TYPE : UInt32
    {
        PARTY_VOICE_CHAT_TRANSCRIPTION_PHRASE_TYPE_HYPOTHESIS = 0,
        PARTY_VOICE_CHAT_TRANSCRIPTION_PHRASE_TYPE_FINAL = 1,
    }
}