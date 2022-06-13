﻿using System;
using PartyCSharpSDK.Interop;

namespace PartyCSharpSDK
{
    //typedef enum PARTY_DESTROYED_REASON
    //{
    //    PARTY_DESTROYED_REASON_REQUESTED,
    //    PARTY_DESTROYED_REASON_DISCONNECTED,
    //    PARTY_DESTROYED_REASON_KICKED,
    //    PARTY_DESTROYED_REASON_DEVICE_LOST_AUTHENTICATION,
    //    PARTY_DESTROYED_REASON_CREATION_FAILED,
    //}
    //PARTY_DESTROYED_REASON;
    public enum PARTY_DESTROYED_REASON : UInt32
    {
        PARTY_DESTROYED_REASON_REQUESTED,
        PARTY_DESTROYED_REASON_DISCONNECTED,
        PARTY_DESTROYED_REASON_KICKED,
        PARTY_DESTROYED_REASON_DEVICE_LOST_AUTHENTICATION,
        PARTY_DESTROYED_REASON_CREATION_FAILED,
    }
}