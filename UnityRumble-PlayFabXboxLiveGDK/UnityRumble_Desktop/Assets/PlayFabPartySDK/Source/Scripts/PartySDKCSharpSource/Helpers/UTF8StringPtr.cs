﻿using System;
using System.Runtime.InteropServices;

namespace PartyCSharpSDK
{
    /// <summary>
    /// Represents a null-terminated UTF-8 char* whose _pointer value_ is marshalled between managed and unmanaged code.
    /// </summary>>
    [StructLayout(LayoutKind.Sequential)]
    public struct UTF8StringPtr
    {
        /// <summary>
        /// Constructor for marshaling a UTF8 char* _to_ managed code.  Requires an existing DisposableCollection to add a buffer to.
        /// </summary>
        public UTF8StringPtr(string str, DisposableCollection disposableCollection)
        {
            if (str == null)
            {
                this.pointer = IntPtr.Zero;
            }
            else
            {
                byte[] utf8Bytes = Converters.StringToNullTerminatedUTF8ByteArray(str);
                DisposableBuffer buffer = new DisposableBuffer(utf8Bytes.Length);
                Marshal.Copy(source: utf8Bytes, startIndex: 0, destination: buffer.IntPtr, length: utf8Bytes.Length);
                disposableCollection.Add(buffer);
                this.pointer = buffer.IntPtr;
            }
        }

        public string GetString()
        {
            if (this.pointer == IntPtr.Zero)
            {
                return null;
            }
            else
            {
                return Converters.PtrToStringUTF8(this.pointer);
            }
        }

        private IntPtr pointer;
    }
}
