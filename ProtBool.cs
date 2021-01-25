using System.Runtime.InteropServices;

namespace SWAC {
    /// <summary>
    /// Wrapper for int32 value protection.
    /// </summary>
    [StructLayout (LayoutKind.Explicit)]
    public struct ProtBool {
        /// <summary>
        /// Get encrypted value (for serialization or something else).
        /// </summary>
        /// <value>The encrypted value.</value>
        public int EncryptedValue {
            get {
                // Workaround for default struct constructor init.
                if (_conv == 0 && _encrypt == 0) {
                    _conv = XorMask;
                }
                return _encrypt;
            }
        }

        const uint XorMask = 0xaaaaaaaa;

        [FieldOffset (0)]
        int _encrypt;

        [FieldOffset (0)]
        uint _conv;

        public static implicit operator bool (ProtBool v) {
            v._conv ^= XorMask;
            var f = v._encrypt;
            v._conv ^= XorMask;
            return f == 10;
        }

        public static implicit operator ProtBool (bool va)
        {
            var v = va ? 10 : -10;
            var p = new ProtBool();
            p._encrypt = v;
            p._conv ^= XorMask;
            return p;
        }
    }
}