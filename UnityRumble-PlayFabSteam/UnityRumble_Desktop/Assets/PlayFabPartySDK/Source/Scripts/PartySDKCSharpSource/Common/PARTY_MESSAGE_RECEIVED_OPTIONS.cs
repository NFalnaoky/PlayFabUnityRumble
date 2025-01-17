﻿using System;
using PartyCSharpSDK.Interop;

namespace PartyCSharpSDK
{
    //typedef enum PARTY_MESSAGE_RECEIVED_OPTIONS
    //{
    //    PARTY_MESSAGE_RECEIVED_OPTIONS_NONE = 0x0000,
    //    PARTY_MESSAGE_RECEIVED_OPTIONS_GUARANTEED_DELIVERY = 0x0001,
    //    PARTY_MESSAGE_RECEIVED_OPTIONS_SEQUENTIAL_DELIVERY = 0x0002,
    //    PARTY_MESSAGE_RECEIVED_OPTIONS_REQUIRED_FRAGMENTATION = 0x0004,
    //}
    //PARTY_MESSAGE_RECEIVED_OPTIONS;
    public enum PARTY_MESSAGE_RECEIVED_OPTIONS : UInt32
    {
        PARTY_MESSAGE_RECEIVED_OPTIONS_NONE = 0x0000,
        PARTY_MESSAGE_RECEIVED_OPTIONS_GUARANTEED_DELIVERY = 0x0001,
        PARTY_MESSAGE_RECEIVED_OPTIONS_SEQUENTIAL_DELIVERY = 0x0002,
        PARTY_MESSAGE_RECEIVED_OPTIONS_REQUIRED_FRAGMENTATION = 0x0004,
    }
}